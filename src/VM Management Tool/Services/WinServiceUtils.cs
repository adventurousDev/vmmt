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
            SetStartupType(serviceName, "manual");
        }

        /// <summary>
        /// Disables the Windows service
        /// </summary>
        public static void DisableService(string serviceName)
        {
            SetStartupType(serviceName, "disabled");
        }
        public static string GetStartupType(string serviceName)
        {
            try
            {
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
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not change serice startup type" + e.Message);
            }
        }
        public static void SetStartupType(string serviceName, string startupType)
        {
            try
            {
                using (PowerShell shell = PowerShell.Create())
                {
                    shell.AddCommand("set-service").AddParameter("name", serviceName);
                    shell.AddParameter("startuptype", startupType);
                    
                    shell.Invoke();
                    if (shell.HadErrors)
                    {
                        throw new Exception("Errors setting service startup mode");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not change serice startup type" + e.Message);
            }


        }
    }
}
