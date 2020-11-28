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
using VMManagementTool.Configuration;
using VMManagementTool.Services;
using VMManagementTool.Session;
using VMManagementTool.UI;
using static VMManagementTool.Session.OSOTSessionState;
using static VMManagementTool.Session.WindowsUpdateSessionState;

namespace VMManagementTool
{
    /// <summary>
    /// Interaction logic for AdvanceConfigPage.xaml
    /// </summary>
    public partial class AdvanceConfigPage : Page
    {
        public AdvanceConfigPage()
        {
            InitializeComponent();
            //Dism is only spupported starting in win 8.1 so hide its options
            if (Environment.OSVersion.Version < new Version("6.3"))
            {
                chkbxDism.IsChecked = false;
                chkbxDism.IsEnabled = false;
                chkbxDism.Visibility = Visibility.Collapsed;
            }

            osotTemplateDropDown.ItemsSource = ConfigurationManager.Instance.OSOTTemplatesData;
            osotTemplateDropDown.DisplayMemberPath = "ID";
            osotTemplateDropDown.SelectedItem = ConfigurationManager.Instance.OSOTTemplatesData.Where((otmd) => otmd.Type == Configuration.OSOTTemplateType.System).FirstOrDefault();
            osotTemplateDropDown.SelectionChanged += OsotTemplateDropDown_SelectionChanged;

        }
        OptimizationTemplateManager templateManagerCache;
        private void OsotTemplateDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            templateManagerCache = null;
        }

        private HashSet<string> stepsSelection;

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            var session = new OptimizationSession();

            if (wuCheckBox.IsChecked ?? false)
            {
                session.WindowsUpdateSessionState = new WindowsUpdateSessionState();
                session.WindowsUpdateSessionState.RestartBehavior = (RestartBehaviors)wuRestartDropdown.SelectedValue;
            }

            if (osotCheckBox.IsChecked ?? false)
            {
                session.OSOTSessionState = new OSOTSessionState();
                session.OSOTSessionState.StepsChoiceOption = (StepsChoice)stepsChoiceGroup.Children.OfType<RadioButton>().FirstOrDefault((r) => r.IsChecked ?? false).Tag;
                session.OSOTSessionState.OSOTTemplateMetadata = (OSOTTemplateMeta)osotTemplateDropDown.SelectedItem;
                if (session.OSOTSessionState.StepsChoiceOption == StepsChoice.Custom)
                {
                    session.OSOTSessionState.CustomStepsChoice = stepsSelection ?? new HashSet<string>();
                }
            }

            if (cleanupCheckBox.IsChecked ?? false)
            {
                session.CleanupSessionState = new CleanupSessionState();
                session.CleanupSessionState.RunDiskCleanmgr = (bool)chkbxDiskCleanup.IsChecked;
                session.CleanupSessionState.RunSDelete = (bool)chkbxSdelete.IsChecked;
                session.CleanupSessionState.RunDefrag = (bool)chkbxDefrag.IsChecked;
                session.CleanupSessionState.RunDism = (bool)chkbxDism.IsChecked;
            }

            SessionManager.Instance.StartOptimizationSession(session);


            Page nextPage = SessionManager.Instance.GetNextSessionPage();
            /*
            if (wuCheckBox.IsChecked ?? false)
            {
                nextPage = new RunWinUpdatesPage();
            }
            else if (osotCheckBox.IsChecked ?? false)
            {
                nextPage = new RunOSOTTempaltePage();
            }
            else if (cleanupCheckBox.IsChecked ?? false)
            {
                nextPage = new RunCleanupOptimizationsPage();
            }
            */


            NavigationService.Navigate(nextPage);


        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                if ("top".Equals(((Control)sender).Tag))
                {


                    var isChecked = ((CheckBox)sender).IsChecked;
                    ((Panel)((GroupBox)((CheckBox)sender).Parent).Content).IsEnabled = (bool)isChecked;
                }
                nextRunButton.IsEnabled = (
                    (bool)wuCheckBox.IsChecked || (bool)osotCheckBox.IsChecked
                    ||
                        ((bool)cleanupCheckBox.IsChecked && (
                                    (bool)chkbxDefrag.IsChecked ||
                                    (bool)chkbxDiskCleanup.IsChecked ||
                                    (bool)chkbxSdelete.IsChecked ||
                                    (bool)chkbxDism.IsChecked
                                )
                        )
                    );
            }
        }

        private void ChooseStepsButtonClick(object sender, RoutedEventArgs e)
        {

            var selectDialog = new ChooseStepsWindow((OSOTTemplateMeta)osotTemplateDropDown.SelectedItem);
            if (templateManagerCache != null)
            {
                selectDialog.OtManager = templateManagerCache;
            }

            if (selectDialog.ShowDialog() ?? false && selectDialog.Selection.Count > 0)
            {
                customStepsRadio.IsChecked = true;
                stepsSelection = selectDialog.Selection;
                templateManagerCache = selectDialog.OtManager;
            }

        }
    }
}
