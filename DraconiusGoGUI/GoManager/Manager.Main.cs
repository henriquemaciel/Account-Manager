using Newtonsoft.Json;
using DraconiusGoGUI.AccountScheduler;
using DraconiusGoGUI.Enums;
using DraconiusGoGUI.Extensions;
using DraconiusGoGUI.DracoManager.Models;
using DraconiusGoGUI.Models;
using DraconiusGoGUI.ProxyManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DracoLib.Core.Extensions;
using DracoProtos.Core.Objects;
using DracoProtos.Core.Base;
using DracoLib.Core.Text;
using System.Globalization;

namespace DraconiusGoGUI.DracoManager
{
    public partial class Manager
    {
        private Client _client = new Client();
        private Random _rand = new Random();

        private int _totalZeroExpStops = 0;
        private bool _firstRun = true;
        private int _failedInventoryReponses = 0;
        private const int _failedInventoryUntilBan = 3;
        private int _fleeingCreatureResponses = 0;
        private bool _potentialCreatureBan = false;
        private const int _fleeingCreatureUntilBan = 3;
        private bool _potentialBuildingBan = false;
        private int _failedBuildingResponse = 0;
        private bool _autoRestart = false;
        private bool _wasAutoRestarted = false;
        private ManualResetEvent _pauser = new ManualResetEvent(true);
        private DateTime TimeAutoCatch = DateTime.Now;
        private bool CatchDisabled = false;

        public bool _proxyIssue = false;

        //Needs to be saved on close
        public GoProxy CurrentProxy { get; set; }

        //DracoText for translations
        public Strings Strings { get; set; }

        [JsonIgnore]
        public ProxyHandler ProxyHandler { get; set; }

        private bool _isPaused { get { return !_pauser.WaitOne(0); } }

        [JsonConstructor]
        public Manager()
        {
            Stats = new FAvaUpdate();
            PlayerData = new FUserInfo();
            UserBag = new FBagUpdate();
            Logs = new List<Log>();
            Tracker = new Tracker();
            LoadFarmLocations();
        }

        public Manager(ProxyHandler handler)
        {
            UserSettings = new Settings();
            Logs = new List<Log>();
            Stats = new FAvaUpdate();
            PlayerData = new FUserInfo();
            UserBag = new FBagUpdate();
            Tracker = new Tracker();
            ProxyHandler = handler;
            LoadFarmLocations();
        }

        public async Task<MethodResult> AcLogin()
        {
            int retries = 1;
            initretrie:

            LogCaller(new LoggerEventArgs(String.Format("Attempting to login retry: #{0} ...", retries ), LoggerTypes.Info));
            AccountState = AccountState.Conecting;

            MethodResult result = await _client.DoLogin(this);

            if (result == null)
            {
                LogCaller(new LoggerEventArgs(String.Format("Attempting to login null result. Stopping ..."), LoggerTypes.Debug));
                Stop();
            }

            LogCaller(new LoggerEventArgs(result.Message, LoggerTypes.Debug));

            if (!result.Success)
            {
                if (retries > 0)
                {
                    retries--;
                    LogCaller(new LoggerEventArgs(result.Message, LoggerTypes.Warning));
                    await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                    goto initretrie;
                }
                else
                {
                    LogCaller(new LoggerEventArgs(result.Message, LoggerTypes.FatalError));
                    if (AccountState == AccountState.Conecting || AccountState == AccountState.Good)
                        AccountState = AccountState.Unknown;
                    Stop();
                }
            }
            else
            {
                if (AccountState != AccountState.Good)
                {
                    AccountState = AccountState.Good;
                }

                if (CurrentProxy != null)
                {
                    ProxyHandler.ResetFailCounter(CurrentProxy);
                }
            }

            return result;
        }

