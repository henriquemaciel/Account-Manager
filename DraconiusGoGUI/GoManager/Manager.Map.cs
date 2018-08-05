using DracoLib.Core.Exceptions;
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
        private List<FWildCreature> CatchableCreatures { get; set; } = new List<FWildCreature>();
        private List<FBuilding> AllBuildings { get; set; } = new List<FBuilding>();
        private List<FChest> AllChests { get; set; } = new List<FChest>();
        //private List<FHatchedEggs> HatchedEggs { get; set; } = new List<FHatchedEggs>();

        private async Task<MethodResult<bool>> UpdateMap(double lat, double lng, float horizontalAcc)
        {
            FUpdate UserMap = await Task.Run(() => _client.DracoClient.GetMapUpdate(lat, lng, horizontalAcc));

            if (UserMap == null || UserMap.items.Count == 0)
            {
                return new MethodResult<bool>
                {
                    Success = false,
                    Message = "Unable to set player position."
                };
            }

            FCreatureUpdate creatures = UserMap.items.Find(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
            FHatchedEggs hatched = UserMap.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
            FChestUpdate chests = UserMap.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
            FBuildingUpdate buildings = UserMap.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;
            FAvaUpdate avatar = UserMap.items.Find(o => o.GetType() == typeof(FAvaUpdate)) as FAvaUpdate;
            FUserInfo playerdata = UserMap.items.Find(o => o.GetType() == typeof(FUserInfo)) as FUserInfo;

            if (avatar != null)
                Stats = avatar;

            if (playerdata != null)
                PlayerData = playerdata;

            if (creatures != null && creatures.wilds.Count > 0)
            {
                CatchableCreatures = creatures.wilds;
            }

            if (buildings.tileBuildings.Any())
            {
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
                // Not need sort the buildings, will be sorted after.
                AllBuildings = BuildingData;
            }

            if (chests.chests.Any())
            {
                //We need all visible chests (full list, with or without vision activated) we will goint to the exact point of each chest to open it, over there.
                if (chests.chests.Count() > 0)
                {
                    LogCaller(new LoggerEventArgs("visible chests: " + chests.chests.Count, Enums.LoggerTypes.Debug));
                    AllChests = chests.chests.OrderBy(x => x.coords.distanceTo(new GeoCoords { latitude = lat, longitude = lng })).ToList();
                }
            }

            if (hatched != null)
            {
                if (hatched.egg.isHatching)
                    LogCaller(new LoggerEventArgs("Hatched Eggs: " + hatched.egg.eggType, Enums.LoggerTypes.Success));
            }

            return new MethodResult<bool>
            {
                Message = "Success",
                Success = true
            };
        } 

        private MethodResult<List<FBuilding>> GetAllBuildings()
        {
            if (AllBuildings == null || AllBuildings.Count == 0)
            {
                return new MethodResult<List<FBuilding>>
                {
                    Message = "No buildings data found. Potential temp IP ban or bad location."
                };
            }

            return new MethodResult<List<FBuilding>>
            {
                Data = AllBuildings,
                Success = true
            };
        }

        private async Task<MethodResult<List<FWildCreature>>> GetCatchableCreatures()
        {
            if (!UserSettings.MimicWalking)
                await UpdateMap(UserSettings.Latitude, UserSettings.Longitude, (float)UserSettings.HorizontalAccuracy);

            if (CatchableCreatures == null || CatchableCreatures.Count == 0)
            {
                return new MethodResult<List<FWildCreature>>
                {
                    Message = "No Creatures Found."
                };
            }

            return new MethodResult<List<FWildCreature>>
            {
                Data = CatchableCreatures,
                Success = true
            };
        }

        private async Task<MethodResult<List<FChest>>> GetAllChests()
        {
            if (!UserSettings.MimicWalking)
                await UpdateMap(UserSettings.Latitude, UserSettings.Longitude, (float)UserSettings.HorizontalAccuracy);

            if (AllChests == null || AllChests.Count == 0)
            {
                return new MethodResult<List<FChest>>
                {
                    Message = "No chests found in range."
                };
            }

            return new MethodResult<List<FChest>>
            {
                Data = AllChests,
                Success = true
            };
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
