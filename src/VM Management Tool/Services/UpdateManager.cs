using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VMManagementTool.Services
{
    class UpdateManager : IDisposable
    {
        const string UPDATE_DATA_VERSION_KEY = "version";
        const string UPDATE_DATA_URL_KEY = "URL";

        volatile bool aborted = false;
        string tempPath;
        public string NewVersion { get { return updateData["version"].ToString(); } }
        public string CurrentVersion { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(3); } }
        private Dictionary<string, object> updateData;
        WebClient webClient;
        public void Update_()
        {
            try
            {
                //1. download the executable into tmp
                tempPath = Path.GetTempFileName();
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                    //webClient.DownloadFileAsync(new Uri(EXECUTABLE_URL), tempPath);
                }
            }
            catch (Exception ex)
            {

                Log.Error("UpdateManager.Update", ex.Message);

            }


        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //UpdateAndRestart();
        }
        void UpdateAndRestart_()
        {
            try
            {
                if (!File.Exists(tempPath))
                {
                    //todo handle this better
                    //log at least
                    return;
                }
                //2. if defined perform verifications *
                //3. get the path of current executable
                var exePath = Application.ResourceAssembly.Location;
                //4. rename the current executable
                if (File.Exists(exePath + "_temp"))
                {
                    File.Delete(exePath + "_temp");
                }
                File.Move(exePath, exePath + "_temp");
                //5. copy the new exe and rename to the origianl name of current one
                File.Move(tempPath, exePath);
                //6. initiate start of the new one 
                System.Diagnostics.Process.Start(exePath);//will this still have the old exe name? 
                                                          //7. close the current 
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {

                Log.Error("UpdateManager.WebClient_DownloadFileCompleted", ex.Message);
            }
        }

        public async Task<bool> IsNewerVersionAvailable()
        {
            try
            {
                string updateManifestJSON;
                using (var client = new HttpClient())
                {
                    //1. downlaod the json manifest
                    var resp = await client.GetAsync(Settings.UPDATE_MANIFEST_URL);
                    updateManifestJSON = await resp.Content.ReadAsStringAsync();
                }

                //2. parse it and get the remote version                
                updateData = JsonConvert.DeserializeObject<Dictionary<string, object>>(updateManifestJSON);

                var newVer = updateData[UPDATE_DATA_VERSION_KEY].ToString();
                Version newVersion = new Version(newVer);
                return newVersion > Assembly.GetExecutingAssembly().GetName().Version;


            }
            catch (Exception ex)
            {
                Log.Error("UpdateManager.IsNewerVersionAvailable", ex.Message);
                return false;
            }


        }

        public async Task UpdateAndRestartAsync()
        {
            try
            {
                //1. get, validate the url
                var zipUrl = updateData[UPDATE_DATA_URL_KEY].ToString();
                //2. download the zip as random temp file
                var tempZipFile = Path.GetTempFileName();
                webClient = new WebClient();
                try
                {
                    await webClient.DownloadFileTaskAsync(zipUrl, tempZipFile);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                if (aborted)
                {
                    return;
                }
                //3. unzip it into random temp dir
                var tempDir = Path.Combine(Path.GetTempPath(),Path.GetRandomFileName());
                await ExtractAllAync(tempZipFile, tempDir);
                //4. find the setup.exe and start it
                var setupExeFile = Path.Combine(tempDir, "setup.exe");
                if (!File.Exists(setupExeFile))
                {
                    throw new Exception("Could not retrieve setup.exe");
                }
                if (aborted)
                {
                    return;
                }
                System.Diagnostics.Process.Start(setupExeFile);

                Application.Current.Shutdown();
            }
            finally
            {
                webClient.Dispose();
                webClient = null;
            }
            
        }

        async Task ExtractAllAync(string zipFile, string dir)
        {
            await Task.Run(()=>ZipFile.ExtractToDirectory(zipFile, dir));
        }
        public void Abort()
        {
            aborted = true;
            webClient?.CancelAsync();
            
        }

        public void Dispose()
        {
            //nothing to do as the webclient is disposed after usage
        }
    }
}
