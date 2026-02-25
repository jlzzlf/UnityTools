using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/InputConfig",fileName =  "InputConfig")]
public class InputConfig : ScriptableObject
{
    [FormerlySerializedAs("inputActionAsset")]
    [Header("InputActions文件（每个项目不同）")]
    [Tooltip("拖入本项目使用的 .inputactions 文件")]
    public InputActionAsset actions;

    [Header("启动时启用的ActionMap（名称必须与InputActions里一致）")]
    [Tooltip("InputService 初始化时会自动 Enable 这些 ActionMap")]
    public List<string> enableActionMapsOnstart = new() { };
    
    [Header("常用输入动作路径（Map/Action）")]
    [Tooltip("格式：Map/Action，例如：Player/Move（名称必须与 InputActions 里一致）")]
    public string moveActionPath = "";

    [Tooltip("例如：Player/Look（可选）")]
    public string lookActionPath = "";

    [Tooltip("例如：Player/Jump（可选）")]
    public string jumpActionPath = "";

    [Tooltip("例如：Player/Interact（可选）")]
    public string interactActionPath = "";

    [Tooltip("例如：UI/Navigate（可选）")]
    public string uiNavigateActionPath = "";

    [Tooltip("例如：UI/Submit（可选）")]
    public string uiSubmitActionPath = "";

    [Tooltip("例如：UI/Cancel（可选）")]
    public string uiCancelActionPath = "";


    [Header("重绑定交互默认设置")]
    [Tooltip("键盘取消重绑定的按键路径")]
    public string cancelBindingPathKeyboard = "<Keyboard>/escape";

    [Tooltip("手柄取消重绑定的按键路径")]
    public string cancelBindingPathGamepad = "<Gamepad>/start";

    [Tooltip("重绑定时排除鼠标位置/移动（避免误触）")]
    public bool excludeMousePositionAndDelta = true;

    [Header("安全检查")]
    [Tooltip("找不到 Action 时是否抛出异常（调试阶段建议勾选）")]
    public bool throwIfActionNotFound = true;

}
