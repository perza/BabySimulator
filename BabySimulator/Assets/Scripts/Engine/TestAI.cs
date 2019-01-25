using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAI : MonoBehaviour
{
    FuzzyMembership m_State;


    // Start is called before the first frame update
    void Start()
    {
        m_State = new FuzzyMembership();

        m_State.Add(0f, 0f);
        m_State.Add(150f, 0.4f);
        m_State.Add(350f, 0.6f);
        m_State.Add(425f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        float mem = m_State.GetMembership(200f);
        mem = m_State.GetMembership(0f);
        mem = m_State.GetMembership(150f);
        mem = m_State.GetMembership(425f);
        mem = m_State.GetMembership(600f);
        mem = m_State.GetMembership(-200f);
    }
}
