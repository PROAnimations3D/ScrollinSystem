using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Finish Dialog", menuName = "Tournament/Finish Dialog", order = -5570)]
    public partial class TournamentFinishDialog : BaseNpcDialog
    {

        public const int CONFIRM_MENU_INDEX = 0;
        public const int CANCEL_MENU_INDEX = 1;

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
                    if(characterEntity.TournamentGM())
                    {
                        GameInstance.ServerGameMessageHandlers.SendGameMessageByCharacterId(characterEntity.Id, UITextKeys.UI_SUCCES_FINISH_TOURNAMENT);
                        BaseGameNetworkManager.Singleton.TournamentFinish(characterEntity.ConnectionId);
                        return;
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

            if (!owningCharacter.TournamentGM())
            {
                uiNpcDialog.uiTextDescription.text = "not allowed";
            }
            else
            {
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
            BaseNpcDialog dialog = null;
            if (to != null && to.node != null)
                dialog = to.node as BaseNpcDialog;
        }
    }
}
