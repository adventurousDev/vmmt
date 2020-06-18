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
    class Group : IGroupChild
    {

        public string Name { get; private set; }
        public string Description { get; private set; }

        [JsonIgnore]
        public Group Parent { get; set; }
        public IList<IGroupChild> Children { get; private set; }

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

            if (selected.HasValue)
            {
                UpdateChildrenSelections(selected.Value);
            }
            if(Parent!=null && updateParent)
            {
                Parent?.OnChildSelectionChanged();
            }


            OnPropertyChanged("UISelected");




        }

        public Group(string name, string description)
        {
            Name = name;
            Description = description;
            Children = new List<IGroupChild>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal void OnChildSelectionChanged()
        {
            IGroupChild prevChild = null;
            foreach (var child in Children)
            {
                if (prevChild == null || prevChild.UISelected == child.UISelected)
                {
                    prevChild = child;
                    continue;
                }
                else
                {
                    SetUISelected(null, true);
                    return;
                }

            }
            SetUISelected(prevChild.UISelected, true);
        }
        void UpdateChildrenSelections(bool value)
        {
            foreach (var child in Children)
            {
                child.SetUISelected(value, false);
            }
        }
        public void AddChild(IGroupChild child)
        {
            Children.Add(child);
            child.Parent = this;
        }
    }
}
