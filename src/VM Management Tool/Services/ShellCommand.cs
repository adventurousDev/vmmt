using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services
{
    class ShellCommand
    {
        public string Command { get; set; }
        public string Arguments { get; set; }

        public ShellCommand(string args)
        {
            //Command = cmd;
            Command = "cmd.exe";
            Arguments = "/c " + args;
        }
        public ShellCommand(string args, string cmd)
        {
            Command = cmd;
            Arguments = args;
        }


        public bool TryExecute(out string output, int timeout = 3000)
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
                    else if (proc.ExitCode != 0)
                    {
                        //var err = proc.StandardError.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {

                return false;

            }
            return false;

        }
    }
}
