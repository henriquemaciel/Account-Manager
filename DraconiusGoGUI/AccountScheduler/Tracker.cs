using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DraconiusGoGUI.AccountScheduler
{
    public class Tracker
    {
        public Dictionary<DateTime, TrackerValues> Values { get; set; }

        public Tracker()
        {
            Values = new Dictionary<DateTime, TrackerValues>();
        }

        [JsonIgnore]
        public int CreatureCaught { get;  set; }

        [JsonIgnore]
        public int PokestopsFarmed { get;  set; }

        public void AddValues(int Creature = 0, int pokestops = 0)
        {
            lock (Values)
            {
                CreatureCaught += Creature;
                PokestopsFarmed += pokestops;

                //Add to dictionary
                DateTime currentTime = GetCurrentHourDateTime();

                if (Values.ContainsKey(currentTime))
                {
                    TrackerValues trackerValues = Values[currentTime];

                    trackerValues.Creature += Creature;
                    trackerValues.Pokestops += pokestops;
                }
                else
                {
                    TrackerValues trackerValues = new TrackerValues
                    {
                        Creature = Creature,
                        Pokestops = pokestops
                    };

                    Values.Add(currentTime, trackerValues);

                    CalculatedTrackingHours();
                }
            }
        }

        public void CalculatedTrackingHours()
        {
            DateTime currentTime = GetCurrentHourDateTime();
            DateTime resetTime = currentTime.AddHours(-23); //Tracks last 23 hours

            lock (Values)
            {
                Values = Values.Where(x => x.Key >= resetTime).ToDictionary(x => x.Key, x => x.Value);

                int Creature = 0;
                int pokestops = 0;

                //Should only contain last 24 hours worth of values
                foreach(KeyValuePair<DateTime, TrackerValues> tracker in Values)
                {
                    Creature += tracker.Value.Creature;
                    pokestops += tracker.Value.Pokestops;
                }

                CreatureCaught = Creature;
                PokestopsFarmed = pokestops;
            }
        }

        public DateTime GetCurrentHourDateTime()
        {
            DateTime currentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);

            return currentTime;
        }
    }
}
