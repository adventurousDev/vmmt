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

        public void StartOptimizationSession()
        {
            optimizationSession = new VMMTOptimizationSession();
        }

        public void AddOptimizationResults(string optimizationTask, object result )
        {
            optimizationSession.AddResult(optimizationTask, result);
        }
            
    }
}
