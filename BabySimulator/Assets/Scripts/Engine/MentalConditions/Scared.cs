using System.Collections;
using System.Collections.Generic;

/// <summary>
/// // time since scary event AND scaryness of the event
/// </summary>
public class Scared : Condition
{
    // Start is called before the first frame update
    public Scared(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(10, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {
    }
}
