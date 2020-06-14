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
using System.Windows.Shapes;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {

        public OptionsWindow()
        {
            InitializeComponent();
            LoadValues();
            Loaded += OptionsWindow_Loaded;
        }

        private void OptionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OptionsWindow_Loaded;

        }

        void LoadValues()
        {
            var val = ConfigurationManager.Instance.GetUserSetting<long>("log", "level", 0);
            logDetailLevelDropdown.SelectedValue = val;
        }
        void SaveValues()
        {
            ConfigurationManager.Instance.SaveUserSetting("log", "level", (int)logDetailLevelDropdown.SelectedValue, false);
            ConfigurationManager.Instance.SaveUserSettingsToDisk();
            

        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            SaveValues();
            Close();
        }
    }
}
