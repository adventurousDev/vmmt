using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services.Optimization.Actions
{
    //todo decided against custom action sub-types for now
    //so these will need to be removed probably
    class RegistryAction : Action_
    {
        public enum RegistryCommand
        {
            Add,
            DeleteKey,
            DeleteValue,
            Load,
            Unload

        }

        public RegistryCommand Command { get; private set; }
        
        public RegistryAction( RegistryCommand command, Dictionary<string, string> params_) :base(params_)
        {
            Command = command;
            
        }
    }
}
