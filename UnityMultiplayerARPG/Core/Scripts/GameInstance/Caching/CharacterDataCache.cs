﻿using System.Collections.Generic;

namespace MultiplayerARPG
{
    public sealed class CharacterDataCache
    {
        public bool IsRecaching { get; private set; }
        private CharacterStats stats;
        public CharacterStats Stats => stats;
        public Dictionary<Attribute, float> Attributes { get; }
        public Dictionary<BaseSkill, int> Skills { get; }
        public Dictionary<DamageElement, float> Resistances { get; }
        public Dictionary<DamageElement, float> Armors { get; }
        public Dictionary<DamageElement, MinMaxFloat> IncreaseDamages { get; }
        public Dictionary<EquipmentSet, int> EquipmentSets { get; }
        public int MaxHp => (int)stats.hp;
        public int MaxMp => (int)stats.mp;
        public int MaxStamina => (int)stats.stamina;
        public int MaxFood => (int)stats.food;
        public int MaxWater => (int)stats.water;
        public float AtkSpeed => stats.atkSpeed;
        public float MoveSpeed => stats.moveSpeed;
        public float BaseMoveSpeed { get; private set; }
        public float TotalItemWeight { get; private set; }
        public int TotalItemSlot { get; private set; }
        public float LimitItemWeight { get; private set; }
        public int LimitItemSlot { get; private set; }
        public bool DisallowMove { get; private set; }
        public bool DisallowAttack { get; private set; }
        public bool DisallowUseSkill { get; private set; }
        public bool DisallowUseItem { get; private set; }
        public bool FreezeAnimation { get; private set; }
        public bool IsHide { get; private set; }
        public bool MuteFootstepSound { get; private set; }
        public bool IsOverweight { get; private set; }
        public bool HavingChanceToRemoveBuffWhenAttack { get; private set; }
        public bool HavingChanceToRemoveBuffWhenAttacked { get; private set; }
        public bool HavingChanceToRemoveBuffWhenUseSkill { get; private set; }
        public bool HavingChanceToRemoveBuffWhenUseItem { get; private set; }
        public bool HavingChanceToRemoveBuffWhenPickupItem { get; private set; }

        public CharacterDataCache()
        {
            Attributes = new Dictionary<Attribute, float>();
            Resistances = new Dictionary<DamageElement, float>();
            Armors = new Dictionary<DamageElement, float>();
            IncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
            Skills = new Dictionary<BaseSkill, int>();
            EquipmentSets = new Dictionary<EquipmentSet, int>();
        }

        public CharacterDataCache MarkToMakeCaches()
        {
            IsRecaching = true;
            return this;
        }

        public CharacterDataCache MakeCache(ICharacterData characterData)
        {
            // Don't make cache if not needed
            if (!IsRecaching)
                return this;

            IsRecaching = false;

            characterData.GetAllStats(
                ref stats,
                Attributes,
                Resistances,
                Armors,
                IncreaseDamages,
                Skills,
                EquipmentSets,
                false);

            if (characterData.GetDatabase() != null)
                BaseMoveSpeed = characterData.GetDatabase().Stats.baseStats.moveSpeed;

            TotalItemWeight = GameInstance.Singleton.GameplayRule.GetTotalWeight(characterData, stats);
            TotalItemSlot = GameInstance.Singleton.GameplayRule.GetTotalSlot(characterData, stats);
            LimitItemWeight = GameInstance.Singleton.GameplayRule.GetLimitWeight(characterData, stats);
            LimitItemSlot = GameInstance.Singleton.GameplayRule.GetLimitSlot(characterData, stats);

            IsOverweight = (GameInstance.Singleton.IsLimitInventorySlot && TotalItemSlot > LimitItemSlot) || (GameInstance.Singleton.IsLimitInventoryWeight && TotalItemWeight > LimitItemWeight);
            DisallowMove = false;
            DisallowAttack = false;
            DisallowUseSkill = false;
            DisallowUseItem = false;
            FreezeAnimation = false;
            IsHide = false;
            MuteFootstepSound = false;
            HavingChanceToRemoveBuffWhenAttack = false;
            HavingChanceToRemoveBuffWhenAttacked = false;
            HavingChanceToRemoveBuffWhenUseSkill = false;
            HavingChanceToRemoveBuffWhenUseItem = false;
            HavingChanceToRemoveBuffWhenPickupItem = false;

            bool allAilmentsWereApplied = false;
            if (characterData.PassengingVehicleEntity != null)
            {
                UpdateAppliedAilments(characterData.PassengingVehicleEntity.GetBuff());
                allAilmentsWereApplied = AllAilmentsWereApplied();
            }

            if (!allAilmentsWereApplied)
            {
                foreach (CharacterBuff characterBuff in characterData.Buffs)
                {
                    UpdateAppliedAilments(characterBuff.GetBuff());
                    allAilmentsWereApplied = AllAilmentsWereApplied();
                    if (allAilmentsWereApplied)
                        break;
                }
            }

            if (!allAilmentsWereApplied)
            {
                foreach (CharacterSummon characterBuff in characterData.Summons)
                {
                    UpdateAppliedAilments(characterBuff.GetBuff());
                    allAilmentsWereApplied = AllAilmentsWereApplied();
                    if (allAilmentsWereApplied)
                        break;
                }
            }

            if (!allAilmentsWereApplied)
            {
                foreach (BaseSkill tempSkill in Skills.Keys)
                {
                    if (tempSkill == null || tempSkill.IsActive || !tempSkill.IsBuff)
                        continue;
                    UpdateAppliedAilments(new CalculatedBuff(tempSkill.Buff, Skills[tempSkill]));
                    allAilmentsWereApplied = AllAilmentsWereApplied();
                    if (allAilmentsWereApplied)
                        break;
                }
            }

            return this;
        }

