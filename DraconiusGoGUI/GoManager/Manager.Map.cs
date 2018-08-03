﻿using DracoLib.Core.Exceptions;
using DracoLib.Core.Extensions;
using DraconiusGoGUI.Extensions;
using DracoProtos.Core.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        private FUpdate UserMap { get; set; }

        private async Task<MethodResult<List<FWildCreature>>> GetCatchableCreatureAsync()
        {
            return await Task.Run(async () =>
            {
                //This is not good but refresh map at faster change pos, need maybe other solution
                if (!UserSettings.MimicWalking)
                    UserMap = await Task.Run(() => _client.DracoClient.GetMapUpdate(UserSettings.Latitude, UserSettings.Longitude, (float)UserSettings.HorizontalAccuracy));

                if (UserMap == null || UserMap.items.Count == 0)
                    return new MethodResult<List<FWildCreature>>();

                var creatures = UserMap.items.FirstOrDefault(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
                //FHatchedEggs hatched = map.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
                //FChestUpdate chests = map.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
                //FBuildingUpdate buildings = map.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;
                FAvaUpdate avatar = UserMap.items.Find(o => o.GetType() == typeof(FAvaUpdate)) as FAvaUpdate;
                FUserInfo playerdata = UserMap.items.Find(o => o.GetType() == typeof(FUserInfo)) as FUserInfo;

                if (avatar != null)
                    Stats = avatar;

                if (playerdata != null)
                    PlayerData = playerdata;

                if (creatures == null && creatures.wilds.Count == 0)
                {
                    return new MethodResult<List<FWildCreature>>
                    {
                        Data = new List<FWildCreature>(),
                        Message = "No Creatures Found",
                        Success = false
                    };
                }

                return new MethodResult<List<FWildCreature>>
                {
                    Data = creatures.wilds,
                    Success = true,
                    Message = "Success"
                };
            });
        }

        private async Task<MethodResult<List<FBuilding>>> GetAllBuildingsAsync()
        {
            return await Task.Run(() =>
            {
                //if (!UserSettings.MimicWalking)
                //   UserMap = await Task.Run(() => _client.DracoClient.GetMapUpdate(UserSettings.Latitude, UserSettings.Longitude, (float)UserSettings.HorizontalAccuracy));

                if (UserMap == null || UserMap.items == null || UserMap.items.Count == 0)
                    return new MethodResult<List<FBuilding>>();

                //FCreatureUpdate creatures = map.items.Find(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
                //FHatchedEggs hatched = map.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
                //FChestUpdate chests = map.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
                FBuildingUpdate buildings = UserMap.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;
                FAvaUpdate avatar = UserMap.items.Find(o => o.GetType() == typeof(FAvaUpdate)) as FAvaUpdate;
                FUserInfo playerdata = UserMap.items.Find(o => o.GetType() == typeof(FUserInfo)) as FUserInfo;

                if (!buildings.tileBuildings.Any())
                {
                    return new MethodResult<List<FBuilding>>
                    {
                        Message = "No buildings data found. Potential temp IP ban or bad location",
                    };
                }

                if (avatar != null)
                    Stats = avatar;

                if (playerdata != null)
                    PlayerData = playerdata;

                var ALLBuildings = buildings.tileBuildings.Values.ToArray().SelectMany(t => t.buildings).ToList();

                if (!ALLBuildings.Any())
                {
                    throw new DracoError("Not Buildings.");
                }

                var BuildingData = new List<FBuilding>();

                foreach (var Building in ALLBuildings)
                {
                    if (!IsValidLocation(Building.coords.latitude, Building.coords.longitude))
                    {
                        continue;
                    }

                    if (Building.pitstop != null && Building.pitstop.cooldown)
                    {
                        continue;
                    }

                    var defaultLocation = new GeoCoordinate(UserSettings.Latitude, UserSettings.Longitude);
                    var BuildingLocation = new GeoCoordinate(Building.coords.latitude, Building.coords.longitude);

                    double distance = CalculateDistanceInMeters(defaultLocation, BuildingLocation);

                    if (distance > UserSettings.MaxTravelDistance)
                    {
                        continue;
                    }

                    BuildingData.Add(Building);
                }

                if (!BuildingData.Any())
                {
                    throw new DracoError("Not Buildings.");
                }

                return new MethodResult<List<FBuilding>>
                {
                    Message = "Success",
                    Success = true,
                    Data = BuildingData
                };
            });
        }

        private async Task<MethodResult<List<FChest>>> GetAllChestsInRangeAsync()
        {
            return await Task.Run(() =>
            {
                //if (!UserSettings.MimicWalking)
                //    UserMap = await Task.Run(() => _client.DracoClient.GetMapUpdate(UserSettings.Latitude, UserSettings.Longitude, (float)UserSettings.HorizontalAccuracy));

                if (UserMap == null || UserMap.items.Count == 0)
                    return new MethodResult<List<FChest>>();

                //FCreatureUpdate creatures = map.items.Find(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
                //FHatchedEggs hatched = map.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
                //FBuildingUpdate buildings = map.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;
                FChestUpdate chests = UserMap.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
                FAvaUpdate avatar = UserMap.items.Find(o => o.GetType() == typeof(FAvaUpdate)) as FAvaUpdate;
                FUserInfo playerdata = UserMap.items.Find(o => o.GetType() == typeof(FUserInfo)) as FUserInfo;

                if (avatar != null)
                    Stats = avatar;

                if (playerdata != null)
                    PlayerData = playerdata;

                if (chests.chests.Any())
                {
                    return new MethodResult<List<FChest>>
                    {
                        Message = "No chests found in range.",
                    };
                }

                var _chests = chests.chests.Where(x => x.coords.distanceTo(new GeoCoords { latitude = UserSettings.Latitude, longitude = UserSettings.Longitude }) < 20);

                if (_chests.Count() == 0)
                {
                    return new MethodResult<List<FChest>>
                    {
                        Message = "No chests found in range.",
                    };
                }

                LogCaller(new LoggerEventArgs("visible chests: " + chests.chests.Count, Enums.LoggerTypes.Debug));
                LogCaller(new LoggerEventArgs("in range chests: " + _chests.Count(), Enums.LoggerTypes.Debug));

                return new MethodResult<List<FChest>>
                {
                    Message = "Success",
                    Success = true,
                    Data = _chests.ToList()
                };
            });
        }

        /*
        private async Task<MethodResult<MapCreature>> GetIncenseCreatures()
        {
            if (!_client.ClientSession.IncenseUsed)
            {
                if (UserSettings.UseIncense)
                {
                    var incenses = Items.Where(x => x.ItemId == ItemId.ItemIncenseOrdinary
                    || x.ItemId == ItemId.ItemIncenseCool
                    || x.ItemId == ItemId.ItemIncenseFloral
                    || x.ItemId == ItemId.ItemIncenseSpicy
                    );

                    if (incenses.Count() > 0)
                    {
                        await UseIncense(incenses.FirstOrDefault().ItemId);

                        if (_client.ClientSession.Map.IncenseCreature != null)
                        {
                            return new MethodResult<MapCreature>
                            {
                                Data = _client.ClientSession.Map.IncenseCreature,
                                Success = true,
                                Message = "Succes"
                            };
                        }
                    }
                }
            }

            if (_client.ClientSession.Map.IncenseCreature != null)
            {
                return new MethodResult<MapCreature>
                {
                    Data = _client.ClientSession.Map.IncenseCreature,
                    Success = true,
                    Message = "Succes"
                };
            }
            return new MethodResult<MapCreature>();
        }
        */
    }
}
