using System;

namespace SettingPanelEnhancementQX.Utils
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PatchGroup : Attribute
    {
        public string GroupName { get; set; }

        public PatchGroup(string groupName)
        {
            GroupName = groupName;
        }
    }
}
