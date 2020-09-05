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
    public class SessionManager
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
        public enum SessionState
        {
            None,
            Paused,
            Active
        }
        public SessionState CurrentState { get; set; }
        //public const string WIN_UPDATE_RESULTS_KEY = "winupdateresults";
        //public const string OSOT_RESULTS_KEY = "osotresults";
        //public const string CLEANUP_RESULTS_KEY = "cleanupresults";
        public event Action<SessionState> SessionStateChanged;
        public void StartOptimizationSession(OptimizationSession optimizationSession)
        {
            this.optimizationSession = optimizationSession;
            ChangeSessionState(SessionState.Active);
        }
        public void ResumeOptimizationSession()
        {
            if (optimizationSession == null)
            {
                throw new Exception("No sesson loaded to resume");
            }
            ChangeSessionState(SessionState.Active);
        }
        void ChangeSessionState(SessionState state)
        {
            CurrentState = state;
            SessionStateChanged?.Invoke(state);
        }
        public OptimizationSession FinishCurrentSession()
        {
            var tmpRef = optimizationSession;
            optimizationSession = null;
            ChangeSessionState(SessionState.None);
            return tmpRef;
        }

        public void SetWinUpdateResults(Dictionary<string, WinUpdateStatus> results, bool completed = true)
        {
            optimizationSession.WindowsUpdateSessionState.Results = results;
            optimizationSession.WindowsUpdateSessionState.IsCompleted = completed;
        }
        public void SetWinUpdateAborted()
        {
            optimizationSession.WindowsUpdateSessionState.IsAborted = true;
        }
        public void SetWinUpdateCompleted()
        {
            optimizationSession.WindowsUpdateSessionState.IsCompleted = true;
        }

        public void SetOSOTResults(List<(string, bool)> results, bool completed = true)
        {
            optimizationSession.OSOTSessionState.Results = results;
            optimizationSession.OSOTSessionState.IsCompleted = completed;
        }
        public void SetOSOTAborted()
        {
            optimizationSession.OSOTSessionState.IsAborted = true;

        }
        public void SetCleanupResults(List<(string, bool, int)> results, bool completed = true)
        {
            optimizationSession.CleanupSessionState.Results = results;
            optimizationSession.CleanupSessionState.IsCompleted = completed;
        }
        public void SetCleanupAborted()
        {
            optimizationSession.CleanupSessionState.IsAborted = true;
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
                ChangeSessionState(SessionState.Paused);
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
            //if the session is aborted move it to report page
            if (optimizationSession.IsAborted)
            {
                return new ReportPage();
            }

            if (optimizationSession.WindowsUpdateSessionState != null && !optimizationSession.WindowsUpdateSessionState.IsCompleted
                && !optimizationSession.WindowsUpdateSessionState.IsAborted)
            {
                return new RunWinUpdatesPage();
            }

            if (optimizationSession.OSOTSessionState != null && !optimizationSession.OSOTSessionState.IsCompleted
                && !optimizationSession.OSOTSessionState.IsAborted)
            {
                return new RunOSOTTempaltePage();
            }

            if (optimizationSession.CleanupSessionState != null && !optimizationSession.CleanupSessionState.IsCompleted
                && !optimizationSession.CleanupSessionState.IsAborted)
            {
                return new RunCleanupOptimizationsPage();
            }

            return new ReportPage();
        }
        public List<SessionStage> GetSessionStages()
        {
            var stages = new List<SessionStage>();
            if (optimizationSession == null)
            {
                return null;
            }
            bool activeset = false;
            if (optimizationSession.WindowsUpdateSessionState != null)
            {
                //&& optimizationSession.WindowsUpdateSessionState.Results == null
                var stage = new SessionStage();
                stage.Title = "Windows Updates";
                if (optimizationSession.WindowsUpdateSessionState.IsAborted || optimizationSession.IsAborted)
                {
                    stage.State = SessionStage.States.Aborted;
                }
                else if (optimizationSession.WindowsUpdateSessionState.IsCompleted )
                {
                    stage.State = SessionStage.States.Processed;
                }
                else if (!activeset)
                {
                    stage.State = SessionStage.States.Active;
                    activeset = true;
                }
                else
                {
                    stage.State = SessionStage.States.Scheduled;
                }
                stages.Add(stage);
            }

            if (optimizationSession.OSOTSessionState != null)
            {
                var stage = new SessionStage();
                stage.Title = "OSOT Optimizations";
                if (optimizationSession.OSOTSessionState.IsAborted || optimizationSession.IsAborted)
                {
                    stage.State = SessionStage.States.Aborted;
                }
                else if (optimizationSession.OSOTSessionState.IsCompleted)
                {
                    stage.State = SessionStage.States.Processed;
                }
                else if (!activeset)
                {
                    stage.State = SessionStage.States.Active;
                    activeset = true;
                }
                else
                {
                    stage.State = SessionStage.States.Scheduled;
                }
                stages.Add(stage);
            }

            if (optimizationSession.CleanupSessionState != null)
            {
                var stage = new SessionStage();
                stage.Title = "Cleanup";
                if (optimizationSession.CleanupSessionState.IsAborted || optimizationSession.IsAborted)
                {
                    stage.State = SessionStage.States.Aborted;
                }
                else if (optimizationSession.CleanupSessionState.IsCompleted)
                {
                    stage.State = SessionStage.States.Processed;
                }
                else if (!activeset)
                {
                    stage.State = SessionStage.States.Active;
                    activeset = true;
                }
                else
                {
                    stage.State = SessionStage.States.Scheduled;
                }
                stages.Add(stage);
            }

            //this means all steps are processed/ aborted
            //so the active page is Report

            var rstage = new SessionStage();
            rstage.Title = "Report";
            rstage.State = activeset ? SessionStage.States.Scheduled : SessionStage.States.Active;
            stages.Add(rstage);


            return stages;
        }
        public void SetAllAborted()
        {
            optimizationSession.IsAborted = true;
        }
    }
}
