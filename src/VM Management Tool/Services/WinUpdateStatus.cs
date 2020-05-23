using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services
{
    public class WinUpdateStatus
    {

        public string Title { get; set; }
        public List<string> KBIds { get; set; }
        public bool IsInstalled { get;  set; }

        public string Error { get;  set; }

        //for json deserialization
        public WinUpdateStatus()
        {

        }
        public WinUpdateStatus(string updateTitle, List<string> KBs )
        {
            Title = updateTitle;
            KBIds = KBs;
           
        }
        

    }
}
