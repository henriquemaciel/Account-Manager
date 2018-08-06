using Newtonsoft.Json;
using DraconiusGoGUI.AccountScheduler;
using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.DracoManager.Models;
using DraconiusGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DracoProtos.Core.Objects;
using DracoProtos.Core.Base;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        public byte[] LogHeaderSettings { get; set; }
        public AccountState AccountState { get; set; }
        public Settings UserSettings { get; set; }
        public Tracker Tracker { get; set; }
        public Scheduler AccountScheduler { get; set; }
        public FAvaUpdate Stats { get; set; }
        
        //[JsonIgnore]
        //public Dictionary<CreatureMove, MoveSettings> MoveSettings { get; private set; }
        //[JsonIgnore]
        //public Dictionary<BadgeType, BadgeSettings> BadgeSettings { get; private set; }
        //[JsonIgnore]
        //public Dictionary<ItemId, ItemSettings> ItemSettings { get; private set; }
        //[JsonIgnore]
        //public GymBattleSettings BattleSettings { get; private set; }
        //[JsonIgnore]
        //public CreatureUpgradeSettings UpgradeSettings { get; private set; }
        //[JsonIgnore]
        //public MoveSequenceSettings GetMoveSequenceSettings { get; private set; }
        //[JsonIgnore]
        //public EncounterSettings GetEncounterSettings { get; private set; }
        //[JsonIgnore]
        //public IapItemDisplay GetIapItemDisplay { get; private set; }
        //[JsonIgnore]
        //public IapSettings GetIapSettings { get; private set; }
        //[JsonIgnore]
        //public EquippedBadgeSettings GetEquippedBadgeSettings { get; private set; }
        //[JsonIgnore]
        //public QuestSettings GetQuestSettings { get; private set; }
        //[JsonIgnore]
        //public AvatarCustomizationSettings GetAvatarCustomizationSettings { get; private set; }
        //[JsonIgnore]
        //public FormSettings GetFormSettings { get; private set; }
        //[JsonIgnore]
        //public GenderSettings GetGenderSettings { get; private set; }
        //[JsonIgnore]
        //public GymBadgeGmtSettings GetGymBadgeGmtSettings { get; private set; }
        //[JsonIgnore]
        //public WeatherAffinity GetWeatherAffinity { get; private set; }
        //[JsonIgnore]
        //public WeatherBonus GetWeatherBonus { get; private set; }
        //[JsonIgnore]
        //public CreatureScaleSetting GetCreatureScaleSetting { get; private set; }
        //[JsonIgnore]
        //public TypeEffectiveSettings GetTypeEffectiveSettings { get; private set; }
        //[JsonIgnore]
        //public CameraSettings GetCameraSettings { get; private set; }
        //[JsonIgnore]
        //public GymLevelSettings GetGymLevelSettings { get; private set; }
        //[JsonIgnore]
        //public GetPlayerProfileResponse PlayerProfile { get; private set; }

        [JsonIgnore]
        public string SchedulerName
        {
            get
            {
                return AccountScheduler == null ? String.Empty : AccountScheduler.Name;
            }
        }

        [JsonIgnore]
        public int CreatureCaught
        {
            get
            {
                return Tracker == null ? 0 : Tracker.CreatureCaught;
            }
            set
            {
                Tracker.CreatureCaught = value;
            }
        }

        [JsonIgnore]
        public int BuildingsFarmed
        {
            get
            {
                return Tracker == null ? 0 : Tracker.BuildingsFarmed;
            }
            set
            {
                Tracker.BuildingsFarmed = value;
            }
        }

        [JsonIgnore]
        public string GroupName
        {
            get
            {
                return String.IsNullOrEmpty(UserSettings.GroupName) ? String.Empty : UserSettings.GroupName;
            }
        }

        [JsonIgnore]
        public string Proxy
        {
            get
            {
                var proxyEx = new ProxyEx
                {
                    Address = UserSettings.ProxyIP,
                    Port = UserSettings.ProxyPort,
                    Username = UserSettings.ProxyUsername,
                    Password = UserSettings.ProxyPassword
                };

                return proxyEx.ToString();
            }
        }

        [JsonIgnore]
        public BotState State { get; set; }

        [JsonIgnore]
        public FUserInfo PlayerData { get; set; } = new FUserInfo();

        [JsonIgnore]
        public FBagUpdate UserBag { get; set; } = new FBagUpdate();

        [JsonIgnore]
        public List<Log> Logs { get; private set; }

        [JsonIgnore]
        public List<FBagItem> Items { get; private set; } = new List<FBagItem>();

        [JsonIgnore]
        public List<FUserCreature> Creatures { get; private set; } = new List<FUserCreature>();

        [JsonIgnore]
        public List<FCreadexEntry> DracoDex { get; private set; }  = new List<FCreadexEntry>();

        [JsonIgnore]
        public Dictionary<CreatureType, int> CreatureCandy { get; private set; } = new Dictionary<CreatureType, int>();

        [JsonIgnore]
        public List<FIncubator> Incubators { get; private set; } = new List<FIncubator>();

        [JsonIgnore]
        public List<FEgg> Eggs { get; private set; } = new List<FEgg>();

        //[JsonIgnore]
        //public Dictionary<CreatureType, CreatureSettings> PokeSettings { get; private set; }

        //[JsonIgnore]
        //public PlayerLevelSettings LevelSettings { get; private set; }

        [JsonIgnore]
        public List<FarmLocation> FarmLocations { get; private set; }

        [JsonIgnore]
        public bool IsRunning { get; private set; }

        [JsonIgnore]
        public bool StartingUp { get; private set; }

        [JsonIgnore]
        public int TotalLogs
        {
            get
            {
                return Logs == null ? 0 : Logs.Count;
            }
        }

        [JsonIgnore]
        public string LastLogMessage
        {
            get
            {
                if (Logs == null || Logs.Count == 0)
                {
                    return String.Empty;
                }

                lock (Logs)
                {
                    string message = Logs.Last().Message;

                    return String.IsNullOrEmpty(message) ? String.Empty : message;
                }
            }
        }

        [JsonIgnore]
        public string AccountName
        {
            get
            {
                return UserSettings == null ? "???" : UserSettings.AccountName;
            }
        }

        [JsonIgnore]
        public int Level
        {          
            get
            {
                return Stats == null ? 0 : Stats.level;
            }
            
            set
            {
                Stats.level = value;
            }
        }

        [JsonIgnore]
        public string Team
        {
            get
            {
                return Stats?.alliance.Value.ToString() ?? "Neutral";
            }
            set
            {
                UserSettings.DefaultTeam = value;
            }
        }

        [JsonIgnore]
        public int MaxLevel
        {
            get
            {
                return UserSettings == null ? 0 : UserSettings.MaxLevel;
            }
        }

        [JsonIgnore]
        public string TillLevelUp
        {
            get
            {
                if(ExpPerHour == 0)
                {
                    return "Unknown";
                }

                long currentExp = Stats.currentExperience;
                long required = Stats.nextLevelExperience - (long)Stats.exp;
                long needed = required - currentExp;

                int expPerHour = ExpPerHour;

                double totalHours = (double)needed / expPerHour;

                if(totalHours <= 0)
                {
                    return "Now";
                }

                TimeSpan time = TimeSpan.FromHours(totalHours);

                if(time.TotalHours < 1)
                {
                    return String.Format("{0:0}m {1:00}s", time.Minutes, time.Seconds);
                }

                return time.TotalHours >= 24 ? String.Format("{0:0}d {1:0}h {2:00}m", time.Days, time.Hours, time.Seconds) : String.Format("{0:0}h {1:00}m {2:00}s", time.Hours, time.Minutes, time.Seconds);
            }
        }

        [JsonIgnore]
        public string RemainingRunningTime
        {
            get
            {
                if (Math.Abs(MaxRuntime) < 0.0001) {
                    return "Unlimited";
                }

                double remainingHours = MaxRuntime - _runningStopwatch.Elapsed.TotalHours;
                TimeSpan time = TimeSpan.FromHours(remainingHours);


                if (time.TotalHours < 1)
                {
                    return String.Format("{0:0}m {1:00}s", time.Minutes, time.Seconds);
                }

                return time.TotalHours >= 24 ? String.Format("{0:0}d {1:0}h {2:00}m", time.Days, time.Hours, time.Seconds) : String.Format("{0:0}h {1:00}m {2:00}s", time.Hours, time.Minutes, time.Seconds);
            }
        }

        [JsonIgnore]
        public string ExpRatio
        {
            get
            {
                if (Stats == null)
                {
                    return "??/??";
                }

                long currentExp = Stats.currentExperience;
                long required = Stats.nextLevelExperience - (long)Stats.exp;

                double ratio = 0;

                if (required != 0)
                {
                    ratio = (double)currentExp / required * 100;
                }

                return String.Format("{0}/{1} ({2:0.00}%)", currentExp, required, ratio);
            }
        }

        [JsonIgnore]
        public int MaxItemStorage
        {
            get
            {
                return UserBag == null ? 250 : UserBag.allowedItemsSize;
            }
        }

        [JsonIgnore]
        public int MaxCreatureStorage
        {
            get
            {
                return Stats == null ? 250 : Stats.creatureStorageSize;
            }
        }

        [JsonIgnore]
        public int TotalStardust
        {
            get
            {
                if(Stats == null || Stats.dust == 0)
                {
                    return 0;
                }
               return Stats.dust == 0 ? 0 : Stats.dust;
            }
        }

        [JsonIgnore]
        public int TotalPokeCoins
        {
            get
            {
                if (Stats == null || Stats.coins == 0)
                {
                    return 0;
                }

                return Stats.coins == 0 ? 0 : Stats.coins;
            }
        }

        [JsonIgnore]
        public int ExpPerHour
        {
            get
            {
                double totalHours = _runningStopwatch.Elapsed.TotalHours;

                if (Math.Abs(totalHours) < 0.0001) {
                    return 0;
                }

                double expPerHour = ExpGained / totalHours;

                return (int)expPerHour;
            }
        }

        [JsonIgnore]
        public string RunningTime
        {
            get
            {
                //return String.Format("{0:c}", _runningStopwatch.Elapsed);
                return _runningStopwatch.Elapsed.ToString(@"dd\.hh\:mm\:ss");
            }
        }

        [JsonIgnore]
        public int ExpGained { get; set; } = 0;

        [JsonIgnore]
        public int TotalBuildingExp { get; set; }

        [JsonIgnore]
        public double MaxRuntime
        {
            get
            {
                return UserSettings == null ? 0 : UserSettings.RunForHours;
            }
        }

        public DateTime UseCristaldateTime { get; set; } = DateTime.Now;

        [JsonIgnore]
        public bool CristalActive
        {
            get
            {
                if (_client.LoggedIn)
                {
                    if (UseCristaldateTime >= DateTime.Now)
                        return true;

                }
                    return false;
            }
        }

        public DateTime UseDragonVisiondateTime { get; set; } = DateTime.Now;

        [JsonIgnore]
        public bool DragonVisonActive
        {
            get
            {
                if (_client.LoggedIn)
                {
                    if (UseDragonVisiondateTime >= DateTime.Now)
                        return true;

                }
                return false;
            }
        }


        private Stopwatch _runningStopwatch = new Stopwatch();

        private void ExpIncrease(int amount)
        {
            ExpGained += amount;
            //Stats.Experience += amount;
        }

        private int RemainingPokeballs()
        {
            int total = 0;
            
            FBagItem data = Items.FirstOrDefault(x => x.type == ItemType.MAGIC_BALL_SIMPLE);

            if(data != null)
            {
                total += data.count;
            }

            data = Items.FirstOrDefault(x => x.type == ItemType.MAGIC_BALL_NORMAL);

            if (data != null)
            {
                total += data.count;
            }
            data = Items.FirstOrDefault(x => x.type == ItemType.MAGIC_BALL_GOOD);

            if (data != null)
            {
                total += data.count;
            }

            return total;
        }

        private bool HasPokeballsLeft()
        {
            return RemainingPokeballs() > 0;
        }
    }
}
