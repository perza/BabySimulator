using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// This measure is based on time since stomach became empty. Time is given in hours.
/// 
/// Hungry is emphasized by FeedinClock
/// 
/// </summary>
public class Hungry : Condition
{
    // Start is called before the first frame update
    public Hungry(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(2, 0f);      // cow starts to get hungry after 2 hours
        m_Condition.Add(4, 0.75f);      // cow starts to get hungry after 2 hours
        m_Condition.Add(7*24, 1f);   // after seven days of starvation cow dies
    }

    protected override float GetScaledLatestValue ()
    {
        // time in hours
        return m_LatestValue / 3600f;
    }

    public void DEBUG_SetValue (float new_val)
    {
        m_LatestValue = new_val;
    }

    public override void Update()
    {
        if (0 == m_Cow.m_Stomach)
            m_LatestValue += GameManager.m_Instance.m_GameDeltaTime;

        //:NOTE: The hungry cow eats to a buffer, so that it does not get not-hungry after the first mouthful!

        // Debug.Log("HUNGRY: " + GetState());

        //:TODO: add emphasize to hunger around the regular feeding time
    }
}
