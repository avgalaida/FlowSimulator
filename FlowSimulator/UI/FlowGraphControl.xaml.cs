using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Node;
using FlowGraphBase.Node.StandardActionNode;
using FlowGraphBase.Node.StandardVariableNode;
using FlowGraphBase.Process;
using FlowGraphBase.Script;
using FlowSimulator.CustomNode;
using NetworkModel;
using NetworkUI;
using ZoomAndPan;

namespace FlowSimulator.UI
{
    public partial class FlowGraphControl : UserControl
    {

        private static readonly List<NodeViewModel> _ClipboardNodes = new List<NodeViewModel>(10);

        private bool _IsContextMenuCreated;

        public event SelectionChangedEventHandler SelectionChanged;

        public FlowGraphControlViewModel ViewModel => (FlowGraphControlViewModel)DataContext;

        public FlowGraphControl()
        {
            DataContextChanged += OnDataContextChanged;
            InitializeComponent();

            Loaded += OnLoaded;
        }

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FlowGraphControlViewModel fgcvm;

            if (e.OldValue is FlowGraphControlViewModel)
            {
                fgcvm = DataContext as FlowGraphControlViewModel;
                fgcvm.ContextMenuOpened -= OnContextMenuOpened;
            }

            if (e.NewValue is FlowGraphControlViewModel)
            {
                fgcvm = DataContext as FlowGraphControlViewModel;
                fgcvm.ContextMenuOpened += OnContextMenuOpened;
            }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_IsContextMenuCreated == false)
            {
                _IsContextMenuCreated = true;

                IEnumerable<Type> classes = AppDomain.CurrentDomain.GetAssemblies()
                           .SelectMany(t => t.GetTypes())
                           .Where(t => t.IsClass
                               && t.IsGenericType == false
                               && t.IsInterface == false
                               && t.IsAbstract == false
                               && t.IsSubclassOf(typeof(SequenceNode)));

                foreach (Type type in classes)
                {
                    Attribute browsableAtt = Attribute.GetCustomAttribute(type, typeof(Visible), true);
                    if (browsableAtt != null
                        && ((Visible)browsableAtt).Value == false)
                    {
                        continue;
                    }

                    Attribute categAtt = Attribute.GetCustomAttribute(type, typeof(Category), true);
                    Attribute nameAtt = Attribute.GetCustomAttribute(type, typeof(Name), true);

                    if (nameAtt == null
                        || string.IsNullOrWhiteSpace(((Name)nameAtt).DisplayName))
                    {
                        LogManager.Instance.WriteLine(
                            LogVerbosity.Error,
                            "Не удается создать меню для типа '{0}', так как имя атрибута не указано",
                            type.FullName);
                        continue;
                    }

                    string categPath = categAtt == null ? "" : ((Category)categAtt).CategoryPath;

                    MenuItem parent = CreateParentMenuItemNode(categPath, menuItemCreateNode);
                    MenuItem item = new MenuItem
                    {
                        Header = ((Name)nameAtt).DisplayName,
                        Tag = type
                    };
                    item.Click += MenuItemCreateNode_Click;
                    parent.Items.Add(item);
                }
            }
        }

        MenuItem CreateParentMenuItemNode(string categPath_, MenuItem parent_)
        {
            if (string.IsNullOrWhiteSpace(categPath_))
            {
                return parent_;
            }

            string[] folders = categPath_.Split('/');
            categPath_ = categPath_.Remove(0, folders[0].Length);
            if (categPath_.Length > 1) categPath_ = categPath_.Remove(0, 1);

            foreach (MenuItem item in parent_.Items)
            {
                if (folders[0].Equals(item.Header))
                {
                    return CreateParentMenuItemNode(categPath_, item);
                }
            }

            MenuItem child = new MenuItem { Header = folders[0] };
            parent_.Items.Add(child);

            return CreateParentMenuItemNode(categPath_, child);
        }

        void MenuItemCreateNode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                Type type = item.Tag as Type;
                CreateNode((SequenceNode)Activator.CreateInstance(type));
            }
        }

        void OnContextMenuOpened(object sender, EventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void networkControl_ConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        {
            var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(networkControl);

            var connection = ViewModel.ConnectionDragStarted(draggedOutConnector, curDragPoint);

            e.Connection = connection;
        }

        private void networkControl_QueryConnectionFeedback(object sender, QueryConnectionFeedbackEventArgs e)
        {
            var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
            var draggedOverConnector = (ConnectorViewModel)e.DraggedOverConnector;
            object feedbackIndicator = null;
            bool connectionOk = true;

            ViewModel.QueryConnnectionFeedback(draggedOutConnector, draggedOverConnector, out feedbackIndicator, out connectionOk);

            e.FeedbackIndicator = feedbackIndicator;

            e.ConnectionOk = connectionOk;
        }

        private void networkControl_ConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            Point curDragPoint = Mouse.GetPosition(networkControl);
            var connection = (ConnectionViewModel)e.Connection;
            ViewModel.ConnectionDragging(curDragPoint, connection);
        }

        private void networkControl_ConnectionDragCompleted(object sender, ConnectionDragCompletedEventArgs e)
        {
            var connectorDraggedOut = (ConnectorViewModel)e.ConnectorDraggedOut;
            var connectorDraggedOver = (ConnectorViewModel)e.ConnectorDraggedOver;
            var newConnection = (ConnectionViewModel)e.Connection;
            ViewModel.ConnectionDragCompleted(newConnection, connectorDraggedOut, connectorDraggedOver);
        }

        private void DeleteSelectedNodes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            networkControl.IsUndoRegisterEnabled = false;
            ViewModel.DeleteSelectedNodes();
            networkControl.IsUndoRegisterEnabled = true;
        }
        private void DeleteNode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var node = (NodeViewModel)e.Parameter;
            ViewModel.DeleteNode(node, true);
        }

        private void DeleteConnection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var connection = (ConnectionViewModel)e.Parameter;
            ViewModel.DeleteConnection(connection, true);
        }

        private void CreateNode(SequenceNode node_)
        {
            ViewModel.CreateNode(node_, origContentMouseDownPoint, true);
        }

        private void Node_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var node = (NodeViewModel)element.DataContext;
            node.Size = new Size(element.ActualWidth, element.ActualHeight);
        }

        private MouseHandlingMode mouseHandlingMode = MouseHandlingMode.None;
        private Point origZoomAndPanControlMouseDownPoint;
        private Point origContentMouseDownPoint;
        private MouseButton mouseButtonDown;
        private Rect prevZoomRect;
        private double prevZoomScale;
        private bool prevZoomRectSet;

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (networkControl.IsDragging)
            {
                scrollViewer.DoMouseDown();
            }

            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            scrollViewer.DoMouseUp();
            base.OnPreviewMouseUp(e);
        }

        private void MouseDragScrollViewerDragHorizontal(double offset_)
        {
            zoomAndPanControl.ContentOffsetX += offset_;
        }

        private void MouseDragScrollViewerDragVertical(double offset_)
        {
            zoomAndPanControl.ContentOffsetY += offset_;
        }   

        private void networkControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            networkControl.Focus();
            Keyboard.Focus(networkControl);

            mouseButtonDown = e.ChangedButton;
            origZoomAndPanControlMouseDownPoint = e.GetPosition(zoomAndPanControl);
            origContentMouseDownPoint = e.GetPosition(networkControl);

            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                (e.ChangedButton == MouseButton.Left ||
                 e.ChangedButton == MouseButton.Right))
            {
                mouseHandlingMode = MouseHandlingMode.Zooming;
            }
            else if (mouseButtonDown == MouseButton.Left &&
                     (Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                mouseHandlingMode = MouseHandlingMode.Panning;
            }

            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                networkControl.CaptureMouse();
                e.Handled = true;
            }
        }

        private void networkControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseHandlingMode != MouseHandlingMode.None)
            {
                if (mouseHandlingMode == MouseHandlingMode.Panning)
                {
                }
                else if (mouseHandlingMode == MouseHandlingMode.Zooming)
                {
                    if (mouseButtonDown == MouseButton.Left)
                    {
                        ZoomIn(origContentMouseDownPoint);
                    }
                    else if (mouseButtonDown == MouseButton.Right)
                    {
                        ZoomOut(origContentMouseDownPoint);
                    }
                }
                else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
                {
                    ApplyDragZoomRect();
                }

                networkControl.IsClearSelectionOnEmptySpaceClickEnabled = true;

                Mouse.OverrideCursor = null;

                networkControl.ReleaseMouseCapture();
                mouseHandlingMode = MouseHandlingMode.None;
                e.Handled = true;
            }
        }

        private void networkControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseHandlingMode == MouseHandlingMode.Panning)
            {
                Point curZoomAndPanControlMousePoint = e.GetPosition(zoomAndPanControl);
                Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
                double dragThreshold = 10;
                if (Math.Abs(dragOffset.X) > dragThreshold ||
                    Math.Abs(dragOffset.Y) > dragThreshold)
                {
                    mouseHandlingMode = MouseHandlingMode.DragPanning;
                    networkControl.IsClearSelectionOnEmptySpaceClickEnabled = false;
                    Mouse.OverrideCursor = Cursors.ScrollAll;
                }

                e.Handled = true;
            }
            else if (mouseHandlingMode == MouseHandlingMode.DragPanning)
            {
                Point curContentMousePoint = e.GetPosition(networkControl);
                Vector dragOffset = curContentMousePoint - origContentMouseDownPoint;

                zoomAndPanControl.ContentOffsetX -= dragOffset.X;
                zoomAndPanControl.ContentOffsetY -= dragOffset.Y;

                e.Handled = true;
            }
            else if (mouseHandlingMode == MouseHandlingMode.Zooming)
            {
                Point curZoomAndPanControlMousePoint = e.GetPosition(zoomAndPanControl);
                Vector dragOffset = curZoomAndPanControlMousePoint - origZoomAndPanControlMouseDownPoint;
                double dragThreshold = 10;
                if (mouseButtonDown == MouseButton.Left &&
                    (Math.Abs(dragOffset.X) > dragThreshold ||
                    Math.Abs(dragOffset.Y) > dragThreshold))
                {
                    mouseHandlingMode = MouseHandlingMode.DragZooming;
                    Point curContentMousePoint = e.GetPosition(networkControl);
                    InitDragZoomRect(origContentMouseDownPoint, curContentMousePoint);
                }

                e.Handled = true;
            }
            else if (mouseHandlingMode == MouseHandlingMode.DragZooming)
            {
                Point curContentMousePoint = e.GetPosition(networkControl);
                SetDragZoomRect(origContentMouseDownPoint, curContentMousePoint);

                e.Handled = true;
            }
        }

        private void networkControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                Point curContentMousePoint = e.GetPosition(networkControl);
                ZoomIn(curContentMousePoint);
            }
            else if (e.Delta < 0)
            {
                Point curContentMousePoint = e.GetPosition(networkControl);
                ZoomOut(curContentMousePoint);
            }
        }
        private void networkControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                Point doubleClickPoint = e.GetPosition(networkControl);
                zoomAndPanControl.AnimatedSnapTo(doubleClickPoint);
            }
        }

        private void ZoomIn_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var o = networkControl.SelectedNode;

            ZoomIn(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        private void ZoomOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomOut(new Point(zoomAndPanControl.ContentZoomFocusX, zoomAndPanControl.ContentZoomFocusY));
        }

        private void JumpBackToPrevZoo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            JumpBackToPrevZoom();
        }

        private void JumpBackToPrevZoo_CanExecuted(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = prevZoomRectSet;
        }

        private void FitContent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            IList nodes = null;

            if (networkControl.SelectedNodes.Count > 0)
            {
                nodes = networkControl.SelectedNodes;
            }
            else
            {
                nodes = ViewModel.Network.Nodes;
                if (nodes.Count == 0)
                {
                    return;
                }
            }

            SavePrevZoomRect();

            Rect actualContentRect = DetermineAreaOfNodes(nodes);

            actualContentRect.Inflate(networkControl.ActualWidth / 40, networkControl.ActualHeight / 40);

            zoomAndPanControl.AnimatedZoomTo(actualContentRect);
        }

        private Rect DetermineAreaOfNodes(IList nodes)
        {
            NodeViewModel firstNode = (NodeViewModel)nodes[0];
            Rect actualContentRect = new Rect(firstNode.X, firstNode.Y, firstNode.Size.Width, firstNode.Size.Height);

            for (int i = 1; i < nodes.Count; ++i)
            {
                NodeViewModel node = (NodeViewModel)nodes[i];
                Rect nodeRect = new Rect(node.X, node.Y, node.Size.Width, node.Size.Height);
                actualContentRect = Rect.Union(actualContentRect, nodeRect);
            }
            return actualContentRect;
        }

        private void Fill_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SavePrevZoomRect();

            zoomAndPanControl.AnimatedScaleToFit();
        }

        private void OneHundredPercent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SavePrevZoomRect();

            zoomAndPanControl.AnimatedZoomTo(1.0);
        }

        private void JumpBackToPrevZoom()
        {
            zoomAndPanControl.AnimatedZoomTo(prevZoomScale, prevZoomRect);

            ClearPrevZoomRect();
        }

        private void ZoomOut(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale - 0.1, contentZoomCenter);
        }

        private void ZoomIn(Point contentZoomCenter)
        {
            zoomAndPanControl.ZoomAboutPoint(zoomAndPanControl.ContentScale + 0.1, contentZoomCenter);
        }

        private void InitDragZoomRect(Point pt1, Point pt2)
        {
            SetDragZoomRect(pt1, pt2);

            dragZoomCanvas.Visibility = Visibility.Visible;
            dragZoomBorder.Opacity = 0.5;
        }

        private void SetDragZoomRect(Point pt1, Point pt2)
        {
            double x, y, width, height;

            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            Canvas.SetLeft(dragZoomBorder, x);
            Canvas.SetTop(dragZoomBorder, y);
            dragZoomBorder.Width = width;
            dragZoomBorder.Height = height;
        }

        private void ApplyDragZoomRect()
        {
            SavePrevZoomRect();

            double contentX = Canvas.GetLeft(dragZoomBorder);
            double contentY = Canvas.GetTop(dragZoomBorder);
            double contentWidth = dragZoomBorder.Width;
            double contentHeight = dragZoomBorder.Height;
            zoomAndPanControl.AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));

            FadeOutDragZoomRect();
        }

        private void FadeOutDragZoomRect()
        {
            AnimationHelper.StartAnimation(dragZoomBorder, OpacityProperty, 0.0, 0.1,
                delegate
                {
                    dragZoomCanvas.Visibility = Visibility.Collapsed;
                });
        }

        private void SavePrevZoomRect()
        {
            prevZoomRect = new Rect(zoomAndPanControl.ContentOffsetX, zoomAndPanControl.ContentOffsetY, zoomAndPanControl.ContentViewportWidth, zoomAndPanControl.ContentViewportHeight);
            prevZoomScale = zoomAndPanControl.ContentScale;
            prevZoomRectSet = true;
        }

        private void ClearPrevZoomRect()
        {
            prevZoomRectSet = false;
        }

        private void networkControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is NetworkView view
                && view.IsUndoRegisterEnabled)
            {
                if (e.RemovedItems.Count > 0)
                {
                    List<NodeViewModel> list = new List<NodeViewModel>();

                    foreach (object node in e.RemovedItems)
                    {
                        if (node is NodeViewModel model)
                        {
                            list.Add(model);
                        }
                    }

                    if (list.Count > 0)
                    {
                        ViewModel.OnNodesDeselectedChanged(networkControl, list);
                    }
                }

                if (e.AddedItems.Count > 0)
                {
                    List<NodeViewModel> list = new List<NodeViewModel>();

                    foreach (object node in e.AddedItems)
                    {
                        if (node is NodeViewModel model)
                        {
                            list.Add(model);
                        }
                    }

                    if (list.Count > 0)
                    {
                        ViewModel.OnNodesSelectedChanged(networkControl, list);
                    }
                }
            }

            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, e.RemovedItems, e.AddedItems));
        }

        private void EditCustomVariable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe)
            {
                if (fe.DataContext is VariableNode varNode)
                {
                    {
                        LogManager.Instance.WriteLine(LogVerbosity.Warning, "Узел переменной => Нет пользовательского редактора для этого типа");
                    }
                }
            }
        }

        private void DropList_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.StringFormat) ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                try
                {
                    string data = e.Data.GetData(DataFormats.StringFormat) as string;

                    if (data.StartsWith(FlowGraphDataControl.DragPrefixFunction))
                    {
                        string id = data.Split('#')[1];
                        SequenceFunction func = GraphDataManager.Instance.GetFunctionById(int.Parse(id));
                        CallFunctionNode seqNode = new CallFunctionNode(func);
                        ViewModel.CreateNode(seqNode, e.GetPosition(networkControl), false);
                    }
                    else if (data.StartsWith(FlowGraphDataControl.DragPrefixNamedVar))
                    {
                        string name = data.Split('#')[1];
                        NamedVariableNode seqNode = new NamedVariableNode(name);
                        ViewModel.CreateNode(seqNode, e.GetPosition(networkControl), false);
                    }
                    else if (data.StartsWith(FlowGraphDataControl.DragPrefixScriptElement))
                    {
                        string idStr = data.Split('#')[1];
                        int id = int.Parse(idStr);
                        ScriptElement el = GraphDataManager.Instance.GetScriptById(id);
                        ScriptNode seqNode = new ScriptNode(el);
                        ViewModel.CreateNode(seqNode, e.GetPosition(networkControl), false);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.WriteException(ex);
                }
            }
        }

        private void FlowGraphCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (networkControl.SelectedNodes.Count > 0)
                {
                    _ClipboardNodes.Clear();

                    foreach (var node in networkControl.SelectedNodes)
                    {
                        _ClipboardNodes.Add(node as NodeViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteException(ex);
            }
        }

        private void FlowGraphPaste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (_ClipboardNodes.Count > 0)
                {
                    networkControl.SelectedNodes.Clear();
                    networkControl.IsUndoRegisterEnabled = false;
                    IEnumerable<NodeViewModel> nodes = ViewModel.CopyNodes(_ClipboardNodes);

                    foreach (NodeViewModel node in nodes)
                    {
                        networkControl.SelectedNodes.Add(node);
                    }

                    networkControl.IsUndoRegisterEnabled = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteException(ex);
            }
        }

        private void FlowGraphUndo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.UndoRedoManager.Undo();
        }

        private void FlowGraphRedo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.UndoRedoManager.Redo();
        }

        private void Launch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CreateSequence();
            ProcessLauncher.Instance.LaunchSequence(ViewModel.Sequence, typeof(EventNodeTestStarted), 0, "test");
        }
        
        private void networkControl_NodeDragStarted(object sender, NodeDragStartedEventArgs e)
        {
            ViewModel.OnNodeDragStarted(sender as NetworkView, e);
        }

        private void networkControl_NodeDragCompleted(object sender, NodeDragCompletedEventArgs e)
        {
            ViewModel.OnNodeDragCompleted(sender as NetworkView, e);
        }
    }
}
