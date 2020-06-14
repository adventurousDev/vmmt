using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using VMManagementTool.Services;
using VMManagementTool.UI;

namespace VMManagementTool
{
    public class ConfigurationManager
    {
        private static readonly object instancelock = new object();
        private static ConfigurationManager instance = null;
        Dictionary<string, object> configuration;
        Dictionary<string, Dictionary<string, object>> userSettings;

        public static ConfigurationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new ConfigurationManager();
                        }
                    }
                }
                return instance;
            }
        }


        public string GetStringConfig(string key)
        {
            configuration.TryGetValue(key, out object ret);
            return ret?.ToString();
        }


        public const string CONFIG_KEY_OSOT_URL = "osotTemplateURL";
        public const string CONFIG_KEY_UPDATE_MANIFEST_URL = "updateManifestURL";
        public const string CONFIG_KEY_EXTERNAL_TOOLS = "externalTools";
        public const string CONFIG_KEY_EXTERNAL_TOOLS_NAME = "name";
        public const string CONFIG_KEY_EXTERNAL_TOOLS_IS_ZIPPED = "isZipped";
        public const string CONFIG_KEY_EXTERNAL_TOOLS_FORCE_UPDATE = "forceUpdate";
        public const string CONFIG_KEY_EXTERNAL_TOOLS_FILES = "files";
        public const string CONFIG_KEY_EXTERNAL_TOOLS_URL = "URL";

        public async Task Init(Action<string> updateProgressMsg = null)
        {
            Log.Debug("ConfigurationManager.Init", "start");
            updateProgressMsg?.Invoke("Updating cofigurations");
            //1. load configuration
            await LoadConfiguraion().ConfigureAwait(true);
            Log.Debug("ConfigurationManager.Init", "config done");
            //2. check for updates
            //--- force update
            updateProgressMsg?.Invoke("Checking for updates");
            var updateManager = new UpdateManager(GetStringConfig(ConfigurationManager.CONFIG_KEY_UPDATE_MANIFEST_URL));
            var avaialable = await updateManager.CheckForUpdates().ConfigureAwait(true);
            bool updateCancelled = true;
            if (avaialable)
            {
                updateCancelled = !(new UpdateWindow(updateManager).ShowDialog() ?? false);
            }

            if (!updateCancelled)
            {
                //this should not happen because if update is not cancelled 
                //then app should be restarted
                throw new Exception("This should never happen!");

            }
            Log.Debug("ConfigurationManager.Init", "update check done");
            updateProgressMsg?.Invoke("Updating optimization template");
            //3. check/ update OSOT tempalte
            var osotURL = GetStringConfig(CONFIG_KEY_OSOT_URL);
            if (osotURL != null)
            {
                var filePath = Path.Combine(Configs.CONFIGS_DIR, Configs.REMOTE_OPTIMIZATION_TEMPLATE_FILE_NAME);
                FileUtils.TryDownloadFile(osotURL, filePath, 5000);
            }
            Log.Debug("ConfigurationManager.Init", "osot xml done");
            //4. check/ download exteranl tools 
            //becasue of the extracting we run this in bg worker
            updateProgressMsg?.Invoke("Fetching components");
            await Task.Run(() => TryFetchExternalTools()).ConfigureAwait(true);


            updateProgressMsg?.Invoke("Loading user settings");
            await Task.Run(LoadUserSettings).ConfigureAwait(true);

            //set log level from settings
            Log.LogLevel = (int)GetUserSetting<long>("log", "level", 0);

            Log.Debug("ConfigurationManager.Init", "end");

        }
        //initialize the app skipping all the web operations
        public void InitLight()
        {
            //1.load default 
            var defaultConfig = LoadConfigFromXMLFile(Configs.DEFAULT_CONFIG_FILE_PATH);
            Dictionary<string, object> remoteConfig = new Dictionary<string, object>();
            try
            {
                //2.try dwonload the remote 
                //create the configs dir in case it does not exist yet
                Directory.CreateDirectory(Configs.CONFIGS_DIR);
                var filePath = Path.Combine(Configs.CONFIGS_DIR, Configs.REMOTE_CONFIG_FILE_NAME);

                
                //3.try load the remote(new or old)
                var remoteConfigFilePath = Path.Combine(Configs.CONFIGS_DIR, Configs.REMOTE_CONFIG_FILE_NAME);
                if (File.Exists(remoteConfigFilePath))
                {
                    remoteConfig = LoadConfigFromXMLFile(remoteConfigFilePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error("ConfigurationManager.LoadConfiguraion", ex.Message);
            }

            //4.merge deafult and remote
            //by adding anything from default missing in remote
            foreach (var entry in defaultConfig)
            {
                if (!remoteConfig.ContainsKey(entry.Key))
                {
                    remoteConfig.Add(entry.Key, entry.Value);
                }
            }

            configuration = remoteConfig;

            //await Task.Run(LoadUserSettings).ConfigureAwait(true);
            LoadUserSettings();

            //set log level from settings
            Log.LogLevel = (int)GetUserSetting<long>("log", "level", 0);
        }
        private async Task TryFetchExternalTools()
        {

            if (configuration.TryGetValue(CONFIG_KEY_EXTERNAL_TOOLS, out object toolsObj) && toolsObj is List<Dictionary<string, object>> toolConfigList)
            {
                foreach (var toolConfig in toolConfigList)
                {

                    var isZipped = FetchDictValue<string>(toolConfig, CONFIG_KEY_EXTERNAL_TOOLS_IS_ZIPPED, "false") == "true";
                    var forceUpdate = FetchDictValue<string>(toolConfig, CONFIG_KEY_EXTERNAL_TOOLS_FORCE_UPDATE, "false") == "true";
                    var files = FetchDictValue<string>(toolConfig, CONFIG_KEY_EXTERNAL_TOOLS_FILES, null);
                    var URL = FetchDictValue<string>(toolConfig, CONFIG_KEY_EXTERNAL_TOOLS_URL, null);
                    var name = FetchDictValue<string>(toolConfig, CONFIG_KEY_EXTERNAL_TOOLS_NAME, null); ;

                    //these are mandatory and we can not do anything without them
                    if (files == null || URL == null)
                    {
                        continue;
                    }

                    if (name == null)
                    {
                        name = Convert.ToBase64String(Encoding.UTF8.GetBytes(URL)).Replace("=", "");
                        if (name.Length > 100)
                        {
                            name = name.Substring(name.Length - 100);
                        }
                    }
                    await CheckFetchExternalTool(name, files, URL, forceUpdate, isZipped);


                }
            }
        }

        public async Task CheckFetchExternalTool(string name, string files, string URL, bool forceUpdate, bool isZipped)
        {
            Log.Debug("CheckFetchExternalTool", "Start");
            //1. check if the files are already there or if they need to be fetched
            var toolDir = Path.Combine(Configs.TOOLS_DIR, name);
            HashSet<string> missingFiles = new HashSet<string>();
            if (!forceUpdate && Directory.Exists(toolDir))
            {
                foreach (var file in files.Split(','))
                {
                    if (file.Length == 0)
                    {
                        continue;
                    }

                    var filePath = Path.Combine(toolDir, file);
                    var fileExists = File.Exists(filePath);


                    if (!fileExists)
                    {
                        missingFiles.Add(file);
                    }
                }
                //if all files are there, there is nothing else to do
                if (missingFiles.Count == 0)
                {
                    return;
                }
            }
            else
            {
                //process all files
                foreach (var file in files.Split(','))
                {
                    if (file.Length == 0)
                    {
                        continue;
                    }
                    missingFiles.Add(file);
                }
            }

            //2. initiate download and unzip 

            //prepare the directory
            Directory.CreateDirectory(toolDir);

            if (isZipped)
            {
                await FileUtils.TryDownloadAndUnzip(URL, missingFiles, toolDir, null).ConfigureAwait(false);
            }
            else
            {
                await FileUtils.TryDownloadFile(URL, Path.Combine(toolDir, files), null).ConfigureAwait(false);
            }
            Log.Debug("CheckFetchExternalTool", "End");


        }
        private T FetchDictValue<T>(Dictionary<string, object> dict, string key, T defaultVal)
        {
            
            if (dict.TryGetValue(key, out object val) && val is T castVal)
            {
                return castVal;
            }
            else
            {
                return defaultVal;
            }
        }
        public async Task LoadConfiguraion()
        {
            //1.load default 
            var defaultConfig = LoadConfigFromXMLFile(Configs.DEFAULT_CONFIG_FILE_PATH);
            Dictionary<string, object> remoteConfig = new Dictionary<string, object>();
            try
            {
                //2.try dwonload the remote 
                //create the configs dir in case it does not exist yet
                Directory.CreateDirectory(Configs.CONFIGS_DIR);
                var filePath = Path.Combine(Configs.CONFIGS_DIR, Configs.REMOTE_CONFIG_FILE_NAME);
                
                //using temp file because the old version was getting overridden by empty file when failing download
                var tmpFile = Path.GetTempFileName();
                var donwloaded = await FileUtils.TryDownloadFile(Configs.CONFIG_FILE_URL, tmpFile, Configs.CONFIG_DOWNLOAD_TIMEOUT).ConfigureAwait(true);
                File.Delete(filePath);
                File.Move(tmpFile, filePath);
                //3.try load the remote(new or old)
                var remoteConfigFilePath = Path.Combine(Configs.CONFIGS_DIR, Configs.REMOTE_CONFIG_FILE_NAME);
                if (File.Exists(remoteConfigFilePath))
                {
                    remoteConfig = LoadConfigFromXMLFile(remoteConfigFilePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error("ConfigurationManager.LoadConfiguraion", ex.Message);
            }

            //4.merge deafult and remote
            //by adding anything from default missing in remote
            foreach (var entry in defaultConfig)
            {
                if (!remoteConfig.ContainsKey(entry.Key))
                {
                    remoteConfig.Add(entry.Key, entry.Value);
                }
            }

            configuration = remoteConfig;



        }
        public Dictionary<string, object> LoadConfigFromXMLFile(string filePath)
        {

            var root = XElement.Load(filePath);
            var config = ParseDictionary(root);
            return config;
        }

        Dictionary<string, object> ParseDictionary(XElement root)
        {
            var ret = new Dictionary<string, object>();
            foreach (var element in root.Elements())
            {
                object value;
                if (element.HasElements)
                {
                    if (element.Attribute("isList")?.Value == "true")
                    {
                        value = ParseList(element);
                    }
                    else
                    {
                        value = ParseDictionary(element);

                    }
                }
                else
                {
                    value = element.Value;

                }
                ret.Add(element.Name.LocalName, value);
            }
            return ret;
        }
        List<Dictionary<string, object>> ParseList(XElement root)
        {
            var ret = new List<Dictionary<string, object>>();
            foreach (var element in root.Elements())
            {
                var dict = ParseDictionary(element);
                ret.Add(dict);
            }

            return ret;
        }

        void LoadUserSettings()
        {
            try
            {
                if (File.Exists(Configs.USER_SETTINGS_FILE_PATH))
                {
                    var json = File.ReadAllText(Configs.USER_SETTINGS_FILE_PATH);
                    userSettings = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);
                }
                else
                {
                    userSettings = GenerateDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                Log.Error("ConfigurationManager.LoadUserSettings", ex.Message);
                userSettings = GenerateDefaultSettings();
            }
        }
        Dictionary<string, Dictionary<string, object>> GenerateDefaultSettings()
        {
            var settings = new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "general", null
                },
                {
                    "log",
                    new Dictionary<string,object>
                    {
                        { "level", 0}
                    }
                },
            };
            return settings;
        }
        public T GetUserSetting<T>(string section, string key, T defaultt)
        {
            if (userSettings.TryGetValue(section, out var sectionDict))
            {
                var val = FetchDictValue<T>(sectionDict, key, defaultt);
                return val;
            }
            return defaultt;
        }
        public void SaveUserSetting(string section, string key, object value, bool saveToDisk = true)
        {
            bool valueChanged = true;
            if (userSettings.ContainsKey(section))
            {
                if (userSettings[section].ContainsKey(key))
                {
                    if (userSettings[section][key] == value)
                    {
                        //this will avoid saving to disk if there is no change
                        valueChanged = false;
                    }
                    else
                    {
                        userSettings[section][key] = value;
                        TriggerSettingChangeActions(section, key, value);
                    }


                }
                else
                {
                    userSettings[section].Add(key, value);
                }
            }
            else
            {
                //add new section
                userSettings.Add(section, new Dictionary<string, object> { { key, value } });
            }

            if (saveToDisk && valueChanged)
            {
                SaveUserSettingsToDisk();
            }

        }
        public void SaveUserSettingsToDisk()
        {
            var json = JsonConvert.SerializeObject(userSettings);
            File.WriteAllText(Configs.USER_SETTINGS_FILE_PATH, json);
        }

        void TriggerSettingChangeActions(string section, string key, object value)
        {
            try
            {
                if (section.Equals("log") && key.Equals("level"))
                {
                    Log.LogLevel = (int)value;
                }
            }
            catch (Exception ex)
            {
                Log.Error("ConfigurationManager.TriggerSettingChangeActions", $"Setting: {section}-{key} = {value} ; Exception: {ex.Message}");
            }
        }
    }
}
