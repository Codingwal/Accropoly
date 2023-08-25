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
                GameObject manager = GameObject.Find("Manager");

                _instance = manager.GetComponent<T>();

                if (_instance == null)
                {
                    T component = manager.AddComponent<T>();
                    _instance = component;
                }
            }
            return _instance;
        }
    }
}
public abstract class SingletonPersistant<T> : Singleton<T> where T : MonoBehaviour, new()
{
    private void Awake() {
        Object.DontDestroyOnLoad(Object.FindObjectOfType(typeof(T)));
    }
}
