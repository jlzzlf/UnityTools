using System;
using System.Collections.Generic;
using UnityEngine;

namespace JLZ.Editor.ProjectTools
{
    [CreateAssetMenu(
        fileName = "ProjectStructureTemplate",
        menuName = "Tools/Project Structure Template")]
    public sealed class ProjectStructureTemplate : ScriptableObject
    {
        [Header("Root")]
        public string rootFolderName = "Game";

        [Tooltip("生成脚本时使用的基础命名空间前缀，例如 Game 或 MyCompany.MyGame")]
        public string baseNamespace = "Game";

        [Header("Folders (relative to root)")]
        [Tooltip("相对于 Root 的目录，比如 Scripts/Runtime/Core")]
        public List<string> folders = new();

        [Header("asmdef")]
        public List<AsmdefDefinition> asmdefs = new();

        [Header("Placeholder Scripts")]
        public List<PlaceholderScriptDefinition> placeholderScripts = new();

        [Header("README")]
        [TextArea(8, 30)]
        public string readmeContent =
@"# Project Structure

## 目录说明
- Scripts/Runtime: 运行时代码
- Scripts/Editor: 编辑器工具
- Scripts/Generated: 自动生成代码
- Scripts/Tests: 测试代码
- GameData/Configs: 配置型 ScriptableObject 资源
- GameData/Definitions: 定义型 ScriptableObject 资源
- GameData/Input: Input System 资源
- Content: 场景、美术、预制体、音频等内容资源

## 命名空间建议
- Runtime: {BASE_NS}.Runtime.*
- Editor: {BASE_NS}.Editor.*
- Tests: {BASE_NS}.Tests.*
";
    }

    [Serializable]
    public sealed class AsmdefDefinition
    {
        [Tooltip("asmdef 文件相对 root 的目录，比如 Scripts/Runtime")]
        public string folder;

        [Tooltip("程序集名称")]
        public string name;

        [Tooltip("引用的程序集名称")]
        public List<string> references = new();

        [Tooltip("仅编辑器程序集")]
        public bool editorOnly;

        [Tooltip("是否自动引用预编译程序集")]
        public bool autoReferenced = true;
    }

    [Serializable]
    public sealed class PlaceholderScriptDefinition
    {
        [Tooltip("脚本相对 root 的目录，比如 Scripts/Runtime/Core/Bootstrap")]
        public string folder;

        [Tooltip("文件名，不带 .cs")]
        public string fileName;

        [Tooltip("命名空间相对 baseNamespace 的后缀，比如 Runtime.Core.Bootstrap")]
        public string namespaceSuffix;

        [Tooltip("类名")]
        public string className;

        [Tooltip("脚本类型")]
        public PlaceholderScriptType scriptType;

        [Tooltip("若为 MonoBehaviour，可额外注释说明用途")]
        [TextArea(2, 6)]
        public string description;
    }

    public enum PlaceholderScriptType
    {
        PlainClass,
        MonoBehaviour,
        ScriptableObject,
        Interface
    }
}