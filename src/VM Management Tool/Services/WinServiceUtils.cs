using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services
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
                if (sc.Status == ServiceControllerStatus.Running)
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
            try
            {
                using (PowerShell shell = PowerShell.Create())
                {
                    shell.AddCommand("set-service").AddParameter("name", serviceName);
                    shell.AddParameter("startuptype", "manual");
                    shell.Invoke();
                }



                /*
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = "/C powershell.exe Set-Service '" + serviceName + "' -startuptype \"Manual\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = "cmd.exe"
                    }
                };
                //proc.OutputDataReceived += (s, e) => LogWriter.LogWrite(e.Data);
                //proc.ErrorDataReceived += (s, e) => LogWriter.LogWrite(e.Data);
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();*/
            }
            catch (Exception e)
            {
                throw new Exception("Could not enable the service, error: " + e.Message);
            }
        }

        /// <summary>
        /// Disables the Windows service
        /// </summary>
        public static void DisableService(string serviceName)
        {
            try
            {
                using (PowerShell shell = PowerShell.Create())
                {
                    shell.AddCommand("set-service").AddParameter("name", serviceName);
                    shell.AddParameter("startuptype", "disabled");
                    shell.Invoke();
                }



                /*
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = "/C powershell.exe Set-Service '" + serviceName + "' -startuptype \"DISABLED\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = "cmd.exe"
                    }
                };
                //proc.OutputDataReceived += (s, e) => LogWriter.LogWrite(e.Data);
                //proc.ErrorDataReceived += (s, e) => LogWriter.LogWrite(e.Data);
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                */
            }
            catch (Exception e)
            {
                throw new Exception("Could not disable the service, error: " + e.Message);
            }
        }
    }
}