        public MethodResult Start()
        {
            if (IsRunning || AccountState == AccountState.Conecting)
            {
                return new MethodResult
                {
                    Message = "Bot already running"
                };
            }

            if (State != BotState.Stopped)
            {
                return new MethodResult
                {
                    Message = "Please wait for bot to fully stop"
                };
            }

            State = BotState.Starting;

            //Fixing a bug on my part
            if (Tracker == null)
            {
                Tracker = new Tracker();
            }

            ServicePointManager.DefaultConnectionLimit = Int32.MaxValue;

            if (!_wasAutoRestarted)
            {
                ExpGained = 0;
            }

            IsRunning = true;
            _totalZeroExpStops = 0;
            _client.SetSettings(this);
            _pauser.Set();
            _autoRestart = false;
            //_wasAutoRestarted = false;
            _rand = new Random();

            var t = new Thread(RunningThread)
            {
                IsBackground = true
            };

            LogCaller(new LoggerEventArgs("Bot started", LoggerTypes.Info));

            _runningStopwatch.Start();
            _potentialCreatureBan = false;
            _fleeingCreatureResponses = 0;

            t.Start();

            return new MethodResult
            {
                Message = "Bot started"
            };
        }

        public void Restart()
        {
            if (!IsRunning)
            {
                Start();

                return;
            }

            LogCaller(new LoggerEventArgs("Restarting bot", LoggerTypes.Info));

            _autoRestart = true;

            Stop();
        }

        public void Pause()
        {
            if (!IsRunning || State == BotState.Pausing || State == BotState.Paused)
            {
                return;
            }

            _pauser.Reset();
            _runningStopwatch.Stop();

            LogCaller(new LoggerEventArgs("Pausing bot ...", LoggerTypes.Info));

            State = BotState.Pausing;
        }

        public void UnPause()
        {
            if (!IsRunning || State != BotState.Paused)
            {
                return;
            }

            _pauser.Set();
            _runningStopwatch.Start();

            LogCaller(new LoggerEventArgs("Unpausing bot ...", LoggerTypes.Info));

            State = BotState.Running;
        }

        public void TogglePause()
        {
            if (State == BotState.Paused || State == BotState.Pausing)
            {
                UnPause();
            }
            else
            {
                Pause();
            }
        }

        private bool WaitPaused()
        {
            if (_isPaused)
            {
                LogCaller(new LoggerEventArgs("Bot paused", LoggerTypes.Info));

                State = BotState.Paused;
                _pauser.WaitOne();

                return true;
            }

            /*if (_client?.ClientSession?.State == SessionState.Paused)
            {
                LogCaller(new LoggerEventArgs("Bot paused", LoggerTypes.Info));

                State = BotState.Paused;

                return true;
            }*/

            return false;
        }

        private bool CheckTime()
        {
            if (Math.Abs(UserSettings.RunForHours) < 0.001)
            {
                return false;
            }

            if (_runningStopwatch.Elapsed.TotalHours >= UserSettings.RunForHours)
            {
                Stop();

                LogCaller(new LoggerEventArgs("Max runtime reached. Stopping ...", LoggerTypes.Info));

                return true;
            }

            return false;
        }

