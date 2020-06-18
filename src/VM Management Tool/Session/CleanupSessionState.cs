using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Session
{
    public class CleanupSessionState : SessionState
    {
        public List<(string, bool, int)> Results { get; set; }
        public bool RunDiskCleanmgr { get; set; }
        public bool RunSDelete { get; set; }
        public bool RunDefrag { get; set; }
        public bool RunDism { get; set; }
    }
}
