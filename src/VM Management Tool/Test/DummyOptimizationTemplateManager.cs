using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Test
{
    class DummyOptimizationTemplateManager
    {
        public event Action<int, string> RunProgressChanged;
        public event Action<bool> RunCompleted;
        int fakeSteps = 200;

        internal async Task LoadAsync(string templatePath)
        {
            await Task.Delay(500);
        }

        internal async void RunDefaultSteps()
        {
            Random random = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < fakeSteps; i++)
            {
                
                //random delay: in 30% probablility of being longer(300ms)
                int delay = random.Next(1, 11) < 4 ? 300 : 150;
                await Task.Delay(delay);

                RunProgressChanged?.Invoke(i*100/fakeSteps, $"Something step {i} something");
            }
            RunCompleted?.Invoke(true) ;
        }

        internal void Abort()
        {
            RunCompleted?.Invoke(false) ;
        }

        internal async Task CleanupAsync()
        {
            return;
        }
    }
}
