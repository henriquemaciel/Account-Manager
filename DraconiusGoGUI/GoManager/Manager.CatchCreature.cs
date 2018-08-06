using System.Globalization;
using DraconiusGoGUI.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DraconiusGoGUI.Enums;
using DracoProtos.Core.Objects;
using System.Linq;
using DracoProtos.Core.Base;
using DraconiusGoGUI.Models;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        List<ulong> LastedEncountersIds = new List<ulong>();

        private async Task<MethodResult> CatchInsenceCreature()
        {
            if (!UserSettings.CatchCreature)
            {
                return new MethodResult
                {
                    Message = "Catching Creature disabled"
                };
            }

            if (Tracker.CreatureCaught >= UserSettings.CatchCreatureDayLimit)
            {
                LogCaller(new LoggerEventArgs("Catch Creature limit actived", LoggerTypes.Info));
                return new MethodResult
                {
                    Message = "Limit actived"
                };
            }

            if (FilledCreatureStorage() >= 100)
            {
                return new MethodResult
                {
                    Message = "Creature Inventory Full."
                };
            }

            if (!CatchDisabled)
            {
                if (RemainingPokeballs() < 1)
                {
                    LogCaller(new LoggerEventArgs("You don't have any pokeball catching Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                    CatchDisabled = true;
                    TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                    return new MethodResult();
                }
            }
            else
                // Need suite
                return new MethodResult();

            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

            return new MethodResult
            {
                Success = true
            };
        }

        public void RemoveMapCreature(FWildCreature wildCreature)
        {
            if (CatchableCreatures == null || CatchableCreatures.Count == 0)
                return;

                CatchableCreatures.Remove(wildCreature);
        }

        private async Task<MethodResult> CatchNeabyCreature()
        {
            if (!UserSettings.CatchCreature)
            {
                return new MethodResult
                {
                    Message = "Catching Creature disabled"
                };
            }

            if (Tracker.CreatureCaught >= UserSettings.CatchCreatureDayLimit)
            {
                LogCaller(new LoggerEventArgs("Catch Creature limit actived", LoggerTypes.Info));
                return new MethodResult
                {
                    Message = "Limit actived"
                };
            }

            if (FilledCreatureStorage() >= 100)
            {
                return new MethodResult
                {
                    Message = "Creature Inventory Full."
                };
            }

            MethodResult<List<FWildCreature>> catchableResponse = await GetCatchableCreatures();

            if (!catchableResponse.Success || catchableResponse.Data == null || catchableResponse.Data.Count <= 0)
            {
                return new MethodResult();
            }

            string creatures = null;

            foreach (var creature in catchableResponse.Data)
            {
                if (String.IsNullOrEmpty(creatures))
                    creatures = Strings.GetCreatureName(creature.name);
                else
                    creatures = creatures + " " + Strings.GetCreatureName(creature.name);
            }

            LogCaller(new LoggerEventArgs($"{catchableResponse.Data.Count} Creatures Found: " + creatures, LoggerTypes.Info));

            // NOTE: this toArray() force a new list object, this is needed because the real list changes at remove an element and breaks the loop
            foreach (var Creature in catchableResponse.Data.Where(x => x != null).ToArray())
            {
                if (Creature == null)
                {
                    LogCaller(new LoggerEventArgs("Creature is null. Ignoring", LoggerTypes.Debug));
                    return new MethodResult();
                }
                LogCaller(new LoggerEventArgs($"Trying to catch: " + Strings.GetCreatureName(Creature.name), LoggerTypes.Debug));
                RemoveMapCreature(Creature);

                MethodResult<FCatchingCreature> result = await EncounterCreature(Creature);

                if (!result.Success)
                {
                    return new MethodResult();
                }

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                if (result.Data == null )
                {
                    LogCaller(new LoggerEventArgs("Creature Data is null. Ignoring", LoggerTypes.Debug));
                    return new MethodResult();
                }

                if ( result.Data.isCreatureStorageFull)
                {
                    LogCaller(new LoggerEventArgs("Creature Storage is full. Ignoring catching", LoggerTypes.Debug));

                    var transferResult = await TransferFilteredCreature();

                    if (transferResult.Success)
                    {
                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                    }

                    return new MethodResult();
                }

                MethodResult catchResult = await CatchCreature(result.Data, Creature);

                if (!catchResult.Success)
                    return catchResult;

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
            }
       
            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult> CatchCreature(FCatchingCreature catchingCreaure, FWildCreature wildCreature)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }

            if (!CatchDisabled)
            {
                if (RemainingPokeballs() < 1)
                {
                    LogCaller(new LoggerEventArgs("You don't have any pokeball catching Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                    CatchDisabled = true;
                    TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                    return new MethodResult();
                }
                var resCatch = new FCatchCreatureResult();
                var maxTries = 5; // TODO: make this configurable
                var times = maxTries; 
                var success = false;
                var message = "";

                do
                {
                    var ball = Items.FirstOrDefault(x => (x.type == ItemType.MAGIC_BALL_SIMPLE || x.type == ItemType.MAGIC_BALL_NORMAL || x.type == ItemType.MAGIC_BALL_GOOD) && x.count > 0 );
                    if (ball == null) {
                        message = $"No balls. Skipping catching creature {Strings.GetCreatureName(catchingCreaure.name)}";
                        break;
                    }
                    resCatch = _client.DracoClient.Creatures.Catch(catchingCreaure.id, ball.type, catchingCreaure.quality, new Random().NextDouble() >= 0.5);
                    if (resCatch.caught)
                    {
                        int expGained = 0;
                        int candyGained = 0;

                        if (resCatch.expCreatureExisting > 0)
                            expGained = resCatch.expCreatureExisting;
                        else
                            expGained = resCatch.expCreatureNew;

                        candyGained = resCatch.candies;

                        message = $"Creature {Strings.GetCreatureName(resCatch.userCreature.name)}, with cp { resCatch.userCreature.cp }, caught using a {Strings.GetItemName(ball.type)}, exp { expGained }, candies { candyGained }";
                        success = true;

                        Tracker.AddValues(1, 0);

                        ExpIncrease(expGained);

                        _fleeingCreatureResponses = 0;

                        UpdateInventory(InventoryRefresh.CreatureCandy);

                        UpdateInventory(InventoryRefresh.Creature);
                    }
                    else if (resCatch.runAway)
                    {
                        _fleeingCreatureResponses++;
                        message = $"Creature {Strings.GetCreatureName(catchingCreaure.name)}, with cp {catchingCreaure.cp}, fled.";
                    }
                    ball.count--;
                    times--;
                } while (!resCatch.caught && !resCatch.runAway && times > 0);
                if (times<= 0)
                    message = $"Creature {Strings.GetCreatureName(resCatch.userCreature.name)}, with cp {resCatch.userCreature.cp}, not caught after of {maxTries} tries.";

                LogCaller(new LoggerEventArgs(message, success ? LoggerTypes.Success : LoggerTypes.Warning));

                return new MethodResult
                {
                    Message = message,
                    Success = success
                };
            }
            return new MethodResult();
        }

        private async Task<MethodResult<FCatchingCreature>> EncounterCreature(FWildCreature mapCreature)
        {

            if (mapCreature == null)
            {
                return new MethodResult<FCatchingCreature>();
            }

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<FCatchingCreature>();
                }
            }

            if (!CreatureWithinCatchSettings(mapCreature.name))
            {
                return new MethodResult<FCatchingCreature>();
            }

            FCatchingCreature eResponse = null;
            try
            {
                eResponse = _client.DracoClient.Creatures.Encounter(mapCreature.id);
            }
            catch (Exception ex)
            {
                _fleeingCreatureResponses++;
                LogCaller(new LoggerEventArgs(String.Format("Failed to Encounter Creature {0}.", mapCreature.id), LoggerTypes.Warning, ex));
                return new MethodResult<FCatchingCreature> { Message = ex.Message };
            }

            if (eResponse == null)
                return new MethodResult<FCatchingCreature>();

            return new MethodResult<FCatchingCreature>
            {
                Data = eResponse,
                Success = true,
                Message = "Success"
            };
        }
 
        private bool CreatureWithinCatchSettings(CreatureType CreatureId)
        { 
            CatchSetting catchSettings = UserSettings.CatchSettings.FirstOrDefault(x => x.Id == CreatureId);

            if (catchSettings == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to find catch setting for {0}. Attempting to catch", CreatureId), LoggerTypes.Warning));

                return false;
            }

            if (catchSettings.Catch)
            {
                return true;
            }

            LogCaller(new LoggerEventArgs(String.Format("Skipping catching {0}", CreatureId), LoggerTypes.Info));
            return false;
        }

        //TODO: Maybe look this for better ball
        /*
        private ItemType GetBestBall(FCreatureUpdate CreatureData)
        {
            if (Items == null || CreatureData == null || CreatureData.CreatureId == CreatureId.Missingno)
            {
                return ItemId.ItemUnknown;
            }

            int CreatureCp = CreatureData.Cp;
            //double ivPercent = CalculateIVPerfection(encounter.WildCreature.CreatureData).Data;

            ItemData pokeBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemPokeBall);
            ItemData greatBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemGreatBall);
            ItemData ultraBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemUltraBall);
            ItemData masterBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemMasterBall);
            ItemData premierBalls = Items.FirstOrDefault(x => x.ItemId == ItemId.ItemPremierBall);

            if (masterBalls != null && masterBalls.Count > 0 && CreatureCp >= 1200)
            {
                masterBalls.Count--;

                return ItemId.ItemMasterBall;
            }

            if (ultraBalls != null && ultraBalls.Count > 0 && CreatureCp >= 750)
            {
                ultraBalls.Count--;

                return ItemId.ItemUltraBall;
            }

            if (greatBalls != null && greatBalls.Count > 0 && CreatureCp >= 1000)
            {
                greatBalls.Count--;

                return ItemId.ItemGreatBall;
            }

            if (pokeBalls != null && pokeBalls.Count > 0)
            {
                pokeBalls.Count--;

                return ItemId.ItemPokeBall;
            }

            if (greatBalls != null && greatBalls.Count > 0)
            {
                greatBalls.Count--;

                return ItemId.ItemGreatBall;
            }

            if (ultraBalls != null && ultraBalls.Count > 0)
            {
                ultraBalls.Count--;

                return ItemId.ItemUltraBall;
            }

            if (masterBalls != null && masterBalls.Count > 0)
            {
                masterBalls.Count--;

                return ItemId.ItemMasterBall;
            }

            if (premierBalls != null && premierBalls.Count > 0)
            {
                premierBalls.Count--;

                return ItemId.ItemPremierBall;
            }

            return ItemId.ItemUnknown;
        }

        private async Task UseBerry(ulong encounterId, string spawnId, ItemId berry)
        {
            ItemData berryData = Items.FirstOrDefault(x => x.ItemId == berry);

            if (berryData == null || berryData.Count <= 0)
            {
                return;
            }

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return;
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseItemEncounter,
                RequestMessage = new UseItemEncounterMessage
                {
                    EncounterId = encounterId,
                    Item = berryData.ItemId,
                    SpawnPointGuid = spawnId
                }.ToByteString()
            });

            if (response == null)
                return;

            UseItemEncounterResponse useItemEncounterResponse = UseItemEncounterResponse.Parser.ParseFrom(response);

            switch (useItemEncounterResponse.Status)
            {
                case UseItemEncounterResponse.Types.Status.ActiveItemExists:
                    LogCaller(new LoggerEventArgs("Faill: " + useItemEncounterResponse.Status, LoggerTypes.Debug));
                    break;
                case UseItemEncounterResponse.Types.Status.AlreadyCompleted:
                    LogCaller(new LoggerEventArgs("Faill: " + useItemEncounterResponse.Status, LoggerTypes.Debug));
                    break;
                case UseItemEncounterResponse.Types.Status.InvalidItemCategory:
                    LogCaller(new LoggerEventArgs("Faill: " + useItemEncounterResponse.Status, LoggerTypes.Debug));
                    break;
                case UseItemEncounterResponse.Types.Status.NoItemInInventory:
                    LogCaller(new LoggerEventArgs("Faill: " + useItemEncounterResponse.Status, LoggerTypes.Debug));
                    break;
                case UseItemEncounterResponse.Types.Status.Success:
                    int remaining = berryData.Count - 1;
                    berryData.Count = remaining;
                    LogCaller(new LoggerEventArgs(String.Format("Successfully used {0}. Remaining: {1}", berryData.ItemId.ToString().Replace("Item", ""), remaining), LoggerTypes.Success));

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                    break;
            }
        }

        private async Task UseBerry(MapCreature Creature, ItemId berry)
        {
            await UseBerry(Creature.EncounterId, Creature.SpawnPointId, berry);
        }

        private bool HitInsideReticle()
        {
            lock (_rand)
            {
                return _rand.Next(1, 101) <= UserSettings.InsideReticuleChance;

            }
        }

        //Encounter Incense
        private async Task<MethodResult<IncenseEncounterResponse>> EncounterIncenseCreature(MapCreature mapCreature)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<IncenseEncounterResponse>();
                }
            }

            if (mapCreature == null || mapCreature.CreatureId == CreatureId.Missingno)
                return new MethodResult<IncenseEncounterResponse>();

            if (LastedEncountersIds.Contains(mapCreature.EncounterId))
                return new MethodResult<IncenseEncounterResponse>();

            if (!CatchDisabled)
            {
                if (RemainingPokeballs() < 1)
                {
                    LogCaller(new LoggerEventArgs("You don't have any pokeball catching Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                    CatchDisabled = true;
                    TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                    return new MethodResult<IncenseEncounterResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.IncenseEncounter,
                RequestMessage = new IncenseEncounterMessage
                {
                    EncounterId = mapCreature.EncounterId,
                    EncounterLocation = mapCreature.SpawnPointId
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<IncenseEncounterResponse>();

            IncenseEncounterResponse eResponse = IncenseEncounterResponse.Parser.ParseFrom(response);

            switch (eResponse.Result)
            {
                case IncenseEncounterResponse.Types.Result.IncenseEncounterNotAvailable:
                    break;
                case IncenseEncounterResponse.Types.Result.IncenseEncounterUnknown:
                    break;
                case IncenseEncounterResponse.Types.Result.IncenseEncounterSuccess:
                    if (LastedEncountersIds.Count > 30)
                        LastedEncountersIds.Clear();

                    LastedEncountersIds.Add(eResponse.CreatureData.Id);

                    return new MethodResult<IncenseEncounterResponse>
                    {
                        Data = eResponse,
                        Success = true,
                        Message = "Success"
                    };
                case IncenseEncounterResponse.Types.Result.CreatureInventoryFull:
                    break;
            }

            if (LastedEncountersIds.Count > 30)
                LastedEncountersIds.Clear();

            LastedEncountersIds.Add(mapCreature.EncounterId);

            LogCaller(new LoggerEventArgs(String.Format("Encounter incense failed with response {0}", eResponse.Result), LoggerTypes.Warning));
            return new MethodResult<IncenseEncounterResponse>();
        }
        */
    }
}
