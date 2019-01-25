using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedingPostModel : HomeObject
{
    GameObject m_FeedingPostView;

    // Start is called before the first frame update
    public FeedingPostModel(GameObject feed_view)
    {
        m_FeedingPostView = feed_view;
    }

    // Update is called once per frame
    public new void Update()
    {
    }

    public Vector3 GetPosition ()
    {
        return m_FeedingPostView.transform.position;
    }
}
