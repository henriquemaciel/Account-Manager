using System;

namespace DraconiusGoGUI.Models
{
    [Serializable]
    public class CatchSetting
    {
        //public CreatureId Id { get; set; }
        public bool Catch { get; set; }
        public bool UsePinap { get; set; }
        public bool Snipe { get; set; }

        public CatchSetting()
        {
            //Id = CreatureId.Missingno;
            Catch = true;
            //Snipe = true;
            UsePinap = true;
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
