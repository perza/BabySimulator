using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedingPostModel : HomeObject
{
    // Start is called before the first frame update
    public FeedingPostModel(GameObject feed_view) : base (feed_view, "FeedingPost")
    {
    }

    // Update is called once per frame
    public new void Update()
    {
    }

    public Vector3 GetPosition ()
    {
        return m_ViewObject.transform.position;
    }
}
