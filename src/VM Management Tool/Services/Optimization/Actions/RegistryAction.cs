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

            //The question this method is answering is:Would the state of the value(type, value) after execution be the
            //same as it is now.

            //ADD command ---------------------------------------------------------------

            //if there is only key defined
            //-- check that key exists 
            //if the value AND type AND data is defined
            //-- then check that the key has the value with that type and that data value
            //-- and no type is effectively the same as REG_SZ

            string keyName = RegistryUtils.NormalizeHive(Params[PARAM_NAME_KEY]);
            using (RegistryKey theKey = RegistryUtils.GetRegistryKey(keyName))
            {
                
                if (theKey == null)
                {
                    return StatusResult.Mismatch;
                }

                if (Params.TryGetValue(PARAM_NAME_VALUE, out string valueName)
                      && Params.TryGetValue(PARAM_NAME_DATA, out string data))
                {
                    //there is value available 

                    //get the value and its type(kind)


                    Params.TryGetValue(PARAM_NAME_TYPE, out string type);
                    if (type == null)
                    {
                        type = "REG_SZ";
                    }
                    var valueKind = RegistryUtils.String2RegistryValueKind(type);

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
                            if (data.Equals(valueStrCurr))
                            {
                                return StatusResult.Match;
                            }
                        }

                    }


                }
                else
                {
                    //at tis point the key has been found 
                    //but no other params are there to check 
                    //so it is a match
                    return StatusResult.Match;
                } 
            }


            //data+"!" makes sure that out dummy deafult will never accidentially 
            //equal to the template value
            //var value = Registry.GetValue(keyName, valueName, data + "!");




            return StatusResult.Mismatch;


        }
    }
}
