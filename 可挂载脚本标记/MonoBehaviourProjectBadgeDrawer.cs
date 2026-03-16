using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JLZ.Editor.ProjectViewBadges
{
    [InitializeOnLoad]
    internal static class MonoBehaviourProjectBadgeDrawer
    {
        private readonly struct ScriptBadgeInfo
        {
            public ScriptBadgeInfo(bool isMonoBehaviourScript, bool isAttachable)
            {
                IsMonoBehaviourScript = isMonoBehaviourScript;
                IsAttachable = isAttachable;
            }

            public bool IsMonoBehaviourScript { get; }
            public bool IsAttachable { get; }
        }

        private const float ListBadgeWidth = 26f;
        private const float ListBadgeHeight = 14f;
        private const float GridBadgeWidth = 28f;
        private const float GridBadgeHeight = 15f;

        private static readonly Color s_attachableBadgeColor = new(0.18f, 0.56f, 0.94f, 0.95f);
        private static readonly Color s_baseClassBadgeColor = new(0.40f, 0.45f, 0.52f, 0.92f);
        private static readonly Color s_badgeBorderColor = new(0f, 0f, 0f, 0.18f);
        private static readonly Dictionary<string, ScriptBadgeInfo> s_scriptBadgeInfoByGuid = new(StringComparer.Ordinal);
        private static readonly GUIContent s_attachableBadgeContent = new("MB", "MonoBehaviour script");
        private static readonly GUIContent s_baseClassBadgeContent = new("MB", "MonoBehaviour base class");

        private static GUIStyle s_badgeStyle;

        static MonoBehaviourProjectBadgeDrawer()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;

            EditorApplication.projectChanged -= ClearCache;
            EditorApplication.projectChanged += ClearCache;
        }

        private static void ClearCache()
        {
            s_scriptBadgeInfoByGuid.Clear();
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            Event currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type != EventType.Repaint)
                return;

            if (selectionRect.width <= 0f || selectionRect.height <= 0f)
                return;

            if (!TryGetScriptBadgeInfo(guid, out ScriptBadgeInfo badgeInfo))
                return;

            Rect badgeRect = GetBadgeRect(selectionRect, IsListMode(selectionRect));
            if (badgeRect.width <= 2f || badgeRect.height <= 2f)
                return;

            DrawBadge(
                badgeRect,
                badgeInfo.IsAttachable ? s_attachableBadgeColor : s_baseClassBadgeColor,
                badgeInfo.IsAttachable ? s_attachableBadgeContent : s_baseClassBadgeContent);
        }

        private static bool TryGetScriptBadgeInfo(string guid, out ScriptBadgeInfo badgeInfo)
        {
            if (s_scriptBadgeInfoByGuid.TryGetValue(guid, out badgeInfo))
                return badgeInfo.IsMonoBehaviourScript;

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath) ||
                !assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                badgeInfo = new ScriptBadgeInfo(false, false);
                s_scriptBadgeInfoByGuid[guid] = badgeInfo;
                return false;
            }

            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
            Type scriptType = monoScript != null ? monoScript.GetClass() : null;
            bool isMonoBehaviourScript = scriptType != null && typeof(MonoBehaviour).IsAssignableFrom(scriptType);
            bool isAttachable = isMonoBehaviourScript && !scriptType.IsAbstract && !scriptType.ContainsGenericParameters;

            badgeInfo = new ScriptBadgeInfo(isMonoBehaviourScript, isAttachable);
            s_scriptBadgeInfoByGuid[guid] = badgeInfo;
            return isMonoBehaviourScript;
        }

        private static bool IsListMode(Rect selectionRect)
        {
            return selectionRect.height <= EditorGUIUtility.singleLineHeight + 6f;
        }

        private static Rect GetBadgeRect(Rect selectionRect, bool isListMode)
        {
            if (isListMode)
            {
                float width = Mathf.Min(ListBadgeWidth, Mathf.Max(18f, selectionRect.width - 4f));
                return new Rect(
                    selectionRect.xMax - width - 4f,
                    selectionRect.y + (selectionRect.height - ListBadgeHeight) * 0.5f,
                    width,
                    ListBadgeHeight);
            }

            float widthGrid = Mathf.Min(GridBadgeWidth, Mathf.Max(20f, selectionRect.width - 8f));
            return new Rect(
                selectionRect.xMax - widthGrid - 6f,
                selectionRect.y + 4f,
                widthGrid,
                GridBadgeHeight);
        }

        private static void DrawBadge(Rect badgeRect, Color backgroundColor, GUIContent content)
        {
            EnsureBadgeStyle();

            EditorGUI.DrawRect(badgeRect, s_badgeBorderColor);

            Rect innerRect = new(
                badgeRect.x + 1f,
                badgeRect.y + 1f,
                Mathf.Max(0f, badgeRect.width - 2f),
                Mathf.Max(0f, badgeRect.height - 2f));

            EditorGUI.DrawRect(innerRect, backgroundColor);
            GUI.Label(innerRect, content, s_badgeStyle);
        }

        private static void EnsureBadgeStyle()
        {
            if (s_badgeStyle != null)
                return;

            s_badgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                padding = new RectOffset(0, 0, 0, 0),
                clipping = TextClipping.Clip
            };

            s_badgeStyle.normal.textColor = Color.white;
            s_badgeStyle.hover.textColor = Color.white;
            s_badgeStyle.active.textColor = Color.white;
            s_badgeStyle.focused.textColor = Color.white;
        }
    }
}
