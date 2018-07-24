using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DracoProtos.Core.Objects;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        private async Task<MethodResult> SearchPokestop(FBuilding Building)
        {
            if (Building == null)
                return new MethodResult();

            const int maxFortAttempts = 5;
            for (int i = 0; i < maxFortAttempts; i++)
            {
                if (!_client.LoggedIn)
                {
                    return new MethodResult();
                }
                if (Building.pitstop.cooldown)
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

                try
                {
                    response = _client.DracoClient.TryUseBuilding(UserSettings.Latitude, UserSettings.Longitude, Building.id, Building.coords.latitude, Building.coords.longitude, Building.dungeonId);
                }
                catch(Exception ex)
                {
                    LogCaller(new LoggerEventArgs($"Building {Building.id} fail" + ex, LoggerTypes.FatalError));
                    return new MethodResult();
                }

                if (response.items.Count < 2)
                {
                    LogCaller(new LoggerEventArgs($"Invalid response", LoggerTypes.Warning));
                    return new MethodResult();
                }
                var text = "Award Received: ";
                var loot = response.items.FirstOrDefault(x => x is FPickItemsResponse) as FPickItemsResponse;
                foreach (var item in loot.loot.lootList) {
                    var itemItem = item as FLootItemItem;
                    if (itemItem!=null)
                        text += $"[{itemItem.qty}] {Strings.GetItemName(itemItem.item)}, ";
                    else
                        text += $"[{item.qty}] XP, ";
                }
                LogCaller(new LoggerEventArgs(text, LoggerTypes.Success));
                if (loot.levelUpLoot != null) {
                    text = "Level Up Award: ";
                    foreach (var item in loot.levelUpLoot.lootList)
                    {
                        var itemItem = item as FLootItemItem;
                        if (itemItem != null)
                            text = $"[{itemItem.qty}] {Strings.GetItemName(itemItem.item)}, ";
                        else
                            text += $"[{item.qty}] XP, ";
                    }
                    LogCaller(new LoggerEventArgs(text, LoggerTypes.Success));
                }
                // copy pitstop values
                Building.pitstop = (response.items.FirstOrDefault(x => x is FBuilding) as FBuilding).pitstop;

                Tracker.AddValues(0, 1);

                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

                return new MethodResult
                {
                    Success = true,
                    Message = "Success"
                };

            }
            //FortSearchResponse fortResponse = null;

            //string fort = Building.Type == FortType.Checkpoint ? "Fort" : "Gym";

            /*
            for (int i = 0; i < maxFortAttempts; i++)
            {
                if (!_client.LoggedIn)
                {
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                var response = await Task.Run(() => _client.DracoClient.UseBuilding(UserSettings.Latitude, UserSettings.Longitude, Building.id, Building.coords.latitude, Building.coords.longitude, Building.dungeonId) as FLoot);

                if (response.lootList.Count == 0)
                    return new MethodResult();

                string _message = String.Format("Searched {0}. Exp: {1}. Items: {2}.",
                              Building,
                              response.GetExp(),
                              StringUtil.GetSummedFriendlyNameOfItemAwardList(response.lootList));

                LogCaller(new LoggerEventArgs(_message, LoggerTypes.Success));

                return new MethodResult
                {
                    Success = true,
                    Message = "Success"
                };
            }
            */
            return new MethodResult();

            /*
            fortResponse = FortSearchResponse.Parser.ParseFrom(response);

            switch (fortResponse.Result)
            {
                case FortSearchResponse.Types.Result.ExceededDailyLimit:
                    AccountState = AccountState.SoftBan;
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}. Stoping ...", fort, fortResponse.Result), LoggerTypes.Warning));
                    Stop();
                    break;
                case FortSearchResponse.Types.Result.InCooldownPeriod:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", fort, fortResponse.Result), LoggerTypes.Warning));
                    return new MethodResult();//break;
                case FortSearchResponse.Types.Result.InventoryFull:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", fort, fortResponse.Result), LoggerTypes.Warning));
                    //Recycle if inventory full
                    await RecycleFilteredItems();
                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                    return new MethodResult();
                case FortSearchResponse.Types.Result.NoResultSet:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", fort, fortResponse.Result), LoggerTypes.Warning));
                    break;
                case FortSearchResponse.Types.Result.OutOfRange:
                    if (_potentialPokeStopBan)
                    {
                        if (AccountState != AccountState.SoftBan)
                        {
                            LogCaller(new LoggerEventArgs("Building ban detected. Marking state", LoggerTypes.Warning));
                        }

                        AccountState = AccountState.SoftBan;

                        if (fortResponse.ExperienceAwarded != 0)
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
                                    _potentialPokeStopBan = true;
                                }

                                if (AccountState != AccountState.SoftBan)
                                {
                                    //Only occurs when out of range is found
                                    if (fortResponse.ExperienceAwarded == 0)
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
                            _failedPokestopResponse++;
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
                                    RequestType = RequestType.FortSearch,
                                    RequestMessage = new FortSearchMessage
                                    {
                                        FortId = Building.Id,
                                        FortLatitude = Building.Latitude,
                                        FortLongitude = Building.Longitude,
                                        PlayerLatitude = _client.ClientSession.Player.Latitude,
                                        PlayerLongitude = _client.ClientSession.Player.Longitude
                                    }.ToByteString()
                                });

                                if (_response == null)
                                    return new MethodResult();

                                fortResponse = FortSearchResponse.Parser.ParseFrom(_response);

                                switch (fortResponse.Result)
                                {
                                    case FortSearchResponse.Types.Result.ExceededDailyLimit:
                                        LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", fort, fortResponse.Result), LoggerTypes.Warning));
                                        return new MethodResult();
                                    case FortSearchResponse.Types.Result.InventoryFull:
                                        LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", fort, fortResponse.Result), LoggerTypes.Warning));
                                        //Recycle if inventory full
                                        await RecycleFilteredItems();
                                        return new MethodResult();
                                    case FortSearchResponse.Types.Result.Success:
                                        string _message = String.Format("Searched {0}. Exp: {1}. Items: {2}.",
                                                        fort,
                                                        fortResponse.ExperienceAwarded,
                                                        StringUtil.GetSummedFriendlyNameOfItemAwardList(fortResponse.ItemsAwarded.ToList())


                                        //Successfully grabbed stop
                                        if (AccountState == AccountState.SoftBan)// || AccountState == Enums.AccountState.HashIssues)
                                        {
                                            AccountState = AccountState.Good;

                                            LogCaller(new LoggerEventArgs("Soft ban was removed", LoggerTypes.Info));
                                        }

                                        ExpIncrease(fortResponse.ExperienceAwarded);
                                        TotalPokeStopExp += fortResponse.ExperienceAwarded;

                                        Tracker.AddValues(0, 1);

                                        if (fortResponse.ExperienceAwarded == 0)
                                        {
                                            //Softban on the fleeing Creature. Reset.
                                            _fleeingCreatureResponses = 0;
                                            _potentialCreatureBan = false;

                                            ++_totalZeroExpStops;
                                            _message += String.Format(" No exp gained. Attempt {0} of {1}", i + 1, maxFortAttempts);
                                            LogCaller(new LoggerEventArgs(_message, LoggerTypes.Success));
                                            continue;
                                        }

                                        LogCaller(new LoggerEventArgs(_message, LoggerTypes.Success));

                                        _totalZeroExpStops = 0;
                                        _potentialPokeStopBan = false;

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

                        _potentialPokeStopBan = true;
                        _proxyIssue = true;
                        //Display error only on first notice
                        LogCaller(new LoggerEventArgs("Building out of range. Potential temp Building ban or IP ban or daily limit reached.", LoggerTypes.Warning));
                    }

                    _failedPokestopResponse++;
                    //Let it continue down
                    continue;
                case FortSearchResponse.Types.Result.PoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search {0}. Response: {1}", fort, fortResponse.Result), LoggerTypes.Warning));
                    break;
                case FortSearchResponse.Types.Result.Success:
                    string message = String.Format("Searched {0}. Exp: {1}. Items: {2}.",
                        fort,
                        fortResponse.ExperienceAwarded,
                        StringUtil.GetSummedFriendlyNameOfItemAwardList(fortResponse.ItemsAwarded.ToList())

                    //Successfully grabbed stop
                    if (AccountState == AccountState.SoftBan)// || AccountState == Enums.AccountState.HashIssues)
                    {
                        AccountState = AccountState.Good;

                        LogCaller(new LoggerEventArgs("Soft ban was removed", LoggerTypes.Info));
                    }

                    ExpIncrease(fortResponse.ExperienceAwarded);
                    TotalPokeStopExp += fortResponse.ExperienceAwarded;

                    Tracker.AddValues(0, 1);

                    if (fortResponse.ExperienceAwarded == 0)
                    {
                        //Softban on the fleeing Creature. Reset.
                        _fleeingCreatureResponses = 0;
                        _potentialCreatureBan = false;

                        ++_totalZeroExpStops;
                        message += String.Format(" No exp gained. Attempt {0} of {1}", i + 1, maxFortAttempts);
                        LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));
                        continue;
                    }

                    LogCaller(new LoggerEventArgs(message, LoggerTypes.Success));

                    _totalZeroExpStops = 0;
                    _potentialPokeStopBan = false;

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
        private async Task<MethodResult<AddFortModifierResponse>> AddFortModifier(string fortId, ItemId modifierType = ItemId.ItemTroyDisk)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<AddFortModifierResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.AddFortModifier,
                RequestMessage = new AddFortModifierMessage
                {
                    FortId = fortId,
                    ModifierType = modifierType,
                    PlayerLatitude = _client.ClientSession.Player.Latitude,
                    PlayerLongitude = _client.ClientSession.Player.Longitude
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<AddFortModifierResponse>();

            var addFortModifierResponse = AddFortModifierResponse.Parser.ParseFrom(response);

            switch (addFortModifierResponse.Result)
            {
                case AddFortModifierResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs(String.Format("Add fort modifier {0} success.", modifierType.ToString().Replace("Item","")), LoggerTypes.Success));
                    return new MethodResult<AddFortModifierResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = addFortModifierResponse
                    };
                case AddFortModifierResponse.Types.Result.FortAlreadyHasModifier:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search Fort. Response: {0}", addFortModifierResponse.Result), LoggerTypes.Warning));
                    break;
                case AddFortModifierResponse.Types.Result.NoItemInInventory:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search Fort. Response: {0}", addFortModifierResponse.Result), LoggerTypes.Warning));
                    break;
                case AddFortModifierResponse.Types.Result.PoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search Fort. Response: {0}", addFortModifierResponse.Result), LoggerTypes.Warning));
                    break;
                case AddFortModifierResponse.Types.Result.TooFarAway:
                    LogCaller(new LoggerEventArgs(String.Format("Failed to search Fort. Response: {0}", addFortModifierResponse.Result), LoggerTypes.Warning));
                    break;
           }
            return new MethodResult<AddFortModifierResponse>();
        }
        */
    }
}

