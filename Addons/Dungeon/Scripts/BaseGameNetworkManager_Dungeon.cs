using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;
using MultiplayerARPG.MMO;
using Cysharp.Threading.Tasks;
using System;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
#if UNITY_EDITOR || UNITY_SERVER
        public MapNetworkManager MapNetwork
        {
            get { return MMOServerInstance.Singleton.MapNetworkManager; }
        }
#endif

#if UNITY_EDITOR || UNITY_SERVER
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        [Header("Dungeon")]
        public ushort dungeonBossMsg = 500;
        public bool DungeonRunning { get; private set; }

        public System.DateTime LastBossTime { get; private set; }

        public float BossTime { get; private set; }

        private bool bossKilled;

        private DungeonMapInfo dungeonMapInfo;
        public DungeonMapInfo DungeonMap
        {
            get {
                DungeonMapInfo map = null;
                if (CurrentMapInfo != null && (CurrentMapInfo as DungeonMapInfo))
                    map = CurrentMapInfo as DungeonMapInfo;
                return map; }
        }

        [DevExtMethods("RegisterClientMessages")]
        protected void RegisterClientMessages_Dungeon()
        {
            RegisterClientMessage(dungeonBossMsg, HandleDungeonStatusAtClient);
        }

        [DevExtMethods("OnStartServer")]
        protected void OnStartServer_Dungeon()
        {
            CancelInvoke(nameof(Update_DungeonIn));
            InvokeRepeating(nameof(Update_DungeonIn), 1, 1);


            float repeat = DateTime.Now.Second;
            repeat = 60 - repeat;
            CancelInvoke(nameof(Update_DungeonOut));
            InvokeRepeating(nameof(Update_DungeonOut), repeat, 60);
        }

        [DevExtMethods("OnPeerConnected")]
        protected void OnPeerConnected_Dungeon(long connectionId)
        {
            SendDungeonStatus(connectionId);
        }

        [DevExtMethods("OnServerOnlineSceneLoaded")]
        protected void OnServerOnlineSceneLoaded_Dungeon()
        {
            if (!(CurrentMapInfo is DungeonMapInfo))
                return;

            DungeonMapInfo mapInfo = CurrentMapInfo as DungeonMapInfo;
            BossTime = mapInfo.battleDuration * 60f;
            LastBossTime = DateTime.Now;
        }

        [DevExtMethods("Clean")]
        protected void Clean_Dungeon()
        {
            CancelInvoke(nameof(Update_DungeonIn));
            CancelInvoke(nameof(Update_DungeonOut));

        }

        public void SendDungeonStatus()
        {
            if (!IsServer)
                return;
            foreach (long connectionId in Server.ConnectionIds)
            {
                SendDungeonStatus(connectionId);
            }
        }

        public void SendDungeonStatus(long connectionId)
        {
            if (!IsServer)
                return;
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, dungeonBossMsg, (writer) =>
            {
                writer.Put(DungeonRunning);
                writer.Put(BossTime);
            });
        }

        private void HandleDungeonStatusAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;
            DungeonRunning = messageHandler.Reader.GetBool();
            BossTime = messageHandler.Reader.GetFloat();
        }

        public void Update_DungeonOut()
        {
            if (!IsServer || CurrentMapInfo == null)
                return;

            var Dungeons = CurrentGameInstance.Dungeons;
            DateTime currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            foreach (var evnt in Dungeons.Keys)
            {
                DateTime dungeonTime = new DateTime(Dungeons[evnt].Year, Dungeons[evnt].Month, Dungeons[evnt].Day, Dungeons[evnt].Hour, Dungeons[evnt].Minute, 0);

                if (DateTime.Compare(currentDate, dungeonTime) == 0)
                {
                    SendGlobalAnnounceDungeon(string.Format(DungeonMap.eventStartedMessage, evnt.eventTitle));
                }

                if(evnt.alerTimes != null && evnt.alerTimes.Length > 0)
                {
                    foreach(int time in evnt.alerTimes)
                    {
                        if (currentDate == dungeonTime.AddMinutes(-time))
                        {
                            foreach (IPlayerCharacterData player in ServerUserHandlers.GetPlayerCharacters())
                            {
                                BasePlayerCharacterEntity characterEntity = player as BasePlayerCharacterEntity;
                                SendMessageDungeon(characterEntity.ConnectionId, string.Format(evnt.eventAlertMessage, evnt.eventTitle, time));
                            }
                            break;
                        }
                    }
                }
            }
        }
        public void Update_DungeonIn()
        {
            if (!IsServer || CurrentMapInfo == null || !(CurrentMapInfo is DungeonMapInfo))
                return;

            if(!DungeonRunning && DungeonMap.IsOn)
            {
                DungeonRunning = true;
                SendDungeonStatus();
            }

            if (DungeonRunning && !DungeonMap.IsOn)
            {
                SendGlobalAnnounceDungeon(string.Format(DungeonMap.eventEndedMessage, DungeonMap.eventTitle));
                DungeonRunning = false;
                SendDungeonStatus();
                ReturnTownPlayers();
            }

            if (DungeonRunning)
            {

                if(BossTime > 0)
                BossTime -= 1f;

                SendDungeonStatus();

                if ((DateTime.Now - LastBossTime).TotalMinutes >= DungeonMap.battleDuration)
                {
                    foreach (IPlayerCharacterData player in ServerUserHandlers.GetPlayerCharacters())
                    {
                        DungeonRunning = false;
                        BasePlayerCharacterEntity characterEntity = player as BasePlayerCharacterEntity;
                        SendMessageDungeon(characterEntity.ConnectionId, DungeonMap.eventFinishBattleDuration);
                        WarpCharacter(WarpPortalType.Default, characterEntity, characterEntity.RespawnMapName, characterEntity.RespawnPosition, false, Vector3.zero);
                    }
                }
            }

            if(bossKilled && (DateTime.Now - LastBossTime).TotalSeconds >= DungeonMap.warpToTown)
            {
                ReturnTownPlayers();
            }
        }

        void ReturnTownPlayers()
        {
            foreach (IPlayerCharacterData player in ServerUserHandlers.GetPlayerCharacters())
            {
                BasePlayerCharacterEntity characterEntity = player as BasePlayerCharacterEntity;
                WarpCharacter(WarpPortalType.Default, characterEntity, characterEntity.RespawnMapName, characterEntity.RespawnPosition, false, Vector3.zero);
            }
        }
        public void BossKilled(bool isParty, string characterName)
        {
            DungeonMapInfo mapInfo = DungeonMap;

            DungeonRunning = false;
            string message;

            LastBossTime = DateTime.Now;

            BossTime = mapInfo.warpToTown;

            bossKilled = true;
            if (isParty)
            {
                message = string.Format(mapInfo.eventBossKilledParty, characterName, mapInfo.DungeonBoss.Title);
            }
            else
            {
                message = string.Format(mapInfo.eventBossKilledSolo, characterName, mapInfo.DungeonBoss.Title);
            }
            foreach (IPlayerCharacterData player in ServerUserHandlers.GetPlayerCharacters())
            {
                DungeonRunning = false;
                BasePlayerCharacterEntity characterEntity = player as BasePlayerCharacterEntity;
                SendMessageDungeon(characterEntity.ConnectionId, string.Format(mapInfo.eventKilledBossReturnTown, mapInfo.warpToTown));
            }

            SendGlobalAnnounceDungeon(message);

        }

        public void DungeonTrigger()
        {
            DungeonRunning = false;
            BossTime = DungeonMap.warpToTown;
            LastBossTime = DateTime.Now;
            bossKilled = true;
            foreach (IPlayerCharacterData player in ServerUserHandlers.GetPlayerCharacters())
            {
                BasePlayerCharacterEntity characterEntity = player as BasePlayerCharacterEntity;
                SendMessageDungeon(characterEntity.ConnectionId, string.Format(DungeonMap.eventKilledBossReturnTown, DungeonMap.warpToTown));
            }
        }

        public void DungeonDoorBreak(string doorId)
        {
            foreach (IPlayerCharacterData player in ServerUserHandlers.GetPlayerCharacters())
            {
                BasePlayerCharacterEntity characterEntity = player as BasePlayerCharacterEntity;
                SendMessageDungeon(characterEntity.ConnectionId, string.Format(DungeonMap.eventDoorDestroyed, doorId));
            }
        }

        public bool CheckMapPartyMembersDungeon(BasePlayerCharacterEntity characterEntity)
        {
            PartyData party;
            if(GameInstance.ServerPartyHandlers.TryGetParty(characterEntity.PartyId, out party))
            {
                if(party != null)
                {
                    bool notMemberMap = false;
                    SocialCharacterData[] members =  party.GetMembers();
                    foreach(SocialCharacterData member in members)
                    {
                        IPlayerCharacterData character;
                        if(!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(member.id, out character))
                        {
                            notMemberMap = true;
                        }
                    }
                    return notMemberMap;
                }
            }

            return false;
        }


        public bool CheckPartyLevelDungeon(BasePlayerCharacterEntity characterEntity, DungeonMapInfo mapInfo)
        {
            PartyData party;
            if (GameInstance.ServerPartyHandlers.TryGetParty(characterEntity.PartyId, out party))
            {
                if (party != null)
                {
                    bool notenoughLevel = false;
                    SocialCharacterData[] members = party.GetMembers();
                    foreach (SocialCharacterData member in members)
                    {
                        IPlayerCharacterData character;
                        if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(member.id, out character))
                        {
                            if(character.Level < mapInfo.minLevel || character.Level > mapInfo.maxLevel)
                            {
                                notenoughLevel = true;
                            }
                        }
                    }
                    if(notenoughLevel)
                    {
                        ServerGameMessageHandlers.SendGameMessageByCharacterId(characterEntity.Id, UITextKeys.UI_ERROR_NOT_ENOUGH_PARTY_MEMBER_LEVEL_DUNGEON);
                    }
                    return notenoughLevel;
                }
            }

            if(party == null)
            {
                if(characterEntity.Level < mapInfo.minLevel || characterEntity.Level > mapInfo.maxLevel)
                {
                    ServerGameMessageHandlers.SendGameMessageByCharacterId(characterEntity.Id, UITextKeys.UI_ERROR_NOT_ENOUGH_LEVEL);
                    return true;
                }
            }

            return false;
        }


        private void SendGlobalAnnounceDungeon(string message)
        {
#if UNITY_EDITOR || UNITY_SERVER
            ChatMessage chat = new ChatMessage()
            {
                channel = ChatChannel.System,
                senderName = CHAT_SYSTEM_ANNOUNCER_SENDER,
                message = message,
                sendByServer = true,

            };

            if (MapNetwork.ClusterClient.IsNetworkActive)
            {
                MapNetwork.ClusterClient.SendPacket(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) =>
                {
                    writer.PutValue(chat);
                    writer.Put("");
                    writer.Put("");
                });
            }
#endif
        }

        private void SendMessageDungeon(long connectionId, string message)
        {
            ChatMessage chat = new ChatMessage()
            {
                channel = ChatChannel.System,
                senderName = CHAT_SYSTEM_ANNOUNCER_SENDER,
                message = message,
                sendByServer = true,
            };
            
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Chat, chat);
        }
    }

}