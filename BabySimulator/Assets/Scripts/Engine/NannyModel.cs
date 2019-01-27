using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]

public class NannyModel : HomeObject
{
    GameObject m_TargetFeedingPost;

    public NannyModel(GameObject view) : base (view, "NannyModel")
    {
        m_WalkingSpeed = 2.6f;
        m_TurningSpeed = 2.6f;
    }

    enum Tasks { NONE, FEED, SLEEP, DIAPER, HUG, REMOVE }

    Tasks m_CurrentTask = Tasks.NONE;

    public new void Update ()
    {

        // If task in progress, carry on
        switch (m_CurrentTask)
        {
            case Tasks.DIAPER:
                break;
            case Tasks.FEED:
                FeedBaby();
                // catch the baby
                // find empty feeding chair
                // carry it to feeding chair
                // done
                break;
            case Tasks.HUG:
                break;
            case Tasks.REMOVE:
                // Release baby from action post
                break;
            case Tasks.SLEEP:
                break;
            case Tasks.NONE:

                if (BabyManager.m_Instance.m_Babies.Count == 0) return;

                BabyModel top_prio_baby = null; 

                // scan the babies in babymanager for most urgent need
                foreach (BabyModel baby in BabyManager.m_Instance.m_Babies)
                {
                    if (!baby.IsNannyTarget && baby.NeedsReleaseFromActionPost)
                    {
                        // :NOTE: Prioritize releasing babies from action posts in order to avoid deadlocks
                        // :NOTE: It is basically possible that nannies still get into deadlock, if they pick tasks, and action posts become filled.
                        // Nannies should be able to cancel tasks if they can not complete them
                        top_prio_baby = baby;
                        break;
                    }

                    // Do not target a baby that is already handled by another nanny
                    if (!baby.IsNannyTarget && (top_prio_baby == null || baby.m_Happy.GetState() < top_prio_baby.m_Happy.GetState()))
                    {
                        top_prio_baby = baby;
                    }
                }

                // When baby selected pick a task
                if (null != top_prio_baby)
                {
                    top_prio_baby.IsNannyTarget = true;

                    m_TargetBaby = top_prio_baby;

                    // Analyze the problem with this baby

                    switch (top_prio_baby.m_CurrentPrimaryAction)
                    {
                        case BabyModel.PrioritizedPrimaryAction.ATTACK:
                            m_CurrentTask = Tasks.HUG;
                            break;
                        case BabyModel.PrioritizedPrimaryAction.DRINK:
                            m_CurrentTask = Tasks.FEED;
                            break;
                        case BabyModel.PrioritizedPrimaryAction.EAT:
                            m_CurrentTask = Tasks.FEED;
                            break;
                        case BabyModel.PrioritizedPrimaryAction.ESCAPE:
                            m_CurrentTask = Tasks.HUG;
                            break;
                        case BabyModel.PrioritizedPrimaryAction.SLEEP:
                            m_CurrentTask = Tasks.SLEEP;
                            break;
                        case BabyModel.PrioritizedPrimaryAction.DIAPER:
                            m_CurrentTask = Tasks.DIAPER;
                            break;
                        default:
                            m_TargetBaby.IsNannyTarget = false;
                            break;
                    }

                }
                break;
        }
    }

    BabyModel m_TargetBaby;

    enum FeedingPhase { GETPATH_BABY, STAND_UP, MOVE, FEED, FINISH, CANCEL, GETPATH_FEEDINGCHAIR, MOVE_FEEDINGCHAIR, MOVE_TO_BABY };
    FeedingPhase m_FeedingPhase = FeedingPhase.GETPATH_BABY;

