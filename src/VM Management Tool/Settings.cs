using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool
{
    static class Settings
    {
        //public const string OPTIMIZATION_TEMPLATE_PATH = @"..\..\..\..\testdata\Hayk_Windows 10 1507-1803-Server 2016_comp.xml";
        public const string OPTIMIZATION_TEMPLATE_PATH = @"Resources\default.xml";
        //todo because of EULA the executable needs to be downloaded
        //because it can not be distributed with the app
        public const string SDELETE_FOLDER = @"C:\bwLehrpool\SDelete";

        public const bool DEBUG = true;
        public const string VERSION = "1.0.0"; 
    }
}
