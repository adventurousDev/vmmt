using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services
{
    class WinUpdateStatus
    {

        public string Title { get; private set; }
        public List<string> KBIds { get; private set; }
        public bool IsInstalled { get;  set; }

        public string Error { get;  set; }

        public WinUpdateStatus(string updateTitle, List<string> KBs )
        {
            Title = updateTitle;
            KBIds = KBs;
           
        }
        

    }
}