    /// <summary>
    ///     Description: Go to feeding place and eat
    ///     Triggers: Hunger
    ///     Concrete actions: StandUp, Walking, Eating, Pushing, Bullying
    /// </summary>
    private bool FeedBaby()
    {
        // :NOTE: that actions over several frames must be cancellable
        // :NOTE: always reset the action phase params both in completion and cancellation

        if (CancelAction())
        {
            ResetCancel();
            m_FeedingPhase = FeedingPhase.CANCEL;
        }

        // Throw exception if some phase gets into dead end
        try
        {
            switch (m_FeedingPhase)
            {
                case FeedingPhase.CANCEL:
                    throw new Exception("Eat canceled");

                case FeedingPhase.GETPATH_BABY:
                    // 1. Get path to a free feeding post
                    // Select a free feeding post
                    //      If no free feeding post, then select a feeding post with a lowest hierarchy cow
                    //      If no feeding post with lower hierachy cow, select a feeding post with a higher hierarchy cow
                    //      If still no feeding post, return false
                    // If feeding post found, get path
                    //      If no path (cow is blocked), return false

                    //List<FeedingPostModel> feed_posts = HomeManager.m_Instance.GetFeedingPosts();
                    //if (feed_posts.Count == 0)
                    //    return false;
                    GetPath(m_TargetBaby.m_HomeObjectView.transform.position);

                    // m_FeedingPhase = FeedingPhase.MOVE;
                    m_CurrentConcreteAction = ConcreteAction.WALKING;

                    // m_ActionChanged = true;

                //    break;
                //case FeedingPhase.MOVE_TO_BABY:
                    // 3. Move to feeding post

                    //:TODO: Check collision with baby as stop walking condition

                    if (!Walking(1f))
                    {
                        // pick the babe immediately so it does escape
                        PickUpBaby();

                        m_FeedingPhase = FeedingPhase.GETPATH_FEEDINGCHAIR;
                        m_CurrentConcreteAction = ConcreteAction.WALKING;
                        m_ActionChanged = true;
                        // Debug.Log("START EATING");
                    }

                    break;

                case FeedingPhase.GETPATH_FEEDINGCHAIR:

                    List<FeedingPostModel> feed_posts = HomeManager.m_Instance.GetFeedingPosts();
                    if (feed_posts.Count == 0)
                        return false;

                    m_TargetFeedingPost = feed_posts[0].m_HomeObjectView.gameObject;

                    GetPath(feed_posts[0].GetPosition());

                    m_FeedingPhase = FeedingPhase.MOVE_FEEDINGCHAIR;
                    m_CurrentConcreteAction = ConcreteAction.WALKING;

                    break;
                case FeedingPhase.MOVE_FEEDINGCHAIR:

                    if (!Walking(1f))
                    {
                        m_FeedingPhase = FeedingPhase.FEED;
                        m_CurrentConcreteAction = ConcreteAction.WALKING;
                    }

                    break;

                case FeedingPhase.FEED:
                    // 4. Put baby in chair. It will start eating.

                    if (!Feeding())
                    {
                        m_FeedingPhase = FeedingPhase.FINISH;
                        m_CurrentConcreteAction = ConcreteAction.TO_IDLING;
                    }

                    break;
                case FeedingPhase.FINISH:
                    // 5. Finish

                    if (!ToIdling())
                    {
                        ResetHungry();

                        m_FeedingPhase = FeedingPhase.GETPATH_BABY;
                        m_CurrentTask = Tasks.NONE;
                        m_CurrentConcreteAction = ConcreteAction.STANDING;

                        // m_ActionChanged = true;

                        // Debug.Log("FINISHED");
                        return true;
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            //Ensure that cow always ends up in a stable state, i.e. idling while standing or lying
            m_FeedingPhase = FeedingPhase.FINISH;
            m_CurrentConcreteAction = ConcreteAction.TO_IDLING;
        }

        // Debug.Log("EATING");

        return false;
    }

    private void PickUpBaby()
    {
        ((NannyView)(m_HomeObjectView.GetComponent<HomeObjectView>())).CarryBaby(m_TargetBaby.m_HomeObjectView.gameObject);
    }

    private bool Feeding()
    {
        ((NannyView)(m_HomeObjectView.GetComponent<HomeObjectView>())).DropBaby(m_TargetFeedingPost);
        return false;
    }

    void ResetHungry()
    {
        m_TargetBaby.IsNannyTarget = false;
        m_TargetBaby = null;
    }

    bool m_CancelAction = false;
    void ResetCancel()
    {
        m_CancelAction = false;
    }
    bool CancelAction()
    {
        return m_CancelAction;
    }



}