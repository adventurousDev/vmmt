using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Test
{
    class DummyWinOptimizationsManger
    {
        public event Action<bool> CleanmgrCompleted;
        public event Action<bool> DefragCompleted;
        public event Action<bool> SdeleteCompleted;
        int stage = 0;//1 clean, 2 sdelete, 3 defrag
        int fakeProgressDuration = 4000;//4 seconds
        internal void Abort()
        {
            switch (stage)
            {
                case 1:
                    CleanmgrCompleted?.Invoke(false);
                    stage = 0;
                    break;
                case 2:
                    SdeleteCompleted?.Invoke(false);
                    stage = 0;
                    break;
                case 3:
                    DefragCompleted?.Invoke(false);
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
                CleanmgrCompleted?.Invoke(true);
            }

        }

        internal async void StartSdelete()
        {
            stage = 2;
            await Task.Delay(fakeProgressDuration);

            //consider abortion
            if (stage == 2)
            {
                SdeleteCompleted?.Invoke(true);
            }
        }

        internal async void StartDefrag()
        {
            stage = 3;
            await Task.Delay(fakeProgressDuration);

            //consider abortion
            if (stage == 3)
            {
                DefragCompleted?.Invoke(true);
            }
            stage = 0;

        }
    }
}
