using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        /// <summary>
        /// Variables
        /// </summary>
        public Dictionary<string, PlayingCharacterData> PlayersKDA = new Dictionary<string, PlayingCharacterData>();
        DateTime ServerStartTime;

        /// <summary>
        /// Client Messages
        /// </summary>
        [Header("Event Manager")]
        public ushort EventStatusMsgType = 10200;
        public ushort EventPlayerListMessageType = 10201;
        public ushort EventFinishedMsgType = 10202;

        /// <summary>
        /// Events
        /// </summary>
        public event System.Action<PlayingCharacterDataMessage> onReceivePlayersList;
        public event System.Action<int> onEventMatchFinished;

        #region DevExt Methods
        /// <summary>
        /// Client Messages Registration Extentions
        /// </summary>
        /// DevExtMethod that is called when Client messages are registered.
        /// These will handles Event Status, Player List at clients.
        [DevExtMethods("RegisterClientMessages")]
        protected void RegisterClientMessages_MatchEvent()
        {
            RegisterClientMessage(EventStatusMsgType, HandleMatchEventStatusAtClient);
            RegisterClientMessage(EventFinishedMsgType, HandleEventFinishedAtClient);
            RegisterClientMessage(EventPlayerListMessageType, HandlePlayerListAtClient);
        }

        /// <summary>
        /// On Server Starts Extentions
        /// </summary>
        /// DevExtMethod that is called when a map server has started.
        /// This sets up our server events for when a player spawns in arena.
        /// and invoke Match updater repeatedly every minute.
        [DevExtMethods("OnServerOnlineSceneLoaded")]
        protected void OnStartServer_MatchManager()
        {
            //Setup Clean Player KDA
            if (CurrentMapInfo is MatchEventMapInfo)
            {
                PlayersKDA = new Dictionary<string, PlayingCharacterData>();
            }

            //Setup Team Divide Event
            onRegisterCharacter += SetPlayerCharacterTeam;
            ServerStartTime = DateTime.Now;
            CancelInvoke(nameof(Update_MatchManager));

            //Run a event match update every minute.
            float seconds = DateTime.Now.Second;
            seconds = 60 - seconds;
            InvokeRepeating(nameof(Update_MatchManager), seconds, 60);

            //Run persistent match update every x seconds.
            InvokeRepeating(nameof(Update_PersistentMapManager), seconds, 10);

            if (CurrentMapInfo is MatchEventMapInfo)
                this.InvokeInstanceDevExtMethods("OnStartServer_MatchManager");
        }

        /// <summary>
        /// On Peer Connected Extentions
        /// </summary>
        /// DevExtMethod that is called when a player has made connection to a map server
        /// This will send the all Events status to the player client, to sync events to server.
        [DevExtMethods("OnPeerConnected")]
        protected virtual void OnPeerConnected_MatchManager(long connectionId)
        {
            foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                SendMatchEventStatus(connectionId, matchEvents.name, matchEvents.starting);
        }
        #endregion

        #region Client Message Handlers

        /// <summary>
        /// CLient Message Handlers
        /// </summary>
        /// <param name="messageHandler"></param>
        /// This Region is for all messages the client will receive from server.
        private void HandleEventFinishedAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;

            bool Finished = messageHandler.Reader.GetBool();
            int teamId = messageHandler.Reader.GetPackedInt();

            if (Finished)
                if (onEventMatchFinished != null)
                    onEventMatchFinished.Invoke(teamId);
        }

        private void HandleMatchEventStatusAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;

            string name = messageHandler.Reader.GetString();
            bool active = messageHandler.Reader.GetBool();
            foreach (MatchEvents e in GameInstance.Singleton.MatchTimers.Keys)
                if (e.name.Equals(name))
                {
                    e.starting = active;
                }
        }

        private void HandlePlayerListAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;

            PlayingCharacterDataMessage characters = messageHandler.ReadMessage<PlayingCharacterDataMessage>();

            if (onReceivePlayersList != null)
                onReceivePlayersList.Invoke(characters);
        }

        #endregion

        #region Server Send Messages Handlers
        /// <summary>
        /// Server Messages
        /// </summary>
        /// This Region handles all messages that are send from server to clients regarding events.
        public void SendPlayerListToClient(long connectionId)
        {
            if (!IsServer)
                return;
            PlayingCharacterDataMessage playingCharacterDataMessage = new PlayingCharacterDataMessage();
            playingCharacterDataMessage.playingCharacterDatas = PlayersKDA.Values.ToArray();

            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, EventPlayerListMessageType, playingCharacterDataMessage);
        }

        public void SendMatchEventStatus(string name, bool active)
        {
            if (!IsServer)
                return;
            foreach (long connectionId in Server.ConnectionIds)
            {
                SendMatchEventStatus(connectionId, name, active);
            }
        }

        public void SendMatchEventStatus(long connectionId, string name, bool active)
        {
            if (!IsServer)
                return;

            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, EventStatusMsgType, (writer) =>
            {
                writer.Put(name);
                writer.Put(active);
            });
        }

        public void SendEventFinishedStatus(MatchEvents match)
        {
            if (!IsServer)
                return;

            int Teamid = RewardWinner(match);
            foreach (long connectionId in Server.ConnectionIds)
            {
                SendEventFinishedStatus(connectionId, Teamid);
            }
        }

        public void SendEventFinishedStatus(long connectionId, int teamid)
        {
            if (!IsServer)
                return;

            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, EventFinishedMsgType, (writer) =>
            {
                writer.Put(true);
                writer.PutPackedInt(teamid);
            });
        }

        public virtual void SendSystemNotice(SystemNoticeType systemNoticeType, MatchEvents matchEvents, int Timer)
        {
            switch (systemNoticeType)
            {
                case SystemNoticeType.Alert:
                    ServerSendSystemAnnounce(matchEvents.title + " will begin in " + Timer + " minutes");
                    break;
                case SystemNoticeType.Started:
                    ServerSendSystemAnnounce(matchEvents.eventStartedMessage);
                    break;
                case SystemNoticeType.Ended:
                    ServerSendSystemAnnounce(matchEvents.eventEndedMessage);
                    break;
            }
        }
        #endregion

        #region Team Divide section
        /// <summary>
        /// Player count integer.
        /// </summary>
        /// This is used to determine how many players joined the instance, and give them a team.
        int playerCount = 0;
        /// <summary>
        /// On player Registered
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="playerCharacter"></param>
        /// When a player is registered on the map this will check if event is a team event.
        /// If event is team event they will receive a team.
        /// Players will be added to the PlayerKDA Dictionairy to be tracked on kills and deaths.
        public virtual void SetPlayerCharacterTeam(long connectionId, BasePlayerCharacterEntity playerCharacter)
        {
            if (CurrentMapInfo == null || !(CurrentMapInfo as MatchEventMapInfo))
                return;

            MatchEventMapInfo matchEventMapInfo = CurrentMapInfo as MatchEventMapInfo;

            ///If mapinfo override start position.
            if (matchEventMapInfo.OverrideStartPosition)
            {
                MatchEvents matchEvent = null;
                foreach (var matches in GameInstance.Singleton.MatchEvents)
                {
                    ///Find match based on type 
                    if (matchEventMapInfo.matchEvent == matches)
                        matchEvent = matches;
                }

                foreach (var matches in GameInstance.Singleton.RunningEvents)
                {
                    ///Find match based on type 
                    if (matchEventMapInfo.matchEvent == matches)
                        matchEvent = matches;
                }

                if (matchEvent == null)
                    return;

                TeamData team = new TeamData();

                ///Assign team based on player count.
                if (matchEvent.SplitInTeams)
                {
                    if (playerCount % 2 == 0)
                        team.id = matchEvent.Teams[0].DataId;
                    else
                        team.id = matchEvent.Teams[1].DataId;

                    playerCharacter.TeamData = team;
                }

                ///Warp team to specified mapinfo team base.
                WarpCharacter(WarpPortalType.Default,
                    playerCharacter,
                    null,
                    matchEventMapInfo.GetStartMapPosition(team.id),
                    false, Vector3.zero);

                playerCount++;
            }

            ///Reset the players hp/mp/stamina to max when event starts.
            playerCharacter.CurrentHp = playerCharacter.MaxHp;
            playerCharacter.CurrentMp = playerCharacter.MaxMp;
            playerCharacter.CurrentStamina = playerCharacter.MaxStamina;

            ///Create PlayingCharacter K/D Data, and add player to List.
            PlayingCharacterData playingCharacterData = PlayingCharacterData.Create(playerCharacter, 0, 0);
            if(!PlayersKDA.ContainsKey(playerCharacter.CharacterName))
            PlayersKDA.Add(playerCharacter.CharacterName, playingCharacterData);
        }

        #endregion

        #region Match Updater
        /// <summary>
        /// EVent Matches Updater
        /// </summary>
        /// This is called once a minute to check and update every event.
        public virtual void Update_MatchManager()
        {
            if (!IsServer || CurrentMapInfo == null)
                return;

            ///Get current time - seconds.
            DateTime dateNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            DateTime dateValue;

            ///Get Match list.
            var MatchTimers = CurrentGameInstance.MatchTimers;

            ///Cycles through list to check Events Status.
            foreach (var matches in MatchTimers.Keys.ToList())
            {
                dateValue = new DateTime(MatchTimers[matches].Year, MatchTimers[matches].Month, MatchTimers[matches].Day, MatchTimers[matches].Hour, MatchTimers[matches].Minute, 0);

                ///Checks if event started. if yes send out system announcement and teleport players.
                if (DateTime.Compare(dateNow, dateValue) == 0)
                {
                    matches.starting = false;
                    SendSystemNotice(SystemNoticeType.Started, matches, 0);
                    SendMatchEventStatus(matches.name, matches.starting);

                    if (CurrentMapInfo is MatchEventHubMapInfo)
                    {
                        MatchEventHubMapInfo mapInfo = CurrentMapInfo as MatchEventHubMapInfo;
                        if (mapInfo.matchEvent == matches)
                        {
                            TeleportPlayers(matches);
                        }
                    }

                    if (matches.RunTestEvents)
                        MatchTimers[matches] = DateTime.Now.AddMinutes(matches.Interval);
                    else
                        CurrentGameInstance.SetEventTimer(matches);

                }

                ///Checks if event has ended, If ended update the events list for the next scheduled time.
                if (DateTime.Compare(dateNow, dateValue.AddMinutes(matches.GetEventDate(DateTime.Now).EventDuration)) == 0 && matches.sendEndingMessage)
                {
                    SendSystemNotice(SystemNoticeType.Ended, matches, 0);
                }

                ///Checks if event has ended, If ended send out status to players and expel them from arena.
                if (CurrentMapInfo is MatchEventMapInfo)
                {
                    DateTime ServerBoot = new DateTime(ServerStartTime.Year, ServerStartTime.Month, ServerStartTime.Day, ServerStartTime.Hour, ServerStartTime.Minute, 0);
                    if (DateTime.Compare(dateNow, ServerBoot.AddMinutes(matches.GetEventDate(DateTime.Now).EventDuration)) >= 0)
                    {
                        MatchEventMapInfo mapInfo = CurrentMapInfo as MatchEventMapInfo;
                        if (mapInfo.matchEvent == matches)
                        {
                            if (matches.sendEndingMessage)
                                SendEventFinishedStatus(matches);
                            Invoke(nameof(ExpelPlayers), matches.ExpelDelay);
                        }
                    }
                }

                ///Checks if event should announce when its about to start ahead of time.
                ///If event has aler timers, send out system announcement to all players.
                if (matches.alertAheadOfTime)
                {
                    if (matches.alertTimer.Length > 0)
                    {
                        bool announced = false;
                        foreach (int Timer in matches.alertTimer)
                        {
                            if (dateNow == dateValue.AddMinutes(-Timer))
                            {
                                matches.starting = true;
                                SendMatchEventStatus(matches.name, matches.starting);
                                SendSystemNotice(SystemNoticeType.Alert, matches, Timer);
                                announced = true;
                                break;
                            }
                        }

                        if (!announced)
                            if (dateNow > dateValue.AddMinutes(-matches.alertTimer[0]) && DateTime.Compare(dateNow, dateValue) == -1)
                            {
                                matches.starting = true;
                                SendMatchEventStatus(matches.name, matches.starting);
                            }
                    }
                }
            }
        }

        public virtual void Update_PersistentMapManager()
        {
            if (!IsServer || CurrentMapInfo == null)
                return;

            ///Get current time - seconds.
            DateTime dateNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            ///Cycles through list to check Events Status.
            foreach (MatchEvents matches in CurrentGameInstance.RunningEvents)
            {

                if (CurrentMapInfo is MatchEventHubMapInfo)
                {
                    MatchEventHubMapInfo mapInfo = CurrentMapInfo as MatchEventHubMapInfo;
                    ///Get all players on Selected Map.

                    if (mapInfo.ConsistentEvent)
                    {
                        List<IPlayerCharacterData> playerCharacters = new List<IPlayerCharacterData>(ServerUserHandlers.GetPlayerCharacters());

                        if (mapInfo.matchEvent == matches && playerCharacters.Count >= matches.MinimumPlayersInInstance)
                        {
                            TeleportPlayers(matches);
                        }
                    }
                }

                ///Checks if event has ended, If ended send out status to players and expel them from arena.
                if (CurrentMapInfo is MatchEventMapInfo)
                {
                    DateTime ServerBoot = new DateTime(ServerStartTime.Year, ServerStartTime.Month, ServerStartTime.Day, ServerStartTime.Hour, ServerStartTime.Minute, 0);
                    if (DateTime.Compare(dateNow, ServerBoot.AddMinutes(matches.GetEventDate(DateTime.Now).EventDuration)) >= 0)
                    {
                        MatchEventMapInfo mapInfo = CurrentMapInfo as MatchEventMapInfo;
                        if (mapInfo.matchEvent == matches)
                        {
                            RewardWinner(matches);
                            Invoke(nameof(ExpelPlayers), matches.ExpelDelay);
                        }
                    }
                    else
                    {
                        this.InvokeInstanceDevExtMethods("Update_PersistentMapManager",matches);
                    }
                }
            }
        }

        #endregion

        #region Player Teleport Handlers
        /// <summary>
        /// Player Teleport Handlers.
        /// </summary>
        /// This will find all players in  the Event hub. groups them in categories, and teleports them to the event.
        protected virtual void TeleportPlayers(MatchEvents matchEvent)
        {
            string instanceId;

            ///Get all players on Selected Map.
            List<IPlayerCharacterData> playerCharacters = new List<IPlayerCharacterData>(ServerUserHandlers.GetPlayerCharacters());
            ///Save the players up in different levelranges.
            Dictionary<List<IPlayerCharacterData>, LevelRange> LevelSplit = new Dictionary<List<IPlayerCharacterData>, LevelRange>();

            ///divide players into levelranges.
            foreach (LevelRange levelRange in matchEvent.levelRanges)
            {
                List<IPlayerCharacterData> playerLevels = new List<IPlayerCharacterData>();
                foreach (BasePlayerCharacterEntity player in playerCharacters)
                    if (player.Level >= levelRange.MinimumLevel && player.Level <= levelRange.MaximumLevel)
                        playerLevels.Add(player);

                LevelSplit.Add(playerLevels, levelRange);
            }

            ///split player into teams.
            List<IPlayerCharacterData> Groups;
            foreach (var playercategory in LevelSplit)
            {
                ///Count how many instances should there be made
                int AmountOfTeams = (int)Math.Ceiling(((float)playercategory.Key.Count / (float)matchEvent.AmountPlayersPerInstance));
                int playerindex = 0;
                for (int i = 0; i < AmountOfTeams; i++)
                {
                    Groups = new List<IPlayerCharacterData>();
                    for (int j = 0; j < matchEvent.AmountPlayersPerInstance; j++)
                    {

                        Groups.Add(playercategory.Key[playerindex]);
                        playerindex++;

                        if (playerindex > playercategory.Key.Count - 1)
                            break;
                    }
                    instanceId = GenericUtils.GetUniqueId();

                    ///Teleport Group to event arena.
                    if (Groups.Count >= matchEvent.MinimumPlayersInInstance)
                        WarpCharacterToEvent(matchEvent.warpPortalType,
                            Groups,
                            instanceId,
                            playercategory.Value.matchEventMapInfo.name,
                            playercategory.Value.matchEventMapInfo.StartPosition,
                            false, Vector3.zero);
                }
            }
        }

        /// <summary>
        /// Explel Players on Event end.
        /// </summary>
        /// Find all players in the arena and teleport them back to their spawn position.
        public void ExpelPlayers()
        {
            /// Teleport characters outside of event back to Respawn position
            List<IPlayerCharacterData> playerCharacters = new List<IPlayerCharacterData>(ServerUserHandlers.GetPlayerCharacters());
            TeamData team = new TeamData();
            team.id = 0;
            for (int i = 0; i < playerCharacters.Count; ++i)
            {
                playerCharacters[i].TeamData = team;
                WarpCharacter(WarpPortalType.Default,
                 playerCharacters[i] as BasePlayerCharacterEntity,
                 playerCharacters[i].RespawnMapName,
                 playerCharacters[i].RespawnPosition,
                 false, Vector3.zero);
            }
        }

        /// <summary>
        /// Checks if there is only 1 player in the arena left.
        /// </summary>
        /// This is used for last man standing, When there is only 1 survivor left, this will automatically end the event.
        /// The player doesnt need to w8 for the whole duration before being teleported out.
        public virtual void LastPlayerCheck()
        {
            var MatchTimers = CurrentGameInstance.MatchTimers;

            if (ServerUserHandlers.GetPlayerCharacters().ToArray().Length <= 1)
                foreach (var matches in MatchTimers.Keys.ToList())
                {
                    MatchEventMapInfo mapInfo = CurrentMapInfo as MatchEventMapInfo;
                    if (mapInfo.matchEvent.matchType == matches.matchType)
                    {
                        SendEventFinishedStatus(matches);
                        Invoke(nameof(ExpelPlayers), matches.ExpelDelay);
                    }
                }
        }
        /// <summary>
        /// Warp To events Handler.
        /// </summary>
        /// <param name="warpPortalType"></param>
        /// <param name="playerCharacterEntities"></param>
        /// <param name="instanceid"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        public virtual void WarpCharacterToEvent(WarpPortalType warpPortalType, List<IPlayerCharacterData> playerCharacterEntities, string instanceid, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            switch (warpPortalType)
            {
                case WarpPortalType.Default:
                    WarpCharacterToEvent(playerCharacterEntities, instanceid, mapName, position, overrideRotation, rotation);
                    break;
                case WarpPortalType.EnterInstance:
                    WarpCharacterToEventInstance(playerCharacterEntities, instanceid, mapName, position, overrideRotation, rotation);
                    break;
            }

        }
        /// <summary>
        /// Warp to Events.
        /// </summary>
        /// <param name="playerCharacterEntities"></param>
        /// <param name="instanceid"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        public virtual void WarpCharacterToEvent(List<IPlayerCharacterData> playerCharacterEntities, string instanceid, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            foreach (BasePlayerCharacterEntity basePlayerCharacter in playerCharacterEntities)
                WarpCharacter(basePlayerCharacter, mapName, position, overrideRotation, rotation);
        }
        /// <summary>
        /// Warp to Instance of Event.
        /// </summary>
        /// <param name="playerCharacterEntities"></param>
        /// <param name="instanceid"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        public virtual void WarpCharacterToEventInstance(List<IPlayerCharacterData> playerCharacterEntities, string instanceid, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            ///LAN: Lan does not use instances, so the default is the same as normal warping.
            foreach (BasePlayerCharacterEntity basePlayerCharacter in playerCharacterEntities)
            {
                WarpCharacter(basePlayerCharacter, mapName, position, overrideRotation, rotation);
            }
        }
        #endregion

        #region Match Reward Handlers 

        /// <summary>
        /// Reward for each type of event.
        /// </summary>
        /// <param name="matchEvent"></param>
        /// Checks event type and send out rewards based on the type.
        /// if event was a team match. returns team id to client for winning team.
        protected virtual int RewardWinner(MatchEvents matchEvent)
        {
            switch (matchEvent.matchType)
            {
                case MatchType.LastManStanding:
                    return LastManStandingReward(matchEvent);
                case MatchType.BattleArena:
                    return BattleArenaReward(matchEvent);
                case MatchType.TeamDeathMatch:
                    return TeamDeathMatchReward(matchEvent);
            }
            ///Invokes DevExt Methods for Rewardwinner for extra rewards or Different Match types.
            this.InvokeInstanceDevExtMethods("RewardWinner", matchEvent);

            return 0;
        }

        protected virtual int LastManStandingReward(MatchEvents matchEvent)
        {
            ///Rewards winning player.
            BasePlayerCharacterEntity playerCharacterData = null;

            ///Checks if there is only 1 survivor left.
            if (ServerUserHandlers.GetPlayerCharacters().ToArray().Length == 1)
            {
                if (ServerUserHandlers.TryGetPlayerCharacterByName(ServerUserHandlers.GetPlayerCharacters().ToArray()[0].CharacterName, out playerCharacterData))
                {
                    playerCharacterData.RewardExp(matchEvent.rewardsOnWin, 1f, RewardGivenType.None);
                    playerCharacterData.RewardCurrencies(matchEvent.rewardsOnWin, 1f, RewardGivenType.None);
                }
            }
            else
            {
                ///If there is multiple players still in arena after event finishes. checks which of the players had the most kills.
                ///The player with the most kills will win the event and get rewarded.
                ///Sort players in the match based on kills from highest to lowest.
                var sortedDict = from entry in PlayersKDA orderby entry.Value.kills descending select entry;

                if (sortedDict.ToArray().Length > 1)
                    if (sortedDict.ToArray()[0].Value.kills > sortedDict.ToArray()[1].Value.kills)
                    {
                        if (ServerUserHandlers.TryGetPlayerCharacterByName(sortedDict.ToArray()[0].Value.characterName, out playerCharacterData) && ServerUserHandlers.GetPlayerCharacters().Contains(playerCharacterData))
                        {
                            playerCharacterData.RewardExp(matchEvent.rewardsOnWin, 1f, RewardGivenType.None);
                            playerCharacterData.RewardCurrencies(matchEvent.rewardsOnWin, 1f, RewardGivenType.None);
                        }
                    }
            }

            ///Send out global announcement to everyone, this will broadcast the winner.
            if (matchEvent.GlobalAnnouncement && playerCharacterData != null)
            {
                ServerSendSystemAnnounce(playerCharacterData.CharacterName + " " + matchEvent.GlobalAnnounceMsg);
            }

            return 0;
        }
        protected virtual int BattleArenaReward(MatchEvents matchEvent)
        {
            ///Reward players on the winning player.
            BasePlayerCharacterEntity playerCharacterData = null;
            ///Sort players in the match based on kills from highest to lowest.
            var sortedDict = from entry in PlayersKDA orderby entry.Value.kills descending select entry;

            ///When event ends, this will checks which of the players had the most kills.
            ///The player with the most kills will win the event and get rewarded.
            if (sortedDict.ToArray().Length > 1)
                if (sortedDict.ToArray()[0].Value.kills > sortedDict.ToArray()[1].Value.kills)
                {
                    if (ServerUserHandlers.TryGetPlayerCharacterByName(sortedDict.ToArray()[0].Value.characterName, out playerCharacterData) && ServerUserHandlers.GetPlayerCharacters().Contains(playerCharacterData))
                    {
                        playerCharacterData.RewardExp(matchEvent.rewardsOnWin, 1f, RewardGivenType.None);
                        playerCharacterData.RewardCurrencies(matchEvent.rewardsOnWin, 1f, RewardGivenType.None);
                    }
                }

            ///Send out global announcement to everyone, this will broadcast the winner.
            if (matchEvent.GlobalAnnouncement && playerCharacterData != null)
            {
                ServerSendSystemAnnounce(playerCharacterData.CharacterName + " " + matchEvent.GlobalAnnounceMsg);
            }
            return 0;
        }

        protected virtual int TeamDeathMatchReward(MatchEvents matchEvent)
        {
            Dictionary<int, int> TeamPoint = new Dictionary<int, int>();

            ///Divide the playerKDA List into Teams, and count total Kills from each.
            foreach (PlayingCharacterData playingCharacterData in PlayersKDA.Values)
            {
                if (TeamPoint.ContainsKey(playingCharacterData.teamID))
                    TeamPoint[playingCharacterData.teamID] = TeamPoint[playingCharacterData.teamID] + playingCharacterData.kills;
                else
                    TeamPoint.Add(playingCharacterData.teamID, playingCharacterData.kills);
            }

            ///Sort the point per team list from Highest to Lowest.
            var sortedDict = from entry in TeamPoint orderby entry.Value descending select entry;

            ///Reward players on the winning team.
            BasePlayerCharacterEntity playerCharacterData;
            if (sortedDict.ToArray().Length > 1)
                if (sortedDict.ToArray()[0].Value > sortedDict.ToArray()[1].Value)
                {
                    foreach (PlayingCharacterData playingCharacterData in PlayersKDA.Values)
                    {
                        if (playingCharacterData.teamID == sortedDict.First().Key)
                            if (ServerUserHandlers.TryGetPlayerCharacterByName(playingCharacterData.characterName, out playerCharacterData) && ServerUserHandlers.GetPlayerCharacters().Contains(playerCharacterData))
                            {
                                playerCharacterData.RewardExp(matchEvent.rewardsOnWin, 1f, RewardGivenType.None);
                                playerCharacterData.RewardCurrencies(matchEvent.rewardsOnWin, 1f, RewardGivenType.None);
                            }
                    }

                    ///Send out global announcement to everyone, this will broadcast the winning team.
                    if (matchEvent.GlobalAnnouncement)
                    {
                        foreach (Team team in matchEvent.Teams)
                            if (team.DataId == sortedDict.First().Key)
                                ServerSendSystemAnnounce(team.Title + " " + matchEvent.GlobalAnnounceMsg);
                    }
                    return sortedDict.First().Key;
                }
            return 0;
        }


        #endregion

    }

    public enum SystemNoticeType : byte
    {
        Alert,
        Started,
        Ended,
    }

    public struct PlayingCharacterDataMessage : INetSerializable
    {

        public PlayingCharacterData[] playingCharacterDatas;

        public void Deserialize(NetDataReader reader)
        {
            playingCharacterDatas = reader.GetArray<PlayingCharacterData>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(playingCharacterDatas);
        }
    }
}
