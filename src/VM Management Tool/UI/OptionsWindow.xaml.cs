using Microsoft.Win32;
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
using System.Windows.Shapes;
using VMManagementTool.Configuration;
using Path = System.IO.Path;

namespace VMManagementTool.UI
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {

        public OptionsWindow()
        {
            InitializeComponent();
            LoadValues();
            Loaded += OptionsWindow_Loaded;
            templatesListBox.ItemsSource = ConfigurationManager.Instance.OSOTTemplatesData;
            templatesListBox.DisplayMemberPath = "ID";
            templatesListBox.SelectionChanged += TemplatesListBox_SelectionChanged;
        }

        private void TemplatesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (templatesListBox.SelectedItems.Count > 0 && !templatesListBox.SelectedItems.Cast<OSOTTemplateMeta>().Any((otm) => otm.Type == OSOTTemplateType.System))
            {
                deleteButton.IsEnabled = true;
            }
            else
            {
                deleteButton.IsEnabled = false;
            }
        }

        private void OptionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OptionsWindow_Loaded;

        }

        void LoadValues()
        {
            var val = ConfigurationManager.Instance.GetUserSetting<long>("log", "level", 0);
            logDetailLevelDropdown.SelectedValue = val;
        }
        void SaveValues()
        {
            ConfigurationManager.Instance.SaveUserSetting("log", "level", Convert.ToInt32(logDetailLevelDropdown.SelectedValue), false);
            ConfigurationManager.Instance.SaveUserSettingsToDisk();


        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            SaveValues();
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.Copy(Log.LOG_FILE, saveFileDialog.FileName);
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in templatesListBox.SelectedItems)
            {
                if (item is OSOTTemplateMeta template && template.Type != OSOTTemplateType.System)
                {

                    ConfigurationManager.Instance.DeleteOSOTTemplate(template);
                }
            }
            templatesListBox.Items.Refresh();
        }

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML file (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                
                var importSuccess = ConfigurationManager.Instance.ImportOSOTTEmplate(openFileDialog.FileName, out string err);
                var msg = "";
                if (importSuccess)
                {
                    templatesListBox.Items.Refresh();
                    msg = "Successfully imported.";
                }
                else
                {
                    msg = $"Import failed: {err}; See log for details.";
                }
                MessageBox.Show(msg);
                
                
            }
           
        }
    }
}
