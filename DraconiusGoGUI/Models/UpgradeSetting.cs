using DracoProtos.Core.Base;
using System;

namespace DraconiusGoGUI.Models
{
    [Serializable]
    public class UpgradeSetting
    {
        public CreatureType Id { get; set; }
        public bool Upgrade { get; set; }

        public UpgradeSetting()
        {
            Id = 0;
        }

        public string Name
        {
            get
            {
                return Id.ToString();
            }
        }
    }
}