        private async void RunningThread()
        {
            const int failedWaitTime = 5000;
            int currentFails = 0;

            //Reset account state
            AccountState = AccountState.Good;

            while (IsRunning)
            {
                if (CheckTime())
                {
                    break;
                }

                WaitPaused();

                if ((_proxyIssue || CurrentProxy == null) && UserSettings.AutoRotateProxies)
                {
                    bool success = await ChangeProxy();

                    //Fails when it's stopping
                    if (!success)
                    {
                        continue;
                    }

                    //Have to restart to set proxy
                    Restart();

                    _proxyIssue = false;
                }

                StartingUp = true;

                if (currentFails >= UserSettings.MaxFailBeforeReset)
                {
                    currentFails = 0;
                    break;
                }

                if (_failedInventoryReponses >= _failedInventoryUntilBan)
                {
                    AccountState = AccountState.PermanentBan;
                    LogCaller(new LoggerEventArgs("Potential account ban", LoggerTypes.Warning));
                    break;
                }

                ++currentFails;

                var result = new MethodResult();

                #region Startup

                try
                {
                    if (!_client.LoggedIn)
                    {
                        //Login
                        result = await AcLogin();

                        if (!result.Success)
                        {
                            //A failed login should require longer wait
                            await Task.Delay(failedWaitTime * 3);

                            continue;
                        }
                    }

                    /*if (_client.ClientSession.AccessToken.IsExpired)
                    {
                        Restart();
                    }*/

                    if (UserSettings.StopOnAPIUpdate)
                    {
                        //Get Game settings
                        LogCaller(new LoggerEventArgs("Grabbing game settings ...", LoggerTypes.Debug));
                        try
                        {
                            var remote = new Version();
                            //if (_client.ClientSession.GlobalSettings != null)
                            //    remote = new Version(_client.ClientSession.GlobalSettings?.MinimumClientVersion);
                            //if (_client.VersionStr < remote)
                            //{
                            //    LogCaller(new LoggerEventArgs($"Emulates API {_client.VersionStr} ...", LoggerTypes.FatalError, new Exception($"New API needed {remote}. Stopping ...")));
                            //    break;
                            //}
                        }
                        catch (Exception ex1)
                        {
                            //if (AccountState != AccountState.CaptchaReceived || AccountState != AccountState.HashIssues)
                            //    AccountState = AccountState.TemporalBan;
                            LogCaller(new LoggerEventArgs("Exception: " + ex1, LoggerTypes.Debug));
                            LogCaller(new LoggerEventArgs("Game settings failed", LoggerTypes.FatalError, new Exception("Maybe this account is banned ...")));
                            break;
                        }
                    }

                    //Get Creature settings
                    /*if (PokeSettings == null)
                    {
                        LogCaller(new LoggerEventArgs("Grabbing Creature settings ...", LoggerTypes.Debug));

                        result = await GetItemTemplates();

                        if (!result.Success)
                        {
                            //if (AccountState != AccountState.CaptchaReceived || AccountState != AccountState.HashIssues)
                            //    AccountState = AccountState.TemporalBan;
                            LogCaller(new LoggerEventArgs("Load Creature settings failed", LoggerTypes.FatalError, new Exception("Maybe this account is banned ...")));
                            break;
                        }
                    }*/

                    //Auto complete tutorials
                    if (UserSettings.CompleteTutorial)
                    {
                        /*
                        if (!PlayerData.TutorialState.Contains(TutorialState.AvatarSelection))
                        {
                            result = await MarkStartUpTutorialsComplete(true);

                            if (!result.Success)
                            {
                                LogCaller(new LoggerEventArgs("Failed. Marking startup tutorials completed..", LoggerTypes.Warning));

                                break;
                            }

                            LogCaller(new LoggerEventArgs("Marking startup tutorials completed.", LoggerTypes.Success));

                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                        }
                        */
                    }

                    _failedInventoryReponses = 0;

                    WaitPaused();

                    //End startup phase
                    StartingUp = false;

                    //Prevent changing back to running state
                    if (State != BotState.Stopping)
                    {
                        State = BotState.Running;
                    }

                    //Update location
                    if (_firstRun)
                    {
                        LogCaller(new LoggerEventArgs("Setting default location ...", LoggerTypes.Debug));

                        UserMap = new FUpdate();

                        result = await UpdateLocation(new GeoCoordinate(UserSettings.Latitude, UserSettings.Longitude));

                        if (!result.Success)
                        {
                            break;
                        }

                        UpdateInventory(InventoryRefresh.All);
                    }

                    #endregion

                    #region BuildingTask

                    //Get Buildings
                    //Goto her if count or meters is < of settings
                    reloadAllBuildings:

                    LogCaller(new LoggerEventArgs("Getting buildings...", LoggerTypes.Info));                   
                    MethodResult<List<FBuilding>> Buildings = await GetAllBuildingsAsync();

                    if (!Buildings.Success)
                    {
                        await Task.Delay(failedWaitTime);
                        continue;
                    }

                    int BuildingNumber = 1;
                    int totalStops = Buildings.Data.Count;

                    if (totalStops == 0)
                    {
                        _proxyIssue = false;
                        _potentialBuildingBan = false;

                        LogCaller(new LoggerEventArgs(String.Format("{0}. Failure {1}/{2}", Buildings.Message, currentFails, UserSettings.MaxFailBeforeReset), LoggerTypes.Warning));

                        if (UserSettings.AutoRotateProxies && currentFails >= UserSettings.MaxFailBeforeReset)
                        {
                            if (Buildings.Message.StartsWith("No Building data found.", StringComparison.Ordinal))
                            {
                                _proxyIssue = true;
                                await ChangeProxy();
                            }
                        }

                        await Task.Delay(failedWaitTime);

                        continue;
                    }

                    int currentFailedStops = 0;

                    var BuildingsToFarm = new Queue<FBuilding>(Buildings.Data.Where(x=>x!=null));

                    while (BuildingsToFarm.Any())
                    {
                        // In each iteration of the loop we store the current level
                        int prevLevel = Level;

                        if (!IsRunning || currentFailedStops >= UserSettings.MaxFailBeforeReset)
                        {
                            break;
                        }

                        if (CheckTime())
                        {
                            break;
                        }

                        BuildingsToFarm = new Queue<FBuilding>(BuildingsToFarm.OrderBy(x => CalculateDistanceInMeters(UserSettings.Latitude, UserSettings.Longitude, x.coords.latitude, x.coords.longitude)));

                        FBuilding Building = BuildingsToFarm.FirstOrDefault();

                        if (Building == null)
                            continue;

                        var player = new GeoCoordinate(UserSettings.Latitude, UserSettings.Longitude);
                        var BuildingLocation = new GeoCoordinate(Building.coords.latitude, Building.coords.longitude);
                        double distance = CalculateDistanceInMeters(player, BuildingLocation);
 
                        if (UserSettings.MaxBuildingMeters > 0)
                        {
                            double rand = UserSettings.MaxBuildingMetersRandom - UserSettings.MaxBuildingMeters;
                            BuildingsToFarm = new Queue<FBuilding>(BuildingsToFarm.OrderBy(x => distance <= rand));

                            if (BuildingsToFarm.Count < 1)
                            {
                                rand = UserSettings.MaxBuildingMetersRandom + UserSettings.MaxBuildingMeters;
                                BuildingsToFarm = new Queue<FBuilding>(BuildingsToFarm.OrderBy(x => distance <= rand));
                            }

                            if (BuildingsToFarm.Count < 1)
                            {
                                //Pass restart if value is 0 or meter no ok recommended 250-300
                                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenLocationUpdates, UserSettings.LocationupdateDelayRandom));
                                goto reloadAllBuildings;
                            }
                        }

                        if (BuildingsToFarm.Count < 1)
                            continue;

                        if (UserSettings.GoOnlyToGyms && Building.type != BuildingType.ARENA)
                            continue;

                        Building = BuildingsToFarm.Dequeue();
                        LogCaller(new LoggerEventArgs("Building DeQueued: " + Building.id, LoggerTypes.Debug));

                        string _building = "";
                        LoggerTypes loggerTypes = LoggerTypes.Info;

                        switch (Building.type)
                        {
                            case BuildingType.ARENA:
                                _building = "Arena";
                                if (Level >= 5 && !UserSettings.DefaultTeam.Equals("Neutral") && !String.IsNullOrEmpty(UserSettings.DefaultTeam))
                                {
                                    loggerTypes = LoggerTypes.Gym;
                                }
                                continue;
                            case BuildingType.STOP:
                                _building = "Pillar of Abundance";
                                break;
                            case BuildingType.OBELISK:
                                _building = "Obelisk";
                                continue;
                            case BuildingType.ROOST:
                                _building = "Roost";
                                continue;
                            case BuildingType.PORTAL:
                                _building = "Portal";
                                break;
                            case BuildingType.LIBRARY:
                                _building = "Library";
                                continue;
                            case BuildingType.DUNGEON_STOP:
                                _building = "Dungeon";
                                break;
                            default:
                                _building = Building.type.ToString();
                                break;
                        }

                        if (!UserSettings.SpinGyms && Building.type == BuildingType.ARENA)
                            continue;

                        LogCaller(new LoggerEventArgs(String.Format("Going to a {0}. Building {1} of {2}. Distance {3:0.00}m", _building, BuildingNumber, totalStops, distance), loggerTypes));

                        //Go to Buildings
                        MethodResult walkResult = await GoToLocation(new GeoCoordinate(Building.coords.latitude, Building.coords.longitude));

                        if (!walkResult.Success)
                        {
                            LogCaller(new LoggerEventArgs("Too many failed walking attempts. Restarting to fix ...", LoggerTypes.Warning));
                            LogCaller(new LoggerEventArgs("Result: " + walkResult.Message, LoggerTypes.Debug));
                            break;
                        }

                        if (CatchDisabled)
                        {
                            //Check delay if account not have balls
                            var now = DateTime.Now;
                            LogCaller(new LoggerEventArgs("Now: " + now.ToLongDateString() + " " + now.ToLongTimeString(), LoggerTypes.Info));
                            LogCaller(new LoggerEventArgs("TimeAutoCatch: " + TimeAutoCatch.ToLongDateString() + " " + TimeAutoCatch.ToLongTimeString(), LoggerTypes.Info));
                            if (now > TimeAutoCatch)
                            {
                                CatchDisabled = false;
                                LogCaller(new LoggerEventArgs("Enable catch after wait time.", LoggerTypes.Info));
                            }
                        }

                        if (!CatchDisabled)
                        {
                            int remainingPokeballs = RemainingPokeballs();
                            LogCaller(new LoggerEventArgs("Remaining Balls: " + remainingPokeballs, LoggerTypes.Info));
                            double filledCreatureStorage = FilledCreatureStorage();

                            if (remainingPokeballs > 0)
                            {
                                if (filledCreatureStorage <= 100)
                                {
                                    //Catch nearby Creature
                                    MethodResult nearbyCreatureResponse = await CatchNeabyCreature();
                                    if (nearbyCreatureResponse.Success)
                                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                                    //Catch incense Creature
                                    MethodResult incenseCreatureResponse = await CatchInsenceCreature();
                                    if (incenseCreatureResponse.Success)
                                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                                    //Catch lured Creature
                                    //MethodResult luredCreatureResponse = await CatchLuredCreature(Building);
                                    //if (luredCreatureResponse.Success)
                                    //    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                                    //Check sniping NearyCreature
                                    //MethodResult Snipe = await SnipeAllNearyCreature();
                                    //if (Snipe.Success)
                                    //{
                                    //    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                        //this as walk to Creature sinpe pos is not good .. continue for new pos..
                                    //    continue;
                                    //}
                                }
                                else
                                {
                                   LogCaller(new LoggerEventArgs("You inventory Creature storage is full please transfer some Creatures.", LoggerTypes.Warning));
                                    await TransferFilteredCreature();
                                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                }
                            }
                            else
                            {
                                LogCaller(new LoggerEventArgs("You don't have any pokeball catching Creature will be disabled during " + UserSettings.DisableCatchDelay.ToString(CultureInfo.InvariantCulture) + " minutes.", LoggerTypes.Info));
                                CatchDisabled = true;
                                TimeAutoCatch = DateTime.Now.AddMinutes(UserSettings.DisableCatchDelay);
                            }
                        }

                        //Stop bot instantly
                        if (!IsRunning)
                        {
                            break;
                        }

                        //Clean inventory,
                        if (UserSettings.RecycleItems)
                        {
                            await RecycleFilteredItems();
                            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                        }

                        //if too balls ignore stops..
                        if (RemainingPokeballs() >= UserSettings.BallsToIgnoreStops && UserSettings.IgnoreStopsIfTooBalls)
                            continue;

                        //Search 
                        //|| Building.type == BuildingType.PORTAL || Building.type == BuildingType.DUNGEON_STOP)
                        if (Building.type == BuildingType.STOP)
                        {
                            MethodResult searchResult = await SearchBuilding(Building);

                            //OutOfRange will show up as a success
                            if (searchResult.Success)
                            {
                                currentFailedStops = 0;
                                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                            }
                            else
                            {
                                if (currentFailedStops > 10)
                                {
                                    break;
                                }
                                ++currentFailedStops;
                            }
                        }

                        if (UserSettings.OpenChests)
                        {
                            var chestsResult = await GetAllChestsInRangeAsync();
                            if (chestsResult.Success && chestsResult.Data.Count > 0 && chestsResult.Data != null)
                            {
                                // NOTE: this toArray() force a new list object, this is needed because the real list changes at remove an element and breaks the loop
                                foreach (var chest in chestsResult.Data.ToArray())
                                {
                                    var openResult =  _client.DracoClient.OpenChest(chest);
                                    if (openResult.loot==null)
                                    {
                                        continue;
                                    }
                                    RemoveChest(chest);
                                    var text = "Chest Opened. Award Received: ";
                                    foreach (var item in openResult.loot.lootList.Where(x => x is FLootItemItem).GroupBy(y => (y as FLootItemItem).item))
                                    {
                                        text += $"[{item.Sum(x => x.qty)}] {Strings.GetItemName(item.Key)}, ";
                                    }
                                    var xpqty = openResult.loot.lootList.Where(x => x is FLootItemExp).Sum(x => x.qty);
                                    if (xpqty > 0)
                                    {
                                        text += $"[{xpqty}] XP, ";
                                        ExpIncrease( xpqty);
                                    }

                                    LogCaller(new LoggerEventArgs(text, LoggerTypes.Success));
                                    if (openResult.levelUpLoot != null)
                                    {
                                        text = "Level Up Award: ";
                                        foreach (var item in openResult.levelUpLoot.lootList.Where(x => x is FLootItemItem).GroupBy(y => (y as FLootItemItem).item))
                                        {
                                            text += $"[{item.Sum(x => x.qty)}] {Strings.GetItemName(item.Key)}, ";
                                        }
                                        xpqty = openResult.levelUpLoot.lootList.Where(x => x is FLootItemExp).Sum(x => x.qty);
                                        if (xpqty > 0)
                                        {
                                            text += $"[{xpqty}] XP, ";
                                            ExpIncrease( xpqty);
                                        }
                                        LogCaller(new LoggerEventArgs(text, LoggerTypes.Success));
                                    }
                                }
                                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                            }
                        }

                        /* Search Old Refs:
                        double filledInventorySpace = FilledInventoryStorage();
                        LogCaller(new LoggerEventArgs(String.Format("Filled Inventory Storage: {0:0.00}%", filledInventorySpace), LoggerTypes.Debug));

                        if ((filledInventorySpace < UserSettings.SearchBuildingBelowPercent) && (filledInventorySpace <= 100))
                        {
                            if (Building.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                            {
                                if (Building.Type == BuildingType.Gym && Level >= 5 && (!string.IsNullOrEmpty(UserSettings.DefaultTeam) || UserSettings.DefaultTeam != "Neutral"))
                                {
                                    if (!PlayerData.TutorialState.Contains(TutorialState.GymTutorial) && UserSettings.CompleteTutorial)
                                    {
                                        if (PlayerData.Team == TeamColor.Neutral)
                                        {
                                            TeamColor team = TeamColor.Neutral;

                                            foreach (TeamColor _team in Enum.GetValues(typeof(TeamColor)))
                                            {
                                                if (UserSettings.DefaultTeam == _team.ToString())
                                                {
                                                    team = _team;
                                                }
                                            }

                                            if (team != TeamColor.Neutral)
                                            {
                                                var setplayerteam = await SetPlayerTeam(team);

                                                if (setplayerteam.Success)
                                                {
                                                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                                                    result = await MarkTutorialsComplete(new[] { TutorialState.GymTutorial });

                                                    if (!result.Success)
                                                    {
                                                        LogCaller(new LoggerEventArgs("Failed. Marking Gym tutorials completed..", LoggerTypes.Warning));
                                                        continue;
                                                    }

                                                    LogCaller(new LoggerEventArgs("Marking Gym tutorials completed.", LoggerTypes.Success));

                                                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                                }
                                            }
                                        }
                                    }

                                    if (PlayerData.TutorialState.Contains(TutorialState.GymTutorial) && UserSettings.CompleteTutorial)
                                    {
                                        //Check for missed tutorials
                                        foreach (TutorialState tuto in Enum.GetValues(typeof(TutorialState)))
                                        {
                                            if (!PlayerData.TutorialState.Contains(tuto))
                                            {
                                                DialogResult box = MessageBox.Show($"Tutorial {tuto.ToString()} is not completed on this account {PlayerData.Username}! Complete this?", "Confirmation", MessageBoxButtons.YesNo);

                                                if (box == DialogResult.Yes)
                                                {
                                                    result = await MarkTutorialsComplete(new[] { tuto });
                                                    if (result.Success)
                                                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                                }
                                            }
                                        }

                                        var gyminfo = await GymGetInfo(Building);
                                        if (gyminfo.Success)
                                        {
                                            LogCaller(new LoggerEventArgs("Gym Name: " + gyminfo.Data.Name, LoggerTypes.Info));
                                            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                        }
                                        else
                                            continue;

                                        MethodResult spingym = await SearchBuilding(Building);

                                        //OutOfRange will show up as a success
                                        if (spingym.Success)
                                        {
                                            currentFailedStops = 0;
                                            //Try to deploy, full gym is 6 now
                                            if (gyminfo.Data.GymStatusAndDefenders.GymDefender.Count < 6)
                                            {
                                                //Checks team color if same of player or Neutral
                                                if (Building.OwnedByTeam == PlayerData.Team || Building.OwnedByTeam == TeamColor.Neutral)
                                                {
                                                    //Check if config as deploy actived
                                                    if (UserSettings.DeployCreature)
                                                    {
                                                        //Try to deploy
                                                        await GymDeploy(Building);
                                                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                                    }
                                                }
                                            }
                                            //Here try to attack gym not released yet
                                            //
                                            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
                                        }
                                        else
                                        {
                                            if (currentFailedStops > 10)
                                            {
                                                break;
                                            }
                                            ++currentFailedStops;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!PlayerData.TutorialState.Contains(TutorialState.BuildingTutorial) && UserSettings.CompleteTutorial)
                                    {
                                        result = await MarkTutorialsComplete(new[] { TutorialState.BuildingTutorial, TutorialState.CreatureBerry, TutorialState.UseItem });

                                        if (!result.Success)
                                        {
                                            LogCaller(new LoggerEventArgs("Failed. Marking Building, Creatureberry, useitem, Creaturecapture tutorials completed..", LoggerTypes.Warning));

                                            break;
                                        }

                                        LogCaller(new LoggerEventArgs("Marking Building, Creatureberry, useitem, Creaturecapture tutorials completed.", LoggerTypes.Success));

                                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                    }

                                    if (UserSettings.RequestBuildingDetails)
                                    {
                                        var BuildingDetails = await BuildingDetails(Building);
                                        if (BuildingDetails.Success)
                                        {
                                            LogCaller(new LoggerEventArgs("Building Name: " + BuildingDetails.Data.Name, LoggerTypes.Info));
                                            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                        }
                                        else
                                            continue;
                                    }
                                    
                                    MethodResult searchResult = await SearchBuilding(Building);

                                    //OutOfRange will show up as a success
                                    if (searchResult.Success)
                                    {
                                        currentFailedStops = 0;
                                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                    }
                                    else
                                    {
                                        if (currentFailedStops > 10)
                                        {
                                            break;
                                        }
                                        ++currentFailedStops;
                                    }
                                }
                            }
                            else
                            {
                                LogCaller(new LoggerEventArgs(String.Format("Skipping Building. In cooldown"), LoggerTypes.Info));
                            }
                        }
                        else
                        {
                            LogCaller(new LoggerEventArgs(String.Format("Skipping Building. Inventory Currently at {0:0.00}% filled", filledInventorySpace), LoggerTypes.Info));
                        }
                        */

                        //Stop bot instantly
                        if (!IsRunning)
                        {
                            break;
                        }

                        // evolve, transfer, etc on first and every 10 stops
                        if (IsRunning && ((BuildingNumber > 4 && BuildingNumber % 10 == 0) || BuildingNumber == 1))
                        {

                            if (UserSettings.EvolveCreature)
                            {
                                MethodResult evolveResult = await EvolveFilteredCreature();

                                if (evolveResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                }
                            }

                            if (UserSettings.TransferCreature)
                            {
                                MethodResult transferResult = await TransferFilteredCreature();

                                if (transferResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                }
                            }

                            if (UserSettings.UpgradeCreature)
                            {
                                MethodResult upgradeResult = await UpgradeFilteredCreature();

                                if (upgradeResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                }
                            }

                            if (UserSettings.IncubateEggs)
                            {
                                MethodResult incubateResult = await IncubateEggs();

                                if (incubateResult.Success)
                                {
                                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                                }
                            }

                            UpdateInventory(InventoryRefresh.All); //all inventory
                        }

                        WaitPaused();

                        UpdateInventory(InventoryRefresh.Stats);

                        ++BuildingNumber;

                        if (UserSettings.MaxLevel > 0 && Level >= UserSettings.MaxLevel)
                        {
                            LogCaller(new LoggerEventArgs(String.Format("Max level of {0} reached.", UserSettings.MaxLevel), LoggerTypes.Info));
                            await ExportToPGPool();
                            Stop();
                        }

                        if (_totalZeroExpStops > 25)
                        {
                            LogCaller(new LoggerEventArgs("Potential Building SoftBan.", LoggerTypes.Warning));
                            AccountState = AccountState.SoftBan;
                            // reset values
                            _totalZeroExpStops = 0;
                            break;
                        }

                        if (_potentialBuildingBan)
                        {
                            //Break out of Building loop to test for ip ban
                            break;
                        }

                        if (Tracker.CreatureCaught >= UserSettings.CatchCreatureDayLimit && Tracker.BuildingsFarmed >= UserSettings.SpinBuildingsDayLimit)
                        {
                            LogCaller(new LoggerEventArgs("Daily limits reached. Stoping ...", LoggerTypes.Warning));
                            Stop();
                        }

                        if (UserSettings.UseLuckEggConst && Level >= UserSettings.LevelForConstLukky && IsRunning)
                        {
                            MethodResult luckEggResult = await UseLuckyEgg();

                            if (luckEggResult.Success)
                            {
                                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
                            }
                        }
                    }
                }
                catch (StackOverflowException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs(ex.Message, LoggerTypes.FatalError));
                    break;
                }
                catch (TaskCanceledException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs("TaskCanceledException. Restarting ...", LoggerTypes.Warning, ex));
                }
                catch (OperationCanceledException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs("OperationCanceledException. Stopping ...", LoggerTypes.Warning, ex));
                    break;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    AccountState = AccountState.Unknown;
                    LogCaller(new LoggerEventArgs("Skipping request. Restarting ...", LoggerTypes.Exception, ex));
                }
               catch (Exception ex)
                {
                    LogCaller(new LoggerEventArgs("Unknown exception occured. Restarting ...", LoggerTypes.Exception, ex));
                }

                #endregion

                currentFails = 0;
                _firstRun = false;
            }

            Stop();
            //Bot stopped all task end
            State = BotState.Stopped;
            LogCaller(new LoggerEventArgs(String.Format("Bot fully stopped at {0}", DateTime.Now), LoggerTypes.Info));

            if (_autoRestart)
            {
                _wasAutoRestarted = true;
                Start();
            }
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                _client.Logout();
                return;
            }

            //Bot wait for end actions in progress...
            State = BotState.Stopping;
            LogCaller(new LoggerEventArgs("Bot stopping. Please wait for actions to complete ...", LoggerTypes.Info));

            //Remove proxy
            if (UserSettings.AutoRemoveOnStop)
                RemoveProxy();

            _pauser.Set();
            _runningStopwatch.Stop();
            _failedInventoryReponses = 0;

            if (!_autoRestart)
            {
                _runningStopwatch.Reset();
            }

            IsRunning = false;
            _firstRun = true;
        }

        private void LoadFarmLocations()
        {
            FarmLocations = new List<FarmLocation>
            {
                new FarmLocation
                {
                    Name = "Current"
                },

                new FarmLocation
                {
                    Latitude = -33.870225,
                    Longitude = 151.208343,
                    Name = "Sydney, Australia"
                },

                new FarmLocation
                {
                    Latitude = 35.665705,
                    Longitude = 139.753348,
                    Name = "Tokyo, Japan"
                },

                new FarmLocation
                {
                    Latitude = 40.764665,
                    Longitude = -73.973184,
                    Name = "Central Park, NY"
                },

                new FarmLocation
                {
                    Latitude = 45.03009,
                    Longitude = -93.31934,
                    Name = "6Building, Cleveland"
                },

                new FarmLocation
                {
                    Latitude = 35.696428,
                    Longitude = 139.814404,
                    Name = "9Lures, Tokyo Japan"
                },

                new FarmLocation
                {
                    Latitude = 40.755184,
                    Longitude = -73.983724,
                    Name = "7Buildings, Central Park NY"
                },

                new FarmLocation
                {
                    Latitude = 51.22505600,
                    Longitude = 6.80713000,
                    Name = "Dusseldorf, Germany"
                },

                new FarmLocation
                {
                    Latitude = 46.50759600,
                    Longitude = 6.62834800,
                    Name = "Lausanne, Suisse"
                },

                new FarmLocation
                {
                    Latitude = 52.373806,
                    Longitude = 4.903985,
                    Name = "Amsterdam, Netherlands"
                }
            };
        }

        public void RemoveChest(FChest chestobj)
        {
            var chestContainer = UserMap.items.FirstOrDefault(o => o.GetType() == typeof(FChestUpdate)) as FChestUpdate;
            if (chestContainer != null)
            {
                chestContainer.chests.Remove(chestobj);
            }
        }
        public void ClearStats()
        {
            _fleeingCreatureResponses = 0;
            TotalBuildingExp = 0;
            Tracker.Values.Clear();
            Tracker.CalculatedTrackingHours();
        }
    }
}
