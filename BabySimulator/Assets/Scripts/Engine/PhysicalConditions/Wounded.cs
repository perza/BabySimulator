using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wounds kill overtime, unless they do heal.
/// No cumulative effect overtime, but only the latest state of all wounds.
/// </summary>
public class Wounded : Condition
{
    // Start is called before the first frame update
    public Wounded(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(10, 1f);      // cow starts to get hungry after 2 hours

    }

    public override void Update()
    {
        m_LatestValue = 0;

        for (int i=0; i< m_Baby.m_Wounds.Count; i++)
        {
            m_LatestValue += m_Baby.m_Wounds[i] * GameManager.m_Instance.m_GameDeltaTime;
        }

        m_LatestValue = Mathf.Clamp(m_LatestValue, 0f, 1f);
    }
}
