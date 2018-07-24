using DracoProtos.Core.Base;
using System;

namespace DraconiusGoGUI.Models
{
    [Serializable]
    public class EvolveSetting
    {
        public CreatureType Id { get; set; }
        public bool Evolve { get; set; }
        public int MinCP { get; set; }

        public EvolveSetting()
        {
            Id = CreatureType.MONSTER_WATER_1;
        }

        public string Name { get; set; }
    }
}
