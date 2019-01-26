using System.Collections;
using System.Collections.Generic;

/// <summary>
/// // Hungry, Thirsty, Sick, Wound OR Sleepy = 1, membership
/// </summary>
public class Dead : Condition
{
    // Start is called before the first frame update
    public Dead(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(10, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {

    }

}
