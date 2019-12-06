using Newtonsoft.Json;
using System;
using System.Text;
using WUApiLib;

namespace VM_Management_Tool.Services
{
    class WinUpdatesManager : ISearchCompletedCallback
    {
        private static readonly object Instancelock = new object();
        private static WinUpdatesManager instance = null;

        public static WinUpdatesManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (Instancelock)
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

        ISearchJob searchJob;
        IUpdateSearcher updateSearcher;
        private WinUpdatesManager()
        {

        }

        public event Action<string> NewInfo;
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

        public void CheckForUpdates()
        {
            Info("Checking for updates...");
            var session = new UpdateSession();
            updateSearcher = session.CreateUpdateSearcher();

            Info("Update searcher params are: " + Dump(updateSearcher));


            //todo, make this async
            searchJob = updateSearcher.BeginSearch("IsInstalled=0", this, null);

        }
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
            else if (obj is IUpdate update)
            {
                stringBuilder.AppendLine(GetJsonKeyValPair("Title", update.Title, depth, false));
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

        //for now this will be our callback 
        //This needs some testing because can be problematic accorting to: 
        //https://docs.microsoft.com/en-us/windows/win32/wua_sdk/guidelines-for-asynchronous-wua-operations
        void ISearchCompletedCallback.Invoke(ISearchJob searchJob, ISearchCompletedCallbackArgs callbackArgs)
        {
            var searchResult = updateSearcher.EndSearch(searchJob);

            Info($"Found {searchResult.Updates.Count} updates:" + Environment.NewLine);

            foreach (IUpdate update in searchResult.Updates)
            {
                Info(Dump(update));
            }
        }
    }

}
