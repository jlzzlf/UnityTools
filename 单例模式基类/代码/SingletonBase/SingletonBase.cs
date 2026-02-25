using System;
using System.Reflection;

/// <summary>
/// 不继承Mono behaviour的单例模式基类
/// 抽象类防止外部实例化
/// </summary>
/// <typeparam name="T">子类类型</typeparam>
public abstract class SingletonBase<T> where T :class
{
    //饿汉式单例（CLR加载时实现）
    
    public static T Instance { get; } = CreateInstance();

    //使用反射创建实例 防止反射破坏单例
    private static T CreateInstance()
    {
        //通过反射创建实例（兼容私有构造函数）
        ConstructorInfo constructor = typeof(T).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            Type.EmptyTypes,
            null);

        if (constructor == null)
        {
            throw new Exception($"单例类 {typeof(T).Name} 必须包含私有无参构造函数！");
        }

        T instance = (T)constructor.Invoke(null);

        // 检查单例实例是否已经通过反射创建，如果已创建则抛出异常
        FieldInfo instanceField = typeof(SingletonBase<T>).GetField(
        "instance",
        BindingFlags.NonPublic | BindingFlags.Static);

        // 检查静态字段是否已经初始化，以防止通过反射重复创建单例
        if (instanceField != null && instanceField.GetValue(null) != null)
        {
            throw new Exception($"禁止通过反射重复创建单例 {typeof(T).Name}！");
        }

        return instance;
    }
}
