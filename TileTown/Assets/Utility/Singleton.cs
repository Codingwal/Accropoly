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
            Debug.Log("Instance has been requested");
            if (_instance == null)
            {
                Debug.Log("Creating new instance");

                GameObject manager = GameObject.Find("Manager");
                T component = manager.AddComponent<T>();
                _instance = component;
            }
            return _instance;
        }
    }
}
public abstract class SingletonPersistant<T> where T : MonoBehaviour, new()
{
    protected static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {

                GameObject manager = GameObject.Find("Manager");
                T component = manager.AddComponent<T>();
                _instance = component;

                Object.DontDestroyOnLoad(manager);
            }
            return _instance;
        }
    }
}
