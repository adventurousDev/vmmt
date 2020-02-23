﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using VMManagementTool.Services.Optimization.Actions;

namespace VMManagementTool.Services.Optimization
{
    class OptimizationTemplate
    {

        const string OS_REF_METADATA_KEY = "osdefinitions";

        public Dictionary<string, object> Metadata { get; set; }
        public IList<Group> RootGroups;

        //events 
        public event Action<string> NewInfo;
        public OptimizationTemplate()
        {

        }
        public void Load(string path)
        {
            try
            {


                var xmlReader = XmlReader.Create(path, new XmlReaderSettings());

                var doc = new XPathDocument(xmlReader);
                var nav = doc.CreateNavigator();
                nav.MoveToRoot();

                //first fetch meta/ global data
                //this is:
                //* all sequence attributes 
                //* os definitions 
                //* log data

                Metadata = new Dictionary<string, object>();

                var rootAttrIterator = nav.Select("/sequence/@*");
                while (rootAttrIterator.MoveNext())
                {
                    Metadata.Add(rootAttrIterator.Current.Name, rootAttrIterator.Current.Value);
                }

                Dictionary<string, string> osDefDictionary = new Dictionary<string, string>();
                var osCollectionIterator = nav.Select("/sequence/globalVarList/osCollection/*");
                while (osCollectionIterator.MoveNext())
                {
                    var osId = osCollectionIterator.Current.GetAttribute("osId", "");
                    var osName = osCollectionIterator.Current.GetAttribute("name", "");
                    if (string.Empty.Equals(osId) || string.Empty.Equals(osName))
                    {
                        continue;

                    }
                    osDefDictionary.Add(osId, osName);
                }
                Metadata.Add(OS_REF_METADATA_KEY, osDefDictionary);

                //skipping log for now because it seems unimportant for our purposes

                //next, get all the groups and steps

                //the collection of root groups(normally there is just one)
                RootGroups = new List<Group>();

                var rootGroupIterator = nav.Select("/sequence/group");

                while (rootGroupIterator.MoveNext())
                {
                    var group = RecursivelyParseGroup(rootGroupIterator.Current.Clone());
                    RootGroups.Add(group);
                }

                //test shit ****************************************************************
                /*
                var steps = GetAllSteps();
                nav.MoveToRoot();
                var stepsIterator = nav.Select("//step[@name]");
                var xmlNames = new List<string>();//new HashSet<string>();
                //var actionsIterator = nav.Select("//action");
                while (stepsIterator.MoveNext())
                {
                    var name = stepsIterator.Current.GetAttribute("name", "");
                    if (name == "")
                    {
                        throw new Exception("no name!!!");
                    }
                    xmlNames.Add(name);
                }

                //var onjNames = new HashSet<string>(steps.Select((step) => { return step.Name; }));

                //xmlNames.SymmetricExceptWith(onjNames);
                //string json = JsonConvert.SerializeObject(steps);
                //Log(json);
                xmlNames.Sort();
                string names = string.Join(Environment.NewLine,xmlNames);
                Log(names);
                */
                //string json = JsonConvert.SerializeObject(RootGroups);
                //Log(json);
                xmlReader.Close();
            }
            catch (Exception e)
            {
                throw;
            }



        }

        public List<Step> GetAllSteps()
        {
            var res = new List<Step>();
            if (RootGroups != null)
            {
                foreach (Group rootGroup in RootGroups)
                {
                    RecursivelyAddSteps(res, rootGroup);
                };
            }
            return res;
        }
        public void PrintAllShellCMDs()
        {
            var shellCmds = GetAllSteps().Where((s) => s.Action is ShellExecuteAction).Select((s) => (s.Action as ShellExecuteAction).ShellCommand);
            foreach (var cmd in shellCmds)
            {
                Log(cmd);
            }

        }
        void RecursivelyAddSteps(List<Step> theList, Group group)
        {
            foreach (var child in group.Children)
            {
                if (child is Step step)
                {
                    theList.Add(step);
                }
                else
                {
                    RecursivelyAddSteps(theList, child as Group);
                }
            }
        }
        Group RecursivelyParseGroup(XPathNavigator groupXNav)
        {
            //it has to be group
            if (groupXNav.NodeType != XPathNodeType.Element || groupXNav.Name != "group")
            {
                throw new Exception("RecursivelyParseGroup must be called for <group> elements only");
            }
            //parse the group
            var name = groupXNav.GetAttribute("name", "");
            var desc = groupXNav.GetAttribute("description", "");
            Group theGroup = new Group(name, desc);

            //iterate over the children and handle each
            var childIterator = groupXNav.SelectChildren(XPathNodeType.Element);
            while (childIterator.MoveNext())
            {
                IGroupChild childObj = null;
                if (childIterator.Current.NodeType == XPathNodeType.Element && childIterator.Current.Name == "group")
                {
                    childObj = RecursivelyParseGroup(childIterator.Current.Clone());
                }
                else if (childIterator.Current.NodeType == XPathNodeType.Element && childIterator.Current.Name == "step")
                {
                    childObj = ParseStep(childIterator.Current.Clone());
                }
                else
                {
                    //this should never happen, but if there is some anomaly 
                    //we can safely ignore it
                    continue;
                }
                theGroup.AddChild(childObj);
            }

            return theGroup;

        }

