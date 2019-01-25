using System.Collections;
using System.Collections.Generic;

/// <summary>
/// // Sick OR Wounded OR Hungry OR Sleepy > 0, membership
/// </summary>
public class Pain : Condition
{
    public Pain(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0);
        m_Condition.Add(1, 1);
    }

    public override void Update()
    {

    }
}
