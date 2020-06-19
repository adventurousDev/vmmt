using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VMManagementTool.Services
{
    public static class FileUtils
    {
        public static async Task<bool> TryDownloadFile(string URL, string filePath, int? timeout = null)
        {
            WebClient webClient = null;
            CancellationTokenSource cts = null;
            try
            {


                webClient = new WebClient();
                webClient.Proxy = null;
                if (timeout != null)
                {
                    cts = new CancellationTokenSource((int)timeout);
                    cts.Token.Register(() => webClient.CancelAsync());
                }

                await webClient.DownloadFileTaskAsync(URL, filePath);
                return File.Exists(filePath);

            }
            catch (Exception ex)
            {
                Log.Error("FileManager.TryDownloadFile", ex.ToString());
                return false;
            }
            finally
            {
                webClient?.Dispose();
                cts?.Dispose();
            }
        }

        public static async Task TryDownloadAndUnzip(string zipURL, HashSet<string> extractFiles, string extractIntoDir, int? downloadTimeout)
        {
            //download into temp
            var tempZipFile = Path.GetTempFileName();
            var downloaded = await TryDownloadFile(zipURL, tempZipFile, downloadTimeout);
            if (downloaded)
            {
                using (ZipArchive archive = ZipFile.OpenRead(tempZipFile))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (extractFiles.Contains(entry.Name))
                        {
                            string destinationPath = Path.Combine(extractIntoDir, entry.Name);
                            File.Delete(destinationPath);
                            entry.ExtractToFile(destinationPath);
                        }
                    }
                }
            }
            try
            {
                File.Delete(tempZipFile);
            }
            catch { }
        }
    }
}
