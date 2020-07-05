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
using VMManagementTool.Session;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for ReportPage.xaml
    /// </summary>
    public partial class ReportPage : Page
    {
        public ReportPage()
        {
            InitializeComponent();
            this.Loaded += ReportPage_Loaded;

        }

        private void ReportPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("ReportPage", "Report Page Loaded");
            this.Loaded -= ReportPage_Loaded;
            PrintResults();
        }

        void PrintResults()
        {
            try
            {
                //debug test session serialization
                //VMMTSessionManager.Instance.SaveSessionForResume();

                //VMMTSessionManager.Instance.LoadPausedSession();

                var session = SessionManager.Instance.FinishCurrentSession();

                var winUpdateResults = session.WindowsUpdateSessionState?.Results;
                var osotResults = session.OSOTSessionState?.Results;
                var cleanupResults = session.CleanupSessionState?.Results;
                if (winUpdateResults != null)
                {
                    if (!session.WindowsUpdateSessionState.IsAborted)
                    {
                        theConsole.AppendText($"Windows Updates: {(winUpdateResults.Count == 0 ? "no updates found" : "")}" + Environment.NewLine);

                        foreach (var updateTitle in winUpdateResults.Keys)
                        {
                            var updateStatus = winUpdateResults[updateTitle];
                            var outTitle = updateTitle + "(" + string.Join(",", updateStatus.KBIds) + ")";
                            theConsole.AppendText($"        {outTitle} - {(updateStatus.IsInstalled ? "successfully installed" : "failed to install: " + updateStatus.Error)}{Environment.NewLine}");
                        }


                        //theConsole.AppendText(Environment.NewLine);
                    }
                    else
                    {
                        theConsole.AppendText($"Windows Updates: aborted" + Environment.NewLine);
                    }
                    
                }
                

                if (osotResults != null)
                {

                    if (!session.OSOTSessionState.IsAborted)
                    {
                        var successfulCount = osotResults.Count((st) => st.Item2);
                        var fails = osotResults.Count - successfulCount;
                        //theConsole.AppendText($"OSOT Template: {successfulCount} - succeeded; {fails} - failed" + Environment.NewLine);
                        Paragraph para = new Paragraph();
                        para.Margin = new Thickness(0); // remove indent between paragraphs

                        Hyperlink link = new Hyperlink();
                        link.IsEnabled = true;
                        link.Inlines.Add("view details");
                        link.Click += (x, y) => ShowOSOTDetailsDialog(session);


                        para.Inlines.Add(new Run($"OSOT Template Steps: {successfulCount} - succeeded; {fails} - failed ("));
                        para.Inlines.Add(link);
                        para.Inlines.Add(new Run(")"));

                        theConsole.Document.Blocks.Add(para);
                        theConsole.AppendText(Environment.NewLine);
                        theConsole.AppendText(Environment.NewLine);
                    }
                    else
                    {
                        theConsole.AppendText($"OSOT Template Steps: aborted" + Environment.NewLine);
                    }

                }
                

                if (cleanupResults != null)
                {
                    if (!session.CleanupSessionState.IsAborted)
                    {
                        theConsole.AppendText($"Cleanup:" + Environment.NewLine);

                        foreach (var cleanupResult in cleanupResults)
                        {
                            theConsole.AppendText($"        {cleanupResult.Item1} - {(cleanupResult.Item2 ? "success" : $"fail:(code {cleanupResult.Item3})")}{Environment.NewLine}");
                        }
                    }
                    else
                    {
                        theConsole.AppendText($"Cleanup: aborted" + Environment.NewLine);
                    }
                }
               
            }
            catch (Exception ex)
            {

                Log.Error("ReportPage.PrintResults", ex.ToString());
            }

        }

        void ShowOSOTDetailsDialog(OptimizationSession session)
        {
            var dialog = new OSOTDetailReport(session);
            dialog.ShowDialog();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
