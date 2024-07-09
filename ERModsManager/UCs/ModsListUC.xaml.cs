using ERModsMerger.Core;
using ERModsMerger.Core.Utility;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ERModsManager.UCs
{
    /// <summary>
    /// Logique d'interaction pour ModsListUC.xaml
    /// </summary>
    public partial class ModsListUC : UserControl
    {
        public ModsListUC()
        {
            InitializeComponent();

            foreach(var mod in ModsMergerConfig.LoadedConfig.Mods)
            {
                var modItem = new ModItemUC();
                modItem.ModName = mod.Name;
                modItem.CurrentPath = mod.Path;
                modItem.ModEnabled = mod.Enabled;
                modItem.ModDeleted += ModItem_ModDeleted;
                modItem.MouseDown += ModItem_MouseDown;
                modItem.ModEnabledChanged += ModItem_ModEnabledChanged;
                ModsListStackPanel.Children.Add(modItem);
            }
        }

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    var ext = System.IO.Path.GetExtension(file);
                    var name = System.IO.Path.GetFileName(file);
                    var nameNoExt = System.IO.Path.GetFileNameWithoutExtension(file);

                    if(FIlesTreeAligner.TryAlignAndCopyToModsToMerge(file, nameNoExt))
                    {
                        var modItem = new ModItemUC();
                        modItem.ModName = nameNoExt;
                        modItem.InitialFilePath = file;
                        modItem.CurrentPath = ModsMergerConfig.LoadedConfig.AppDataFolderPath + "\\ModsToMerge\\" + nameNoExt;
                        modItem.ModDeleted += ModItem_ModDeleted;
                        modItem.MouseDown += ModItem_MouseDown;
                        modItem.ModEnabledChanged += ModItem_ModEnabledChanged;
                        ModsListStackPanel.Children.Add(modItem);

                        ModsMergerConfig.LoadedConfig.Mods.Add(new ModConfig(modItem.ModName, modItem.CurrentPath, true));
                        ModsMergerConfig.SaveConfig(Global.ConfigFilePath);
                    }
                    else // nope
                    {
                        string mess = "Unable to load this mod, format or folder structure might be too complicated to parse.\n\n" +
                                      "Try to respect this structure:\n\n" +
                                      "📂  MyMod\n" +
                                      "   📄  regulation.bin\n" +
                                      "   📂  event\n" +
                                      "      📄  m10_00_00_00.emevd.dcx\n" +
                                      "      📄  m12_01_00_00.emevd.dcx\n" +
                                      "      📄  ...\n" +
                                      "   📂  parts\n" +
                                      "      📄  am_f_0000.partsbnd.dcx\n" +
                                      "      📄  wp_a_0424_l.partsbnd.dcx\n" +
                                      "      📄  ...\n\n\n" +
                                      "*Don't drop folder / .zip that contain more than 1 mod / version.\n" +
                                      "*Only drop folder or .zip, if you want to drop only one file (eg: regulation.bin), it must be in a named folder (it's important so the app know what's the name of your mod).";

                        MessageBox.Show(mess, "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ModItem_ModEnabledChanged(object? sender, EventArgs e)
        {
            ModItemUC item = ((ModItemUC)sender);


            if (item.ModEnabled)
               ModsMergerConfig.LoadedConfig.Mods.Find(x=>x.Name == item.ModName).Enabled = true;
            else
                ModsMergerConfig.LoadedConfig.Mods.Find(x => x.Name == item.ModName).Enabled = false;


            ModsMergerConfig.SaveConfig(Global.ConfigFilePath);
        }

        private void ModItem_ModDeleted(object? sender, EventArgs e)
        {
            var Result = MessageBox.Show($"Delete: {((ModItemUC)sender).ModName}?", "", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (Result == MessageBoxResult.Yes)
            {
                ModItemUC item = ((ModItemUC) sender);
                ModsListStackPanel.Children.Remove(item);
                Directory.Delete(item.CurrentPath, true);

                ModsMergerConfig.LoadedConfig.Mods.RemoveAll(x=>x.Name==item.ModName);
                ModsMergerConfig.SaveConfig(Global.ConfigFilePath);
            }
        }

        ModItemUC? MoveableModItemUC;
        private void ModItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var modItem = sender as ModItemUC;
            this.MouseMove += ModsListUC_MouseMove;
            this.MouseLeave += ModsListUC_MouseLeave;
            this.MouseUp += ModsListUC_MouseUp;

            this.Cursor = Cursors.SizeNS;

            MoveableModItemUC = sender as ModItemUC;
            MoveableModItemUC.Background = new SolidColorBrush(Color.FromArgb(50,255,255,255));
        }

        private void ModsListUC_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.MouseMove -= ModsListUC_MouseMove;
            this.MouseLeave -= ModsListUC_MouseLeave;
            this.MouseUp -= ModsListUC_MouseUp;
            this.Cursor = Cursors.Arrow;
            MoveableModItemUC.Background = new SolidColorBrush(Color.FromArgb(190, 0, 0, 0));
        }

        private void ModsListUC_MouseMove(object sender, MouseEventArgs e)
        {
            var currentModItemRelativeLocation = GetRelativePos(MoveableModItemUC, ModsListStackPanel);
            var mouseRelativeLocation =  e.GetPosition(ModsListStackPanel);

            int currentIndex = ModsListStackPanel.Children.IndexOf(MoveableModItemUC);
            int indexElemAbove = currentIndex -1;
            int indexElemBelow = currentIndex + 1;

            if(indexElemAbove > -1 && mouseRelativeLocation.Y < GetRelativePos(ModsListStackPanel.Children[indexElemAbove], ModsListStackPanel).Y)
            {
                //move up
                int indexInConfig = ModsMergerConfig.LoadedConfig.Mods.FindIndex(x => x.Name == MoveableModItemUC.ModName);
                var modItemConfig = ModsMergerConfig.LoadedConfig.Mods[indexInConfig];
                ModsMergerConfig.LoadedConfig.Mods.Remove(modItemConfig);
                ModsMergerConfig.LoadedConfig.Mods.Insert(indexElemAbove,modItemConfig);
                ModsMergerConfig.SaveConfig(Global.ConfigFilePath);

                ModsListStackPanel.Children.Remove(MoveableModItemUC);
                ModsListStackPanel.Children.Insert(indexElemAbove, MoveableModItemUC);
            }
            else if(indexElemBelow < ModsListStackPanel.Children.Count && mouseRelativeLocation.Y > GetRelativePos(ModsListStackPanel.Children[indexElemBelow], ModsListStackPanel).Y)
            {
                //move down
                int indexInConfig = ModsMergerConfig.LoadedConfig.Mods.FindIndex(x => x.Name == MoveableModItemUC.ModName);
                var modItemConfig = ModsMergerConfig.LoadedConfig.Mods[indexInConfig];
                ModsMergerConfig.LoadedConfig.Mods.Remove(modItemConfig);
                ModsMergerConfig.LoadedConfig.Mods.Insert(indexElemBelow, modItemConfig);
                ModsMergerConfig.SaveConfig(Global.ConfigFilePath);

                ModsListStackPanel.Children.Remove(MoveableModItemUC);
                ModsListStackPanel.Children.Insert(indexElemBelow, MoveableModItemUC);
            }
                    
         }

        private Point GetRelativePos(UIElement elem, UIElement container)
        {
            Point relative =  elem.TranslatePoint(new Point(0, 0), container);
            relative.Y += elem.RenderSize.Height / 2;

            return relative;
        }

        private void ModsListUC_MouseLeave(object sender, MouseEventArgs e)
        {
            this.MouseMove -= ModsListUC_MouseMove;
            this.MouseLeave -= ModsListUC_MouseLeave;
            this.MouseUp -= ModsListUC_MouseUp;
            this.Cursor = Cursors.Arrow;
            MoveableModItemUC.Background = new SolidColorBrush(Color.FromArgb(190, 0, 0, 0));
        }


       

    }
}
