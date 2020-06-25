using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services
{
    class ShellCommand
    {
        public string Command { get; set; }
        public string Arguments { get; set; }

        public ShellCommand(string args) : this("/c " + args, Path.Combine(SystemUtils.GetSystem32Path(true), "cmd.exe"))
        {
            //Command = cmd;
            //had to switch to full path w/ sysnative
            //because it should always call the version of cmd.exe with the same architecture 
            //as the OS
            //for isntance there was a problem with DISM returning 11, 
            //when running it from 32 bit app on 64 bit Windows which is realted to that
           
        }
        public ShellCommand(string args, string cmd)
        {
            Command = cmd;
            if (WindowsIdentity.GetCurrent().IsSystem)
            {
                args = TranslateSystemPaths(args);
            }
            Arguments = args;
        }

        string TranslateSystemPaths(string data)
        {
            //Sometimes %USERPROFILE%\.. env var is used to find the C:\Users folder. This 
            //does not work for SYSTEM because its profile is in system32, but the PUBLIC seems to be 
            //constant for all users. This is hacky, but so is user relying on %USERPROFILE%, and the change
            //should happen there.
            return data.Replace("%USERPROFILE%", "%PUBLIC%");
        }

        public bool TryExecute(out string output, int timeout = 30000)
        {
            output = null;
            try
            {
                using (var proc = new Process())
                {
                    proc.StartInfo.FileName = Command;
                    proc.StartInfo.Arguments = Arguments;

                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;

                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.StartInfo.RedirectStandardError = true;

                    proc.Start();

                    if (proc.WaitForExit(timeout)
                        && proc.ExitCode == 0
                        )
                    {
                        output = proc.StandardOutput.ReadToEnd();

                        return true;
                    }
                    //var err = proc.StandardError.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Log.Error("ShellCommand.TryExecute", e.Message);
                return false;

            }
            return false;

        }
    }
}
