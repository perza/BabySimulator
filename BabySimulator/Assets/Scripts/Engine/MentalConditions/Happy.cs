using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// // Raises for events like milking, feeding, sleeping, unless some Pain or similar 
// interferes. Wears off.
/// 
/// </summary>
public class Happy : Condition
{
    // Start is called before the first frame update
    public Happy(BabyModel baby) : base(baby)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(1, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {
        // Happyness is average of positive and 1-negative physical and mental conditions

        //float aver = 1f - m_Baby.m_Hungry.GetState() +  // 1
        //             1f - m_Baby.m_Lonely.GetState() +  // 2
        //             1f - m_Baby.m_Pain.GetState() +    // 3
        //             1f - m_Baby.m_Sick.GetState() +    // 4
        //             1f - m_Baby.m_Thirsty.GetState() + // 5
        //             1f - m_Baby.m_Tired.GetState() +   // 6
        //             1f - m_Baby.m_Sleepy.GetState();   // 7

        // Happiness is 1-max of the negative feelings

        List<float> temp = new List<float>
        {
            1f - m_Baby.m_Hungry.GetState(),
            1f - m_Baby.m_Lonely.GetState(),
            1f - m_Baby.m_Pain.GetState(),
            1f - m_Baby.m_Sick.GetState(),
            1f - m_Baby.m_Thirsty.GetState(),
            1f - m_Baby.m_Tired.GetState(),
            1f - m_Baby.m_Sleepy.GetState()
        };

        m_LatestValue = temp.Min(x => x);
    }
}
