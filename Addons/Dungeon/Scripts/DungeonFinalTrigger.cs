using UnityEngine;

namespace MultiplayerARPG
{
    public class DungeonFinalTrigger : MonoBehaviour
    {
        public bool directFinish;
        private bool istrigger;

        private void Awake()
        {
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }
        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        private void TriggerEnter(GameObject other)
        {
            BasePlayerCharacterEntity gameEntity = other.GetComponent<BasePlayerCharacterEntity>();
            if (gameEntity == null || istrigger)
                return;

            istrigger = true;
            if(directFinish)
            gameEntity.CallServerDungeonFinalTrigger();
        }
    }
}
