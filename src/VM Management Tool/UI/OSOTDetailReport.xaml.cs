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
using System.Windows.Shapes;
using VMManagementTool.Session;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for OSOTDetailReport.xaml
    /// </summary>
    public partial class OSOTDetailReport : Window
    {
        OptimizationSession session;
        public OSOTDetailReport(OptimizationSession session)
        {
            InitializeComponent();
            this.session = session;
            Loaded += OSOTDetailReport_Loaded;
        }

        private void OSOTDetailReport_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OSOTDetailReport_Loaded;
            PrintResults();

        }

        void PrintResults()
        {
            theConsole.Document.Blocks.Clear();
            foreach (var stepResult in session.OSOTSessionState.Results ?? new List<(string, bool)>())
            {
                if((successRadioBtn.IsChecked ?? false) && !stepResult.Item2)
                {
                    continue;
                }
                if ((failRadioBtn.IsChecked ?? false) && stepResult.Item2)
                {
                    continue;
                }

                theConsole.AppendText($"{stepResult.Item1} - {(stepResult.Item2 ? "success" : "fail")}{Environment.NewLine}");
            }
        }

        private void RadioBtn_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            PrintResults();
        }
    }
}
