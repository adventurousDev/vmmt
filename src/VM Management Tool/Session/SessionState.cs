using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Session
{
    public abstract class SessionState
    {
        public bool IsAborted { get; set; }
    }
}
