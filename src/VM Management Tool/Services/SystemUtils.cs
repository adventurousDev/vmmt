using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services
{
    static class SystemUtils
    {
        const string RESTART_SHTASK_NAME = "VMManagementTool.Restart";
        public static void ScheduleAfterRestart()
        {
            
            var myExePath = Assembly.GetEntryAssembly().Location;
            var workDirPath = Path.GetDirectoryName(myExePath);
            //an alternative to task scheduler could be Run/ RunOnce registy keys
            var schCmd = $"schtasks.exe /create /tn \"{RESTART_SHTASK_NAME}\" /ru SYSTEM /sc ONLOGON /tr \"psexec -d -accepteula -i 1 -w '{workDirPath}' '{myExePath}' '/resume'\"";
            //todo check that the task does not exist before attempting creation
            //and then reactivate the exception below
            var cmd = new ShellCommand(schCmd);
            if (!cmd.TryExecute(out _))
            {
                //throw new Exception("Unable to schedule after restart resume");
                Log.Error("SystemUtils::ScheduleAfterRestart", "Unable to schedule resume task");
            }
        }

        public static void DeleteResumeTask()
        {
            var schCmd = $"schtasks.exe /delete /tn \"{RESTART_SHTASK_NAME}\" /f";
            var cmd = new ShellCommand(schCmd);
            if (!cmd.TryExecute(out _))
            {
                Log.Error("SystemUtils::DeleteResumeTask", "Unable to delete resume task");
            }
        }

        public static void RestartSystem()
        {
            var cmdStr = "shutdown /r";
            var cmd = new ShellCommand(cmdStr);
            if (!cmd.TryExecute(out _))
            {
                throw new Exception("Unable to perform PC restart");
            }
        }

    }
}
