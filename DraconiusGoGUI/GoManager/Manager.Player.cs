using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.Enums;
using System.Net.Http;
using System.Collections.Generic;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        public async Task<MethodResult> UpdateDetails()
        {
            //UpdateInventory(InventoryRefresh.All);

            LogCaller(new LoggerEventArgs("Updating details", LoggerTypes.Debug));

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));

            return new MethodResult
            {
                Success = true
            };
        }

        public async Task<MethodResult> ExportStats()
        {
            MethodResult result = await UpdateDetails();

            //Prevent API throttling
            await Task.Delay(500);

            if (!result.Success)
            {
                return result;
            }

            //Possible some objects were empty.
            var builder = new StringBuilder();
            builder.AppendLine("=== Trainer Stats ===");

            /*
            if (Stats != null && PlayerData != null)
            {
                builder.AppendLine(String.Format("Group: {0}", UserSettings.GroupName));
                builder.AppendLine(String.Format("Username: {0}", UserSettings.Username));
                builder.AppendLine(String.Format("Password: {0}", UserSettings.Password));
                builder.AppendLine(String.Format("Level: {0}", Stats.Level));
                //builder.AppendLine(String.Format("Current Trainer Name: {0}", PlayerData.Username));
                //builder.AppendLine(String.Format("Team: {0}", PlayerData.Team));
                builder.AppendLine(String.Format("Stardust: {0:N0}", TotalStardust));
                builder.AppendLine(String.Format("Unique Pokedex Entries: {0}", Stats.UniquePokedexEntries));
            }
            else
            {
                builder.AppendLine("Failed to grab stats");
            }
            */
            builder.AppendLine();

            builder.AppendLine("=== Creature ===");

            /*
            if (Creature != null)
            {
                foreach (CreatureData Creature in Creature.OrderByDescending(x => x.Cp))
                {
                    string candy = "Unknown";

                    MethodResult<CreatureSettings> pSettings = GetCreatureSetting(Creature.CreatureId);

                    if (pSettings.Success)
                    {
                        Candy pCandy = CreatureCandy.FirstOrDefault(x => x.FamilyId == pSettings.Data.FamilyId);

                        if (pCandy != null)
                        {
                            candy = pCandy.Candy_.ToString("N0");
                        }
                    }

                    double perfectResult = CalculateIVPerfection(Creature);
                    string iv = "Unknown";

                    iv = Math.Round(perfectResult, 2).ToString() + "%";

                    builder.AppendLine(String.Format("Creature: {0,-10} CP: {1, -5} IV: {2,-7} Primary: {3, -14} Secondary: {4, -14} Candy: {5}", Creature.CreatureId, Creature.Cp, iv, Creature.Move1.ToString().Replace("Fast", ""), Creature.Move2, candy));
                }
            }
            */

            //Remove the hardcoded directory later
            try
            {
                string directoryName = "AccountStats";

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                string fileName = UserSettings.Username.Split('@').First();

                string filePath = Path.Combine(directoryName, fileName) + ".txt";

                File.WriteAllText(filePath, builder.ToString());

                LogCaller(new LoggerEventArgs(String.Format("Finished exporting stats to file {0}", filePath), LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to export stats due to exception", LoggerTypes.Warning, ex));

                return new MethodResult();
            }
        }

        private async Task<MethodResult> ClaimLevelUpRewards(int level)
        {
            if (!UserSettings.ClaimLevelUpRewards || level < 2)
            {
                return new MethodResult();
            }

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }

            /*
            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.LevelUpRewards,
                RequestMessage = new LevelUpRewardsMessage
                {
                    Level = level
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult();

            LevelUpRewardsResponse levelUpRewardsResponse = LevelUpRewardsResponse.Parser.ParseFrom(response);
            string rewards = StringUtil.GetSummedFriendlyNameOfItemAwardList(levelUpRewardsResponse.ItemsAwarded);
            LogCaller(new LoggerEventArgs(String.Format("Grabbed rewards for level {0}. Rewards: {1}", level, rewards), LoggerTypes.LevelUp));
            */
            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult> GetPlayer(bool nobuddy = true, bool noinbox = true)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }
            /*
            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GetPlayer,
                RequestMessage = new GetPlayerMessage
                {
                    PlayerLocale = _client.PlayerLocale
                }.ToByteString()
            }, true, nobuddy, noinbox);

            if (response == null)
                return new MethodResult();

            var parsedResponse = GetPlayerResponse.Parser.ParseFrom(response);
            */
            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult> GetPlayerProfile()
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }
            /*
            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GetPlayerProfile,
                RequestMessage = new GetPlayerProfileMessage
                {
                    PlayerName = PlayerData.Username
                }.ToByteString()
            }, true, false, true);

            if (response == null)
                return new MethodResult();

            var parsedResponse = GetPlayerProfileResponse.Parser.ParseFrom(response);

            PlayerProfile = parsedResponse;
            */
            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult> SetPlayerTeam(/*TeamColor team*/)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }
            /*
            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.SetPlayerTeam,
                RequestMessage = new SetPlayerTeamMessage
                {
                    Team = team
                }.ToByteString()
            }, true);

            if (response == null)
                return new MethodResult();

            SetPlayerTeamResponse setPlayerTeamResponse = SetPlayerTeamResponse.Parser.ParseFrom(response);

            LogCaller(new LoggerEventArgs($"Set player Team completion request wasn't successful. Team: {team.ToString()}", LoggerTypes.Success));

            // not nedded pogolib set this auto
            //_client.ClientSession.Player.Data = setPlayerTeamResponse.PlayerData;
            */
            return new MethodResult
            {
                Success = true
            };
        }

        public async Task<MethodResult> SetBuddyCreature(/*CreatureData Creature*/)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }
            /*
            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.SetBuddyCreature,
                RequestMessage = new SetBuddyCreatureMessage
                {
                    CreatureId = Creature.Id
                }.ToByteString()
            }, true);

            if (response == null)
                return new MethodResult();

            SetBuddyCreatureResponse setBuddyCreatureResponse = SetBuddyCreatureResponse.Parser.ParseFrom(response);

            switch (setBuddyCreatureResponse.Result)
            {
                case SetBuddyCreatureResponse.Types.Result.ErrorInvalidCreature:
                    LogCaller(new LoggerEventArgs($"Faill to set buddy Creature, reason: {setBuddyCreatureResponse.Result.ToString()}", LoggerTypes.Warning));
                    break;
                case SetBuddyCreatureResponse.Types.Result.ErrorCreatureDeployed:
                    LogCaller(new LoggerEventArgs($"Faill to set buddy Creature, reason: {setBuddyCreatureResponse.Result.ToString()}", LoggerTypes.Warning));
                    break;
                case SetBuddyCreatureResponse.Types.Result.ErrorCreatureIsEgg:
                    LogCaller(new LoggerEventArgs($"Faill to set buddy Creature, reason: {setBuddyCreatureResponse.Result.ToString()}", LoggerTypes.Warning));
                    break;
                case SetBuddyCreatureResponse.Types.Result.ErrorCreatureNotOwned:
                    LogCaller(new LoggerEventArgs($"Faill to set buddy Creature, reason: {setBuddyCreatureResponse.Result.ToString()}", LoggerTypes.Warning));
                    break;
                case SetBuddyCreatureResponse.Types.Result.Success:
                    PlayerData.BuddyCreature = new BuddyCreature
                    {
                        Id = Creature.Id,
                        //LastKmAwarded = PokeSettings[Creature.CreatureId].KmBuddyDistance,
                        //StartKmWalked = PokeSettings[Creature.CreatureId].KmDistanceToHatch
                    };

                    setBuddyCreatureResponse.UpdatedBuddy = PlayerData.BuddyCreature;

                    LogCaller(new LoggerEventArgs($"Set buddy Creature completion request wasn't successful. Creature buddy: {Creature.CreatureId.ToString()}", LoggerTypes.Buddy));

                    UpdateInventory(InventoryRefresh.Creature);

                    return new MethodResult
                    {
                        Success = true
                    };
                case SetBuddyCreatureResponse.Types.Result.Unest:
                    LogCaller(new LoggerEventArgs($"Faill to set buddy Creature, reason: {setBuddyCreatureResponse.Result.ToString()}", LoggerTypes.Warning));
                    break;
            }*/
            return new MethodResult();
        }

        private async Task<MethodResult> ExportToPGPool()
        {
            if (UserSettings.EnablePGPool && !String.IsNullOrEmpty(UserSettings.PGPoolEndpoint))
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(UserSettings.PGPoolEndpoint);
                        var content = new StringContent($"level={UserSettings.MaxLevel}&condition=good&accounts={UserSettings.AuthType.ToString().ToLower()}," + UserSettings.AccountName + "," + UserSettings.Password, Encoding.UTF8, "application/x-www-form-urlencoded");

                        using (var request = new HttpRequestMessage(HttpMethod.Post, "account/add"))
                        {
                            request.Content = content;

                            await client.SendAsync(request).ContinueWith(async responseTask =>
                            {
                                var res = await responseTask.Result.Content.ReadAsStringAsync();
                                if (!res.Contains("Successfully added"))
                                {
                                    LogCaller(new LoggerEventArgs(String.Format(res), LoggerTypes.Info));
                                    LogCaller(new LoggerEventArgs(String.Format("Error Sending Account To PGPool!"), LoggerTypes.Warning));
                                    LogCaller(new LoggerEventArgs(String.Format("PGPool Response: {0}", responseTask.Result), LoggerTypes.Warning));
                                }
                                else
                                {
                                    LogCaller(new LoggerEventArgs(String.Format(res), LoggerTypes.Info));
                                    LogCaller(new LoggerEventArgs(String.Format("Account successfully sent to PGPool"), LoggerTypes.Success));
                                    UserSettings.GroupName = $"PGPool lv{UserSettings.MaxLevel}";
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogCaller(new LoggerEventArgs(String.Format(ex.Message), LoggerTypes.Warning));
                }
            }
            return new MethodResult();
        }
    }
}