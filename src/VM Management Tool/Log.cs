using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool
{
    static class Log
    {
        const string LOG_FILE = "log.log";
        public static void Error(string tag, string message)
        {
            WriteToFile($"ERROR:    {tag}    {message}");
        }
        public static void Info(string tag, string message)
        {
            WriteToFile($"INFO:    {tag}    {message}");
        }
        static void WriteToFile(string msg)
        {
            var time = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss");
            File.AppendAllText(LOG_FILE, $"{time} {msg} {Environment.NewLine}");
        }

    }
}
