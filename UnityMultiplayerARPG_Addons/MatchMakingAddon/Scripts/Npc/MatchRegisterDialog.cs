using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Match Register Dialog", menuName = "Create GameData/CalleGaming/Event/Match Register Dialog", order = -4798)]
    public partial class MatchRegisterDialog : BaseNpcDialog
    {
        public const int CONFIRM_MENU_INDEX = 0;
        public const int CANCEL_MENU_INDEX = 1;

        public MatchEvents matchEvent;


        ///Any event
        public bool WarpOnRegister;
        public WarpPortalType warpPortalType;
        public BaseMapInfo warpMap;
        public Vector3 warpPosition;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog warpCancelDialog;

        public override bool IsShop
        {
            get { return false; }
        }

        public override void GoToNextDialog(BasePlayerCharacterEntity characterEntity, byte menuIndex)
        {
            /// This dialog is current NPC dialog
            BaseNpcDialog nextDialog = null;

            switch (menuIndex)
            {
                case CONFIRM_MENU_INDEX:
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                    {
                        if (matchEvent == matchEvents && !matchEvent.Consistent)
                            if (matchEvents.starting)
                            {
                                BaseGameNetworkManager.Singleton.WarpCharacter(warpPortalType, characterEntity, warpMap.Id, warpPosition, false, Vector3.zero);
                            }
                    }

                    foreach (MatchEvents matchEvents in GameInstance.Singleton.RunningEvents)
                    {
                        if (matchEvent == matchEvents && matchEvent.Consistent)
                            BaseGameNetworkManager.Singleton.WarpCharacter(warpPortalType, characterEntity, warpMap.Id, warpPosition, false, Vector3.zero);
                    }

                    GameInstance.ServerGameMessageHandlers.SendGameMessage(GameInstance.PlayingCharacterEntity.ConnectionId, UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE);
                    return;
                case CANCEL_MENU_INDEX:
                    nextDialog = warpCancelDialog;
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
            menuActions.Add(confirmMenuAction);
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

            BaseNpcDialog dialog = null;
            if (to != null && to.node != null)
                dialog = to.node as BaseNpcDialog;
        }

    }
}
