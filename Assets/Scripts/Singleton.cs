using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    public virtual void Awake()
    {
        if (Instance == null)
            Instance = this as T;
        else
            Debug.LogWarning("Multiple instances of " + GetType().Name, this);
    }
}