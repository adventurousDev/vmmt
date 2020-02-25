using Newtonsoft.Json;
using System;
using System.Text;
using WUApiLib;

namespace VMManagementTool.Services
{
    class WinUpdatesManager : ISearchCompletedCallback, IDownloadProgressChangedCallback, IDownloadCompletedCallback, IInstallationCompletedCallback, IInstallationProgressChangedCallback
    {
        public const string WUA_SERVICE_NAME = "wuauserv";

        private static readonly object instancelock = new object();
        private static WinUpdatesManager instance = null;

        //todo does this really need to be a singleton?
        public static WinUpdatesManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new WinUpdatesManager();
                        }
                    }
                }
                return instance;
            }
        }

        IUpdateSession3 updateSession;

        ISearchJob searchJob_;
        IUpdateSearcher updateSearcher;

        IDownloadJob downloadJob_;
        IUpdateDownloader updateDownloader;

        IInstallationJob installationJob_;
        IUpdateInstaller updateInstaller;

        UpdateCollection updateCollection;



        public WinUpdatesManager()
        {

        }

        public event Action<string> NewInfo;
        public event Action UpdatesFound;
        public event Action ReadyToInstall;

        public event Action<bool> CheckCompleted;
        public event Action<bool> DownloadCompleted;
        public event Action<bool> InstallationCompleted;
        public event Action<int, string> ProgressChanged;


        public void LoadHsitory()
        {
            Info("Loading update history...");

            //todo should the session object be unique?
            var session = new UpdateSession();
            //todo should the searcher object be unique(used for hsitory and checking for new)?
            var searcher = session.CreateUpdateSearcher();
            searcher.Online = false;

            var hist = searcher.QueryHistory(0, int.MaxValue);//session.QueryHistory("", 0, int.MaxValue);
            Info($"There are {hist.Count} update history entries: ");
            foreach (var item in hist)
            {
                Info(Dump(item));
            }
        }
        public void AbortAll()
        {
            AbortChecking();
            AbortDownload();
            AbortInstall();

        }
        //according to https://docs.microsoft.com/en-us/windows/win32/wua_sdk/guidelines-for-asynchronous-wua-operations,
        //because we use the same object as a callback, that "has" the job objecs,
        //we need to call cleanup to avoid circular references and leaks
        public void CleanUp()
        {
            searchJob_?.CleanUp();
            downloadJob_?.CleanUp();
            installationJob_?.CleanUp();
        }
        internal void AbortChecking()
        {
            Info("Requesting abort...");
            searchJob_?.RequestAbort();

        }
        internal void AbortDownload()
        {
            Info("Requesting abort downlaod...");
            downloadJob_?.RequestAbort();

        }
        internal void AbortInstall()
        {

            installationJob_?.RequestAbort();
        }

        public void CheckForUpdates(bool online = true)
        {

            Info("Checking for updates...");
            updateSession = new UpdateSession();

            updateSearcher = updateSession.CreateUpdateSearcher();
            updateSearcher.Online = online;


            Info("Update searcher params are: " + Dump(updateSearcher));



            searchJob_ = updateSearcher.BeginSearch("IsInstalled=0", this, null);

        }

        public void DownloadUpdates()
        {
            UpdateCollection toDwnload = new UpdateCollection();
            foreach (IUpdate update in updateCollection)
            {
                if (!update.IsDownloaded && !update.IsInstalled)
                {
                    toDwnload.Add(update);
                }
            }

            if (toDwnload.Count > 0)
            {
                //updateSession = new UpdateSession();
                //todo what if I:
                //1. crete  new session?
                //2. create the Downlaoder with new keywoard
                updateDownloader = updateSession.CreateUpdateDownloader();

                updateDownloader.Updates = updateCollection;
                Info("Update downloader params are: " + Dump(updateDownloader));

                downloadJob_ = updateDownloader.BeginDownload(this, this, null);
            }
            else if (updateCollection.Count > 0)
            {
                ReadyToInstall?.Invoke();
            }


        }

        public void InstallUpdates()
        {
            updateInstaller = updateSession.CreateUpdateInstaller();

            updateInstaller.Updates = updateCollection;
            Info("Starting update installation: " + Dump(updateInstaller));
            //var result = updateInstaller.RunWizard("Fucking hell!!!");
            //OnInstallationComplete(result);
            installationJob_ = updateInstaller.BeginInstall(this, this, null);
        }

        //------ WUA callbacks ----------------------------------------------

        //Search Complete callback
        //for now this will be our callback 
        //This needs some testing because can be problematic accorting to: 
        //https://docs.microsoft.com/en-us/windows/win32/wua_sdk/guidelines-for-asynchronous-wua-operations
        void ISearchCompletedCallback.Invoke(ISearchJob searchJob, ISearchCompletedCallbackArgs callbackArgs)
        {
            var searchResult = updateSearcher.EndSearch(searchJob);

            if (searchResult.ResultCode != OperationResultCode.orcSucceeded)
            {
                Info($"Update search failed with code: {searchResult.ResultCode}");
                CheckCompleted?.Invoke(false);
                return;
            }

            Info($"Found {searchResult.Updates.Count} updates:" + Environment.NewLine);

            foreach (IUpdate update in searchResult.Updates)
            {
                Info(Dump(update));
            }
            /*
            Info($"There are {searchResult.RootCategories.Count} cateories:" + Environment.NewLine);

            foreach (ICategory category in searchResult.RootCategories)
            {
                Info(Dump(category));
            }
            */
            if (searchResult.Updates.Count > 0)
            {
                updateCollection = searchResult.Updates;
                UpdatesFound?.Invoke();
                CheckCompleted?.Invoke(true);

            }
            else
            {
                CheckCompleted?.Invoke(false);
            }
        }


        //Downlaod Complete callback
        void IDownloadCompletedCallback.Invoke(IDownloadJob downloadJob, IDownloadCompletedCallbackArgs callbackArgs)
        {
            var downloadResult = updateDownloader.EndDownload(downloadJob);


            if (downloadResult.ResultCode != OperationResultCode.orcSucceeded)
            {
                Info($"Download failed with code: {downloadResult.ResultCode}");
                //return;
                DownloadCompleted?.Invoke(false);
            }
            else
            {
                ReadyToInstall?.Invoke();
                DownloadCompleted?.Invoke(true);

            }

            for (int i = 0; i < downloadJob.Updates.Count; i++)
            {
                Info($"Download status for update {downloadJob.Updates[i].Title}: {downloadResult.GetUpdateResult(i).ResultCode}");
            }
        }
        //Download Progress callback
        void IDownloadProgressChangedCallback.Invoke(IDownloadJob downloadJob, IDownloadProgressChangedCallbackArgs callbackArgs)
        {
            Info($"Download progress: {callbackArgs.Progress.PercentComplete}%; " +
                $"update {downloadJob.Updates[callbackArgs.Progress.CurrentUpdateIndex].Title}: {callbackArgs.Progress.CurrentUpdatePercentComplete}%");
            Info(Dump(callbackArgs.Progress));
            ProgressChanged?.Invoke(callbackArgs.Progress.PercentComplete, downloadJob.Updates[callbackArgs.Progress.CurrentUpdateIndex].Title);

        }
        
        //Installation Complete callback
        void IInstallationCompletedCallback.Invoke(IInstallationJob installationJob, IInstallationCompletedCallbackArgs callbackArgs)
        {
            var installResult = updateInstaller.EndInstall(installationJob);

            OnInstallationComplete(installResult);
        }

        private void OnInstallationComplete(IInstallationResult installResult)
        {
            if (installResult.ResultCode != OperationResultCode.orcSucceeded)
            {
                Info($"Installation failed with code: {installResult.ResultCode}");
                InstallationCompleted?.Invoke(false);
                return;
            }

            InstallationCompleted?.Invoke(true);

            for (int i = 0; i < updateInstaller.Updates.Count; i++)
            {
                Info($"Installation status for update {updateInstaller.Updates[i].Title}: {installResult.GetUpdateResult(i).ResultCode}");
            }
            Info($"Is reboot required? : {installResult.RebootRequired}");

        }

        //Installation Progress callback
        void IInstallationProgressChangedCallback.Invoke(IInstallationJob installationJob, IInstallationProgressChangedCallbackArgs callbackArgs)
        {
            Info($"Install progress: {callbackArgs.Progress.PercentComplete}%; " +
               $"Update {installationJob.Updates[callbackArgs.Progress.CurrentUpdateIndex].Title}: {callbackArgs.Progress.CurrentUpdatePercentComplete}%");
            //Info(Dump(callbackArgs.Progress));
            ProgressChanged?.Invoke(callbackArgs.Progress.PercentComplete, installationJob.Updates[callbackArgs.Progress.CurrentUpdateIndex].Title);
        }

        //------- Log Utils -------------------------------------------------------------------

        private void Info(string text)
        {
            NewInfo?.Invoke(text);
        }
        string Dump(object obj, int depth = 1)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("{");
            if (obj is IUpdateHistoryEntry updateHistoryEntry)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("ClientApplicationID", updateHistoryEntry.ClientApplicationID, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Date", updateHistoryEntry.Date, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Description", updateHistoryEntry.Description, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("HResult", updateHistoryEntry.HResult, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("ServerSelection", updateHistoryEntry.ServerSelection, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("ServiceID", updateHistoryEntry.ServiceID, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("SupportUrl", updateHistoryEntry.SupportUrl, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Title", updateHistoryEntry.Title, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("UninstallationNotes", updateHistoryEntry.UninstallationNotes, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("UninstallationSteps", updateHistoryEntry.UninstallationSteps, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("UnmappedResultCode", updateHistoryEntry.UnmappedResultCode, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("UpdateIdentity", updateHistoryEntry.UpdateIdentity, depth, false));




            }
            else if (obj is IUpdateIdentity updateIdentity)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("UnmappedResultCode", updateIdentity.RevisionNumber, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("UpdateIdentity", updateIdentity.UpdateID, depth, false));

            }
            else if (obj is IStringCollection stringCollection)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("string collection of", stringCollection.Count, depth, false));
            }
            else if (obj is IUpdateSearcher updateSearcher)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("IncludePotentiallySupersededUpdates", updateSearcher.IncludePotentiallySupersededUpdates, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Online", updateSearcher.Online, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("ServerSelection", updateSearcher.ServerSelection, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("ServiceID", updateSearcher.ServiceID, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("CanAutomaticallyUpgradeService", updateSearcher.CanAutomaticallyUpgradeService, depth, false));
            }
            else if (obj is IUpdateDownloader updateDownloader)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("IsForced", updateDownloader.IsForced, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Priority", updateDownloader.Priority, depth, false));
            }
            else if (obj is IUpdate update)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("Title", update.Title, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("RebootBehavior", update.InstallationBehavior.RebootBehavior, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("IsDownloaded", update.IsDownloaded, depth, false));
                stringBuilder.AppendLine(GetJsonKeyValPair("IsInstalled", update.IsInstalled, depth, false));
                stringBuilder.AppendLine(GetJsonKeyValPair("Category", update.Categories[0].Name, depth, false));
            }
            else if (obj is ICategory category)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("Name", category.Name, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("CategoryID", category.CategoryID, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Description", category.Description, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Type", category.Type, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Parent category", category.Parent?.Name, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Child count", category.Children.Count, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("Updates coount", category.Updates.Count, depth, false));

            }
            else if (obj is IDownloadProgress downloadProgress)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("CurrentUpdateBytesDownloaded", downloadProgress.CurrentUpdateBytesDownloaded, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("CurrentUpdateBytesToDownload", downloadProgress.CurrentUpdateBytesToDownload, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("CurrentUpdateDownloadPhase", downloadProgress.CurrentUpdateDownloadPhase, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("CurrentUpdateIndex", downloadProgress.CurrentUpdateIndex, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("CurrentUpdatePercentComplete", downloadProgress.CurrentUpdatePercentComplete, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("PercentComplete", downloadProgress.PercentComplete, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("TotalBytesDownloaded", downloadProgress.TotalBytesDownloaded, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("TotalBytesToDownload", downloadProgress.TotalBytesToDownload, depth));

            }
            else if (obj is IUpdateInstaller updateInstaller)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("AllowSourcePrompts", updateInstaller.AllowSourcePrompts, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("IsBusy", updateInstaller.IsBusy, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("IsForced", updateInstaller.IsForced, depth));
                stringBuilder.AppendLine(GetJsonKeyValPair("RebootRequiredBeforeInstallation", updateInstaller.RebootRequiredBeforeInstallation, depth, false));
            }
            else
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("object", obj.ToString(), depth));
            }
            stringBuilder.Append("}".PadLeft(depth * 4, ' '));

            return stringBuilder.ToString();
        }
        string GetJsonKeyValPair(string name, object value, int depth, bool comma = true)
        {
            if (value?.ToString() == "System.__ComObject")
            {
                value = Dump(value, depth + 1);
            }
            else
            {
                value = $"\"{value?.ToString().Replace("\"", "'")}\"";
            }
            var res = $"\"{name}\":{value}{(comma ? "," : "")}";

            return res.PadLeft(res.Length + depth * 4); ;
        }

    }

}
