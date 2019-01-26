using UnityEngine;
using System.Collections;

public class PersistentSceneSingleton<T> : MonoBehaviour where T : PersistentSceneSingleton<T>
{
    public static T m_Instance;

    public virtual void Awake()
    {
        if (!m_Instance)
        {
            m_Instance = this as T;
            DontDestroyOnLoad(this.transform.root.gameObject);
        }
        else
        {
            // Destroy(this.transform.root.gameObject);
            throw new System.Exception ("SINGLETON CREATE RETRY " + m_Instance.name);
        }
    }
}