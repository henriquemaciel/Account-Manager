using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.Models;
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
            MethodResult<List<FUserCreature>> response = GetCreatureToEvolve();

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

            if (UserSettings.UseCristal && !UserSettings.UseCristalConst)
            {
                MethodResult result = await UseCristal();

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

            return new MethodResult();
        }

        public async Task<MethodResult> EvolveCreature(IEnumerable<FUserCreature> CreatureToEvolve)
        {
            //Shouldn't happen
            if (CreatureToEvolve.Count() < 1)
            {
                LogCaller(new LoggerEventArgs("Null value sent to evolve Creature", LoggerTypes.Debug));

                return new MethodResult();
            }

            foreach (FUserCreature Creature in CreatureToEvolve)
            {
                if (Creature == null)
                {
                    LogCaller(new LoggerEventArgs("Null Creature data in IEnumerable", LoggerTypes.Debug));

                    continue;
                }

                if (Creature.isArenaDefender || Creature.isLibraryDefender)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Creature {0} is in library or arena.", Strings.GetCreatureName(Creature.name)), LoggerTypes.Warning));
                    //await TransferCreature(new List<CreatureData> { Creature });
                    continue;
                }

                if (!CanEvolveCreature(Creature))
                {
                    LogCaller(new LoggerEventArgs(String.Format("Skipped {0}, this Creature cant not be evolued maybe is deployed Creature or you not have needed resources.", Strings.GetCreatureName(Creature.name)), LoggerTypes.Info));
                    continue;
                }

                //CreatureSettings CreatureSettings = GetCreatureSetting((Creature).CreatureId).Data;
                //ItemId itemNeeded = CreatureSettings.EvolutionBranch.Select(x => x.EvolutionItemRequirement).FirstOrDefault();

                if (!_client.LoggedIn)
                {
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                var totype = Creature.possibleEvolutions.Keys.FirstOrDefault();
                var response = _client.DracoClient.Call(new UserCreatureService().EvolveCreature(Creature.id, totype));

                if (response == null)
                    return new MethodResult();

                int exp = response.loot.GetExp();
                ExpIncrease(exp);
                //_expGained += evolveCreatureResponse.ExperienceAwarded;

                LogCaller(new LoggerEventArgs(
                        String.Format("Successully evolved {0} to {1}. Experience: {2}. Cp: {3}. IV: {4:0.00}%",
                                    Strings.GetCreatureName(Creature.name),
                                    Strings.GetCreatureName(totype),
                                    exp,
                                    Creature.cp,
                                    CalculateIVPerfection(Creature)),
                                    LoggerTypes.Evolve));

                UpdateInventory(InventoryRefresh.Creature);
                UpdateInventory(InventoryRefresh.CreatureCandy);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
            }

            return new MethodResult
            {
                Success = true
            };
        }

        private async Task<MethodResult<int>> GetEvolutionCandy(CreatureType CreatureId)
        {
            //remove warn
            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
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
            if (!UserSettings.EvolveCreature)
            {
                LogCaller(new LoggerEventArgs("Evolving disabled", LoggerTypes.Info));

                return new MethodResult<List<FUserCreature>>
                {
                    Data = new List<FUserCreature>(),
                    Message = "Evolving disabled",
                    Success = true
                };
            }

            var CreatureToEvolve = new List<FUserCreature>();

            IEnumerable<IGrouping<CreatureType, FUserCreature>> groupedCreature = Creatures.OrderByDescending(x => x.name).GroupBy(x => x.name);

            foreach (IGrouping<CreatureType, FUserCreature> group in groupedCreature)
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
                
                var setting = Creatures.FirstOrDefault().possibleEvolutions.Keys.FirstOrDefault();
                /*
                if (!PokeSettings.TryGetValue(group.Key, out setting))
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to find settings for Creature {0}", group.Key), LoggerTypes.Info));

                    continue;
                }
                */
                //Candy CreatureCandy = CreatureCandy.FirstOrDefault(x => x.FamilyId == setting.FamilyId);
                List<FUserCreature> CreatureGroupToEvolve = group.Where(x => x.cp >= evolveSetting.MinCP).OrderByDescending(x => CalculateIVPerfection(x)).ToList();
                /*
                if (CreatureCandy == null)
                {
                    LogCaller(new LoggerEventArgs(String.Format("No candy found for Creature {0}", group.Key), LoggerTypes.Info));

                    continue;
                }
                */
                int candyToEvolve = Creatures.FirstOrDefault().improveCandiesCost;

                if (candyToEvolve == 0)
                {
                    LogCaller(new LoggerEventArgs(String.Format("No evolution for Creature {0}", group.Key), LoggerTypes.Info));

                    continue;
                }

                int totalCreature = CreatureGroupToEvolve.Count;
                int totalCandy = Creatures.FirstOrDefault().GetCandyCount(Stats);

                int maxCreature = totalCandy / candyToEvolve;

                foreach (FUserCreature pData in CreatureGroupToEvolve.Take(maxCreature))
                {
                    if (!CanEvoleCreature(pData))
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Skipped {0}, this Creature cant not be evolued maybe is a favorit, is deployed or is a buddy Creature.", Strings.GetCreatureName(pData.name)), LoggerTypes.Info));
                        continue;
                    }
                    else
                        CreatureToEvolve.Add(pData);
                }
            }

            return new MethodResult<List<FUserCreature>>
            {
                Data = CreatureToEvolve,
                Message = "Success",
                Success = true
            };
        }

        private async Task<MethodResult> UseCristal()
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return result;
                }
            }

            if (CristalActive)
            {
                return new MethodResult
                {
                    Message = "Cristak already active",
                    Success = true                    
                };
            }

            var data = Items.FirstOrDefault(x => x.type == ItemType.EXPERIENCE_BOOSTER);

            if (data == null || data.count == 0)
            {
                return new MethodResult();
            }

            var response = _client.DracoClient.Call(new ItemService().UseExperienceBooster());
 
            if (response == null)
                return new MethodResult();

            LogCaller(new LoggerEventArgs(String.Format("Cristal used. Remaining: {0}", data.count - 1), LoggerTypes.Success));
            UseCristaldateTime = DateTime.Now.AddMinutes(30);

            return new MethodResult
            {
                Success = true
            };
        }

        public double FilledCreatureInventorySpace()
        {
            if (Creatures == null || PlayerData == null)
            {
                return 0;
            }

            return (double)(Creatures.Count) / Stats.creatureStorageSize * 100;
        }

        private bool CanEvolveCreature(FUserCreature Creature)
        {
            
            // Can't evolve Creature in gyms.
            if (Creature.isArenaDefender || Creature.isLibraryDefender)
                return false;

            // Can't evolve Creature that are not evolvable.
            if (Creature.possibleEvolutions.Count == 0)
                return false;

            /*int familyCandy = CreatureCandy.FirstOrDefault(x => x.Key == Creature.candyType).Value;

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
            return true;// canEvolve;
        }
    }
}
