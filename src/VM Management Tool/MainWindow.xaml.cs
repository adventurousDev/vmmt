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
using VM_Management_Tool.Services;

namespace VM_Management_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WinUpdatesManager.Instance.NewInfo += LogUpdateInto;
            WinUpdatesManager.Instance.UpdatesFound += Instance_UpdatesFound;
            WinUpdatesManager.Instance.ReadyToInstall += Instance_ReadyToInstall;
        }

        private void Instance_ReadyToInstall()
        {
            install.IsEnabled = true;
        }

        private void Instance_UpdatesFound()
        {
            this.Dispatcher.Invoke(() =>
                download.IsEnabled = true
           );
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //
            //WinUpdatesManager.Instance.LoadHsitory();
            WinUpdatesManager.Instance.CheckForUpdates((bool)onlineCheckBox.IsChecked);
        }

        private void LogUpdateInto(string msg)
        {
            var date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            this.Dispatcher.Invoke(() =>
                 theConsole.AppendText(date + ": " + msg + Environment.NewLine)
            ) ;
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            WinUpdatesManager.Instance.AbortChecking();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            WinUpdatesManager.Instance.DownloadUpdates();
        }

        private void abortD_Click(object sender, RoutedEventArgs e)
        {
            WinUpdatesManager.Instance.AbortDownload();
        }

        private void theConsole_TextChanged(object sender, TextChangedEventArgs e)
        {
            theConsole.CaretIndex = theConsole.Text.Length;
            theConsole.ScrollToEnd();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            theConsole?.Clear();
        }

        private void install_Click(object sender, RoutedEventArgs e)
        {
            WinUpdatesManager.Instance.InstallUpdates();
        }

        private void abortInstall_Click(object sender, RoutedEventArgs e)
        {
            WinUpdatesManager.Instance.AbortInstall();
        }
    }
}
