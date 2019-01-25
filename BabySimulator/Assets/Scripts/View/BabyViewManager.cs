using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyViewManager : PersistentSceneSingleton<BabyViewManager>
{
    public GameObject m_BabyPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void AddBaby ()
    {
        // Instantate a new cow view
        GameObject baby_view = Instantiate(m_BabyPrefab);
        baby_view.name = "Cow";
        baby_view.transform.parent = transform;
        baby_view.SetActive(true);

        // instantiate a new cow model
        BabyModel baby_eng = GameManager.m_Instance.AddBaby(baby_view);
        // link the cow model with cow view
        baby_view.GetComponent<BabyView>().Init(baby_eng);
    }
}
