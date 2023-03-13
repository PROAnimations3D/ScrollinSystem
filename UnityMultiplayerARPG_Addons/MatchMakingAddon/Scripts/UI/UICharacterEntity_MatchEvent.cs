using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UICharacterEntity_MatchEvent : UICharacterEntity
    {
        protected override void UpdateUI()
        {
            if (!ValidateToUpdateUI())
            {
                CacheCanvas.enabled = false;
                return;
            }
            base.UpdateUI();

            // Update character buffs every `updateUIRepeatRate` seconds
            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data);

            if (GameInstance.PlayingCharacterEntity != Data && BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo)
            {
                EntityInfo instigator = Data.GetInfo();
                if (!GameInstance.PlayingCharacterEntity.IsAlly(instigator))
                {
                    uiTextTitle.color = Color.red;
                    CacheCanvas.enabled = Vector3.Distance(GameInstance.PlayingCharacterEntity.EntityTransform.position, Data.EntityTransform.position) <= visibleDistance;
                }else
                    uiTextTitle.color = Color.white;
            }
        }
    }
}
