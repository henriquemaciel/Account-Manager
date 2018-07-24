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
            FUpdate map = _client.DracoClient.GetMapUpdate(UserSettings.Latitude, UserSettings.Longitude, (float)UserSettings.HorizontalAccuracy);
            FCreatureUpdate creatures = map.items.Find(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
            //FHatchedEggs hatched = map.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
            //FChestUpdate chests = map.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
            //FBuildingUpdate buildings = map.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;

            if (map.items.Find(o => o.GetType() == typeof(FAvaUpdate)) is FAvaUpdate avatar)
                Stats = avatar;

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

        private async Task<MethodResult<List<FBuilding>>> GetAllFortsAsync()
        {
            FUpdate map = _client.DracoClient.GetMapUpdate(UserSettings.Latitude, UserSettings.Longitude, (float)UserSettings.HorizontalAccuracy);
            //FCreatureUpdate creatures = map.items.Find(o => o.GetType() == typeof(FCreatureUpdate)) as FCreatureUpdate;
            //FHatchedEggs hatched = map.items.Find(o => o.GetType() == typeof(FHatchedEggs)) as FHatchedEggs;
            //FChestUpdate chests = map.items.Find(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
            FBuildingUpdate buildings = map.items.Find(o => o.GetType() == typeof(FBuildingUpdate)) as FBuildingUpdate;

            if (map.items.Find(o => o.GetType() == typeof(FAvaUpdate)) is FAvaUpdate avatar)
                Stats = avatar;

            //var _buildings[];
            var _buildings = buildings.tileBuildings.Values.ToArray().SelectMany(t => t.buildings).ToList();

            if (_buildings.Count() == 0)
            {
                //throw new OperationCanceledException("Not cells.");
            }

            if (!buildings.tileBuildings.Any())
            {
                return new MethodResult<List<FBuilding>>
                {
                    Message = "No buildings data found. Potential temp IP ban or bad location",
                };
            }
            /*
            var fortData = new List<FBuildingUpdate>();

            foreach (var fort in buildings.tileBuildings)
            {
                if (!IsValidLocation(fort.Latitude, fort.Longitude))
                {
                    continue;
                }

                if (fort.CooldownCompleteTimestampMs >= DateTime.UtcNow.ToUnixTime())
                {
                    continue;
                }

                var defaultLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
                var fortLocation = new GeoCoordinate(fort.Latitude, fort.Longitude);

                double distance = CalculateDistanceInMeters(defaultLocation, fortLocation);

                if (distance > UserSettings.MaxTravelDistance)
                {
                    continue;
                }

                fortData.Add(fort);
            }

            if (fortData.Count == 0)
            {
                return new MethodResult<List<FortData>>
                {
                    Message = "No searchable Buildings found within range",
                };
            }

            if (UserSettings.ShuffleBuildings)
            {
                var rnd = new Random();
                fortData = fortData.OrderBy(x => rnd.Next()).ToList();
            }
            else
            {
                fortData = fortData.OrderBy(x => CalculateDistanceInMeters(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude, x.Latitude, x.Longitude)).ToList();
            }
            */
            return await Task.Run(() => new MethodResult<List<FBuilding>>
            {
                Message = "Success",
                Success = true,
                Data = _buildings
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
