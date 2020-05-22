using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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

        Dictionary<string, WinUpdateStatus> updateResults = new Dictionary<string, WinUpdateStatus>();

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

        //todo remove, or refactor to return results and w/ exception handling
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
            try
            {
                AbortChecking();
                AbortDownload();
                AbortInstall();
            }
            catch (Exception ex)
            {

                Log.Error("WinUpdatesManager::AbortAll", ex.ToString());

            }

        }

        //according to https://docs.microsoft.com/en-us/windows/win32/wua_sdk/guidelines-for-asynchronous-wua-operations,
        //because we use the same object as a callback, that "has" the job objecs,
        //we need to call cleanup to avoid circular references and leaks
        public void CleanUp()
        {
            try
            {
                searchJob_?.CleanUp();
                downloadJob_?.CleanUp();
                installationJob_?.CleanUp();
            }
            catch (Exception ex)
            {

                Log.Error("WinUpdatesManager::CleanUp", ex.ToString());

            }
        }
        void AbortChecking()
        {
            Info("Requesting abort...");
            searchJob_?.RequestAbort();

        }
        void AbortDownload()
        {
            Info("Requesting abort downlaod...");
            downloadJob_?.RequestAbort();

        }
        void AbortInstall()
        {

            installationJob_?.RequestAbort();
        }

        public async void CheckForUpdates(bool online = true)
        {

            try
            {
                Info("Checking for updates...");
                updateSession = new UpdateSession();

                updateSearcher = updateSession.CreateUpdateSearcher();
                updateSearcher.Online = online;


                Info("Update searcher params are: " + Dump(updateSearcher));



                searchJob_ = updateSearcher.BeginSearch("IsInstalled=0", this, null);
            }
            catch (Exception ex)
            {

                Log.Error("WinUpdatesManager::CheckForUpdates", ex.ToString());
                await Task.Delay(250);
                CheckCompleted?.Invoke(false);
            }

        }

        public async void DownloadUpdates()
        {
            try
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
                    ReadyToInstall?.Invoke();//todo remove this for production
                    DownloadCompleted?.Invoke(true);

                }
            }
            catch (Exception ex)
            {

                Log.Error("WinUpdatesManager::DownloadUpdates", ex.ToString());
                //this will allow the caller UI method to finish and 
                //the event will be handled like expected asyncronously
                await Task.Delay(250);
                DownloadCompleted?.Invoke(false);
            }


        }

        public async void InstallUpdates()
        {
            try
            {
                updateInstaller = updateSession.CreateUpdateInstaller();

                updateInstaller.Updates = updateCollection;
                Info("Starting update installation: " + Dump(updateInstaller));
                //var result = updateInstaller.RunWizard("Fucking hell!!!");
                //OnInstallationComplete(result);
                installationJob_ = updateInstaller.BeginInstall(this, this, null);
            }
            catch (Exception ex)
            {
                Log.Error("WinUpdatesManager::InstallUpdates", ex.ToString());

                //this will allow the caller UI method to finish and 
                //the event will be handled like expected asyncronously
                await Task.Delay(250);
                InstallationCompleted?.Invoke(false);

            }
        }

        //------ WUA callbacks ----------------------------------------------

        //Search Complete callback
        //for now this will be our callback 
        //This needs some testing because can be problematic accorting to: 
        //https://docs.microsoft.com/en-us/windows/win32/wua_sdk/guidelines-for-asynchronous-wua-operations
        void ISearchCompletedCallback.Invoke(ISearchJob searchJob, ISearchCompletedCallbackArgs callbackArgs)
        {
            try
            {
                var searchResult = updateSearcher.EndSearch(searchJob);

                if (searchResult.ResultCode != OperationResultCode.orcSucceeded)
                {
                    Info($"Update search failed with code: {searchResult.ResultCode}");
                    CheckCompleted?.Invoke(false);
                    return;
                }
                //todo could updates partially fail: the result above not success, but still some  updates available? 
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


                    foreach (IUpdate update in updateCollection)
                    {
                        List<string> KBs = new List<string>();
                        foreach (string KB in update.KBArticleIDs)
                        {
                            KBs.Add(KB);
                        }

                        WinUpdateStatus updateStatus = new WinUpdateStatus(update.Title, KBs);
                        updateResults.Add(update.Title, updateStatus);
                    }

                    UpdatesFound?.Invoke();
                    CheckCompleted?.Invoke(true);

                }
                else
                {
                    CheckCompleted?.Invoke(false);
                }
            }
            catch (Exception ex)
            {
                CheckCompleted?.Invoke(false);
                Log.Error("ISearchCompletedCallback.Invoke", ex.ToString());

            }
        }


        //Downlaod Complete callback
        void IDownloadCompletedCallback.Invoke(IDownloadJob downloadJob, IDownloadCompletedCallbackArgs callbackArgs)
        {
            try
            {
                var downloadResult = updateDownloader.EndDownload(downloadJob);

                //todo could downloads partially fail: the result above not success, but still some  updates available for install? 
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
                    var resultCode = downloadResult.GetUpdateResult(i).ResultCode;
                    var title = downloadJob.Updates[i].Title;
                    if (resultCode != OperationResultCode.orcSucceeded)
                    {
                        //the title must be there
                        //but if it it not for some reason we will detect this (via dict. exception)
                        updateResults[title].Error = resultCode.ToString();
                        updateResults[title].IsInstalled = false;
                    }


                    Info($"Download status for update {title}: {resultCode}");

                }
            }
            catch (Exception ex)
            {
                DownloadCompleted?.Invoke(false);

                Log.Error("IDownloadCompletedCallback.Invoke", ex.ToString());

            }
        }
        //Download Progress callback
        void IDownloadProgressChangedCallback.Invoke(IDownloadJob downloadJob, IDownloadProgressChangedCallbackArgs callbackArgs)
        {
            try
            {
                Info($"Download progress: {callbackArgs.Progress.PercentComplete}%; " +
                        $"update {downloadJob.Updates[callbackArgs.Progress.CurrentUpdateIndex].Title}: {callbackArgs.Progress.CurrentUpdatePercentComplete}%");
                Info(Dump(callbackArgs.Progress));
                ProgressChanged?.Invoke(callbackArgs.Progress.PercentComplete, downloadJob.Updates[callbackArgs.Progress.CurrentUpdateIndex].Title);
            }
            catch (Exception ex)
            {

                Log.Error("IDownloadProgressChangedCallback.Invoke", ex.ToString());

            }

        }

        //Installation Complete callback
        void IInstallationCompletedCallback.Invoke(IInstallationJob installationJob, IInstallationCompletedCallbackArgs callbackArgs)
        {
            try
            {
                var installResult = updateInstaller.EndInstall(installationJob);

                OnInstallationComplete(installResult);
            }
            catch (Exception ex)
            {

                InstallationCompleted?.Invoke(false);
                Log.Error("IInstallationCompletedCallback.Invoke", ex.ToString());

            }
        }

        private void OnInstallationComplete(IInstallationResult installResult)
        {
            //todo can there be partial success, and we miss some results?
            if (installResult.ResultCode != OperationResultCode.orcSucceeded)
            {
                Info($"Installation failed with code: {installResult.ResultCode}");
                InstallationCompleted?.Invoke(false);
                return;
            }



            for (int i = 0; i < updateInstaller.Updates.Count; i++)
            {
                var resultCode = installResult.GetUpdateResult(i).ResultCode;
                var title = updateInstaller.Updates[i].Title;
                if (resultCode != OperationResultCode.orcSucceeded)
                {
                    //the title must be there
                    //but if it is not for some reason we will detect this (via dict. exception)
                    updateResults[title].Error = resultCode.ToString();
                    updateResults[title].IsInstalled = false;
                }
                else
                {
                    updateResults[title].IsInstalled = true;
                }

                Info($"Installation status for update {title}: {resultCode}");
            }
            Info($"Is reboot required? : {installResult.RebootRequired}");


            InstallationCompleted?.Invoke(true);

        }

        //Installation Progress callback
        void IInstallationProgressChangedCallback.Invoke(IInstallationJob installationJob, IInstallationProgressChangedCallbackArgs callbackArgs)
        {
            try
            {
                Info($"Install progress: {callbackArgs.Progress.PercentComplete}%; " +
                       $"Update {installationJob.Updates[callbackArgs.Progress.CurrentUpdateIndex].Title}: {callbackArgs.Progress.CurrentUpdatePercentComplete}%");
                //Info(Dump(callbackArgs.Progress));
                ProgressChanged?.Invoke(callbackArgs.Progress.PercentComplete, installationJob.Updates[callbackArgs.Progress.CurrentUpdateIndex].Title);
            }
            catch (Exception ex)
            {
                Log.Error("IInstallationProgressChangedCallback.Invoke", ex.ToString());
            }
        }

        //------- Log Utils -------------------------------------------------------------------

        void Info(string text)
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

        public object GetResults()
        {
            //todo how do we comm. full state/ failure (vs per-update state below)?
            return updateResults;

        }
    }

}
