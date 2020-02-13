using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services.Optimization.Actions
{
    //todo decided against custom action sub-types for now
    //so these will need to be removed probably
    class ServiceAction : Action_
    {
        public const string PARAM_NAME_SERVICE_NAME = "serviceName";
        public const string PARAM_NAME_START_MODE = "startMode";
        public ServiceAction(Dictionary<string, string> params_) : base(params_)
        {

        }

        public override StatusResult CheckStatus()
        {
            try
            {
                var name = Params[PARAM_NAME_SERVICE_NAME];
                var mode = NormalizeStartMode(Params[PARAM_NAME_START_MODE]);
                var actualMode = WinServiceUtils.GetStartupType(name);
                if(actualMode.ToLower() == mode.ToLower())
                {
                    return StatusResult.Match;
                }
                else
                {
                    return StatusResult.Mismatch;
                }
            }
            catch (Exception e)
            {

                return StatusResult.Unavailable;
            }
        }

        public override bool Execute()
        {
            try
            {
                var name = Params[PARAM_NAME_SERVICE_NAME];
                var mode = NormalizeStartMode(Params[PARAM_NAME_START_MODE]);
                 WinServiceUtils.SetStartupType(name, mode);
                return true;
               
            }
            catch (Exception e)
            {

                return false;
            }
        }
        private string NormalizeStartMode(string startModeParam)
        {
            switch (startModeParam.ToLower())
            {
                case "manual":
                    return "manual";
                case "automatic":
                case "auto":
                    return "automatic";
                case "disabled":
                    return "disabled";
                case "system":
                    return "system";
                case "boot":
                    return "boot";
                default:
                    throw new Exception("Unknown service start mode: "+startModeParam);
            }
        }
    }
}
