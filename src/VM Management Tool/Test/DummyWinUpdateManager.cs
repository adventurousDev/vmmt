using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VMManagementTool.Test
{
    class DummyWinUpdateManager
    {
        public event Action<bool> CheckCompleted;
        public event Action<bool> DownloadCompleted;
        public event Action<bool> InstallationCompleted;
        public event Action<int, string> ProgressChanged;
        int timeout = 5000;
        int stage = 0;
        CancellationTokenSource cts;

        async Task SimualteProgress(string action)
        {


            DateTime utcNow = DateTime.UtcNow;
            int showprogress = 0;
            cts = new CancellationTokenSource();

            while (true)
            {
                long elapsed = ((DateTime.UtcNow.Ticks - utcNow.Ticks) / 10000);
                if (elapsed > timeout)
                {
                    ProgressChanged?.Invoke(100, "");
                    await Task.Delay(500).ConfigureAwait(false);
                    cts?.Dispose();
                    cts = null;
                    return;
                }

                int actualprogress = (int)((elapsed / (float)timeout) * 100);
                if (actualprogress - showprogress >= 10)
                {
                    showprogress = actualprogress;
                    string msg = "";
                    if (action.Length != 0)
                    {
                        msg = $"{action} Something {showprogress} something update bla bla bla and somethign to make it very long ";
                    }
                    ProgressChanged?.Invoke(showprogress, msg);
                }

                await Task.Delay(200, cts.Token).ConfigureAwait(false);

            }


        }

        internal async void CheckForUpdates()
        {
            stage = 1;


            try
            {
                await SimualteProgress("");
            }
            catch (Exception)
            {

                return;
            }
            CheckCompleted?.Invoke(true);
        }

        internal async void DownloadUpdates()
        {
            stage = 2;

            try
            {
                await SimualteProgress("Downloading:");

            }
            catch (Exception)
            {

                return;
            }
            DownloadCompleted?.Invoke(true);

        }

        internal async void InstallUpdates()
        {
            stage = 3;

            try
            {
                await SimualteProgress("Installing:");

            }
            catch (Exception)
            {

                return;
            }
            InstallationCompleted?.Invoke(true);

        }

        internal async void AbortAll()
        {
            //simulate long running abort
            await Task.Delay(10000);
            switch (stage)
            {
                case 1:
                    CheckCompleted?.Invoke(false);
                    break;
                case 2:
                    DownloadCompleted?.Invoke(false);
                    break;
                case 3:
                    InstallationCompleted?.Invoke(false);
                    break;

            }
            cts?.Cancel();
            cts?.Dispose();
        }

        internal void CleanUp()
        {

        }
    }
}
