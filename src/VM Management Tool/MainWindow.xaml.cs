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
using VM_Management_Tool.Services.Optimization;

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

            WinOptimizationsManager.Instance.NewInfo += LogUpdateInto;
            WinOptimizationsManager.Instance.SDeleteProgressChanged += Instance_SDeleteProgressChanged;
        }

        private void Instance_SDeleteProgressChanged(string stage, int percentage)
        {
            this.Dispatcher.Invoke(() =>
                {
                    progressStage.Content = stage;

                    if (percentage == -8)
                    {
                        progressBar.IsIndeterminate = true;
                    }
                    else if (percentage > 0)
                    {
                        progressBar.IsIndeterminate = false;
                        progressBar.Value = percentage;
                    }
                    else
                    {
                        progressBar.IsIndeterminate = false;
                        progressBar.Value = 0;
                    }

                }
           );

        }

        private void Instance_ReadyToInstall()
        {

            this.Dispatcher.Invoke(() =>
                install.IsEnabled = true
           );
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
            var outputmsg = date + ": " + msg + Environment.NewLine;
            this.Dispatcher.Invoke(() =>
                 theConsole.AppendText(outputmsg)
            );

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

        private void enableService_Click(object sender, RoutedEventArgs e)
        {
            WinServiceUtils.EnableService("wuauserv");
        }

        private void disableService_Click(object sender, RoutedEventArgs e)
        {
            WinServiceUtils.DisableService("wuauserv");
        }

        private async void startService_Click(object sender, RoutedEventArgs e)
        {
            LogUpdateInto("starting service...");

            bool result = await WinServiceUtils.StartServiceAsync("wuauserv", 5000);
            if (result)
            {
                LogUpdateInto("success!");
            }
            else
            {
                LogUpdateInto("fail!");
            }
        }

        private async void stopService_Click(object sender, RoutedEventArgs e)
        {
            LogUpdateInto("stopping service...");

            bool result = await WinServiceUtils.StopServiceAsync("wuauserv", 5000);
            if (result)
            {
                LogUpdateInto("success!");
            }
            else
            {
                LogUpdateInto("fail!");
            }
        }

        private void sdeleteBtn_Click(object sender, RoutedEventArgs e)
        {

            WinOptimizationsManager.Instance.RunSDelete();
        }

        private void cleanmgrBtn_Click(object sender, RoutedEventArgs e)
        {
            WinOptimizationsManager.Instance.RunCleanmgr();
        }

        private void hidewindow_Click(object sender, RoutedEventArgs e)
        {
            WinOptimizationsManager.Instance.HideCleanMgrWndow();
        }

        private void cleanmgrReg_Click(object sender, RoutedEventArgs e)
        {
            WinOptimizationsManager.Instance.TmpRegisty();
        }

        private void defragBtn_Click(object sender, RoutedEventArgs e)
        {
            //WinOptimizationsManager.Instance.RunDefragPS();
            WinOptimizationsManager.Instance.RunDefrag();
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            var template = new OptimizationTemplate();
            template.NewInfo += LogUpdateInto;
            string path = @"C:\Users\Student\Desktop\VMwareOSOptimizationTool_b1140_15488330\bwlp_Windows_10.xml";
            //string path = @"C:\Users\Student\Desktop\VMwareOSOptimizationTool_b1140_15488330\Hayk_Windows 10 1507-1803-Server 2016_comp.xml";
            template.Load(path);
            //template.PrintAllShellCMDs();
            var onlyRunSet = new HashSet<string>()
            {
                "Application Experience - Microsoft Compatibility Appraiser",
            };
            template.RunAll(onlyRunSet);
        }

        private void regtestButton_Click(object sender, RoutedEventArgs e)
        {
            var template = new CustomTests();
            template.NewInfo += LogUpdateInto;

            template.ReadRegValue(regKeyTextBox.Text, regValTextBox.Text);

        }

        private void regtestButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            var template = new CustomTests();
            template.NewInfo += LogUpdateInto;
            //template.CreateRegValue();
            //template.CreateRegKey();
            template.DeleteKey();
        }

        private void randomTest_Click(object sender, RoutedEventArgs e)
        {
            var template = new CustomTests();
            template.NewInfo += LogUpdateInto;
            template.TestServiceStuff();
        }
    }
}
