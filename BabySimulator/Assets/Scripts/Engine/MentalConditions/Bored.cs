using System.Collections;
using System.Collections.Generic;

/// <summary>
/// // Time since conflict event?
/// </summary>
public class Bored : Condition
{
    // Start is called before the first frame update
    public Bored(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(10, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {
    }


}
