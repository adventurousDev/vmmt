using Microsoft.Win32;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace VMManagementTool.Services
{
    static class WinServiceUtils
    {


        public static async Task<bool> StopServiceAsync(string serviceName, int timeout)
        {
            ServiceController sc = new ServiceController(serviceName);

            try
            {
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    return true;
                }
                else if (sc.CanStop)
                {
                    sc.Stop();
                    DateTime utcNow = DateTime.UtcNow;
                    sc.Refresh();
                    while (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        if ((DateTime.UtcNow.Ticks - utcNow.Ticks) / 10000 > timeout)
                        {
                            return false;
                        }
                        await Task.Delay(250).ConfigureAwait(false);
                        sc.Refresh();

                    }

                    return true;
                }



            }
            catch (Exception ex)
            {
                Log.Error("WinServiceUtils.StopServiceAsync", ex.ToString());
                return false;
            }
            finally
            {
                sc.Close();
            }
            return false;
        }
        public static async Task<bool> StartServiceAsync(string serviceName, int timeout)
        {
            ServiceController sc = new ServiceController(serviceName);

            try
            {
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    return true;
                }
                else
                {
                    sc.Start();
                    DateTime utcNow = DateTime.UtcNow;
                    sc.Refresh();
                    while (sc.Status != ServiceControllerStatus.Running)
                    {
                        if ((DateTime.UtcNow.Ticks - utcNow.Ticks) / 10000 > timeout)
                        {
                            return false;
                        }
                        await Task.Delay(250).ConfigureAwait(false);
                        sc.Refresh();

                    }

                    return true;
                }





            }
            catch (Exception ex)
            {
                Log.Error("WinServiceUtils.StartServiceAsync", ex.ToString());
                return false;
            }
            finally
            {
                sc.Close();
            }

        }
        /// <summary>
        /// Enables the Windows Service
        /// </summary>
        public static void EnableService(string serviceName)
        {
            SetStartupType(serviceName, "manual");
        }

        /// <summary>
        /// Disables the Windows service
        /// </summary>
        public static void DisableService(string serviceName)
        {
            SetStartupType(serviceName, "disabled");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception">When get-service PS cmdlet fails</exception>
        public static string GetStartupType(string serviceName)
        {
            /*
            using (PowerShell shell = PowerShell.Create())
            {
                shell.AddCommand("get-service").AddParameter("name", serviceName);
                //shell.AddParameter("startuptype", startupType);
                var resCollection = shell.Invoke();
                if (resCollection.Count > 0)
                {
                    var startType = resCollection[0].Properties["StartType"].Value;
                    return startType.ToString();
                }
                else
                {
                    throw new Exception($"The service {serviceName} not found.");
                }
            }*/
            return GetServiceStartupModeReg(serviceName);

        }
        /// <exception cref="System.Exception">When set-service PS cmdlet fails</exception>
        public static void SetStartupType(string serviceName, string startupType)
        {
            /*
            using (PowerShell shell = PowerShell.Create())
            {
                shell.AddCommand("set-service").AddParameter("name", serviceName);
                shell.AddParameter("startuptype", startupType);

                shell.Invoke();
                if (shell.HadErrors)
                {
                    throw new Exception("Errors setting service startup mode");
                }
            }*/
            SetServiceStartupModeCMD(serviceName, startupType);



        }

        public static bool TrySetStartupType(string serviceName, bool enabled)
        {
            string startupType = "";
            if (enabled)
            {
                startupType = "manual";
            }
            else
            {
                startupType = "disabled";
            }

            try
            {
                SetServiceStartupModeCMD(serviceName, startupType);
                return true;
            }
            catch (Exception e)
            {
                Log.Error("WinServiceUtils.SetStartupTypeAsync", e.Message);
                return false;
            }

        }

        public static void SetServiceStartupModeCMD(string serviceName, string startupMode)
        {
            //<boot|system|auto|demand|disabled|delayed-auto>
            string modeStr;
            switch (startupMode)
            {
                //ps to sc string translation
                case "automatic":
                    modeStr = "auto";
                    break;
                case "manual":
                    modeStr = "demand";
                    break;
                case "disabled":
                    modeStr = "disabled";
                    break;
                default:
                    throw new Exception("Unsupported service strat mode: "+startupMode) ;
                    break;
            }
            var args = $"sc config {serviceName} start={modeStr}";
            var cmd = new ShellCommand(args);
            if (cmd.TryExecute(out string scOutput))
            {
                Log.Debug("WinServiceUtils.SetServiceStartupModeCMD", "sc output: " + scOutput);
            }
            else
            {
                throw new Exception("Setting service start mode via sc failed:");
            }


        }
        public static string GetServiceStartupModeReg(string serviceName)
        {
            using (RegistryKey serviceKey = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}", false))
            {
                if (serviceKey != null)
                {
                    int mode = (int)serviceKey.GetValue("Start");
                    string modeStr;
                    switch (mode)
                    {
                        case 0:
                            modeStr = "boot";
                            break;
                        case 1:
                            modeStr = "system";
                            break;
                        case 2:
                            modeStr = "automatic";
                            break;
                        case 3:
                            modeStr = "manual";
                            break;
                        case 4:
                            modeStr = "disabled";
                            break;
                        default:
                            modeStr = "unknown";
                            break;
                    }

                    return modeStr;
                }
                else
                {
                    throw new Exception($"Service {serviceName} does not exist");
                }
            }
        }
    }
}
