using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services.Optimization.Actions
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
            //no need to check load and unload
            //these have to always run            
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
                                if (data.ToLower().Equals(valueStrCurr))
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
                using (RegistryKey theKey = RegistryUtils.GetRegistryKey(keyName, RegistryView.Registry64))
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
    }
}
