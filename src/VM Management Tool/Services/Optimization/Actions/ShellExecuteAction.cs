using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services.Optimization.Actions
{
    //todo decided against custom action sub-types for now
    //so these will need to be removed probably
    class ShellExecuteAction : Action_
    {
       

        public string ShellCommand { get; private set; }
        public ShellExecuteAction(string shellCommand) : base(null)
        {
            ShellCommand = shellCommand;
        }

        public override bool Execute()
        {
            return new ShellCommand(ShellCommand).TryExecute(out _);
        }

        public override StatusResult CheckStatus()
        {
            return StatusResult.Unavailable;
        }
    }
}
