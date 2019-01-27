using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class BabyModel : HomeObject
{
    // m_CowView is used as location storage, needed for navigation and collisions

    // Wellness - Physical conditions
    public Pain m_Pain;         // Cow is sudffering from pain due some physical condition
    public Sick m_Sick;
    public Wounded m_Wounded;   // Cow has a physical injury
    public Damage m_Damage;     // We are getting injured
    public Tired m_Tired;
    public Hungry m_Hungry;
    public Thirsty m_Thirsty;
    public Dead m_Dead;

    // Contentment - Mental conditions
    public Angry m_Anger;
    public Sleepy m_Sleepy;
    public Scared m_Scared;
    public Bored m_Bored;
    public Lonely m_Lonely;
    public Happy m_Happy;
    public Eager m_Eager;

    // Cow physic params
    public float m_Stomach = 0; // fill rate, 0=empty, 1=full
    float m_StomachBuffer = 0f; // cow eats first to buffer, then when either buffer full or eating interrupted, 
                                // buffer is moved to stomach

    // list of all the wounds the cow is suffering
    // :TODO: add healing/dying based on the wounds
    public List<float> m_Wounds = new List<float>();
    float m_HealingSpeed = 0.01f;

    // cow constants
    const float DAILY_FORAGE = 20f; // kg

    static int m_CowHierachyPositions = 0; // static counter for cows is used as the initial cow position in herd hierarchy. Priority 0 is the herd leader.
    int m_CowHierarchyPosition = 0; // cow position in herd hierarchy
                                    //:NOTE: CowManager holds list of all cows, and separate ref the herd leader

    // Baby is carried by nurse
    bool m_IsCarried = false;
    public bool IsCarried
    {
        get
        {
            return m_IsCarried;
        }
        set
        {
            if (value)
            {
                m_IsCarried = true;
                m_HomeObjectView.gameObject.GetComponentInChildren<BoxCollider>().enabled = false;
                m_CancelAction = true;
            }
            else
            {
                m_HomeObjectView.gameObject.GetComponentInChildren<BoxCollider>().enabled = true;
                m_IsCarried = false;
            }
        }
    }

    // Baby is targeted by nanny
    public bool IsNannyTarget = false;

    // These babies are prioritized to avoid deadlocks (i.e. action posts are reserved, and nannies can not get tasks completed)
    // :NOTE: It is basically possible that nannies still get into deadlock, if they pick tasks, and action posts become filled.
    // Nannies should be able to cancel tasks if they can not complete them
    public bool NeedsReleaseFromActionPost = false;

    // List of action positions
    public bool InFeedingChair = false;
    public bool InBed = false;
    public bool InDiaperChange = false;
    public bool InLap = false;


    // :NOTE: keep these in priority order for inferencing
    public enum PrioritizedPrimaryAction
    {
        DIE,       // Highest priority
        ESCAPE,
        ATTACK,
        DRINK,
        EAT,
        SLEEP,
        EXPLORE,
        DIAPER, // needs diaper change: stay still and cry
        PLAY,
        IDLE       // Lowest priority
    };

    public PrioritizedPrimaryAction m_CurrentPrimaryAction = PrioritizedPrimaryAction.IDLE;
    float[] m_PrimaryActionValues;

    // Current pose tells the starting position
    // We need to know these before starting any new animation, so that when starting walking,
    // the cow knows to first raise up.
    // We do NOT want to stop any earlier action by returning to default standing pose.
    public enum CurrentBasePose { LYING, STANDING };
    public CurrentBasePose m_CurrentBasePose = CurrentBasePose.STANDING;

    float m_BoostPerLevel;
    const float MAX_PRIO_BOOST = 0.5f;

    // Start is called before the first frame update
    public BabyModel(GameObject baby_view) :  base (baby_view, "BabyModel")
    {
        m_WalkingSpeed = 1.3f;
        m_TurningSpeed = 1.3f;

        m_CowHierarchyPosition = m_CowHierachyPositions;
        m_CowHierachyPositions++;

        m_PrimaryActionValues = new float[Enum.GetNames(typeof(PrioritizedPrimaryAction)).Length];

        m_BoostPerLevel = MAX_PRIO_BOOST / m_PrimaryActionValues.Length;

        GenerateConditions();

        NotifyStateChangeObservers(ConcreteAction.STANDING);

        //:TEST:
        //m_Hungry.DEBUG_SetValue(2f * 3600f);

    }

    // Update is called once per frame
    public new void Update()
    {
        PrioritizedPrimaryAction new_primary_action = PrioritizedPrimaryAction.IDLE;

        // Update physic params
        UpdatePhysicParams();

        // Update conditions
        UpdatePhysicalConditions();
        UpdateMentalConditions();

        if (m_IsCarried)
        {
            // Not move anything when carried
            // :TODO: The methods such as Eat or Sleep must be called directly 
            // :NOTE: there is a pending cancel in place, which will cancel any
            // Action that was interrupted by carrying begin.

            return;
        }

        if (m_ActionInterrupted)
        {
            // We do not start a new action until the previous is gracefully exited.
            // Each primary action must have a separate method for Interruption.
            return;
        }
        else
        {
            // Choose primary action
            new_primary_action = ActionInference();

            // Choose concrete action
            if (m_CurrentPrimaryAction != new_primary_action)
            {
                m_ActionInterrupted = InterruptCurrentAction(m_CurrentPrimaryAction);
                m_ActionChanged = true;
                m_CurrentPrimaryAction = new_primary_action;

                return;
            }

            //:NOTE: the current action may be continuing from the previous frame, or starting from beginning.
            // The action state is checked inside the action method.
            ExecuteAction(m_CurrentPrimaryAction);

            // :BUG: actionachanged must be concrete action!

            if (m_ActionChanged)
            {
                NotifyStateChangeObservers(m_CurrentConcreteAction);
                m_ActionChanged = false;
            }
        }
    }

    // Update physic params
    void UpdatePhysicParams()
    {
        if (m_Stomach > 0)
            m_Stomach -= GameManager.m_Instance.m_GameDeltaTime / 3600f; // full stomach becomes empty in one hour

        for (int i = 0; i < m_Wounds.Count; i++)
        {
            m_Wounds[i] -= m_HealingSpeed * GameManager.m_Instance.m_GameDeltaTime;
        }
    }

    // Update conditions
    void UpdatePhysicalConditions()
    {
        // Wellness - Physical conditions
        m_Pain.Update();
        m_Sick.Update();
        m_Wounded.Update();
        m_Damage.Update();
        m_Tired.Update();
        m_Hungry.Update();
        m_Thirsty.Update();
        m_Dead.Update();
    }
    void UpdateMentalConditions()
    {
        // Contentment - Mental conditions
        m_Anger.Update();
        m_Sleepy.Update();
        m_Scared.Update();
        m_Bored.Update();
        m_Lonely.Update();
        m_Happy.Update();
        m_Eager.Update();
    }

    PrioritizedPrimaryAction ActionInference()
    {
        //:TODO: not all these need to be evaluated in every frame. Most could be done
        // e.g. once per minute or even less. 
        // However, Escape and Attack need to have quick reponses, so those could be evaluated 
        // e.g. once per second.

        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.ATTACK] = InferAttack();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.DIE] = InferDie();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.DRINK] = InferDrink();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.EAT] = InferEat();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.ESCAPE] = InferEscape();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.EXPLORE] = InferExplore();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.PLAY] = InferPlay();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.IDLE] = InferIdle();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.SLEEP] = InferSleep();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.SLEEP] = InferDiaper();

        int selected = 0;

        // higher priority actions get boosted in comparison to lower priority actions. Otherwise
        // lower priority action might get launched in close call cases, which is not realistic.
        // E.g. if cow is starving, it should not go to sleep, even if the sleep action had a bit higher
        // fuzzy value.
        for (int i = 1; i < m_PrimaryActionValues.Length; i++)
        {
            if (m_PrimaryActionValues[i] > m_PrimaryActionValues[selected] * (1f + (i - selected) * m_BoostPerLevel))
                selected = i;
        }

        return (PrioritizedPrimaryAction)selected;
    }

    bool InterruptCurrentAction(PrioritizedPrimaryAction action)
    {
        return false;
    }

    void ExecuteAction(PrioritizedPrimaryAction action)
    {
        switch (action)
        {
            case PrioritizedPrimaryAction.ATTACK:
                Attack();
                break;
            case PrioritizedPrimaryAction.DIE:
                Die();
                break;
            case PrioritizedPrimaryAction.DRINK:
                Drink();
                break;
            case PrioritizedPrimaryAction.EAT:
                Eat();
                break;
            case PrioritizedPrimaryAction.ESCAPE:
                Escape();
                break;
            case PrioritizedPrimaryAction.EXPLORE:
                Explore();
                break;
            case PrioritizedPrimaryAction.PLAY:
                Play();
                break;
            case PrioritizedPrimaryAction.IDLE:
                Idle();
                break;
            case PrioritizedPrimaryAction.SLEEP:
                Sleep();
                break;
            case PrioritizedPrimaryAction.DIAPER:
                Diaper();
                break;
        }

    }

    /// <summary>
    /// This generates the conditions of the new cow
    /// </summary>
    private void GenerateConditions()
    {
        //:TODO: add individual variation into fuzzy distributions

        // Wellness - Physical conditions
        m_Pain = new Pain(this);
        m_Sick = new Sick(this);
        m_Wounded = new Wounded(this);
        m_Damage = new Damage(this);
        m_Tired = new Tired(this);
        m_Hungry = new Hungry(this);
        m_Thirsty = new Thirsty(this);
        m_Dead = new Dead(this);

        // Contentment - Mental conditions
        m_Anger = new Angry(this);
        m_Sleepy = new Sleepy(this);
        m_Scared = new Scared(this);
        m_Bored = new Bored(this);
        m_Lonely = new Lonely(this);
        m_Happy = new Happy(this);
        m_Eager = new Eager(this);
    }


    ///////////////////////
    /// PRIMARY ACTIONS ///
    ///////////////////////

    /// <summary>
    ///     Description: Cow is idle when no other action is triggered
    ///     Triggers: No triggers, default action
    ///     Concrete actions: Standing/Lying
    /// </summary>
    public void Idle()
    { }

    enum EatingPhase { GETPATH, STAND_UP, MOVE, EAT, FINISH, CANCEL };
    EatingPhase m_EatingPhase = EatingPhase.GETPATH;

    /// <summary>
    ///     Description: Go to feeding place and eat
    ///     Triggers: Hunger
    ///     Concrete actions: StandUp, Walking, Eating, Pushing, Bullying
    /// </summary>
    public bool Eat()
    {
        // :NOTE: that actions over several frames must be cancellable
        // :NOTE: always reset the action phase params both in completion and cancellation

        if (CancelAction())
        {
            ResetCancel();
            m_EatingPhase = EatingPhase.CANCEL;
        }

        // Throw exception if some phase gets into dead end
        try
        {
            switch (m_EatingPhase)
            {
                case EatingPhase.CANCEL:
                    throw new Exception("Eat canceled");

                case EatingPhase.GETPATH:
                    // 1. Get path to a free feeding post
                    // Select a free feeding post
                    //      If no free feeding post, then select a feeding post with a lowest hierarchy cow
                    //      If no feeding post with lower hierachy cow, select a feeding post with a higher hierarchy cow
                    //      If still no feeding post, return false
                    // If feeding post found, get path
                    //      If no path (cow is blocked), return false

                    List<FeedingPostModel> feed_posts = HomeManager.m_Instance.GetFeedingPosts();
                    if (feed_posts.Count == 0)
                        return false;
                    GetPath(feed_posts[0].GetPosition());

                    // Debug.Log("GET PATH");
                    if (m_CurrentBasePose == CurrentBasePose.LYING)
                    {
                        m_EatingPhase = EatingPhase.STAND_UP;
                        m_CurrentConcreteAction = ConcreteAction.STAND_UP;
                        // Debug.Log("START STAND_UP");
                    }
                    else
                    {
                        m_EatingPhase = EatingPhase.MOVE;
                        m_CurrentConcreteAction = ConcreteAction.WALKING;
                        // Debug.Log("START WALKING");
                    }


                    m_ActionChanged = true;

                    break;
                case EatingPhase.STAND_UP:
                    // 2. If lying, StandUp
                    if (!StandUp())
                    {
                        m_CurrentBasePose = CurrentBasePose.STANDING;
                        m_EatingPhase = EatingPhase.MOVE;
                        m_CurrentConcreteAction = ConcreteAction.WALKING;
                        m_ActionChanged = true;
                        // Debug.Log("START WALKING");
                    }

                    break;
                case EatingPhase.MOVE:
                    // 3. Move to feeding post

                    if (!Walking())
                    {
                        m_EatingPhase = EatingPhase.EAT;
                        m_CurrentConcreteAction = ConcreteAction.EATING;
                        m_ActionChanged = true;
                        // Debug.Log("START EATING");
                    }

                    break;
                case EatingPhase.EAT:
                    // 4. Eat

                    if (!Eating())
                    {
                        m_EatingPhase = EatingPhase.FINISH;
                        m_CurrentConcreteAction = ConcreteAction.TO_IDLING;
                        m_ActionChanged = true;
                        // Debug.Log("START FINISH");
                    }

                    break;
                case EatingPhase.FINISH:
                    // 5. Finish

                    if (!ToIdling())
                    {
                        ResetHungry();

                        m_EatingPhase = EatingPhase.GETPATH;
                        m_CurrentPrimaryAction = PrioritizedPrimaryAction.IDLE;
                        m_CurrentConcreteAction = ConcreteAction.STANDING;
                        m_ActionChanged = true;

                        // Debug.Log("FINISHED");
                        return true;
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            //Ensure that cow always ends up in a stable state, i.e. idling while standing or lying
            m_EatingPhase = EatingPhase.FINISH;
            m_CurrentConcreteAction = ConcreteAction.TO_IDLING;
        }

        // Debug.Log("EATING");

        return false;
    }

    void ResetHungry()
    {
        //:BUG: this means that even smallest amount of food resets the hunger for several hours.
        // Change this so that content feeling lasts some percentual amount of max time, depending 
        // on the stomach fill rate at the end of feeding.
        // As a matter of fact, the Hunger should depend on the amount of food eaten, not only on time since eaten
        if (m_StomachBuffer > 0)
        {
            m_Hungry.Reset();
        }

        m_Stomach = m_StomachBuffer;
        m_StomachBuffer = 0;
    }

    //:TODO: Ensure that cow always ends up in a stable state, i.e. idling while standing or lying
    bool ReturnToDefaultState()
    {
        return true;
    }

    /// <summary>
    ///     Description: Go to watering place and drink
    ///     Triggers: Thirst.Assumption is that there is always water available, i.e.no specific watering times.
    ///     Concrete actions: Walking, Drinking, Pushing, Bullying
    /// </summary>
    public void Drink()
    { }

    /// <summary>
    /// </summary>
    public void Milk()
    { }

    /// <summary>
    /// </summary>
    public void Attack()
    {
        // fight over a toy

    }

    /// <summary>
    ///     Description: Run away from the cause of fear.
    ///     Triggers: Scared.Note, Scared with no way to escape may convert to Angry.
    ///     Concrete actions: Running
    /// </summary>
    public void Escape()
    { }

    enum ExplorePhase { GETPATH, MOVE, FINISH, CANCEL };
    ExplorePhase m_ExplorePhase = ExplorePhase.GETPATH;

    /// <summary>
    ///      Description: Walk around studying place.
    ///      Triggers: Bored
    ///      Concrete actions: Walking
    /// </summary>
    public bool Explore()
    {

    /// <summary>
    ///     Description: Go to feeding place and eat
    ///     Triggers: Hunger
    ///     Concrete actions: StandUp, Walking, Eating, Pushing, Bullying
    /// </summary>
        // :NOTE: that actions over several frames must be cancellable
        // :NOTE: always reset the action phase params both in completion and cancellation

        if (CancelAction())
        {
            ResetCancel();
            m_ExplorePhase = ExplorePhase.CANCEL;
        }

        // Throw exception if some phase gets into dead end
        try
        {
            switch (m_ExplorePhase)
            {
                case ExplorePhase.CANCEL:
                    throw new Exception("Eat canceled");

                case ExplorePhase.GETPATH:
                    // 1. Get path to a free feeding post
                    // Select a free feeding post
                    //      If no free feeding post, then select a feeding post with a lowest hierarchy cow
                    //      If no feeding post with lower hierachy cow, select a feeding post with a higher hierarchy cow
                    //      If still no feeding post, return false
                    // If feeding post found, get path
                    //      If no path (cow is blocked), return false


                    // Generate random point within the game area.

                    Vector3 low_left = new Vector3(0,0,0), high_top = new Vector3(0, 0, 0);

                    HomeManager.m_Instance.GetBounds(ref low_left, ref high_top);

                    float x = UnityEngine.Random.Range(low_left.x, high_top.x);
                    float z = UnityEngine.Random.Range(low_left.z, high_top.z);
                    float y = 0.5f;

                    //List<FeedingPostModel> feed_posts = HomeManager.m_Instance.GetFeedingPosts();
                    //if (feed_posts.Count == 0)
                    //    return false;
                    GetPath(new Vector3(x, y, z)); // feed_posts[0].GetPosition());

                    // Debug.Log("GET PATH");
                    //if (m_CurrentBasePose == CurrentBasePose.LYING)
                    //{
                    //    m_ExplorePhase = ExplorePhase.STAND_UP;
                    //    m_CurrentConcreteAction = ConcreteAction.STAND_UP;
                    //    // Debug.Log("START STAND_UP");
                    //}
                    //else
                    //{
                        m_ExplorePhase = ExplorePhase.MOVE;
                        m_CurrentConcreteAction = ConcreteAction.WALKING;
                        // Debug.Log("START WALKING");
                    //}


                    m_ActionChanged = true;

                    break;
                //case ExplorePhase.STAND_UP:
                //    // 2. If lying, StandUp
                //    if (!StandUp())
                //    {
                //        m_CurrentBasePose = CurrentBasePose.STANDING;
                //        m_EatingPhase = EatingPhase.MOVE;
                //        m_CurrentConcreteAction = ConcreteAction.WALKING;
                //        m_ActionChanged = true;
                //        // Debug.Log("START WALKING");
                //    }

                //    break;
                case ExplorePhase.MOVE:
                    // 3. Move to feeding post

                    if (!Walking())
                    {
                        m_ExplorePhase = ExplorePhase.FINISH;
                        m_CurrentConcreteAction = ConcreteAction.TO_IDLING;
                        m_ActionChanged = true;
                        // Debug.Log("START EATING");
                    }

                    break;
                //case ExplorePhase.EAT:
                //    // 4. Eat

                //    if (!Eating())
                //    {
                //        m_EatingPhase = EatingPhase.FINISH;
                //        m_CurrentConcreteAction = ConcreteAction.TO_IDLING;
                //        m_ActionChanged = true;
                //        // Debug.Log("START FINISH");
                //    }

                //    break;
                case ExplorePhase.FINISH:
                    // 5. Finish

                    if (!ToIdling())
                    {
                        ResetHungry();

                        m_ExplorePhase = ExplorePhase.GETPATH;
                        m_CurrentPrimaryAction = PrioritizedPrimaryAction.IDLE;
                        m_CurrentConcreteAction = ConcreteAction.STANDING;
                        m_ActionChanged = true;

                        // Debug.Log("FINISHED");
                        return true;
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            //Ensure that cow always ends up in a stable state, i.e. idling while standing or lying
            m_ExplorePhase = ExplorePhase.FINISH;
            m_CurrentConcreteAction = ConcreteAction.TO_IDLING;
        }

        // Debug.Log("EATING");

        return false;
    }


    /// <summary>
    ///      Description: Walk around studying place.
    ///      Triggers: Bored
    ///      Concrete actions: Walking
    /// </summary>
    public void Play()
    {
        // get list of toys from housemanager

        // select closest free toy, go there and play

        // if not free toys, select closest reserved toy and start fighting over it

        // if no toys at all, start crying with toy bubble
    }

    /// <summary>
    ///     Description: Cow seeks isolation from the herd
    ///     Triggers: Anguished
    ///     Concrete actions: Walking, Standing/Lying
    /// </summary>
    public void Solitude()
    { }

    /// <summary>
    ///     Description: Search a nice place within the herd and lay down for sleeping.
    ///     Triggers: Sleepy.
    ///     Concrete actions: Walking, Lying, Sleeping
    /// </summary>
    public void Sleep()
    {
        // reduce tiredness to 0, then wake up in idle
    }

    public void Diaper()
    {
        // reduce tiredness to 0, then wake up in idle
    }

    /// <summary>
    /// Description: Die by starvation, sickness or wound.
    /// Triggers: Max(Hunger, Sick, Wound) = 1.
    /// Concrete actions: Standing/Lying, Dying
    /// </summary>
    public void Die()
    { }

    ///////////////////////
    /// CONCRETE ACTIONS ///
    ///////////////////////

    public bool Idling()
    { return false; }

    public void Lying()
    { }

    public void Standing()
    { }

    bool m_CancelAction = false;
    void ResetCancel()
    {
        m_CancelAction = false;
    }
    bool CancelAction()
    {
        return m_CancelAction;
    }


    /// <summary>
    /// 
    /// Returns true if an object is blocking our path to the target.
    /// 
    /// </summary>
    /// <param name="target"> target tells the direction we are currently heading</param>
    /// <returns></returns>
    bool IsBlocked(Vector3 target)
    {
        // Find direction to the closest point, or the collision point

        // If the direction is 


        return false;
    }

    public void TurnLeft()
    { }

    public void TurnRight()
    { }

    public void Pushing()
    { }

    public void Staggering()
    { }

    public void Running()
    { }

    public void Pushed()
    { }

    public bool StandUp()
    { return false; }

    public void LieDown()
    { }

    public void Sleeping()
    { }

    public void Dying()
    { }

    public void Mooing()
    { }

    public void Milking()
    { }

    public bool Eating()
    {
        //:TODO: make actual implementation instead of this test

        //:TODO: flush the stomach buffer if eating is interrupted.

        //:NOTE: that as the cow eats to buffer first, it can die to starvation while eating (if near starvation when starting to eat). 
        // This is kind of natural, as the food never affects immediately to a starving animal.


        // Check that baby is positioned into feeding chair

        if (!InFeedingChair)
            return true;

        m_StomachBuffer += GameManager.m_Instance.m_GameDeltaTime / 1800f;

        if (m_StomachBuffer >= 1f)
            return false;
        else
            return true;
    }

    public void Drinking()
    { }

    public void Ruminating()
    { }


    ///////////////////////////////////////////
    /// INFERENCE RULES FOR PRIMARY ACTIONS ///
    ///////////////////////////////////////////

    float InferAttack() { return 0f; }
    float InferDie() { return 0f; }
    float InferDrink() { return 0f; }
    float InferEat()
    {
        return 1f;

        return m_Hungry.GetState();
    }
    float InferEscape() { return 0f; }
    float InferExplore()
    {
        // If nothing to complain, choose a random location and move there
        if (m_Happy.GetState() < 0.5f &&
            m_Eager.GetState() < 0.5f &&
            m_Hungry.GetState() < 0.25f &&
            m_Lonely.GetState() < 0.05f &&
            m_Pain.GetState() < 0.05f &&
            m_Scared.GetState() < 0.05f &&
            m_Sick.GetState() < 0.05f &&
            m_Sleepy.GetState() < 0.05f &&
            m_Thirsty.GetState() < 0.05f &&
            m_Tired.GetState() < 0.05f)
            return 1f;

        return 0f;
    }
    float InferIdle() { return 0f; }
    float InferSleep() { return 0f; }
    float InferDiaper() { return 0f; }
    float InferPlay()
    {
        if (m_Happy.GetState() > 0.5f &&
            m_Eager.GetState() > 0.5f &&
            m_Hungry.GetState() < 0.25f &&
            m_Lonely.GetState() < 0.05f &&
            m_Pain.GetState() < 0.05f &&
            m_Scared.GetState() < 0.05f &&
            m_Sick.GetState() < 0.05f &&
            m_Sleepy.GetState() < 0.05f &&
            m_Thirsty.GetState() < 0.05f &&
            m_Tired.GetState() < 0.05f)
            return 1f;

        return 0f; }


    internal float GetInjuryDepth()
    {
        return 0f;
    }


}
