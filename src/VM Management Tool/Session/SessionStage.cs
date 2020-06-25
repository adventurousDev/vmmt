using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Session
{
    public class SessionStage
    {
        public string Title { get; set; }
        public States State { get; set; }
        public enum States
        {
            Scheduled,
            Processed,            
            Aborted,
            Active
        }
    }
}
