using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeViewer : MonoBehaviour
{
    // Start is called before the first frame update
    public Text m_Time;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        m_Time.text = HomeManager.m_Instance.getSimulationTimeAsString();
    }
}
