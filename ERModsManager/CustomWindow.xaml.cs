using CommunityToolkit.Mvvm.Input;
using static System.Windows.WindowState;
using static System.Windows.Visibility;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using ERModsMerger.Core;
using System.Windows.Threading;

namespace ERModsManager
{
    /// <summary>
    /// Logique d'interaction pour CustomWindow.xaml
    /// </summary>
    public partial class CustomWindow
    {
        public CustomWindow()
        {
            Launch();
            InitializeComponent();

            LOG.WpfOutput = true;

            DataContext = this;
            StateChanged += (_, _) => RefreshMaximizeRestoreButton();

            RefreshMaximizeRestoreButton();
            MinimizeCommand = new RelayCommand(() => Minimize());
            MaximizeRestoreCommand = new RelayCommand(() => MaximizeRestore());
            CloseCommand = new RelayCommand(() => Close());

            InitSpecialButtons();
            this.Topmost = true;
        }

        void Launch()
        {
            if (!Directory.Exists(Global.ERMMAppDataFolder))
                FirstLaunch();

            if(ModsMergerConfig.LoadedConfig == null)
            {
                ModsMergerConfig.LoadConfig(Global.ConfigFilePath);
                ModsMergerConfig.SaveConfig(Global.ConfigFilePath); // save to apply eventual new properties
            }

            ModsMerger.MergeFinish += ModsMerger_MergeFinish;
        }

        

        void FirstLaunch()
        {
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ModsMergerConfig config = new ModsMergerConfig();

            if (!File.Exists(config.GamePath + "\\regulation.bin"))
            {
                MessageBox.Show("Hello there! Hope you're good. Seems it's your first launch of this app so welcome!\n\nBut Elden Ring game path couldn't be found, please tell me where it is.", "", MessageBoxButton.OK, MessageBoxImage.Question);
                config.GamePath = MainConfigUC.SearchForEldenRingPath();
                MainConfigUC.LabelGamePath.Content = config.GamePath;
            }
                

            config.AppDataFolderPath = appdataPath + "\\ERModsManager";
            Directory.CreateDirectory(config.AppDataFolderPath);

            config.ModsToMergeFolderPath = config.AppDataFolderPath + "\\ModsToMerge";
            Directory.CreateDirectory(config.ModsToMergeFolderPath);
            config.MergedModsFolderPath = config.AppDataFolderPath + "\\MergedMods";
            Directory.CreateDirectory(config.MergedModsFolderPath);

            ModsMergerConfig.LoadedConfig = config;

            //save and reload to trigger extraction
            ModsMergerConfig.SaveConfig(Global.ConfigFilePath);
            ModsMergerConfig.LoadConfig(Global.ConfigFilePath);
        }

        #region SpecialTopControls

        public RelayCommand ShowConfigUICommand { get; set; }
        public RelayCommand ShowLogsUICommand { get; set; }
        public RelayCommand HelpCommand { get; set; }

        void InitSpecialButtons()
        {
            ShowConfigUICommand = new RelayCommand(() =>
            {
                if (MainConfigUC.Visibility == Visibility.Visible)
                {
                    MainConfigUC.Visibility = Visibility.Hidden;
                    BtnConfigs.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    if (MainLogsUC.Visibility == Visibility.Visible)
                        ShowLogsUICommand.Execute(null);

                    if (MainHelpUC.Visibility == Visibility.Visible)
                        HelpCommand.Execute(null);

                    MainConfigUC.Visibility = Visibility.Visible;
                    BtnConfigs.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                }

            });

            ShowLogsUICommand = new RelayCommand(() =>
            {
                if (MainLogsUC.Visibility == Visibility.Visible)
                {
                    MainLogsUC.Visibility = Visibility.Hidden;
                    BtnLogs.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    if (MainConfigUC.Visibility == Visibility.Visible)
                        ShowConfigUICommand.Execute(null);

                    if (MainHelpUC.Visibility == Visibility.Visible)
                        HelpCommand.Execute(null);

                    MainLogsUC.Visibility = Visibility.Visible;
                    BtnLogs.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                }

            });

            HelpCommand = new RelayCommand(() =>
            {
                if (MainHelpUC.Visibility == Visibility.Visible)
                {
                    MainHelpUC.Visibility = Visibility.Hidden;
                    BtnHelp.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    if (MainConfigUC.Visibility == Visibility.Visible)
                        ShowConfigUICommand.Execute(null);

                    if (MainLogsUC.Visibility == Visibility.Visible)
                        ShowLogsUICommand.Execute(null);

                    MainHelpUC.Visibility = Visibility.Visible;
                    BtnHelp.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                }
            });
        }


        #endregion


        #region WindowControls


        public RelayCommand MinimizeCommand { get; set; }
        public RelayCommand MaximizeRestoreCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }


        private bool IsMaximized => WindowState == Maximized;
        public void Minimize() => WindowState = Minimized;
        public void MaximizeRestore() => WindowState = IsMaximized ? Normal : Maximized;
        private void RefreshMaximizeRestoreButton()
        {
            MaximizeButton.Visibility = IsMaximized ? Collapsed : Visible;
            RestoreButton.Visibility = IsMaximized ? Visible : Collapsed;
        }

        private void WindowChromeWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                DragMove();
        }

        #endregion



        private void BtnMerge_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
            Merge();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;

            string path = ModsMergerConfig.LoadedConfig.AppDataFolderPath + "\\ModEngine2\\launchmod_eldenring.bat";

            string command = "cd " + ModsMergerConfig.LoadedConfig.AppDataFolderPath + "\\ModEngine2\n" +
                            "modengine2_launcher.exe -t er -c config_eldenring.toml";

            File.WriteAllText(path, command);
            Process.Start(path);
            Thread.Sleep(500);
            this.Close();
        }

        private void Merge()
        {
            if (MainLogsUC.Visibility == Visibility.Hidden)
                ShowLogsUICommand.Execute(null);

            BtnMerge.IsEnabled = false;
            BtnPlay.IsEnabled = false;

            MainLogsUC.TxtLogs.Text = "";

            Task.Run(() =>
            {
                ModsMerger.StartMerge(false, true);
            });
        }

        private void ModsMerger_MergeFinish(bool finished)
        {
            Application.Current.Dispatcher.BeginInvoke(
             DispatcherPriority.Background,
             new Action(() => {
                 this.BtnMerge.IsEnabled = true;
                 this.BtnPlay.IsEnabled = true;
                 this.Topmost = true;
                 MainLogsUC.TxtLogs.Text += "\n\nMerging Done!";
             }));

        }
    }
}
