using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JLZ.Editor.ProjectTools
{
    public static class ProjectStructureGenerator
    {
        private const string DefaultTemplateAssetPath =
            "Assets/Editor/ProjectTools/DefaultProjectStructureTemplate.asset";

        public static ProjectStructureTemplate GetOrCreateDefaultTemplate()
        {
            var template = AssetDatabase.LoadAssetAtPath<ProjectStructureTemplate>(DefaultTemplateAssetPath);
            if (template != null)
                return template;

            EnsureFolder("Assets/Editor");
            EnsureFolder("Assets/Editor/ProjectTools");

            template = ScriptableObject.CreateInstance<ProjectStructureTemplate>();
            FillDefaultTemplate(template);

            AssetDatabase.CreateAsset(template, DefaultTemplateAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return template;
        }

        public static void Generate(ProjectStructureTemplate template)
        {
            if (template == null)
            {
                EditorUtility.DisplayDialog("Error", "Template 不能为空。", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(template.rootFolderName))
            {
                EditorUtility.DisplayDialog("Error", "Root Folder Name 不能为空。", "OK");
                return;
            }

            string root = $"Assets/{template.rootFolderName}";
            EnsureFolder(root);

            int folderCount = GenerateFolders(root, template);
            int asmdefCount = GenerateAsmdefs(root, template);
            int placeholderCount = GeneratePlaceholderScripts(root, template);
            string readmePath = GenerateReadme(root, template);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "完成",
                $"项目结构生成完成。\n\n" +
                $"Root: {root}\n" +
                $"Folders: {folderCount}\n" +
                $"asmdef: {asmdefCount}\n" +
                $"Placeholder Scripts: {placeholderCount}\n" +
                $"README: {readmePath}",
                "OK");
        }

        private static int GenerateFolders(string root, ProjectStructureTemplate template)
        {
            int created = 0;

            foreach (string relative in template.folders.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                string fullPath = CombineAssetPath(root, relative);
                if (!AssetDatabase.IsValidFolder(fullPath))
                {
                    EnsureFolder(fullPath);
                    created++;
                }
            }

            return created;
        }

        private static int GenerateAsmdefs(string root, ProjectStructureTemplate template)
        {
            int created = 0;

            foreach (var def in template.asmdefs)
            {
                if (def == null || string.IsNullOrWhiteSpace(def.folder) || string.IsNullOrWhiteSpace(def.name))
                    continue;

                string folderPath = CombineAssetPath(root, def.folder);
                EnsureFolder(folderPath);

                string asmdefPath = CombineFsPath(folderPath, $"{def.name}.asmdef");
                if (File.Exists(asmdefPath))
                    continue;

                var asmdefJson = new AsmdefJson
                {
                    name = def.name,
                    references = def.references?.ToArray() ?? Array.Empty<string>(),
                    includePlatforms = def.editorOnly ? new[] { "Editor" } : Array.Empty<string>(),
                    excludePlatforms = Array.Empty<string>(),
                    allowUnsafeCode = false,
                    overrideReferences = false,
                    precompiledReferences = Array.Empty<string>(),
                    autoReferenced = def.autoReferenced,
                    defineConstraints = Array.Empty<string>(),
                    versionDefines = Array.Empty<VersionDefine>(),
                    noEngineReferences = false
                };

                string json = JsonUtility.ToJson(asmdefJson, true);
                File.WriteAllText(asmdefPath, json, new UTF8Encoding(false));
                created++;
            }

            return created;
        }

        private static int GeneratePlaceholderScripts(string root, ProjectStructureTemplate template)
        {
            int created = 0;

            foreach (var def in template.placeholderScripts)
            {
                if (def == null || string.IsNullOrWhiteSpace(def.folder) ||
                    string.IsNullOrWhiteSpace(def.fileName) || string.IsNullOrWhiteSpace(def.className))
                    continue;

                string folderPath = CombineAssetPath(root, def.folder);
                EnsureFolder(folderPath);

                string filePath = CombineFsPath(folderPath, $"{def.fileName}.cs");
                if (File.Exists(filePath))
                    continue;

                string scriptContent = BuildScriptContent(template.baseNamespace, def);
                File.WriteAllText(filePath, scriptContent, new UTF8Encoding(false));
                created++;
            }

            return created;
        }

        private static string GenerateReadme(string root, ProjectStructureTemplate template)
        {
            string path = CombineFsPath(root, "README_ProjectStructure.md");
            if (File.Exists(path))
                return path.Replace("\\", "/");

            string content = template.readmeContent
                .Replace("{BASE_NS}", template.baseNamespace)
                .Replace("\r\n", "\n");

            File.WriteAllText(path, content, new UTF8Encoding(false));
            return path.Replace("\\", "/");
        }

        private static string BuildScriptContent(string baseNamespace, PlaceholderScriptDefinition def)
        {
            string fullNamespace = BuildNamespace(baseNamespace, def.namespaceSuffix);
            string summaryComment = string.IsNullOrWhiteSpace(def.description)
                ? string.Empty
                : $"/// <summary>\n    /// {def.description}\n    /// </summary>\n    ";

            switch (def.scriptType)
            {
                case PlaceholderScriptType.Interface:
                    return
$@"namespace {fullNamespace}
{{
    public interface {def.className}
    {{
    }}
}}
";
                case PlaceholderScriptType.ScriptableObject:
                    return
$@"using UnityEngine;

namespace {fullNamespace}
{{
    [CreateAssetMenu(fileName = ""{def.className}"", menuName = ""Game/{def.className}"")]
    public sealed class {def.className} : ScriptableObject
    {{
    }}
}}
";
                case PlaceholderScriptType.MonoBehaviour:
                    return
$@"using UnityEngine;

namespace {fullNamespace}
{{
    public sealed class {def.className} : MonoBehaviour
    {{
        {summaryComment}private void Awake()
        {{
        }}
    }}
}}
";
                default:
                    return
$@"namespace {fullNamespace}
{{
    public sealed class {def.className}
    {{
    }}
}}
";
            }
        }

        private static string BuildNamespace(string baseNamespace, string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                return baseNamespace;

            return $"{baseNamespace}.{suffix}";
        }

        private static string CombineAssetPath(string left, string right)
        {
            return $"{left.TrimEnd('/')}/{right.TrimStart('/')}";
        }

        private static string CombineFsPath(string assetFolderPath, string fileName)
        {
            string projectRoot = Directory.GetCurrentDirectory().Replace("\\", "/");
            return $"{projectRoot}/{assetFolderPath.TrimStart('/')}/{fileName}";
        }

        public static void EnsureFolder(string fullAssetPath)
        {
            if (AssetDatabase.IsValidFolder(fullAssetPath))
                return;

            string[] parts = fullAssetPath.Split('/');
            if (parts.Length < 2 || parts[0] != "Assets")
            {
                Debug.LogError($"非法资源路径: {fullAssetPath}");
                return;
            }

            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static void FillDefaultTemplate(ProjectStructureTemplate template)
        {
            template.rootFolderName = "Game";
            template.baseNamespace = "Game";

            template.folders = new List<string>
            {
                // Scripts
                "Scripts",
                "Scripts/Runtime",
                "Scripts/Runtime/Core",
                "Scripts/Runtime/Core/Bootstrap",
                "Scripts/Runtime/Core/Common",
                "Scripts/Runtime/Core/Events",
                "Scripts/Runtime/Gameplay",
                "Scripts/Runtime/Gameplay/Combat",
                "Scripts/Runtime/Gameplay/Actors",
                "Scripts/Runtime/Gameplay/Progression",
                "Scripts/Runtime/Gameplay/Spawning",
                "Scripts/Runtime/Gameplay/Interaction",
                "Scripts/Runtime/Systems",
                "Scripts/Runtime/Systems/Input",
                "Scripts/Runtime/Systems/Save",
                "Scripts/Runtime/Systems/Audio",
                "Scripts/Runtime/Systems/Camera",
                "Scripts/Runtime/Systems/Addressables",
                "Scripts/Runtime/UI",
                "Scripts/Runtime/Shared",

                "Scripts/Editor",
                "Scripts/Generated",
                "Scripts/Generated/Input",
                "Scripts/Tests",
                "Scripts/Tests/EditMode",
                "Scripts/Tests/PlayMode",

                // GameData
                "GameData",
                "GameData/Configs",
                "GameData/Configs/Combat",
                "GameData/Configs/Actors",
                "GameData/Configs/Progression",
                "GameData/Configs/Spawn",
                "GameData/Configs/UI",

                "GameData/Definitions",
                "GameData/Definitions/Weapons",
                "GameData/Definitions/Enemies",
                "GameData/Definitions/Upgrades",
                "GameData/Definitions/Items",

                "GameData/Input",
                "GameData/Tables",
                "GameData/RuntimeSets",
                "GameData/Variables",

                // Content
                "Content",
                "Content/Scenes",
                "Content/Prefabs",
                "Content/UI",
                "Content/Characters",
                "Content/Environment",
                "Content/VFX",
                "Content/Audio",
                "Content/Materials",
                "Content/Animations",
                "Content/Sprites",
                "Content/Models",

                // Misc
                "Plugins",
                "ThirdParty",
                "Gizmos",
                "Utilities"
            };

            template.asmdefs = new List<AsmdefDefinition>
            {
                new()
                {
                    folder = "Scripts/Runtime",
                    name = "Game.Runtime",
                    references = new List<string>()
                },
                new()
                {
                    folder = "Scripts/Editor",
                    name = "Game.Editor",
                    references = new List<string> { "Game.Runtime" },
                    editorOnly = true
                },
                new()
                {
                    folder = "Scripts/Tests/EditMode",
                    name = "Game.Tests.EditMode",
                    references = new List<string> { "Game.Runtime" }
                },
                new()
                {
                    folder = "Scripts/Tests/PlayMode",
                    name = "Game.Tests.PlayMode",
                    references = new List<string> { "Game.Runtime" }
                }
            };

            template.placeholderScripts = new List<PlaceholderScriptDefinition>
            {
                new()
                {
                    folder = "Scripts/Runtime/Core/Bootstrap",
                    fileName = "GameBootstrapper",
                    className = "GameBootstrapper",
                    namespaceSuffix = "Runtime.Core.Bootstrap",
                    scriptType = PlaceholderScriptType.MonoBehaviour,
                    description = "项目启动入口，可用于初始化全局系统与服务。"
                },
                new()
                {
                    folder = "Scripts/Runtime/Systems/Input",
                    fileName = "InputRouter",
                    className = "InputRouter",
                    namespaceSuffix = "Runtime.Systems.Input",
                    scriptType = PlaceholderScriptType.MonoBehaviour,
                    description = "输入路由层，承接 Input System 事件并转发到业务层。"
                },
                new()
                {
                    folder = "Scripts/Runtime/Core/Events",
                    fileName = "GameEventBus",
                    className = "GameEventBus",
                    namespaceSuffix = "Runtime.Core.Events",
                    scriptType = PlaceholderScriptType.PlainClass,
                    description = "全局事件总线占位类。"
                },
                new()
                {
                    folder = "Scripts/Runtime/Gameplay/Combat",
                    fileName = "IDamageable",
                    className = "IDamageable",
                    namespaceSuffix = "Runtime.Gameplay.Combat",
                    scriptType = PlaceholderScriptType.Interface,
                    description = "可受伤接口。"
                },
                new()
                {
                    folder = "Scripts/Runtime/Gameplay/Combat",
                    fileName = "IHealth",
                    className = "IHealth",
                    namespaceSuffix = "Runtime.Gameplay.Combat",
                    scriptType = PlaceholderScriptType.Interface,
                    description = "生命值读取接口。"
                },
                new()
                {
                    folder = "Scripts/Runtime/Gameplay/Combat",
                    fileName = "DamageConfig",
                    className = "DamageConfig",
                    namespaceSuffix = "Runtime.Gameplay.Combat",
                    scriptType = PlaceholderScriptType.ScriptableObject,
                    description = "伤害配置 ScriptableObject 占位类。"
                }
            };

            EditorUtility.SetDirty(template);
        }

        [Serializable]
        private sealed class AsmdefJson
        {
            public string name;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public string[] precompiledReferences;
            public bool autoReferenced;
            public string[] defineConstraints;
            public VersionDefine[] versionDefines;
            public bool noEngineReferences;
        }

        [Serializable]
        private sealed class VersionDefine
        {
            public string name;
            public string expression;
            public string define;
        }
    }
}