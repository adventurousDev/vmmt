using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using VMManagementTool.Test;
using VMManagementTool.Services;
using VMManagementTool.UIUtils;
using System.ComponentModel;
using VMManagementTool.Session;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for RunWinUpdatesPage.xaml
    /// </summary>
    public partial class RunWinUpdatesPage : Page
    {
        //WinUpdatesManager winUpdateManager;
        DummyWinUpdateManager winUpdateManager;
        volatile bool aborted = false;
        bool resumeInstall = false;
        WindowsUpdateSessionState sessionParams;
        public RunWinUpdatesPage(bool resume = false)
        {
            InitializeComponent();
            sessionParams = SessionManager.Instance.GetWinUpdateParams();

            resumeInstall = resume;

            Loaded += RunWinUpdatesPage_Loaded;


            var hostWin = Application.Current.MainWindow;
            hostWin.Closing += HostWin_Closing;
            Unloaded += (s, e) => { hostWin.Closing -= HostWin_Closing; };
        }

        private void HostWin_Closing(object sender, CancelEventArgs e)
        {

            Abort();
            Cleanup().Wait();
        }



        private async void RunWinUpdatesPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("RunWinUpdatesPage", $"Win Update Page Loaded; After reboot? : {resumeInstall}");
            Loaded -= RunWinUpdatesPage_Loaded;
            if (resumeInstall)
            {
                await ResumeAfterRestart().ConfigureAwait(false);
            }
            else
            {


                ResetProgress();
                StartInfiniteProgress("preparing...");
                var prepared = await Prepare().ConfigureAwait(false);
                if (aborted || !prepared)
                {
                    ResetProgress();

                    SetParagraphLook(checkParagrath, TextLook.Skipped);
                    SetParagraphLook(downloadParagrath, TextLook.Skipped);
                    SetParagraphLook(installParagrath, TextLook.Skipped);

                    FinishAndProceed();
                    return;
                }
                //start with checking for updates right away
                //winUpdateManager = new WinUpdatesManager();
                winUpdateManager = new DummyWinUpdateManager();

                winUpdateManager.CheckCompleted += WinUpdateManager_CheckCompleted;
                winUpdateManager.ProgressChanged += WinUpdateManager_ProgressChanged;

                //checking reports no progress so indeterminate
                StartInfiniteProgress("");

                SetParagraphLook(checkParagrath, TextLook.Processing);
                winUpdateManager.CheckForUpdates();
            }
        }

        private async Task ResumeAfterRestart()
        {
            StartInfiniteProgress("");
            SetParagraphLook(checkParagrath, TextLook.Completed);
            SetParagraphLook(downloadParagrath, TextLook.Completed);
            SetParagraphLook(installParagrath, TextLook.Processing);
            //delay for user to have to time to notice what is going on
            await Task.Delay(500);
            //todo consider checking that during/ after restart things went as required
            WinUpdateManager_InstallationCompleted(true, false);


        }

        private void StartInfiniteProgress(string label)
        {
            this.Dispatcher.Invoke(() =>
            {
                progressBar.IsIndeterminate = true;
                currentUpdateLblText.Text = label;
            }
            );
        }

        private void WinUpdateManager_ProgressChanged(int progress, string info)
        {
            this.Dispatcher.Invoke(() =>
            {
                progressBar.Value = progress;
                currentUpdateLblText.Text = info;
            }
           );


        }

        void ResetProgress()
        {
            this.Dispatcher.Invoke(() =>
                {
                    progressBar.Value = 0;
                    currentUpdateLblText.Text = "";
                    progressBar.IsIndeterminate = false;
                }
            );

        }

        private void WinUpdateManager_CheckCompleted(bool found)
        {
            ResetProgress();
            SetParagraphLook(checkParagrath, TextLook.Completed);
            if (found && !aborted)
            {


                SetParagraphLook(downloadParagrath, TextLook.Processing);


                //continue to Download
                winUpdateManager.DownloadCompleted += WinUpdateManager_DownloadCompleted;

                winUpdateManager.DownloadUpdates();

            }
            else
            {
                if (aborted)
                {
                    SetParagraphLook(checkParagrath, TextLook.Skipped);

                }
                //SetParagraphLook(checkParagrath, TextLook.Completed);
                SetParagraphLook(downloadParagrath, TextLook.Skipped);
                SetParagraphLook(installParagrath, TextLook.Skipped);

                //move on
                FinishAndProceed();
                //Proceed("check failed or nothing to install");

            }
        }

        private void WinUpdateManager_DownloadCompleted(bool success)
        {


            ResetProgress();
            if (success && !aborted)
            {
                SetParagraphLook(downloadParagrath, TextLook.Completed);
                SetParagraphLook(installParagrath, TextLook.Processing);

                //continue to Install
                winUpdateManager.InstallationCompleted += WinUpdateManager_InstallationCompleted;
                winUpdateManager.InstallUpdates();
            }
            else
            {
                SetParagraphLook(downloadParagrath, TextLook.Skipped);
                SetParagraphLook(installParagrath, TextLook.Skipped);

                FinishAndProceed();
                //move on
                //Proceed("download failed!");

            }
        }

        private void WinUpdateManager_InstallationCompleted(bool success, bool restartNeeded)
        {


            ResetProgress();


            if (success && !aborted)
            {
                if (restartNeeded && sessionParams.RestartBehavior != WindowsUpdateSessionState.RestartBehaviors.Skip)
                {
                    bool restartAccepted = false;
                    if (sessionParams.RestartBehavior == WindowsUpdateSessionState.RestartBehaviors.Ask)
                    {
                        if (MessageBox.Show("Restart the PC to finish Windows Updates?", "Restart Required", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            restartAccepted = true;
                        }
                        else
                        {
                            restartAccepted = false;
                        }
                    }
                    else if (sessionParams.RestartBehavior == WindowsUpdateSessionState.RestartBehaviors.Automatic)
                    {
                        restartAccepted = true;
                    }
                    if (restartAccepted)
                    {
                        StartInfiniteProgress("restarting the system...");
                        SessionManager.Instance.SetWinUpdateResults(winUpdateManager.GetResults(), false);
                        SessionManager.Instance.SaveSessionForResume();
                        SystemUtils.ScheduleAfterRestart();
                        SystemUtils.RestartSystem();
                        return;
                    }

                }



                SetParagraphLook(installParagrath, TextLook.Completed);





            }
            else
            {
                SetParagraphLook(installParagrath, TextLook.Skipped);
                //move on
                //Proceed("install failed!");
            }
            FinishAndProceed();
        }

        private void abortStepButton_Click(object sender, RoutedEventArgs e)
        {
            Abort();






        }
        void Abort()
        {
            aborted = true;
            Dispatcher.Invoke(() => abortButton.IsEnabled = false);

            if (winUpdateManager != null)
            {
                //as we are aborting we do not want to be getting any more progress events
                winUpdateManager.ProgressChanged -= WinUpdateManager_ProgressChanged;

                winUpdateManager.AbortAll();
            }
            StartInfiniteProgress("aborting...");
        }

        async Task<bool> Prepare()
        {
            bool enabled = WinServiceUtils.TrySetStartupType(WinUpdatesManager.WUA_SERVICE_NAME, true);
            if (!enabled)
            {
                return false;
            }
            return await WinServiceUtils.StartServiceAsync(WinUpdatesManager.WUA_SERVICE_NAME, 60000).ConfigureAwait(false);

        }
        async Task Cleanup()
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

                //in case there is still somehting going on at the moment
                //todo can this cause problems?
                //winUpdateManager.AbortAll();

                winUpdateManager.CleanUp();

                winUpdateManager = null;



            }
            await WinServiceUtils.StopServiceAsync(WinUpdatesManager.WUA_SERVICE_NAME, 60000).ConfigureAwait(false);
            WinServiceUtils.TrySetStartupType(WinUpdatesManager.WUA_SERVICE_NAME, false);
        }

        void SetParagraphLook(Paragraph paragraph, TextLook look)
        {
            Dispatcher.Invoke(() =>
            {
                FontWeight fontWeight = FontWeights.Normal;
                TextDecorationCollection textDecorations = null;
                var textColorBrush = SystemColors.ControlTextBrush;
                switch (look)
                {
                    case TextLook.Processing:
                        fontWeight = FontWeights.Bold;
                        textColorBrush = SystemColors.ControlTextBrush;
                        break;
                    case TextLook.Completed:
                        fontWeight = FontWeights.Normal;
                        textColorBrush = SystemColors.ControlTextBrush;

                        break;
                    case TextLook.Skipped:
                        fontWeight = FontWeights.Normal;
                        textColorBrush = SystemColors.ControlTextBrush;
                        textDecorations = TextDecorations.Strikethrough;
                        break;
                    case TextLook.ToProcess:
                        fontWeight = FontWeights.Normal;
                        textColorBrush = SystemColors.ControlTextBrush;

                        break;

                }
                paragraph.Foreground = textColorBrush;
                paragraph.FontWeight = fontWeight;
                paragraph.TextDecorations = textDecorations;
            }
            );

        }

        async void FinishAndProceed()
        {

            if (aborted)
            {
                SessionManager.Instance.SetWinUpdateAborted();
            }
            //when we arrive here after resume
            //in that case the session data is already restored from disk
            else if (!resumeInstall)
            {
                SessionManager.Instance.SetWinUpdateResults(winUpdateManager.GetResults());
            }
            else
            {
                SessionManager.Instance.SetWinUpdateCompleted();
            }
            StartInfiniteProgress("finishing...");
            await Task.Run(Cleanup).ConfigureAwait(false);


            //a delay for user to have the last look
            await Task.Delay(500).ConfigureAwait(false);

            //open the next Page 
            Dispatcher.Invoke(() =>
            {
                var page = SessionManager.Instance.GetNextSessionPage();
                NavigationService.Navigate(page);

            }
            );


        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {            
            SessionManager.Instance.SetAllAborted();
            Abort();
        }
    }
}
