using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [DevExtMethods("Awake")]
        protected void Awake_KillReward()
        {
            onReceivedDamage += OnReceivedDamage_KillReward;
        }

        [DevExtMethods("OnDestroy")]
        protected void OnDestroy_KillReward()
        {
            onReceivedDamage -= OnReceivedDamage_KillReward;
        }

        private void OnReceivedDamage_KillReward(HitBoxPosition position,
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
            if (!this.IsDead() || !(CurrentMapInfo as MatchEventMapInfo))
                return;

            MatchEventMapInfo matchEventMapInfo = CurrentMapInfo as MatchEventMapInfo;

            // Will reward only when character killed by player's character
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
                    if (monsterCharacterEntity.IsSummoned)
                        playerAttacker = monsterCharacterEntity.Summoner as BasePlayerCharacterEntity;
                }
                MatchEvents match =null;

                foreach (MatchEvents matchEvents in CurrentGameInstance.MatchEvents)
                    if (matchEvents == matchEventMapInfo.matchEvent)
                        match = matchEvents;

                if (match != null && playerAttacker !=null)
                {
                    playerAttacker.RewardExp(match.rewardsPerKill, 1f, RewardGivenType.None);
                    playerAttacker.RewardCurrencies(match.rewardsPerKill, 1f, RewardGivenType.None);
                }

            }
        }
    }
}
