using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(MatchRegisterDialog))]
    [CanEditMultipleObjects]
    public class MatchRegisterDialogEditor : BaseCustomEditor
    {
        private static MatchRegisterDialog cacheMatchRegisterDialog;
        protected override void SetFieldCondition()
        {
            if (cacheMatchRegisterDialog == null)
                cacheMatchRegisterDialog = CreateInstance<MatchRegisterDialog>();

            if ((target as MatchRegisterDialog).graph == null)
            {
                hiddenFields.Add("graph");
                hiddenFields.Add("position");
                hiddenFields.Add("ports");
            }
            hiddenFields.Add("input");
            // Normal
            //ShowOnEnum(nameof(cacheMatchRegisterDialog.type), nameof(MatchType.None), nameof(cacheMatchRegisterDialog.menus));
        }
    }
}
