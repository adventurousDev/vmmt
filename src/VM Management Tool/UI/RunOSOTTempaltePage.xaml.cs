using System;
using System.Collections.Generic;
using System.IO;
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
using VMManagementTool.Test;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for RunOSOTTempaltePage.xaml
    /// </summary>
    public partial class RunOSOTTempaltePage : Page
    {
        OptimizationTemplateManager optimizationTemplateManager;
        //DummyOptimizationTemplateManager optimizationTemplateManager;
        const int INDEFINITE_PROGRESS = -1;
        public RunOSOTTempaltePage()
        {
            InitializeComponent();
            Loaded += RunOSOTTempaltePage_Loaded;

        }

        private async void RunOSOTTempaltePage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("RunOSOTTempaltePage", "OSOT Page Loaded");
            Loaded -= RunOSOTTempaltePage_Loaded;
            //prepare: load the template with infinite progress asyncrounously
            SetProgress(INDEFINITE_PROGRESS, "loading the template...");


            optimizationTemplateManager = new OptimizationTemplateManager();
            //optimizationTemplateManager = new DummyOptimizationTemplateManager();

            //for debuging
            //SetProgress(INDEFINITE_PROGRESS, Directory.GetCurrentDirectory());
            //await Task.Delay(15000);

            //for smoother user experience
            await Task.Delay(250);

            optimizationTemplateManager.RunProgressChanged += OptimizationTemplateManager_RunProgressChanged;
            optimizationTemplateManager.RunCompleted += OptimizationTemplateManager_RunCompleted;
            //todo get the path from approapriate source
            string templatePath = Settings.OPTIMIZATION_TEMPLATE_PATH;
            try
            {
                //the loading can fail; then we just need to finsihproceed to the next page
                await optimizationTemplateManager.LoadAsync(templatePath);

                optimizationTemplateManager.RunDefaultStepsAsync();
            }
            catch (Exception ex)
            {
                OptimizationTemplateManager_RunCompleted(false);
            }

            

            
        }
        //the parameter signifies sucess (not aborted or failed): false mean aborted or failed 
        private void OptimizationTemplateManager_RunCompleted(bool successful)
        {
            //cleanup and proceed to the next page 
            //todo also fetch and save the status for report
            FinishAndProceed();

        }

        private void OptimizationTemplateManager_RunProgressChanged(int progress, string stepName)
        {
            SetProgress(progress, $"executing: {stepName}");
        }

        void SetProgress(int value, string label)
        {
            Dispatcher.Invoke(() =>
                {
                    if (value < 0)
                    {
                        progressBar.IsIndeterminate = true;
                    }
                    else
                    {
                        progressBar.IsIndeterminate = false;
                        progressBar.Value = value;
                    }
                    currentUpdateLblText.Text = label;
                }
            );
        }
        
        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            SetProgress(INDEFINITE_PROGRESS, "aborting...");
            optimizationTemplateManager.Abort();
        }
        async void FinishAndProceed()
        {
            VMMTSessionManager.Instance.SetOSOTResults(optimizationTemplateManager.GetResults());
            if (optimizationTemplateManager != null)
            {
                optimizationTemplateManager.RunProgressChanged -= OptimizationTemplateManager_RunProgressChanged;
                optimizationTemplateManager.RunCompleted -= OptimizationTemplateManager_RunCompleted;
            }

            SetProgress(INDEFINITE_PROGRESS, "finishing...");


            await optimizationTemplateManager.CleanupAsync();

            //a delay for user to have the last look
            await Task.Delay(500);
            //todo save the state if not yet done by now
            //open the next Page
            Dispatcher.Invoke(() =>
            {
                var page = new RunCleanupOptimizations();
                NavigationService.Navigate(page);

            }
            );


        }
    }
}
