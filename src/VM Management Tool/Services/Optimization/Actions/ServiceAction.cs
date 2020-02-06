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
        public ServiceAction(Dictionary<string, string> params_) : base(params_)
        {

        }
    }
}
