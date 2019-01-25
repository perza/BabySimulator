using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class manages the building and machinery
/// </summary>
public class HomeManager : PersistentEngineSingleton<HomeManager>
{
    List<HomeObject> m_StaticBarnObjects; // items that do not change over time: walls, fencess, etc.
    List<HomeObject> m_DynamicBarnObjects; // items that change over time
    List<FeedingPostModel> m_FeedingPostModels;

    // m_BarnClock tells the date and time of simulation
    Clock m_BarnClock;
    // m_BarnClockEvents list of events that may be repeating, like feeding times
    List<ClockEvent> m_BarnClockEvents;

    // Start is called before the first frame update
    public HomeManager()
    {
        m_StaticBarnObjects = new List<HomeObject>();
        m_DynamicBarnObjects = new List<HomeObject>();
        m_FeedingPostModels = new List<FeedingPostModel>();

        m_BarnClock = new Clock();
    }

    float time_since_view_clock_update = 0;
    Clock.Date barn_date;
    Clock.Time barn_time;


    public void Update()
    {
        foreach (HomeObject dynamic_barn_object in m_DynamicBarnObjects)
        {
            dynamic_barn_object.Update();
        }

        m_BarnClock.Update();

        time_since_view_clock_update += GameManager.m_Instance.m_GameDeltaTime;

        if (time_since_view_clock_update > 60f)
        {
            time_since_view_clock_update = 0;
            barn_date = m_BarnClock.CurrentDate;
            barn_time = m_BarnClock.CurrentTime;
        }
    }

    public string getSimulationDateAsString ()
    {
        return m_BarnClock.CurrentDate.m_Year.ToString() + "." + m_BarnClock.CurrentDate.m_Month.ToString() + "." + m_BarnClock.CurrentDate.m_Day.ToString();
    }

    public string getSimulationTimeAsString()
    {
        return m_BarnClock.CurrentTime.m_Hour.ToString() + ":" + m_BarnClock.CurrentTime.m_Minute.ToString();
    }

    public FeedingPostModel AddDynamicObject(GameObject feed_view)
    {
        switch (feed_view.name)
        {
            case "FeedingPost":
                m_DynamicBarnObjects.Add(new FeedingPostModel(feed_view));
                m_FeedingPostModels.Add((FeedingPostModel)m_DynamicBarnObjects[m_DynamicBarnObjects.Count - 1]);
                return (FeedingPostModel)m_DynamicBarnObjects[m_DynamicBarnObjects.Count - 1];
        }

        return null;
    }

    public List<FeedingPostModel> GetFeedingPosts ()
    {
        return m_FeedingPostModels;
    }

}
