using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;
using MultiplayerARPG.MMO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {

        public Dictionary<string, TournamentCharacter> participants = new Dictionary<string, TournamentCharacter>();
        public Dictionary<int, TournamentFinished> tournamentFinishs = new Dictionary<int, TournamentFinished>();

        [Header("Tournament")]
        public ushort tournamentFinishMsg = 570;
        public ushort tournamentFightMsg = 571;
        public ushort tournamentResetMsg = 572;
        public ushort tournamentParticipantMsg = 573;

        public float TournamentFightCountDown { get; private set; }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public MapNetworkManager MapNetworkTournament
        {
            get { return MMOServerInstance.Singleton.MapNetworkManager; }
        }
#endif
        public int RegisteredCount
        {
            get { return participants.Count; }
        }

        private TournamentMapInfo tournamentMapInfo;
        public TournamentMapInfo TournamentMap
        {
            get
            {
                TournamentMapInfo map = null;
                if (CurrentMapInfo != null && (CurrentMapInfo as TournamentMapInfo))
                    map = CurrentMapInfo as TournamentMapInfo;
                return map;
            }
        }

        public bool TournamentReset { get; private set; }

        [DevExtMethods("RegisterClientMessages")]
        protected void RegisterClientMessages_Tournament()
        {
            RegisterClientMessage(tournamentFightMsg, HandleTournamentFightAtClient);
            RegisterClientMessage(tournamentResetMsg, HandleTournamentResetClient);
            RegisterClientMessage(tournamentParticipantMsg, HandleTournamentParticipantClient);
            RegisterClientMessage(tournamentFinishMsg, HandleTournamentFinishClient);
        }

        [DevExtMethods("OnStartServer")]
        protected void OnStartServer_Tournament()
        {
            CancelInvoke(nameof(Update_TournamentIn));
            InvokeRepeating(nameof(Update_TournamentIn), 1, 1);


            float repeat = DateTime.Now.Second;
            repeat = 60 - repeat;
            CancelInvoke(nameof(Update_TournamentOut));
            InvokeRepeating(nameof(Update_TournamentOut), repeat, 60);
        }

        [DevExtMethods("OnPeerConnected")]
        protected void OnPeerConnected_Tournament(long connectionId)
        {
            foreach(var itm in GameInstance.Singleton.Tournaments.Keys)
            {
                if (itm == null)
                    continue;
                SendTournamentReset(connectionId, itm.DataId);
            }
            SendTournamentParticipant(connectionId);

            SendTournamentFinish(connectionId);
        }


        public override void OnPeerDisconnected(long connectionId, DisconnectReason disconnectInfo, SocketError socketError)
        {
            this.InvokeInstanceDevExtMethods("OnPeerDisconnect", connectionId, disconnectInfo, socketError);
            base.OnPeerDisconnected(connectionId, disconnectInfo, socketError);

        }
        [DevExtMethods("OnPeerDisconnect")]
        protected void OnPeerDisconnected_Tournament(long connectionId, DisconnectReason disconnectInfo, SocketError socketError)
        {
            base.OnPeerDisconnected(connectionId, disconnectInfo, socketError);
            if (!(CurrentMapInfo as TournamentMapInfo))
                return;

            TournamentCharacter runner = new TournamentCharacter();
            foreach(TournamentCharacter item in participants.Values.ToList())
            {
                if(item.connectionId == connectionId && item.tournamentIn)
                {
                    runner = item;
                }
            }
            if (string.IsNullOrEmpty(runner.characterId))
                return;

            runner.death = true;
            runner.tournamentIn = false;
            participants[runner.characterId] = runner;

            TournamentCharacter winner;
            winner = participants.Values.Where(x => x.tournamentIn).FirstOrDefault();
            winner.kills++;
            winner.tournamentIn = false;
            participants[winner.characterId] = winner;
            BasePlayerCharacterEntity winnerEntity;
            if(ServerUserHandlers.TryGetPlayerCharacterById(winner.characterId, out winnerEntity))
            {
                winnerEntity.Teleport(TournamentMap.StartPosition, new Quaternion());
            }
            SendMessageGenerallyTournament($"<color=white>{runner.characterName}</color> left the game and winner <color=white>{winner.characterName}</color>");
            SendTournamentParticipant();
            TournamentFightCountDown = 0f;
            SendTournamentFightStatus(winner.connectionId);
        }

        [DevExtMethods("OnServerOnlineSceneLoaded")]
        protected void OnServerOnlineSceneLoaded_Tournament()
        {
            if (!(CurrentMapInfo is TournamentMapInfo))
                return;

            participants = new Dictionary<string, TournamentCharacter>();
            tournamentFinishs = new Dictionary<int, TournamentFinished>();
        }

        [DevExtMethods("Clean")]
        protected void Clean_Tournament()
        {
            CancelInvoke(nameof(Update_TournamentOut));
            CancelInvoke(nameof(Update_TournamentIn));

        }
        public void SendTournamentFinishGlobal()
        {
            if (!IsServer)
                return;

            TournamentFinishMessage mess = new TournamentFinishMessage();
            mess.finishs = tournamentFinishs.Values.ToArray();

            if (MMOServerInstance.Singleton.MapNetworkManager.ClusterClient.IsNetworkActive)
            {
                MMOServerInstance.Singleton.MapNetworkManager.ClusterClient.SendPacket(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.TournamentFinish, (writer) =>
                {
                    writer.PutValue(mess);
                });
            }
        }

        public void SendTournamentGlobalReset(int dataId)
        {
            if (!IsServer)
                return;

            TournamentResetMessage mess = new TournamentResetMessage();
            mess.tournament = dataId;

            if (MMOServerInstance.Singleton.MapNetworkManager.ClusterClient.IsNetworkActive)
            {
                MMOServerInstance.Singleton.MapNetworkManager.ClusterClient.SendPacket(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.TournamentReset, (writer) =>
                {
                    writer.PutValue(mess);
                });
            }
        }
        public void SendTournamentFinish()
        {
            if (!IsServer)
                return;
            foreach(long connectionId in Server.ConnectionIds)
            {
                SendTournamentFinish(connectionId);
            }
        }

        public void SendTournamentFinish(long connectionId)
        {
            if (!IsServer)
                return;
            TournamentFinishMessage message = new TournamentFinishMessage();
            message.finishs = tournamentFinishs.Values.ToArray();
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, tournamentFinishMsg, (writer) =>
            {
                writer.PutValue(message);
            });
        }
        public void SendTournamentReset(int dataId)
        {
            if (!IsServer)
                return;
            foreach(var itm in Server.ConnectionIds)
            {
                SendTournamentReset(itm, dataId);
            }
        }

        public void SendTournamentReset(long connectionId, int dataId)
        {
            if (!IsServer)
                return;

            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, tournamentResetMsg, (writer) =>
            {
                writer.PutPackedInt(dataId);
            });
        }
        public void SendTournamentParticipant()
        {
            if (!IsServer)
                return;

            foreach (long connectionId in Server.ConnectionIds)
            {
                SendTournamentParticipant(connectionId);
            }
        }

        public void SendTournamentParticipant(long connectionId)
        {
            if (!IsServer)
                return;

            TournamentStatusMessage mess = new TournamentStatusMessage();
            mess.participants = participants.Values.ToArray();

            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, tournamentParticipantMsg, (writer) =>
            {
                writer.PutValue(mess);
            });
        }

        public void SendTournamentFightStatus(long connectionId)
        {
            if (!IsServer)
                return;

            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, tournamentFightMsg, (writer) =>
            {
                writer.Put(TournamentFightCountDown);
            });
        }

        private void HandleTournamentResetClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;

            int tournament = messageHandler.Reader.GetPackedInt();

            participants = new Dictionary<string, TournamentCharacter>();

            if(tournamentFinishs.ContainsKey(tournament))
            tournamentFinishs.Remove(tournament);

            foreach (var itm in GameInstance.Singleton.Tournaments.Keys)
            {
                if (itm == null)
                    continue;
                if (itm.DataId == tournament)
                    itm.finished = false;
            }
        }
        private void HandleTournamentFinishClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;

            TournamentFinishMessage mess = messageHandler.ReadMessage<TournamentFinishMessage>();

            foreach (var item in mess.finishs)
            {
                foreach(var itm in GameInstance.Singleton.Tournaments.Keys)
                {
                    if (itm == null)
                        continue;
                    if (itm.DataId == item.dataId)
                        itm.finished = item.finish;
                }
            }
        }

        private void HandleTournamentParticipantClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;

            TournamentStatusMessage mess = messageHandler.ReadMessage<TournamentStatusMessage>();
            foreach(var item in mess.participants)
            {
                participants[item.characterId] = item;
            }
        }

        private void HandleTournamentFightAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
                return;
            TournamentFightCountDown = messageHandler.Reader.GetFloat();       
        }

        public void Update_TournamentIn()
        {
            if (!IsServer || CurrentMapInfo == null || !(CurrentMapInfo as TournamentMapInfo))
                return;

            if (TournamentFightCountDown > 0)
                TournamentFightCountDown -= 1f;

            var fighting = GetTournamentFighting();
            
            foreach(var fight in fighting)
            {
                SendTournamentFightStatus(fight.connectionId);
            }
        }

        public void Update_TournamentOut()
        {
            if (!IsServer || CurrentMapInfo == null)
                return;

            var Tournaments = CurrentGameInstance.Tournaments;
            DateTime currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            foreach (var evnt in Tournaments.Keys)
            {
                DateTime tournamentTime = new DateTime(Tournaments[evnt].Year, Tournaments[evnt].Month, Tournaments[evnt].Day, Tournaments[evnt].Hour, Tournaments[evnt].Minute, 0);

                if (DateTime.Compare(currentDate, tournamentTime) == 0)
                {
                    SendTournamentGlobalReset(evnt.DataId);
                    foreach(BasePlayerCharacterEntity player in ServerUserHandlers.GetPlayerCharacters())
                    {
                        SendMessageTournament(player.ConnectionId, string.Format(evnt.eventStartedMessage, evnt.eventTitle));
                    }
                }

                if (evnt.alerTimes != null && evnt.alerTimes.Length > 0)
                {
                    foreach (int time in evnt.alerTimes)
                    {
                        if (currentDate == tournamentTime.AddMinutes(-time))
                        {
                            foreach (BasePlayerCharacterEntity player in ServerUserHandlers.GetPlayerCharacters())
                            {
                                SendMessageTournament(player.ConnectionId, string.Format(evnt.eventAlertMessage, evnt.eventTitle, time));
                            }
                            break;
                        }
                    }
                }
            }
        }

        private TournamentCharacter GetTournamentWinner()
        {
            List<TournamentCharacter> ranks = participants.Values.ToList();
            var sortDict = ranks.OrderByDescending(x=> !x.death);
            TournamentCharacter winner = sortDict.ToList().FirstOrDefault();
            return winner;
        }

        public List<TournamentCharacter> GetTournamentRanks()
        {
            List<TournamentCharacter> ranks = participants.Values.ToList();
            var sortDict = ranks.OrderByDescending(x => !x.death);
            return sortDict.ToList();
        }

        private List<TournamentCharacter> GetTournamentKillRanks()
        {
            List<TournamentCharacter> ranks = participants.Values.ToList();
            var sortDict = ranks.OrderByDescending(x => x.kills);
            return sortDict.ToList();
        }

        private List<TournamentCharacter> ServerTournamentPlaces()
        {
            List<TournamentCharacter> list = new List<TournamentCharacter>();
            foreach(TournamentCharacter item in participants.Values.ToList())
            {
                if (!item.death && CheckTournamentPPOnline(item.characterId))
                    list.Add(item);
            }
            return list;
        }

        private bool CheckTournamentPPOnline(string characterId)
        {
            if (ServerUserHandlers.TryGetPlayerCharacterById(characterId, out _))
                return true;
            return false;
        }

        private List<TournamentCharacter> GetTournamentFighting()
        {
            List<TournamentCharacter> fighting = participants.Values.ToList();
            for (int i = 0; i < fighting.Count; i++)
            {
                if (!fighting[i].tournamentIn)
                    fighting.RemoveAt(i);
            }
            return fighting;
        }

        public void TournamentFinish(long connectionId)
        {
            BasePlayerCharacterEntity characterEntity;
            if(ServerUserHandlers.TryGetPlayerCharacter(connectionId, out characterEntity))
            {
                if (characterEntity.UserLevel > 0)
                {
                    TournamentCharacter winner = GetTournamentWinner();
                    TournamentWinner winReward = TournamentMap.winnerReward;
                    SetWinnerRewardTournament(winner, winReward);

                    for (int i = 0; i < TournamentMap.killRewards.Length; i++)
                    {
                        TournamentReward reward = TournamentMap.killRewards[i];
                        List<TournamentCharacter> ranks = GetTournamentKillRanks();

                        int rank = 0;
                        foreach (TournamentCharacter character in ranks.ToList())
                        {
                            rank++;
                            if (rank >= reward.minRank && rank <= reward.maxRank)
                            {
                                SetKillRewardTournament(character, reward, rank);
                            }
                        }
                    }
                    BackendFinishTournament();
                }
                else
                {
                    SendMessageTournament(connectionId, "not allowed");
                }
            }
        }

        private void BackendFinishTournament()
        {
            foreach (IPlayerCharacterData player in ServerUserHandlers.GetPlayerCharacters())
            {
                BasePlayerCharacterEntity characterEntity = player as BasePlayerCharacterEntity;
                SendMessageTournament(characterEntity.ConnectionId, "tournament finish return to warping town");
                WarpCharacter(WarpPortalType.Default, characterEntity, characterEntity.RespawnMapName, characterEntity.RespawnPosition, false, Vector3.zero);
            }

            TournamentFinished finished = TournamentFinished.Create(TournamentMap.DataId, true);
            tournamentFinishs[finished.dataId] = finished;
            SendTournamentFinishGlobal();
            SendMessageGenerallyTournament(string.Format(TournamentMap.eventEndedMessage, TournamentMap.eventTitle));
        }

        public void SetupPlayersTournament(long connectionId)
        {
            string message = "";
            List<TournamentCharacter> places = ServerTournamentPlaces();
            List<string> placesName = new List<string>();
            if(places.Count == 1)
            {
                message = "not start only 1 participant";
                SendMessageTournament(connectionId, message);
            }
            else if(participants.Count < TournamentMap.registerLimit)
            {
	            message = "not enough participants count";
	            SendMessageTournament(connectionId, message);
            }
            else if (places.Count > 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    int random = UnityEngine.Random.Range(0, places.Count);
                    var characters = places;

                    TournamentCharacter character = characters[random];
                    places.Remove(character);
                    BasePlayerCharacterEntity playerEntity;
                    if(ServerUserHandlers.TryGetPlayerCharacterById(character.characterId, out playerEntity))
                    {
                    }

                    character.tournamentIn = true;
                    participants[character.characterId] = character;
                    playerEntity.CurrentHp = playerEntity.MaxHp;
                    playerEntity.CurrentMp = playerEntity.MaxMp;
                    playerEntity.CurrentStamina = playerEntity.CurrentStamina;
                    TournamentFightCountDown = TournamentMap.readyDuration;

                    playerEntity.TournamentTargetClear();

                    Quaternion rotation = TournamentMap.positions[i].rotation;
                    playerEntity.Teleport(TournamentMap.positions[i].position, rotation);

                    SendTournamentFightStatus(playerEntity.ConnectionId);
                    SendTournamentParticipant();

                    placesName.Add(playerEntity.CharacterName);
                }
                string plc = $"<color=white>{placesName[0]}</color> vs <color=White>{placesName[1]}</color>";
                SendMessageGenerallyTournament(plc);
            }
            else
            {
                message = "not enough player count";
                SendMessageTournament(connectionId, message);
            }
        }

        public bool CheckYourRegisterTournament(string id)
        {
            if (participants.ContainsKey(id))
                return true;

            return false;
        }

        public bool TournamentFightReady()
        {
            if (TournamentFightCountDown <= 0f)
                return true;
            return false;
        }

        public bool RegisterTournamentFull()
        {
            if (RegisteredCount >= TournamentMap.registerLimit)
                return true;
            return false;
        }

        public void RegisterTournament(BasePlayerCharacterEntity participant)
        {
            if(!participants.ContainsKey(participant.Id))
            {
                TournamentCharacter character = TournamentCharacter.Create(participant, participant.ConnectionId, 0, false, participant.Level, participant.DataId);
                participants[character.characterId] =  character;
            }
            SendTournamentParticipant();
        }

        public void ResetTournament()
        {
            participants.Clear();
            SendTournamentParticipant();
            SendTournamentGlobalReset(TournamentMap.DataId);
        }

        public void LosePlayerTournament(BasePlayerCharacterEntity loser, BasePlayerCharacterEntity attacker)
        {
            TournamentCharacter lose;
            TournamentCharacter winner;

            if (participants.TryGetValue(loser.Id, out lose))
            {
                lose.death = true;
                lose.tournamentIn = false;
                participants[loser.Id] = lose;
            }

            if (participants.TryGetValue(attacker.Id, out winner))
            {
                winner.kills++;
                winner.tournamentIn = false;
                participants[attacker.Id] = winner;
            }

            Quaternion rotation = new Quaternion(0, TournamentMap.StartRotation.y, 0, 0);
            loser.Teleport(TournamentMap.StartPosition, rotation);
            attacker.Teleport(TournamentMap.StartPosition, rotation);

            loser.CurrentHp = loser.MaxHp;
            loser.CurrentMp = loser.MaxMp;
            attacker.CurrentHp = attacker.MaxHp;
            attacker.CurrentMp = attacker.MaxMp;

            string message = $"<color=white>{winner.characterName}</color> killed <color=White>{lose.characterName}</color>";

            SendMessageGenerallyTournament(message);
            SendTournamentParticipant();
        }

        public int LastPlayersTournament()
        {
            List<TournamentCharacter> list = new List<TournamentCharacter>();
            foreach(var item in participants.Values.ToList())
            {
                if (!item.death)
                    list.Add(item);
            }

            return list.Count;
        }
        private void SetKillRewardTournament(TournamentCharacter winner, TournamentReward reward,  int rank)
        {
            Mail tempMail;
            tempMail = new Mail()
            {
                SenderId = "Tournament",
                SenderName = "Tournament Manager",
                ReceiverId = winner.userId,
                Title =  "#" + rank  + " "  + TournamentMap.killRewardMailTitle,
                Content = TournamentMap.killRewardMailContent,
                Gold = reward.winRewardGold,
            };

            foreach (CurrencyAmount currencyAmount in reward.winRewardCurrencies)
            {
                if (currencyAmount.currency == null) continue;
                tempMail.Currencies.Add(CharacterCurrency.Create(currencyAmount.currency.DataId, currencyAmount.amount));
            }
            foreach (ItemAmount itemAmount in reward.winRewardItems)
            {
                if (itemAmount.item == null) continue;
                tempMail.Items.Add(CharacterItem.Create(itemAmount.item, 1, itemAmount.amount));
            }
            ServerMailHandlers.SendMail(tempMail);
        }

        private void SetWinnerRewardTournament(TournamentCharacter winner, TournamentWinner reward)
        {
            Mail tempMail;
            tempMail = new Mail()
            {
                SenderId = "Tournament",
                SenderName = "Tournament Manager",
                ReceiverId = winner.userId,
                Title = TournamentMap.winMailTitle,
                Content = TournamentMap.winMailContent,
                Gold = reward.winRewardGold,
            };

            foreach (CurrencyAmount currencyAmount in reward.winRewardCurrencies)
            {
                if (currencyAmount.currency == null) continue;
                tempMail.Currencies.Add(CharacterCurrency.Create(currencyAmount.currency.DataId, currencyAmount.amount));
            }
            foreach (ItemAmount itemAmount in reward.winRewardItems)
            {
                if (itemAmount.item == null) continue;
                tempMail.Items.Add(CharacterItem.Create(itemAmount.item, 1, itemAmount.amount));
            }
            ServerMailHandlers.SendMail(tempMail);
        }

        public bool CheckTournamentFighting(string id)
        {
            TournamentCharacter character;
            if (participants.TryGetValue(id, out character))
            {
                return character.tournamentIn;
            }
            return false;
        }

        private void SendGlobalAnnounceTournament(string message)
        {
#if UNITY_EDITOR || UNITY_SERVER
            ChatMessage chat = new ChatMessage()
            {
                channel = ChatChannel.System,
                senderName = CHAT_SYSTEM_ANNOUNCER_SENDER,
                message = message,
                sendByServer = true,
            };

            if (MapNetworkTournament.ClusterClient.IsNetworkActive)
            {
                MapNetworkTournament.ClusterClient.SendPacket(0, DeliveryMethod.ReliableOrdered, MMOMessageTypes.Chat, (writer) =>
                {
                    writer.PutValue(chat);
                    writer.Put("");
                    writer.Put("");
                });
            }
#endif
        }

        private void SendMessageTournament(long connectionId, string message)
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

        public void SendMessageGenerallyTournament(string message)
        {
            foreach (long connectionId in Server.ConnectionIds)
            {
                SendMessageTournament(connectionId, message);
            }
        }
    }
}
