using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool
{
    class VMMTOptimizationSession
    {
        //optimization outcomes/ errors/ states of different tasks
        Dictionary<string, object> optimizationResults = new Dictionary<string, object>();
        public VMMTOptimizationSession()
        {
            //todo consider generating some sort of GUID here
        }
        public void AddResult(string optimizationTask, object result)
        {
            optimizationResults.Add(optimizationTask, result);
        }
        public Dictionary<string, object> GetResults()
        {
            return optimizationResults;
        }
    }
}
