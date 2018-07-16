using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        /*
        public async Task<MethodResult> TransferCreature(IEnumerable<CreatureData> CreaturesToTransfer)
        {
            List<CreatureData> CreatureToTransfer = new List<CreatureData>();

            foreach (var pokToTranfer in CreaturesToTransfer)
            {
                if (!CanTransferOrEvoleCreature(pokToTranfer))
                    LogCaller(new LoggerEventArgs(String.Format("Skipped {0}, this Creature cant not be transfered maybe is a favorit, is deployed or is a buddy Creature.", pokToTranfer.CreatureId), LoggerTypes.Info));
                else
                    CreatureToTransfer.Add(pokToTranfer);
            }

            if (CreaturesToTransfer.Count() == 0 || CreaturesToTransfer.FirstOrDefault() == null)
                return new MethodResult();

            LogCaller(new LoggerEventArgs(String.Format("Found {0} Creature to transfer", CreatureToTransfer.Count()), LoggerTypes.Info));

            if (!UserSettings.TransferAtOnce)
            {
                foreach (CreatureData Creature in CreatureToTransfer)
                {
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
                        RequestType = RequestType.ReleaseCreature,
                        RequestMessage = new ReleaseCreatureMessage
                        {
                            CreatureId = Creature.Id
                        }.ToByteString()
                    });

                    if (response == null)
                        return new MethodResult();

                    ReleaseCreatureResponse releaseCreatureResponse = ReleaseCreatureResponse.Parser.ParseFrom(response);
                    switch (releaseCreatureResponse.Result)
                    {
                        case ReleaseCreatureResponse.Types.Result.Success:
                            LogCaller(new LoggerEventArgs(String.Format("Successully transferred {0}. Cp: {1}. IV: {2:0.00}%",
                                Creature.CreatureId,
                                Creature.Cp,
                                CalculateIVPerfection(Creature)),
                                LoggerTypes.Transfer));

                            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                            RemoveInventoryItem(GetCreatureHashKey(Creature.Id));
                            UpdateInventory(InventoryRefresh.CreatureCandy);
                            continue;
                        case ReleaseCreatureResponse.Types.Result.ErrorCreatureIsBuddy:
                            LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                                Creature.CreatureId,
                                releaseCreatureResponse.Result), LoggerTypes.Warning));
                            continue;
                        case ReleaseCreatureResponse.Types.Result.ErrorCreatureIsEgg:
                            LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                                Creature.CreatureId,
                                releaseCreatureResponse.Result), LoggerTypes.Warning));
                            continue;
                        case ReleaseCreatureResponse.Types.Result.CreatureDeployed:
                            LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                                Creature.CreatureId,
                                releaseCreatureResponse.Result), LoggerTypes.Warning));
                            continue;
                        case ReleaseCreatureResponse.Types.Result.Failed:
                            LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}",
                                Creature.CreatureId), LoggerTypes.Warning));
                            continue;
                        case ReleaseCreatureResponse.Types.Result.Unset:
                            LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                                Creature.CreatureId,
                                releaseCreatureResponse.Result), LoggerTypes.Warning));
                            continue;
                    }
                }

                UpdateInventory(InventoryRefresh.Creature);

                return new MethodResult
                {
                    Success = true
                };
            }
            else
            {
                var CreatureIds = CreatureToTransfer.Select(x => x.Id);

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
                    RequestType = RequestType.ReleaseCreature,
                    RequestMessage = new ReleaseCreatureMessage
                    {
                        CreatureIds = { CreatureIds }
                    }.ToByteString()
                });

                if (response == null)
                    return new MethodResult();

                ReleaseCreatureResponse releaseCreatureResponse = ReleaseCreatureResponse.Parser.ParseFrom(response);

                switch (releaseCreatureResponse.Result)
                {
                    case ReleaseCreatureResponse.Types.Result.Success:
                        LogCaller(new LoggerEventArgs(
                            String.Format("Successully candy awarded {0} of {1} Creatures.",
                                releaseCreatureResponse.CandyAwarded,
                                CreatureToTransfer.Count()),
                            LoggerTypes.Transfer));

                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                        foreach (var CreatureId in CreatureIds)
                        {
                            RemoveInventoryItem(GetCreatureHashKey(CreatureId));
                        }
                        UpdateInventory(InventoryRefresh.CreatureCandy);
                        break;
                    case ReleaseCreatureResponse.Types.Result.ErrorCreatureIsBuddy:
                        LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                            CreatureToTransfer.Count(),
                            releaseCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                    case ReleaseCreatureResponse.Types.Result.ErrorCreatureIsEgg:
                        LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                            CreatureToTransfer.Count(),
                            releaseCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                    case ReleaseCreatureResponse.Types.Result.CreatureDeployed:
                        LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                            CreatureToTransfer.Count(),
                            releaseCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                    case ReleaseCreatureResponse.Types.Result.Failed:
                        LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}",
                            CreatureToTransfer.Count()), LoggerTypes.Warning));
                        break;
                    case ReleaseCreatureResponse.Types.Result.Unset:
                        LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                            CreatureToTransfer.Count(),
                            releaseCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                }

                UpdateInventory(InventoryRefresh.Creature);

                return new MethodResult
                {
                    Success = true
                };
            }
        }

        private async Task<MethodResult> TransferFilteredCreature()
        {

            double configPercentCreatures = UserSettings.PercTransPoke * 0.01;

            double percentCreature = PlayerData.MaxCreatureStorage * configPercentCreatures;

            if (percentCreature > Creature.Count)
            {
                return new MethodResult
                {
                    Message = "Not yet reached 90% of the storage of Pokémon"
                };
            }

            MethodResult<List<CreatureData>> transferResult = GetCreatureToTransfer();

            if (!transferResult.Success || transferResult.Data.Count == 0)
            {
                return new MethodResult();
            }

            await TransferCreature(transferResult.Data);

            return new MethodResult
            {
                Success = true,
                Message = "Success"
            };
        }

        public MethodResult<List<CreatureData>> GetCreatureToTransfer()
        {
            if (!UserSettings.TransferCreature)
            {
                LogCaller(new LoggerEventArgs("Transferring disabled", LoggerTypes.Debug));

                return new MethodResult<List<CreatureData>>
                {
                    Data = new List<CreatureData>(),
                    Message = "Transferring disabled",
                    Success = true
                };
            }

            if (!Creature.Any())
            {
                LogCaller(new LoggerEventArgs("You have no Creature", LoggerTypes.Info));

                return new MethodResult<List<CreatureData>>
                {
                    Message = "You have no Creature"
                };
            }

            var CreatureToTransfer = new List<CreatureData>();

            IEnumerable<IGrouping<CreatureId, CreatureData>> groupedCreature = Creature.GroupBy(x => x.CreatureId);

            foreach (IGrouping<CreatureId, CreatureData> group in groupedCreature)
            {
                TransferSetting settings = UserSettings.TransferSettings.FirstOrDefault(x => x.Id == group.Key);

                if (settings == null)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to find transfer settings for Creature {0}", group.Key), LoggerTypes.Warning));

                    continue;
                }

                if (!settings.Transfer)
                {
                    continue;
                }

                switch (settings.Type)
                {
                    case TransferType.All:
                        CreatureToTransfer.AddRange(group.ToList());
                        break;
                    case TransferType.BelowCP:
                        CreatureToTransfer.AddRange(GetCreatureBelowCP(group, settings.MinCP));
                        break;
                    case TransferType.BelowIVPercentage:
                        CreatureToTransfer.AddRange(GetCreatureBelowIVPercent(group, settings.IVPercent));
                        break;
                    case TransferType.KeepPossibleEvolves:
                        CreatureToTransfer.AddRange(GetCreatureByPossibleEvolve(group, settings.KeepMax));
                        break;
                    case TransferType.KeepStrongestX:
                        CreatureToTransfer.AddRange(GetCreatureByStrongest(group, settings.KeepMax));
                        break;
                    case TransferType.KeepXHighestIV:
                        CreatureToTransfer.AddRange(GetCreatureByIV(group, settings.KeepMax));
                        break;
                    case TransferType.BelowCPAndIVAmount:
                        CreatureToTransfer.AddRange(GetCreatureBelowCPIVAmount(group, settings.MinCP, settings.IVPercent));
                        break;
                    case TransferType.BelowCPOrIVAmount:
                        CreatureToTransfer.AddRange(GetCreatureBelowIVPercent(group, settings.IVPercent));
                        CreatureToTransfer.AddRange(GetCreatureBelowCP(group, settings.MinCP));
                        CreatureToTransfer = CreatureToTransfer.DistinctBy(x => x.Id).ToList();
                        break;
                    case TransferType.Slashed:
                        CreatureToTransfer.AddRange(group.ToList());
                        CreatureToTransfer = CreatureToTransfer.DistinctBy(x => x.IsBad).ToList();
                        break;
                }
            }

            if (UserSettings.TransferSlashCreatures)
            {
                var slashCreatures = Creature.Where(x => x.IsBad);
                foreach (var slashCreature in slashCreatures)
                {
                    var inlist = CreatureToTransfer.FirstOrDefault(x => x.Id == slashCreature.Id);
                    if (inlist == null)
                    {
                        CreatureToTransfer.Add(slashCreature);
                    }
                }
            }

            return new MethodResult<List<CreatureData>>
            {
                Data = CreatureToTransfer,
                Message = String.Format("Found {0} Creature to transfer", CreatureToTransfer.Count),
                Success = true
            };
        }

        private List<CreatureData> GetCreatureBelowCPIVAmount(IGrouping<CreatureId, CreatureData> Creature, int minCp, double percent)
        {
            var toTransfer = new List<CreatureData>();

            foreach (CreatureData pData in Creature)
            {
                double perfectResult = CalculateIVPerfection(pData);

                if (perfectResult >= 0 && perfectResult < percent && pData.Cp < minCp)
                {
                    toTransfer.Add(pData);
                }
            }

            return toTransfer;
        }

        private List<CreatureData> GetCreatureBelowCP(IGrouping<CreatureId, CreatureData> Creature, int minCp)
        {
            return Creature.Where(x => x.Cp < minCp).ToList();
        }

        private List<CreatureData> GetCreatureBelowIVPercent(IGrouping<CreatureId, CreatureData> Creature, double percent)
        {
            var toTransfer = new List<CreatureData>();

            foreach (CreatureData pData in Creature)
            {
                double perfectResult = CalculateIVPerfection(pData);

                if (perfectResult >= 0 && perfectResult < percent)
                {
                    toTransfer.Add(pData);
                }
            }

            return toTransfer;
        }

        private List<CreatureData> GetCreatureByStrongest(IGrouping<CreatureId, CreatureData> Creature, int amount)
        {
            return Creature.OrderByDescending(x => x.Cp).Skip(amount).ToList();
        }

        private List<CreatureData> GetCreatureByIV(IGrouping<CreatureId, CreatureData> Creature, int amount)
        {
            if (!Creature.Any())
            {
                return new List<CreatureData>();
            }

            //Test out first one to make sure things are correct
            double perfectResult = CalculateIVPerfection(Creature.First());

            return Creature.OrderByDescending(x => CalculateIVPerfection(x)).ThenByDescending(x => x.Cp).Skip(amount).ToList();
        }

        private List<CreatureData> GetCreatureByPossibleEvolve(IGrouping<CreatureId, CreatureData> Creature, int limit)
        {
            CreatureSettings setting = null;
            if (!PokeSettings.TryGetValue(Creature.Key, out setting))
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to find settings for Creature {0}", Creature.Key), LoggerTypes.Info));

                return new List<CreatureData>();
            }

            int CreatureCandy = 0;

            if (CreatureCandy.Any(x => x.FamilyId == setting.FamilyId))
            {
                CreatureCandy = CreatureCandy.Where(x => x.FamilyId == setting.FamilyId).FirstOrDefault().Candy_;
                //int CreatureCandy = CreatureCandy.SingleOrDefault(x => x.FamilyId == setting.FamilyId).Candy_;
            }

            int candyToEvolve = setting.EvolutionBranch.Select(x => x.CandyCost).FirstOrDefault();
            int totalCreature = Creature.Count();

            if (candyToEvolve == 0)
            {
                //Not thinks good
                return Creature.OrderByDescending(x => x.Cp).ToList();
                //return new List<CreatureData>();
            }

            int maxCreature = CreatureCandy / candyToEvolve;

            if (maxCreature > limit)
            {
                maxCreature = limit;
            }

            return Creature.OrderByDescending(x => x.Cp).Skip(maxCreature).ToList();
        }

        // NOTE: this is the real IV Percent, using only Individual values.
        public static double CalculateIVPerfection(CreatureData Creature)
        {
            // NOTE: 45 points = 15 at points + 15 def points + 15 sta points
            //  100/45 simplifying is 20/9
            return ((double)Creature.IndividualAttack + Creature.IndividualDefense + Creature.IndividualStamina) * 20 / 9;
        }

        // This other Percent gives different IV % for the same IVs depending of the Creature level.
        public MethodResult<double> CalculateIVPerfectionUsingMaxCP(CreatureData Creature)
        {
            MethodResult<CreatureSettings> settingResult = GetCreatureSetting(Creature.CreatureId);

            if (!settingResult.Success)
            {
                return new MethodResult<double>
                {
                    Data = 0,
                    Message = settingResult.Message
                };
            }

            double maxCp = CalculateMaxCpMultiplier(Creature);
            double minCp = CalculateMinCpMultiplier(Creature);
            double curCp = CalculateCpMultiplier(Creature);

            double perfectPercent = (curCp - minCp) / (maxCp - minCp) * 100.0;

            return new MethodResult<double>
            {
                Data = perfectPercent,
                Message = "Success",
                Success = true
            };
        }

        public async Task<MethodResult> FavoriteCreature(IEnumerable<CreatureData> CreatureToFavorite, bool favorite = true)
        {
            foreach (CreatureData Creature in CreatureToFavorite)
            {
                bool isFavorited = true;
                string message = "unfavorited";

                if (Creature.Favorite == 0)
                {
                    isFavorited = false;
                    message = "favorited";
                }

                if (isFavorited == favorite)
                {
                    continue;
                }

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
                    RequestType = RequestType.SetFavoriteCreature,
                    RequestMessage = new SetFavoriteCreatureMessage
                    {
                        CreatureId = (long)Creature.Id,
                        IsFavorite = favorite
                    }.ToByteString()
                });

                if (response == null)
                    return new MethodResult();

                SetFavoriteCreatureResponse setFavoriteCreatureResponse = null;

                setFavoriteCreatureResponse = SetFavoriteCreatureResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs(
                    String.Format("Successully {3} {0}. Cp: {1}. IV: {2:0.00}%",
                        Creature.CreatureId,
                        Creature.Cp,
                        CalculateIVPerfection(Creature), message),
                    LoggerTypes.Success));

                UpdateInventory(InventoryRefresh.Creature);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                return new MethodResult
                {
                    Success = true
                };
            }
            return new MethodResult();
        }

        public async Task<MethodResult> RenameCreature(IEnumerable<CreatureData> CreatureToRename)
        {
            foreach (CreatureData Creature in CreatureToRename)
            {
                string input = Prompt.ShowDialog($"New nickname for {Creature.CreatureId.ToString()}", "Rename");

                if (String.IsNullOrEmpty(input))
                {
                    input = String.Empty;
                }

                if (input == Creature.Nickname)
                {
                    continue;
                }               

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
                    RequestType = RequestType.NicknameCreature,
                    RequestMessage = new NicknameCreatureMessage
                    {
                        CreatureId = Creature.Id,
                        Nickname = input
                    }.ToByteString()
                });

                if (response == null)
                    return new MethodResult();

                NicknameCreatureResponse nicknameCreatureResponse = null;

                nicknameCreatureResponse = NicknameCreatureResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs(
                    String.Format("Successully  renamed: {0} to: {1}.",
                        Creature.CreatureId,
                        String.IsNullOrEmpty(input) ? $"Default [{Creature.CreatureId.ToString()}]" : input), LoggerTypes.Success));

                UpdateInventory(InventoryRefresh.Creature);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                return new MethodResult
                {
                    Success = true
                };
            }
            return new MethodResult();
        }

        private double CalculateMaxCpMultiplier(CreatureData poke)
        {
            CreatureSettings CreatureSettings = GetCreatureSetting(poke.CreatureId).Data;

            return (CreatureSettings.Stats.BaseAttack + 15) * Math.Sqrt(CreatureSettings.Stats.BaseDefense + 15) *
            Math.Sqrt(CreatureSettings.Stats.BaseStamina + 15);
        }

        private double CalculateCpMultiplier(CreatureData poke)
        {
            CreatureSettings CreatureSettings = GetCreatureSetting(poke.CreatureId).Data;

            return (CreatureSettings.Stats.BaseAttack + poke.IndividualAttack) *
            Math.Sqrt(CreatureSettings.Stats.BaseDefense + poke.IndividualDefense) *
            Math.Sqrt(CreatureSettings.Stats.BaseStamina + poke.IndividualStamina);
        }

        private double CalculateMinCpMultiplier(CreatureData poke)
        {
            CreatureSettings CreatureSettings = GetCreatureSetting(poke.CreatureId).Data;

            return CreatureSettings.Stats.BaseAttack * Math.Sqrt(CreatureSettings.Stats.BaseDefense) * Math.Sqrt(CreatureSettings.Stats.BaseStamina);
        }

        public async Task<MethodResult> UpgradeCreature(IEnumerable<CreatureData> CreaturesToUpgrade)
        {
            if (CreaturesToUpgrade.Count() == 0 || CreaturesToUpgrade.FirstOrDefault() == null)
                return new MethodResult();

            foreach (var Creature in CreaturesToUpgrade)
            {
                if (!CanUpgradeCreature(Creature))
                {
                    LogCaller(new LoggerEventArgs(String.Format("Skipped {0}, this Creature cant not be upgraded maybe is deployed Creature or you not have needed resources.", Creature.CreatureId), LoggerTypes.Info));
                    continue;
                }

                if (!_client.LoggedIn)
                {
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                int cpBefore = Creature.Cp;

                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.UpgradeCreature,
                    RequestMessage = new UpgradeCreatureMessage
                    {
                        CreatureId = Creature.Id
                    }.ToByteString()
                });

                if (response == null)
                    return new MethodResult();

                var upgradeCreatureResponse = UpgradeCreatureResponse.Parser.ParseFrom(response);

                switch (upgradeCreatureResponse.Result)
                {
                    case UpgradeCreatureResponse.Types.Result.Success:
                        UpdateInventory(InventoryRefresh.Creature);
                        UpdateInventory(InventoryRefresh.CreatureCandy);
                        LogCaller(new LoggerEventArgs(String.Format("Upgrade Creature {0} success, CP before: {1} CP after: {2}.", Creature.CreatureId, cpBefore, upgradeCreatureResponse.UpgradedCreature.Cp), LoggerTypes.Upgrade));
                        break;
                    case UpgradeCreatureResponse.Types.Result.ErrorInsufficientResources:
                        LogCaller(new LoggerEventArgs(String.Format("Failed to upgrade Creature. Response: {0}", upgradeCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                    case UpgradeCreatureResponse.Types.Result.ErrorCreatureIsDeployed:
                        LogCaller(new LoggerEventArgs(String.Format("Failed to upgrade Creature. Response: {0}", upgradeCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                    case UpgradeCreatureResponse.Types.Result.ErrorCreatureNotFound:
                        LogCaller(new LoggerEventArgs(String.Format("Failed to upgrade Creature. Response: {0}", upgradeCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                    case UpgradeCreatureResponse.Types.Result.ErrorUpgradeNotAvailable:
                        LogCaller(new LoggerEventArgs(String.Format("Failed to upgrade Creature. Response: {0}", upgradeCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                    case UpgradeCreatureResponse.Types.Result.Unset:
                        LogCaller(new LoggerEventArgs(String.Format("Failed to upgrade Creature. Response: {0}", upgradeCreatureResponse.Result), LoggerTypes.Warning));
                        break;
                }
            }

            return new MethodResult
            {
                Success = true,
                Message = "Success",
            };
        }

        public float GetLevelFromCpMultiplier(double combinedCpMultiplier)
        {
            double level;
            if (combinedCpMultiplier < 0.734f)
            {
                // compute polynomial approximation obtained by regression
                level = 58.35178527 * combinedCpMultiplier * combinedCpMultiplier
                        - 2.838007664 * combinedCpMultiplier + 0.8539209906;
            }
            else
            {
                // compute linear approximation obtained by regression
                level = 171.0112688 * combinedCpMultiplier - 95.20425243;
            }
            // round to nearest .5 value and return
            return (float)(Math.Round((level) * 2) / 2.0);
        }

        private bool CanTransferOrEvoleCreature(CreatureData Creature, bool allmodes = false)
        {
            // Can't transfer Creature null.
            if (Creature == null || Creature.CreatureId == CreatureId.Missingno)
                return false;

            // Can't transfer Creature check all modes.
            if (allmodes && Creature.IsBad)
                return false;

            // Can't transfer Creature in gyms.
            if (!string.IsNullOrEmpty(Creature.DeployedFortId))
                return false;

            // Can't transfer buddy Creature
            var buddy = PlayerData?.BuddyCreature;
            if (buddy != null && buddy.Id == Creature.Id)
                return false;

            // Can't transfer favorite
            if (Creature.Favorite == 1)
                return false;

            return true;
        }

        private bool CanUpgradeCreature(CreatureData Creature)
        {
            // Can't upgrade Creature in gyms.
            if (!string.IsNullOrEmpty(Creature.DeployedFortId))
                return false;

            int CreatureLevel = (int)GetLevelFromCpMultiplier(Creature.CpMultiplier + Creature.AdditionalCpMultiplier);

            // Can't evolve unless Creature level is lower than trainer.
            if (CreatureLevel >= Level + 2)
                return false;

            int familyCandy = CreatureCandy.Where(x => x.FamilyId == GetCreatureSetting(Creature.CreatureId).Data.FamilyId).FirstOrDefault().Candy_;

            // Can't evolve if not enough candy.
            int CreatureCandyNeededAlready = UpgradeSettings.CandyCost[CreatureLevel];
            if (familyCandy < CreatureCandyNeededAlready)
                return false;

            // Can't evolve if not enough stardust.
            var stardustToUpgrade = UpgradeSettings.StardustCost[CreatureLevel];
            if (TotalStardust < stardustToUpgrade)
                return false;

            return true;
        }

        private async Task<MethodResult> UpgradeFilteredCreature()
        {
            MethodResult<List<CreatureData>> upgradeResult = GetCreatureToUpgrade();

            if (!upgradeResult.Success || upgradeResult.Data.Count == 0)
            {
                return new MethodResult();
            }

            LogCaller(new LoggerEventArgs(upgradeResult.Message, LoggerTypes.Info));

            await UpgradeCreature(upgradeResult.Data);

            return new MethodResult
            {
                Success = true,
                Message = "Success"
            };
        }

        public MethodResult<List<CreatureData>> GetCreatureToUpgrade()
        {
            if (!UserSettings.UpgradeCreature)
            {
                LogCaller(new LoggerEventArgs("Upgrade disabled", LoggerTypes.Debug));

                return new MethodResult<List<CreatureData>>
                {
                    Data = new List<CreatureData>(),
                    Message = "Upgrade disabled",
                    Success = true
                };
            }

            if (!Creature.Any())
            {
                LogCaller(new LoggerEventArgs("You have no Creature", LoggerTypes.Info));

                return new MethodResult<List<CreatureData>>
                {
                    Message = "You have no Creature"
                };
            }

            var CreatureToUpgrade = new List<CreatureData>();

            IEnumerable<IGrouping<CreatureId, CreatureData>> groupedCreature = Creature.GroupBy(x => x.CreatureId);

            foreach (IGrouping<CreatureId, CreatureData> group in groupedCreature)
            {
                UpgradeSetting settings = UserSettings.UpgradeSettings.FirstOrDefault(x => x.Id == group.Key);

                if (settings == null)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to find upgrade settings for Creature {0}", group.Key), LoggerTypes.Warning));

                    continue;
                }

                if (!settings.Upgrade)
                {
                    continue;
                }

                CreatureToUpgrade.AddRange(group.ToList());
            }

            return new MethodResult<List<CreatureData>>
            {
                Data = CreatureToUpgrade,
                Message = String.Format("Found {0} Creature to upgrade", CreatureToUpgrade.Count),
                Success = true
            };
        }

        //List Creature shadowban for reference
        private static HashSet<int> _commonShadowIDs = new HashSet<int> { 16, 19, 23, 27, 29, 32, 43, 46, 52, 54, 60, 69, 77, 81, 98, 118, 120, 129, 177, 183, 187, 191, 194, 209, 218, 293, 304, 320, 325, 339 };
        */
    }
}
