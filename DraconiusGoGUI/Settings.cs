using Newtonsoft.Json;
using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DracoProtos.Core.Base;

namespace DraconiusGoGUI
{
    [Serializable]
    public class Settings
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double HorizontalAccuracy { get; set; }
        public string DeviceId { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceModelBoot { get; set; }
        public string HardwareManufacturer { get; set; }
        public string HardwareModel { get; set; }
        public string FirmwareBrand { get; set; }
        public string FirmwareType { get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public string TimeZone { get; set; }
        public string POSIX { get; set; }


        public bool AutoFavoritShiny { get; set; }
        public bool UseIncense { get; set; }
        public bool UseLuckEggConst { get; set; }
        public int LevelForConstLukky { get; set; }
        public string DefaultTeam { get; set; }
        public double DisableCatchDelay { get; set; }
        public bool SpinGyms { get; set; }
        public bool GoOnlyToGyms { get; set; }
        public bool DeployCreature { get; set; }
        public string GroupName { get; set; }
        public string AccountName { get; set; }
        public AuthType AuthType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool MimicWalking { get; set; }
        public int WalkingSpeed { get; set; }
        public bool EncounterWhileWalking { get; set; }
        public double MaxPokestopMeters { get; set; }
        public int MaxPokestopMetersRandom { get; set; }
        public int MaxTravelDistance { get; set; }
        public bool UseLuckyEgg { get; set; }
        public bool ClaimLevelUpRewards { get; set; }
        public int MinCreatureBeforeEvolve { get; set; }
        public bool RecycleItems { get; set; }
        public bool TransferCreature { get; set; }
        public bool EvolveCreature { get; set; }
        public bool UpgradeCreature { get; set; }
        public bool CatchCreature { get; set; }
        public bool IncubateEggs { get; set; }
        public int MaxLevel { get; set; }
        public int PercTransItems { get; set; }
        public int PercTransPoke { get; set; }
        public bool SPF { get; set; }

        public double SearchFortBelowPercent { get; set; }
        public int CatchCreatureDayLimit { get; set; }
        public int SpinPokestopsDayLimit { get; set; }
        public bool SnipeAllCreaturesNoInPokedex { get; set; }
        public double ForceEvolveAbovePercent { get; set; }
        public bool StopOnAPIUpdate { get; set; }
        public int SoftBanBypassTimes { get; set; }

        public int MaxLogs { get; set; }
        public double RunForHours { get; set; }

        //Humanization
        public bool EnableHumanization { get; set; }
        public int InsideReticuleChance { get; set; }

        public int DelayBetweenPlayerActions { get; set; }
        public int PlayerActionDelayRandom { get; set; }

        public int DelayBetweenLocationUpdates { get; set; }
        public int LocationupdateDelayRandom { get; set; }

        public int GeneralDelay { get; set; }
        public int GeneralDelayRandom { get; set; }

        public double WalkingSpeedOffset { get; set; }
        //End Humanization

        public string ProxyIP { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public bool AutoRotateProxies { get; set; }
        public bool AutoRemoveOnStop { get; set; }
        public bool StopOnIPBan { get; set; }
        public int MaxFailBeforeReset { get; set; }
        public bool UseBerries { get; set; }
        public bool OnlyUnlimitedIncubator { get; set; }
        public bool TransferSlashCreatures { get; set; }
        public bool ShufflePokestops { get; set; }
        public bool GetArBonus { get; set; }
        public decimal ARBonusProximity { get; set; }
        public decimal ARBonusAwareness { get; set; }
        public bool CompleteTutorial { get; set; }
        public bool TransferAtOnce { get; set; }
        public bool ShowDebugLogs { get; set; }
        public bool DownloadResources { get; set; }
        public bool RequestFortDetails { get; set; }
        public int BallsToIgnoreStops { get; set; }
        public bool IgnoreStopsIfTooBalls { get; set; }
        public bool UseSoftBanBypass { get; set; }
        public string PGPoolEndpoint { get; set; }
        public bool EnablePGPool { get; set; }


        public AccountState StopAtMinAccountState { get; set; }

        public ProxyEx Proxy
        {
            get
            {
                return new ProxyEx
                {
                    Address = ProxyIP,
                    Port = ProxyPort,
                    Username = ProxyUsername,
                    Password = ProxyPassword
                };
            }
        }

        public List<InventoryItemSetting> ItemSettings { get; set; }
        public List<TransferSetting> TransferSettings { get; set; }
        public List<CatchSetting> CatchSettings { get; set; }
        public List<EvolveSetting> EvolveSettings { get; set; }
        public List<UpgradeSetting> UpgradeSettings { get; set; }

        [JsonConstructor]
        public Settings(bool jsonConstructor = true)
        {
            LoadDefaults();
        }

        public Settings()
        {
            //Defaults
            LoadDefaults();
            RandomizeDevice();
            LoadInventorySettings();
            LoadCatchSettings();
            LoadEvolveSettings();
            LoadTransferSettings();
            LoadUpgradeSettings();
        }

        public void LoadDefaults()
        {
            GroupName = "Default";
            AuthType = AuthType.DEVICE;
            MimicWalking = true;
            CatchCreature = true;
            WalkingSpeed = 9;
            MaxTravelDistance = 30000;
            EncounterWhileWalking = true;
            EnableHumanization = false;
            InsideReticuleChance = 100;
            MinCreatureBeforeEvolve = 0;
            StopAtMinAccountState = AccountState.Unknown;
            DelayBetweenPlayerActions = 500;
            DelayBetweenLocationUpdates = 1000;
            GeneralDelay = 800;
            MaxLogs = 400;
            MaxFailBeforeReset = 3;
            StopOnIPBan = true;
            SearchFortBelowPercent = 1000;
            CatchCreatureDayLimit = 500;
            SpinPokestopsDayLimit = 700;
            ForceEvolveAbovePercent = 1000;
            PercTransItems = 90;
            PercTransPoke = 40;
            StopOnAPIUpdate = true;
            SpinGyms = false;
            Latitude = 40.764665;
            Longitude = -73.973184;
            Country = "US";
            Language = "en";
            TimeZone = "America/New_York";
            POSIX = "en-us";
            DisableCatchDelay = 3;
            DownloadResources = false;
            DefaultTeam = "Neutral";
            ShowDebugLogs = false;
            GoOnlyToGyms = false;
            AutoFavoritShiny = true;
            SnipeAllCreaturesNoInPokedex = false;
            EncounterWhileWalking = true;
            RequestFortDetails = false;
            BallsToIgnoreStops = 80;
            IgnoreStopsIfTooBalls = false;
            MinCreatureBeforeEvolve = 1;
            UseSoftBanBypass = true;
            SoftBanBypassTimes = 40;
            LevelForConstLukky = 9;
            UseLuckEggConst = false;
            UseLuckyEgg = true;
            UseIncense = true;
            MaxPokestopMeters = 100.00;
            MaxPokestopMetersRandom = 50;
            EnablePGPool = false;
            PGPoolEndpoint = "http://127.0.0.1:4242/";
        }

        public void LoadCatchSettings()
        {
            CatchSettings = new List<CatchSetting>();
            /*
            foreach (CreatureId Creature in Enum.GetValues(typeof(CreatureId)))
            {
                if (Creature == CreatureId.Missingno)
                {
                    continue;
                }

                var cSettings = new CatchSetting
                {
                    Id = Creature
                };

                CatchSettings.Add(cSettings);
            }
            */
        }

        public void LoadInventorySettings()
        {
            ItemSettings = new List<InventoryItemSetting>();
            /*
            foreach (ItemId item in Enum.GetValues(typeof(ItemId)))
            {
                if (item == ItemId.ItemUnknown)
                {
                    continue;
                }

                var itemSetting = new InventoryItemSetting
                {
                    Id = item
                };

                ItemSettings.Add(itemSetting);
            }
            */
        }

        public void LoadEvolveSettings()
        {
            EvolveSettings = new List<EvolveSetting>();
            /*
            foreach (CreatureId Creature in Enum.GetValues(typeof(CreatureId)))
            {
                if (Creature == CreatureId.Missingno)
                {
                    continue;
                }

                var setting = new EvolveSetting
                {
                    Id = Creature,
                    Evolve = true
                };

                EvolveSettings.Add(setting);
            }
            */
        }

        public void LoadTransferSettings()
        {
            TransferSettings = new List<TransferSetting>();
            /*
            foreach (CreatureId Creature in Enum.GetValues(typeof(CreatureId)))
            {
                if (Creature == CreatureId.Missingno)
                {
                    continue;
                }

                var setting = new TransferSetting
                {
                    Id = Creature,
                    Transfer = true
                };

                TransferSettings.Add(setting);
            }
            */
        }

        public void LoadUpgradeSettings()
        {
            UpgradeSettings = new List<UpgradeSetting>();
            /*
            foreach (CreatureId Creature in Enum.GetValues(typeof(CreatureId)))
            {
                if (Creature == CreatureId.Missingno)
                {
                    continue;
                }

                var setting = new UpgradeSetting
                {
                    Id = Creature,
                    //Upgrade = true
                };

                UpgradeSettings.Add(setting);
            }
            */
        }

        public void RandomizeDeviceId()
        {
            /*
            var device = DeviceInfoUtil.GetRandomDevice();
            DeviceId = device.DeviceInfo.DeviceId;
            */
        }

        public void RandomizeDevice()
        {
            /*
            var device = DeviceInfoUtil.GetRandomDevice();
            DeviceId = device.DeviceInfo.DeviceId;
            DeviceBrand = device.DeviceInfo.DeviceBrand;
            DeviceModel = device.DeviceInfo.DeviceModel;
            DeviceModelBoot = device.DeviceInfo.DeviceModelBoot;
            HardwareManufacturer = device.DeviceInfo.HardwareManufacturer;
            HardwareModel = device.DeviceInfo.HardwareModel;
            FirmwareBrand = device.DeviceInfo.FirmwareBrand;
            FirmwareType = device.DeviceInfo.FirmwareType;
            */
        }

        private byte RandomByte()
        {
            using (var randomizationProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[1];
                randomizationProvider.GetBytes(randomBytes);
                return randomBytes.Single();
            }
        }
    }
}
