using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowColliderContact : MonoBehaviour
{
    public BoxCollider m_Collider;

    // Start is called before the first frame update
    void Start()
    {
        m_Collider.name = "C";
    }
}
