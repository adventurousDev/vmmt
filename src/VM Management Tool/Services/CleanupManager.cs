using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;


namespace VMManagementTool.Services
{
    class CleanupManager : IDisposable
    {

        public const string TOOL_NAME_CLEANMGR = "Disk Cleanup";
        public const string TOOL_NAME_SDELETE = "SDelete";
        public const string TOOL_NAME_DEFRAG = "Defrag";
        public const string TOOL_NAME_DISM = "Dism";

        Process sDeleteProc;
        Process cleanmgrProc;
        Process defragProc;
        Process dismProc;

        //public event Action<string> NewInfo;
        //public event Action<int> SDeleteExited;
        //public event Action<string> SDeleteError;
        //public event Action<string, int> SDeleteProgressChanged;


        //public event Action<bool> CleanmgrCompleted;
        //public event Action<bool> DefragCompleted;
        //public event Action<bool> SdeleteCompleted;
        public event Action<string, bool> ToolCompleted;

        //public event Action<int, string> ProgressChanged;


        List<(string, bool, int)> results = new List<(string, bool, int)>();

        public bool SDeleteRunning { get { return sDeleteProc != null; } }
        private const int SW_HIDE = 0;
        private const int CLEANMGR_STATEFLAGS_ID = 9999;

        volatile bool defragProcExited = false;

        long lastSdeleteProgressTime = 0;
        int lastProgress = 0;

        public void StartTool(string name)
        {
            switch (name)
            {
                case TOOL_NAME_CLEANMGR:
                    StartCleanmgr();
                    break;
                case TOOL_NAME_DISM:
                    StartDism();
                    break;
                case TOOL_NAME_SDELETE:
                    StartSdelete();
                    break;
                case TOOL_NAME_DEFRAG:
                    StartDefrag();
                    break;


            }
        }

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
                    //SDeleteError?.Invoke("SDelete is already running");
                    Log.Error("CleanupManager.RunSDelete", "SDelete is already running");
                    ToolCompleted?.Invoke(TOOL_NAME_SDELETE, false);
                    return;
                }


                sDeleteProc = new Process();
                var executable = Environment.Is64BitOperatingSystem ? "sdelete64.exe" : "sdelete.exe";
                var path = Path.Combine(Configs.TOOLS_DIR, "SDelete", executable);
                /*
                var path = Path.Combine(Configs.SDELETE_FOLDER, executable);
                if (executable == "sdelete64.exe" && !File.Exists(path))
                {
                    //try using 32 bit because it is more probable to be there 
                    path = Path.Combine(@"C:\bwLehrpool\SDelete", "sdelete.exe");
                }*/
                sDeleteProc.StartInfo.FileName = path;
                sDeleteProc.StartInfo.Arguments = "/accepteula -z c:";
                sDeleteProc.StartInfo.UseShellExecute = true;
                sDeleteProc.StartInfo.CreateNoWindow = false;
                //sDeleteProc.StartInfo.RedirectStandardOutput = true;
                //sDeleteProc.StartInfo.RedirectStandardError = true;

                sDeleteProc.EnableRaisingEvents = true;
                //sDeleteProc.OutputDataReceived += SDeleteProc_OutputDataReceived;
                //sDeleteProc.ErrorDataReceived += SDeleteProc_ErrorDataReceived;
                sDeleteProc.Exited += SDeleteProc_Exited;

