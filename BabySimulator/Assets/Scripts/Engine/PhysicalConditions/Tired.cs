using System.Collections;
using System.Collections.Generic;

/// <summary>
/// // Walked + 10 * Runned distance, meters
/// </summary>
public class Tired : Condition
{
    // Start is called before the first frame update
    public Tired(BabyModel cow) : base(cow)
    {
        m_Condition.Add(0, 0f);
        m_Condition.Add(10, 1f);      // cow starts to get hungry after 2 hours
    }

    public override void Update()
    {
    }


}
