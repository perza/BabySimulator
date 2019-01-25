using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Damage
/// 
/// Damage measures the acute, on-going physical injury while it is inflicted to the cow.
/// 
/// It increases as sum of severity and time
/// 
/// 
/// </summary>
public class Damage : Condition
{
    // 
    public Damage(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(10, 1f);      // cow starts to get hungry after 2 hours
    }

    protected override float GetScaledLatestValue()
    {
        // time in hours
        return m_LatestValue / 3600f;
    }

    public void DEBUG_SetValue(float new_val)
    {
        m_LatestValue = new_val;
    }

    float m_DamageTime = 0;

    float m_DamageDepth = 0f;

    public override void Update()
    {
    }
}
