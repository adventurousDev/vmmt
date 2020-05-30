using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services.Optimization.Actions
{
    class CustomCheckAction : Action_
    {
        public enum CustomCheckTarget
        {
            DiskCount,
            DiskSpace,
            InstalledProgram

        }
        public CustomCheckTarget Target { get; private set; }
        public CustomCheckAction(CustomCheckTarget target, Dictionary<string, string> params_) : base(params_)
        {
            Target = target;
        }

        public override bool Execute()
        {
            Log.Error("CustomCheckAction.Execute", "Not supported.");
            return false;
        }

        public override StatusResult CheckStatus()
        {
            Log.Error("CustomCheckAction.CheckStatus", "Not supported.");
            return StatusResult.Unavailable;
        }
    }
}
