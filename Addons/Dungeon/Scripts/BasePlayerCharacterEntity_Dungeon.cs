using System.Collections;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [Category("Dungeon Addon")]
        [HideInInspector]
        public string dungeonStatus;
        [SerializeField]
        protected SyncListCharacterDungeon dungeons = new SyncListCharacterDungeon();
        public IList<PlayerDungeon> Dungeons
        {
            get { return dungeons; }
            set
            {
                dungeons.Clear();
                dungeons.AddRange(value);
            }
        }

        public void CallServerDungeonStatus(string doorId)
        {
            RPC(ServerDungeonStatus, doorId);
        }

        [AllRpc]
        private void ServerDungeonStatus(string doorId)
        {
            dungeonStatus = doorId;
        }

        public void CallServerDungeonFinalTrigger()
        {
            RPC(ServerDungeonFinalTrigger);
        }

        [AllRpc]
        private void ServerDungeonFinalTrigger()
        {
            CurrentGameManager.DungeonTrigger();
        }
    }
}
