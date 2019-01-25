using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DateViewer : MonoBehaviour
{
    public Text m_Date;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_Date.text = HomeManager.m_Instance.getSimulationDateAsString();
    }
}
