using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IPlayerCharacterData : ICharacterData
    {
        /// <summary>
        /// Create a teamData for IPlayercharacters.
        /// </summary>
        TeamData TeamData { get; set; }
    }

    public partial class PlayerCharacterData
    {
        /// <summary>
        /// implements Team data from Interface
        /// </summary>
        public TeamData TeamData { get; set; }
    }

    public abstract partial class BasePlayerCharacterEntity
    {
        /// <summary>
        /// Events for Teamid Change.
        /// </summary>
        public event System.Action<TeamData> onTeamIdChange;

        /// <summary>
        /// Team data get /set
        /// </summary>
        public TeamData TeamData { get { return teamData.Value; } set { teamData.Value = value; } }

        /// <summary>
        /// Awake Extentions
        /// </summary>
        /// DevExtMethod that is called when on Baseplayercharacter Awake.
        /// These setups the OnTeamIdChange event.
        [DevExtMethods("Awake")]
        void Setup()
        {
            teamData.onChange += OnTeamIdChange;
        }

        [DevExtMethods("Destroy")]
        void Desetup()
        {
            teamData.onChange -= OnTeamIdChange;
        }

        [SerializeField]
        protected SyncFieldTeamData teamData = new SyncFieldTeamData();

        protected virtual void OnTeamIdChange(bool isInitial, TeamData teamId)
        {
            if (onTeamIdChange != null)
                onTeamIdChange.Invoke(teamId);
        }
        public void OnSetupNetElements_Team()
        {
            teamData.deliveryMethod = DeliveryMethod.ReliableOrdered;
            teamData.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }


        /// <summary>
        /// Client request to server for players list.
        /// </summary>
        public void CallServerPlayersInEvent()
        {
            RPC(ServerGetPlayersInMatch, ConnectionId);
        }

        /// <summary>
        /// RPC Call to return current players in match
        /// </summary>
        /// This will send a message to client with the current players and their score.
        /// <param name="connectionid"></param>
        [ServerRpc]
        protected void ServerGetPlayersInMatch(long connectionid)
        {
            BaseGameNetworkManager.Singleton.SendPlayerListToClient(connectionid);
        }

        /// <summary>
        /// Warp to leave event.
        /// </summary>
        public void CallServerWarpPlayer()
        {
            RPC(ServerWarpRequest, ConnectionId);
        }

        /// <summary>
        /// RPC call to leave event.
        /// </summary>
        /// <param name="connectionid"></param>
        [ServerRpc]
        protected void ServerWarpRequest(long connectionid)
        {
            CurrentGameManager.WarpCharacter(WarpPortalType.Default,
                   this,
                   respawnMapName,
                   respawnPosition,
                   false, Vector3.zero);
        }

    }
}
