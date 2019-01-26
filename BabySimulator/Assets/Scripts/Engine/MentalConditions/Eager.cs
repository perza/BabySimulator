using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Input:
///  - Baby is eager to do something active; baby does not have anything physical down feeling
/// </summary>
public class Eager : Condition
{
    // Start is called before the first frame update
    public Eager(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(1, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {
        List<float> temp = new List<float>
        {
            1f - m_Baby.m_Pain.GetState(),
            1f - m_Baby.m_Sick.GetState(),
            1f - m_Baby.m_Tired.GetState(),
            1f - m_Baby.m_Sleepy.GetState()
        };

        m_LatestValue = temp.Min(x => x);
    }
}
