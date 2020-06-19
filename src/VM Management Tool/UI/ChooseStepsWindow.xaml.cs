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
using VMManagementTool.Configuration;
using VMManagementTool.Services;
using VMManagementTool.Services.Optimization;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for ChooseStepsWindow.xaml
    /// </summary>
    public partial class ChooseStepsWindow : Window
    {
      
        OSOTTemplateMeta template;
        public OptimizationTemplateManager OtManager { get; set; }
        public HashSet<string> Selection { get; private set; } = new HashSet<string>();
       
        public ChooseStepsWindow(OSOTTemplateMeta template)
        {
            InitializeComponent();
            this.template = template;
            //testdata = GenDummyData();
            //testdata.ForEach((g) => treeView.Items.Add(g));

            Loaded += ChooseStepsWindow_Loaded;



        }

        private async void ChooseStepsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Loaded -= ChooseStepsWindow_Loaded;
                if (OtManager == null)
                {
                    OtManager = new OptimizationTemplateManager();
                    Mouse.OverrideCursor = Cursors.Wait;

                    await OtManager.LoadAsync(template.FilePath);
                    //treeView.ItemsSource = testdata;
                    treeView.LayoutUpdated += TreeView_LayoutUpdated;
                    treeView.ItemsSource = OtManager.RootGroups;
                    
                }
                else
                {
                    treeView.ItemsSource = OtManager.RootGroups;
                }

            }
            catch (Exception ex)
            {
                Log.Error("ChooseStepsWindow.ChooseStepsWindow_Loaded", ex.ToString());
            }
        }

        private void TreeView_LayoutUpdated(object sender, EventArgs e)
        {
            Mouse.OverrideCursor = null;
            treeView.LayoutUpdated -= TreeView_LayoutUpdated;

        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Selection = new HashSet<string>(OtManager.GetSelectedSteps().Select((s) => s.ID));

        }
        List<Group> GenDummyData()
        {
            var dummyData = new List<Group>();

            //Group1
            // --step1
            // --step2
            // --step3
            // --Sgroup1
            //    --step1
            //Group2
            // --step1

            var group1 = new Group("group1", "the group 1 bla bla");
            group1.AddChild(new Step("g1step1", "g1step1 desc", Step.Categories.mandatory, true, false, null, null));
            group1.AddChild(new Step("g1step2", "g1step2 desc", Step.Categories.mandatory, false, false, null, null));
            group1.AddChild(new Step("g1step3", "g1step3 desc", Step.Categories.mandatory, true, false, null, null));
            var sgroup11 = new Group("sgroup11", "subgroup of group 1");
            sgroup11.AddChild(new Step("sg1step1", "sg1step1 desc", Step.Categories.mandatory, true, false, null, null));
            group1.AddChild(sgroup11);

            var group2 = new Group("group2", "the group 2 bla bla");
            group2.AddChild(new Step("g2step1", "g2step1 desc", Step.Categories.mandatory, true, false, null, null));

            dummyData.Add(group1);
            dummyData.Add(group2);

            return dummyData;
        }
    }
}
