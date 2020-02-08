using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM_Management_Tool.Services.Optimization;
using VM_Management_Tool.Services.Optimization.Actions;

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

        public void ReadRegValue(string keyPath, string keyVal)
        {
            try
            {
                string key = @"HKEY_CURRENT_USER\SOFTWARE\0HaykTest";

                string intData = "100";
                string binaryData = "232a";
                string stringdata = "some string";

                var integer = (int)Registry.GetValue(key, "integer", "whatever");
                var binary = (byte[])Registry.GetValue(key, "binary", "whatever");
                var string_ = Registry.GetValue(key, "string", "whatever");

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


        public void CreateRegValue()
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

        public void TestRegistryAction()
        {
            //just key test
            var justkeyparams = new Dictionary<string, string>();
            justkeyparams.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest0");
            var action = new RegistryAction(RegistryAction.RegistryCommand.Add, justkeyparams);

            //binary value test
            var bintestparamas = new Dictionary<string, string>();
            bintestparamas.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest");
            bintestparamas.Add(RegistryAction.PARAM_NAME_VALUE, "binarytest");
            bintestparamas.Add(RegistryAction.PARAM_NAME_TYPE, "REG_DWORD");
            bintestparamas.Add(RegistryAction.PARAM_NAME_DATA, "2324250A");

            var action2 = new RegistryAction(RegistryAction.RegistryCommand.Add, bintestparamas);

            //int value test
            var inttestparamas = new Dictionary<string, string>();
            inttestparamas.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest");
            inttestparamas.Add(RegistryAction.PARAM_NAME_VALUE, "inttest");
            inttestparamas.Add(RegistryAction.PARAM_NAME_TYPE, "REG_DWORD");
            inttestparamas.Add(RegistryAction.PARAM_NAME_DATA, "9992");

            var action3 = new RegistryAction(RegistryAction.RegistryCommand.Add, inttestparamas);

            //string value test
            var stringtestparamas = new Dictionary<string, string>();
            stringtestparamas.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest");
            stringtestparamas.Add(RegistryAction.PARAM_NAME_VALUE, "stringtest");
            //stringtestparamas.Add(RegistryAction.PARAM_NAME_TYPE, "REG_SZ");
            stringtestparamas.Add(RegistryAction.PARAM_NAME_DATA, "test1");

            var action4 = new RegistryAction(RegistryAction.RegistryCommand.Add, stringtestparamas);

            //delete existing key
            var delkeytest = new Dictionary<string, string>();
            delkeytest.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest");
            var action5 = new RegistryAction(RegistryAction.RegistryCommand.DeleteKey, delkeytest);

            //delete non-existent key
            var delkeytest2 = new Dictionary<string, string>();
            delkeytest2.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\nonexistentkey");
            var action6 = new RegistryAction(RegistryAction.RegistryCommand.DeleteKey, delkeytest2);

            //delete existing value
            var delvaltest1 = new Dictionary<string, string>();
            delvaltest1.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest");
            delvaltest1.Add(RegistryAction.PARAM_NAME_VALUE, "inttest");
            var action7 = new RegistryAction(RegistryAction.RegistryCommand.DeleteValue, delvaltest1);

            //delete non-existent value
            var delvaltest2 = new Dictionary<string, string>();
            delvaltest2.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest");
            delvaltest2.Add(RegistryAction.PARAM_NAME_VALUE, "blabla");
            var action8 = new RegistryAction(RegistryAction.RegistryCommand.DeleteValue, delvaltest2);

            Log("key test: " + Enum.GetName(typeof(Action_.StatusResult), action.CheckStatus()));
            Log("binary val test: " + Enum.GetName(typeof(Action_.StatusResult), action2.CheckStatus()));
            Log("int val test: " + Enum.GetName(typeof(Action_.StatusResult), action3.CheckStatus()));
            Log("string val test: " + Enum.GetName(typeof(Action_.StatusResult), action4.CheckStatus()));

            Log("delete key that exists: " + Enum.GetName(typeof(Action_.StatusResult), action5.CheckStatus()));
            Log("delete key that doesn't exist: " + Enum.GetName(typeof(Action_.StatusResult), action6.CheckStatus()));
            Log("delete value that exists: " + Enum.GetName(typeof(Action_.StatusResult), action7.CheckStatus()));
            Log("delete value that doesn't exist: " + Enum.GetName(typeof(Action_.StatusResult), action8.CheckStatus()));



        }
    }
}
