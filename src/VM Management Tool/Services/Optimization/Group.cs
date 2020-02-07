using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM_Management_Tool.Services.Optimization
{
    class Group : IGroupChild
    {
        
        public string Name { get; private set; }
        public string Description { get; private set; }
       
        [JsonIgnore]
        public Group Parent { get; set; }
        public IList<IGroupChild> Children { get; private set; }
        public Group(string name, string description)
        {
            Name = name;
            Description = description;
            Children = new List<IGroupChild>();
        }

        public void AddChild(IGroupChild child)
        {
            Children.Add(child);
            child.Parent =this;
        }
    }
}
