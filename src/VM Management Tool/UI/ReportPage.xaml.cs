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
            PrintResults();
        }

        void PrintResults()
        {
            var session = VMMTSessionManager.Instance.FinishCurrentSession();
            var sessionResults = session.GetResults();
            var winUpdateResults = (Dictionary<string, WinUpdateStatus>)sessionResults[VMMTSessionManager.WIN_UPDATE_RESULTS_KEY];
            var osotResults = (List<(string, bool)>)sessionResults[VMMTSessionManager.OSOT_RESULTS_KEY];
            var cleanupResults = (List<(string, bool, int)>)sessionResults[VMMTSessionManager.CLEANUP_RESULTS_KEY];

            theConsole.AppendText($"Windwos Updates: {(winUpdateResults.Count == 0 ? "no updates found" : "")}" + Environment.NewLine);

            foreach (var updateTitle in winUpdateResults.Keys)
            {
                var updateStatus = winUpdateResults[updateTitle];
                var outTitle = updateTitle + "(" + string.Join(",", updateStatus.KBIds) + ")";
                theConsole.AppendText($"        {outTitle} - {(updateStatus.IsInstalled ? "successfully installed" : "failed to install: " + updateStatus.Error)}{Environment.NewLine}");
            }

            theConsole.AppendText(Environment.NewLine);
            var successfulCount = osotResults.Count((st) => st.Item2);
            var fails = osotResults.Count - successfulCount;
            theConsole.AppendText($"OSOT Template: ({successfulCount} - succeeded; {fails} - failed)" + Environment.NewLine);
            theConsole.AppendText(Environment.NewLine);

            theConsole.AppendText($"Cleanup:" + Environment.NewLine);

            foreach (var cleanupResult in cleanupResults)
            {
                theConsole.AppendText($"        {cleanupResult.Item1} - {(cleanupResult.Item2 ? "success" : $"fail:(code {cleanupResult.Item3})")}{Environment.NewLine}");
            }

        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
