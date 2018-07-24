﻿namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        /*
        private async Task<MethodResult<GymDeployResponse>> GymDeploy(BuildingData gym)
        {
            if (gym.OwnedByTeam != PlayerData.Team)
                return new MethodResult<GymDeployResponse>();

            var Creature = await GetDeployableCreature();

            if (Creature == null || Creature.CreatureId == CreatureId.Missingno)
                return new MethodResult<GymDeployResponse>();

            LogCaller(new LoggerEventArgs(String.Format("Try to deploy Creature {0}.", Creature.CreatureId), LoggerTypes.Info));

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<GymDeployResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GymDeploy,
                RequestMessage = new GymDeployMessage
                {
                    BuildingId = gym.Id,
                    CreatureId = Creature.Id,
                    PlayerLatitude = _client.ClientSession.Player.Latitude,
                    PlayerLongitude = _client.ClientSession.Player.Longitude
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<GymDeployResponse>();

            var gymDeployResponse = GymDeployResponse.Parser.ParseFrom(response);
            switch (gymDeployResponse.Result)
            {
                case GymDeployResponse.Types.Result.ErrorAlreadyHasCreatureOnBuilding:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorBuildingDeployLockout:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorBuildingIsFull:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorInvalidCreature:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorLegendaryCreature:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorNotACreature:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorOpposingTeamOwnsBuilding:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorPlayerBelowMinimumLevel:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorPlayerHasNoNickname:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorPlayerHasNoTeam:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorPoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorCreatureIsBuddy:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorCreatureNotFullHp:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorRaidActive:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorTeamDeployLockout:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorTooManyDeployed:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.ErrorTooManyOfSameKind:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.NoResultSet:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to deploy Creature {0}.", gymDeployResponse.Result), LoggerTypes.Warning));
                    break;
                case GymDeployResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym deploy success.", LoggerTypes.Deploy));
                    return new MethodResult<GymDeployResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = gymDeployResponse
                    };
            }

            return new MethodResult<GymDeployResponse>();
        }

        private async Task<MethodResult<GetRaidDetailsResponse>> GetRaidDetails(BuildingData gym, long raidSeed, int[] lobbyids)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<GetRaidDetailsResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GetRaidDetails,
                RequestMessage = new GetRaidDetailsMessage
                {
                    GymId = gym.Id,
                    PlayerLatDegrees = _client.ClientSession.Player.Latitude,
                    PlayerLngDegrees = _client.ClientSession.Player.Longitude,
                    RaidSeed = raidSeed,
                    LobbyId = { lobbyids }
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<GetRaidDetailsResponse>();

            var getRaidDetailsResponse = GetRaidDetailsResponse.Parser.ParseFrom(response);

            switch (getRaidDetailsResponse.Result)
            {
                case GetRaidDetailsResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get raid detail {0}.", getRaidDetailsResponse.Result), LoggerTypes.Warning));
                    break;
                case GetRaidDetailsResponse.Types.Result.ErrorPlayerBelowMinimumLevel:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get raid detail {0}.", getRaidDetailsResponse.Result), LoggerTypes.Warning));
                    break;
                case GetRaidDetailsResponse.Types.Result.ErrorPoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get raid detail {0}.", getRaidDetailsResponse.Result), LoggerTypes.Warning));
                    break;
                case GetRaidDetailsResponse.Types.Result.ErrorRaidCompleted:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get raid detail {0}.", getRaidDetailsResponse.Result), LoggerTypes.Warning));
                    break;
                case GetRaidDetailsResponse.Types.Result.ErrorRaidUnavailable:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get raid detail {0}.", getRaidDetailsResponse.Result), LoggerTypes.Warning));
                    break;
                case GetRaidDetailsResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym get raid details success.", LoggerTypes.Success));
                    return new MethodResult<GetRaidDetailsResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = getRaidDetailsResponse
                    };
                case GetRaidDetailsResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get raid detail {0}.", getRaidDetailsResponse.Result), LoggerTypes.Warning));
                    break;
            }

            return new MethodResult<GetRaidDetailsResponse>();
        }

        private async Task<MethodResult<StartRaidBattleResponse>> StartRaidBattle(BuildingData gym, long raidSeed, ulong[] attackingCreatureids, int[] lobbyids)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<StartRaidBattleResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.StartRaidBattle,
                RequestMessage = new StartRaidBattleMessage
                {
                    GymId = gym.Id,
                    PlayerLatDegrees = _client.ClientSession.Player.Latitude,
                    PlayerLngDegrees = _client.ClientSession.Player.Longitude,
                    RaidSeed = raidSeed,
                    AttackingCreatureId = { attackingCreatureids },
                    LobbyId = { lobbyids }
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<StartRaidBattleResponse>();

            var startRaidBattleResponse = StartRaidBattleResponse.Parser.ParseFrom(response);

            switch (startRaidBattleResponse.Result)
            {
                case StartRaidBattleResponse.Types.Result.ErrorGymNotFound:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.ErrorInvalidAttackers:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.ErrorLobbyNotFound:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.ErrorNoTicket:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.ErrorPlayerBelowMinimumLevel:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.ErrorPoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.ErrorRaidCompleted:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.ErrorRaidUnavailable:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case StartRaidBattleResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym start raid battle success.", LoggerTypes.Success));
                    return new MethodResult<StartRaidBattleResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = startRaidBattleResponse
                    };
                case StartRaidBattleResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to start raid battle {0}.", startRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
            }

            return new MethodResult<StartRaidBattleResponse>();
        }

        private async Task<MethodResult<AttackRaidBattleResponse>> AttackRaidBattle(BuildingData gym, RepeatedField<BattleAction> attackeractions, string battleid, BattleAction lastbattleAction, long timestampms)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<AttackRaidBattleResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.AttackRaid,
                RequestMessage = new AttackRaidBattleMessage
                {
                    GymId = gym.Id,
                    AttackerActions = { attackeractions },
                    BattleId = battleid,
                    LastRetrievedAction = lastbattleAction,
                    TimestampMs = timestampms
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<AttackRaidBattleResponse>();

            var attackRaidBattleResponse = AttackRaidBattleResponse.Parser.ParseFrom(response);

            switch (attackRaidBattleResponse.Result)
            {
                case AttackRaidBattleResponse.Types.Result.ErrorBattleIdNotRaid:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to attack raid {0}.", attackRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case AttackRaidBattleResponse.Types.Result.ErrorBattleNotFound:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to attack raid {0}.", attackRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case AttackRaidBattleResponse.Types.Result.ErrorGymNotFound:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to attack raid {0}.", attackRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case AttackRaidBattleResponse.Types.Result.ErrorInvalidAttackActions:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to attack raid {0}.", attackRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case AttackRaidBattleResponse.Types.Result.ErrorNotPartOfBattle:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to attack raid {0}.", attackRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
                case AttackRaidBattleResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym attack raid success.", LoggerTypes.Success));
                    return new MethodResult<AttackRaidBattleResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = attackRaidBattleResponse
                    };
                case AttackRaidBattleResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to attack raid {0}.", attackRaidBattleResponse.Result), LoggerTypes.Warning));
                    break;
            }

            return new MethodResult<AttackRaidBattleResponse>();
        }

        private async Task<MethodResult<JoinLobbyResponse>> JoinLobby(BuildingData gym, long raidSeed, bool _private, int[] lobbyids)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<JoinLobbyResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.JoinLobby,
                RequestMessage = new JoinLobbyMessage
                {
                    GymId = gym.Id,
                    PlayerLatDegrees = _client.ClientSession.Player.Latitude,
                    PlayerLngDegrees = _client.ClientSession.Player.Longitude, 
                    GymLatDegrees = gym.Latitude,
                    GymLngDegrees = gym.Longitude,
                    Private = _private,
                    RaidSeed = raidSeed,
                    LobbyId = { lobbyids }
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<JoinLobbyResponse>();

            var joinLobbyResponse = JoinLobbyResponse.Parser.ParseFrom(response);

            switch (joinLobbyResponse.Result)
            {
                case JoinLobbyResponse.Types.Result.ErrorGymLockout:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case JoinLobbyResponse.Types.Result.ErrorNoAvailableLobbies:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case JoinLobbyResponse.Types.Result.ErrorNoTicket:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case JoinLobbyResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case JoinLobbyResponse.Types.Result.ErrorPlayerBelowMinimumLevel:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case JoinLobbyResponse.Types.Result.ErrorPoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case JoinLobbyResponse.Types.Result.ErrorRaidCompleted:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case JoinLobbyResponse.Types.Result.ErrorRaidUnavailable:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case JoinLobbyResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym join lobby success.", LoggerTypes.Success));
                    return new MethodResult<JoinLobbyResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = joinLobbyResponse
                    };
                case JoinLobbyResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to join lobby {0}.", joinLobbyResponse.Result), LoggerTypes.Warning));
                    break;
            }

            return new MethodResult<JoinLobbyResponse>();
        }

        private async Task<MethodResult<LeaveLobbyResponse>> LeaveLobby(BuildingData gym, long raidSeed, int[] lobbyids)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<LeaveLobbyResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.LeaveLobby,
                RequestMessage = new LeaveLobbyMessage
                {
                    GymId = gym.Id,
                    RaidSeed = raidSeed,
                    LobbyId = { lobbyids }
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<LeaveLobbyResponse>();

            var leaveLobbyResponse = LeaveLobbyResponse.Parser.ParseFrom(response);

            switch (leaveLobbyResponse.Result)
            {
                case LeaveLobbyResponse.Types.Result.ErrorLobbyNotFound:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to leave lobby {0}.", leaveLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case LeaveLobbyResponse.Types.Result.ErrorRaidUnavailable:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to leave lobby {0}.", leaveLobbyResponse.Result), LoggerTypes.Warning));
                    break;
                case LeaveLobbyResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym leave lobby success.", LoggerTypes.Success));
                    return new MethodResult<LeaveLobbyResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = leaveLobbyResponse
                    };
                case LeaveLobbyResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to leave lobby {0}.", leaveLobbyResponse.Result), LoggerTypes.Warning));
                    break;
            }

            return new MethodResult<LeaveLobbyResponse>();
        }

        private async Task<MethodResult<SetLobbyCreatureResponse>> SetLobbyCreature(BuildingData gym, long raidSeed, ulong[] Creatureids, int[] lobbyids)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<SetLobbyCreatureResponse>(); ;
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.SetLobbyCreature,
                RequestMessage = new SetLobbyCreatureMessage
                {
                    GymId = gym.Id,
                    RaidSeed = raidSeed,
                    CreatureId = { Creatureids },
                    LobbyId = { lobbyids }
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<SetLobbyCreatureResponse>();

            var setLobbyCreatureResponse = SetLobbyCreatureResponse.Parser.ParseFrom(response);

            switch (setLobbyCreatureResponse.Result)
            {
                case SetLobbyCreatureResponse.Types.Result.ErrorInvalidCreature:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to set lobby Creature {0}.", setLobbyCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case SetLobbyCreatureResponse.Types.Result.ErrorLobbyNotFound:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to set lobby Creature {0}.", setLobbyCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case SetLobbyCreatureResponse.Types.Result.ErrorRaidUnavailable:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to set lobby Creature {0}.", setLobbyCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case SetLobbyCreatureResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym set lobby Creature success.", LoggerTypes.Success));
                    return new MethodResult<SetLobbyCreatureResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = setLobbyCreatureResponse
                    };
                case SetLobbyCreatureResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to set lobby Creature {0}.", setLobbyCreatureResponse.Result), LoggerTypes.Warning));
                    break;
            }

            return new MethodResult<SetLobbyCreatureResponse>();
        }

        private async Task<MethodResult<SetLobbyVisibilityResponse>> SetLobbyVisibility(BuildingData gym, long raidSeed, int[] lobbyids)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<SetLobbyVisibilityResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.SetLobbyVisibility,
                RequestMessage = new SetLobbyVisibilityMessage
                {
                    GymId = gym.Id,
                    RaidSeed = raidSeed,
                    LobbyId = { lobbyids }
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<SetLobbyVisibilityResponse>();

            var setLobbyVisibilityResponse = SetLobbyVisibilityResponse.Parser.ParseFrom(response);

            switch (setLobbyVisibilityResponse.Result)
            {
                case SetLobbyVisibilityResponse.Types.Result.ErrorLobbyNotFound:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to set lobby visibility {0}.", setLobbyVisibilityResponse.Result), LoggerTypes.Warning));
                    break;
                case SetLobbyVisibilityResponse.Types.Result.ErrorNotLobbyCreator:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to set lobby visibility {0}.", setLobbyVisibilityResponse.Result), LoggerTypes.Warning));
                    break;
                case SetLobbyVisibilityResponse.Types.Result.ErrorRaidUnavailable:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to set lobby visibility {0}.", setLobbyVisibilityResponse.Result), LoggerTypes.Warning));
                    break;
                case SetLobbyVisibilityResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym set lobby visibility success.", LoggerTypes.Success));
                    return new MethodResult<SetLobbyVisibilityResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = setLobbyVisibilityResponse
                    };
                case SetLobbyVisibilityResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to set lobby visibility {0}.", setLobbyVisibilityResponse.Result), LoggerTypes.Warning));
                    break;
            }

            return new MethodResult<SetLobbyVisibilityResponse>();
        }

        private async Task<MethodResult<GetGymBadgeDetailsResponse>> GetGymBadgeDetails(BuildingData gym, double latitude, double longitude)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<GetGymBadgeDetailsResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GetGymBadgeDetails,
                RequestMessage = new GetGymBadgeDetailsMessage
                {
                    BuildingId = gym.Id,
                    Latitude = latitude,
                    Longitude = longitude
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<GetGymBadgeDetailsResponse>();

            var getGymBadgeDetailsResponse = GetGymBadgeDetailsResponse.Parser.ParseFrom(response);

            if (getGymBadgeDetailsResponse.Success)
            {
                LogCaller(new LoggerEventArgs("Gym get badge details success.", LoggerTypes.Success));
                return new MethodResult<GetGymBadgeDetailsResponse>
                {
                    Data = getGymBadgeDetailsResponse,
                    Message = "Succes",
                    Success = true
                };
            }
            return new MethodResult<GetGymBadgeDetailsResponse>();
        }

        private async Task<MethodResult<UseItemGymResponse>> UseItemInGym(BuildingData gym, ItemId itemId)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<UseItemGymResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseItemGym,
                RequestMessage = new UseItemGymMessage
                {
                    ItemId = itemId,
                    GymId = gym.Id,
                    PlayerLatitude = _client.ClientSession.Player.Latitude,
                    PlayerLongitude = _client.ClientSession.Player.Longitude
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<UseItemGymResponse>();

            var useItemGymResponse = UseItemGymResponse.Parser.ParseFrom(response);

            switch (useItemGymResponse.Result)
            {
                case UseItemGymResponse.Types.Result.ErrorCannotUse:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to use item gym {0}.", useItemGymResponse.Result), LoggerTypes.Warning));
                    break;
                case UseItemGymResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to use item gym {0}.", useItemGymResponse.Result), LoggerTypes.Warning));
                    break;
                case UseItemGymResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym use item success.", LoggerTypes.Success));
                    return new MethodResult<UseItemGymResponse>
                    {
                        Data = useItemGymResponse,
                        Message = "Succes",
                        Success = true
                    };
                case UseItemGymResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to use item gym {0}.", useItemGymResponse.Result), LoggerTypes.Warning));
                    break;
            }
            return new MethodResult<UseItemGymResponse>();
        }

        private async Task<MethodResult<GymStartSessionResponse>> GymStartSession(BuildingData gym, ulong defendingCreatureId, IEnumerable<ulong> attackingCreatureIds)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<GymStartSessionResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GymStartSession,
                RequestMessage = new GymStartSessionMessage
                {
                    GymId = gym.Id,
                    DefendingCreatureId = defendingCreatureId,
                    AttackingCreatureId = { attackingCreatureIds },
                    PlayerLatDegrees = _client.ClientSession.Player.Latitude,
                    PlayerLngDegrees = _client.ClientSession.Player.Longitude
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<GymStartSessionResponse>();

            var gymStartSessionResponse = GymStartSessionResponse.Parser.ParseFrom(response);

            switch (gymStartSessionResponse.Result)
            {
                case GymStartSessionResponse.Types.Result.ErrorAllCreatureFainted:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorGymBattleLockout:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorGymEmpty:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorGymNeutral:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorGymNotFound:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorGymWrongTeam:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorInvalidDefender:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorPlayerBelowMinimumLevel:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorPoiInaccessible:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorRaidActive:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorTooManyBattles:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorTooManyPlayers:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.ErrorTrainingInvalidAttackerCount:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
                case GymStartSessionResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym start session success.", LoggerTypes.Success));
                    return new MethodResult<GymStartSessionResponse>
                    {
                        Data = gymStartSessionResponse,
                        Message = "Succes",
                        Success = true
                    };
                case GymStartSessionResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym start session {0}.", gymStartSessionResponse.Result), LoggerTypes.Warning));
                    break;
            }
            return new MethodResult<GymStartSessionResponse>();
        }

        private async Task<MethodResult<GymBattleAttackResponse>> GymBattleAttak(BuildingData gym, string battleId, IEnumerable<BattleAction> battleActions, BattleAction lastRetrievedAction, long timestampMs)
        {
            if (gym.OwnedByTeam == PlayerData.Team)
                return new MethodResult<GymBattleAttackResponse>();

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<GymBattleAttackResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GymBattleAttack,
                RequestMessage = new GymBattleAttackMessage
                {
                    BattleId = battleId,
                    GymId = gym.Id,
                    LastRetrievedAction = lastRetrievedAction,
                    PlayerLatDegrees = _client.ClientSession.Player.Latitude,
                    PlayerLngDegrees = _client.ClientSession.Player.Longitude,
                    TimestampMs = timestampMs,
                    AttackerActions = { battleActions }
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<GymBattleAttackResponse>();

            var gymBattleAttackResponse = GymBattleAttackResponse.Parser.ParseFrom(response);

            switch (gymBattleAttackResponse.Result)
            {
                case GymBattleAttackResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym attack success.", LoggerTypes.Success));
                    return new MethodResult<GymBattleAttackResponse>
                    {
                        Data = gymBattleAttackResponse,
                        Message = "Succes",
                        Success = true
                    };
                case GymBattleAttackResponse.Types.Result.ErrorInvalidAttackActions:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym attack {0}.", gymBattleAttackResponse.Result), LoggerTypes.Warning));
                    break;
                case GymBattleAttackResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym attack {0}.", gymBattleAttackResponse.Result), LoggerTypes.Warning));
                    break;
                case GymBattleAttackResponse.Types.Result.ErrorRaidActive:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym attack {0}.", gymBattleAttackResponse.Result), LoggerTypes.Warning));
                    break;
                case GymBattleAttackResponse.Types.Result.ErrorWrongBattleType:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym attack {0}.", gymBattleAttackResponse.Result), LoggerTypes.Warning));
                    break;
                case GymBattleAttackResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to gym attack {0}.", gymBattleAttackResponse.Result), LoggerTypes.Warning));
                    break;
            }
            return new MethodResult<GymBattleAttackResponse>();
        }

        private async Task<MethodResult<GymGetInfoResponse>> GymGetInfo(BuildingData Building)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<GymGetInfoResponse>();
                }
            }

            if (Tracker.BuildingsFarmed >= UserSettings.SpinBuildingsDayLimit)
            {
                LogCaller(new LoggerEventArgs("Spin Gym limit actived", LoggerTypes.Info));
                return new MethodResult<GymGetInfoResponse>
                {
                    Message = "Limit actived"
                };
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GymGetInfo,
                RequestMessage = new GymGetInfoMessage
                {
                    GymId = Building.Id,
                    GymLatDegrees = Building.Latitude,
                    GymLngDegrees = Building.Longitude,
                    PlayerLatDegrees = _client.ClientSession.Player.Latitude,
                    PlayerLngDegrees = _client.ClientSession.Player.Longitude
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<GymGetInfoResponse>();

            var gymGetInfoResponse = GymGetInfoResponse.Parser.ParseFrom(response);

            switch (gymGetInfoResponse.Result)
            {
                case GymGetInfoResponse.Types.Result.ErrorGymDisabled:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get gym info {0}.", gymGetInfoResponse.Result), LoggerTypes.Warning));
                    break;
                case GymGetInfoResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get gym info {0}.", gymGetInfoResponse.Result), LoggerTypes.Warning));
                    break;
                case GymGetInfoResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs("Gym info success.", LoggerTypes.Success));
                    return new MethodResult<GymGetInfoResponse>
                    {
                        Data = gymGetInfoResponse,
                        Message = "Succes",
                        Success = true
                    };
                case GymGetInfoResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill to get gym info {0}.", gymGetInfoResponse.Result), LoggerTypes.Warning));
                    break;
            }
            return new MethodResult<GymGetInfoResponse>();
        }

        private async Task<MethodResult<GymFeedCreatureResponse>> GymFeedCreature(BuildingData gym, ItemData item, CreatureData Creature, int startingQuantity = 1)
        {
            if (gym.OwnedByTeam != PlayerData.Team)
                return new MethodResult<GymFeedCreatureResponse>();

            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return new MethodResult<GymFeedCreatureResponse>();
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GymFeedCreature,
                RequestMessage = new GymFeedCreatureMessage
                {
                    GymId = gym.Id,
                    Item = item.ItemId,
                    CreatureId = Creature.Id,
                    PlayerLatDegrees = _client.ClientSession.Player.Latitude,
                    PlayerLngDegrees = _client.ClientSession.Player.Longitude,
                    StartingQuantity = startingQuantity
                }.ToByteString()
            });

            if (response == null)
                return new MethodResult<GymFeedCreatureResponse>();

            var gymFeedCreatureResponse = GymFeedCreatureResponse.Parser.ParseFrom(response);

            switch (gymFeedCreatureResponse.Result)
            {
                case GymFeedCreatureResponse.Types.Result.Success:
                    LogCaller(new LoggerEventArgs(String.Format("Gym Feed Creature {0} success.", item.ItemId.ToString().Replace("Item", "")), LoggerTypes.Success));
                    return new MethodResult<GymFeedCreatureResponse>
                    {
                        Success = true,
                        Message = "Success",
                        Data = gymFeedCreatureResponse
                    };
                case GymFeedCreatureResponse.Types.Result.ErrorCannotUse:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorGymBusy:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorGymClosed:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorNoBerriesLeft:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorNotInRange:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorCreatureFull:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorCreatureNotThere:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorRaidActive:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorTooFast:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorTooFrequent:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorWrongCount:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.ErrorWrongTeam:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                case GymFeedCreatureResponse.Types.Result.Unset:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
                default:
                    LogCaller(new LoggerEventArgs(String.Format("Faill Gym Feed Creature {0}.", gymFeedCreatureResponse.Result), LoggerTypes.Warning));
                    break;
            }
            return new MethodResult<GymFeedCreatureResponse>();
        }

        private async Task<CreatureData> GetDeployableCreature()
        {
            List<ulong> excluded = new List<ulong>();
            var CreatureList = Creature;
            CreatureData Creature = null;

            CreatureData settedbuddy = Creature.Where(w => w.Id == PlayerData?.BuddyCreature?.Id && PlayerData?.BuddyCreature?.Id > 0).Select(w => w).FirstOrDefault();
            CreatureData buddy = settedbuddy ?? new CreatureData();

            var defendersFromConfig = CreatureList.Where(w =>
                w.Id != buddy?.Id &&
                string.IsNullOrEmpty(w.DeployedBuildingId)
            ).ToList();

            foreach (var _Creature in defendersFromConfig.OrderByDescending(o => o.Cp))
            {
                if (_Creature.Stamina <= 0)
                    await ReviveCreature(_Creature);

                if (_Creature.Stamina < _Creature.StaminaMax && _Creature.Stamina > 0)
                    await HealCreature(_Creature);

                if (_Creature.Stamina < _Creature.StaminaMax)
                    excluded.Add(_Creature.Id);
                else
                    return _Creature;
            }

            while (Creature == null)
            {
                CreatureList = CreatureList
                    .Where(w => !excluded.Contains(w.Id) && w.Id != PlayerData.BuddyCreature?.Id)
                    .OrderByDescending(p => p.Cp)
                    .ToList();

                if (CreatureList.Count == 0)
                    return null;

                if (CreatureList.Count == 1)
                    Creature = CreatureList.FirstOrDefault();

                Creature = CreatureList.FirstOrDefault(p => string.IsNullOrEmpty(p.DeployedBuildingId)
                );

                if (Creature.Stamina <= 0)
                    await ReviveCreature(Creature);

                if (Creature.Stamina < Creature.StaminaMax && Creature.Stamina > 0)
                    await HealCreature(Creature);

                if (Creature.Stamina < Creature.StaminaMax)
                {
                    excluded.Add(Creature.Id);
                    Creature = null;
                }
            }
            return Creature;
        }

        private async Task<bool> HealCreature(CreatureData Creature)
        {
            var normalPotions = Items.Select(x => x.ItemId == ItemId.ItemPotion).Count();
            var superPotions = Items.Select(x => x.ItemId == ItemId.ItemSuperPotion).Count();
            var hyperPotions = Items.Select(x => x.ItemId == ItemId.ItemHyperPotion).Count();
            var maxPotions = Items.Select(x => x.ItemId == ItemId.ItemMaxPotion).Count();

            var healPower = normalPotions * 20 + superPotions * 50 + hyperPotions * 200;

            if (healPower < (Creature.StaminaMax - Creature.Stamina) && maxPotions > 0)
            {
                if (await UseMaxPotion(Creature, maxPotions))
                {
                    UpdateInventory(InventoryRefresh.Items);
                    return true;
                }
            }

            while (normalPotions + superPotions + hyperPotions > 0 && (Creature.Stamina < Creature.StaminaMax))
            {
                if (((Creature.StaminaMax - Creature.Stamina) > 200 || ((normalPotions * 20 + superPotions * 50) < (Creature.StaminaMax - Creature.Stamina))) && hyperPotions > 0)
                {
                    if (!await UseHyperPotion(Creature, hyperPotions))
                        return false;
                    hyperPotions--;
                    UpdateInventory(InventoryRefresh.Items);
                }
                else
                if (((Creature.StaminaMax - Creature.Stamina) > 50 || normalPotions * 20 < (Creature.StaminaMax - Creature.Stamina)) && superPotions > 0)
                {
                    if (!await UseSuperPotion(Creature, superPotions))
                        return false;
                    superPotions--;
                    UpdateInventory(InventoryRefresh.Items);
                }
                else
                {
                    if (!await UsePotion(Creature, normalPotions))
                        return false;
                    normalPotions--;
                    UpdateInventory(InventoryRefresh.Items);
                }
            }

            return Creature.Stamina == Creature.StaminaMax;
        }

        private async Task<bool> UseMaxPotion(CreatureData Creature, int maxPotions)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return false;
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseItemPotion,
                RequestMessage = new UseItemPotionMessage
                {
                    ItemId = ItemId.ItemMaxPotion,
                    CreatureId = Creature.Id,
                }.ToByteString()
            });

            if (response == null)
                return false;

            var useItemPotionResponse = UseItemPotionResponse.Parser.ParseFrom(response);

            switch (useItemPotionResponse.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    Creature.Stamina = useItemPotionResponse.Stamina;
                    LogCaller(new LoggerEventArgs(String.Format("Success to use {0}, CP: {1} on {2}", ItemId.ItemMaxPotion.ToString().Replace("Item",""), Creature.Cp, Creature.CreatureId), LoggerTypes.Success));
                    break;
                case UseItemPotionResponse.Types.Result.ErrorDeployedToBuilding:
                    LogCaller(new LoggerEventArgs($"Creature: {Creature.CreatureId.ToString()} (CP: {Creature.Cp}) is already deployed to a gym...", LoggerTypes.Warning));
                    return false;
                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;
                default:
                    return false;
            }
            return true;
        }

        private async Task<bool> UseHyperPotion(CreatureData Creature, int hyperPotions)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return false;
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseItemPotion,
                RequestMessage = new UseItemPotionMessage
                {
                    ItemId = ItemId.ItemHyperPotion,
                    CreatureId = Creature.Id,
                }.ToByteString()
            });

            if (response == null)
                return false;

            var useItemPotionResponse = UseItemPotionResponse.Parser.ParseFrom(response);

            switch (useItemPotionResponse.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    Creature.Stamina = useItemPotionResponse.Stamina;
                    LogCaller(new LoggerEventArgs(String.Format("Success to use {0}, CP: {1} on {2}", ItemId.ItemHyperPotion.ToString().Replace("Item", ""), Creature.Cp, Creature.CreatureId), LoggerTypes.Success));
                    break;
                case UseItemPotionResponse.Types.Result.ErrorDeployedToBuilding:
                    LogCaller(new LoggerEventArgs($"Creature: {Creature.CreatureId.ToString()} (CP: {Creature.Cp}) is already deployed to a gym...", LoggerTypes.Warning));
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private async Task<bool> UseSuperPotion(CreatureData Creature, int superPotions)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return false;
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseItemPotion,
                RequestMessage = new UseItemPotionMessage
                {
                    ItemId = ItemId.ItemSuperPotion,
                    CreatureId = Creature.Id,
                }.ToByteString()
            });

            if (response == null)
                return false;

            var useItemPotionResponse = UseItemPotionResponse.Parser.ParseFrom(response);

            switch (useItemPotionResponse.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    Creature.Stamina = useItemPotionResponse.Stamina;
                    LogCaller(new LoggerEventArgs(String.Format("Success to use {0}, CP: {1} on {2}", ItemId.ItemSuperPotion.ToString().Replace("Item", ""), Creature.Cp, Creature.CreatureId), LoggerTypes.Success));
                    break;
                case UseItemPotionResponse.Types.Result.ErrorDeployedToBuilding:
                    LogCaller(new LoggerEventArgs($"Creature: {Creature.CreatureId.ToString()} (CP: {Creature.Cp}) is already deployed to a gym...", LoggerTypes.Warning));
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private async Task<bool> UsePotion(CreatureData Creature, int normalPotions)
        {
            if (!_client.LoggedIn)
            {
                MethodResult result = await AcLogin();

                if (!result.Success)
                {
                    return false;
                }
            }

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseItemPotion,
                RequestMessage = new UseItemPotionMessage
                {
                    ItemId = ItemId.ItemPotion,
                    CreatureId = Creature.Id,
                }.ToByteString()
            });

            if (response == null)
                return false;

            var useItemPotionResponse = UseItemPotionResponse.Parser.ParseFrom(response);

            switch (useItemPotionResponse.Result)
            {
                case UseItemPotionResponse.Types.Result.Success:
                    Creature.Stamina = useItemPotionResponse.Stamina;
                    LogCaller(new LoggerEventArgs(String.Format("Success to use {0}, CP: {1} on {2}", ItemId.ItemPotion.ToString().Replace("Item", ""), Creature.Cp, Creature.CreatureId), LoggerTypes.Success));
                    break;
                case UseItemPotionResponse.Types.Result.ErrorDeployedToBuilding:
                    LogCaller(new LoggerEventArgs($"Creature: {Creature.CreatureId.ToString()} (CP: {Creature.Cp}) is already deployed to a gym...", LoggerTypes.Warning));
                    return false;

                case UseItemPotionResponse.Types.Result.ErrorCannotUse:
                    return false;

                default:
                    return false;
            }
            return true;
        }

        private async Task ReviveCreature(CreatureData Creature)
        {
            int healPower = 0;

            if (Items.Select(x => x.ItemId == ItemId.ItemMaxPotion).Count() > 0)
                healPower = Int32.MaxValue;
            else
            {
                var normalPotions = Items.Select(x => x.ItemId == ItemId.ItemPotion).Count();
                var superPotions = Items.Select(x => x.ItemId == ItemId.ItemSuperPotion).Count();
                var hyperPotions = Items.Select(x => x.ItemId == ItemId.ItemHyperPotion).Count();

                healPower = normalPotions * 20 + superPotions * 50 + hyperPotions * 200;
            }

            var normalRevives = Items.Select(x => x.ItemId == ItemId.ItemRevive).Count();
            var maxRevives = Items.Select(x => x.ItemId == ItemId.ItemMaxRevive).Count();

            if ((healPower >= Creature.StaminaMax / 2 || maxRevives == 0) && normalRevives > 0 && Creature.Stamina <= 0)
            {
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
                    RequestType = RequestType.UseItemRevive,
                    RequestMessage = new UseItemReviveMessage
                    {
                        ItemId = ItemId.ItemRevive,
                        CreatureId = Creature.Id,
                    }.ToByteString()
                });

                if (response == null)
                    return;

                var useItemRevive = UseItemReviveResponse.Parser.ParseFrom(response);

                switch (useItemRevive.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        UpdateInventory(InventoryRefresh.Items);
                        Creature.Stamina = useItemRevive.Stamina;
                        LogCaller(new LoggerEventArgs(String.Format("Success to use {0}, CP: {1} on {2}", ItemId.ItemRevive.ToString().Replace("Item", ""), Creature.Cp, Creature.CreatureId), LoggerTypes.Success));
                        break;
                    case UseItemReviveResponse.Types.Result.ErrorDeployedToBuilding:
                        LogCaller(new LoggerEventArgs($"Creature: {Creature.CreatureId.ToString()} (CP: {Creature.Cp}) is already deployed to a gym...", LoggerTypes.Warning));
                        return;
                    case UseItemReviveResponse.Types.Result.ErrorCannotUse:
                        return;
                    default:
                        return;
                }
                return;
            }

            if (maxRevives > 0 && Creature.Stamina <= 0)
            {
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
                    RequestType = RequestType.UseItemRevive,
                    RequestMessage = new UseItemReviveMessage
                    {
                        ItemId = ItemId.ItemMaxRevive,
                        CreatureId = Creature.Id,
                    }.ToByteString()
                });

                if (response == null)
                    return;

                var useItemRevive = UseItemReviveResponse.Parser.ParseFrom(response);

                switch (useItemRevive.Result)
                {
                    case UseItemReviveResponse.Types.Result.Success:
                        UpdateInventory(InventoryRefresh.Items);
                        Creature.Stamina = useItemRevive.Stamina;
                        LogCaller(new LoggerEventArgs(String.Format("Success to use {0}, CP: {1} on {2}", ItemId.ItemMaxRevive.ToString().Replace("Item", ""), Creature.Cp, Creature.CreatureId), LoggerTypes.Success));
                        break;
                    case UseItemReviveResponse.Types.Result.ErrorDeployedToBuilding:
                        LogCaller(new LoggerEventArgs($"Creature: {Creature.CreatureId.ToString()} (CP: {Creature.Cp}) is already deployed to a gym...", LoggerTypes.Warning));
                        return;
                    case UseItemReviveResponse.Types.Result.ErrorCannotUse:
                        return;
                    default:
                        return;
                }
            }
        }
        */
    }
}