        internal void RunAll(HashSet<string> onlyRun = null)
        {
            var csvDelimiter = '`';

            var allSteps = GetAllSteps();
            Log($"Starting processing {allSteps.Count} steps");
            int lastPercentage = 0;
            int stepCounter = 0;

            StringBuilder csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("");
            string curentGroup = "none";
            string csv = $"Group{csvDelimiter} Step{csvDelimiter}Type{csvDelimiter}Category{csvDelimiter} State Before{csvDelimiter} Execution result{csvDelimiter} State After";
            csvBuilder.AppendLine(csv);
            foreach (var step in allSteps)
            {
                if (onlyRun != null && ! onlyRun.Contains(step.Name))
                {
                    continue;
                }
                stepCounter++;
                int percentage = (int)((stepCounter*1f / allSteps.Count) * 100);
                if(percentage - lastPercentage>=10)
                {
                    Log($"Processing steps: {percentage}%");
                    lastPercentage = percentage;
                }

                if (step.Parent.Name != curentGroup)
                {
                    curentGroup = step.Parent.Name;
                    csv = $"{curentGroup}";
                    csvBuilder.AppendLine(csv);

                }
                Log($"Processing step: {step.Name}");
                var stateBefore = step.Action.CheckStatus();
                bool execResult = step.Action.Execute();
                var stateAfter = step.Action.CheckStatus();

                csv = $"{csvDelimiter}{step.Name}{csvDelimiter} {step.Action.GetType()}{csvDelimiter} {step.Category}{csvDelimiter} {stateBefore}{csvDelimiter} {execResult}{csvDelimiter} {stateAfter}";
                csvBuilder.AppendLine(csv);

            }

            Log(csvBuilder.ToString());

        }


        Step ParseStep(XPathNavigator stepXNav)
        {
            //it has to be step
            if (stepXNav.NodeType != XPathNodeType.Element || stepXNav.Name != "step")
            {
                throw new Exception("ParseStep must be called for <step> elements only");
            }
            //parse the given step element into Step object
            string name = stepXNav.GetAttribute("name", "");
            string description = stepXNav.GetAttribute("description", "");

            Step.Categories category;
            var categoryString = stepXNav.GetAttribute("name", "");
            switch (categoryString)
            {
                case "recommended":
                    category = Step.Categories.recommended;
                    break;
                case "mandatory":
                    category = Step.Categories.mandatory;
                    break;
                default:
                    category = Step.Categories.optional;
                    break;
            }

            bool defaultSelected = stepXNav.GetAttribute("defaultSelected", "").ToLower() == "true";
            bool rebootRequired = stepXNav.GetAttribute("isRebootRequired", "").ToLower() == "true";

            Action_ action = ParseAction(stepXNav.SelectSingleNode("action"));
            Condition condition = ParseCondition(stepXNav.SelectSingleNode("condition"));

            Step step = new Step(name, description, category, defaultSelected, rebootRequired, action, condition);
            return step;
        }



        Action_ ParseAction(XPathNavigator actionXNav, bool subAction = false)
        {
            if (actionXNav == null)
            {
                return null;
            }
            Action_ action = null;
            string type = actionXNav.SelectSingleNode("type")?.Value;
            switch (type)
            {
                case "Registry":
                    action = ParseRegistryAction(actionXNav);
                    break;
                case "Service":
                    action = ParseServiceAction(actionXNav);
                    break;
                case "ShellExecute":
                    action = ParseShellExecuteAction(actionXNav);
                    break;
                case "SchTasks":
                    action = ParseSchTasksAction(actionXNav);
                    break;
                case "Custom Check":
                    action = ParseCustomCheckAction(actionXNav);
                    break;
                default:
                    throw new Exception("Invalid action type");
            }

            //fetch messageOnly attribute
            action.MessageOnly = actionXNav.GetAttribute("messageOnly", "").ToLower() == "true";

            //this flag/ check is not even necessary because the XML structure
            //would express it
            if (!subAction)
            {
                //todo also deal with sub-actions
                var customOptimizationXNav = actionXNav.SelectSingleNode("customOptimization");
                if (customOptimizationXNav != null)
                {
                    action.CustomOptimization = ParseAction(customOptimizationXNav, true);

                }
                var customRollbackXNav = actionXNav.SelectSingleNode("customrollback");
                if (customRollbackXNav != null)
                {
                    action.CustomRollback = ParseAction(customRollbackXNav, true);

                }
            }

            return action;
        }

