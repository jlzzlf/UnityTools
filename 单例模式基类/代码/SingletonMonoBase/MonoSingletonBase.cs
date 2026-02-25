using System;
using UnityEngine;

/// <summary>
/// Mono单例模式基类
/// 重写 OnSingletonAwake 方法实现初始化逻辑
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class MonoSingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            //若实例被销毁，重新创建
            if(!_instance && _isGlobalSingleton)
            {
                CreateSingletonInstance();
            }
            return _instance;
        }
    }

    //是否是全局单例
    private static bool _isGlobalSingleton = true;

    //在unity加载任何场景之前执行
    // ReSharper disable Unity.PerformanceAnalysis
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateSingletonInstance()
    {
        if (_instance)
            return;
        //查找场景中是否已有该组件
        _instance = FindAnyObjectByType<T>();

        //如果场景中无实例 创建新GameObject挂载
        if (_instance)
            return;
        GameObject singletonObj = new(typeof(T).Name + "_Singleton");
        _instance = singletonObj.AddComponent<T>();

        if(_isGlobalSingleton)
        {
            DontDestroyOnLoad(singletonObj);
        }
    }

    /// <summary>
    /// 在OnSingletonAwake方法中调用，可设置是否全局单例
    /// 默认为true
    /// </summary>
    /// <param name="isGlobal"></param>
    protected static void SetGlobalSingleton(bool isGlobal)
    {
        _isGlobalSingleton = isGlobal;
    }


    /// <summary>
    /// 不建议重写Awake方法
    /// 如需添加初始化逻辑，请重写OnSingletonAwake方法!
    /// </summary>
    [Obsolete("请重写 OnSingletonAwake 方法实现初始化逻辑", false)]
    protected virtual void Awake()
    {
        if(_instance==null)
        {
            _instance = this as T;
            
            //检测是否有重复的单例
            if(_instance!=this)
            {
                DestroyImmediate(this);
                Debug.Log($"检测到重复的 {typeof(T).Name} 单例，已销毁多余实例！");
            }

            //子类初始化逻辑入口
            OnSingletonAwake();
        }
    }

    protected virtual void OnSingletonAwake() { }
}
