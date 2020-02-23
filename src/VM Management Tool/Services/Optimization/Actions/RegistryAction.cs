using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services.Optimization.Actions
{

    class RegistryAction : Action_
    {

        public const string PARAM_NAME_KEY = "keyName";
        public const string PARAM_NAME_VALUE = "valueName";
        public const string PARAM_NAME_TYPE = "type";
        public const string PARAM_NAME_DATA = "data";
        public const string PARAM_NAME_FILENAME = "fileName";
        public enum RegistryCommand
        {
            Add,
            DeleteKey,
            DeleteValue,
            Load,
            Unload

        }

        public RegistryCommand Command { get; private set; }

        public RegistryAction(RegistryCommand command, Dictionary<string, string> params_) : base(params_)
        {
            Command = command;

        }
        public override StatusResult CheckStatus()
        {
            //no need to check load and unload commands
            //these have to always run anyway            
            if (Command == RegistryCommand.Load || Command == RegistryCommand.Unload)
            {
                return StatusResult.Unavailable;
            }

            //The question this method is answering is: Would the state of the value(type, value) after execution be 
            //the same as it is now.
            string keyName = RegistryUtils.NormalizeHive(Params[PARAM_NAME_KEY]);
            if (Command == RegistryCommand.Add)
            {
                //ADD command ---------------------------------------------------------------

                //if there is only key defined
                //-- check that key exists 
                //if the value AND data is defined(no type is will be treated as SZ)
                //-- then check that the key has the value with that type and that data value
                //-- and no type is effectively the same as REG_SZ


                using (RegistryKey theKey = RegistryUtils.GetRegistryKey(keyName, RegistryView.Registry64))
                {

                    if (theKey == null)
                    {

                        return StatusResult.Mismatch;


                    }



                    if (Params.TryGetValue(PARAM_NAME_VALUE, out string valueName)
                          && Params.TryGetValue(PARAM_NAME_DATA, out string data))
                    {
                        //there is value and data available 

                        //see if type is also provided, otherwise it is effectively an SZ 
                        Params.TryGetValue(PARAM_NAME_TYPE, out string type);
                        if (type == null)
                        {
                            type = "REG_SZ";
                        }
                        var valueKind = RegistryUtils.String2RegistryValueKind(type);

                        //now fetch the actual value and type and compare with
                        // the template parameters
                        var valueDataCurr = theKey.GetValue(valueName);
                        if (valueDataCurr != null)
                        {
                            //this value exists 
                            //now check the type
                            var valueKindCurr = theKey.GetValueKind(valueName);
                            if (valueKindCurr == valueKind)
                            {
                                //finally check the actual value
                                string valueStrCurr = RegistryUtils.RegValue2String(valueDataCurr, valueKindCurr);
                                if (data.ToLower().Equals(valueStrCurr.ToLower()))
                                {
                                    return StatusResult.Match;
                                }
                            }

                        }


                    }
                    else
                    {
                        //at this point the key has been found 
                        //but no other params are there to check 
                        //so it is a match
                        return StatusResult.Match;
                    }
                }
            }
            else
            {
                using (RegistryKey theKey = RegistryUtils.GetRegistryKey(keyName, RegistryView.Registry64))//todo get the view dynamically
                {
                    if (theKey == null)
                    {
                        //if the key does not exist then there is no need to delete it (or its values)
                        return StatusResult.Match;
                    }
                    else if (Command == RegistryCommand.DeleteValue)
                    {
                        //check if the value exists
                        Params.TryGetValue(PARAM_NAME_VALUE, out string valueName);
                        var valueDataCurr = theKey.GetValue(valueName);
                        if (valueDataCurr == null)
                        {
                            //the specified value does not currently exist > match
                            return StatusResult.Match;
                        }
                        else
                        {
                            //the specified value does currently exist > mismatch
                            return StatusResult.Mismatch;
                        }
                    }
                    else
                    {
                        //the key exists and the commnad is DeleteKey so it is a mismatch
                        return StatusResult.Mismatch;
                    }



                }
            }


            //data+"!" makes sure that out dummy deafult will never accidentially 
            //equal to the template value
            //var value = Registry.GetValue(keyName, valueName, data + "!");




            return StatusResult.Mismatch;


        }
        bool ExecuteAdd()
        {
            try
            {
                string keyName = RegistryUtils.NormalizeHive(Params[PARAM_NAME_KEY]);
                //create the key, or open it if already there
                using (RegistryKey theKey = RegistryUtils.CreateOrOpenRegistryKey(keyName, RegistryView.Registry64))//todo get the view dynamically
                {
                    //check if value name and data is available 
                    if (Params.TryGetValue(PARAM_NAME_VALUE, out string valueName)
                         && Params.TryGetValue(PARAM_NAME_DATA, out string data))
                    {
                        Params.TryGetValue(PARAM_NAME_TYPE, out string type);
                        var valueKind = RegistryUtils.String2RegistryValueKind(type);
                        if (RegistryUtils.TryParseRegValueData(data, valueKind, out object dataVal))
                        {
                            theKey.SetValue(valueName, dataVal, valueKind);
                        }
                        else
                        {
                            throw new Exception($"Wrong data for registry value of type: {type}");
                        }

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        bool ExecuteDeleteKey()
        {
            try
            {
                string keyName = RegistryUtils.NormalizeHive(Params[PARAM_NAME_KEY]);
                using (var baseKey = RegistryUtils.GetBaseRegistryKey(keyName, RegistryView.Registry64))
                {
                    var keyRelativePath = keyName.Substring(keyName.IndexOf('\\')+1);
                    //todo should we care if the value does not exist in the first place?
                    baseKey.DeleteSubKeyTree(keyRelativePath, false);
                    return true;
                    
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }
        bool ExecuteDeleteValue()
        {
            try
            {
                string keyName = RegistryUtils.NormalizeHive(Params[PARAM_NAME_KEY]);
                //create the key, or open it if already there
                using (RegistryKey theKey = RegistryUtils.CreateOrOpenRegistryKey(keyName, RegistryView.Registry64))//todo get the view dynamically
                {
                    //check if value name and data is available 
                    if (Params.TryGetValue(PARAM_NAME_VALUE, out string valueName))
                    {
                        theKey.DeleteValue(valueName, false);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }
        bool ExecuteLoad()
        {
            //Had to switch to using cmd, becasue jsut reg would not work for load 
            //no matter what. Was getting an error: 
            //ERROR: The system was unable to find the specified registry key or value.
            //string cmd = "cmd.exe"; this is now default for ShellCommand
            var keyName = Params[PARAM_NAME_KEY];
            var fileName = Params[PARAM_NAME_FILENAME];
            string args = $"reg load \"{keyName}\" \"{fileName}\"";

            var shellCommand = new ShellCommand( args);

            if(shellCommand.TryExecute(out string output))
            {
                if(output.TrimEnd() == "The operation completed successfully.")
                {
                    return true;
                }
            }
            return false;

        }
        bool ExecuteUnload()
        {
            //Had to switch to using cmd, becasue jsut reg would not work for load 
            //no matter what. Was getting an error: 
            //ERROR: The system was unable to find the specified registry key or value.
            //string cmd = "cmd.exe";
            var keyName = Params[PARAM_NAME_KEY];
            string args = $"reg unload \"{keyName}\"";

            var shellCommand = new ShellCommand(args);

            if (shellCommand.TryExecute(out string output))
            {
                if (output.TrimEnd() == "The operation completed successfully.")
                {
                    return true;
                }
            }
            return false;

        }
        public override bool Execute()
        {
            switch (Command)
            {
                case RegistryCommand.Add:
                    return ExecuteAdd();
                case RegistryCommand.DeleteKey:
                    return ExecuteDeleteKey();
                case RegistryCommand.DeleteValue:
                    return ExecuteDeleteValue();
                case RegistryCommand.Load:
                    return ExecuteLoad();
                case RegistryCommand.Unload:
                    return ExecuteUnload();
            }

            return false;
        }
    }
}
