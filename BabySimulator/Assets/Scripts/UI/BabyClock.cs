using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BabyClock : MonoBehaviour
{
    public Text timeText;
    public Text dateText;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var _clock = Clock.m_Instance;
    
        timeText.text = HomeManager.m_Instance.getSimulationTimeAsString();
        dateText.text = HomeManager.m_Instance.getSimulationDateAsString();
    }
}
