using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Configuration
{
    public class OSOTTemplateMeta
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string FilePath { get; set; }
        public OSOTTemplateType Type { get; set; }
        public string ID { get { return $"{Name} ({Version})"; } }


    }
    public enum OSOTTemplateType 
    { 
        Unknown,
        System, 
        User
    }
}
