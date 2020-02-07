using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services
{
    static class RegistryUtils
    {
        public static string NormalizeHive(string regKey)
        {
            if (string.IsNullOrEmpty(regKey))
            {
                throw new ArgumentException("Registry key can not be null or empty string");
            }
            string[] parts = regKey.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            string properHive = TranslateHive(parts[0]);
            if (properHive == null)
            {
                return null;
            }
            else
            {
                parts[0] = properHive;
                return string.Join("\\", parts);
            }

        }
        private static string TranslateHive(string hiveStr)
        {
            string res = null;
            switch (hiveStr.ToUpper())
            {
                case "HKCR":
                case "HKEY_CLASSES_ROOT":
                    res = "HKEY_CLASSES_ROOT";
                    break;
                case "HKCC":
                case "HKEY_CURRENT_CONFIG":
                    res = "HKEY_CURRENT_CONFIG";
                    break;
                case "HKU":
                case "HKEY_USERS":
                    res = "HKEY_USERS";
                    break;
                case "HKCU":
                case "HKEY_CURRENT_USER":
                    res = "HKEY_CURRENT_USER";
                    break;
                case "HKLM":
                case "HKEY_LOCAL_MACHINE":
                    res = "HKEY_LOCAL_MACHINE";
                    break;


            }
            return res;
        }
        public static RegistryHive GetRegistryHive(string regKey)
        {
            string hiveStr = regKey.Substring(0, regKey.IndexOf('\\'));
            RegistryHive registryHive;
            switch (hiveStr.ToUpper())
            {
                case "HKCR":
                case "HKEY_CLASSES_ROOT":
                    registryHive = RegistryHive.ClassesRoot;
                    break;
                case "HKCC":
                case "HKEY_CURRENT_CONFIG":
                    registryHive = RegistryHive.CurrentConfig;
                    break;
                case "HKU":
                case "HKEY_USERS":
                    registryHive = RegistryHive.Users;
                    break;
                case "HKCU":
                case "HKEY_CURRENT_USER":
                    registryHive = RegistryHive.CurrentUser;
                    break;
                case "HKLM":
                case "HKEY_LOCAL_MACHINE":
                    registryHive = RegistryHive.LocalMachine;
                    break;
                default:
                    throw new Exception($"Wrong registry key: unknown root ({hiveStr})");
            }
            return registryHive;
        }

        public static RegistryValueKind String2RegistryValueKind(string kindString)
        {
            var ret = RegistryValueKind.None;
            switch (kindString.ToUpper())
            {
                case "REG_SZ":
                    ret = RegistryValueKind.String;
                    break;
                case "REG_EXPAND_SZ":
                    ret = RegistryValueKind.ExpandString;
                    break;

                case "REG_BINARY":
                    ret = RegistryValueKind.Binary;
                    break;
                case "REG_DWORD":
                    ret = RegistryValueKind.DWord;
                    break;
                case "REG_MULTI_SZ":
                    ret = RegistryValueKind.MultiString;
                    break;
                case "REG_QWORD":
                    ret = RegistryValueKind.QWord;
                    break;

            }
            return ret;
        }

        public static RegistryKey GetRegistryKey(string keyAbsPath, RegistryView registryView = RegistryView.Default)
        {
            using (var hive = RegistryKey.OpenBaseKey(GetRegistryHive(keyAbsPath), registryView))
            {
                int subPathStart = keyAbsPath.IndexOf('\\');
                if (subPathStart == 0)
                {
                    string inHivePath = keyAbsPath.Substring(subPathStart).TrimEnd('\\');
                    var resSubKey = hive.OpenSubKey(inHivePath);
                    return resSubKey;

                }
                else
                {
                    //if the key is jsut the hive we have to return it
                    //opening it again because using is gonna dispose the used one
                    //todo test this
                    return RegistryKey.OpenBaseKey(GetRegistryHive(keyAbsPath), registryView);
                }



            }
        }

        public static string RegValue2String(object value, RegistryValueKind kind)
        {
            switch (kind)
            {
                case RegistryValueKind.Binary:
                    //assert it is byte[]
                    if (value is byte[] bytes)
                    {
                        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
                    }
                    else
                    {
                        throw new Exception("Unexpected value of binary registry key");
                    }
                   
                    //for now handling everyting else with ToString()
                default:
                    return value.ToString();
                   
            }
        }
    }
}
