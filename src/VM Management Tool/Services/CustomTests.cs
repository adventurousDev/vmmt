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

        public void CreateRegKey()
        {
            try
            {
                string key = @"SOFTWARE\0HaykTest";
                string key2 = @"hkey_local_machine\SOFTWARE\0HaykTest\another";

                //Registry.SetValue(key, "", "");
                //Registry.SetValue(key2, "", "");
                //Registry.SetValue(key2, "newValue", 66, RegistryValueKind.DWord);
                //Registry.SetValue(@"hkey_local_machine\tutu", "", "");

                using (RegistryKey hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    //try create with exising key
                    //

                    var exkey = hive.OpenSubKey(key, true);

                    exkey.SetValue("inttest", 666);

                    //var wroong = hive.OpenSubKey(key, false);

                    //wroong.SetValue("inttest", 5);

                }

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
        public void DeleteKey()
        {
            try
            {
                string key = @"SOFTWARE\1HaykTest";
                string key2 = @"SOFTWARE\0HaykTest\another";

                //Registry.SetValue(key, "", "");
                //Registry.SetValue(key2, "", "");
                //Registry.SetValue(key2, "newValue", 66, RegistryValueKind.DWord);
                //Registry.SetValue(@"hkey_local_machine\tutu", "", "");

                using (RegistryKey hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {


                    //hive.DeleteSubKeyTree(key,false);
                    var theKye = hive.OpenSubKey(key, true);//?.DeleteValue("newstringtest",false);

                    theKye.Close();



                }

            }
            catch (Exception e)
            {

                Log(e.ToString());
            }
        }

        public void TestRegistryAction()
        {
            /* 
            //just key test
            var justkeyparams = new Dictionary<string, string>();
            justkeyparams.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest\addNewKey");
            var action = new RegistryAction(RegistryAction.RegistryCommand.Add, justkeyparams);


            //binary value test
            var bintestparamas2 = new Dictionary<string, string>();
            bintestparamas2.Add(RegistryAction.PARAM_NAME_KEY, @"HKCU\SOFTWARE\0hayktest");
            bintestparamas2.Add(RegistryAction.PARAM_NAME_VALUE, "newbinarytest");
            bintestparamas2.Add(RegistryAction.PARAM_NAME_TYPE, "REG_BINARY");
            bintestparamas2.Add(RegistryAction.PARAM_NAME_DATA, "0300000064A102EF4C3ED101");

            var action1 = new RegistryAction(RegistryAction.RegistryCommand.Add, bintestparamas2);

            //binary value test
            var bintestparamas = new Dictionary<string, string>();
            bintestparamas.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest\subpath");
            bintestparamas.Add(RegistryAction.PARAM_NAME_VALUE, "newbinarytest");
            bintestparamas.Add(RegistryAction.PARAM_NAME_TYPE, "REG_BINARY");
            bintestparamas.Add(RegistryAction.PARAM_NAME_DATA, "2324250A");

            var action2 = new RegistryAction(RegistryAction.RegistryCommand.Add, bintestparamas);

            //int value test
            var inttestparamas = new Dictionary<string, string>();
            inttestparamas.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest");
            inttestparamas.Add(RegistryAction.PARAM_NAME_VALUE, "newinttest");
            inttestparamas.Add(RegistryAction.PARAM_NAME_TYPE, "REG_DWORD");
            inttestparamas.Add(RegistryAction.PARAM_NAME_DATA, "9992");

            var action3 = new RegistryAction(RegistryAction.RegistryCommand.Add, inttestparamas);

            //string value test
            var stringtestparamas = new Dictionary<string, string>();
            stringtestparamas.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest");
            stringtestparamas.Add(RegistryAction.PARAM_NAME_VALUE, "newstringtest");
            stringtestparamas.Add(RegistryAction.PARAM_NAME_TYPE, "REG_SZ");
            stringtestparamas.Add(RegistryAction.PARAM_NAME_DATA, "test1");

            var action4 = new RegistryAction(RegistryAction.RegistryCommand.Add, stringtestparamas);


            Log("Add key result: " + action.Execute());
            Log("Add binay 1 result: " + action1.Execute());
            Log("Add binay result: " + action2.Execute());
            Log("Add dword result: " + action3.Execute());
            Log("Add string result: " + action4.Execute());
            
            //delete existing key w/ children
            var delkeytest = new Dictionary<string, string>();
            delkeytest.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\1hayktest");
            var action5 = new RegistryAction(RegistryAction.RegistryCommand.DeleteKey, delkeytest);
           
            //delete existing key w/o children
            var delkeytestNoch = new Dictionary<string, string>();
            delkeytestNoch.Add(RegistryAction.PARAM_NAME_KEY, @"HKLM\SOFTWARE\0hayktest\more1");
            var action5Plus = new RegistryAction(RegistryAction.RegistryCommand.DeleteKey, delkeytestNoch);

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

            //Log("key test: " + Enum.GetName(typeof(Action_.StatusResult), action.CheckStatus()));
            //Log("binary val test: " + Enum.GetName(typeof(Action_.StatusResult), action2.CheckStatus()));
            //Log("int val test: " + Enum.GetName(typeof(Action_.StatusResult), action3.CheckStatus()));
            //Log("string val test: " + Enum.GetName(typeof(Action_.StatusResult), action4.CheckStatus()));

            Log("delete key that exists and has children: " + action5.Execute());
            Log("delete key that exists and has no children: " + action5Plus.Execute());
            Log("delete key that doesn't exist: " + action6.Execute());
            Log("delete value that exists: " + action7.Execute());
            Log("delete value that doesn't exist: " + action8.Execute());

           */
            //load/ unload tests

            var loadtest = new Dictionary<string, string>();
            loadtest.Add(RegistryAction.PARAM_NAME_KEY, @"HKU\temp");
            loadtest.Add(RegistryAction.PARAM_NAME_FILENAME, @"%USERPROFILE%\..\Default User\NTUSER.DAT");
            var action = new RegistryAction(RegistryAction.RegistryCommand.Load, loadtest);


            var unloadtest = new Dictionary<string, string>();
            unloadtest.Add(RegistryAction.PARAM_NAME_KEY, @"HKU\temp");
            var action2 = new RegistryAction(RegistryAction.RegistryCommand.Unload, unloadtest);

            Log("Unload when not yet loaded: " + action2.Execute());
            Log("Load when not yet loaded: " + action.Execute());
            Log("Load when already loaded: " + action.Execute());
            Log("Unload when loaded: " + action2.Execute());

        }

        internal void TestSchtaskAction()
        {
            var action = new SchTasksAction(new Dictionary<string, string>()
            {
                [SchTasksAction.PARAM_NAME_TASK_NAME] = @"\Microsoft\XblGameSave\XblGameSaveTask",
                [SchTasksAction.PARAM_NAME_STATUS] = "DISABLED"
            });
            var action2 = new SchTasksAction(new Dictionary<string, string>()
            {
                [SchTasksAction.PARAM_NAME_TASK_NAME] = @"\Microsoft\XblGameSave\XblGameSaveTaskLogon",
                [SchTasksAction.PARAM_NAME_STATUS] = "DISABLED"
            });
            var action3 = new SchTasksAction(new Dictionary<string, string>()
            {
                [SchTasksAction.PARAM_NAME_TASK_NAME] = @"\Microsoft\XblGameSave\XblGameSaveTaskLogon",
                [SchTasksAction.PARAM_NAME_STATUS] = "ENABLED"
            });

            var action4 = new SchTasksAction(new Dictionary<string, string>()
            {
                [SchTasksAction.PARAM_NAME_TASK_NAME] = @"\Microsoft\afjsak\XblGameSaveTaskLogon",
                [SchTasksAction.PARAM_NAME_STATUS] = "DISABLED"
            });

            //Log("exec d=d: " + Enum.GetName(typeof(Action_.StatusResult), action.CheckStatus()));
            //Log("check r=d: " + Enum.GetName(typeof(Action_.StatusResult), action2.CheckStatus()));
            //Log("check r=e: " + Enum.GetName(typeof(Action_.StatusResult), action3.CheckStatus()));
            //Log("check ?=d: " + Enum.GetName(typeof(Action_.StatusResult), action4.CheckStatus()));
            Log("exec d=d: " + action.Execute());
            Log("exec r=d: " + action2.Execute());
            Log("exec d=e: " + action3.Execute());
            Log("exec ?=d: " + action4.Execute());

        }

        public void TestShellAction()
        {
            var cmd = "netsh advfirewall set allprofiles state off";
            var action = new ShellExecuteAction(cmd);
            ;
            Log("Turn off firewall; result: " + action.Execute());
        }

        public void TestServiceStuff()
        {
            //var name = "gupdatem";
            //WinServiceUtils.SetStartupType(name,"manual");
            //WinServiceUtils.DisableService(name);
            //Log(WinServiceUtils.GetStartupType(name));

            //* disable an enabled service | gupdate
            //* enable a disabled service | gupdatem
            //* change a disabled service to automatic with auto | HomeGroupProvider
            //* try change a non-existent service state | blablaserv

        

            PrintTypeAndChangeTo("gupdate","DISABLED");
            PrintTypeAndChangeTo("gupdatem", "MANUAL");
            PrintTypeAndChangeTo("HomeGroupProvider", "AUTO");
            PrintTypeAndChangeTo("blablaserv", "DISABLED");


        }
        private void PrintTypeAndChangeTo(string name, string templateStartType)
        {
            var paramz = new Dictionary<String, string>()
            {
                [ServiceAction.PARAM_NAME_SERVICE_NAME] = name,
                [ServiceAction.PARAM_NAME_START_MODE] = templateStartType
            };
            var serviceAction = new ServiceAction(paramz);
            Log("service " + name +"; "+ Enum.GetName(typeof(Action_.StatusResult), serviceAction.CheckStatus())+" | type:  "+templateStartType);
            Log("service " + name+ "; execution result: " + serviceAction.Execute());
        }
    }
}
