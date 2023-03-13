using UnityEngine;

namespace MultiplayerARPG
{
    public class DungeonStatusTrigger : MonoBehaviour
    {
        public string doorId;

        private void Awake()
        {
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }
        private void OnTriggerStay(Collider other)
        {
            TriggerStay(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TriggerStay(other.gameObject);
        }

        private void TriggerStay(GameObject other)
        {
            BasePlayerCharacterEntity gameEntity = other.GetComponent<BasePlayerCharacterEntity>();
            if (gameEntity == null)
                return;

            gameEntity.CallServerDungeonStatus(doorId);
        }
    }
}
