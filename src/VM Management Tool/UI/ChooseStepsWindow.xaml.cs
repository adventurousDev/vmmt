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
using VMManagementTool.Services.Optimization;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for ChooseStepsWindow.xaml
    /// </summary>
    public partial class ChooseStepsWindow : Window
    {
        public ChooseStepsWindow()
        {
            InitializeComponent();
            
            
           

           
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {

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
            group1.AddChild(new Step("g1step2", "g1step2 desc", Step.Categories.mandatory, true, false, null, null));
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
