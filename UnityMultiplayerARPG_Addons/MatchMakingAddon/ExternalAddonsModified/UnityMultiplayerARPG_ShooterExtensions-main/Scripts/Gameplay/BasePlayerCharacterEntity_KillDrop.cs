using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [DevExtMethods("Awake")]
        protected void Awake_KillDrop()
        {
            onReceivedDamage += OnReceivedDamage_KillDrop;
        }

        [DevExtMethods("OnDestroy")]
        protected void OnDestroy_KillDrop()
        {
            onReceivedDamage -= OnReceivedDamage_KillDrop;
        }

        private void OnReceivedDamage_KillDrop(HitBoxPosition position,
            Vector3 fromPosition,
            IGameEntity attacker,
            CombatAmountType combatAmountType,
            int totalDamage,
            CharacterItem weapon,
            BaseSkill skill,
            int skillLevel,
            CharacterBuff buff,
            bool isDamageOverTime)
        {
         //This is build into the kit now, leaving it for cuase of error if dont delete script from project.
        }
    }
}
