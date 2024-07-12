using ERModsMerger.Core;
using ERModsMerger.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ERModsManager.UCs
{
    /// <summary>
    /// Logique d'interaction pour FileTreeUC.xaml
    /// </summary>
    public partial class FileTreeUC : UserControl
    {
        public bool FileEnabled { get; set; }

        private string _currentPath = string.Empty;

        public FileTreeUC? parent { get; set; }

        public List<ModFileConfig> ModFileConfigs;

        public FileTreeUC()
        {
            InitializeComponent();
            ModFileConfigs = new List<ModFileConfig>();
            DataContext = this;

            FileEnabled = true;
        }

        public void Load(string folderPath, bool showHeadFolder = false)
        {
            _currentPath = folderPath;

            int foundIndex = ModFileConfigs.FindIndex(x => x.Path == _currentPath);

            if(foundIndex == -1)
                ModFileConfigs.Add(new ModFileConfig(_currentPath, true));
            else
                FileEnabled = ModFileConfigs[foundIndex].Enabled;

            if (!showHeadFolder)
            {
                Header.Visibility = Visibility.Hidden;
                StackPanelSubFileTree.Margin = new Thickness(0);
                StackPanelSubFileTree.Height = double.NaN;
            }
                

            List<string> paths = new List<string>();

            Utils.FindAllFiles(folderPath, ref paths, false, true);

            HeaderName.Content = folderPath.Split("\\").Last();

            foreach (string path in paths)
            {
                FileAttributes attr = File.GetAttributes(path);

                FileTreeUC filetree = new FileTreeUC();
                filetree.ModFileConfigs = ModFileConfigs;
                filetree.parent = this;
                StackPanelSubFileTree.Children.Add(filetree);
                filetree.Load(path, true);

                if (!attr.HasFlag(FileAttributes.Directory)) // path is a directory / folder
                {
                    filetree.imgFolder.Visibility = Visibility.Hidden;
                    filetree.imgFile.Visibility = Visibility.Visible;
                }
            }
        }

        private void Header_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(StackPanelSubFileTree.Height != 0)
                StackPanelSubFileTree.Height = 0;
            else
                StackPanelSubFileTree.Height = double.NaN;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }


        public void SwitchEnableRecursive(bool enabled, bool direct = false)
        {
            CheckBoxEnableFileFolder.IsChecked = enabled;
            foreach (var child in StackPanelSubFileTree.Children)
            {
                ((FileTreeUC)child).SwitchEnableRecursive(enabled);
            }

            //recursive up when enabled
            if(enabled && parent != null && !parent.FileEnabled)
                parent.SwitchEnableRecursiveUp();

            var fileConfig = ModFileConfigs.Find(x=>x.Path == _currentPath);
            if (fileConfig != null)
            {
                fileConfig.Enabled = enabled;
            }
        }

        public void SwitchEnableRecursiveUp()
        {
            CheckBoxEnableFileFolder.IsChecked = true;
            //recursive up when enabled
            if (parent != null && !parent.FileEnabled)
                parent.SwitchEnableRecursiveUp();

            var fileConfig = ModFileConfigs.Find(x => x.Path == _currentPath);
            if (fileConfig != null)
            {
                fileConfig.Enabled = true;
            }
        }

        private void CheckBoxEnableFileFolder_Click(object sender, RoutedEventArgs e)
        {
            SwitchEnableRecursive(FileEnabled);
            ModsMergerConfig.LoadedConfig.Save();
        }
    }
}
