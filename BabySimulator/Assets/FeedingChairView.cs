using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedingChairView : HomeObjectView
{

    public GameObject m_BabyHolder;
    public GameObject m_Baby;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    new void Update()
    {
        if (null != m_Baby)
        {
            m_Baby.transform.position = m_BabyHolder.transform.position;
            m_Baby.transform.rotation = m_BabyHolder.transform.rotation;
        }

    }

    public void CarryBaby(GameObject baby)
    {
        m_Baby = baby;
        ((BabyModel)(m_Baby.GetComponent<HomeObjectView>().m_HomeObjectModel)).IsCarried = true;

        ((BabyModel)(m_Baby.GetComponent<HomeObjectView>().m_HomeObjectModel)).InFeedingChair = true;

    }

    public void DropBaby(GameObject target = null)
    {
        ((BabyModel)(m_Baby.GetComponent<HomeObjectView>().m_HomeObjectModel)).IsCarried = false;
        ((BabyModel)(m_Baby.GetComponent<HomeObjectView>().m_HomeObjectModel)).InFeedingChair = false;
        m_Baby = null;

    }

}
