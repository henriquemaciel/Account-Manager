using System;

namespace DraconiusGoGUI.Models
{
    [Serializable]
    public class FarmLocation
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
