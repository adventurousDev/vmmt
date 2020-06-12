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
    public class UpdateManager
    {
        const string UPDATE_DATA_VERSION_KEY = "version";
        const string UPDATE_DATA_URL_KEY = "URL";

        volatile bool aborted = false;
        string tempPath;
        public string NewVersion { get { return updateData["version"].ToString(); } }
        public string CurrentVersion { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(3); } }
        private Dictionary<string, object> updateData;
        WebClient webClient;
        string manifestURL;

        public UpdateManager(string manifestURL)
        {
            this.manifestURL = manifestURL;
        }




        public async Task<bool> CheckForUpdates()
        {
            try
            {
                if (updateData == null)
                {
                    string updateManifestJSON;
                    using (var client = new HttpClient())
                    {
                        //1. downlaod the json manifest
                        var resp = await client.GetAsync(manifestURL);
                        updateManifestJSON = await resp.Content.ReadAsStringAsync();
                    }

                    //2. parse it and get the remote version                
                    updateData = JsonConvert.DeserializeObject<Dictionary<string, object>>(updateManifestJSON);
                }

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
                var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                await ExtractAllAync(tempZipFile, tempDir);

                try
                {
                    File.Delete(tempZipFile);
                }
                catch { }

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
            await Task.Run(() => ZipFile.ExtractToDirectory(zipFile, dir));
        }
        public void Abort()
        {
            aborted = true;
            webClient?.CancelAsync();

        }


    }
}
