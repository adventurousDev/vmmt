using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services.Optimization.Actions
{
    class SchTasksAction : Action_
    {
        public const string PARAM_NAME_TASK_NAME = "taskName";
        public const string PARAM_NAME_STATUS = "status";

        public SchTasksAction(Dictionary<string, string> params_) : base(params_)
        {

        }

        //todo rewrite this using COM API
        public override StatusResult CheckStatus()
        {
            try
            {
                var taskName = Params[PARAM_NAME_TASK_NAME];
                string args = $"schtasks /query /tn \"{taskName}\" /hresult /fo csv /nh";
                var cmd = new ShellCommand(args);
                if (cmd.TryExecute(out string taskStatusCSV))
                {
                    var actualStatus = taskStatusCSV.Split(new[] { '\r', '\n' },StringSplitOptions.RemoveEmptyEntries)[0].Split(',')[2];//{Disabled, Ready}
                    var checkStatus = Params[PARAM_NAME_STATUS];// {DISABLED, ENABLED}
                    if((actualStatus.Trim().Trim('"') == "Disabled" && checkStatus == "DISABLED")
                        ||
                        (actualStatus.Trim().Trim('"') == "Ready" && checkStatus == "ENABLED")
                        )
                    {
                        return StatusResult.Match;
                    }
                    else
                    {
                        return StatusResult.Mismatch;
                    }
                }
                
            }
            catch (Exception)
            {

                return StatusResult.Unavailable;
            }

            return StatusResult.Unavailable;
           
                
        }
        //todo rewrite this using COM API
        public override bool Execute()
        {
            try
            {
                var taskName = Params[PARAM_NAME_TASK_NAME];
                var setStatus = Params[PARAM_NAME_STATUS];// {DISABLED, ENABLED}

                string statusSwitch = null;
                if (setStatus == "DISABLED")
                {
                    statusSwitch = "/disable";
                }
                else if (setStatus == "ENABLED")
                {
                    statusSwitch = "/enable";
                }
                else
                {
                    return false;
                }

                string args = $"schtasks /change /tn \"{taskName}\" {statusSwitch} /hresult";
                var cmd = new ShellCommand(args);
                return cmd.TryExecute(out _);
            }
            catch (Exception)
            {

                return false;
            }
            
        }
    }
}
