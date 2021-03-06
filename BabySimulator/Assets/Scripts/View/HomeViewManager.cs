﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BarnViewManager manages the visual models in the simulator scene
/// </summary>
[Serializable]
public class HomeViewManager : PersistentSceneSingleton<HomeViewManager>
{
    public GameObject m_Building;
    public List<GameObject> m_Walls;
    public List<GameObject> m_Fences;
    public List<GameObject> m_FeedingPosts;
    public List<GameObject> m_WateringPosts;

    public GameObject m_NannyPrefab;
    public GameObject m_NannyFolder;

    // Start is called before the first frame update
    void Start()
    {
        // Idea is that the barn is created in builder mode, which stores the elements into this game object

        // Create model objects based on the view objects  
        
        foreach(GameObject go in m_FeedingPosts)
        {
            GameManager.m_Instance.AddFeedingPost(go);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddNanny()
    {
        // Instantate a new cow view
        GameObject nanny_view = Instantiate(m_NannyPrefab);
        nanny_view.name = "Nanny";

        // for some reason persistent object can not parent the instantiated objects
        if (null == m_NannyFolder)
            m_NannyFolder = GameObject.Find("Nannies");

        nanny_view.transform.parent = m_NannyFolder.transform;
        nanny_view.SetActive(true);

        // instantiate a new cow model
        NannyModel baby_eng = GameManager.m_Instance.AddNanny(nanny_view);
        // link the cow model with cow view
        nanny_view.GetComponent<HomeObjectView>().Init(baby_eng);
    }

}
