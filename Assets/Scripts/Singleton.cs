using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Current { get; private set; }

    public virtual void Awake()
    {
        if (Current == null)
            Current = this as T;
        else
            OnMultipleInstance();
    }

    /// <summary>
    /// Called when another singleton already exists before this script.
    /// </summary>
    protected virtual void OnMultipleInstance()
    {
        Debug.LogWarning("Multiple instances of " + GetType().Name, this);
    }
}