using UnityEditor;
using UnityEngine;

namespace JLZ.Editor.ProjectTools
{
    public sealed class ProjectStructureGeneratorWindow : EditorWindow
    {
        private ProjectStructureTemplate _template;
        private Vector2 _scroll;

        [MenuItem("Tools/Project/Project结构生成器")]
        public static void Open()
        {
            GetWindow<ProjectStructureGeneratorWindow>("项目结构生成工具");
        }

        private void OnEnable()
        {
            if (_template == null)
            {
                _template = ProjectStructureGenerator.GetOrCreateDefaultTemplate();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Project Structure Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "通过模板一键生成项目目录、asmdef、README 和常用占位脚本。",
                MessageType.Info);

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                _template = (ProjectStructureTemplate)EditorGUILayout.ObjectField(
                    "模板",
                    _template,
                    typeof(ProjectStructureTemplate),
                    false);

                if (GUILayout.Button("创建默认模板", GUILayout.Width(180)))
                {
                    _template = ProjectStructureGenerator.GetOrCreateDefaultTemplate();
                    EditorGUIUtility.PingObject(_template);
                }
            }

            EditorGUILayout.Space();

            if (_template == null)
            {
                EditorGUILayout.HelpBox("请先指定一个 ProjectStructureTemplate。", MessageType.Warning);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("根目录名称", _template.rootFolderName);
                EditorGUILayout.TextField("基础命名空间", _template.baseNamespace);
                EditorGUILayout.IntField("目录数量", _template.folders?.Count ?? 0);
                EditorGUILayout.IntField("自定义程序集数量", _template.asmdefs?.Count ?? 0);
                EditorGUILayout.IntField("占位脚本数量", _template.placeholderScripts?.Count ?? 0);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("在项目中定位模板资源", GUILayout.Height(28)))
            {
                EditorGUIUtility.PingObject(_template);
                Selection.activeObject = _template;
            }

            if (GUILayout.Button("生成项目结构", GUILayout.Height(36)))
            {
                ProjectStructureGenerator.Generate(_template);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}