using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Dungeon Dialog", menuName = "Dungeon/Dungeon Dialog", order = -5552)]
    public partial class DungeonNpcDialog : BaseNpcDialog
    {

        public const int CONFIRM_MENU_INDEX = 0;
        public const int CANCEL_MENU_INDEX = 1;

        public DungeonMapInfo mapInfo;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog warpCancelDialog;

        [Category("Dungeon")]
        public string startedEvent;
        [Tooltip("started by language keys")]
        public LanguageData[] startedEvents;

        public string StartedEvent
        {
            get { return Language.GetText(startedEvents, startedEvent); }
        }
        public string notStartEvent;
        [Tooltip("notStarted by language keys")]
        public LanguageData[] notStartEvents;

        public string NotStartEvent
        {
            get { return Language.GetText(notStartEvents, notStartEvent); }
        }

        public override bool IsShop
        {
            get { return false; }
        }

        public override void GoToNextDialog(BasePlayerCharacterEntity characterEntity, byte menuIndex)
        {
            characterEntity.NpcAction.ClearNpcDialogData();
            /// This dialog is current NPC dialog
            BaseNpcDialog nextDialog = null;

            switch (menuIndex)
            {
                case CONFIRM_MENU_INDEX:
                    int limit = mapInfo.limitLogin;
                    int used = TrialDungeon(characterEntity);

                    if(used >= limit)
                    {
                        GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(characterEntity.Id, UITextKeys.UI_ERROR_NOT_ENOUGH_TRIAL_DUNGEON);
                        return;
                        
                    }
                    else if(!mapInfo.IsOn)
                    {
                        GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(characterEntity.Id, UITextKeys.UI_ERROR_NOT_START_EVENT_DUNGEON);
                        return;
                    }
                    else if(BaseGameNetworkManager.Singleton.CheckMapPartyMembersDungeon(characterEntity))
                    {
                        GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(characterEntity.Id, UITextKeys.UI_ERROR_NOT_SAME_PARTY_MAP_DUNGEON);
                        return;
                    }
                    else if(BaseGameNetworkManager.Singleton.CheckPartyLevelDungeon(characterEntity, mapInfo))
                    {
                        return;
                    }
                    else
                    {
                        PlayerDungeon dungeon = PlayerDungeon.Create(characterEntity.Id, mapInfo.DataId);
                        characterEntity.Dungeons.Add(dungeon);
                        BaseGameNetworkManager.Singleton.WarpCharacter(WarpPortalType.EnterInstance, characterEntity, mapInfo.Id, mapInfo.StartPosition, false, Vector3.zero);
                    }
                    break;
                case CANCEL_MENU_INDEX:
                    characterEntity.NpcAction.CurrentNpcDialog = GetValidatedDialogOrNull(warpCancelDialog, characterEntity);
                    break;
            }

            if (nextDialog == null || !nextDialog.ValidateDialog(characterEntity))
                return;

            return;
        }

        public int TrialDungeon(BasePlayerCharacterEntity player)
        {
            int used = 0;
            foreach (PlayerDungeon dungeon in player.Dungeons)
            {
                if (dungeon.DataId == mapInfo.DataId)
                    used++;
            }
            return used;
        }

        public override void RenderUI(UINpcDialog uiNpcDialog)
        {
            BasePlayerCharacterEntity owningCharacter = GameInstance.PlayingCharacterEntity;

            List<UINpcDialogMenuAction> menuActions = new List<UINpcDialogMenuAction>();
            UINpcDialogMenuAction confirmMenuAction;
            UINpcDialogMenuAction cancelMenuAction;

            confirmMenuAction = new UINpcDialogMenuAction();
            cancelMenuAction = new UINpcDialogMenuAction();
            confirmMenuAction.title = uiNpcDialog.messageWarpConfirm;
            confirmMenuAction.menuIndex = CONFIRM_MENU_INDEX;
            cancelMenuAction.title = uiNpcDialog.MessageWarpCancel;
            cancelMenuAction.menuIndex = CANCEL_MENU_INDEX;
            int limit = mapInfo.limitLogin;

            if (mapInfo.IsOn && TrialDungeon(owningCharacter) == limit)
            {
                uiNpcDialog.uiTextDescription.text = "Remaining trial finish";
            }
            else if(mapInfo.IsOn && TrialDungeon(owningCharacter) < limit)
            {
                uiNpcDialog.uiTextDescription.text = StartedEvent + "\n" + "Remaining trial:" + " " + TrialDungeon(owningCharacter) + "/" + limit;

                menuActions.Add(confirmMenuAction);
            }
            else
            {
                uiNpcDialog.uiTextDescription.text = NotStartEvent;
            }
            menuActions.Add(cancelMenuAction);


            // Menu
            if (uiNpcDialog.uiMenuRoot != null)
                uiNpcDialog.uiMenuRoot.SetActive(menuActions.Count > 0);
            UINpcDialogMenu tempUiNpcDialogMenu;
            uiNpcDialog.CacheMenuList.Generate(menuActions, (index, menuAction, ui) =>
            {
                tempUiNpcDialogMenu = ui.GetComponent<UINpcDialogMenu>();
                tempUiNpcDialogMenu.Data = menuAction;
                tempUiNpcDialogMenu.uiNpcDialog = uiNpcDialog;
                tempUiNpcDialogMenu.Show();
            });
        }

        public override void UnrenderUI(UINpcDialog uiNpcDialog)
        {
            if (uiNpcDialog.uiMenuRoot != null)
                uiNpcDialog.uiMenuRoot.SetActive(false);

            if (uiNpcDialog.uiSellItemRoot != null)
                uiNpcDialog.uiSellItemRoot.SetActive(false);

            if (uiNpcDialog.uiSellItemDialog != null)
                uiNpcDialog.uiSellItemDialog.Hide();

            if (uiNpcDialog.uiCharacterQuest != null)
                uiNpcDialog.uiCharacterQuest.Hide();

            if (uiNpcDialog.uiCraftItem != null)
                uiNpcDialog.uiCraftItem.Hide();
        }

        public override bool ValidateDialog(BasePlayerCharacterEntity characterEntity)
        {
            return true;
        }

        protected override void SetDialogByPort(NodePort from, NodePort to)
        {
            if (from.node != this)
                return;
        }
    }
}
