using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services.Optimization
{
    class Step : IGroupChild
    {
        public enum Categories
        {
            recommended,
            optional,
            mandatory

        }
        
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Categories Category { get; private set; }
        public bool DefaultSelected { get; private set; }
        public bool RebootRequired { get; private set; }
        public Group Parent { get; set; }
        public Action_ Action { get; private set; }
        
        //condition could later be transformed to multiple conditions
        //according to XML schema 
        //so this should stay private and the evaluation be performed via a method?
        private Condition Condition { get;  set; }

        public Step(string name, string description, Categories category, bool defaultSelected, bool rebootRequired, Action_ action, Condition condition)
        {
            Name = name;
            Description = description;
            Category = category;
            DefaultSelected = defaultSelected;
            RebootRequired = rebootRequired;
            Action = action;
            Condition = condition;
        }

        
    }
}
