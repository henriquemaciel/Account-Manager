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
        private async Task<MethodResult<List<FWildCreature>>> GetCatchableCreatureAsync()
        {
            FUpdate map = _client.DracoClient.GetMapUpdate();
            FCreatureUpdate creatures = map.items.Find(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
            //FHatchedEggs hatched = map.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
            //FChestUpdate chests = map.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
            //FBuildingUpdate buildings = map.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;
            FAvaUpdate avatar = map.items.Find(o => o.GetType() == typeof(FAvaUpdate)) as FAvaUpdate;
            FUserInfo playerdata = map.items.Find(o => o.GetType() == typeof(FUserInfo)) as FUserInfo;

            if (avatar != null)
                Stats = avatar;

            if (playerdata != null)
                PlayerData = playerdata;

            if (creatures.wilds.Count == 0)
            {
                //throw new OperationCanceledException("Not cells.");
            }

            return await Task.Run(() => new MethodResult<List<FWildCreature>>
            {
                Data = creatures.wilds,
                Success = true,
                Message = "Success"
            });
        }

        private async Task<MethodResult<List<FBuilding>>> GetAllBuildingsAsync()
        {
            FUpdate map = _client.DracoClient.GetMapUpdate();
            //FCreatureUpdate creatures = map.items.Find(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
            //FHatchedEggs hatched = map.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
            //FChestUpdate chests = map.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
            FBuildingUpdate buildings = map.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;
            FAvaUpdate avatar = map.items.Find(o => o.GetType() == typeof(FAvaUpdate)) as FAvaUpdate;
            FUserInfo playerdata = map.items.Find(o => o.GetType() == typeof(FUserInfo)) as FUserInfo;

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

            //var _buildings[];
            var ALLBuildings = buildings.tileBuildings.Values.ToArray().SelectMany(t => t.buildings).ToList();

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

            if (BuildingData.Count() == 0)
            {
                throw new DracoError("Not Buildings.");
            }

            return await Task.Run(() => new MethodResult<List<FBuilding>>
            {
                Message = "Success",
                Success = true,
                Data = BuildingData
            });
        }

        private async Task<MethodResult<List<FChest>>> GetAllChestsInRangeAsync()
        {
            FUpdate map = _client.DracoClient.GetMapUpdate();
            //FCreatureUpdate creatures = map.items.Find(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
            //FHatchedEggs hatched = map.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
            //FBuildingUpdate buildings = map.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;
            FChestUpdate chests = map.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
            FAvaUpdate avatar = map.items.Find(o => o.GetType() == typeof(FAvaUpdate)) as FAvaUpdate;
            FUserInfo playerdata = map.items.Find(o => o.GetType() == typeof(FUserInfo)) as FUserInfo;

            if (avatar != null)
                Stats = avatar;

            if (playerdata != null)
                PlayerData = playerdata;

            if (!chests.chests.Any())
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

            return await Task.Run(() => new MethodResult<List<FChest>>
            {
                Message = "Success",
                Success = true,
                Data = _chests.ToList()
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
