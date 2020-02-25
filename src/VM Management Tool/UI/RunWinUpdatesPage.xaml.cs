﻿using System;
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
using VM_Management_Tool.Test;
using VMManagementTool.Services;

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
        public RunWinUpdatesPage()
        {
            InitializeComponent();
            Loaded += RunOptimizationsPage_Loaded;
            Unloaded += RunWinUpdatesPage_Unloaded;
        }

        private void RunWinUpdatesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            //Cleanup();
        }

        private async void RunOptimizationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            ResetProgress();
            StartInfiniteProgress("preparing...");
            await Prepare().ConfigureAwait(false);
            if (aborted)
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

        private void WinUpdateManager_InstallationCompleted(bool success)
        {


            ResetProgress();
            if (success && !aborted)
            {
                SetParagraphLook(installParagrath, TextLook.Completed);
                //todo check if reboot needed and move on to reboot
                //otherwise just move on


            }
            else
            {
                SetParagraphLook(installParagrath, TextLook.Skipped);
                //move on
                //Proceed("install failed!");
            }
            FinishAndProceed();
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
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




            //Proceed("aborted!");
        }
        void Proceed(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(msg);
            }
           );

        }
        async Task<bool> Prepare()
        {
            bool enabled = await WinServiceUtils.SetStartupTypeAsync(WinUpdatesManager.WUA_SERVICE_NAME, true, 5000).ConfigureAwait(false);
            if (!enabled)
            {
                return false;
            }
            return await WinServiceUtils.StartServiceAsync(WinUpdatesManager.WUA_SERVICE_NAME, 5000).ConfigureAwait(false);

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

                winUpdateManager.CleanUp();

                winUpdateManager = null;

                bool enabled = await WinServiceUtils.SetStartupTypeAsync(WinUpdatesManager.WUA_SERVICE_NAME, false, 5000).ConfigureAwait(true);

                await WinServiceUtils.StartServiceAsync(WinUpdatesManager.WUA_SERVICE_NAME, 5000).ConfigureAwait(true);

            }
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
        private enum TextLook
        {
            Processing,
            Completed,
            Skipped,
            ToProcess,

        }
        async void FinishAndProceed()
        {
            StartInfiniteProgress("finishing...");
            await Cleanup();

            //a delay for user to have the last look
            await Task.Delay(500);
            //todo save the state if not yet done by now
            //open the next Page
            Dispatcher.Invoke(() =>
            {
                var page = new RunOSOTTempaltePage();
                NavigationService.Navigate(page);

            }
            );


        }
    }
}