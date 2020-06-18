using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMManagementTool.Configuration;

namespace VMManagementTool.Session
{
    public class OSOTSessionState : SessionState
    {
        public List<(string, bool)> Results { get; set; }
        public OSOTTemplateMeta OSOTTemplateMetadata { get; set; }
        public HashSet<string> CustomStepsChoice { get; set; }
        public StepsChoice StepsChoiceOption { get; set; }

        public enum StepsChoice : long
        {
            Default = 0,
            All = 1,
            Custom = 2
        }
    }
}
