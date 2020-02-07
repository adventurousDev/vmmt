using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Management.Automation;
using System.Collections.ObjectModel;

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
        Process cleanmgrProc;
        Process defragProc;

        public event Action<string> NewInfo;
        public event Action<int> SDeleteExited;
        public event Action<string> SDeleteError;
        public event Action<string, int> SDeleteProgressChanged;
        public bool SDeleteRunning { get { return sDeleteProc != null; } }
        private const int SW_HIDE = 0;
        private const int CLEANMGR_STATEFLAGS_ID = 9999;

        volatile bool defragProcExited = false;

        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        public void HideCleanMgrWndow()
        {
            var hWnd = cleanmgrProc.MainWindowHandle.ToInt32();
            ShowWindow(hWnd, SW_HIDE);
        }

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
        private void PrepareCleanmgrRegistry()
        {
            //todo will this(64 view) cause errror on 32 system?
            RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey volumeCahches = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches", true);

            foreach (string keyname in volumeCahches.GetSubKeyNames())
            {
                if (keyname.Equals("Update Cleanup"))
                {
                    //for now just ignoring the update cleanup because it is taking to long
                    continue;
                }

                RegistryKey key = volumeCahches.OpenSubKey(keyname, true);

                key.SetValue($"StateFlags{CLEANMGR_STATEFLAGS_ID}", "2", Microsoft.Win32.RegistryValueKind.DWord);
                //Registry.SetValue(key.Name, $"StateFlags{CLEANMGR_STATEFLAGS_ID}", "2", Microsoft.Win32.RegistryValueKind.DWord);
                //Info(key.GetValue($"StateFlags{CLEANMGR_STATEFLAGS_ID}").ToString()); // Replace KEY_NAME with what you're looking for
            }
            volumeCahches.Close();
            root.Close();
        }
        public void TmpRegisty()
        {
            RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey volumeCahches = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches", true);

            foreach (string keyname in volumeCahches.GetSubKeyNames())
            {
                if (keyname.Equals("Update Cleanup"))
                {
                    //for now just ignoring the update cleanup because it is taking to long
                    continue;
                }

                RegistryKey key = volumeCahches.OpenSubKey(keyname, true);

                key.SetValue($"StateFlags{CLEANMGR_STATEFLAGS_ID}", "2", Microsoft.Win32.RegistryValueKind.DWord);
                //Registry.SetValue(key.Name, $"StateFlags{CLEANMGR_STATEFLAGS_ID}", "2", Microsoft.Win32.RegistryValueKind.DWord);
                //Info(key.GetValue($"StateFlags{CLEANMGR_STATEFLAGS_ID}").ToString()); // Replace KEY_NAME with what you're looking for
            }
        }
        public void RunCleanmgr()
        {
            if (cleanmgrProc != null)
            {
                //SDeleteError?.Invoke("SDelete is already running");
                return;
            }
            Info("Starting cleanmgr...");
            PrepareCleanmgrRegistry();

            //todo deregister events before loosing reference
            cleanmgrProc = new Process();
            cleanmgrProc.StartInfo.FileName = "cleanmgr.exe";
            cleanmgrProc.StartInfo.Arguments = $"/sagerun:{CLEANMGR_STATEFLAGS_ID}";
            cleanmgrProc.StartInfo.UseShellExecute = false;
            cleanmgrProc.StartInfo.CreateNoWindow = true;
            cleanmgrProc.StartInfo.RedirectStandardOutput = true;
            cleanmgrProc.StartInfo.RedirectStandardError = true;

            cleanmgrProc.EnableRaisingEvents = true;
            cleanmgrProc.OutputDataReceived += CleanmgrProc_OutputDataReceived;
            cleanmgrProc.ErrorDataReceived += CleanmgrProc_ErrorDataReceived;
            cleanmgrProc.Exited += CleanmgrProc_Exited;


            cleanmgrProc.Start();
            cleanmgrProc.BeginErrorReadLine();
            cleanmgrProc.BeginOutputReadLine();
            Info($"started cleanmgr; PID = {cleanmgrProc.Id}");

        }

        public void RunDefrag()
        {
            //1. Enable defrag service 
            WinServiceUtils.EnableService("defragsvc");
            //2. Create the process and register for the events
            //3. Run the process
            //4. Print the outputs 
            //5. Handle the ending
            if (defragProc != null)
            {
                //SDeleteError?.Invoke("SDelete is already running");
                return;
            }
            defragProcExited = false;
            Info("Starting defrag...");
            //PrepareCleanmgrRegistry();

            //todo deregister events before loosing reference
            defragProc = new Process();
            defragProc.StartInfo.FileName = Path.Combine(@"C:\Windows\Sysnative", "Defrag.exe");

            defragProc.StartInfo.Arguments = $"/C /O /V /H /U";
            defragProc.StartInfo.UseShellExecute = false;
            defragProc.StartInfo.CreateNoWindow = true;
            defragProc.StartInfo.RedirectStandardOutput = true;
            defragProc.StartInfo.RedirectStandardError = true;
            defragProc.StartInfo.RedirectStandardInput = true;


            defragProc.EnableRaisingEvents = true;
            defragProc.OutputDataReceived += DefragProc_OutputDataReceived;
            defragProc.ErrorDataReceived += DefragProc_ErrorDataReceived; ;
            defragProc.Exited += DefragProc_Exited;


            defragProc.Start();
            defragProc.BeginErrorReadLine();
            defragProc.BeginOutputReadLine();
            Info($"started defrag; PID = {defragProc.Id}");
        }

        #region trying out running defrag in powershell
        [Obsolete]
        public void RunDefragPS()
        {
            try
            {
                WinServiceUtils.EnableService("defragsvc");
                PowerShell powershell = PowerShell.Create();

                PSDataCollection<PSObject> output = new PSDataCollection<PSObject>();
                output.DataAdded += Output_DataAdded;
                powershell.InvocationStateChanged += Powershell_InvocationStateChanged;

                powershell.AddScript("defrag /C /O /V /H /U");


                IAsyncResult asyncResult = powershell.BeginInvoke<PSObject, PSObject>(null, output);





            }
            catch (Exception e)
            {
                throw new Exception("Could not enable the service, error: " + e.Message);
            }

        }

        [Obsolete]
        private void Powershell_InvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
        {
            Info("Incovation state chaged: " + e.InvocationStateInfo.State);
        }
        [Obsolete]
        private void Output_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<PSObject> myp = (PSDataCollection<PSObject>)sender;

            Collection<PSObject> results = myp.ReadAll();
            Info("Data added; count: " + results.Count);
            foreach (PSObject result in results)
            {
                Info(result.ToString());
            }
        }
        #endregion


        private void DefragProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Info($"defrag process Error: {e.Data}");
        }

        private void DefragProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var output = e.Data;
            if (e.Data == null && defragProcExited)
            {
                var exitCode = defragProc.ExitCode;
                bool hasExited = defragProc.HasExited;
                defragProc.Close();
                defragProc = null;
                Info($"Finished execution. Exit code: {exitCode}; really exited {hasExited}; ");
            }
            else
            {
                Info($"defrag process Info: {output}");

            }
        }

        private void DefragProc_Exited(object sender, EventArgs e)
        {
            defragProcExited = true;
            //todo also consider adding a timeout in case the null data is never received


        }


        private void CleanmgrProc_Exited(object sender, EventArgs e)
        {
            var exitCode = cleanmgrProc.ExitCode;
            Info($"Finished execution. Exit code: {exitCode}");
            cleanmgrProc.Close();
            cleanmgrProc = null;
        }

        private void CleanmgrProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Info($"cleanmgr process Error: {e.Data}");
        }

        private void CleanmgrProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            Info($"cleanmgr process Info: {e.Data}");
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