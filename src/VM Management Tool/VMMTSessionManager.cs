using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool
{
    class VMMTSessionManager
    {

        private static readonly object instancelock = new object();
        private static VMMTSessionManager instance = null;

        private VMMTOptimizationSession optimizationSession;

        public static VMMTSessionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new VMMTSessionManager();
                        }
                    }
                }
                return instance;
            }
        }

        public const string WIN_UPDATE_RESULTS_KEY = "winupdateresults";
        public const string OSOT_RESULTS_KEY = "osotresults";
        public const string CLEANUP_RESULTS_KEY = "cleanupresults";

        public void StartOptimizationSession()
        {
            optimizationSession = new VMMTOptimizationSession();
        }
        public VMMTOptimizationSession FinishCurrentSession()
        {
            var tmpRef = optimizationSession;
            optimizationSession = null;
            return tmpRef;
        }
        public void AddOptimizationResults(string optimizationTask, object result )
        {
            optimizationSession.AddResult(optimizationTask, result);
        }
            
    }
}
