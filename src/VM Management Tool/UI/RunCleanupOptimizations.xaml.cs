using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using VMManagementTool.Test;
using VMManagementTool.UIUtils;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for RunCleanupOptimizations.xaml
    /// </summary>
    public partial class RunCleanupOptimizations : Page, IDisposable
    {
        const int INDEFINITE_PROGRESS = -1;

        CleanupManager cleanupManager;
        //DummyCleanupManager cleanupManager;

        bool aborted = false;
        public RunCleanupOptimizations()
        {
            InitializeComponent();
            Loaded += RunCleanupOptimizations_Loaded;
            var hostWin = Application.Current.MainWindow;
            hostWin.Closing += HostWin_Closing;
            Unloaded += (s, e) => { hostWin.Closing -= HostWin_Closing; };
        }

        private void HostWin_Closing(object sender, CancelEventArgs e)
        {
            
            Abort();
        }

        

        private async void RunCleanupOptimizations_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("RunCleanupOptimizations", "Cleanup Page Loaded");
            Loaded -= RunCleanupOptimizations_Loaded;
            cleanupManager = new CleanupManager();
            //cleanupManager = new DummyCleanupManager();

            //for smoother user experience
            await Task.Delay(500);

            //run cleanmgr
            SetParagraphLook(cleanmgrParagrath, TextLook.Processing);
            SetProgress(INDEFINITE_PROGRESS, "");

            cleanupManager.CleanmgrCompleted += WinOptimizationsManager_CleanmgrCompleted;
            cleanupManager.StartCleanmgr();
        }

        private async void WinOptimizationsManager_CleanmgrCompleted(bool success)
        {
            ResetProgress();

            if (success)
            {
                SetParagraphLook(cleanmgrParagrath, TextLook.Completed);

            }
            else
            {
                SetParagraphLook(cleanmgrParagrath, TextLook.Skipped);

            }

            SetParagraphLook(sdeleteParagrath, TextLook.Processing);

            //for smoother user experience
            await Task.Delay(500);

            if (aborted)
            {
                SetParagraphLook(sdeleteParagrath, TextLook.Skipped);
                FinishAndProceed();
                return;
            }
            SetProgress(INDEFINITE_PROGRESS, "");
            //proceed to sdelete
            //winOptimizationsManager.ProgressChanged += WinOptimizationsManager_ProgressChanged;
            
            cleanupManager.SdeleteCompleted += WinOptimizationsManager_SdeleteCompleted;           
            cleanupManager.StartSdelete();
        }

        private async void WinOptimizationsManager_SdeleteCompleted(bool success)
        {
            ResetProgress();

            if (success)
            {
                SetParagraphLook(sdeleteParagrath, TextLook.Completed);

            }
            else
            {
                SetParagraphLook(sdeleteParagrath, TextLook.Skipped);

            }

            SetParagraphLook(defragParagrath, TextLook.Processing);

            //for smoother user experience
            await Task.Delay(500);
            if (aborted)
            {
                SetParagraphLook(defragParagrath, TextLook.Skipped);
                FinishAndProceed();
                return;
            }
            //proceed to defrag
            SetProgress(INDEFINITE_PROGRESS, "");
            cleanupManager.DefragCompleted += WinOptimizationsManager_DefragCompleted;
            cleanupManager.StartDefrag();
        }

        private void WinOptimizationsManager_DefragCompleted(bool success)
        {
            ResetProgress();
            if (success)
            {
                SetParagraphLook(defragParagrath, TextLook.Completed);

            }
            else
            {
                SetParagraphLook(defragParagrath, TextLook.Skipped);

            }

            FinishAndProceed();
        }



        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            Abort();
        }
        private void Abort()
        {
            aborted = true;
            cleanupManager.Abort();
        }
        void SetProgress(int value, string label)
        {
            Dispatcher.Invoke(() =>
            {
                if (value < 0)
                {
                    progressBar.IsIndeterminate = true;
                }
                else
                {
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = value;
                }
                currentUpdateLblText.Text = label;
            }
            );
        }
        void ResetProgress()
        {
            SetProgress(0, "");
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
                VMMTSessionManager.Instance.SetCleanupResults(null);
            }
            else
            {
                VMMTSessionManager.Instance.SetCleanupResults(cleanupManager?.GetResults());
            }
            
            //deregister events(jsut in case)
            if (cleanupManager != null)
            {
                cleanupManager.CleanmgrCompleted -= WinOptimizationsManager_CleanmgrCompleted;
                cleanupManager.SdeleteCompleted -= WinOptimizationsManager_SdeleteCompleted;
                cleanupManager.DefragCompleted -= WinOptimizationsManager_DefragCompleted;
            }

            SetProgress(INDEFINITE_PROGRESS, "finishing...");



            //a delay for user to have the last look
            await Task.Delay(500);
            //todo save the state if not yet done by now
            //open the next Page
            Dispatcher.Invoke(() =>
                {
                    var page = new ReportPage();
                    NavigationService.Navigate(page);

                }
            );


        }

        public void Dispose()
        {
            cleanupManager?.Dispose();
        }
    }
}
