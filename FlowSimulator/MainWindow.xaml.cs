using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using FlowGraphBase;
using FlowGraphBase.Logger;
using FlowGraphBase.Process;
using FlowGraphBase.Script;
using FlowSimulator.FlowGraphs;
using FlowSimulator.UI;
using Npgsql;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace FlowSimulator
{
    public partial class MainWindow : Window
    {
        private readonly string _UserSettingsFile = "userSettings.xml";
        private readonly string _DockSettingsFile = "dockSettings.xml";

        private MruManager _MruManager;
        private const string _RegistryPath = "Software\\Natixis\\FlowSimulator";

        private string _FileOpened = "";

        private double _LastLeft, _LastTop, _LastWidth, _LastHeight;

        internal static MainWindow Instance
        {
            get;
            private set;
        }

        internal FlowGraphManagerControl FlowGraphManagerControl => flowGraphManagerControl;

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            SetTitle();

            LogManager.Instance.WriteLine(LogVerbosity.Info, "Система визуальный разработки моделей машинного обучения - v{0} запущена", ver);
            VariableTypeInspector.SetDefaultValues();
            NamedVarEditTemplateManager.Initialize();

            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _MruManager = new MruManager();
                _MruManager.Initialize(
                    this,						// owner form
                    menuItemRecentFiles,        // Recent Files menu item
                    menuItemFile,		        // parent
                    _RegistryPath);			// Registry path to keep MRU list

                _MruManager.MruOpenEvent += delegate(object sender_, MruFileOpenEventArgs e_)
                {
                    SaveChangesOnDemand();
                    LoadFile(e_.FileName);
                };

                LoadSettings();

                if (string.IsNullOrWhiteSpace(_MruManager.GetFirstFileName) == false)
                {
                    LoadFile(_MruManager.GetFirstFileName);
                }

                _LastLeft = Left;
                _LastTop = Top;
                _LastWidth = Width;
                _LastHeight = Height;


                ProcessLauncher.Instance.StartLoop();

                //startProjectThings();
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteException(ex);
            }
        }
        
        private void startProjectThings()
        {

            Sequence newSeq = new Sequence("Основа");
            GraphDataManager.Instance.AddSequence(newSeq);

            FlowGraphControlViewModel wm = new FlowGraphControlViewModel(newSeq);
            FlowGraphManager.Instance.Add(wm);

            Instance.FlowGraphManagerControl.OpenGraphInNewTab(newSeq);
        }
        private void OnClosed(object sender, EventArgs e)
        {
            ProcessLauncher.Instance.StopLoop();
            LogManager.Instance.WriteLine(LogVerbosity.Info, "Завершено пользователем");

            try
            {
               //SaveSettings();
                //SaveChangesOnDemand();
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteException(ex);
            }
        }

        private void Resume_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessLauncher.Instance.Resume();
        }

        private void NextStep_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessLauncher.Instance.NextStep();
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessLauncher.Instance.Pause();
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessLauncher.Instance.Stop();
        }

        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewProject();
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenProject();
        }

        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveProject();
        }

        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAsProject();
        }

        private void ExitCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Exit();
        }

        //тестим бд
        private void dbTest_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            dbTest();
        }

        private void dbTest()
        {
            string connectionString = String.Format("Server={0};Port={1};" +
                                        "User Id={2}; Password={3};Database={4};",
                                        "localhost", 5432, "postgres",
                                        "sqwes", "testDB");

            NpgsqlConnection conn = null;

            conn = new NpgsqlConnection(connectionString);

            conn.Open();

            string fileName = "AbstractProject";

            string sqlQuery = "select file from table1 where name = 'AbstractProject'";
            NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, conn);

            ProjectManager.Clear();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlReader xmlReader = XmlReader.Create(new StringReader(cmd.ExecuteScalar().ToString()));
                xmlDoc.Load(xmlReader);

                XmlNode rootNode = xmlDoc.SelectSingleNode("FlowSimulator");

                if (rootNode != null
                    && rootNode.Attributes.GetNamedItem("version") != null)
                {
                    int version = int.Parse(rootNode.Attributes["version"].Value);
                }

                NamedVariableManager.Instance.Load(rootNode);
                FlowGraphManager.Instance.Load(rootNode); // GraphDataManager.Instance.Load(rootNode) done inside

                LogManager.Instance.WriteLine(LogVerbosity.Info, "'{0}' successfully loaded", fileName);
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteException(ex);
            }

            conn.Close();
        }

        private void MenuItemLayout_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            LayoutAnchorable l = item.Tag as LayoutAnchorable;
            l.IsVisible = !l.IsVisible;
            item.IsChecked = l.IsVisible;
        }

        private void Clear()
        {
            ProjectManager.Clear();
            
            var list = dockingManager1.Layout.Descendents().OfType<LayoutDocument>().ToList();

            foreach (LayoutDocument ld in list)
            {
                ld.Close();
            }

        }

        private void NewProject()
        {
            SaveChangesOnDemand();

            LogManager.Instance.WriteLine(LogVerbosity.Info, "Новый проект");
            LogManager.Instance.NbErrors = 0;

            _FileOpened = "";

            Clear();
            SetTitle();
            startProjectThings();
        }

        private void OpenProject()
        {
            SaveChangesOnDemand();

            OpenFileDialog form = new OpenFileDialog();
            form.Filter = "Xml files (*.xml)|*.xml";
            form.Multiselect = false;

            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadFile(form.FileName);
            }

            form.Dispose();
        }

        private void SaveProject()
        {
            if (string.IsNullOrWhiteSpace(_FileOpened))
            {
                SaveAsProject();
            }
            else
            {
                SaveFile(_FileOpened);
            }
        }

        private void SaveAsProject()
        {
            SaveFileDialog form = new SaveFileDialog();
            form.Filter = "Xml files (*.xml)|*.xml";

            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFile(form.FileName);
            }

            form.Dispose();
        }

        private void Exit()
        {
            Close();
        }

        private void LoadFile(string fileName_, bool addToMRU = true)
        {
            if (File.Exists(fileName_))
            {
                Clear();

                if (ProjectManager.OpenFile(fileName_))
                {
                    _FileOpened = fileName_;
                    SetTitle();

                    if (addToMRU)
                    {
                        _MruManager.Add(fileName_);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(this, 
                        "Can't load the file '" + fileName_ + "'. Please check the log.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _MruManager.Remove(fileName_);
                }
            }
            else
            {
                System.Windows.MessageBox.Show(this, 
                    "The file '" + fileName_ + "' doesn't exist.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _MruManager.Remove(fileName_);
            }
        }

        private void SaveFile(string fileName_)
        {
            if (ProjectManager.SaveFile(fileName_))
            {
                _MruManager.Add(fileName_);
                _FileOpened = fileName_;
            }
        }

        private void SaveChangesOnDemand()
        {

           if (System.Windows.MessageBox.Show(this, "Сохранить изменения ?", "", 
               MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
               {
                SaveProject();
               }
            
        }

        private void LoadSettings()
        {
            double l = Left, t = Top, w = Width, h = Height;
            WindowState winState = WindowState;

            if (File.Exists(_UserSettingsFile))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(_UserSettingsFile);

                XmlNode winNode = xmlDoc.SelectSingleNode("FlowSimulator/Window");

                int version = int.Parse(winNode.Attributes["version"].Value);

                l = int.Parse(winNode.Attributes["left"].Value);
                t = int.Parse(winNode.Attributes["top"].Value);
                w = int.Parse(winNode.Attributes["width"].Value);
                h = int.Parse(winNode.Attributes["height"].Value);
                winState = (WindowState)Enum.Parse(typeof(WindowState), winNode.Attributes["windowState"].Value);

                XmlNode rootNode = xmlDoc.SelectSingleNode("FlowSimulator");

            }

            if (File.Exists(_DockSettingsFile))
            {
                try
                {
                    var serializer = new XmlLayoutSerializer(dockingManager1);
                    serializer.LayoutSerializationCallback += (s, args) =>
                    {
                        if (args.Content is LayoutAnchorable layout)
                        {
                            if (layout.CanHide
                                && layout.IsHidden)
                            {
                                layout.Hide();
                            }
                        }
                    };

                    using (var stream = new StreamReader(_DockSettingsFile))
                        serializer.Deserialize(stream);
                }
                catch (Exception ex3)
                {
                    LogManager.Instance.WriteException(ex3);
                }
            }

            Left = l;
            Top = t;
            Width = w;
            Height = h;
            WindowState = winState;
        }

        private void SaveSettings()
        {
            var serializer = new XmlLayoutSerializer(dockingManager1);
            using (var stream = new StreamWriter(_DockSettingsFile))
            {
                serializer.Serialize(stream);
            }

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.AddRootNode("FlowSimulator");
            rootNode.AddAttribute("version", "1");
            XmlNode winNode = xmlDoc.CreateElement("Window");
            rootNode.AppendChild(winNode);
            winNode.AddAttribute("version", "1");

            if (WindowState == WindowState.Minimized)
            {
                winNode.AddAttribute("windowState", Enum.GetName(typeof(WindowState), WindowState.Normal));
                winNode.AddAttribute("left", _LastLeft.ToString());
                winNode.AddAttribute("top", _LastTop.ToString());
                winNode.AddAttribute("width", _LastWidth.ToString());
                winNode.AddAttribute("height", _LastHeight.ToString());
            }
            else
            {
                winNode.AddAttribute("windowState", Enum.GetName(typeof(WindowState), WindowState));
                winNode.AddAttribute("left", Left.ToString());
                winNode.AddAttribute("top", Top.ToString());
                winNode.AddAttribute("width", Width.ToString());
                winNode.AddAttribute("height", Height.ToString());
            }

            //             _SessionControl.SaveSettings(rootNode);
            //             _MessageControl.SaveSettings(rootNode);
            //             _TaskControl.SaveSettings(rootNode);
            //             _ReportControl.SaveSettings(rootNode);
            //             _LogControl.SaveSettings(rootNode);
            //             _ScriptControl.SaveSettings(rootNode);

            xmlDoc.Save(_UserSettingsFile);
        }

        private void SetTitle()
        {
            Title = "Система визуальной разработки моделей машинного обучения";

            if (string.IsNullOrWhiteSpace(_FileOpened) == false)
            {
                Title += " - " + _FileOpened;
            }
        }

        public void OpenScriptElement(ScriptElement el_)
        {
            var list = dockingManager1.Layout.Descendents().OfType<LayoutDocument>();

            foreach (LayoutDocument ld in list)
            {
                if (ld.Content is ScriptElementControl control)
                {
                    if (control.Script.Id == el_.Id)
                    {
                        ld.IsSelected = true;
                        return;
                    }
                }
            }

            var firstDocumentPane = dockingManager1.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            if (firstDocumentPane != null)
            {
                LayoutDocument doc = new LayoutDocument
                {
                    Title = el_.Name,
                    Content = new ScriptElementControl(el_)
                };
                firstDocumentPane.Children.Add(doc);
                doc.IsSelected = true;
            }
        }
    }
}
