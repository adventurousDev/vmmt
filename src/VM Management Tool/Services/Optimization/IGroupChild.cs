using System.ComponentModel;

namespace VMManagementTool.Services.Optimization
{
    internal interface IGroupChild : INotifyPropertyChanged
    {
        Group Parent { get; set; }
        bool? UISelected { get; set; }
        void SetUISelected(bool? selected, bool updateParent);
    }
}