﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Utils;

namespace NetworkUI
{
    /// <summary>
    /// The main class that implements the network/flow-chart control.
    /// </summary>
    public partial class NetworkView : Control
    {
        private static readonly DependencyPropertyKey NodesPropertyKey =
            DependencyProperty.RegisterReadOnly("Nodes", typeof(ImpObservableCollection<object>), typeof(NetworkView),
                new FrameworkPropertyMetadata());
        public static readonly DependencyProperty NodesProperty = NodesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ConnectionsPropertyKey =
            DependencyProperty.RegisterReadOnly("Connections", typeof(ImpObservableCollection<object>), typeof(NetworkView),
                new FrameworkPropertyMetadata());
        public static readonly DependencyProperty ConnectionsProperty = ConnectionsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty NodesSourceProperty =
            DependencyProperty.Register("NodesSource", typeof(IEnumerable), typeof(NetworkView),
                new FrameworkPropertyMetadata(NodesSource_PropertyChanged));

        public static readonly DependencyProperty ConnectionsSourceProperty =
            DependencyProperty.Register("ConnectionsSource", typeof(IEnumerable), typeof(NetworkView),
                new FrameworkPropertyMetadata(ConnectionsSource_PropertyChanged));

        public static readonly DependencyProperty IsClearSelectionOnEmptySpaceClickEnabledProperty =
            DependencyProperty.Register("IsClearSelectionOnEmptySpaceClickEnabled", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty EnableConnectionDraggingProperty =
            DependencyProperty.Register("EnableConnectionDragging", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(true));

        private static readonly DependencyPropertyKey IsDraggingConnectionPropertyKey =
            DependencyProperty.RegisterReadOnly("IsDraggingConnection", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingConnectionProperty = IsDraggingConnectionPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsNotDraggingConnectionPropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDraggingConnection", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingConnectionProperty = IsNotDraggingConnectionPropertyKey.DependencyProperty;

        public static readonly DependencyProperty EnableNodeDraggingProperty =
            DependencyProperty.Register("EnableNodeDragging", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(true));

        private static readonly DependencyPropertyKey IsDraggingNodePropertyKey =
            DependencyProperty.RegisterReadOnly("IsDraggingNode", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingNodeProperty = IsDraggingNodePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsNotDraggingNodePropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDraggingNode", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingNodeProperty = IsDraggingNodePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsDragging", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsNotDraggingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsNotDragging", typeof(bool), typeof(NetworkView),
                new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsNotDraggingProperty = IsNotDraggingPropertyKey.DependencyProperty;
 
        public static readonly DependencyProperty NodeItemTemplateProperty =
            DependencyProperty.Register("NodeItemTemplate", typeof(DataTemplate), typeof(NetworkView));

        public static readonly DependencyProperty NodeItemTemplateSelectorProperty =
            DependencyProperty.Register("NodeItemTemplateSelector", typeof(DataTemplateSelector), typeof(NetworkView));

        public static readonly DependencyProperty NodeItemContainerStyleProperty =
            DependencyProperty.Register("NodeItemContainerStyle", typeof(Style), typeof(NetworkView));

        public static readonly DependencyProperty ConnectionItemTemplateProperty =
            DependencyProperty.Register("ConnectionItemTemplate", typeof(DataTemplate), typeof(NetworkView));

        public static readonly DependencyProperty ConnectionItemTemplateSelectorProperty =
            DependencyProperty.Register("ConnectionItemTemplateSelector", typeof(DataTemplateSelector), typeof(NetworkView));

        public static readonly DependencyProperty ConnectionItemContainerStyleProperty =
            DependencyProperty.Register("ConnectionItemContainerStyle", typeof(Style), typeof(NetworkView));

        public static readonly RoutedEvent NodeDragStartedEvent =
            EventManager.RegisterRoutedEvent("NodeDragStarted", RoutingStrategy.Bubble, typeof(NodeDragStartedEventHandler), typeof(NetworkView));

        public static readonly RoutedEvent NodeDraggingEvent =
            EventManager.RegisterRoutedEvent("NodeDragging", RoutingStrategy.Bubble, typeof(NodeDraggingEventHandler), typeof(NetworkView));

        public static readonly RoutedEvent NodeDragCompletedEvent =
            EventManager.RegisterRoutedEvent("NodeDragCompleted", RoutingStrategy.Bubble, typeof(NodeDragCompletedEventHandler), typeof(NetworkView));

        public static readonly RoutedEvent ConnectionDragStartedEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragStarted", RoutingStrategy.Bubble, typeof(ConnectionDragStartedEventHandler), typeof(NetworkView));

        public static readonly RoutedEvent QueryConnectionFeedbackEvent =
            EventManager.RegisterRoutedEvent("QueryConnectionFeedback", RoutingStrategy.Bubble, typeof(QueryConnectionFeedbackEventHandler), typeof(NetworkView));

        public static readonly RoutedEvent ConnectionDraggingEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragging", RoutingStrategy.Bubble, typeof(ConnectionDraggingEventHandler), typeof(NetworkView));

        public static readonly RoutedEvent ConnectionDragCompletedEvent =
            EventManager.RegisterRoutedEvent("ConnectionDragCompleted", RoutingStrategy.Bubble, typeof(ConnectionDragCompletedEventHandler), typeof(NetworkView));

        public static readonly RoutedCommand SelectAllCommand;
        public static readonly RoutedCommand SelectNoneCommand;
        public static readonly RoutedCommand InvertSelectionCommand;
        public static readonly RoutedCommand CancelConnectionDraggingCommand;

        /// <summary>
        /// Cached reference to the NodeItemsControl in the visual-tree.
        /// </summary>
        private NodeItemsControl _nodeItemsControl;

        /// <summary>
        /// Cached reference to the ItemsControl for connections in the visual-tree.
        /// </summary>
        private ItemsControl _connectionItemsControl;

        /// <summary>
        /// Cached list of currently selected nodes.
        /// </summary>
        private List<object> _initialSelectedNodes;

        public NetworkView()
        {
            //
            // Create a collection to contain nodes.
            //
            Nodes = new ImpObservableCollection<object>();

            //
            // Create a collection to contain connections.
            //
            Connections = new ImpObservableCollection<object>();

            IsUndoRegisterEnabled = true;

            //
            // Default background is white.
            //
            //this.Background = Brushes.White;

            //
            // Add handlers for node and connector drag events.
            //
            AddHandler(NodeItem.NodeDragStartedEvent, new NodeDragStartedEventHandler(NodeIte_DragStarted));
            AddHandler(NodeItem.NodeDraggingEvent, new NodeDraggingEventHandler(NodeIte_Dragging));
            AddHandler(NodeItem.NodeDragCompletedEvent, new NodeDragCompletedEventHandler(NodeIte_DragCompleted));
            AddHandler(ConnectorItem.ConnectorDragStartedEvent, new ConnectorItemDragStartedEventHandler(ConnectorIte_DragStarted));
            AddHandler(ConnectorItem.ConnectorDraggingEvent, new ConnectorItemDraggingEventHandler(ConnectorIte_Dragging));
            AddHandler(ConnectorItem.ConnectorDragCompletedEvent, new ConnectorItemDragCompletedEventHandler(ConnectorIte_DragCompleted));
        }

        /// <summary>
        /// Event raised when the user starts dragging a node in the network.
        /// </summary>
        public event NodeDragStartedEventHandler NodeDragStarted
        {
            add => AddHandler(NodeDragStartedEvent, value);
            remove => RemoveHandler(NodeDragStartedEvent, value);
        }

        /// <summary>
        /// Event raised while user is dragging a node in the network.
        /// </summary>
        public event NodeDraggingEventHandler NodeDragging
        {
            add => AddHandler(NodeDraggingEvent, value);
            remove => RemoveHandler(NodeDraggingEvent, value);
        }

        /// <summary>
        /// Event raised when the user has completed dragging a node in the network.
        /// </summary>
        public event NodeDragCompletedEventHandler NodeDragCompleted
        {
            add => AddHandler(NodeDragCompletedEvent, value);
            remove => RemoveHandler(NodeDragCompletedEvent, value);
        }

        /// <summary>
        /// Event raised when the user starts dragging a connector in the network.
        /// </summary>
        public event ConnectionDragStartedEventHandler ConnectionDragStarted
        {
            add => AddHandler(ConnectionDragStartedEvent, value);
            remove => RemoveHandler(ConnectionDragStartedEvent, value);
        }

        /// <summary>
        /// Event raised while user drags a connection over the connector of a node in the network.
        /// The event handlers should supply a feedback objects and data-template that displays the 
        /// object as an appropriate graphic.
        /// </summary>
        public event QueryConnectionFeedbackEventHandler QueryConnectionFeedback
        {
            add => AddHandler(QueryConnectionFeedbackEvent, value);
            remove => RemoveHandler(QueryConnectionFeedbackEvent, value);
        }

        /// <summary>
        /// Event raised when a connection is being dragged.
        /// </summary>
        public event ConnectionDraggingEventHandler ConnectionDragging
        {
            add => AddHandler(ConnectionDraggingEvent, value);
            remove => RemoveHandler(ConnectionDraggingEvent, value);
        }

        /// <summary>
        /// Event raised when the user has completed dragging a connection in the network.
        /// </summary>
        public event ConnectionDragCompletedEventHandler ConnectionDragCompleted
        {
            add => AddHandler(ConnectionDragCompletedEvent, value);
            remove => RemoveHandler(ConnectionDragCompletedEvent, value);
        }

        /// <summary>
        /// Collection of nodes in the network.
        /// </summary>
        public ImpObservableCollection<object> Nodes
        {
            get => (ImpObservableCollection<object>)GetValue(NodesProperty);
            private set => SetValue(NodesPropertyKey, value);
        }

        /// <summary>
        /// Collection of connections in the network.
        /// </summary>
        public ImpObservableCollection<object> Connections
        {
            get => (ImpObservableCollection<object>)GetValue(ConnectionsProperty);
            private set => SetValue(ConnectionsPropertyKey, value);
        }

        /// <summary>
        /// A reference to the collection that is the source used to populate 'Connections'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable ConnectionsSource
        {
            get => (IEnumerable)GetValue(ConnectionsSourceProperty);
            set => SetValue(ConnectionsSourceProperty, value);
        }

        /// <summary>
        /// A reference to the collection that is the source used to populate 'Nodes'.
        /// Used in the same way as 'ItemsSource' in 'ItemsControl'.
        /// </summary>
        public IEnumerable NodesSource
        {
            get => (IEnumerable)GetValue(NodesSourceProperty);
            set => SetValue(NodesSourceProperty, value);
        }

        /// <summary>
        /// Set to 'true' to enable the clearing of selection when empty space is clicked.
        /// This is set to 'true' by default.
        /// </summary>
        public bool IsClearSelectionOnEmptySpaceClickEnabled
        {
            get => (bool) GetValue(IsClearSelectionOnEmptySpaceClickEnabledProperty);
            set => SetValue(IsClearSelectionOnEmptySpaceClickEnabledProperty, value);
        }

        /// <summary>
        /// Set to 'true' to enable drag out of connectors to create new connections.
        /// </summary>
        public bool EnableConnectionDragging
        {
            get => (bool)GetValue(EnableConnectionDraggingProperty);
            set => SetValue(EnableConnectionDraggingProperty, value);
        }

        /// <summary>
        /// Dependency property that is set to 'true' when the user is 
        /// dragging out a connection.
        /// </summary>
        public bool IsDraggingConnection
        {
            get => (bool)GetValue(IsDraggingConnectionProperty);
            private set => SetValue(IsDraggingConnectionPropertyKey, value);
        }

        /// <summary>
        /// Dependency property that is set to 'false' when the user is 
        /// dragging out a connection.
        /// </summary>
        public bool IsNotDraggingConnection
        {
            get => (bool)GetValue(IsNotDraggingConnectionProperty);
            private set => SetValue(IsNotDraggingConnectionPropertyKey, value);
        }

        /// <summary>
        /// Set to 'true' to enable dragging of nodes.
        /// </summary>
        public bool EnableNodeDragging
        {
            get => (bool)GetValue(EnableNodeDraggingProperty);
            set => SetValue(EnableNodeDraggingProperty, value);
        }

        /// <summary>
        /// Dependency property that is set to 'true' when the user is 
        /// dragging out a connection.
        /// </summary>
        public bool IsDraggingNode
        {
            get => (bool)GetValue(IsDraggingNodeProperty);
            private set => SetValue(IsDraggingNodePropertyKey, value);
        }

        /// <summary>
        /// Dependency property that is set to 'false' when the user is 
        /// dragging out a connection.
        /// </summary>
        public bool IsNotDraggingNode
        {
            get => (bool)GetValue(IsNotDraggingNodeProperty);
            private set => SetValue(IsNotDraggingNodePropertyKey, value);
        }

        /// <summary>
        /// Set to 'true' when the user is dragging either a node or a connection.
        /// </summary>
        public bool IsDragging
        {
            get => (bool)GetValue(IsDraggingProperty);
            private set => SetValue(IsDraggingPropertyKey, value);
        }

        /// <summary>
        /// Set to 'true' when the user is not dragging anything.
        /// </summary>
        public bool IsNotDragging
        {
            get => (bool)GetValue(IsNotDraggingProperty);
            private set => SetValue(IsNotDraggingPropertyKey, value);
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each node item.
        /// This is the equivalent to 'ItemTemplate' for ItemsControl.
        /// </summary>
        public DataTemplate NodeItemTemplate
        {
            get => (DataTemplate)GetValue(NodeItemTemplateProperty);
            set => SetValue(NodeItemTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector NodeItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(NodeItemTemplateSelectorProperty);
            set => SetValue(NodeItemTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the Style that is applied to the item container for each node item.
        /// This is the equivalent to 'ItemContainerStyle' for ItemsControl.
        /// </summary>
        public Style NodeItemContainerStyle
        {
            get => (Style)GetValue(NodeItemContainerStyleProperty);
            set => SetValue(NodeItemContainerStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the DataTemplate used to display each connection item.
        /// This is the equivalent to 'ItemTemplate' for ItemsControl.
        /// </summary>
        public DataTemplate ConnectionItemTemplate
        {
            get => (DataTemplate)GetValue(ConnectionItemTemplateProperty);
            set => SetValue(ConnectionItemTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets custom style-selection logic for a style that can be applied to each generated container element. 
        /// This is the equivalent to 'ItemTemplateSelector' for ItemsControl.
        /// </summary>
        public DataTemplateSelector ConnectionItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(ConnectionItemTemplateSelectorProperty);
            set => SetValue(ConnectionItemTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the Style that is applied to the item container for each connection item.
        /// This is the equivalent to 'ItemContainerStyle' for ItemsControl.
        /// </summary>
        public Style ConnectionItemContainerStyle
        {
            get => (Style)GetValue(ConnectionItemContainerStyleProperty);
            set => SetValue(ConnectionItemContainerStyleProperty, value);
        }

        /// <summary>
        /// A reference to currently selected node.
        /// </summary>
        public object SelectedNode
        {
            get
            {
                if (_nodeItemsControl != null)
                {
                    return _nodeItemsControl.SelectedItem;
                }

                if (_initialSelectedNodes == null)
                {
                    return null;
                }

                if (_initialSelectedNodes.Count != 1)
                {
                    return null;
                }

                return _initialSelectedNodes[0];
            }
            set
            {
                if (_nodeItemsControl != null)
                {
                    _nodeItemsControl.SelectedItem = value;
                }
                else
                {
                    if (_initialSelectedNodes == null)
                    {
                        _initialSelectedNodes = new List<object>();
                    }

                    _initialSelectedNodes.Clear();
                    _initialSelectedNodes.Add(value);
                }
            }
        }

        /// <summary>
        /// A list of selected nodes.
        /// </summary>
        public IList SelectedNodes
        {
            get
            {
                if (_nodeItemsControl != null)
                {
                    return _nodeItemsControl.SelectedItems;
                }

                if (_initialSelectedNodes == null)
                {
                    _initialSelectedNodes = new List<object>();
                }

                return _initialSelectedNodes;
            }
        }

        /// <summary>
        /// Disable/enable the undo/redo register
        /// </summary>
        public bool IsUndoRegisterEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// An event raised when the nodes selected in the NetworkView has changed.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Bring the currently selected nodes into view.
        /// This affects ContentViewportOffsetX/ContentViewportOffsetY, but doesn't affect 'ContentScale'.
        /// </summary>
        public void BringSelectedNodesIntoView()
        {
            BringNodesIntoView(SelectedNodes);
        }

        /// <summary>
        /// Bring the collection of nodes into view.
        /// This affects ContentViewportOffsetX/ContentViewportOffsetY, but doesn't affect 'ContentScale'.
        /// </summary>
        public void BringNodesIntoView(ICollection nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException("'nodes' argument shouldn't be null.");
            }

            if (nodes.Count == 0)
            {
                return;
            }

            Rect rect = Rect.Empty;

            foreach (var node in nodes)
            {
                NodeItem nodeItem = FindAssociatedNodeItem(node);
                Rect nodeRect = new Rect(nodeItem.X, nodeItem.Y, nodeItem.ActualWidth, nodeItem.ActualHeight);

                if (rect == Rect.Empty)
                {
                    rect = nodeRect;
                }
                else
                {
                    rect.Intersect(nodeRect);
                }
            }

            BringIntoView(rect);
        }

        /// <summary>
        /// Clear the selection.
        /// </summary>
        public void SelectNone()
        {
            SelectedNodes.Clear();
        }

        /// <summary>
        /// Selects all of the nodes.
        /// </summary>
        public void SelectAll()
        {
            if (SelectedNodes.Count != Nodes.Count)
            {
                SelectedNodes.Clear();
                foreach (var node in Nodes)
                {
                    SelectedNodes.Add(node);
                }
            }
        }

        /// <summary>
        /// Inverts the current selection.
        /// </summary>
        public void InvertSelection()
        {
            var selectedNodesCopy = new ArrayList(SelectedNodes);
            SelectedNodes.Clear();

            foreach (var node in Nodes)
            {
                if (!selectedNodesCopy.Contains(node))
                {
                    SelectedNodes.Add(node);
                }
            }
        }

        /// <summary>
        /// When connection dragging is progress this function cancels it.
        /// </summary>
        public void CancelConnectionDragging()
        {
            if (!IsDraggingConnection)
            {
                return;
            }

            //
            // Now that connection dragging has completed, don't any feedback adorner.
            //
            ClearFeedbackAdorner();

            _draggedOutConnectorItem.CancelConnectionDragging();

            IsDragging = false;
            IsNotDragging = true;
            IsDraggingConnection = false;
            IsNotDraggingConnection = true;
            _draggedOutConnectorItem = null;
            _draggedOutNodeDataContext = null;
            _draggedOutConnectorDataContext = null;
            _draggingConnectionDataContext = null;
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static NetworkView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NetworkView), new FrameworkPropertyMetadata(typeof(NetworkView)));

            InputGestureCollection inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            SelectAllCommand = new RoutedCommand("SelectAll", typeof(NetworkView), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.Escape));
            SelectNoneCommand = new RoutedCommand("SelectNone", typeof(NetworkView), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.I, ModifierKeys.Control));
            InvertSelectionCommand = new RoutedCommand("InvertSelection", typeof(NetworkView), inputs);

            CancelConnectionDraggingCommand = new RoutedCommand("CancelConnectionDragging", typeof(NetworkView));

            CommandBinding binding = new CommandBinding
            {
                Command = SelectAllCommand
            };
            binding.Executed += SelectAll_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(NetworkView), binding);

            binding = new CommandBinding
            {
                Command = SelectNoneCommand
            };
            binding.Executed += SelectNone_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(NetworkView), binding);

            binding = new CommandBinding
            {
                Command = InvertSelectionCommand
            };
            binding.Executed += InvertSelection_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(NetworkView), binding);

            binding = new CommandBinding
            {
                Command = CancelConnectionDraggingCommand
            };
            binding.Executed += CancelConnectionDragging_Executed;
            CommandManager.RegisterClassCommandBinding(typeof(NetworkView), binding);
        }

        /// <summary>
        /// Executes the 'SelectAll' command.
        /// </summary>
        private static void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NetworkView c = (NetworkView)sender;
            c.SelectAll();
        }

        /// <summary>
        /// Executes the 'SelectNone' command.
        /// </summary>
        private static void SelectNone_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NetworkView c = (NetworkView)sender;
            c.SelectNone();
        }

        /// <summary>
        /// Executes the 'InvertSelection' command.
        /// </summary>
        private static void InvertSelection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NetworkView c = (NetworkView)sender;
            c.InvertSelection();
        }

        /// <summary>
        /// Executes the 'CancelConnectionDragging' command.
        /// </summary>
        private static void CancelConnectionDragging_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NetworkView c = (NetworkView)sender;
            c.CancelConnectionDragging();
        }

