using LiteNetLibManager;
using UnityEngine;
namespace MultiplayerARPG
{
    public class DungeonMonsterEntity : MonsterCharacterEntity
    {
        [Category("Dungeon")]
        public string doorId;
        public override void Killed(EntityInfo lastAttacker)
        {
            base.Killed(lastAttacker);

            foreach (LiteNetLibIdentity identity in BaseGameNetworkManager.Singleton.Assets.GetSceneObjects())
            {
                DungeonDoorEntity doorEntity = identity.GetComponent<DungeonDoorEntity>();
                if (doorEntity == null)
                    continue;

                if (doorEntity.doorId == doorId)
                {
                    doorEntity.CallServerDungeonMonsterDoor(DataId);
                }
            }

        }
    }
}
