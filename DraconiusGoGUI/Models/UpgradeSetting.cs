using System;

namespace DraconiusGoGUI.Models
{
    [Serializable]
    public class UpgradeSetting
    {
        //public CreatureId Id { get; set; }
        public bool Upgrade { get; set; }

        public UpgradeSetting()
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