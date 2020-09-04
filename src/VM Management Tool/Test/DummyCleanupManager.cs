using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMManagementTool.Services;

namespace VMManagementTool.Test
{
    class DummyCleanupManager
    {
        public const string TOOL_NAME_CLEANMGR = "Disk Cleanup";
        public const string TOOL_NAME_SDELETE = "SDelete";
        public const string TOOL_NAME_DEFRAG = "Defrag";
        public const string TOOL_NAME_DISM = "Dism";

        public event Action<bool> CleanmgrCompleted;
        public event Action<bool> DefragCompleted;
        public event Action<bool> SdeleteCompleted;
        int stage = 0;//1 clean, 2 sdelete, 3 defrag
        int fakeProgressDuration = 1000;//4 seconds

        List<(string, bool, int)> results = new List<(string, bool, int)>();

        public Action<string, bool> ToolCompleted { get; internal set; }

        internal void Abort()
        {
            switch (stage)
            {
                case 1:
                    results.Add((TOOL_NAME_CLEANMGR, false, -1));
                    ToolCompleted?.Invoke(TOOL_NAME_CLEANMGR, false);
                    stage = 0;
                    break;
                case 2:
                    results.Add((TOOL_NAME_SDELETE, false, -1));
                    ToolCompleted?.Invoke(TOOL_NAME_SDELETE, false);
                    stage = 0;
                    break;
                case 3:
                    results.Add((TOOL_NAME_DEFRAG, false, -1));
                    ToolCompleted?.Invoke(TOOL_NAME_DEFRAG, false);
                    stage = 0;
                    break;

            }
        }

        internal async void StartCleanmgr()
        {

            stage = 1;
            await Task.Delay(fakeProgressDuration);
            //consider abortion
            if (stage == 1)
            {
                results.Add((TOOL_NAME_CLEANMGR, true, 0));
                ToolCompleted?.Invoke(TOOL_NAME_CLEANMGR, true);
            }

        }

        internal async void StartSdelete()
        {
            stage = 2;
            await Task.Delay(fakeProgressDuration);

            //consider abortion
            if (stage == 2)
            {
                results.Add((TOOL_NAME_SDELETE, true, 0));
                ToolCompleted?.Invoke(TOOL_NAME_SDELETE, true);

            }
        }

        internal async void StartDefrag()
        {
            stage = 3;
            await Task.Delay(fakeProgressDuration);

            //consider abortion
            if (stage == 3)
            {
                results.Add((TOOL_NAME_DEFRAG, true, 0));
                ToolCompleted?.Invoke(TOOL_NAME_DEFRAG, true);

            }
            stage = 0;

        }
        internal async void StartDism()
        {
            stage = 4;
            await Task.Delay(fakeProgressDuration);

            //consider abortion
            if (stage == 4)
            {
                results.Add((TOOL_NAME_DISM, true, 0));
                ToolCompleted?.Invoke(TOOL_NAME_DISM, true);

            }
           

        }
        public List<(string, bool, int)> GetResults()
        {
            return results;
        }

        internal void StartTool(string next2Run)
        {
            switch (next2Run)
            {
                case TOOL_NAME_CLEANMGR:
                    StartCleanmgr();
                    break;
                case TOOL_NAME_DISM:
                    StartDism();
                    break;
                case TOOL_NAME_SDELETE:
                    StartSdelete();
                    break;
                case TOOL_NAME_DEFRAG:
                    StartDefrag();
                    break;


            }
        }

        internal bool HasCompleted(string tool)
        {
            return results.Exists((r) => r.Item1.Equals(tool));
        }
    }
}
