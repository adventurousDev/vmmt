using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VMManagementTool.Session;
using VMManagementTool.UI;

namespace VMManagementTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<SessionStage> SessionStages { get; set; } = new ObservableCollection<SessionStage>();
        public MainWindow()
        {
            InitializeComponent();
            SessionManager.Instance.SessionStateChanged += Instance_SessionStateChanged;

            var args = Environment.GetCommandLineArgs();
            var dir = Directory.GetCurrentDirectory();
            frame.Navigated += Frame_Navigated;
            if (args.Length > 1 && args[1].Equals("/resume"))
            {
                ConfigurationManager.Instance.InitLight();
                SystemUtils.DeleteResumeTask();
                try
                {
                    SessionManager.Instance.LoadPausedSession();
                }
                catch (Exception ex)
                {
                    Log.Error("MainWindow.MainWindow", ex.ToString());
                    Log.Info("MainWindow.MainWindow", "Unable to load saved session: starting from the beginning");
                    frame.Navigate(new HomePage());
                    return;
                }

                SessionManager.Instance.ResumeOptimizationSession();
                frame.Navigate(new RunWinUpdatesPage(true));
            }
            else
            {

                frame.Navigate(new InitPage());
            }
        }

        private void Instance_SessionStateChanged(SessionManager.SessionState newState)
        {
            if (newState == SessionManager.SessionState.Active)
            {
                menu.IsEnabled = false;
                progressionSidebar.Visibility = Visibility.Visible;
                progressItems.ItemsSource = SessionStages;
            }
            else
            {
                menu.IsEnabled = true;
                //progressionSidebar.Visibility = Visibility.Collapsed;
            }
        }

        private void Frame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content is InitPage)
            {
                menu.IsEnabled = false;
            }
            else if (e.Content is HomePage)
            {
                menu.IsEnabled = true;
                progressionSidebar.Visibility = Visibility.Collapsed;
            }
            if(SessionManager.Instance.CurrentState == SessionManager.SessionState.Active)
            {
                SessionStages.Clear();
                var stages = SessionManager.Instance.GetSessionStages();
                if (stages != null)
                {
                    stages.ForEach(SessionStages.Add);
                }
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

        private void ViewHelpItemSelected(object sender, RoutedEventArgs e)
        {
            var helpUrl = ConfigurationManager.Instance.GetStringConfig(ConfigurationManager.CONFIG_KEY_HELP_URL);
            System.Diagnostics.Process.Start(helpUrl);
        }
    }
}
