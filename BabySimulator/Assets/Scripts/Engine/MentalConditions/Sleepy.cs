using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 
/// // Time since full sleep. Preventing sleep for a week kills the cow. 
// Single full sleep resets the sleepiness to 0. Nap decreases counter linearly.
// Non full sleep decreases the counter percentually. Time since full sleep, hours.
/// </summary>
public class Sleepy : Condition
{
    // Start is called before the first frame update
    public Sleepy(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(10, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {
    }


}
