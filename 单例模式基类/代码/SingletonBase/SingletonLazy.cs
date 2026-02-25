using System;
using System.Reflection;
using System.Threading;

public abstract class SingletonLazy<T> where T :class
{
    //懒汉单例
    private static readonly Lazy<T> LazyInstance = new Lazy<T>(CreateInstance,LazyThreadSafetyMode.ExecutionAndPublication);
    public static T Instance => LazyInstance.Value;

    private static T CreateInstance()
    {
        ConstructorInfo constructor = typeof(T).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            Type.EmptyTypes,
            null);
        if (constructor==null)
        {
            throw new InvalidOperationException($"{typeof(T).Name}必须包含私有无参构造函数!");
        }

        return (T)constructor.Invoke(null);
    }
}
