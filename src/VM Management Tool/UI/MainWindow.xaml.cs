using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using VMManagementTool.Services;
using VMManagementTool.UI;

namespace VMManagementTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var args = Environment.GetCommandLineArgs();
            var dir = Directory.GetCurrentDirectory();
            frame.Navigated += Frame_Navigated;
            if (args.Length > 1 && args[1].Equals("/resume"))
            {
                ConfigurationManager.Instance.InitLight();
                SystemUtils.DeleteResumeTask();
                try
                {
                    VMMTSessionManager.Instance.LoadPausedSession();
                }
                catch (Exception ex)
                {
                    Log.Error("MainWindow.MainWindow", ex.Message);
                    Log.Info("MainWindow.MainWindow", "Unable to load saved session: starting from the beginning");
                    frame.Navigate(new HomePage());
                }
                
                frame.Navigate(new RunWinUpdatesPage(true));
            }
            else
            {

                frame.Navigate(new InitPage());
            }
        }

        private void Frame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if(e.Content is InitPage)
            {
                menu.IsEnabled = false;
            }
            else
            {
                menu.IsEnabled = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            TestWindow.Instance.Show();
            //TestWindow.Instance.Activate();
            TestWindow.Instance.WindowState = WindowState.Normal;

        }

        private void AboutMenuItemSelected(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            
            MessageBox.Show($"Version: {version.ToString(3)}");
        }

        private void UpdateMenuItemSelected(object sender, RoutedEventArgs e)
        {
            new UpdateWindow().ShowDialog();
        }

        private void ExitMenuItemSelected(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OptionsMenuItemSelected(object sender, RoutedEventArgs e)
        {
            new OptionsWindow().ShowDialog();
        }
    }
}
