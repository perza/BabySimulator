using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 
/// // time spend without close contact with other cows, minutes
/// </summary>
public class Lonely : Condition
{
    float m_TimeSinceContact = 0;

    // Start is called before the first frame update
    public Lonely(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(1800f, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {
        // Time since over n meters away from another baby or nurse

        if (m_Baby.m_HomeObjectView.m_ProximityObjects.Count > 0)
        {
            m_TimeSinceContact = 0;
        }
        else
        {
            m_TimeSinceContact += GameManager.m_Instance.m_GameDeltaTime * Clock.m_Instance.m_DayAcceleration;
        }

        m_LatestValue = m_TimeSinceContact;
    }


}
