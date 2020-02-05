using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace VM_Management_Tool.Services.Optimization
{
    class OptimizationTemplate
    {

        const string OS_REF_METADATA_KEY = "osdefinitions";

        public Dictionary<string, object> Metadata { get; set; }
        public IList<Group> Groups;

        //events 
        public event Action<string> NewInfo;
        public OptimizationTemplate()
        {

        }
        public void Load(string path)
        {
            try
            {


                //var xmlReader = XmlReader.Create(path)

                var doc = new XPathDocument(path);
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
                Groups = new List<Group>();

                var rootGroupIterator = nav.Select("/sequence/group");

                while (rootGroupIterator.MoveNext())
                {
                    var group = RecursivelyParseGroup(rootGroupIterator.Current.Clone());
                    Groups.Add(group);
                }

            }
            catch (Exception e)
            {
                throw;
            }

            string json = JsonConvert.SerializeObject(Groups);
            Log(json);

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

        Action_ ParseAction(XPathNavigator actionXNav)
        {
            if (actionXNav == null)
            {
                return null;
            }
            Action_.Types actionType;
            string type = actionXNav.SelectSingleNode("type").Value;
            switch (type)
            {
                case "Registry":
                    actionType = Action_.Types.Registry;
                    break;
                case "Service":
                    actionType = Action_.Types.Service;
                    break;
                case "ShellExecute":
                    actionType = Action_.Types.ShellExecute;
                    break;
                case "SchTasks":
                    actionType = Action_.Types.SchTasks;
                    break;
                case "Custom Check":
                    actionType = Action_.Types.CustomCheck;
                    break;
                default:
                    throw new Exception("Invalid action type");
                    


            }


            return null;
        }
        Condition ParseCondition(XPathNavigator conditionXNav)
        {
            if (conditionXNav == null)
            {
                return null;
            }
            return null;
        }

        void Log(string msg)
        {
            NewInfo?.Invoke(msg);
        }
    }
}
