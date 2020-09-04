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
using VMManagementTool.Session;
using VMManagementTool.Test;
using VMManagementTool.UIUtils;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for RunCleanupOptimizations.xaml
    /// </summary>
    public partial class RunCleanupOptimizationsPage : Page
    {
        const int INDEFINITE_PROGRESS = -1;

        //CleanupManager cleanupManager;
        DummyCleanupManager cleanupManager;
        CleanupSessionState sessionParams;
        bool aborted = false;

        Paragraph cleanmgrParagrath;
        Paragraph sdeleteParagrath;
        Paragraph defragParagrath;
        Paragraph dismParagrath;
        public RunCleanupOptimizationsPage()
        {
            InitializeComponent();
            sessionParams = SessionManager.Instance.GetCleanupParams();
            BuildUIList();


            Loaded += RunCleanupOptimizations_Loaded;
            var hostWin = Application.Current.MainWindow;
            hostWin.Closing += HostWin_Closing;
            Unloaded += (s, e) => { hostWin.Closing -= HostWin_Closing; };

        }

        private void BuildUIList()
        {
            FlowDocument flowDocument = new FlowDocument();
            List list = new List();

            list.MarkerStyle = TextMarkerStyle.Disc;
            list.Margin = new Thickness(0);

            if (sessionParams.RunDiskCleanmgr)
            {
                cleanmgrParagrath = new Paragraph(new Run(CleanupManager.TOOL_NAME_CLEANMGR));
                cleanmgrParagrath.Tag = CleanupManager.TOOL_NAME_CLEANMGR;
                list.ListItems.Add(new ListItem(cleanmgrParagrath));
            }
            if (sessionParams.RunSDelete)
            {
                sdeleteParagrath = new Paragraph(new Run(CleanupManager.TOOL_NAME_SDELETE));
                sdeleteParagrath.Tag = CleanupManager.TOOL_NAME_SDELETE;
                list.ListItems.Add(new ListItem(sdeleteParagrath));
            }
            if (sessionParams.RunDefrag)
            {
                defragParagrath = new Paragraph(new Run(CleanupManager.TOOL_NAME_DEFRAG));
                defragParagrath.Tag = CleanupManager.TOOL_NAME_DEFRAG;
                list.ListItems.Add(new ListItem(defragParagrath));
            }
            if (sessionParams.RunDism)
            {
                dismParagrath = new Paragraph(new Run(CleanupManager.TOOL_NAME_DISM));
                dismParagrath.Tag = CleanupManager.TOOL_NAME_DISM;
                list.ListItems.Add(new ListItem(dismParagrath));
            }

            flowDocument.Blocks.Add(list);

            textArea.Document = flowDocument;

        }
        Paragraph FindToolTextParagrath(string toolName)
        {
            return (textArea.Document.Blocks.FirstBlock as List).ListItems.FirstOrDefault((li) => li.Blocks.FirstBlock.Tag.ToString().Equals(toolName)).Blocks.FirstBlock as Paragraph;
        }

        private void HostWin_Closing(object sender, CancelEventArgs e)
        {

            Abort();
        }



        private async void RunCleanupOptimizations_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("RunCleanupOptimizations", "Cleanup Page Loaded");
            Loaded -= RunCleanupOptimizations_Loaded;
            //cleanupManager = new CleanupManager();
            cleanupManager = new DummyCleanupManager();


            cleanupManager.ToolCompleted += CleanupManager_ToolCompleted;

            //for smoother user experience
            await Task.Delay(500);


            //run next
            var next2Run = GetNextTool();
            if (next2Run == null)
            {
                FinishAndProceed();
                return;
            }

            SetParagraphLook(next2Run, TextLook.Processing);
            SetProgress(INDEFINITE_PROGRESS, "");
            cleanupManager.StartTool(next2Run);
        }
        string GetNextTool()
        {
            if (aborted)
            {
                return null;
            }
            if (sessionParams.RunDiskCleanmgr && !cleanupManager.HasCompleted(CleanupManager.TOOL_NAME_CLEANMGR))
            {
                return CleanupManager.TOOL_NAME_CLEANMGR;
            }
            if (sessionParams.RunDism && !cleanupManager.HasCompleted(CleanupManager.TOOL_NAME_DISM))
            {
                return CleanupManager.TOOL_NAME_DISM;
            }
            if (sessionParams.RunSDelete && !cleanupManager.HasCompleted(CleanupManager.TOOL_NAME_SDELETE))
            {
                return CleanupManager.TOOL_NAME_SDELETE;
            }
            if (sessionParams.RunDefrag && !cleanupManager.HasCompleted(CleanupManager.TOOL_NAME_DEFRAG))
            {
                return CleanupManager.TOOL_NAME_DEFRAG;
            }
           
            return null;

        }
        private async void CleanupManager_ToolCompleted(string toolName, bool success)
        {
            ResetProgress();
            if (success)
            {
                SetParagraphLook(toolName, TextLook.Completed);

            }
            else
            {
                SetParagraphLook(toolName, TextLook.Skipped);

            }
            string nextTool = GetNextTool();
            if (nextTool != null)
            {
                SetParagraphLook(nextTool, TextLook.Processing);
                await Task.Delay(500);
                if (aborted)
                {
                    SetParagraphLook(nextTool, TextLook.Skipped);
                    FinishAndProceed();
                    return;
                }

                SetProgress(INDEFINITE_PROGRESS, "");
                cleanupManager.StartTool(nextTool);
            }
            else
            {
                FinishAndProceed();
            }
        }
        /* OLD compleation handling
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
        */


        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            Abort();
        }
        private void Abort()
        {
            aborted = true;
            cleanupManager?.Abort();
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

        void SetParagraphLook(string toolName, TextLook look)
        {
           
            Dispatcher.Invoke(() =>
            {
                Paragraph paragraph = FindToolTextParagrath(toolName);
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
                SessionManager.Instance.SetCleanupAborted();
            }
            else
            {
                SessionManager.Instance.SetCleanupResults(cleanupManager?.GetResults());
            }

            //deregister events(jsut in case)
            if (cleanupManager != null)
            {
                cleanupManager.ToolCompleted -= CleanupManager_ToolCompleted;

            }

            SetProgress(INDEFINITE_PROGRESS, "finishing...");



            //a delay for user to have the last look
            await Task.Delay(500);
           
            //open the next Page
            Dispatcher.Invoke(() =>
                {
                    var nextPage = SessionManager.Instance.GetNextSessionPage();
                    NavigationService.Navigate(nextPage);

                }
            );


        }


    }
}
