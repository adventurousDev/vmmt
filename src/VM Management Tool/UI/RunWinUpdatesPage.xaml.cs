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
using VMManagementTool.Services;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for RunWinUpdatesPage.xaml
    /// </summary>
    public partial class RunWinUpdatesPage : Page
    {
        WinUpdatesManager winUpdateManager;

        public RunWinUpdatesPage()
        {
            InitializeComponent();
            Loaded += RunOptimizationsPage_Loaded;
            Unloaded += RunWinUpdatesPage_Unloaded;
        }

        private void RunWinUpdatesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            //unregister all events
            //todo is this really necessary: considering that 
            //this reference lives here in the Page only
            if (winUpdateManager != null)
            {
                winUpdateManager.CheckCompleted -= WinUpdateManager_CheckCompleted;
                winUpdateManager.DownloadCompleted -= WinUpdateManager_DownloadCompleted;
                winUpdateManager.InstallationCompleted -= WinUpdateManager_InstallationCompleted;
                winUpdateManager.ProgressChanged -= WinUpdateManager_ProgressChanged;

                winUpdateManager = null;
            }
        }

        private void RunOptimizationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            ResetProgress();
            //start with checking for updates right away
            winUpdateManager = new WinUpdatesManager();
            winUpdateManager.CheckCompleted += WinUpdateManager_CheckCompleted;
            winUpdateManager.ProgressChanged += WinUpdateManager_ProgressChanged;

            //checking reports no progress so indeterminate
            progressBar.IsIndeterminate = true;

            winUpdateManager.CheckForUpdates();

        }

        private void WinUpdateManager_ProgressChanged(int progress, string info)
        {
            progressBar.Value = progress;
            currentUpdateLblText.Text = info;
        }

        void ResetProgress()
        {
            progressBar.Value = 0;
            currentUpdateLblText.Text = "";
            progressBar.IsIndeterminate = false;
        }

        private void WinUpdateManager_CheckCompleted(bool found)
        {
            ResetProgress();
            if (found)
            {
                //continue to Download
                winUpdateManager.DownloadCompleted += WinUpdateManager_DownloadCompleted;
                winUpdateManager.DownloadUpdates();

            }
            else
            {
                //move on
                Proceed("check failed or nothing to install");

            }
        }

        private void WinUpdateManager_DownloadCompleted(bool success)
        {
            ResetProgress();
            if (success)
            {
                //continue to Install
                winUpdateManager.InstallationCompleted += WinUpdateManager_InstallationCompleted;
                winUpdateManager.InstallUpdates();
            }
            else
            {
                //move on
                Proceed("download failed!");

            }
        }

        private void WinUpdateManager_InstallationCompleted(bool success)
        {
            if (success)
            {
                //check if reboot needed and move on to reboot
                //otherwise just move on
            }
            else
            {
                //move on
                Proceed("install failed!");
            }
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {


        }
        void Proceed(string msg)
        {
            MessageBox.Show(msg);
        }
    }
}
