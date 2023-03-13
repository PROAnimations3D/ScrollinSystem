using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [DevExtMethods("Awake")]
        protected void Awake_KillNotify()
        {
            onReceivedDamage += OnReceivedDamage_KillNotify;
        }

        [DevExtMethods("OnDestroy")]
        protected void OnDestroy_KillNotify()
        {
            onReceivedDamage -= OnReceivedDamage_KillNotify;
        }

        private void OnReceivedDamage_KillNotify(HitBoxPosition position,
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
            if (!(CurrentMapInfo is MatchEventMapInfo))
                return;

            if (!this.IsDead())
                return;

            // Will notify only when character killed by player's character
            if (attacker != null)
            {
                BasePlayerCharacterEntity playerAttacker = null;
                BaseMonsterCharacterEntity monsterCharacterEntity;
                // Notify
                if (attacker.Entity is BasePlayerCharacterEntity)
                playerAttacker = attacker.Entity as BasePlayerCharacterEntity;

                if (attacker.Entity is BaseMonsterCharacterEntity)
                {
                    monsterCharacterEntity = attacker.Entity as BaseMonsterCharacterEntity;
                    if(monsterCharacterEntity.IsSummoned)
                    playerAttacker = monsterCharacterEntity.Summoner as BasePlayerCharacterEntity;
                }

                var weaponId = weapon != null ? weapon.dataId : 0;
                var skillId = skill != null ? skill.DataId : 0;

                //SendUpdateKillsToServer(playerAttacker.ConnectionId);

                if(playerAttacker != null)
                CurrentGameManager.SendKillNotify(playerAttacker.CharacterName, playerAttacker.TeamData.id, CharacterName, weaponId, skillId, skillLevel);
            }
        }
    }
}