        private Action_ ParseCustomCheckAction(XPathNavigator actionXNav)
        {
            //parse params
            //these are all known keys:
            var parameters = ParseParams(actionXNav, new[] { "noLess", "noGreater", "programName" });

            //parse command
            //and perform mandatory params and other checks
            var targetStr = actionXNav.SelectSingleNode("target")?.Value;
            CustomCheckAction.CustomCheckTarget target;
            switch (targetStr)
            {
                case "Disc Count":
                    target = CustomCheckAction.CustomCheckTarget.DiskCount;
                    break;
                case "Disc Space":
                    target = CustomCheckAction.CustomCheckTarget.DiskCount;
                    break;
                case "Installed Program":
                    target = CustomCheckAction.CustomCheckTarget.InstalledProgram;
                    //mandatories
                    AsserNonEmptyParamsAndThrow(targetStr, new[] { "programName" }, parameters);
                    break;

                default:
                    throw new Exception($"Unknown custom command target {targetStr}");

            }
            return new CustomCheckAction(target, parameters);

        }

        private Action_ ParseSchTasksAction(XPathNavigator actionXNav)
        {
            //parse params
            //these are all known keys:
            var parameters = ParseParams(actionXNav, new[] { "taskName", "status" });



            return new SchTasksAction(parameters);
        }

        private Action_ ParseShellExecuteAction(XPathNavigator actionXNav)
        {
            //this has no parameters

            //get the string command
            string command = actionXNav.SelectSingleNode("command")?.Value;
            if (string.IsNullOrEmpty(command))
            {
                throw new Exception("SchellExecute action required a non-empty command");
            }

            return new ShellExecuteAction(command);
        }

        private Action_ ParseServiceAction(XPathNavigator actionXNav)
        {
            //parse params
            //these are all known keys:
            var parameters = ParseParams(actionXNav, new[] { "serviceName", "startMode" });

            return new ServiceAction(parameters);
        }

        RegistryAction ParseRegistryAction(XPathNavigator actionXNav)
        {
            //parse params
            //these are all known keys:
            var parameters = ParseParams(actionXNav, new[] {
                RegistryAction.PARAM_NAME_KEY,
                RegistryAction.PARAM_NAME_VALUE,
                RegistryAction.PARAM_NAME_TYPE,
                RegistryAction.PARAM_NAME_DATA,
                RegistryAction.PARAM_NAME_FILENAME
            });



            //parse command
            //and perform mandatory params and other checks
            var commandStr = actionXNav.SelectSingleNode("command")?.Value;
            RegistryAction.RegistryCommand command;
            switch (commandStr)
            {
                case "ADD":
                    command = RegistryAction.RegistryCommand.Add;
                    //mandatories
                    AsserNonEmptyParamsAndThrow(commandStr, new[] { RegistryAction.PARAM_NAME_KEY }, parameters);
                    break;
                case "DELETEKEY":
                    command = RegistryAction.RegistryCommand.DeleteKey;
                    //mandatories
                    AsserNonEmptyParamsAndThrow(commandStr, new[] { RegistryAction.PARAM_NAME_KEY }, parameters);
                    break;
                case "DELETEVALUE":
                    command = RegistryAction.RegistryCommand.DeleteValue;
                    //mandatories
                    AsserNonEmptyParamsAndThrow(commandStr, new[] { RegistryAction.PARAM_NAME_KEY, RegistryAction.PARAM_NAME_VALUE }, parameters);
                    break;
                case "LOAD":
                    command = RegistryAction.RegistryCommand.Load;
                    //mandatories
                    AsserNonEmptyParamsAndThrow(commandStr, new[] { RegistryAction.PARAM_NAME_KEY, RegistryAction.PARAM_NAME_FILENAME }, parameters);

                    break;
                case "UNLOAD":
                    command = RegistryAction.RegistryCommand.Unload;
                    //mandatories
                    AsserNonEmptyParamsAndThrow(commandStr, new[] { RegistryAction.PARAM_NAME_KEY }, parameters);
                    break;
                default:
                    throw new Exception($"Unknown registry command {commandStr}");

            }

            return new RegistryAction(command, parameters);
        }
        Dictionary<string, string> ParseParams(XPathNavigator actionXNav, string[] keys)
        {
            var parameters = new Dictionary<string, string>();
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                string value = actionXNav.SelectSingleNode($"params/{key}")?.Value;
                if (value != null)
                {
                    parameters.Add(key, value);
                }

            }
            return parameters;
        }
        Condition ParseCondition(XPathNavigator conditionXNav)
        {
            //todo implement this
            if (conditionXNav == null)
            {
                return null;
            }
            return null;
        }
        void AsserNonEmptyParamsAndThrow(string command, string[] mandatoryKeys, Dictionary<string, string> parameters)
        {
            for (int i = 0; i < mandatoryKeys.Length; i++)
            {
                if (!parameters.TryGetValue(mandatoryKeys[i], out string value) || string.IsNullOrEmpty(value))
                {
                    throw new Exception($"The command/ target: {command} requires parameter(s): {string.Join(",", mandatoryKeys)}");
                }
            }
        }
        void Log(string msg)
        {
            NewInfo?.Invoke(msg);
        }
    }
}
