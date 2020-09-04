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
        int fakeSteps = 10;
        List<(string, bool)> stepsResults = new List<(string, bool)>();
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
                //random status with 3% of fail possibility
                bool status = random.Next(0, 100) > 3;
                await Task.Delay(delay);
                var step = $"Something step {i} something";
                RunProgressChanged?.Invoke(i*100/fakeSteps, step );
                stepsResults.Add((step, status));
            }
           
            RunCompleted?.Invoke(true) ;
        }

        internal void Abort()
        {
            
            RunCompleted?.Invoke(false) ;
        }
        public List<(string, bool)> GetResults()
        {
            return stepsResults;
        }
        internal async Task CleanupAsync()
        {
            await Task.Delay(50);
            return;
        }

        internal void RunDefaultStepsAsync()
        {
            RunDefaultSteps();
        }

        internal void RunAllStepsAsync()
        {
            RunDefaultSteps();
        }

        internal void RunSelectedStepsAsync(HashSet<string> customStepsChoice)
        {
            RunDefaultSteps();
        }
    }
}