        public void ClearChanceToRemoveBuffWhenAttack()
        {
            HavingChanceToRemoveBuffWhenAttack = false;
        }

        public void ClearChanceToRemoveBuffWhenAttacked()
        {
            HavingChanceToRemoveBuffWhenAttacked = false;
        }

        public void ClearChanceToRemoveBuffWhenUseSkill()
        {
            HavingChanceToRemoveBuffWhenUseSkill = false;
        }

        public void ClearChanceToRemoveBuffWhenUseItem()
        {
            HavingChanceToRemoveBuffWhenUseItem = false;
        }

        public void ClearChanceToRemoveBuffWhenPickupItem()
        {
            HavingChanceToRemoveBuffWhenPickupItem = false;
        }

        #region Helper functions to get stats amount
        public float GetAttribute(string nameId)
        {
            return GetAttribute(nameId.GenerateHashId());
        }

        public float GetAttribute(int dataId)
        {
            Attribute data;
            float result;
            if (GameInstance.Attributes.TryGetValue(dataId, out data) &&
                Attributes.TryGetValue(data, out result))
                return result;
            return 0f;
        }

        public int GetSkill(string nameId)
        {
            return GetSkill(nameId.GenerateHashId());
        }

        public int GetSkill(int dataId)
        {
            BaseSkill data;
            int result;
            if (GameInstance.Skills.TryGetValue(dataId, out data) &&
                Skills.TryGetValue(data, out result))
                return result;
            return 0;
        }

        public float GetResistance(string nameId)
        {
            return GetResistance(nameId.GenerateHashId());
        }

        public float GetResistance(int dataId)
        {
            DamageElement data;
            float result;
            if (GameInstance.DamageElements.TryGetValue(dataId, out data) &&
                Resistances.TryGetValue(data, out result))
                return result;
            return 0f;
        }

        public float GetArmor(string nameId)
        {
            return GetArmor(nameId.GenerateHashId());
        }

        public float GetArmor(int dataId)
        {
            DamageElement data;
            float result;
            if (GameInstance.DamageElements.TryGetValue(dataId, out data) &&
                Armors.TryGetValue(data, out result))
                return result;
            return 0f;
        }

        public MinMaxFloat GetIncreaseDamage(string nameId)
        {
            return GetIncreaseDamage(nameId.GenerateHashId());
        }

        public MinMaxFloat GetIncreaseDamage(int dataId)
        {
            DamageElement data;
            MinMaxFloat result;
            if (GameInstance.DamageElements.TryGetValue(dataId, out data) &&
                IncreaseDamages.TryGetValue(data, out result))
                return result;
            return default(MinMaxFloat);
        }

        public int GetEquipmentSet(string nameId)
        {
            return GetEquipmentSet(nameId.GenerateHashId());
        }

        public int GetEquipmentSet(int dataId)
        {
            EquipmentSet data;
            int result;
            if (GameInstance.EquipmentSets.TryGetValue(dataId, out data) &&
                EquipmentSets.TryGetValue(data, out result))
                return result;
            return 0;
        }

        public void UpdateAppliedAilments(CalculatedBuff buff)
        {
            Buff tempBuff = buff.GetBuff();
            switch (tempBuff.ailment)
            {
                case AilmentPresets.Stun:
                    DisallowMove = true;
                    DisallowAttack = true;
                    DisallowUseSkill = true;
                    DisallowUseItem = true;
                    break;
                case AilmentPresets.Mute:
                    DisallowUseSkill = true;
                    break;
                case AilmentPresets.Freeze:
                    DisallowMove = true;
                    DisallowAttack = true;
                    DisallowUseSkill = true;
                    DisallowUseItem = true;
                    FreezeAnimation = true;
                    break;
                default:
                    if (tempBuff.disallowMove)
                        DisallowMove = true;
                    if (tempBuff.disallowAttack)
                        DisallowAttack = true;
                    if (tempBuff.disallowUseSkill)
                        DisallowUseSkill = true;
                    if (tempBuff.disallowUseItem)
                        DisallowUseItem = true;
                    if (tempBuff.freezeAnimation)
                        FreezeAnimation = true;
                    break;
            }
            if (tempBuff.isHide)
                IsHide = true;
            if (tempBuff.muteFootstepSound)
                MuteFootstepSound = true;
            if (buff.GetRemoveBuffWhenAttackChance() > 0f)
                HavingChanceToRemoveBuffWhenAttack = true;
            if (buff.GetRemoveBuffWhenAttackedChance() > 0f)
                HavingChanceToRemoveBuffWhenAttacked = true;
            if (buff.GetRemoveBuffWhenUseSkillChance() > 0f)
                HavingChanceToRemoveBuffWhenUseSkill = true;
            if (buff.GetRemoveBuffWhenUseItemChance() > 0f)
                HavingChanceToRemoveBuffWhenUseItem = true;
            if (buff.GetRemoveBuffWhenPickupItemChance() > 0f)
                HavingChanceToRemoveBuffWhenPickupItem = true;
        }

        public bool AllAilmentsWereApplied()
        {
            return DisallowMove &&
                DisallowAttack &&
                DisallowUseSkill &&
                DisallowUseItem &&
                FreezeAnimation &&
                IsHide &&
                MuteFootstepSound &&
                HavingChanceToRemoveBuffWhenAttack &&
                HavingChanceToRemoveBuffWhenAttacked &&
                HavingChanceToRemoveBuffWhenUseSkill &&
                HavingChanceToRemoveBuffWhenUseItem &&
                HavingChanceToRemoveBuffWhenPickupItem;
        }
        #endregion
    }
}
