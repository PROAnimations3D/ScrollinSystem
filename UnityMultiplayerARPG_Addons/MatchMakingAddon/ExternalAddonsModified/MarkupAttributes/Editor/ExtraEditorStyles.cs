using UnityEditor;
using UnityEngine;

namespace MarkupAttributes
{
    public static class ExtraEditorStyles
    {
        private static Color darkColor = new Color(0.18f, 0.18f, 0.18f);
        private static Color lightColor = new Color(1f, 1f, 1f, 1f);
        private static GUIStyle boldFoldout;
        public static GUIStyle BoldFoldout
        {
            get
            {
                if (boldFoldout == null)
                {
                    boldFoldout = new GUIStyle(EditorStyles.foldout);
                    boldFoldout.font = EditorStyles.boldFont;
                }
                return boldFoldout;
            }
        }


        public static GUIStyle GroupBox => EditorStyles.helpBox;

        private static GUIStyle frameBox;
        public static GUIStyle TabsBox
        {
            get
            {
                if (frameBox == null)
                {
                    frameBox = new GUIStyle("FrameBox");
                    frameBox.padding = EditorStyles.helpBox.padding;
                }
                return frameBox;
            }
        }

        public static GUIStyle HeaderBox(bool opened)
        {
            InitializeHeaderStyles();
            if (opened)
                return EditorGUIUtility.isProSkin ? headerBoxOpenedDark : headerBoxOpenedLight;
            else
                return EditorGUIUtility.isProSkin ? headerBoxClosedDark : headerBoxClosedLight;
        } 

        private static bool headerStylesInitialized;
        private static GUIStyle headerBoxOpenedDark;
        private static GUIStyle headerBoxOpenedLight;
        private static GUIStyle headerBoxClosedDark;
        private static GUIStyle headerBoxClosedLight;

        private static void InitializeHeaderStyles()
        {
            if (!headerStylesInitialized)
            {
                headerBoxOpenedDark = CreateBoxStyle(GetTexture("MarkupAttributes_HeaderOpened_Dark"));
                headerBoxOpenedLight = CreateBoxStyle(GetTexture("MarkupAttributes_HeaderOpened_Light"));
                headerBoxClosedDark = CreateBoxStyle(GetTexture("MarkupAttributes_HeaderClosed_Dark"));
                headerBoxClosedLight = CreateBoxStyle(GetTexture("MarkupAttributes_HeaderClosed_Light"));
                headerStylesInitialized = true;
            }
        }

        private static GUIStyle CreateBoxStyle(Texture2D texture)
        {
            var style = new GUIStyle(EditorStyles.helpBox);
            style.normal.background = Texture2D.whiteTexture;
            if (texture != null)
                style.normal.background = EditorGUIUtility.isProSkin ? ColoredTexture(darkColor) : ColoredTexture(lightColor);
            return style;
        }

        private static Texture2D GetTexture(string name)
        {
            string[] results = AssetDatabase.FindAssets(name);
            if (results != null && results.Length > 0)
                return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(results[0]));
            return null;
        }

        private static Texture2D ColoredTexture(Color color)
        {
            int w = 4, h = 4;
            Texture2D back = new Texture2D(w, h);
            Color[] buffer = new Color[w * h];
            for (int i = 0; i < w; ++i)
                for (int j = 0; j < h; ++j)
                    buffer[i + w * j] = color;
            back.SetPixels(buffer);
            back.Apply(false);
            return back;
        }
    }
}
