using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NannyView : HomeObjectView
{
    public GameObject m_BabyHolder;
    public GameObject m_Baby;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private new void Update()
    {
        base.Update();

        if (null != m_Baby)
        {
            m_Baby.transform.position = m_BabyHolder.transform.position;
            m_Baby.transform.rotation = m_BabyHolder.transform.rotation;
        }
    }

    public void CarryBaby (GameObject baby)
    {
        m_Baby = baby;
        ((BabyModel)(m_Baby.GetComponent<HomeObjectView>().m_HomeObjectModel)).IsCarried = true;
    }

    public void DropBaby(GameObject target = null)
    {
        if (null != target)
        {
            // :TODO: Drop baby to another holder like feeding chair
        }
        else
        {
            ((BabyModel)(m_Baby.GetComponent<HomeObjectView>().m_HomeObjectModel)).IsCarried = false;
            m_Baby = null;
        }

    }

}
