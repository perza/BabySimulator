using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HomeObject
{
    public List<GameObject> m_ContactObjects;

    public string m_ObjectName="";

    // Start is called before the first frame update
    public HomeObject(string obj_name)
    {
        m_ContactObjects = new List<GameObject>();
        m_ObjectName = obj_name;
    }

    // Update is called once per frame
    public void Update()
    {
    }

    public enum ConcreteAction
    {
        LYING,
        STANDING,
        TO_IDLING,          // Return to idling from any state. This means returning to LYING or STANDING, depending on the m_CurrentBasePose
        WALKING,
        RUNNING,
        NAPPING,            // Sleeping while standing
        DYING,
        PUSHING,
        PUSHED,
        BULLYING,
        STAGGERING,
        SLEEPING,           // Sleeping while lying
        STAND_UP,
        LIE_DOWN,
        TURN_LEFT,
        TURN_RIGHT,
        EATING,
        DRINKING
    };
       
    // A cow view subscribes to cow model to follow the changes in actions
    public event ActionChangeHandler mActionChange;
    public delegate void ActionChangeHandler(ConcreteAction act, float val);

    // Inform possible observers (view) of the new action state start
    public void NotifyStateChangeObservers(ConcreteAction act, float val = 0)
    {
        if (null != mActionChange)
            mActionChange.Invoke(act, val);
    }

    public bool m_ActionInterrupted = false;
    public bool m_ActionChanged = false;

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
