using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMManagementTool.Services;

namespace VMManagementTool.Session
{
    public class WindowsUpdateSessionState : SessionState
    {
        public Dictionary<string, WinUpdateStatus> Results { get; set; }
        public RestartBehaviors RestartBehavior { get; set; }

        public enum RestartBehaviors : long
        {
            Automatic = 0,
            Ask = 1,
            Skip = 2
        }
    }
}
