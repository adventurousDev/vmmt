using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using VMManagementTool.Services;
using VMManagementTool.UI;

namespace VMManagementTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var args = Environment.GetCommandLineArgs();
            var dir = Directory.GetCurrentDirectory();
            
            if (args.Length > 1 && args[1].Equals("/resume"))
            {
                
                SystemUtils.DeleteResumeTask();
                VMMTSessionManager.Instance.LoadPausedSession();
                frame.Navigate(new RunWinUpdatesPage(true));
            }
            else
            {
                
                frame.Navigate(new HomePage());
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            TestWindow.Instance.Show();
            //TestWindow.Instance.Activate();
            TestWindow.Instance.WindowState = WindowState.Normal;

        }
    }
}
