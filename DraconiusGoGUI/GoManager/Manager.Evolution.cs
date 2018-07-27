using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DracoProtos.Core.Base;
using DracoProtos.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        private async Task<MethodResult> EvolveFilteredCreature()
        {
            /*
            MethodResult<List<CreatureData>> response = GetCreatureToEvolve();

            if (response.Data.Count == 0)
            {
                return new MethodResult();
            }

            LogCaller(new LoggerEventArgs(String.Format("{0} Creature to evolve", response.Data.Count), LoggerTypes.Info));

            if (FilledCreatureInventorySpace() <= UserSettings.ForceEvolveAbovePercent)
            {
                LogCaller(new LoggerEventArgs(String.Format("Not enough Creature inventory space {0:0.00}% of {1:0.00}% force evolve above percent.", FilledCreatureInventorySpace(), UserSettings.ForceEvolveAbovePercent), LoggerTypes.Info));

                return new MethodResult();
            }

            if (response.Data.Count < UserSettings.MinCreatureBeforeEvolve)
            {
                LogCaller(new LoggerEventArgs(String.Format("Not enough Creature to evolve. {0} of {1} evolvable Creature.", response.Data.Count, UserSettings.MinCreatureBeforeEvolve), LoggerTypes.Info));

                return new MethodResult();
            }

            if (UserSettings.UseLuckyEgg && !UserSettings.UseLuckEggConst)
            {
                MethodResult result = await UseLuckyEgg();

                if (!result.Success)
                {
                    LogCaller(new LoggerEventArgs("Failed to use lucky egg. Possibly already active. Continuing evolving", LoggerTypes.Info));
                }
            }

            MethodResult evole = await EvolveCreature(response.Data);
            if (evole.Success)
            {
                return new MethodResult
                {
                    Success = true,
                    Message = "Success"
                };
            }
            */
            return new MethodResult();
        }

        public async Task<MethodResult> EvolveCreature(IEnumerable<FUserCreature> CreatureToEvolve)
        {
            /*
            //Shouldn't happen
            if (CreatureToEvolve.Count() < 1)
            {
                LogCaller(new LoggerEventArgs("Null value sent to evolve Creature", LoggerTypes.Debug));

                return new MethodResult();
            }

            foreach (CreatureData Creature in CreatureToEvolve)
            {
                if (Creature == null)
                {
                    LogCaller(new LoggerEventArgs("Null Creature data in IEnumerable", LoggerTypes.Debug));

                    continue;
                }

                if (Creature.IsBad)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Creature {0} is slashed.", Creature.CreatureId), LoggerTypes.Warning));
                    //await TransferCreature(new List<CreatureData> { Creature });
                    continue;
                }

                if (!CanEvolveCreature(Creature))
                {
                    LogCaller(new LoggerEventArgs(String.Format("Skipped {0}, this Creature cant not be upgrated maybe is deployed Creature or you not have needed resources.", Creature.CreatureId), LoggerTypes.Info));
                    continue;
                }

                CreatureSettings CreatureSettings = GetCreatureSetting((Creature).CreatureId).Data;
                ItemId itemNeeded = CreatureSettings.EvolutionBranch.Select(x => x.EvolutionItemRequirement).FirstOrDefault();

                if (!_client.LoggedIn)
                {
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.EvolveCreature,
                    RequestMessage = new EvolveCreatureMessage
                    {
                        CreatureId = Creature.Id,
                        EvolutionItemRequirement = itemNeeded
                    }.ToByteString()
                });

                if (response == null)
                    return new MethodResult();

                EvolveCreatureResponse evolveCreatureResponse = EvolveCreatureResponse.Parser.ParseFrom(response);
                switch (evolveCreatureResponse.Result)
                {
                    case EvolveCreatureResponse.Types.Result.Success:
                        ExpIncrease(evolveCreatureResponse.ExperienceAwarded);
                        //_expGained += evolveCreatureResponse.ExperienceAwarded;

                        LogCaller(new LoggerEventArgs(
                                String.Format("Successully evolved {0} to {1}. Experience: {2}. Cp: {3} -> {4}. IV: {5:0.00}%",
                                            Creature.CreatureId,
                                            CreatureSettings.EvolutionBranch.Select(x => x.Evolution).FirstOrDefault(),
                                            evolveCreatureResponse.ExperienceAwarded,
                                            Creature.Cp,
                                            evolveCreatureResponse.EvolvedCreatureData.Cp,
                                            CalculateIVPerfection(evolveCreatureResponse.EvolvedCreatureData)),
                                            LoggerTypes.Evolve));

                        UpdateInventory(InventoryRefresh.Creature);
                        UpdateInventory(InventoryRefresh.CreatureCandy);

                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                        continue;
                    case EvolveCreatureResponse.Types.Result.FailedInsufficientResources:
                        LogCaller(new LoggerEventArgs("Evolve request failed: Failed Insufficient Resources", LoggerTypes.Warning));
                        continue;
                    case EvolveCreatureResponse.Types.Result.FailedInvalidItemRequirement:
                        LogCaller(new LoggerEventArgs("Evolve request failed: Failed Invalid Item Requirement", LoggerTypes.Warning));
                        continue;
                    case EvolveCreatureResponse.Types.Result.FailedCreatureCannotEvolve:
                        LogCaller(new LoggerEventArgs("Evolve request failed: Failed Creature Cannot Evolve", LoggerTypes.Warning));
                        continue;
                    case EvolveCreatureResponse.Types.Result.FailedCreatureIsDeployed:
                        LogCaller(new LoggerEventArgs("Evolve request failed: Failed Creature IsDeployed", LoggerTypes.Warning));
                        continue;
                    case EvolveCreatureResponse.Types.Result.FailedCreatureMissing:
                        LogCaller(new LoggerEventArgs("Evolve request failed: Failed Creature Missing", LoggerTypes.Warning));
                        continue;
                    case EvolveCreatureResponse.Types.Result.Unset:
                        LogCaller(new LoggerEventArgs("Evolve request failed", LoggerTypes.Warning));
                        continue;
                }
            }
            */
            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult<int>> GetEvolutionCandy(CreatureType CreatureId)
        {
            /*
            if (PokeSettings == null)
            {
                MethodResult result = await GetItemTemplates();

                if (!result.Success)
                {
                    return (MethodResult<int>)result;
                }
            }

            MethodResult<CreatureSettings> settingsResult = GetCreatureSetting(CreatureId);

            if (!settingsResult.Success)
            {
                return new MethodResult<int>
                {
                    Message = settingsResult.Message
                };
            }
            */

            return new MethodResult<int>
            {
                //Data = settingsResult.Data.EvolutionBranch.Select(x => x.CandyCost).FirstOrDefault(),
                Message = "Success",
                Success = true
            };
            
        }

        private MethodResult<List<FUserCreature>> GetCreatureToEvolve()
        {
            /*
            if (!UserSettings.EvolveCreature)
            {
                LogCaller(new LoggerEventArgs("Evolving disabled", LoggerTypes.Info));

                return new MethodResult<List<CreatureData>>
                {
                    Data = new List<CreatureData>(),
                    Message = "Evolving disabled",
                    Success = true
                };
            }

            var CreatureToEvolve = new List<CreatureData>();

            IEnumerable<IGrouping<CreatureId, CreatureData>> groupedCreature = Creature.OrderByDescending(x => x.CreatureId).GroupBy(x => x.CreatureId);

            foreach (IGrouping<CreatureId, CreatureData> group in groupedCreature)
            {
                EvolveSetting evolveSetting = UserSettings.EvolveSettings.FirstOrDefault(x => x.Id == group.Key);

                if (evolveSetting == null)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to find evolve settings for Creature {0}", group.Key), LoggerTypes.Info));

                    continue;
                }

                if (!evolveSetting.Evolve)
                {
                    //Don't evolve
                    continue;
                }
                CreatureSettings setting;
                if (!PokeSettings.TryGetValue(group.Key, out setting))
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to find settings for Creature {0}", group.Key), LoggerTypes.Info));

                    continue;
                }

                Candy CreatureCandy = CreatureCandy.FirstOrDefault(x => x.FamilyId == setting.FamilyId);
                List<CreatureData> CreatureGroupToEvolve = group.Where(x => x.Cp >= evolveSetting.MinCP).OrderByDescending(x => CalculateIVPerfection(x)).ToList();

                if (CreatureCandy == null)
                {
                    LogCaller(new LoggerEventArgs(String.Format("No candy found for Creature {0}", group.Key), LoggerTypes.Info));

                    continue;
                }

                int candyToEvolve = setting.EvolutionBranch.Select(x => x.CandyCost).FirstOrDefault();

                if (candyToEvolve == 0)
                {
                    LogCaller(new LoggerEventArgs(String.Format("No evolution for Creature {0}", group.Key), LoggerTypes.Info));

                    continue;
                }

                int totalCreature = CreatureGroupToEvolve.Count;
                int totalCandy = CreatureCandy.Candy_;

                int maxCreature = totalCandy / candyToEvolve;

                foreach (CreatureData pData in CreatureGroupToEvolve.Take(maxCreature))
                {
                    if (!CanTransferOrEvoleCreature(pData, true))
                        LogCaller(new LoggerEventArgs(String.Format("Skipped {0}, this Creature cant not be transfered maybe is a favorit, is deployed or is a buddy Creature.", pData.CreatureId), LoggerTypes.Info));
                    else
                        CreatureToEvolve.Add(pData);
                }
            }
            */

            return new MethodResult<List<FUserCreature>>
            {
                //Data = CreatureToEvolve,
                Message = "Success",
                Success = true
            };
        }

        private async Task<MethodResult> UseLuckyEgg()
        {
            /*
            if (UsedAlready)
            {
                return new MethodResult
                {
                    Message = "Lucky egg already active"
                };
            }
            */
            return new MethodResult { Message = Strings.GetItemName(ItemType.EXPERIENCE_BOOSTER) + " Not released yet" };

            var data = Items.FirstOrDefault(x => x.type == ItemType.EXPERIENCE_BOOSTER);

            if (data == null || data.count == 0)
            {
                LogCaller(new LoggerEventArgs("No lucky eggs left", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "No lucky eggs"
                };
            }

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }

            var response = await _client.DracoClient.CallAsync(new ItemService().UseExperienceBooster());
 
            if (response == null)
                return new MethodResult();

            LogCaller(new LoggerEventArgs(String.Format("Lucky egg used. Remaining: {0}", data.count - 1), LoggerTypes.Success));

            return new MethodResult
            {
                Success = true
            };
        }

        public double FilledCreatureInventorySpace()
        {
            if (Creature == null || PlayerData == null)
            {
                return 0;
            }

            return (double)(Creature.Count) / Stats.creatureStorageSize * 100;
        }

        private bool CanEvolveCreature(FUserCreature Creature)
        {
            /*
            // Can't evolve Creature in gyms.
            if (!string.IsNullOrEmpty(Creature.DeployedBuildingId))
                return false;

            var settings = PokeSettings.SingleOrDefault(x => x.Value.CreatureId == Creature.CreatureId);

            // Can't evolve Creature that are not evolvable.
            if (settings.Value.EvolutionIds.Count == 0 && settings.Value.EvolutionBranch.Count == 0)
                return false;

            int familyCandy = CreatureCandy.FirstOrDefault(x => x.FamilyId == settings.Value.FamilyId).Candy_;

            bool canEvolve = false;
            // Check requirements for all branches, if we meet the requirements for any of them then we return true.
            foreach (var branch in settings.Value.EvolutionBranch)
            {
                var itemCount = Items.Count(x => x.ItemId == branch.EvolutionItemRequirement);
                var Candies2Evolve = branch.CandyCost; // GetCandyToEvolve(settings);
                var Evolutions = familyCandy / Candies2Evolve;

                if (branch.EvolutionItemRequirement != ItemId.ItemUnknown)
                {
                    if (itemCount == 0)
                        continue;  // Cannot evolve so check next branch
                }

                if (familyCandy < branch.CandyCost)
                    continue;  // Cannot evolve so check next branch

                // If we got here, then we can evolve so break out of loop.
                canEvolve = true;
            }
            */
            return false;// canEvolve;
        }
    }
}
