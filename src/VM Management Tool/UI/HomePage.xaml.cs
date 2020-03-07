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
using VMManagementTool.UI;

namespace VMManagementTool
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            if (advancedRadioBtn.IsChecked ?? false)
            {
                var navTo = new AdvanceConfigPage();
                NavigationService.Navigate(navTo);
            }
            else
            {
                var navTo = new RunWinUpdatesPage();
                //var navTo = new RunOSOTTempaltePage();
                //var navTo = new RunCleanupOptimizations();
                NavigationService.Navigate(navTo);
            }
            
        }

        private void RadioBtn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            if (advancedRadioBtn?.IsChecked ?? false)
            {
                nextRunButton.Content="Configure";
            }
            else
            {
                nextRunButton.Content = "Run";
            }
        }
    }
}
