using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services
{
    [Obsolete]
    class SDeleteTask
    {
        private Process sDeleteProc;

        public event Action<string> NewInfo;

   
        public event Action<string> OnNewOutput;
        public event Action<object , EventArgs > Exited;

        volatile bool aborted = false;
        byte[] outputBuffer;
        public void Run()
        {
            sDeleteProc = new Process();
            var executable = Environment.Is64BitOperatingSystem ? "sdelete64.exe" : "sdelete.exe";
            var path = Path.Combine(Settings.SDELETE_FOLDER, executable);
            if (executable == "sdelete64.exe" && !File.Exists(path))
            {
                //try using 32 bit because it is more probable to be there 
                path = Path.Combine(@"C:\bwLehrpool\SDelete", "sdelete.exe");
            }
            sDeleteProc.StartInfo.FileName = path;
            sDeleteProc.StartInfo.Arguments = "-nobanner -z c:";
            sDeleteProc.StartInfo.UseShellExecute = true;
            sDeleteProc.StartInfo.CreateNoWindow = false;
            //sDeleteProc.StartInfo.RedirectStandardOutput = true;
            //sDeleteProc.StartInfo.RedirectStandardError = true;

            sDeleteProc.EnableRaisingEvents = true;

            sDeleteProc.Exited += SDeleteProc_Exited;

            sDeleteProc.Start();
            //ReadLinesAsync();


        }

        public void RunViaPS()
        {
            try
            {
                PowerShell shell = PowerShell.Create();

                shell.AddScript(@"C:\bwLehrpool\SDelete\sdelete.exe -nobanner -z c:");
                shell.Streams.Verbose.DataAdded += Verbose_DataAdded;
                shell.Streams.Error.DataAdded += Verbose_DataAdded;
                shell.Streams.Debug.DataAdded += Verbose_DataAdded;
                shell.Streams.Information.DataAdded += Verbose_DataAdded;
                shell.Streams.Warning.DataAdded += Verbose_DataAdded;
                //shell.Streams.Verbose.DataAdded += Verbose_DataAdded;
                shell.BeginInvoke();

            }
            catch (Exception e)
            {
                throw new Exception("Could not change serice startup type" + e.Message);
            }
        }

        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<PSObject> myp = (PSDataCollection<PSObject>)sender;

            Collection<PSObject> results = myp.ReadAll();
            Info("Data added; count: " + results.Count);
            foreach (PSObject result in results)
            {
                Info(result.ToString());
            }
        }

        private void SDeleteProc_Exited(object sender, EventArgs e)
        {
            //sDeleteProc.Close();
            //Exited?.Invoke(sender, e);
            Info("Exit");
        }

        async void ReadLinesAsync()
        {
            await Task.Run(ReadLines);
        }
        void ReadLines()
        {
            while (!aborted && !sDeleteProc.StandardOutput.EndOfStream)
            {
                int character = sDeleteProc.StandardOutput.Read();
                var line = (char)character;
                //var line = sDeleteProc.StandardOutput.ReadLine();
                OnNewOutput?.Invoke(line.ToString());
                
                Info(line.ToString()+$" [{character}]");
            }
            Info("Stream Exit");
        }

        private void Info(string text)
        {
            NewInfo?.Invoke(text);
        }

    }
}
