using System.Collections;
using System.Collections.Generic;

/// <summary>
/// // Raises for events like milking, feeding, sleeping, unless some Pain or similar 
                              // interferes. Wears off.
/// 
/// </summary>
public class Happy : Condition
{
    // Start is called before the first frame update
    public Happy(BabyModel cow) : base(cow)
    {
    }

    public override void Update()
    {
    }


}
