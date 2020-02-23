using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services.Optimization
{
    abstract class Action_
    {
        public enum StatusResult
        {
            Unavailable,
            Match,
            Mismatch

        }
       

        public bool MessageOnly { get; set; }
        //we could also keep the mutable dictionary as private and 
        //return it wrapped in readonly here in get
        public IReadOnlyDictionary<string, string> Params { get; protected set; }

        public Action_ CustomOptimization { get; set; }
        public Action_ CustomRollback { get; set; }
        public Action_(Dictionary<string, string> params_)
        {
            if (params_ != null)
            {
                Params = new ReadOnlyDictionary<string, string>(params_);
            }
        }

        public abstract StatusResult CheckStatus(); 
        public abstract bool Execute();
        
    }
}
