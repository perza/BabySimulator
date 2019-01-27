using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

/// <summary>
/// 
/// GameManager instantiates the BarnManager and CowManager, as well as respective views.
/// 
/// It acts as interface between view/builder tool and the simulator engine
/// 
/// </summary>
public class GameManager : PersistentSceneSingleton<GameManager>
{
    BabyManager m_BabyManager;
    HomeManager m_HomeManager;

    public float m_GameDeltaTime;

    public float m_GameSpeed;

    private int _earlierDate;
       
    // Start is called before the first frame update
    void Start()
    {
        m_BabyManager = new BabyManager ();
        m_HomeManager = new HomeManager ();

        m_GameSpeed = 1f;

        _earlierDate = Clock.m_Instance.CurrentDate.m_Day;
    }

    public BabyModel AddBaby (GameObject cow_view)
    {
        return m_BabyManager.AddBaby(cow_view);
    }

    public NannyModel AddNanny(GameObject cow_view)
    {
        return m_HomeManager.AddNanny(cow_view);
    }

    public FeedingPostModel AddFeedingPost(GameObject feed_post)
    {
        return m_HomeManager.AddDynamicObject(feed_post);
    }

    // Update is called once per frame
    void Update()
    {
        m_GameDeltaTime = Time.deltaTime * m_GameSpeed;

        m_HomeManager.Update();
        m_BabyManager.Update();
        
        CheckForIncome();
    }

    private void CheckForIncome()
    {
        if (_earlierDate == Clock.m_Instance.CurrentDate.m_Day) return;
        var cash = 32 * m_BabyManager.m_Babies.Count;
        ResourceHandler.Instance.GainMoney(cash);
        _earlierDate = Clock.m_Instance.CurrentDate.m_Day;
    }
}
