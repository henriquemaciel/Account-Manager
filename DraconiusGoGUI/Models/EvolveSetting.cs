using System;

namespace DraconiusGoGUI.Models
{
    [Serializable]
    public class EvolveSetting
    {
        //public CreatureId Id { get; set; }
        public bool Evolve { get; set; }
        public int MinCP { get; set; }

        public EvolveSetting()
        {
            //Id = CreatureId.Missingno;
        }

        public string Name
        {
            get
            {
                return string.Empty;
                //return Id.ToString();
            }
        }
    }
}
