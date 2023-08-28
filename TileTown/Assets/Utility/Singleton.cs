using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            return _instance;
        }
    }
    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
    }
}
public abstract class SingletonPersistant<T> : Singleton<T> where T : MonoBehaviour, new()
{
    protected override void Awake()
    {
        base.Awake();

        if (this != null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
