using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services
{
    static class WinServiceUtils
    {
        //TODO rewrite this with more consideration
        //and proper APIs
        public static bool StopService(string serviceName)
        {
            ServiceController sc = new ServiceController(serviceName);

            try
            {
                if (sc != null && sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                sc.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static bool StartService(string serviceName)
        {
            ServiceController sc = new ServiceController(serviceName);

            try
            {
                if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                {
                    sc.Start();
                }
                sc.WaitForStatus(ServiceControllerStatus.Running);
                sc.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Enables the Windows Service
        /// </summary>
        public static void EnableService(string serviceName)
        {
            try
            {
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
                proc.WaitForExit();
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
            }
            catch (Exception e)
            {
                throw new Exception("Could not disable the service, error: " + e.Message);
            }
        }
    }
}
