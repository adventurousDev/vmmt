using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VMManagementTool.Services;
using WUApiLib;

namespace VMManagementTool.Test
{
    class DummyWinUpdateManager : IDisposable
    {
        public event Action<bool> CheckCompleted;
        public event Action<bool> DownloadCompleted;
        public event Action<bool, bool> InstallationCompleted;
        public event Action<int, string> ProgressChanged;
        int timeout = 2000;
        int stage = 0;
        CancellationTokenSource cts;

        Dictionary<string, WinUpdateStatus> updateResults = new Dictionary<string, WinUpdateStatus>();
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

            //dummy updates 
            for (int i = 0; i < 3; i++)
            {
                string title = $"A dummy update number {i}";

                WinUpdateStatus updateStatus = new WinUpdateStatus(title,
                    new List<string> { "KB" + (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds.ToString().Substring(0, 6) }
                    );
                updateResults.Add(title, updateStatus);
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

            //simualte download failure 
            string title = $"A dummy update number {2}";
            updateResults[title].Error = OperationResultCode.orcFailed.ToString();
            updateResults[title].IsInstalled = false;
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

            //dummy update install states 
            for (int i = 0; i < 3; i++)
            {
                if (i == 2)
                {
                    continue;
                }
                string title = $"A dummy update number {i}";

                updateResults[title].IsInstalled = true;
            }

            bool reboot = false;
            InstallationCompleted?.Invoke(true, reboot);

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
                    InstallationCompleted?.Invoke(false, false);
                    break;

            }
            cts?.Cancel();
            cts?.Dispose();
        }

        internal void CleanUp()
        {

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
        public Dictionary<string, WinUpdateStatus> GetResults()
        {
            //todo how do we comm. full state/ failure (vs per-update state below)?
            return updateResults;



        }
    }
}
