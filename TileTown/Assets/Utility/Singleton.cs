using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour, new()
{
    protected static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
            }
            return _instance;
        }
    }

    /// <summary>
    ///  Do not override this. Use SingletonAwake instead.
    /// </summary>
    protected virtual void Awake()
    {
        // Check if this is null, if yes, return
        if (this == null)
        {
            return;
        }

        // Check if there already is an instance, if yes, return
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        // Set this as the instance
        _instance = this as T;

        // Call a seperate awake function for singleton objects which is only executed if this is the instance
        SingletonAwake();
    }
    protected virtual void SingletonAwake() { }
}
public abstract class SingletonPersistant<T> : Singleton<T> where T : MonoBehaviour, new()
{
    protected override void Awake()
    {
        // Check if this is null, if yes, return
        if (this == null)
        {
            return;
        }

        // Check if there already is an instance, if yes, return
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        // Set this as the instance
        _instance = this as T;

        // Call a seperate awake function for singleton objects which is only executed if this is the instance
        SingletonAwake();
        DontDestroyOnLoad(gameObject);


    }
}
