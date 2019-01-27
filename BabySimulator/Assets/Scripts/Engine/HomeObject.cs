using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class HomeObject
{
    public List<GameObject> m_ContactObjects;

    public string m_ObjectName="";

    protected GameObject m_ViewObject;

    // public GameObject m_ViewObject;
    public HomeObjectView m_HomeObjectView;
    public Rigidbody m_Rigidbody;

    protected Camera m_Camera;    // path finding
    protected NavMeshPath m_CurrentPath;   // path finding
    protected float m_PivotHeight = 0.5f;     // Pivotpoint from ground

    public ConcreteAction m_CurrentConcreteAction = ConcreteAction.STANDING;

    // Start is called before the first frame update
    public HomeObject(GameObject view, string obj_name)
    {
        m_ViewObject = view;

        m_HomeObjectView = m_ViewObject.GetComponent<HomeObjectView>();
        m_Rigidbody = m_ViewObject.GetComponent<Rigidbody>();

        m_ContactObjects = new List<GameObject>();
        m_ObjectName = obj_name;

        m_Camera = Camera.main;

        m_CurrentPath = new NavMeshPath();
    }

    // Update is called once per frame
    public void Update()
    {
    }

    protected float m_WalkingSpeed = 1.3f;  // 
    protected float m_TurningSpeed = 1.3f;  // 

    Vector3 m_CurrentTarget = Vector3.negativeInfinity;
    int m_NextPathStep = 0;
    float m_PrevDist = float.MaxValue;
    public bool Walking(float stop_dist = 0.1f)
    {
        if (m_CurrentTarget.Equals(Vector3.negativeInfinity))
            m_CurrentTarget = m_CurrentPath.corners[m_NextPathStep];

        // movement
        float step = m_WalkingSpeed * GameManager.m_Instance.m_GameDeltaTime;
        m_ViewObject.transform.position = Vector3.MoveTowards(m_ViewObject.transform.position, m_CurrentTarget, step);

        // set rotation
        Vector3 target_dir = m_CurrentTarget - m_ViewObject.transform.position;
        Vector3 new_dir = Vector3.RotateTowards(m_ViewObject.transform.forward, target_dir, m_TurningSpeed * GameManager.m_Instance.m_GameDeltaTime, 0f);
        m_ViewObject.transform.rotation = Quaternion.LookRotation(new_dir);

        float dist = MagnitudeXZ(m_ViewObject.transform.position, m_CurrentTarget);

        // are we there yet? 
        if (dist < 0.1 || ((m_NextPathStep >= m_CurrentPath.corners.Length - 1) && dist < stop_dist)) // || dist > m_PrevDist)
        {
            m_Rigidbody.velocity = Vector3.zero;

            m_PrevDist = float.MaxValue;
            m_CurrentTarget = Vector3.negativeInfinity;

            if (m_NextPathStep >= m_CurrentPath.corners.Length - 1)
            {
                // This was the last corner, we are finished
                return false;
            }

            m_NextPathStep++;
            return true;
        }
        else
        {
            m_PrevDist = dist;
            return true;
        }
    }

    // We are interested in 2D distances
    float MagnitudeXZ(Vector3 a, Vector3 b)
    {
        float c = Mathf.Abs(a.x - b.x);
        float d = Mathf.Abs(a.z - b.z);

        return Mathf.Sqrt(c * c + d * d);
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

    //////////////////////////
    /// COLLISION HANDLING ///
    //////////////////////////

    /// <summary>
    /// Remove references to object's own colliders from all lists.
    /// </summary>
    public void CleanColliders()
    {
        Debug.Log("clean colliders");

        GameObject go;

        int len = m_ContactObjects.Count - 1;

        for (int i = len; i >= 0; i--)
        {
            go = m_ContactObjects[i];

            if (ReferenceEquals(go, m_ViewObject))
            {
                m_ContactObjects.Remove(go);
            }
        }
    }

    public bool ToIdling()
    { return false; }



    protected void GetPath(Vector3 target)
    {
        // :TEST: Navigation 
        // if (Input.GetMouseButtonDown(0))
        // {
        // Ray ray = m_Camera.ScreenPointToRay(target);
        // RaycastHit hit;

        m_CurrentPath = new NavMeshPath();

        // if (Physics.Raycast(ray, out hit))
        NavMesh.CalculatePath(m_ViewObject.transform.position, target, NavMesh.AllAreas, m_CurrentPath);

        // Adjust the path to current baby height
        for (int i = 0; i < m_CurrentPath.corners.Length; i++)
        {
            m_CurrentPath.corners[i].Set(m_CurrentPath.corners[i].x, m_PivotHeight, m_CurrentPath.corners[i].z);
        }

        // }
    }


}
