using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMManagementTool.Services;

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

        const string SAVED_SESSION_FILE = "session.json";

        //public const string WIN_UPDATE_RESULTS_KEY = "winupdateresults";
        //public const string OSOT_RESULTS_KEY = "osotresults";
        //public const string CLEANUP_RESULTS_KEY = "cleanupresults";

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

        public void SetWinUpdateResults(Dictionary<string, WinUpdateStatus> results)
        {
            optimizationSession.WinUpdateResults = results;
        }
        public void SetOSOTResults(List<(string, bool)> results)
        {
            optimizationSession.OSOTResults = results;
        }
        public void SetCleanupResults(List<(string, bool, int)> results)
        {
            optimizationSession.CleanupResults = results;
        }

        public static bool SavedSessionAvailable()
        {
            return File.Exists(SAVED_SESSION_FILE);
        }
        public void LoadPausedSession()
        {
            if (optimizationSession != null)
            {
                throw new Exception("Can not load saved session because there is already an active session");
            }

            if (SavedSessionAvailable())
            {
                string serializedSessionJson = File.ReadAllText(SAVED_SESSION_FILE);
                optimizationSession = JsonConvert.DeserializeObject<VMMTOptimizationSession>(serializedSessionJson);

                //delte the file after loading 
                File.Delete(SAVED_SESSION_FILE);

            }
        }
        public void SaveSessionForResume()
        {
            if (optimizationSession == null)
            {
                throw new Exception("There is no active session to save");
            }
            string serializedSessionJson = JsonConvert.SerializeObject(optimizationSession);
            File.WriteAllText(SAVED_SESSION_FILE, serializedSessionJson);

            //setting this to null to make sure the session is not attmpted to be changed 
            //after serialization
            optimizationSession = null;
        }
    }
}
