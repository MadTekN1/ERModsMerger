using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Logique d'interaction pour ModItemUC.xaml
    /// </summary>
    public partial class ModItemUC : UserControl
    {
        string _modName = "I am the default pretty long mod name";
        public string ModName { get { return _modName; } set { _modName = value;} }

        string _initialFilePath = "I am the default pretty long mod name";
        public string InitialFilePath { get { return _initialFilePath; } set { _initialFilePath = value; } }

        public string CurrentPath { get; set; }

        bool _modEnabled = true;
        public bool ModEnabled { get { return _modEnabled; } set { _modEnabled = value; } }

        public event EventHandler? ModDeleted;

        public event EventHandler? ModEnabledChanged;

        public ModItemUC()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void DeleteModFromListBtn_Click(object sender, RoutedEventArgs e)
        {
            if(ModDeleted != null)
            {
                ModDeleted(this, EventArgs.Empty);
            }
        }

        private void CheckBoxEnableMod_Changed(object sender, RoutedEventArgs e)
        {
            if (ModEnabledChanged != null)
            {
                ModEnabledChanged(this, e);
            }
        }
    }

}
