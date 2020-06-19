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

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for InitPage.xaml
    /// </summary>
    public partial class InitPage : Page
    {
        public InitPage()
        {
            InitializeComponent();
            Loaded += InitPage_Loaded;
        }

        private async void InitPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded += InitPage_Loaded;

            /*
            await ConfigurationManager.Instance.Init(
                (text) => { Dispatcher.Invoke(() => progressLabel.Content = text); }
                ).ConfigureAwait(true);
                */
            ConfigurationManager.Instance.InitLight();

            var navTo = new HomePage();
            NavigationService.Navigate(navTo);
        }


    }
}
