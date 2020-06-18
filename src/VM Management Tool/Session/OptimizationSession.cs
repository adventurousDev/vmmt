using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMManagementTool.Services;

namespace VMManagementTool.Session
{
    public class OptimizationSession
    {
        public WindowsUpdateSessionState WindowsUpdateSessionState { get; set; }
        public OSOTSessionState OSOTSessionState { get; set; }
        public CleanupSessionState CleanupSessionState { get; set; }
        //public Dictionary<string, WinUpdateStatus> WinUpdateResults { get; set; }
        //public List<(string, bool)> OSOTResults { get; set; }
        //public List<(string, bool, int)> CleanupResults { get; set; }

        public OptimizationSession()
        {
            //todo consider generating some sort of GUID here
        }
        public static OptimizationSession GenerateDefault()
        {
            var session = new OptimizationSession();
            session.WindowsUpdateSessionState = new WindowsUpdateSessionState();
            session.WindowsUpdateSessionState.RestartBehavior = WindowsUpdateSessionState.RestartBehaviors.Automatic;

            session.OSOTSessionState = new OSOTSessionState();
            session.OSOTSessionState.StepsChoiceOption = OSOTSessionState.StepsChoice.Default;
            session.OSOTSessionState.OSOTTemplateMetadata = ConfigurationManager.Instance.OSOTTemplatesData.Where((otmd) => otmd.Type == Configuration.OSOTTemplateType.System).FirstOrDefault();

            session.CleanupSessionState = new CleanupSessionState();            
            session.CleanupSessionState.RunDiskCleanmgr = true;            
            session.CleanupSessionState.RunSDelete = true;
            session.CleanupSessionState.RunDefrag = true;
            session.CleanupSessionState.RunDism = false;

            return session;
        }

    }
}