        /// <summary>
        /// Event raised when a new collection has been assigned to the 'NodesSource' property.
        /// </summary>
        private static void NodesSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkView c = (NetworkView)d;

            //
            // Clear 'Nodes'.
            //
            c.Nodes.Clear();

            if (e.OldValue is INotifyCollectionChanged notifyCollectionChanged)
            {
                //
                // Unhook events from previous collection.
                //
                notifyCollectionChanged.CollectionChanged -= c.NodesSource_CollectionChanged;
            }

            if (e.NewValue != null)
            {
                if (e.NewValue is IEnumerable enumerable)
                {
                    //
                    // Populate 'Nodes' from 'NodesSource'.
                    //
                    foreach (object obj in enumerable)
                    {
                        c.Nodes.Add(obj);
                    }
                }

                if (e.NewValue is INotifyCollectionChanged notifyCollectionChanged2)
                {
                    //
                    // Hook events in new collection.
                    //
                    notifyCollectionChanged2.CollectionChanged += c.NodesSource_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// Event raised when a node has been added to or removed from the collection assigned to 'NodesSource'.
        /// </summary>
        private void NodesSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //
                // 'NodesSource' has been cleared, also clear 'Nodes'.
                //
                Nodes.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    //
                    // For each item that has been removed from 'NodesSource' also remove it from 'Nodes'.
                    //
                    foreach (object obj in e.OldItems)
                    {
                        Nodes.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    //
                    // For each item that has been added to 'NodesSource' also add it to 'Nodes'.
                    //
                    foreach (object obj in e.NewItems)
                    {
                        Nodes.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Event raised when a new collection has been assigned to the 'ConnectionsSource' property.
        /// </summary>
        private static void ConnectionsSource_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NetworkView c = (NetworkView)d;

            //
            // Clear 'Connections'.
            //
            c.Connections.Clear();

            if (e.OldValue != null)
            {
                if (e.NewValue is INotifyCollectionChanged notifyCollectionChanged)
                {
                    //
                    // Unhook events from previous collection.
                    //
                    notifyCollectionChanged.CollectionChanged -= c.ConnectionsSource_CollectionChanged;
                }
            }

            if (e.NewValue != null)
            {
                if (e.NewValue is IEnumerable enumerable)
                {
                    //
                    // Populate 'Connections' from 'ConnectionsSource'.
                    //
                    foreach (object obj in enumerable)
                    {
                        c.Connections.Add(obj);
                    }
                }

                if (e.NewValue is INotifyCollectionChanged notifyCollectionChanged)
                {
                    //
                    // Hook events in new collection.
                    //
                    notifyCollectionChanged.CollectionChanged += c.ConnectionsSource_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// Event raised when a connection has been added to or removed from the collection assigned to 'ConnectionsSource'.
        /// </summary>
        private void ConnectionsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //
                // 'ConnectionsSource' has been cleared, also clear 'Connections'.
                //
                Connections.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    //
                    // For each item that has been removed from 'ConnectionsSource' also remove it from 'Connections'.
                    //
                    foreach (object obj in e.OldItems)
                    {
                        Connections.Remove(obj);
                    }
                }

                if (e.NewItems != null)
                {
                    //
                    // For each item that has been added to 'ConnectionsSource' also add it to 'Connections'.
                    //
                    foreach (object obj in e.NewItems)
                    {
                        Connections.Add(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Called after the visual tree of the control has been built.
        /// Search for and cache references to named parts defined in the XAML control template for NetworkView.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //
            // Cache the parts of the visual tree that we need access to later.
            //

            _nodeItemsControl = (NodeItemsControl)Template.FindName("PART_NodeItemsControl", this);
            if (_nodeItemsControl == null)
            {
                throw new ApplicationException("Failed to find 'PART_NodeItemsControl' in the visual tree for 'NetworkView'.");
            }

            //
            // Synchronize initial selected nodes to the NodeItemsControl.
            //
            if (_initialSelectedNodes != null && _initialSelectedNodes.Count > 0)
            {
                foreach (var node in _initialSelectedNodes)
                {
                    _nodeItemsControl.SelectedItems.Add(node);
                }
            }

            _initialSelectedNodes = null; // Don't need this any more.

            _nodeItemsControl.SelectionChanged += nodeItemsControl_SelectionChanged;

            _connectionItemsControl = (ItemsControl)Template.FindName("PART_ConnectionItemsControl", this);
            if (_connectionItemsControl == null)
            {
                throw new ApplicationException("Failed to find 'PART_ConnectionItemsControl' in the visual tree for 'NetworkView'.");
            }

            _dragSelectionCanvas = (FrameworkElement)Template.FindName("PART_DragSelectionCanvas", this);
            if (_dragSelectionCanvas == null)
            {
                throw new ApplicationException("Failed to find 'PART_DragSelectionCanvas' in the visual tree for 'NetworkView'.");
            }

            _dragSelectionBorder = (FrameworkElement)Template.FindName("PART_DragSelectionBorder", this);
            if (_dragSelectionBorder == null)
            {
                throw new ApplicationException("Failed to find 'PART_dragSelectionBorder' in the visual tree for 'NetworkView'.");
            }
        }

        /// <summary>
        /// Event raised when the selection in 'nodeItemsControl' changes.
        /// </summary>
        private void nodeItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, e.RemovedItems, e.AddedItems));
        }

        /// <summary>
        /// Find the max ZIndex of all the nodes.
        /// </summary>
        internal int FindMaxZIndex()
        {
            if (_nodeItemsControl == null)
            {
                return 0;
            }

            int maxZ = 0;

            for (int nodeIndex = 0; ; ++nodeIndex)
            {
                NodeItem nodeItem = (NodeItem)_nodeItemsControl.ItemContainerGenerator.ContainerFromIndex(nodeIndex);
                if (nodeItem == null)
                {
                    break;
                }

                if (nodeItem.ZIndex > maxZ)
                {
                    maxZ = nodeItem.ZIndex;
                }
            }

            return maxZ;
        }

        /// <summary>
        /// Find the NodeItem UI element that is associated with 'node'.
        /// 'node' can be a view-model object, in which case the visual-tree
        /// is searched for the associated NodeItem.
        /// Otherwise 'node' can actually be a 'NodeItem' in which case it is 
        /// simply returned.
        /// </summary>
        internal NodeItem FindAssociatedNodeItem(object node)
        {
            if (!(node is NodeItem nodeItem))
            {
                nodeItem = _nodeItemsControl.FindAssociatedNodeItem(node);
            }
            return nodeItem;
        }
    }
}
