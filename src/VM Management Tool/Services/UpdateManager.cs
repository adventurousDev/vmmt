using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VMManagementTool.Services
{
    class UpdateManager
    {
        const string EXECUTABLE_URL = "https://ucad59399745e55e3bb85d27d191.dl.dropboxusercontent.com/cd/0/get/AyEz7knh8pUCdTODPS7T04SWUhVCwBbWisHv7GCX9SjiaMLqcLBsJy07qE_RNKKgw6KE6OE2EBAm4HWvERCC_xp_jYvT7y44V5jFizyUgBdBavlAG10UJ6yKC9yjzEKKEZI/file?_download_id=9568158614107032264264608847330493120900970250636047659176779832&_notify_domain=www.dropbox.com&dl=1";
        string tempPath;

        public void Update()
        {
            try
            {
                //1. download the executable into tmp
                tempPath = Path.GetTempFileName();
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(EXECUTABLE_URL), tempPath);
                }
            }
            catch (Exception ex)
            {

                Log.Error("UpdateManager.Update", ex.Message);

            }


        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            UpdateAndRestart();
        }
        void UpdateAndRestart()
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


    }
}
