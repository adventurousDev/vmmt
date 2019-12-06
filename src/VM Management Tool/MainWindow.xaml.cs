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
using VM_Management_Tool.Services;

namespace VM_Management_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WinUpdatesManager.Instance.NewInfo += LogUpdateInto;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //
            //WinUpdatesManager.Instance.LoadHsitory();
            WinUpdatesManager.Instance.CheckForUpdates();
        }

        private void LogUpdateInto(string msg)
        {
            var date = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            this.Dispatcher.Invoke(() =>
                 theConsole.AppendText(date + ": " + msg + Environment.NewLine)
            ) ;
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            WinUpdatesManager.Instance.AbortChecking();
        }
    }
}
