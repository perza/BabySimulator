using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HomeObject
{
    public List<GameObject> m_ContactObjects;

    // Start is called before the first frame update
    public HomeObject()
    {
        m_ContactObjects = new List<GameObject>();
    }

    // Update is called once per frame
    public void Update()
    {
    }

    public void TriggerContactEnter(GameObject other)
    {
        m_ContactObjects.Add(other);
        Debug.Log("ContactEnter: " + other.name);
    }

    public void TriggerContactExit(GameObject other)
    {
        m_ContactObjects.Remove(other);
        Debug.Log("ContactExit: " + other.name);
    }
}
