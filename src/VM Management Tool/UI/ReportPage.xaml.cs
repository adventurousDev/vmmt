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
            PrintResults();
        }

        void PrintResults()
        {
            try
            {
                //debug test session serialization
                //VMMTSessionManager.Instance.SaveSessionForResume();

                //VMMTSessionManager.Instance.LoadPausedSession();

                var session = VMMTSessionManager.Instance.FinishCurrentSession();

                var winUpdateResults = session.WinUpdateResults;
                var osotResults = session.OSOTResults;
                var cleanupResults = session.CleanupResults;
                if (winUpdateResults != null)
                {
                    theConsole.AppendText($"Windwos Updates: {(winUpdateResults.Count == 0 ? "no updates found" : "")}" + Environment.NewLine);

                    foreach (var updateTitle in winUpdateResults.Keys)
                    {
                        var updateStatus = winUpdateResults[updateTitle];
                        var outTitle = updateTitle + "(" + string.Join(",", updateStatus.KBIds) + ")";
                        theConsole.AppendText($"        {outTitle} - {(updateStatus.IsInstalled ? "successfully installed" : "failed to install: " + updateStatus.Error)}{Environment.NewLine}");
                    }


                    theConsole.AppendText(Environment.NewLine);
                }

                if (osotResults != null)
                {


                    var successfulCount = osotResults.Count((st) => st.Item2);
                    var fails = osotResults.Count - successfulCount;
                    theConsole.AppendText($"OSOT Template: ({successfulCount} - succeeded; {fails} - failed)" + Environment.NewLine);
                    theConsole.AppendText(Environment.NewLine);
                }

                if (cleanupResults != null)
                {
                    theConsole.AppendText($"Cleanup:" + Environment.NewLine);

                    foreach (var cleanupResult in cleanupResults)
                    {
                        theConsole.AppendText($"        {cleanupResult.Item1} - {(cleanupResult.Item2 ? "success" : $"fail:(code {cleanupResult.Item3})")}{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {

                Log.Error("ReportPage.PrintResults", ex.Message);
            }

        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
