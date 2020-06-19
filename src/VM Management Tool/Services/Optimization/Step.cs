using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VMManagementTool.Services.Optimization
{
    public class Step : IGroupChild
    {
        public enum Categories
        {
            recommended,
            optional,
            mandatory

        }
        // get => name; set { name = value; OnPropertyChanged(); }
        public string Name { get; set; }
        public string Description { get; set; }
        public Categories Category { get; set; }
        public bool DefaultSelected { get; set; }
        public bool RebootRequired { get; set; }
        [JsonIgnore]
        public Group Parent { get; set; }
        public Action_ Action { get; private set; }

        [JsonIgnore]
        public string ID { get { return Name; } }
        bool? selected = false;
        [JsonIgnore]
        public bool? UISelected { get { return selected; } set { SetUISelected(value, true); } }
        public void SetUISelected(bool? selected, bool updateParent)
        {
            if (selected == this.selected)
            {
                return;
            }
            this.selected = selected;

            if (updateParent)
            {
                Parent?.OnChildSelectionChanged();
            }

            OnPropertyChanged("UISelected");

        }

        //condition could later be transformed to multiple conditions
        //according to XML schema 
        //so this should stay private and the evaluation be performed via a method?
        private Condition Condition { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
