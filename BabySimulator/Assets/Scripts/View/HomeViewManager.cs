using System;
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
}
