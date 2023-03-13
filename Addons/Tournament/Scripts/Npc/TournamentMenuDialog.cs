using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using XNode;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Tournament Npc Menu", menuName = "Tournament/Npc Menu", order = -5575)]
    public partial class TournamentMenuDialog : BaseNpcDialog
    {
        public const int CONFIRM_MENU_INDEX = 0;
        public const int CANCEL_MENU_INDEX = 1;

        public NpcDialogType type;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override)]
        public NpcDialogMenuTournament[] menus;

        public override void PrepareRelatesData()
        {
            if (menus != null && menus.Length > 0)
            {
                foreach (NpcDialogMenuTournament menu in menus)
                {
                    GameInstance.AddNpcDialogs(menu.dialog);
                }
            }
        }

        public override void RenderUI(UINpcDialog uiNpcDialog)
        {
            BasePlayerCharacterEntity owningCharacter = GameInstance.PlayingCharacterEntity;

            if (uiNpcDialog.uiSellItemRoot != null)
                uiNpcDialog.uiSellItemRoot.SetActive(false);

            if (uiNpcDialog.uiSellItemDialog != null)
                uiNpcDialog.uiSellItemDialog.Hide();

            if (uiNpcDialog.uiCharacterQuest != null)
                uiNpcDialog.uiCharacterQuest.Hide();

            if (uiNpcDialog.uiCraftItem != null)
                uiNpcDialog.uiCraftItem.Hide();

            List<UINpcDialogMenuAction> menuActions = new List<UINpcDialogMenuAction>();
            switch (type)
            {
                case NpcDialogType.Normal:
                    if (uiNpcDialog.onSwitchToNormalDialog != null)
                        uiNpcDialog.onSwitchToNormalDialog.Invoke();
                    for (int i = 0; i < menus.Length; ++i)
                    {
                        NpcDialogMenuTournament menu = menus[i];
                        if(menu.isGM)
                        {
                            if(owningCharacter.TournamentGM())
                            {
                                UINpcDialogMenuAction menuAction = new UINpcDialogMenuAction();
                                menuAction.title = menu.Title;
                                menuAction.menuIndex = i;
                                menuActions.Add(menuAction);
                            }
                        }
                        else
                        {
                            UINpcDialogMenuAction menuAction = new UINpcDialogMenuAction();
                            menuAction.title = menu.Title;
                            menuAction.menuIndex = i;
                            menuActions.Add(menuAction);
                        }
                    }
                    break;
            }

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

        public override void GoToNextDialog(BasePlayerCharacterEntity characterEntity, byte menuIndex)
        {
            characterEntity.NpcAction.ClearNpcDialogData();
            // This dialog is current NPC dialog
            switch (type)
            {
                case NpcDialogType.Normal:
                    if (menuIndex >= menus.Length)
                    {
                        // Invalid menu, so no next dialog, so return itself
                        characterEntity.NpcAction.CurrentNpcDialog = GetValidatedDialogOrNull(this, characterEntity);
                        return;
                    }
                    // Changing current npc dialog
                    NpcDialogMenuTournament selectedMenu = menus[menuIndex];
                    if (!selectedMenu.IsPassConditions(characterEntity) || selectedMenu.dialog == null || selectedMenu.isCloseMenu)
                    {
                        // Close dialog, so return null
                        return;
                    }
                    characterEntity.NpcAction.CurrentNpcDialog = GetValidatedDialogOrNull(selectedMenu.dialog, characterEntity);
                    return;
            }
        }

        protected override void SetDialogByPort(NodePort from, NodePort to)
        {
            if (from.node != this)
                return;

            BaseNpcDialog dialog = null;
            if (to != null && to.node != null)
                dialog = to.node as BaseNpcDialog;

            int arrayIndex;
            if (from.fieldName.Contains("menus ") && int.TryParse(from.fieldName.Split(' ')[1], out arrayIndex) && arrayIndex < menus.Length)
                menus[arrayIndex].dialog = dialog;
        }

        public override bool IsShop
        {
            get { return type == NpcDialogType.Shop; }
        }
    }

    [System.Serializable]
    public struct NpcDialogMenuTournament
    {
        [Tooltip("Default title")]
        public string title;
        [Tooltip("Titles by language keys")]
        public LanguageData[] titles;
        public NpcDialogCondition[] showConditions;
        public bool isCloseMenu;
        public bool isGM;
        [BoolShowConditional(nameof(isCloseMenu), false)]
        public BaseNpcDialog dialog;

        public string Title
        {
            get { return Language.GetText(titles, title); }
        }

        public bool IsPassConditions(IPlayerCharacterData character)
        {
            if (dialog != null && !dialog.IsPassMenuCondition(character))
                return false;
            foreach (NpcDialogCondition showCondition in showConditions)
            {
                if (!showCondition.IsPass(character))
                    return false;
            }
            return true;
        }
    }
}
