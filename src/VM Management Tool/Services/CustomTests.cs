using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services
{
    class CustomTests
    {
        //events 
        public event Action<string> NewInfo;
        void Log(string msg)
        {
            NewInfo?.Invoke(msg);
        }

        internal void ReadRegValue(string keyPath, string keyVal)
        {
            try
            {
                string key = @"HKEY_CURRENT_USER\SOFTWARE\0HaykTest";

                string intData = "100";
                string binaryData = "232a";
                string stringdata = "some string";

                var integer =(int) Registry.GetValue(key, "integer","whatever");
                var binary =(byte[]) Registry.GetValue(key, "binary","whatever");
                var string_ = Registry.GetValue(key, "string","whatever");

                bool intcheck = integer.ToString() == intData;
                bool bincheck = binaryData.ToLower() == BitConverter.ToString(binary).Replace("-", "").ToLower();
                bool stringcheck = stringdata == string_;

                Log("OK");
                ////string keyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Active Setup\Installed Components";
                //string key = RegistryUtils.NormalizeHive(keyPath);
                //if (key == null)
                //{
                //    Log($"Invalid registry key {keyPath}");
                //}
                //{
                //    var val = Registry.GetValue(key, keyVal, "Key Not available");
                //    if (val == null)
                //    {
                //        Log($"Key {keyPath} does not exist!");
                //    }
                //    else
                //    {
                //        Log($"Key {keyPath}:{keyVal}  value is: {val}");
                //    }
                //}


            }
            catch (Exception e)
            {
                Log(e.ToString());

            }
        }


        internal void CreateRegValue()
        {
            try
            {
                string key = @"HKEY_CURRENT_USER\SOFTWARE\0HaykTest";
                //string valueName = "StringAsNum";
                string intData = "100";
                string binaryData = "232a";
                string stringdata = "some string";
                Registry.SetValue(key, "intData", intData);
                Registry.SetValue(key, "binaryData", binaryData);
                Registry.SetValue(key, "stringdata", stringdata);

                //valueName = "SomethingDefaultStr";
                //Registry.SetValue(key, valueName, "hello");

                //valueName = "SomethingBinary";
                //Registry.SetValue(key, valueName, 1, RegistryValueKind.Binary);
                //valueName = "SomethingDword";
                //Registry.SetValue(key, valueName, 32, RegistryValueKind.DWord);


                //valueName = "SomethingSZ";
                //Registry.SetValue(key, valueName, 32, RegistryValueKind.DWord);

                //valueName = "SomethingSZ_M";
                //Registry.SetValue(key, valueName, 32, RegistryValueKind.DWord);

            }
            catch (Exception e)
            {

                Log(e.ToString());
            }

        }
    }
}
