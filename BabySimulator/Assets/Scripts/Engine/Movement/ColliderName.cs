using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderName : MonoBehaviour
{
    public Collider m_Collider;
    public string m_ColliderName;

    // Start is called before the first frame update
    void Start()
    {
        m_Collider.name = m_ColliderName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