                Log.Debug("CleanupManager.RunSDelete", "starting SDlete");
                sDeleteProc.Start();
                //sDeleteProc.BeginErrorReadLine();
                //sDeleteProc.BeginOutputReadLine();
            }
            catch (Exception e)
            {
                Log.Error("CleanupManager.RunSDelete", e.ToString());
            }

        }

        private void SDeleteProc_Exited(object sender, EventArgs e)
        {
            try
            {
                var exitCode = sDeleteProc.ExitCode;
                //removing events just in case
                sDeleteProc.Exited -= SDeleteProc_Exited;
                sDeleteProc.Close();
                sDeleteProc = null;

                //SDeleteExited?.Invoke(exitCode);

                results.Add((TOOL_NAME_SDELETE, exitCode == 0, exitCode));
                //SdeleteCompleted?.Invoke(exitCode == 0);
                ToolCompleted?.Invoke(TOOL_NAME_SDELETE, exitCode == 0);
                Log.Debug("CleanupManager.SDeleteProc_Exited", $"code: {exitCode}");
            }
            catch (Exception ex)
            {
                Log.Error("CleanupManager.SDeleteProc_Exited", ex.ToString());
            }
        }





        //CLEANMGR ----------------------------------------------------------

        private void PrepareCleanmgrRegistry()
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
                    throw new Exception("Cleanmgr is already running");
                }

                PrepareCleanmgrRegistry();


                cleanmgrProc = new Process();
                cleanmgrProc.StartInfo.FileName = "cleanmgr.exe";
                cleanmgrProc.StartInfo.Arguments = $"/sagerun:{CLEANMGR_STATEFLAGS_ID}";
                cleanmgrProc.StartInfo.UseShellExecute = false;
                cleanmgrProc.StartInfo.CreateNoWindow = true;
                cleanmgrProc.StartInfo.RedirectStandardOutput = true;
                cleanmgrProc.StartInfo.RedirectStandardError = true;

                cleanmgrProc.EnableRaisingEvents = true;
                //cleanmgrProc.OutputDataReceived += CleanmgrProc_OutputDataReceived;
                //cleanmgrProc.ErrorDataReceived += CleanmgrProc_ErrorDataReceived;
                cleanmgrProc.Exited += CleanmgrProc_Exited;

                Log.Debug("CleanupManager.RunCleanmgr", "starting Cleanmgr");
                cleanmgrProc.Start();
                cleanmgrProc.BeginErrorReadLine();
                cleanmgrProc.BeginOutputReadLine();

            }
            catch (Exception e)
            {

                Log.Error("CleanupManager.RunCleanmgr", e.ToString());
            }

        }

        private void CleanmgrProc_Exited(object sender, EventArgs e)
        {
            try
            {
                var exitCode = cleanmgrProc.ExitCode;
                results.Add((TOOL_NAME_CLEANMGR, exitCode == 0, exitCode));
                //CleanmgrCompleted?.Invoke(exitCode == 0);
                ToolCompleted?.Invoke(TOOL_NAME_CLEANMGR, exitCode == 0);
                cleanmgrProc.Close();
                cleanmgrProc = null;
                Log.Debug("CleanupManager.CleanmgrProc_Exited", $"code: {exitCode}");
            }
            catch (Exception ex)
            {
                Log.Error("CleanupManager.CleanmgrProc_Exited", ex.ToString());
            }

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

                //PrepareCleanmgrRegistry();


                defragProc = new Process();
                defragProc.StartInfo.FileName = Path.Combine(SystemUtils.GetSystem32Path(true), "Defrag.exe");

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

                Log.Debug("CleanupManager.RunDefrag", "starting Defrag");

                defragProc.Start();
                //defragProc.BeginErrorReadLine();
                //defragProc.BeginOutputReadLine();
                //Info($"started defrag; PID = {defragProc.Id}");
            }
            catch (Exception e)
            {
                Log.Error("CleanupManager.RunDefrag", e.ToString());
            }
        }





        private void DefragProc_Exited(object sender, EventArgs e)
        {
            try
            {
                var exitCode = defragProc.ExitCode;
                defragProcExited = true;

                results.Add((TOOL_NAME_DEFRAG, exitCode == 0, exitCode));
                //disable the serivice after using it
                try
                {
                    WinServiceUtils.DisableService("defragsvc");
                }
                catch (Exception ex)
                {
                    Log.Error("CleanupManager.DefragProc_Exited::(disable defragsvc)", ex.ToString());
                }
                //DefragCompleted?.Invoke(defragProc.ExitCode == 0);
                ToolCompleted?.Invoke(TOOL_NAME_DEFRAG, exitCode == 0);

                Log.Debug("CleanupManager.DefragProc_Exited", $"code: {exitCode}");
            }
            catch (Exception ex)
            {
                Log.Error("CleanupManager.DefragProc_Exited", ex.ToString());
            }

        }

        //DISM
        private void StartDism()
        {
            Task.Run(() => RunDism());
        }

        private void RunDism()
        {
            try
            {
                if (Environment.OSVersion.Version < new Version("6.3"))
                {
                    throw new Exception("Dism cleanup commands are only supported starting Windows 8.1");
                }
                if (dismProc != null)
                {
                    //SDeleteError?.Invoke("SDelete is already running");
                    Log.Error("CleanupManager.RunDism", "Dism is already running");
                    ToolCompleted?.Invoke(TOOL_NAME_SDELETE, false);
                    return;
                }


                dismProc = new Process();

                var path = Path.Combine(SystemUtils.GetSystem32Path(true), "dism.exe");


                dismProc.StartInfo.FileName = path;


                //test
                //dismProc.StartInfo.Arguments = "/Online /Cleanup-Image /AnalyzeComponentStore";
                dismProc.StartInfo.Arguments = " /Online /Cleanup-Image /StartComponentCleanup";
                dismProc.StartInfo.UseShellExecute = true;
                dismProc.StartInfo.CreateNoWindow = false;
                //sDeleteProc.StartInfo.RedirectStandardOutput = true;
                //sDeleteProc.StartInfo.RedirectStandardError = true;

                dismProc.EnableRaisingEvents = true;
                //sDeleteProc.OutputDataReceived += SDeleteProc_OutputDataReceived;
                //sDeleteProc.ErrorDataReceived += SDeleteProc_ErrorDataReceived;
                dismProc.Exited += DismProc_Exited;

                Log.Debug("CleanupManager.RunDism", "starting Dism");

                dismProc.Start();
                //sDeleteProc.BeginErrorReadLine();
                //sDeleteProc.BeginOutputReadLine();
            }
            catch (Exception e)
            {
                Log.Error("CleanupManager.RunDism", e.ToString());
            }
        }

        private void DismProc_Exited(object sender, EventArgs e)
        {
            try
            {
                var exitCode = dismProc.ExitCode;
                //removing events just in case
                dismProc.Exited -= DismProc_Exited;
                dismProc.Close();
                dismProc = null;

                //SDeleteExited?.Invoke(exitCode);

                results.Add((TOOL_NAME_DISM, exitCode == 0, exitCode));
                //SdeleteCompleted?.Invoke(exitCode == 0);
                ToolCompleted?.Invoke(TOOL_NAME_DISM, exitCode == 0);
                Log.Debug("CleanupManager.DismProc_Exited", $"code: {exitCode}");
            }
            catch (Exception ex)
            {
                Log.Error("CleanupManager.DismProc_Exited", ex.ToString());
            }

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
                if (dismProc != null && !dismProc.HasExited)
                {
                    dismProc.Kill();

                }
            }
            catch (Exception e)
            {
                Log.Error("CleanupManager.Abort", e.ToString());
            }

        }



        public List<(string, bool, int)> GetResults()
        {
            return results;
        }

        public bool HasCompleted(string tool)
        {
            return results.Exists((r) => r.Item1.Equals(tool));
        }
        public void Dispose()
        {
            cleanmgrProc?.Dispose();
            defragProc?.Dispose();
            sDeleteProc?.Dispose();
            dismProc?.Dispose();
        }
    }
}