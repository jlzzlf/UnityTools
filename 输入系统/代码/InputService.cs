using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    /// <summary>
    /// 场景里只放一个InputService，拖入你的InputConfig
    /// Map默认Player/UI
    /// 支持：启用/禁用Map、只启用某个Map、Move/Look缓存、Jump/Interact事件
    /// </summary>
    public class InputService: MonoBehaviour
    {
        public static  InputService Instance { get; private set; }
        
        [SerializeField] 
        private InputConfig config;

        private InputActionAsset _asset;
        
        // 常用动作缓存（按需扩展）
        private InputAction _move;
        private InputAction _look;
        private InputAction _jump;
        private InputAction _interact;

        private InputAction _uiNavigate;
        private InputAction _uiSubmit;
        private InputAction _uiCancel;
        
        // 输入状态缓存（让外部脚本不用订阅 action）
        private Vector2 _moveValue;
        private Vector2 _lookValue;

        public Vector2 Move => _moveValue;
        public Vector2 Look => _lookValue;

        public event Action JumpPerformed;      
        public event Action InteractPerformed;  
        
        public InputActionAsset Asset => _asset;
        protected  void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 必须先配置 InputConfig，否则 InputService 无法初始化输入系统
            if (!config)
            {
                throw new InvalidOperationException("InputService:未指定InputConfig");
            }
            // InputConfig 存在，但内部未绑定 InputActionAsset（.inputactions 资源）
            if (!config.actions)
            {
                throw new InvalidOperationException("InputActionAsset为空,请在InputConfig中拖入.inputactions文件");
            }

            
            _asset = Instantiate(config.actions);
            
            //TODO:一个方法，加载玩家重绑定覆盖
            
            //缓存常用Action
            _move = FindActionOptional(config.moveActionPath);
            _look = FindActionOptional(config.lookActionPath);
            _jump = FindActionOptional(config.jumpActionPath);
            _interact = FindActionOptional(config.interactActionPath);

            _uiNavigate = FindActionOptional(config.uiNavigateActionPath);
            _uiSubmit = FindActionOptional(config.uiSubmitActionPath);
            _uiCancel = FindActionOptional(config.uiCancelActionPath);
            
            //启用启动时的 ActionMap（由 Config 控制）
            EnableMapsFromConfig();
        }

        private void OnEnable()
        {
            BindValueAction(_move,OnMove);
            BindValueAction(_look,OnLook);
            BindButtonAction(_jump,OnJump);
            BindButtonAction(_interact,OnInteract);
        }

        private void OnDisable()
        {
            UnBindValueAction(_move,OnMove);
            UnBindValueAction(_look,OnLook);
            UnBindButtonAction(_jump,OnJump);
            UnBindButtonAction(_interact,OnInteract);
            _moveValue=Vector2.zero;
            _lookValue=Vector2.zero;
        }

        private void EnableMap(string mapName)
        {
            if(string.IsNullOrWhiteSpace(mapName))
                return;
            var map=_asset.FindActionMap(mapName,false);
            map?.Enable();
        }

        private void DisableMap(string mapName)
        {
            if(string.IsNullOrWhiteSpace(mapName))
                return;
            var map=_asset.FindActionMap(mapName,false);
            map?.Disable();
        }

        /// <summary>
        /// 只启用一个Map（例如打开菜单只启用UI）
        /// </summary>
        public void SwitchToOnlyMap(string mapName)
        {
            foreach (var map in _asset.actionMaps)
            {
                map.Disable();
            }
            EnableMap(mapName);
        }
        private void EnableMapsFromConfig()
        {
            if(config.enableActionMapsOnstart==null)
                return;
            foreach (var mapName in config.enableActionMapsOnstart)
            {
                EnableMap(mapName);
            }
        }

        //查找与订阅
        private InputAction FindActionOptional(string actionPath)
        {
            if(string.IsNullOrWhiteSpace(actionPath))
                return null;
            
            return _asset.FindAction(actionPath,throwIfNotFound:config.throwIfActionNotFound);
        }
        //绑定/解绑值类型Action
        private static void BindValueAction(InputAction inputAction,Action<InputAction.CallbackContext> handler)
        {
            if (inputAction == null)
                return;
            inputAction.performed += handler;
            inputAction.canceled += handler;
        }

        private static void UnBindValueAction(InputAction inputAction, Action<InputAction.CallbackContext> handler)
        {
            if (inputAction == null)
                return;
            inputAction.performed -= handler;
            inputAction.canceled -= handler;
        }

        private static void BindButtonAction(InputAction inputAction, Action<InputAction.CallbackContext> handler)
        {
            if (inputAction == null)
                return;
            inputAction.performed += handler;
        }

        private static void UnBindButtonAction(InputAction inputAction, Action<InputAction.CallbackContext> handler)
        {
            if (inputAction == null)
                return;
            inputAction.performed -= handler;
        }
        private void OnMove(InputAction.CallbackContext context)
        {
            _moveValue = context.ReadValue<Vector2>();
        }
        private void OnLook(InputAction.CallbackContext context)
        {
            _lookValue = context.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            JumpPerformed?.Invoke();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            InteractPerformed?.Invoke();
        }
    }
}
