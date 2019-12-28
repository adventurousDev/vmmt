using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services
{
    class WinOptimizationsManager
    {
        private static readonly object instancelock = new object();
        private static WinOptimizationsManager instance = null;
        public static WinOptimizationsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new WinOptimizationsManager();
                        }
                    }
                }
                return instance;
            }
        }

        Process sDeleteProc;

        public event Action<string> NewInfo;
        public event Action<int> SDeleteExited;
        public event Action<string> SDeleteError;
        public event Action<string, int> SDeleteProgressChanged;
        public bool SDeleteRunning { get { return sDeleteProc != null; } }

        public void RunSDelete()
        {
            if (sDeleteProc != null)
            {
                SDeleteError?.Invoke("SDelete is already running");
                return;
            }

            //todo deregister events before loosing reference
            sDeleteProc = new Process();
            var executable = Environment.Is64BitOperatingSystem ? "sdelete64.exe" : "sdelete.exe";
            var path = Path.Combine(@"C:\SDelete", executable);
            sDeleteProc.StartInfo.FileName = path;
            sDeleteProc.StartInfo.Arguments = "-nobanner -z c:";
            sDeleteProc.StartInfo.UseShellExecute = false;
            sDeleteProc.StartInfo.CreateNoWindow = true;
            sDeleteProc.StartInfo.RedirectStandardOutput = true;
            sDeleteProc.StartInfo.RedirectStandardError = true;

            sDeleteProc.EnableRaisingEvents = true;
            sDeleteProc.OutputDataReceived += SDeleteProc_OutputDataReceived;
            sDeleteProc.ErrorDataReceived += SDeleteProc_ErrorDataReceived;
            sDeleteProc.Exited += SDeleteProc_Exited;


            sDeleteProc.Start();
            sDeleteProc.BeginErrorReadLine();
            sDeleteProc.BeginOutputReadLine();

        }


        private void SDeleteProc_Exited(object sender, EventArgs e)
        {
            var exitCode = sDeleteProc.ExitCode;
            Info($"Finished execution. Exit code: {exitCode}");
            //removing events just in case
            sDeleteProc.OutputDataReceived -= SDeleteProc_OutputDataReceived;
            sDeleteProc.ErrorDataReceived -= SDeleteProc_ErrorDataReceived;
            sDeleteProc.Exited -= SDeleteProc_Exited;
            sDeleteProc.Close();
            sDeleteProc = null;

            SDeleteExited?.Invoke(exitCode);
        }

        private void SDeleteProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Info($"Sdelete process Error: {e.Data}");
            SDeleteError?.Invoke(e.Data);
        }

        private void SDeleteProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            Info($"Sdelete process Info: {e.Data}");
            string stage = e.Data;
            int progress = -1;//no progress
            var percentageIndex = e.Data.LastIndexOf('%');
            if (percentageIndex > 0)
            {
                var spaceBeforePercentageIndex = e.Data.LastIndexOf(' ', percentageIndex);
                var percentageString = e.Data.Substring(spaceBeforePercentageIndex + 1, percentageIndex - spaceBeforePercentageIndex - 1);
                stage = e.Data.Substring(0, spaceBeforePercentageIndex);
                progress = int.Parse(percentageString);
            }

            if (e.Data.StartsWith("Cleaning MFT"))
            {
                stage = "Cleaning MFT";
                progress = -8;//indefinite progress
            }
            
            //todo extract the stage and the progress

            SDeleteProgressChanged?.Invoke(stage, progress);
        }
        private void Info(string text)
        {
            NewInfo?.Invoke(text);
        }
    }
}