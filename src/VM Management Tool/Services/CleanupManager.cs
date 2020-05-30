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

namespace VMManagementTool.Services
{
    class CleanupManager : IDisposable//todo cleanup and exception handlign in main methods(especially async)
    {
        private static readonly object instancelock = new object();
        private static CleanupManager instance = null;
        //todo remove the singleton if unused
        public static CleanupManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new CleanupManager();
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


        public event Action<bool> CleanmgrCompleted;
        public event Action<bool> DefragCompleted;
        public event Action<bool> SdeleteCompleted;

        public event Action<int, string> ProgressChanged;


        List<(string, bool, int)> results = new List<(string, bool, int)>();

        public bool SDeleteRunning { get { return sDeleteProc != null; } }
        private const int SW_HIDE = 0;
        private const int CLEANMGR_STATEFLAGS_ID = 9999;

        volatile bool defragProcExited = false;

        long lastSdeleteProgressTime = 0;
        int lastProgress = 0;



        //SDELETE -----------------------------------------------------------
        public void StartSdelete()
        {
            Task.Run(RunSDelete);
        }
        public void RunSDelete()
        {
            try
            {
                if (sDeleteProc != null)
                {
                    SDeleteError?.Invoke("SDelete is already running");
                    return;
                }

                //todo deregister events before loosing reference
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
                //sDeleteProc.OutputDataReceived += SDeleteProc_OutputDataReceived;
                //sDeleteProc.ErrorDataReceived += SDeleteProc_ErrorDataReceived;
                sDeleteProc.Exited += SDeleteProc_Exited;


                sDeleteProc.Start();
                //sDeleteProc.BeginErrorReadLine();
                //sDeleteProc.BeginOutputReadLine();
            }
            catch (Exception e)
            {
                Log.Error("CleanupManager.RunSDelete", e.Message);
            }

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

            results.Add(("SDelete", exitCode == 0, exitCode));
            SdeleteCompleted?.Invoke(exitCode == 0);
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
            int progress = 0;//no progress
            var percentageIndex = e.Data.LastIndexOf('%');
            if (percentageIndex > 0)
            {
                var spaceBeforePercentageIndex = e.Data.LastIndexOf(' ', percentageIndex);
                var percentageString = e.Data.Substring(spaceBeforePercentageIndex + 1, percentageIndex - spaceBeforePercentageIndex - 1);
                stage = e.Data.Substring(0, spaceBeforePercentageIndex - 1);
                progress = int.Parse(percentageString);
            }

            if (e.Data.StartsWith("Cleaning MFT"))
            {
                stage = "Cleaning MFT";
                progress = -8;//indefinite progress
            }

            //throtle progress for smooth UI
            var now = DateTime.UtcNow.Ticks;
            long elapsed = ((now - lastSdeleteProgressTime) / 10000);

            //update progress no more often than 100 ms
            //and only if the value changed: to avoid the indefinite
            //if (progress != lastSdeleteProgressTime && elapsed >= 5)//disabled throttling 
            {

                lastProgress = progress;
                lastSdeleteProgressTime = now;

                ProgressChanged?.Invoke(progress, stage);
                SDeleteProgressChanged?.Invoke(stage, progress);


            }

        }


        //CLEANMGR ----------------------------------------------------------
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
        public void StartCleanmgr()
        {
            Task.Run(() => RunCleanmgr());
        }
        public void RunCleanmgr()
        {
            try
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
            catch (Exception e)
            {

                Log.Error("CleanupManager.RunCleanmgr", e.Message);
            }

        }

        private void CleanmgrProc_Exited(object sender, EventArgs e)
        {
            var exitCode = cleanmgrProc.ExitCode;
            Info($"Finished execution. Exit code: {exitCode}");
            results.Add(("Disk Cleanup", exitCode == 0, exitCode));
            CleanmgrCompleted?.Invoke(exitCode == 0);
            cleanmgrProc.Close();
            cleanmgrProc = null;


        }

        private void CleanmgrProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Info($"cleanmgr process Error: {e.Data}");
            //todo handle the error
        }

        private void CleanmgrProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }
            Info($"cleanmgr process Info: {e.Data}");
        }

        //DEFRAG ------------------------------------------------------------
        public void StartDefrag()
        {
            Task.Run(() => RunDefrag());
        }
        public void RunDefrag()
        {
            try
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

                defragProc.StartInfo.Arguments = $"/C /O /H /U";
                defragProc.StartInfo.UseShellExecute = true;
                defragProc.StartInfo.CreateNoWindow = false;
                //defragProc.StartInfo.RedirectStandardOutput = true;
                //defragProc.StartInfo.RedirectStandardError = true;
                //defragProc.StartInfo.RedirectStandardInput = true;


                defragProc.EnableRaisingEvents = true;
                //defragProc.OutputDataReceived += DefragProc_OutputDataReceived;
                //defragProc.ErrorDataReceived += DefragProc_ErrorDataReceived; ;
                defragProc.Exited += DefragProc_Exited;


                defragProc.Start();
                //defragProc.BeginErrorReadLine();
                //defragProc.BeginOutputReadLine();
                //Info($"started defrag; PID = {defragProc.Id}");
            }
            catch (Exception e)
            {
                Log.Error("CleanupManager.RunDefrag", e.Message);
            }
        }

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
            var exitCode = defragProc.ExitCode;
            defragProcExited = true;

            results.Add(("Defrag", exitCode == 0, exitCode));

            DefragCompleted?.Invoke(defragProc.ExitCode == 0);

        }


        //MISC ----------------------------------------------------------------
        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        private void HideCleanMgrWndow()
        {
            var hWnd = cleanmgrProc.MainWindowHandle.ToInt32();
            ShowWindow(hWnd, SW_HIDE);
        }

        public void Abort()
        {
            try
            {
                if (cleanmgrProc != null && !cleanmgrProc.HasExited)
                {
                    cleanmgrProc.Kill();
                }
                if (sDeleteProc != null && !sDeleteProc.HasExited)
                {
                    sDeleteProc.Kill();
                }
                if (defragProc != null && !defragProc.HasExited)
                {
                    defragProc.Kill();
                    //defragProc.StandardInput.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error("CleanupManager.Abort", e.Message);
            }

        }

        //debug only
        private void Info(string text)
        {
            NewInfo?.Invoke(text);
            Log.Debug("CleanupManager", text);
        }

        public List<(string, bool, int)> GetResults()
        {
            return results;
        }

        public void Dispose()
        {
            cleanmgrProc?.Dispose();
            defragProc?.Dispose();
            sDeleteProc?.Dispose();
        }
    }
}