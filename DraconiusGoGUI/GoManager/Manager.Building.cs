﻿using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DracoProtos.Core.Objects;
using System;
using System.Threading.Tasks;
using System.Linq;
using DracoProtos.Core.Base;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        private async Task<MethodResult<FUpdate>> EnterInPortal(FBuilding Building)
        {
            try
            {
                FUpdate response = _client.DracoClient.TryUseBuilding(UserSettings.Latitude, UserSettings.Longitude, Building.id, Building.coords.latitude, Building.coords.longitude, Building.dungeonId);

                if (response == null)
                    return new MethodResult<FUpdate>();

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                return new MethodResult<FUpdate>
                {
                    Data = response,
                    Success = true,
                    Message = "Success"
                };
            }
            catch (Exception ex)
            {
                return new MethodResult<FUpdate>
                {
                    Message = ex.Message
                };
            }
        }
        private async Task<MethodResult> SearchBuilding(FBuilding Building)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }

            if (Building == null)
                return new MethodResult();

            if (Stats.isBagFull)
            {
                LogCaller(new LoggerEventArgs($"You bag if full skip {Building.id}. Recycling...", LoggerTypes.Warning));
                await RecycleFilteredItems();
            }

            if (Stats.isEggBagFull)
            {
                LogCaller(new LoggerEventArgs($"You Egg bag if full skip {Building.id}. Recycling...", LoggerTypes.Warning));
                await RecycleFilteredItems();
            }

            if (Stats.isArtifactsBagFull)
            {
                LogCaller(new LoggerEventArgs($"You Artifacts bag if full skip {Building.id}. Recycling...", LoggerTypes.Warning));
                await RecycleFilteredItems();
            }

            if (Building.pitstop != null && Building.pitstop.cooldown)
            {
                LogCaller(new LoggerEventArgs($"Building {Building.id} in cooldowm", LoggerTypes.Warning));
                return new MethodResult();
            }

            if (!Building.available)
            {
                LogCaller(new LoggerEventArgs($"Building {Building.id} no available", LoggerTypes.Warning));
                return new MethodResult();
            }

            FUpdate response = null;
            int ExperienceAwarded = 0;

            try
            {
                response = _client.DracoClient.TryUseBuilding(UserSettings.Latitude, UserSettings.Longitude, Building.id, Building.coords.latitude, Building.coords.longitude, Building.dungeonId);
                UpdateInventory(InventoryRefresh.Items);
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs($"Building {Building.id} fail", LoggerTypes.Exception, ex));
                if (_potentialBuildingBan)
                {
                    if (AccountState != AccountState.SoftBan)
                    {
                        LogCaller(new LoggerEventArgs("Building ban detected. Marking state", LoggerTypes.Warning));
                    }

                    AccountState = AccountState.SoftBan;
                    var _loot = response.items.FirstOrDefault(x => x is FPickItemsResponse) as FPickItemsResponse;
                    var _xpqty = _loot.loot.lootList.Where(x => x is FLootItemExp).Sum(x => x.qty);
                    if (_xpqty == 0)
                    {
                        if (!_potentialCreatureBan && _fleeingCreatureResponses >= _fleeingCreatureUntilBan)
                        {
                            LogCaller(new LoggerEventArgs("Potential Creature ban detected. Setting flee count to 0 avoid false positives", LoggerTypes.Warning));

                            _potentialCreatureBan = true;
                            _fleeingCreatureResponses = 0;
                        }
                        else if (_fleeingCreatureResponses >= _fleeingCreatureUntilBan)
                        {
                            //Already Building banned
                            if (AccountState == AccountState.SoftBan)
                            {
                                _potentialCreatureBan = true;
                                _potentialBuildingBan = true;
                            }

                            if (AccountState != AccountState.SoftBan)
                            {
                                //Only occurs when out of range is found
                                if (_xpqty == 0)
                                {
                                    LogCaller(new LoggerEventArgs("Creature fleeing and failing to grab stops. Potential Creature & Building ban or daily limit reached.", LoggerTypes.Warning));
                                }
                                else
                                {
                                    LogCaller(new LoggerEventArgs("Creature fleeing, yet grabbing stops. Potential Creature ban or daily limit reached.", LoggerTypes.Warning));
                                }
                            }

                            if (UserSettings.StopAtMinAccountState == AccountState.SoftBan)
                            {
                                LogCaller(new LoggerEventArgs("Auto stopping bot ...", LoggerTypes.Info));

                                Stop();
                            }

                            return new MethodResult
                            {
                                Message = "Bans detected",
                            };
                        }
                    }
                }
                return new MethodResult();
            }

            if (response.items.Count < 2)
            {
                LogCaller(new LoggerEventArgs($"Invalid response", LoggerTypes.Warning));
                return new MethodResult();
            }

            var text = "Award Received: ";
            var loot = response.items.FirstOrDefault(x => x is FPickItemsResponse) as FPickItemsResponse;

            foreach (var item in loot.loot.lootList.Where(x => x is FLootItemItem).GroupBy(y => (y as FLootItemItem).item))
            {
                text += $"[{item.Sum(x => x.qty)}] {Strings.GetItemName(item.Key)}, ";
            }

            var xpqty = loot.loot.lootList.Where(x => x is FLootItemExp).Sum(x => x.qty);

            if (xpqty > 0)
            {
                text += $"[{xpqty}] XP, ";
                ExperienceAwarded = xpqty;
            }

            LogCaller(new LoggerEventArgs(text, LoggerTypes.Success));

            if (loot.levelUpLoot != null)
            {
                text = "Level Up Award: ";
                foreach (var item in loot.levelUpLoot.lootList.Where(x => x is FLootItemItem).GroupBy(y => (y as FLootItemItem).item))
                {
                    text += $"[{item.Sum(x => x.qty)}] {Strings.GetItemName(item.Key)}, ";
                }
                xpqty = loot.levelUpLoot.lootList.Where(x => x is FLootItemExp).Sum(x => x.qty);
                if (xpqty > 0)
                {
                    text += $"[{xpqty}] XP, ";
                    ExperienceAwarded = xpqty;
                }

                LogCaller(new LoggerEventArgs(text, LoggerTypes.Success));
            }
            // copy pitstop values
            Building.pitstop = (response.items.FirstOrDefault(x => x is FBuilding) as FBuilding).pitstop;

            ExpIncrease(ExperienceAwarded);
            TotalBuildingExp += ExperienceAwarded;

            Tracker.AddValues(0, 1);

            _totalZeroExpStops = 0;
            _potentialBuildingBan = false;

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

            return new MethodResult
            {
                Success = true,
                Message = "Success"
            };

            /*
            BuildingResponse = BuildingSearchResponse.Parser.ParseFrom(response);

            switch (BuildingResponse.Result)
            {
                case BuildingSearchResponse.Types.Result.ExceededDailyLimit:
                    AccountState = AccountState.SoftBan;
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}. Stoping ...", Building, BuildingResponse.Result), LoggerTypes.Warning));
                    Stop();
                    break;
                case BuildingSearchResponse.Types.Result.InCooldownPeriod:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", Building, BuildingResponse.Result), LoggerTypes.Warning));
                    return new MethodResult();//break;
                case BuildingSearchResponse.Types.Result.InventoryFull:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", Building, BuildingResponse.Result), LoggerTypes.Warning));
                    //Recycle if inventory full
                    await RecycleFilteredItems();
                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                    return new MethodResult();
                case BuildingSearchResponse.Types.Result.NoResultSet:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", Building, BuildingResponse.Result), LoggerTypes.Warning));
                    break;
                case BuildingSearchResponse.Types.Result.OutOfRange:
                    if (_potentialBuildingBan)
                    {
                        if (AccountState != AccountState.SoftBan)
                        {
                            LogCaller(new LoggerEventArgs("Building ban detected. Marking state", LoggerTypes.Warning));
                        }

                        AccountState = AccountState.SoftBan;

                        if (BuildingResponse.ExperienceAwarded != 0)
                        {
                            if (!_potentialCreatureBan && _fleeingCreatureResponses >= _fleeingCreatureUntilBan)
                            {
                                LogCaller(new LoggerEventArgs("Potential Creature ban detected. Setting flee count to 0 avoid false positives", LoggerTypes.Warning));

                                _potentialCreatureBan = true;
                                _fleeingCreatureResponses = 0;
                            }
                            else if (_fleeingCreatureResponses >= _fleeingCreatureUntilBan)
                            {
                                //Already Building banned
                                if (AccountState == AccountState.SoftBan)
                                {
                                    _potentialCreatureBan = true;
                                    _potentialBuildingBan = true;
                                }

                                if (AccountState != AccountState.SoftBan)
                                {
                                    //Only occurs when out of range is found
                                    if (BuildingResponse.ExperienceAwarded == 0)
                                    {
                                        LogCaller(new LoggerEventArgs("Creature fleeing and failing to grab stops. Potential Creature & Building ban or daily limit reached.", LoggerTypes.Warning));
                                    }
                                    else
                                    {
                                        LogCaller(new LoggerEventArgs("Creature fleeing, yet grabbing stops. Potential Creature ban or daily limit reached.", LoggerTypes.Warning));
                                    }
                                }

                                if (UserSettings.StopAtMinAccountState == AccountState.SoftBan)
                                {
                                    LogCaller(new LoggerEventArgs("Auto stopping bot ...", LoggerTypes.Info));

                                    Stop();
                                }

                                return new MethodResult
                                {
                                    Message = "Bans detected",
                                };
                            }
                        }
                    }
                    else //This error should never happen normally, so assume temp ban
                    {
                        if (!UserSettings.UseSoftBanBypass)
                        {
                            _failedBuildingResponse++;
                            LogCaller(new LoggerEventArgs($"Building softban baypass disabled go to next...", LoggerTypes.Info));
                            return new MethodResult();
                        }

                        //by pass softban 
                        int bypass = UserSettings.SoftBanBypassTimes;

                        //Go to location again
                        //LogCaller(new LoggerEventArgs($"Building potential softban baypass enabled go to location again {Building.Latitude}, {Building.Longitude}.", LoggerTypes.Debug));
                        //MethodResult move = UpdateLocation(new GeoCoordinate(Building.Latitude, Building.Longitude));

                        while (bypass > 0)
                        {
                            LogCaller(new LoggerEventArgs($"Building potential softban baypass enabled #{bypass.ToString()}.", LoggerTypes.Info));
                            try
                            {
                                if (!_client.LoggedIn)
                                {
                                    MethodResult result = await AcLogin();

                                    if (!result.Success)
                                    {
                                        return result;
                                    }
                                }

                                var _response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                                {
                                    RequestType = RequestType.BuildingSearch,
                                    RequestMessage = new BuildingSearchMessage
                                    {
                                        BuildingId = Building.Id,
                                        BuildingLatitude = Building.Latitude,
                                        BuildingLongitude = Building.Longitude,
                                        PlayerLatitude = _client.ClientSession.Player.Latitude,
                                        PlayerLongitude = _client.ClientSession.Player.Longitude
                                    }.ToByteString()
                                });

                                if (_response == null)
                                    return new MethodResult();

                                BuildingResponse = BuildingSearchResponse.Parser.ParseFrom(_response);

                                switch (BuildingResponse.Result)
                                {
                                    case BuildingSearchResponse.Types.Result.ExceededDailyLimit:
                                        LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", Building, BuildingResponse.Result), LoggerTypes.Warning));
                                        return new MethodResult();
                                    case BuildingSearchResponse.Types.Result.InventoryFull:
                                        LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", Building, BuildingResponse.Result), LoggerTypes.Warning));
                                        //Recycle if inventory full
                                        await RecycleFilteredItems();
                                        return new MethodResult();
                                    case BuildingSearchResponse.Types.Result.Success:
                                        string _message = String.Format("Searched {0}. Exp: {1}. Items: {2}.",
                                                        Building,
                                                        BuildingResponse.ExperienceAwarded,
                                                        StringUtil.GetSummedFriendlyNameOfItemAwardList(BuildingResponse.ItemsAwarded.ToList())


                                        //Successfully grabbed stop
                                        if (AccountState == AccountState.SoftBan)// || AccountState == Enums.AccountState.HashIssues)
                                        {
                                            AccountState = AccountState.Good;

                                            LogCaller(new LoggerEventArgs("Soft ban was removed", LoggerTypes.Info));
                                        }

                                        ExpIncrease(BuildingResponse.ExperienceAwarded);
                                        TotalBuildingExp += BuildingResponse.ExperienceAwarded;

                                        Tracker.AddValues(0, 1);

                                        if (BuildingResponse.ExperienceAwarded == 0)
                                        {
                                            //Softban on the fleeing Creature. Reset.
                                            _fleeingCreatureResponses = 0;
                                            _potentialCreatureBan = false;

                                            ++_totalZeroExpStops;
                                            _message += String.Format(" No exp gained. Attempt {0} of {1}", i + 1, maxBuildingAttempts);
                                            LogCaller(new LoggerEventArgs(_message, LoggerTypes.Success));
                                            continue;
                                        }

                                        LogCaller(new LoggerEventArgs(_message, LoggerTypes.Success));

                                        _totalZeroExpStops = 0;
                                        _potentialBuildingBan = false;

                                        return new MethodResult
                                        {
                                            Success = true,
                                            Message = "Success"
                                        };
                                }
                            }
                            catch
                            {
                                break;
                            }
                            finally
                            {
                                bypass--;
                                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                            }
                        }

                        _potentialBuildingBan = true;
                        _proxyIssue = true;
                        //Display error only on first notice
                        LogCaller(new LoggerEventArgs("Building out of range. Potential temp Building ban or IP ban or daily limit reached.", LoggerTypes.Warning));
                    }

                    _failedBuildingResponse++;
                    //Let it continue down
                    continue;
                case BuildingSearchResponse.Types.Result.PoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", Building, BuildingResponse.Result), LoggerTypes.Warning));
                    break;
                case BuildingSearchResponse.Types.Result.Success:
                    string message = String.Format("Searched {0}. Exp: {1}. Items: {2}.",
                        Building,
                        BuildingResponse.ExperienceAwarded,
                        StringUtil.GetSummedFriendlyNameOfItemAwardList(BuildingResponse.ItemsAwarded.ToList())

                    //Successfully grabbed stop
                    if (AccountState == AccountState.SoftBan)// || AccountState == Enums.AccountState.HashIssues)
                    {
                        AccountState = AccountState.Good;

                        LogCaller(new LoggerEventArgs("Soft ban was removed", LoggerTypes.Info));
                    }

                    ExpIncrease(BuildingResponse.ExperienceAwarded);
                    TotalBuildingExp += BuildingResponse.ExperienceAwarded;

                    Tracker.AddValues(0, 1);

                    if (BuildingResponse.ExperienceAwarded == 0)
                    {
                        //Softban on the fleeing Creature. Reset.
                        _fleeingCreatureResponses = 0;
                        _potentialCreatureBan = false;

                        ++_totalZeroExpStops;
                        message += String.Format(" No exp gained. Attempt {0} of {1}", i + 1, maxBuildingAttempts);
                        LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));
                        continue;
                    }

                    LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));

                    _totalZeroExpStops = 0;
                    _potentialBuildingBan = false;

                    return new MethodResult
                    {
                        Success = true,
                        Message = "Success"
                    };
            }
        }
        */
        }

        /*
        private async Task<MethodResult<AddBuildingModifierResponse>> AddBuildingModifier(string BuildingId, ItemId modifierType = ItemId.ItemTroyDisk)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<AddBuildingModifierResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.AddBuildingModifier,
                RequestMessage = new AddBuildingModifierMessage
                {
                    BuildingId = BuildingId,
                    ModifierType = modifierType,
                    PlayerLatitude = _client.ClientSession.Player.Latitude,
                    PlayerLongitude = _client.ClientSession.Player.Longitude
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<AddBuildingModifierResponse>();

            var addBuildingModifierResponse = AddBuildingModifierResponse.Parser.ParseFrom(response);

            switch (addBuildingModifierResponse.Result)
            {
                case AddBuildingModifierResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs(String.Format("Add Building modifier {0} success.", modifierType.ToString().Replace("Item","")), LoggerTypes.Success));
                    return new MethodResult<AddBuildingModifierResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = addBuildingModifierResponse
                    };
                case AddBuildingModifierResponse.Types.Result.BuildingAlreadyHasModifier:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search Building. Response: {0}", addBuildingModifierResponse.Result), LoggerTypes.Warning));
                    break;
                case AddBuildingModifierResponse.Types.Result.NoItemInInventory:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search Building. Response: {0}", addBuildingModifierResponse.Result), LoggerTypes.Warning));
                    break;
                case AddBuildingModifierResponse.Types.Result.PoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search Building. Response: {0}", addBuildingModifierResponse.Result), LoggerTypes.Warning));
                    break;
                case AddBuildingModifierResponse.Types.Result.TooFarAway:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search Building. Response: {0}", addBuildingModifierResponse.Result), LoggerTypes.Warning));
                    break;
           }
            return new MethodResult<AddBuildingModifierResponse>();
        }
        */
    }
}

