using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool
{
    public static class Configs
    {
        //public const string OPTIMIZATION_TEMPLATE_PATH = @"..\..\..\..\testdata\Hayk_Windows 10 1507-1803-Server 2016_comp.xml";
        public const string OPTIMIZATION_TEMPLATE_DEFAULT_PATH = @"Resources\Defaults\osot.xml";
        

        //todo because of EULA the executable needs to be downloaded
        //because it can not be distributed with the app
        public const string SDELETE_FOLDER = @"C:\bwLehrpool\SDelete";

        public const bool DEBUG = true;
        public const string VERSION = "1.0.0";

        //public const string UPDATE_MANIFEST_URL = "https://www.dropbox.com/s/0kbyj4guwn3qpbr/update.json?raw=1";
        

        public const string CONFIGS_DIR = "Configuration";
        public const string TOOLS_DIR = "Tools";
        public const int CONFIG_DOWNLOAD_TIMEOUT = 5000;//5 sec
        public const string DEFAULT_CONFIG_FILE_PATH = @"Resources\Defaults\config.xml";
        public const string REMOTE_CONFIG_FILE_NAME = @"config.xml";
        public const string REMOTE_OPTIMIZATION_TEMPLATE_FILE_NAME = @"osot.xml";
        public const string CONFIG_FILE_URL = "https://www.dropbox.com/s/ukiu481to4i8ndn/config.xml?dl=1";
    }
}
