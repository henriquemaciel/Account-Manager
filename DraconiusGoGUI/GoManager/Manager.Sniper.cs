namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        /*
        private ulong _lastPokeSniperId  = 0;

        private bool AlreadySnipped { get; set; } = false;

        private MethodResult<List<NearbyCreature>> RequestPokeSniperRares()
        {
            if (_client.ClientSession.Map.Cells.Count == 0 || _client.ClientSession.Map == null)
            {
                return new MethodResult<List<NearbyCreature>>();
            }

            var cells = _client.ClientSession.Map.Cells;
            List<NearbyCreature> newNearbyCreatures = cells.SelectMany(x => x.NearbyCreatures).ToList();

            if (newNearbyCreatures.Count == 0)
            {
                return new MethodResult<List<NearbyCreature>>
                {
                    Message = "No Creature found"
                };
            }

            return new MethodResult<List<NearbyCreature>>
            {
                Data = newNearbyCreatures,
                Success = true
            };
        }

        public async Task<MethodResult> SnipeAllNearyCreature()
        {
            if (!UserSettings.CatchCreature)
            {
                return new MethodResult
                {
                    Message = "Catching Creature disabled"
                };
            }

            MethodResult<List<NearbyCreature>> pokeSniperResult = RequestPokeSniperRares();

            if (!pokeSniperResult.Success)
            {
                return new MethodResult
                {
                    Message = pokeSniperResult.Message
                };
            }

            //Priorise snipe...
            /*if (Tracker.CreatureCaught >= UserSettings.CatchCreatureDayLimit)
            {
                LogCaller(new LoggerEventArgs("Catch Creature limit actived", LoggerTypes.Info));
                return new MethodResult
                {
                    Message = "Limit actived"
                };
            }*/
            /*
            List<NearbyCreature> CreatureToSnipe = pokeSniperResult.Data.Where(x => x.EncounterId != _lastPokeSniperId && UserSettings.CatchSettings.FirstOrDefault(p => p.Id == x.CreatureId).Snipe && x.DistanceInMeters < UserSettings.MaxTravelDistance && !LastedEncountersIds.Contains(x.EncounterId)).OrderBy(x => x.DistanceInMeters).ToList();

            if (UserSettings.SnipeAllCreaturesNoInPokedex)
            {
                LogCaller(new LoggerEventArgs("Search Creatures no into pokedex ...", LoggerTypes.Info));
                var ids = Pokedex.Select(x => x.CreatureId);
                CreatureToSnipe = pokeSniperResult.Data.Where(x => x.EncounterId != _lastPokeSniperId && !ids.Contains(x.CreatureId) && x.DistanceInMeters < UserSettings.MaxTravelDistance && !LastedEncountersIds.Contains(x.EncounterId)).OrderBy(x => x.DistanceInMeters).ToList();
            }

            if (CreatureToSnipe.Count == 0)
            {
                return new MethodResult
                {
                    Message = "No catchable Creature found"
                };
            }

            LogCaller(new LoggerEventArgs(String.Format("Sniping {0} Creature", CreatureToSnipe.Count), LoggerTypes.Snipe));

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

            //Long running, so can't let this continue
            try
            {
                while (CreatureToSnipe.Any() && IsRunning && !AlreadySnipped)
                {
                    AlreadySnipped = false;

                    NearbyCreature nearbyCreature = CreatureToSnipe.First();
                    CreatureToSnipe.Remove(nearbyCreature);

                    var Buildings = _client.ClientSession.Map.Cells.SelectMany(x => x.Buildings);
                    var BuildingNearby = Buildings.Where(x => x.Id == nearbyCreature.BuildingId).FirstOrDefault();

                    if (BuildingNearby == null || nearbyCreature == null || nearbyCreature.CreatureId == CreatureId.Missingno)
                    {
                        continue;
                    }

                    GeoCoordinate coords = new GeoCoordinate
                    {
                        Latitude = BuildingNearby.Latitude,
                        Longitude = BuildingNearby.Longitude
                    };

                    await CaptureSnipeCreature(coords.Latitude, coords.Longitude, nearbyCreature.CreatureId);

                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                    CreatureToSnipe = CreatureToSnipe.Where(x => UserSettings.CatchSettings.FirstOrDefault(p => p.Id == x.CreatureId).Snipe && BuildingNearby.CooldownCompleteTimestampMs >= DateTime.Now.AddSeconds(30).ToUnixTime() && !LastedEncountersIds.Contains(x.EncounterId) && IsValidLocation(BuildingNearby.Latitude, BuildingNearby.Longitude)).OrderBy(x => x.DistanceInMeters).ToList();

                    if (UserSettings.SnipeAllCreaturesNoInPokedex)
                    {
                        LogCaller(new LoggerEventArgs("Search Creatures no into pokedex ...", LoggerTypes.Debug));

                        var ids = Pokedex.Select(x => x.CreatureId);
                        CreatureToSnipe = CreatureToSnipe.Where(x => x.EncounterId != _lastPokeSniperId && !ids.Contains(x.CreatureId)).OrderBy(x => x.DistanceInMeters).ToList();

                        if (CreatureToSnipe.Count > 0)
                            LogCaller(new LoggerEventArgs("Found Creatures no into pokedex, go to sniping ...", LoggerTypes.Debug));
                    }
                }

                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Error, sniping ...", LoggerTypes.Warning, ex));
            }
            finally
            {
                AlreadySnipped = false;
            }

            return new MethodResult();
       }

        private async Task<MethodResult> CaptureSnipeCreature(double latitude, double longitude, CreatureId Creature)
        {
            var currentLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude);
            var BuildingLocation = new GeoCoordinate(latitude, longitude);

            double distance = CalculateDistanceInMeters(currentLocation, BuildingLocation);
            LogCaller(new LoggerEventArgs(String.Format("Going to sniping {0} at location {1}, {2}. Distance {3:0.00}m", Creature, latitude, longitude, distance), LoggerTypes.Snipe));

            // Not nedded this runs on local pos.../
            //GeoCoordinate originalLocation = new GeoCoordinate(_client.ClientSession.Player.Latitude, _client.ClientSession.Player.Longitude, _client.ClientSession.Player.Altitude);

            //Update location           
            //MethodResult result = await UpdateLocation(new GeoCoordinate(latitude, longitude));

            //Update location           
            MethodResult result = await GoToLocation(new GeoCoordinate(latitude, longitude));           

            if(!result.Success)
            {
                return result;
            }

            if (UserSettings.UsePOGOLibHeartbeat)
                await Task.Delay(10000); //wait for pogolib refreshmapobjects

            //Get catchable Creature

            MethodResult<List<MapCreature>> CreatureResult = await GetCatchableCreatureAsync();

            if(!CreatureResult.Success)
            {
                return new MethodResult
                {
                    Message = CreatureResult.Message
                };
            }

            if (CreatureResult.Data == null || CreatureResult.Data.Count == 0)
                return new MethodResult();

            MapCreature CreatureToSnipe = CreatureResult.Data.FirstOrDefault(x => x.CreatureId == Creature);

            //Encounter
            MethodResult<EncounterResponse> eResponseResult = await EncounterCreature(CreatureToSnipe);

            if (!eResponseResult.Success)
            {
                //LogCaller(new LoggerEventArgs(String.Format("Snipe failed to encounter Creature {0}. Going back to original location, or already catched", Creature), LoggerTypes.Info));
                LogCaller(new LoggerEventArgs(String.Format("Snipe failed to encounter Creature {0}, or already catched", Creature), LoggerTypes.Info));

                //Failed, update location back
                //await UpdateLocation(originalLocation);

                // Not nedded this runs on local pos.../
                //await GoToLocation(originalLocation);

                return new MethodResult
                {
                    Message = eResponseResult.Message
                };
            }

            if (eResponseResult.Data == null || eResponseResult.Data.WildCreature.CreatureData.CreatureId == CreatureId.Missingno)
                return new MethodResult();

            //Update location back
            //MethodResult locationResult = await RepeatAction(() => UpdateLocation(originalLocation), 2);

            // Not nedded this runs on local pos.../
            /*MethodResult locationResult = await RepeatAction(() => GoToLocation(originalLocation), 2);

            if (!locationResult.Success)
            {
                return locationResult;
            }
            */

            //_lastPokeSniperId = CreatureToSnipe.EncounterId;

            /*
            //Catch Creature
            MethodResult catchResult = await CatchCreature(eResponseResult.Data, CreatureToSnipe, true); //Handles logging

            if (catchResult.Success)
            {
                AlreadySnipped = true;                
            }

            return catchResult;
        }

        private async Task<MethodResult> RepeatAction(Func<Task<MethodResult>> action, int tries)
        {
            MethodResult result = new MethodResult();

            for (int i = 0; i < tries; i++)
            {
                result = await action();

                if (result.Success)
                {
                    return result;
                }

                await Task.Delay(1000);
            }

            return result;
        }

        private async Task<MethodResult<T>> RepeatAction<T>(Func<Task<MethodResult<T>>> action, int tries)
        {
            MethodResult<T> result = new MethodResult<T>();

            for (int i = 0; i < tries; i++)
            {
                result = await action();

                if (result.Success)
                {
                    return result;
                }

                await Task.Delay(1000);
            }

            return result;
        }
        */
    }
}
