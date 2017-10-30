using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static GameObject container;
    private static T _instance;
    private static bool instantiated = false;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                if (instantiated == true)
                {
                    Debug.LogError("[ERROR] Prevented stack overflow from recursive call, maybe Instance was used in Awake of " + typeof(T) + "?");
                    return null;
                }

                if (container == null)
                {
                    container = SingletonContainer.GetContainer;
                }

                instantiated = true;
                _instance = container.AddComponent<T>();
            }
            return _instance;
        }

        set
        {
            if (_instance == null)
                _instance = value;
        }
    }
}

public class SingletonContainer
{
    static GameObject container;
    public static GameObject GetContainer
    {
        get
        {
            if (container == null)
            {
                container = new GameObject("SingletonContainer");
            }

            return container;
        }
    }
}
