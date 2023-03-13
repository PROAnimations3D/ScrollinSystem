using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Register Dialog", menuName = "Tournament/Register Dialog", order = -5571)]
    public partial class TournamentRegisterDialog : BaseNpcDialog
    {
        public const int CONFIRM_MENU_INDEX = 0;
        public const int CANCEL_MENU_INDEX = 1;

        public TournamentMapInfo mapInfo;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog cancelDialog;

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
                    if (!mapInfo.IsOn)
                    {
                        GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(characterEntity.Id, UITextKeys.UI_ERROR_NOT_START_TOURNAMENT);
                        return;
                    }
                    else
                    {
                        GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(characterEntity.Id, UITextKeys.UI_SUCCES_REGISTER_TOURNAMENT);
                        BaseGameNetworkManager.Singleton.RegisterTournament(characterEntity);
                    }
                    break;
                case CANCEL_MENU_INDEX:
                    characterEntity.NpcAction.CurrentNpcDialog = GetValidatedDialogOrNull(cancelDialog, characterEntity);
                    break;
            }

            if (nextDialog == null || !nextDialog.ValidateDialog(characterEntity))
                return;

            return;
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

            if (owningCharacter.Level < mapInfo.minLevel)
            {
                uiNpcDialog.uiTextDescription.text = "not enough level";
            }
            else if(owningCharacter.Level > mapInfo.maxLevel)
            {
                uiNpcDialog.uiTextDescription.text = "you high level";
            }
            else if(BaseGameNetworkManager.Singleton.CheckYourRegisterTournament(owningCharacter.Id))
            {
                uiNpcDialog.uiTextDescription.text = "already register";
            }
            else if(BaseGameNetworkManager.Singleton.RegisterTournamentFull())
            {
                uiNpcDialog.uiTextDescription.text = "limit is reached";
            }
            else
            {
                uiNpcDialog.uiTextDescription.text = "you can register";

                menuActions.Add(confirmMenuAction);
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
