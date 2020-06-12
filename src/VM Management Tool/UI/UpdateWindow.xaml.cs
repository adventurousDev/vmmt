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
using System.Windows.Shapes;
using VMManagementTool.Services;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        UpdateManager updateManager ;
        volatile bool updateStarted = false;
        public UpdateWindow()
        {
            InitializeComponent();

            updateManager = new UpdateManager(ConfigurationManager.Instance.GetStringConfig(ConfigurationManager.CONFIG_KEY_UPDATE_MANIFEST_URL));
            Loaded += UpdateWindow_Loaded;
        }


        public UpdateWindow(UpdateManager updateManager) : this()
        {
            this.updateManager = updateManager;
        }
        private void UpdateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UpdateWindow_Loaded;
            StartCheck();
        }

        private void closeButon_Click(object sender, RoutedEventArgs e)
        {
            AbortTasks();
            this.Close();
        }
        void AbortTasks()
        {
            if (updateStarted)
            {
                updateManager.Abort();
            }

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            AbortTasks();
            base.OnClosing(e);

        }
        async void StartCheck()
        {
            progressText.Text = "checking...";
            progressBar.IsIndeterminate = true;
            var available = await updateManager.CheckForUpdates().ConfigureAwait(true);
            progressText.Text = "";
            progressBar.IsIndeterminate = false;
            progressPanel.Visibility = Visibility.Hidden;

            if (available)
            {
                currVersionText.Text = "Current Version:".PadRight(18) + updateManager.CurrentVersion;
                newVersionText.Text = "New Version:".PadRight(18) + updateManager.NewVersion;
                foundPanel.Visibility = Visibility.Visible;
                updateButton.Visibility = Visibility.Visible;
            }
            else
            {
                notfoundPanel.Visibility = Visibility.Visible;
            }

        }

        private async void updateButton_Click(object sender, RoutedEventArgs e)
        {
            updateStarted = true;
            progressText.Text = "updating...";
            closeButon.Content = "Abort";
            updateButton.IsEnabled = false;
            progressPanel.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;
            try
            {
                await updateManager.UpdateAndRestartAsync();
            }
            catch (Exception ex)
            {
                Log.Error("UpdateWindow.updateButton_Click", ex.Message);
                progressPanel.Visibility = Visibility.Hidden;
                notfoundPanel.Visibility = Visibility.Visible;
                notFoundText.Text = "Update failed(see log for details)";
                notFoundText.Foreground = Brushes.Red;
                closeButon.Content = "Close";
                updateButton.Visibility = Visibility.Hidden;
            }
        }

    }
}
