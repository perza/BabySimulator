using System;
using System.Collections;

public abstract class PersistentEngineSingleton<T> where T : PersistentEngineSingleton<T>
{
    public static T m_Instance;

    public PersistentEngineSingleton()
    {
        if (null == m_Instance)
        {
            m_Instance = this as T;
        }
        else
        {
            throw new System.Exception("SINGLETON CREATE RETRY " + m_Instance.ToString());
        }
    }
}