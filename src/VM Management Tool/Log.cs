using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool
{
    public static class Log
    {
        public const int LOGLEVEL_ERRROR = 0;
        public const int LOGLEVEL_INFO = 1;
        public const int LOGLEVEL_DEBUG = 2;
        static object locker = new object();
        public static int LogLevel { get; set; } = 0;
        const string LOG_FILE = "log.log";
        public static void Error(string tag, string message)
        {

            WriteToFile($"ERROR:    {tag}    {message}");
        }
        public static void Info(string tag, string message)
        {
            if (LogLevel < LOGLEVEL_INFO)
            {
                return;
            }
            WriteToFile($"INFO:    {tag}    {message}");
        }

        public static void Debug(string tag, string message)
        {
            if (LogLevel < LOGLEVEL_DEBUG)
            {
                return;
            }
            WriteToFile($"DEBUG:    {tag}    {message}");
        }
        static void WriteToFile(string msg)
        {
            var time = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss.fff");
            
            lock (locker)
            {                
                File.AppendAllText(LOG_FILE, $"{time} {msg} {Environment.NewLine}");
            }

        }

    }
}
