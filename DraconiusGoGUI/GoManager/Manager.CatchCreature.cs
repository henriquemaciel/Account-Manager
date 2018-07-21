using System.Globalization;
using DraconiusGoGUI.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DraconiusGoGUI.Enums;

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

            /*
            if (FilledCreatureStorage() >= 100)
            {
                return new MethodResult
                {
                    Message = "Creature Inventory Full."
                };
            }
            */

            if (!CatchDisabled)
            {
                if (RemainingPokeballs() < 1)
                {
                    LogCaller(new LoggerEventArgs("You don't have any pokeball catching (Lure) Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                    CatchDisabled = true;
                    TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                    return new MethodResult();
                }
            }
            else
                return new MethodResult();

           /* MethodResult<MapCreature> iResponse = await GetIncenseCreatures();

            if (!iResponse.Success || iResponse.Data == null || iResponse.Data.CreatureId == CreatureId.Missingno)
            {
                return new MethodResult();
            }

            if (iResponse.Data.CreatureId == CreatureId.Missingno)
                return new MethodResult();

            if (!CreatureWithinCatchSettings(iResponse.Data.CreatureId))
            {
                return new MethodResult();
            }

            MethodResult<IncenseEncounterResponse> result = await EncounterIncenseCreature(iResponse.Data);

            if (!result.Success)
            {
                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                return new MethodResult();
            }

            MethodResult catchResult = await CatchCreature(result.Data, iResponse.Data);

            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

    */
            return new MethodResult
            {
                Success = true
            };
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

            /*
            if (FilledCreatureStorage() >= 100)
            {
                return new MethodResult
                {
                    Message = "Creature Inventory Full."
                };
            }            

            MethodResult<List<MapCreature>> catchableResponse = await GetCatchableCreatureAsync();

            if (!catchableResponse.Success || catchableResponse.Data == null || catchableResponse.Data.Count == 0)
            {
                return new MethodResult();
            }

            foreach (MapCreature Creature in catchableResponse.Data)
            {
                if (Creature.CreatureId == CreatureId.Missingno)
                    continue;

                if (!CreatureWithinCatchSettings(Creature))
                {
                    continue;
                }

                MethodResult<EncounterResponse> result = await EncounterCreature(Creature);

                if (!result.Success)
                {
                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                    continue;
                }

                MethodResult catchResult = await CatchCreature(result.Data, Creature);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
            }
            */

            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult> CatchLuredCreature(/*FortData fortData*/)
        {
            /*if (fortData.LureInfo == null)
            {
                return new MethodResult
                {
                    Message = "No lure on Building",
                };
            }*/

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

            if (!CatchDisabled)
            {
                if (RemainingPokeballs() < 1)
                {
                    LogCaller(new LoggerEventArgs("You don't have any pokeball catching (Lure) Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                    CatchDisabled = true;
                    TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                    return new MethodResult();
                }
            }
            else
                return new MethodResult();
            /*
            if (fortData.LureInfo.ActiveCreatureId == CreatureId.Missingno)
            {
                return new MethodResult
                {
                    Message = "No lured Creature",
                };
            }

            if (!CreatureWithinCatchSettings(fortData.LureInfo.ActiveCreatureId))
            {
                return new MethodResult();
            }

            MethodResult catchResult = await CatchCreature(fortData);

            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
            */
            return new MethodResult
            {
                Success = true
            };
        }

        //Catch lured Creature
        private async Task<MethodResult> CatchCreature(/*FortData fortData*/)
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
                    LogCaller(new LoggerEventArgs("You don't have any pokeball catching (Lure) Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                    CatchDisabled = true;
                    TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                    return new MethodResult();
                }
            }
            else
                return new MethodResult();
            /*
            if (fortData.LureInfo == null || fortData.LureInfo.ActiveCreatureId == CreatureId.Missingno)
                return new MethodResult();

            if (LastedEncountersIds.Contains(fortData.LureInfo.EncounterId))
                return new MethodResult();

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.DiskEncounter,
                RequestMessage = new DiskEncounterMessage
                {
                    EncounterId = fortData.LureInfo.EncounterId,
                    FortId = fortData.Id,
                    GymLatDegrees = fortData.Latitude,
                    GymLngDegrees = fortData.Longitude,
                    PlayerLatitude = _client.ClientSession.Player.Latitude,
                    PlayerLongitude = _client.ClientSession.Player.Longitude
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult();

            DiskEncounterResponse eResponse = DiskEncounterResponse.Parser.ParseFrom(response);

            switch (eResponse.Result)
            {
                case DiskEncounterResponse.Types.Result.Success:
                    if (LastedEncountersIds.Count > 30)
                        LastedEncountersIds.Clear();

                    LastedEncountersIds.Add(eResponse.CreatureData.Id);

                    CatchCreatureResponse catchCreatureResponse = null;
                    int attemptCount = 1;
                    var berryUsed = false;

                    if (eResponse.CreatureData == null || eResponse.CreatureData.CreatureId == CreatureId.Missingno)
                        return new MethodResult();

                    do
                    {
                        if (!CatchDisabled)
                        {
                            if (RemainingPokeballs() < 1)
                            {
                                LogCaller(new LoggerEventArgs("You don't have any pokeball catching (Lure) Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                                CatchDisabled = true;
                                TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                                return new MethodResult();
                            }
                        }
                        else
                            return new MethodResult();

                        //Uses lowest capture probability
                        float probability = eResponse.CaptureProbability.CaptureProbability_[0];
                        ItemId pokeBall = GetBestBall(eResponse.CreatureData);

                        if (UserSettings.UseBerries)
                        {
                            bool isLowProbability = probability < 0.35;
                            bool isHighCp = eResponse.CreatureData.Cp > 700;
                            bool isHighPerfection = CalculateIVPerfection(eResponse.CreatureData) > 90;

                            if (!berryUsed)
                            {
                                if ((isLowProbability && isHighCp) || isHighPerfection)
                                {
                                    await UseBerry(fortData.LureInfo.EncounterId, fortData.Id, ItemId.ItemRazzBerry);
                                    berryUsed = true;
                                }
                                else
                                {
                                    bool isHighProbability = probability > 0.65;
                                    var catchSettings = UserSettings.CatchSettings.FirstOrDefault(x => x.Id == eResponse.CreatureData.CreatureId);
                                    if (isHighProbability && catchSettings.UsePinap)
                                    {
                                        await UseBerry(fortData.LureInfo.EncounterId, fortData.Id, ItemId.ItemPinapBerry);
                                        berryUsed = true;
                                    }
                                    else if (new Random().Next(0, 100) < 50)
                                    {
                                        // IF we dont use razz neither use pinap, then we will use nanab randomly the 50% of times.
                                        await UseBerry(fortData.LureInfo.EncounterId, fortData.Id, ItemId.ItemNanabBerry);
                                        berryUsed = true;
                                    }
                                }
                            }
                        }

                        double reticuleSize = 1.95;
                        bool hitInsideReticule = true;

                        //Humanization
                        if (UserSettings.EnableHumanization)
                        {
                            reticuleSize = (double)_rand.Next(10, 195) / 100;
                            hitInsideReticule = HitInsideReticle();
                        }

                        //End humanization
                        var arPlusValues = new ARPlusEncounterValues();
                        if (UserSettings.GetArBonus)
                        {
                            LogCaller(new LoggerEventArgs("Using AR Bonus Values", LoggerTypes.Info));
                            arPlusValues.Awareness = (float)UserSettings.ARBonusAwareness;
                            arPlusValues.Proximity = (float)UserSettings.ARBonusProximity;
                            arPlusValues.CreatureFrightened = false;
                        }

                        if (!_client.LoggedIn)
                        {
                            MethodResult result = await AcLogin();

                            if (!result.Success)
                            {
                                return result;
                            }
                        }

                        var catchresponse = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                        {
                            RequestType = RequestType.CatchCreature,
                            RequestMessage = new CatchCreatureMessage
                            {
                                ArPlusValues = arPlusValues,
                                EncounterId = fortData.LureInfo.EncounterId,
                                HitCreature = hitInsideReticule,
                                NormalizedHitPosition = 1,
                                NormalizedReticleSize = reticuleSize,
                                Pokeball = pokeBall,
                                SpawnPointId = fortData.Id,
                                SpinModifier = 1
                            }.ToByteString()
                        });

                        if (catchresponse == null)
                            return new MethodResult();

                        catchCreatureResponse = CatchCreatureResponse.Parser.ParseFrom(catchresponse);
                        string Creature = String.Format("Name: {0}, CP: {1}, IV: {2:0.00}%", fortData.LureInfo.ActiveCreatureId, eResponse.CreatureData.Cp, CalculateIVPerfection(eResponse.CreatureData));
                        string pokeBallName = pokeBall.ToString().Replace("Item", "");

                        switch (catchCreatureResponse.Status)
                        {
                            case CatchCreatureResponse.Types.CatchStatus.CatchError:
                                LogCaller(new LoggerEventArgs(String.Format("Unknown Error. {0}. Attempt #{1}. Status: {2}", Creature, attemptCount, catchCreatureResponse.Status), LoggerTypes.Warning));
                                continue;
                            case CatchCreatureResponse.Types.CatchStatus.CatchEscape:
                                //If we get this response, means we're good
                                _fleeingCreatureResponses = 0;
                                _potentialCreatureBan = false;

                                if (AccountState == AccountState.SoftBan || AccountState == AccountState.HashIssues)
                                {
                                    AccountState = AccountState.Good;

                                    LogCaller(new LoggerEventArgs("Creature ban was lifted", LoggerTypes.Info));
                                }

                                LogCaller(new LoggerEventArgs(String.Format("Escaped ball. {0}. Attempt #{1}. Ball: {2}", Creature, attemptCount, pokeBallName), LoggerTypes.CreatureEscape));
                                continue;
                            case CatchCreatureResponse.Types.CatchStatus.CatchFlee:
                                ++_fleeingCreatureResponses;
                                LogCaller(new LoggerEventArgs(String.Format("Creature fled. {0}. Attempt #{1}. Ball: {2}", Creature, attemptCount, pokeBallName), LoggerTypes.CreatureFlee));
                                continue;
                            case CatchCreatureResponse.Types.CatchStatus.CatchMissed:
                                LogCaller(new LoggerEventArgs(String.Format("Missed. {0}. Attempt #{1}. Status: {2}", Creature, attemptCount, catchCreatureResponse.Status), LoggerTypes.Warning));
                                continue;
                            case CatchCreatureResponse.Types.CatchStatus.CatchSuccess:
                                int expGained = catchCreatureResponse.CaptureAward.Xp.Sum();
                                int candyGained = catchCreatureResponse.CaptureAward.Candy.Sum();

                                Tracker.AddValues(1, 0);

                                ExpIncrease(expGained);

                                //_expGained += expGained;

                                fortData.LureInfo = null;

                                LogCaller(new LoggerEventArgs(String.Format("[Lured] Creature Caught. {0}. Exp {1}. Candy {2}. Attempt #{3}. Ball: {4}", Creature, expGained, candyGained, attemptCount, pokeBallName), LoggerTypes.Success));

                                //Auto favorit shiny
                                if (UserSettings.AutoFavoritShiny && eResponse.CreatureData.CreatureDisplay.Shiny)
                                {
                                    LogCaller(new LoggerEventArgs(String.Format("[{0}] Creature shiny. Auto favorit this Creature.", eResponse.CreatureData.CreatureId.ToString()), LoggerTypes.Info));
                                    await FavoriteCreature(new List<CreatureData> { eResponse.CreatureData }, true);
                                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                }

                                //Creature.Add(eResponse.CreatureData);
                                UpdateInventory(InventoryRefresh.Creature);

                                return new MethodResult
                                {
                                    Message = "Creature caught",
                                    Success = true
                                };
                        }
                        ++attemptCount;

                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                    } while (catchCreatureResponse.Status == CatchCreatureResponse.Types.CatchStatus.CatchMissed || catchCreatureResponse.Status == CatchCreatureResponse.Types.CatchStatus.CatchEscape);
                    return new MethodResult();
                case DiskEncounterResponse.Types.Result.EncounterAlreadyFinished:
                    break;
                case DiskEncounterResponse.Types.Result.NotAvailable:
                    break;
                case DiskEncounterResponse.Types.Result.NotInRange:
                    break;
                case DiskEncounterResponse.Types.Result.CreatureInventoryFull:
                    //Transfert if full
                    LogCaller(new LoggerEventArgs("Faill CreatureInventoryFull.", LoggerTypes.Warning));
                    await TransferFilteredCreature();
                    break;
                case DiskEncounterResponse.Types.Result.Unknown:
                    break;
            }

            if (LastedEncountersIds.Count > 30)
                LastedEncountersIds.Clear();

            LastedEncountersIds.Add(fortData.LureInfo.EncounterId);

            LogCaller(new LoggerEventArgs(String.Format("Faill cath lure on Building {0}. {1}.",fortData.Id, eResponse.Result), LoggerTypes.Warning));
            */
            return new MethodResult();
        }
        
        /*
        private async Task<MethodResult<EncounterResponse>> EncounterCreature(MapCreature mapCreature)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<EncounterResponse>();
                }
            }

            if (mapCreature == null || mapCreature.CreatureId == CreatureId.Missingno)
                return new MethodResult<EncounterResponse>();

            if (AlreadySnipped || mapCreature.EncounterId == _lastPokeSniperId)
                return new MethodResult<EncounterResponse>();

            if (LastedEncountersIds.Contains(mapCreature.EncounterId))
                return new MethodResult<EncounterResponse>();

            if (!CatchDisabled)
            {
                if (RemainingPokeballs() < 1)
                {
                    LogCaller(new LoggerEventArgs("You don't have any pokeball catching Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                    CatchDisabled = true;
                    TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                    return new MethodResult<EncounterResponse>();
                }
            }
            else
                return new MethodResult<EncounterResponse>();

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.Encounter,
                RequestMessage = new EncounterMessage
                {
                    EncounterId = mapCreature.EncounterId,
                    PlayerLatitude = _client.ClientSession.Player.Latitude,
                    PlayerLongitude = _client.ClientSession.Player.Longitude,
                    SpawnPointId = mapCreature.SpawnPointId
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<EncounterResponse>();

            EncounterResponse eResponse = EncounterResponse.Parser.ParseFrom(response);

            switch (eResponse.Status)
            {
                case EncounterResponse.Types.Status.EncounterAlreadyHappened:
                    break;
                case EncounterResponse.Types.Status.EncounterClosed:
                    break;
                case EncounterResponse.Types.Status.EncounterError:
                    break;
                case EncounterResponse.Types.Status.EncounterNotFound:
                    break;
                case EncounterResponse.Types.Status.EncounterNotInRange:
                    break;
                case EncounterResponse.Types.Status.EncounterCreatureFled:
                    break;
                case EncounterResponse.Types.Status.EncounterSuccess:
                    if (LastedEncountersIds.Count > 30)
                        LastedEncountersIds.Clear();

                    LastedEncountersIds.Add(eResponse.WildCreature.EncounterId);

                    return new MethodResult<EncounterResponse>
                    {
                        Data = eResponse,
                        Success = true,
                        Message = "Success"
                    };
                case EncounterResponse.Types.Status.CreatureInventoryFull:
                    //Transfert if full
                    LogCaller(new LoggerEventArgs("Faill CreatureInventoryFull.", LoggerTypes.Warning));
                    await TransferFilteredCreature();
                    break;
            }

            if (LastedEncountersIds.Count > 30)
                LastedEncountersIds.Clear();

            LastedEncountersIds.Add(mapCreature.EncounterId);

            LogCaller(new LoggerEventArgs(String.Format("Faill encounter Creature. {0}.", eResponse.Status), LoggerTypes.Warning));
            return new MethodResult<EncounterResponse> { Message = eResponse.Status.ToString() };
        }
        */

        //Catch encountered Creature
        private async Task<MethodResult> CatchCreature(dynamic eResponse, /*MapCreature mapCreature,*/ bool snipped = false)
        {
            /*
            CreatureData _encounteredCreature = null;
            long _unixTimeStamp = 0;
            ulong _encounterId = 0;
            string _spawnPointId = null;
            string _CreatureType = null;
            //Default catch success
            LoggerTypes _loggerType = LoggerTypes.Success;

            // Calling from CatchNormalCreature
            if (eResponse is EncounterResponse &&
                    (eResponse?.Status == EncounterResponse.Types.Status.EncounterSuccess))
            {
                _encounteredCreature = eResponse.WildCreature?.CreatureData;
                _unixTimeStamp = eResponse.WildCreature?.LastModifiedTimestampMs
                                + eResponse.WildCreature?.TimeTillHiddenMs;
                _spawnPointId = eResponse.WildCreature?.SpawnPointId;
                _encounterId = eResponse.WildCreature?.EncounterId;
                _CreatureType = "Normal";
            }
            // Calling from CatchIncenseCreature
            else if (eResponse is IncenseEncounterResponse &&
                         (eResponse?.Result == IncenseEncounterResponse.Types.Result.IncenseEncounterSuccess))
            {
                _encounteredCreature = eResponse?.CreatureData;
                _unixTimeStamp = mapCreature.ExpirationTimestampMs;
                _spawnPointId = mapCreature.SpawnPointId;
                _encounterId = mapCreature.EncounterId;
                _CreatureType = "Incense";
            }

            if (_encounterId == _lastPokeSniperId || snipped)
            {
                _CreatureType = "Local Snipe: " + _CreatureType;
                _loggerType = LoggerTypes.Snipe;
                AlreadySnipped = true;
            }

            CatchCreatureResponse catchCreatureResponse = null;
            int attemptCount = 1;
            bool berryUsed = false;

            if (_encounteredCreature == null || _encounteredCreature.CreatureId == CreatureId.Missingno)
                return new MethodResult();

            do
            {
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
                    return new MethodResult();

                //Uses lowest capture probability
                float probability = eResponse.CaptureProbability.CaptureProbability_[0];
                ItemId pokeBall = GetBestBall(_encounteredCreature);

                if (UserSettings.UseBerries)
                {
                    bool isLowProbability = probability < 0.40;
                    bool isHighCp = _encounteredCreature.Cp > 800;
                    bool isHighPerfection = CalculateIVPerfection(_encounteredCreature) > 95;

                    if (!berryUsed)
                    {
                        if ((isLowProbability && isHighCp) || isHighPerfection)
                        {
                            await UseBerry(mapCreature, ItemId.ItemRazzBerry);
                            berryUsed = true;
                        }
                        else
                        {
                            bool isHighProbability = probability > 0.65;
                            var catchSettings = UserSettings.CatchSettings.FirstOrDefault(x => x.Id == _encounteredCreature.CreatureId);
                            if (isHighProbability && catchSettings.UsePinap)
                            {
                                await UseBerry(mapCreature, ItemId.ItemPinapBerry);
                                berryUsed = true;
                            }
                            else if (new Random().Next(0, 100) < 50)
                            {
                                // IF we dont use razz neither use pinap, then we will use nanab randomly the 50% of times.
                                await UseBerry(mapCreature, ItemId.ItemNanabBerry);
                                berryUsed = true;
                            }
                        }
                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                    }
                }

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                double reticuleSize = 1.95;
                bool hitInsideReticule = true;

                //Humanization
                if (UserSettings.EnableHumanization)
                {
                    reticuleSize = (double)_rand.Next(10, 195) / 100;
                    hitInsideReticule = HitInsideReticle();
                }

                var arPlusValues = new ARPlusEncounterValues();
                if (UserSettings.GetArBonus)
                {
                    LogCaller(new LoggerEventArgs("Using AR Bonus Values", LoggerTypes.Info));
                    arPlusValues.Awareness = (float)UserSettings.ARBonusAwareness;
                    arPlusValues.Proximity = (float)UserSettings.ARBonusProximity;
                    arPlusValues.CreatureFrightened = false;
                }

                if (!_client.LoggedIn)
                {
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                var catchresponse = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.CatchCreature,
                    RequestMessage = new CatchCreatureMessage
                    {
                        ArPlusValues = arPlusValues,
                        EncounterId = _encounterId,
                        HitCreature = hitInsideReticule,
                        NormalizedHitPosition = 1,
                        NormalizedReticleSize = reticuleSize,
                        Pokeball = pokeBall,
                        SpawnPointId = _spawnPointId,
                        SpinModifier = 1
                    }.ToByteString()
                });

                if (catchresponse == null)
                    return new MethodResult();

                catchCreatureResponse = CatchCreatureResponse.Parser.ParseFrom(catchresponse);

                string Creature = String.Format("Name: {0}, CP: {1}, IV: {2:0.00}%", _encounteredCreature.CreatureId.ToString(), _encounteredCreature.Cp, CalculateIVPerfection(_encounteredCreature));
                string pokeBallName = pokeBall.ToString().Replace("Item", "");

                switch (catchCreatureResponse.Status)
                {
                    case CatchCreatureResponse.Types.CatchStatus.CatchError:
                        LogCaller(new LoggerEventArgs(String.Format("Unknown Error. {0}. Attempt #{1}. Status: {2}", Creature, attemptCount, catchCreatureResponse.Status), LoggerTypes.Warning));
                        ++attemptCount;
                        continue;
                    case CatchCreatureResponse.Types.CatchStatus.CatchEscape:
                        //If we get this response, means we're good
                        _fleeingCreatureResponses = 0;
                        _potentialCreatureBan = false;

                        if (AccountState == Enums.AccountState.SoftBan)
                        {
                            AccountState = Enums.AccountState.Good;

                            LogCaller(new LoggerEventArgs("Creature ban was lifted", LoggerTypes.Info));
                        }

                        LogCaller(new LoggerEventArgs(String.Format("Escaped ball. {0}. Attempt #{1}. Ball: {2}", Creature, attemptCount, pokeBallName), LoggerTypes.CreatureEscape));
                        ++attemptCount;
                        continue;
                    case CatchCreatureResponse.Types.CatchStatus.CatchFlee:
                        ++_fleeingCreatureResponses;
                        LogCaller(new LoggerEventArgs(String.Format("Creature fled. {0}. Attempt #{1}. Ball: {2}", Creature, attemptCount, pokeBallName), LoggerTypes.CreatureFlee));
                        ++attemptCount;
                        continue;
                    case CatchCreatureResponse.Types.CatchStatus.CatchMissed:
                        LogCaller(new LoggerEventArgs(String.Format("Missed. {0}. Attempt #{1}. Status: {2}", Creature, attemptCount, catchCreatureResponse.Status), LoggerTypes.Warning));
                        ++attemptCount;
                        continue;
                    case CatchCreatureResponse.Types.CatchStatus.CatchSuccess:
                        //Reset data
                        _fleeingCreatureResponses = 0;
                        Tracker.AddValues(1, 0);
                        _potentialCreatureBan = false;

                        int expGained = catchCreatureResponse.CaptureAward.Xp.Sum();
                        int candyGained = catchCreatureResponse.CaptureAward.Candy.Sum();

                        ExpIncrease(expGained);

                        //_expGained += expGained;

                        LogCaller(new LoggerEventArgs(String.Format("[{0}] Creature Caught. {1}. Exp {2}. Candy: {3}. Attempt #{4}. Ball: {5}", _CreatureType, Creature, expGained, candyGained, attemptCount, pokeBallName), _loggerType));

                        //Auto favorit shiny
                        if (UserSettings.AutoFavoritShiny && _encounteredCreature.CreatureDisplay.Shiny)
                        {
                            LogCaller(new LoggerEventArgs(String.Format("[{0}] Creature shiny. Auto favorit this Creature.", _encounteredCreature.CreatureId.ToString()), LoggerTypes.Info));
                            await FavoriteCreature(new List<CreatureData> { _encounteredCreature }, true);
                            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                        }

                        //Creature.Add(_encounteredCreature);
                        UpdateInventory(InventoryRefresh.Creature);
                        UpdateInventory(InventoryRefresh.CreatureCandy);

                        return new MethodResult
                        {
                            Message = "Creature caught",
                            Success = true
                        };
                }
                
                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
            } while (catchCreatureResponse.Status == CatchCreatureResponse.Types.CatchStatus.CatchMissed || catchCreatureResponse.Status == CatchCreatureResponse.Types.CatchStatus.CatchEscape);
            */
            return new MethodResult();
        }

        /*
        private bool CreatureWithinCatchSettings(CreatureId CreatureId)
        {
            if (CreatureId == CreatureId.Missingno)
                return false;

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
        

        private bool CreatureWithinCatchSettings(MapCreature Creature)
        {
            if (Creature == null || Creature.CreatureId == CreatureId.Missingno)
                return false;

            CatchSetting catchSettings = UserSettings?.CatchSettings.FirstOrDefault(x => x.Id == Creature.CreatureId);

            if (catchSettings == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to find catch setting for {0}. Attempting to catch", Creature.CreatureId), LoggerTypes.Warning));
                return false;
            }

            if (catchSettings.Catch)
            {
                return true;
            }

            LogCaller(new LoggerEventArgs(String.Format("Skipping catching {0}", Creature.CreatureId.ToString()), LoggerTypes.Info));
            return false;
        }

        private ItemId GetBestBall(CreatureData CreatureData)
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
