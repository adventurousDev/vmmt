using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VMManagementTool.Services;
using VMManagementTool.UI;

namespace VMManagementTool.Session
{
    class SessionManager
    {

        private static readonly object instancelock = new object();
        private static SessionManager instance = null;

        private OptimizationSession optimizationSession;

        public static SessionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new SessionManager();
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

        public void StartOptimizationSession(OptimizationSession optimizationSession)
        {
            this.optimizationSession = optimizationSession;
        }
        public OptimizationSession FinishCurrentSession()
        {
            var tmpRef = optimizationSession;
            optimizationSession = null;
            return tmpRef;
        }

        public void SetWinUpdateResults(Dictionary<string, WinUpdateStatus> results)
        {
            optimizationSession.WindowsUpdateSessionState.Results = results;
        }
        public void SetOSOTResults(List<(string, bool)> results)
        {
            optimizationSession.OSOTSessionState.Results = results;
        }
        public void SetCleanupResults(List<(string, bool, int)> results)
        {
            optimizationSession.CleanupSessionState.Results = results;
        }
        public WindowsUpdateSessionState GetWinUpdateParams()
        {
            return optimizationSession.WindowsUpdateSessionState;
        }
        public OSOTSessionState GetOSOTParams()
        {
            return optimizationSession.OSOTSessionState;
        }
        public CleanupSessionState GetCleanupParams()
        {
            return optimizationSession.CleanupSessionState;
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
                optimizationSession = JsonConvert.DeserializeObject<OptimizationSession>(serializedSessionJson);

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

        //todo rethink this 
        //it feels wrong to create the UI elements in this "model" class
        //this will also get problematic if pages have parameters that depend on caller situation
        public Page GetNextSessionPage()
        {
            if (optimizationSession == null)
            {
                return null;
            }
            
            if (optimizationSession.WindowsUpdateSessionState != null && optimizationSession.WindowsUpdateSessionState.Results == null)
            {
                return new RunWinUpdatesPage();
            }

            if (optimizationSession.OSOTSessionState != null && optimizationSession.OSOTSessionState.Results == null)
            {
                return new RunOSOTTempaltePage();
            }

            if (optimizationSession.CleanupSessionState != null && optimizationSession.CleanupSessionState.Results == null)
            {
                return new RunCleanupOptimizationsPage();
            }

            return new ReportPage();
        }
    }
}
