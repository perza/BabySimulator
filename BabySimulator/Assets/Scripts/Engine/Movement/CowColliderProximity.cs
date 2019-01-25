using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//:TODO: placeholder for processing collisions & handling environment
// This double collider does not necessarily work, but two separate objects could be made to 
// handle proximity sensing and collision avoidance.
public class CowColliderProximity : MonoBehaviour
{
    public SphereCollider m_Proximity;

    // Start is called before the first frame update
    void Start()
    {
        m_Proximity.name = "P";
    }
}
