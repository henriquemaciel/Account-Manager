using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        public int CalculateDelay(int baseDelay, int offset)
        {
            lock(_rand)
            {
                int maxOffset = offset * 2;

                int currentOffset = _rand.Next(0, maxOffset + 1) - offset;

                int returnDelay = baseDelay + currentOffset;

                //API throttles
                return returnDelay <= 500 ? 500 : returnDelay;
            }
        }

        public MethodResult<AccountExportModel> GetAccountExport()
        {
            if(Stats == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("No stats found for {0}. Please update details", UserSettings.Username), LoggerTypes.Warning));
            }

            if (Items.Count == 0) {
                LogCaller(new LoggerEventArgs(String.Format("No items found for {0}. Please update details", UserSettings.Username), LoggerTypes.Warning));
            }

            if (Pokedex.Count == 0) {
                LogCaller(new LoggerEventArgs(String.Format("No pokedex found for {0}. Please update details", UserSettings.Username), LoggerTypes.Warning));
            }

            var exportModel = new AccountExportModel()
            {
                Level = Stats.level,
                Type = UserSettings.AuthType,
                Username = UserSettings.Username,
                Password = UserSettings.Password,
                Pokedex = Pokedex.Select(x => new PokedexEntryExportModel(x)).ToList(),
                Creature = Creature.Select(x => new CreatureDataExportModel(x, CalculateIVPerfection(x))).ToList(),
                Items = Items.Select(x => new ItemDataExportModel(x)).ToList(),
                Eggs = Eggs.Select(x => new EggDataExportModel(x)).ToList(),
                ExportTime = DateTime.Now
            };

            return new MethodResult<AccountExportModel>
            {
                Data = exportModel,
                Success = true
            };
        }

        private async Task<MethodResult<Dictionary<string, string/*CreatureId, CreatureSettings*/>>> GetItemTemplates()
        {
            /*
            if (PokeSettings != null && PokeSettings.Count != 0)
            {
                return new MethodResult<Dictionary<CreatureId, CreatureSettings>>
                {
                    Data = PokeSettings,
                    Message = "Settings already grabbed",
                    Success = true
                };
            }

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<Dictionary<CreatureId, CreatureSettings>>
                    {
                        Message = result.Message
                    };
                }
            }

            if (_client.ClientSession.Templates.ItemTemplates == null)
                return new MethodResult<Dictionary<CreatureId, CreatureSettings>>
                {
                    Message = "Failed to get setting templates"
                };

            var CreatureSettings = new Dictionary<CreatureId, CreatureSettings>();
            var moveSettings = new Dictionary<CreatureMove, MoveSettings>();
            var badgeSettings = new Dictionary<BadgeType, BadgeSettings>();
            var itemSettings = new Dictionary<ItemId, ItemSettings>();
            var battleSettings = new GymBattleSettings();
            var upgradeSettings = new CreatureUpgradeSettings();
            var moveSequenceSettings = new MoveSequenceSettings();
            var encounterSettings = new EncounterSettings();
            var iapItemDisplay = new IapItemDisplay();
            var iapSettings = new IapSettings();
            var equippedBadge = new EquippedBadgeSettings();
            var questSettings = new QuestSettings();
            var avatarCustomization = new AvatarCustomizationSettings();
            var formSettings = new FormSettings();
            var genderSettings = new GenderSettings();
            var gymBadgeSettings = new GymBadgeGmtSettings();
            var weatherAffinities = new WeatherAffinity();
            var weatherBonusSettings = new WeatherBonus();
            var CreatureScaleSettings = new CreatureScaleSetting();
            var typeEffective = new TypeEffectiveSettings();
            var camera = new CameraSettings();
            var gymLevel = new GymLevelSettings();

            foreach (var template in _client.ClientSession.Templates.ItemTemplates)
            {
                if (template.PlayerLevel != null)
                {
                    LevelSettings = template.PlayerLevel;

                    continue;
                }

                if (template.CreatureSettings != null)
                {
                    if (CreatureSettings.ContainsKey(template.CreatureSettings.CreatureId))
                        CreatureSettings.Remove(template.CreatureSettings.CreatureId);
                    CreatureSettings.Add(template.CreatureSettings.CreatureId, template.CreatureSettings);
                }
                else if (template.MoveSettings != null)
                {
                    if (moveSettings.ContainsKey(template.MoveSettings.MovementId))
                        moveSettings.Remove(template.MoveSettings.MovementId);
                    moveSettings.Add(template.MoveSettings.MovementId, template.MoveSettings);
                }
                else if (template.BadgeSettings != null)
                {
                    if (badgeSettings.ContainsKey(template.BadgeSettings.BadgeType))
                        badgeSettings.Remove(template.BadgeSettings.BadgeType);
                    badgeSettings.Add(template.BadgeSettings.BadgeType, template.BadgeSettings);
                }
                else if (template.ItemSettings != null)
                {
                    if (itemSettings.ContainsKey(template.ItemSettings.ItemId))
                        itemSettings.Remove(template.ItemSettings.ItemId);
                    itemSettings.Add(template.ItemSettings.ItemId, template.ItemSettings);
                }
                else if (template.EncounterSettings != null)
                {
                    encounterSettings = template.EncounterSettings;
                }
                else if (template.MoveSequenceSettings != null)
                {
                    moveSequenceSettings = template.MoveSequenceSettings;
                }
                else if (template.BattleSettings != null)
                {
                    battleSettings = template.BattleSettings;
                }
                else if (template.IapItemDisplay != null)
                {
                    iapItemDisplay = template.IapItemDisplay;
                }
                else if (template.IapSettings != null)
                {
                    iapSettings = template.IapSettings;
                }
                else if (template.EquippedBadges != null)
                {
                    equippedBadge = template.EquippedBadges;
                }
                else if (template.QuestSettings != null)
                {
                    questSettings = template.QuestSettings;
                }
                else if (template.AvatarCustomization != null)
                {
                    avatarCustomization = template.AvatarCustomization;
                }
                else if (template.FormSettings != null)
                {
                    formSettings = template.FormSettings;
                }
                else if (template.GenderSettings != null)
                {
                    genderSettings = template.GenderSettings;
                }
                else if (template.GymBadgeSettings != null)
                {
                    gymBadgeSettings = template.GymBadgeSettings;
                }
                else if (template.WeatherAffinities != null)
                {
                    weatherAffinities = template.WeatherAffinities;
                }
                else if (template.WeatherBonusSettings != null)
                {
                    weatherBonusSettings = template.WeatherBonusSettings;
                }
                else if (template.CreatureScaleSettings != null)
                {
                    CreatureScaleSettings = template.CreatureScaleSettings;
                }
                else if (template.TypeEffective != null)
                {
                    typeEffective = template.TypeEffective;
                }
                else if (template.Camera != null)
                {
                    camera = template.Camera;
                }
                else if (template.GymLevel != null)
                {
                    gymLevel = template.GymLevel;
                }
                else if (template.CreatureUpgrades != null)
                {
                    upgradeSettings = template.CreatureUpgrades;
                }
            }

            PokeSettings = CreatureSettings;
            MoveSettings = moveSettings;
            BadgeSettings = badgeSettings;
            ItemSettings = itemSettings;
            BadgeSettings = badgeSettings;
            UpgradeSettings = upgradeSettings;
            GetMoveSequenceSettings = moveSequenceSettings;
            GetEncounterSettings = encounterSettings;
            GetIapItemDisplay = iapItemDisplay;
            GetIapSettings = iapSettings;
            GetEquippedBadgeSettings = equippedBadge;
            GetQuestSettings = questSettings;
            GetAvatarCustomizationSettings = avatarCustomization;
            GetFormSettings = formSettings;
            GetGenderSettings = genderSettings;
            GetGymBadgeGmtSettings = gymBadgeSettings;
            GetWeatherAffinity = weatherAffinities;
            GetWeatherBonus = weatherBonusSettings;
            GetCreatureScaleSetting = CreatureScaleSettings;
            GetTypeEffectiveSettings = typeEffective;
            GetCameraSettings = camera;
            GetGymLevelSettings = gymLevel;
            */
            return new MethodResult<Dictionary<string, string /*CreatureId, CreatureSettings*/>>
            {
                //Data = CreatureSettings,
                Message = "Success",
                Success = true
            };
        }

        public MethodResult<string/*CreatureSettings*/> GetCreatureSetting(/*CreatureId Creature*/)
        {
            /*
            if(PokeSettings == null)
            {
                return new MethodResult<CreatureSettings>
                {
                    Message = "Settings haven't been grabbed yet"
                };
            }

            if (Creature == CreatureId.Missingno)
            {
                return new MethodResult<CreatureSettings>
                {
                    Message = "No settings on Missingno"
                };
            }

            //Shouldn't happen
            CreatureSettings CreatureSettings;
            if (!PokeSettings.TryGetValue(Creature, out  CreatureSettings))
            {
                return new MethodResult<CreatureSettings>()
                {
                    Message = "Creature Id does not exist"
                };
            }
            */
            return new MethodResult<string/*CreatureSettings*/>()
            {
                //Data = CreatureSettings,
                Message = "Success",
                Success = true
            };
        }

        public async Task<MethodResult> ExportConfig(string filename, bool fullconfig)// = false)
        {           
            try
            {
                Settings userSettings = UserSettings;
                if (!fullconfig)
                {
                    userSettings.AccountName = String.Empty;
                    userSettings.Password = String.Empty;
                    userSettings.Username = String.Empty;
                    userSettings.AutoRefreshMap = true;

                    // gyms
                    userSettings.DefaultTeam = "Neutral";
                    userSettings.SpinGyms = false;
                    userSettings.GoOnlyToGyms = false;
                    userSettings.DeployCreature = false;
                    //
                }

                string data = JsonConvert.SerializeObject(userSettings, Formatting.Indented);

                await Task.Run(() => File.WriteAllText(filename, data));

                LogCaller(new LoggerEventArgs("Successfully exported config file", LoggerTypes.Info));

                return new MethodResult
                {
                    Success = true
                };               
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to export config file", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = ex.Message
                };
            }
        }

        public MethodResult ImportConfig(string data)
        {
            try
            {
                List<Manager> managers = Serializer.FromJson<List<Manager>>(data) ?? new List<Manager> { new Manager { UserSettings = Serializer.FromJson<Settings>(data) } };
                foreach (Manager manager in managers)
                {
                    Settings settings = manager.UserSettings;
                    settings.AccountName = UserSettings.AccountName;
                    settings.Password = UserSettings.Password;
                    settings.Username = UserSettings.Username;
                    settings.AuthType = UserSettings.AuthType;
                    settings.ProxyIP = UserSettings.ProxyIP;
                    settings.ProxyPassword = UserSettings.ProxyPassword;
                    settings.ProxyPort = UserSettings.ProxyPort;
                    settings.ProxyUsername = UserSettings.ProxyUsername;
                    settings.GroupName = UserSettings.GroupName;

                    /*/Randomize device id
                    var device = DeviceInfoUtil.GetRandomDevice();
                    settings.DeviceId = device.DeviceInfo.DeviceId;
                    settings.DeviceBrand = device.DeviceInfo.DeviceBrand;
                    settings.DeviceModel = device.DeviceInfo.DeviceModel;
                    settings.DeviceModelBoot = device.DeviceInfo.DeviceModelBoot;
                    settings.HardwareManufacturer = device.DeviceInfo.HardwareManufacturer;
                    settings.HardwareModel = device.DeviceInfo.HardwareModel;
                    settings.FirmwareBrand = device.DeviceInfo.FirmwareBrand;
                    settings.FirmwareType = device.DeviceInfo.FirmwareType;
                    */
                    UserSettings = settings;

                    if (String.IsNullOrEmpty(UserSettings.DeviceBrand))
                    {
                        UserSettings.RandomizeDevice();
                    }
                }
                /*
                Settings settings = Serializer.FromJson<Settings>(data);
                settings.AccountName = UserSettings.AccountName;
                settings.Password = UserSettings.Password;
                settings.Username = UserSettings.Username;
                settings.AuthType = UserSettings.AuthType;
                settings.ProxyIP = UserSettings.ProxyIP;
                settings.ProxyPassword = UserSettings.ProxyPassword;
                settings.ProxyPort = UserSettings.ProxyPort;
                settings.ProxyUsername = UserSettings.ProxyUsername;
                settings.GroupName = UserSettings.GroupName;

                //Randomize device id
                var device = DeviceInfoUtil.GetRandomDevice();
                settings.DeviceId = device.DeviceInfo.DeviceId;
                settings.DeviceBrand = device.DeviceInfo.DeviceBrand;
                settings.DeviceModel = device.DeviceInfo.DeviceModel;
                settings.DeviceModelBoot = device.DeviceInfo.DeviceModelBoot;
                settings.HardwareManufacturer = device.DeviceInfo.HardwareManufacturer;
                settings.HardwareModel = device.DeviceInfo.HardwareModel;
                settings.FirmwareBrand = device.DeviceInfo.FirmwareBrand;
                settings.FirmwareType = device.DeviceInfo.FirmwareType;
                */

                LogCaller(new LoggerEventArgs("Successfully imported config file", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch (Exception ex)
            {
 
                string message = String.Format("Failed to import config. Ex: {0}", ex.Message);

                LogCaller(new LoggerEventArgs(message, LoggerTypes.Exception, ex));

                 return new MethodResult
                 {
                    Message = message
                 };
            }
        }

        public async Task<MethodResult> ImportConfigFromFile(string fileName)
        {
            try
            {
                if(!File.Exists(fileName))
                {
                    return new MethodResult
                    {
                        Message = "File does not exist"
                    };
                }

                string data = await Task.Run(() => File.ReadAllText(fileName));

                return ImportConfig(data);
            }
            catch(Exception ex)
            {
                string message = String.Format("Failed to load config file.");

                LogCaller(new LoggerEventArgs(message, LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = message
                };
            }
        }

        public void RestoreUpgradeDefaults()
        {
            UserSettings.LoadUpgradeSettings();
        }

        public void RestoreCatchDefaults()
        {
            UserSettings.LoadCatchSettings();
        }

        public void RestoreInventoryDefaults()
        {
            UserSettings.LoadInventorySettings();
        }

        public void RestoreEvolveDefaults()
        {
            UserSettings.LoadEvolveSettings();
        }

        public void RestoreTransferDefaults()
        {
            UserSettings.LoadTransferSettings();
        }


        public void RandomDeviceId()
        {
            UserSettings.RandomizeDeviceId();
        }

        public void RandomDevice()
        {
            UserSettings.RandomizeDevice();
        }
        
        public override bool Equals(object obj)
        {
            var tempManager = obj as Manager;

            if(tempManager == null)
            {
                return base.Equals(obj);
            }

            return tempManager.UserSettings.Username == this.UserSettings.Username;
        }

        public override int GetHashCode()
        {
            return UserSettings.Username == null ? 0 : UserSettings.Username.GetHashCode();
        }
    }
}
