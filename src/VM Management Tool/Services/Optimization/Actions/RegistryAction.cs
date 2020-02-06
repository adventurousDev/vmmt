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
            //ADD command ---------------------------------------------------------------

            //if there is only key defined
            //-- check that key exists 
            //if the value AND type AND data is defined
            //-- then check that the key has the value with that type and that data value

            string keyName = Params[PARAM_NAME_KEY];
            //check if key exists 
            //* first need to get the root and match it with hive

            //get the root/ hive

            string hive = keyName.Substring(0, keyName.IndexOf('\\'));
            switch (hive)
            {
                case "HKCR":
                case "HKEY_CLASSES_ROOT":
                    //deal with this 
                    break;
                case "HKCC":
                case "HKEY_CURRENT_CONFIG":
                    //deal with this 
                    break;
                case "HKU":
                case "HKEY_USERS":
                    //deal with this 
                    break;
                case "HKCU":
                case "HKEY_CURRENT_USER":
                    //deal with this 
                    break;
                case "HKLM":
                case "HKEY_LOCAL_MACHINE":
                    //deal with this 
                    break;
                default:
                    throw new Exception($"Wrong registry key: uncknown root ({hive})");
            }

            //if yes continue to the value check
            if (Params.ContainsKey(PARAM_NAME_VALUE)
                && Params.ContainsKey(PARAM_NAME_TYPE)
                && Params.ContainsKey(PARAM_NAME_DATA))
            {

            }

            return StatusResult.Unavailable;


        }
    }
}
