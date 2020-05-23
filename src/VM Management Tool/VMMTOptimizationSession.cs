using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMManagementTool.Services;

namespace VMManagementTool
{
    class VMMTOptimizationSession
    {
        public Dictionary<string, WinUpdateStatus> WinUpdateResults { get; set; }
        public List<(string, bool)> OSOTResults { get; set; }
        public List<(string, bool, int)> CleanupResults { get; set; }

        public VMMTOptimizationSession()
        {
            //todo consider generating some sort of GUID here
        }
        
    }
}
