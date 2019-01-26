using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyViewManager : PersistentSceneSingleton<BabyViewManager>
{
    public GameObject m_BabyPrefab;
    public GameObject m_BabyFolder;

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
        baby_view.name = "Baby";

        // for some reason persistent object can not parent the instantiated objects
        if (null == m_BabyFolder)
            m_BabyFolder = GameObject.Find("Babies");

        baby_view.transform.parent = m_BabyFolder.transform;
        baby_view.SetActive(true);

        // instantiate a new cow model
        BabyModel baby_eng = GameManager.m_Instance.AddBaby(baby_view);
        // link the cow model with cow view
        baby_view.GetComponent<BabyView>().Init(baby_eng);
    }
}
