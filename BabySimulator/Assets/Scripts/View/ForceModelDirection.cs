using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceModelDirection : MonoBehaviour
{
    float lockPosition = 0;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(lockPosition, transform.rotation.eulerAngles.y, lockPosition);
    }
}
