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
            Id = CreatureType.MONSTER_WATER_1;
        }

        public string Name { get; set; }
    }
}