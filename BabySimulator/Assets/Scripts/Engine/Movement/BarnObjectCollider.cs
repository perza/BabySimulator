using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnObjectCollider : MonoBehaviour
{
    // public BoxCollider m_Collider;

    public HomeObject m_BarnObjectModel;

    // Start is called before the first frame update
    void Start()
    {
        // m_Collider.name = "CowColliderInjury";

        m_BarnObjectModel = gameObject.GetComponent<HomeObjectView>().m_HomeObjectModel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision other)
    {
        ;

        switch (other.GetContact(0).thisCollider.name)
        {
            case "C": // Contact collider
                m_BarnObjectModel.TriggerContactEnter(other.gameObject);
                break;
        }
    }

    /// <summary>
    /// We tell the other object which gameobject exits the collision. 
    /// We assume that the other gameobject sends us the same info.
    /// </summary>
    /// <param name="other"></param>
    public void OnCollisionExit(Collision other)
    {
        switch (other.collider.name)
        {
            case "P": // Proximity collider
                other.gameObject.GetComponent<BarnObjectCollider>().CollisionExit("P", gameObject);
                break;
            case "C": // Contact collider
                other.gameObject.GetComponent<BarnObjectCollider>().CollisionExit("C", gameObject);
                break;
            case "I":// Injury collider
                other.gameObject.GetComponent<BarnObjectCollider>().CollisionExit("I", gameObject);
                break;
        }
    }

    public void CollisionExit(string coll_name, GameObject other)
    {
        switch (coll_name)
        {
            case "C": // Contact collider
                m_BarnObjectModel.TriggerContactExit(other);
                break;
        }
    }

}
