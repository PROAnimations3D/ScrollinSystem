using LiteNetLibManager;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MultiplayerARPG
{
    public class DungeonDoorEntity : MonsterCharacterEntity
    {
        [Category("Dungeon")]
        [Header("Camera")]
        public bool isBoosDoor;
        public float cameraDuration;
        [Header("Generally")]
        public string doorId;
        public bool destroyNoLeftEnemies;
        public DungeonMonsterCharacterTarget[] targets;
        private bool finish;
        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.harvestableTag;
            gameObject.layer = CurrentGameInstance.harvestableLayer;
            isDestroyed = false;
            finish = false;
        }

        public void CallServerDungeonMonsterDoor(int dataId)
        {
            RPC(ServerDungeonMonsterDoor, dataId);
        }

        [AllRpc]
        private void ServerDungeonMonsterDoor(int dataId)
        {
            for(int i = 0; i < targets.Length; i++)
            {
                if(targets[i].monster.DataId == dataId)
                {
                    targets[i].currentMonster++;

                    if (targets[i].currentMonster >= targets[i].targetMonster)
                        targets[i].finish = true;
                }
            }
            List<DungeonMonsterCharacterTarget> desc = targets.OrderByDescending(x => x.finish).ToList();
            foreach(var item in desc)
            {
                finish = item.finish;
            }
            if(destroyNoLeftEnemies)
            {
                if(finish)
                {
                    NetworkDestroy();
                }
            }
        }

        public override void Killed(EntityInfo lastAttacker)
        {
            base.Killed(lastAttacker);
            BaseGameNetworkManager.Singleton.DungeonDoorBreak(EntityTitle);
            CallServerDungeonBossCamera();
               
        }

        public void CallServerDungeonBossCamera()
        {
            RPC(ServerDungeonBossCamera);
        }

        [AllRpc]
        private void ServerDungeonBossCamera()
        {
            if(isBoosDoor)
            {
                UIDungeon uiDungeon = FindObjectOfType<UIDungeon>();
                if (uiDungeon != null)
                {
                    uiDungeon.BossFightUI(cameraDuration);
                }
            }
        }

        public override bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            if (!base.CanReceiveDamageFrom(instigator))
                return false;
            return CurrentGameManager.DungeonRunning && finish;
        }
    }

    [System.Serializable]
    public struct DungeonMonsterCharacterTarget
    {
        public MonsterCharacter monster;
        public int targetMonster;
        [HideInInspector]
        public int currentMonster;
        [HideInInspector]
        public bool finish;
    }
}
