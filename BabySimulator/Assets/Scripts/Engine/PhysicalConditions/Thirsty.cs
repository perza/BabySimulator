using System.Collections;
using System.Collections.Generic;

/// <summary>
/// // Time since stomach empty of water, minutes
/// </summary>
public class Thirsty : Condition
{
    // Start is called before the first frame update
    public Thirsty(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(10, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {

    }
}
