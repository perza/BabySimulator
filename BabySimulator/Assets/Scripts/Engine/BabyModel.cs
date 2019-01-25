using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class BabyModel : HomeObject
{
    // m_CowView is used as location storage, needed for navigation and collisions
    GameObject m_BabyView;
    Rigidbody m_Rigidbody;

    private Camera m_Camera;    // path finding
    private NavMeshPath m_CurrentPath;   // path finding

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
    float m_WalkingSpeed = 1.3f;  // 
    float m_TurningSpeed = 40f;  // 
    float m_BabyHeight = -0.45f;     // Pivotpoint from ground

    // list of all the wounds the cow is suffering
    // :TODO: add healing/dying based on the wounds
    public List<float> m_Wounds = new List<float>();
    float m_HealingSpeed = 0.01f;

    // cow constants
    const float DAILY_FORAGE = 20f; // kg

    static int m_CowHierachyPositions = 0; // static counter for cows is used as the initial cow position in herd hierarchy. Priority 0 is the herd leader.
    int m_CowHierarchyPosition = 0; // cow position in herd hierarchy
    //:NOTE: CowManager holds list of all cows, and separate ref the herd leader

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
        IDLE       // Lowest priority
    };

    PrioritizedPrimaryAction m_CurrentPrimaryAction = PrioritizedPrimaryAction.IDLE;
    float[] m_PrimaryActionValues;

    public enum ConcreteAction
    {
        LYING,
        STANDING,
        TO_IDLING,          // Return to idling from any state. This means returning to LYING or STANDING, depending on the m_CurrentBasePose
        WALKING,
        RUNNING,
        NAPPING,            // Sleeping while standing
        DYING,
        PUSHING,
        PUSHED,
        BULLYING,
        STAGGERING,
        SLEEPING,           // Sleeping while lying
        STAND_UP,
        LIE_DOWN,
        TURN_LEFT,
        TURN_RIGHT,
        MOOING,
        EATING,
        DRINKING,
        MILKING,
        RUMINATING,
    };

    ConcreteAction m_CurrentConcreteAction = ConcreteAction.STANDING;

    // Current pose tells the starting position
    // We need to know these before starting any new animation, so that when starting walking,
    // the cow knows to first raise up.
    // We do NOT want to stop any earlier action by returning to default standing pose.
    public enum CurrentBasePose { LYING, STANDING };
    public CurrentBasePose m_CurrentBasePose = CurrentBasePose.STANDING;

    float m_BoostPerLevel;
    const float MAX_PRIO_BOOST = 0.5f;

    // Start is called before the first frame update
    public BabyModel(GameObject cow_view)
    {
        m_CowHierarchyPosition = m_CowHierachyPositions;
        m_CowHierachyPositions++;

        m_PrimaryActionValues = new float[Enum.GetNames(typeof(PrioritizedPrimaryAction)).Length];

        m_BoostPerLevel = MAX_PRIO_BOOST / m_PrimaryActionValues.Length;

        m_BabyView = cow_view;
        m_Rigidbody = m_BabyView.GetComponent<Rigidbody>();

        m_Camera = Camera.main;
        m_CurrentPath = new NavMeshPath();

        GenerateConditions();

        NotifyStateChangeObservers(ConcreteAction.STANDING);

        //:TEST:
        m_Hungry.DEBUG_SetValue(2f * 3600f);

    }

    // A cow view subscribes to cow model to follow the changes in actions
    public event ActionChangeHandler mActionChange;
    public delegate void ActionChangeHandler(ConcreteAction act, float val);

    // Inform possible observers (view) of the new action state start
    public void NotifyStateChangeObservers(ConcreteAction act, float val = 0)
    {
        if (null != mActionChange)
            mActionChange.Invoke(act, val);
    }

    bool m_ActionInterrupted = false;
    bool m_ActionChanged = false;

    // Update is called once per frame
    public new void Update()
    {
        PrioritizedPrimaryAction new_primary_action = PrioritizedPrimaryAction.IDLE;

        // Update physic params
        UpdatePhysicParams();

        // Update conditions
        UpdatePhysicalConditions();
        UpdateMentalConditions();

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
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.DRINK] = Inferdrink();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.EAT] = InferEat();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.ESCAPE] = InferEscape();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.EXPLORE] = InferExplore();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.IDLE] = InferIdle();
        m_PrimaryActionValues[(int)PrioritizedPrimaryAction.SLEEP] = InferSleep();

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
            case PrioritizedPrimaryAction.IDLE:
                Idle();
                break;
            case PrioritizedPrimaryAction.SLEEP:
                Sleep();
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
    ///     Description: Walk to the milking station, then proceed into milking, stand still while milked, then walk off the station.Cow seeks milking if it has enough milk in the udder, and (probably) when it is time for milking.Are there regular milking times?
    ///     Triggers: Needs milking.
    ///     Concrete actions: Walking, Milking, Pushing, Bullying
    /// </summary>
    public void Milk()
    { }

    /// <summary>
    ///     Description: Search a nice place within the herd and start ruminating food while standing or lying
    ///     Triggers: Needs ruminating .Assumption is that cow needs to start ruminating once its stomach has preprocessed the forage.
    ///     Concrete actions: Walking, Standing/Lying, Ruminating
    /// </summary>
    public void Ruminate()
    { }

    /// <summary>
    ///     Description: Bully another cow, human or other object. Maybe running or walking, depending on the attack target.
    ///     Triggers: Angry.Note, Scared with no way to escape may convert to Angry, depending on the cow hierarchy.
    ///     Concrete actions: Walking/Running, Bullying
    /// </summary>
    public void Attack()
    { }

    /// <summary>
    ///     Description: Run away from the cause of fear.
    ///     Triggers: Scared.Note, Scared with no way to escape may convert to Angry.
    ///     Concrete actions: Running
    /// </summary>
    public void Escape()
    { }

    /// <summary>
    ///      Description: Walk around studying place.
    ///      Triggers: Bored
    ///      Concrete actions: Walking
    /// </summary>
    public void Explore()
    { }

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
    { }

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

    public bool ToIdling()
    { return false; }

    public void Lying()
    { }

    public void Standing()
    { }

    // We are interested in 2D distances only
    float MagnitudeXZ(Vector3 a, Vector3 b)
    {
        float c = Mathf.Abs(a.x - b.x);
        float d = Mathf.Abs(a.z - b.z);

        return Mathf.Sqrt(c * c + d * d);
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

    Vector3 m_CurrentTarget = Vector3.negativeInfinity;
    int m_NextPathStep = 0;
    float m_PrevDist = float.MaxValue;
    public bool Walking()
    {

        if (m_CurrentTarget.Equals(Vector3.negativeInfinity))
            m_CurrentTarget = m_CurrentPath.corners[m_NextPathStep];

        //if (m_ContactObjects.Count > 0)
        //{
        //    if (IsBlocked(m_CurrentTarget))
        //    {
        //        // Push and Bully action

        //    }

        //    return true;
        //}

        // movement
        // float step = m_WalkingSpeed * GameManager.m_Instance.m_GameDeltaTime;
        // m_CowView.transform.position = Vector3.MoveTowards(m_CowView.transform.position, m_CurrentTarget, step);


        
            // Debug.Log("ID: " + id + ", m_Rigidbody.velocity: " + m_Rigidbody.velocity + "Target: " + m_Target);

            // m_CowView.transform.position = Vector3.MoveTowards(m_CowView.transform.position, m_CurrentTarget, step);

            //float step = m_WalkingSpeed * GameManager.m_Instance.m_GameDeltaTime;
            //m_Rigidbody.MovePosition(m_CowView.transform.position - (m_CowView.transform.position - m_CurrentTarget).normalized * step);


        Vector3 target_dir = m_CurrentTarget - m_BabyView.transform.position;

        float signed_angle = Vector3.SignedAngle(target_dir, m_BabyView.transform.forward, Vector3.down);

        // Vector3 m_EulerAngleVelocity = new Vector3(0, 1, 0) * Mathf.Sign(signed_angle);
        Vector3 m_EulerAngleVelocity = new Vector3(0, 1, 0);

        // Vector3 new_dir = Vector3.RotateTowards(m_CowView.transform.forward, target_dir, m_TurningSpeed * GameManager.m_Instance.m_GameDeltaTime, 0f);

        // Debug.Log("dir delta: " + (target_dir.normalized - m_CowView.transform.forward).magnitude);

        if ((target_dir.normalized - m_BabyView.transform.forward).magnitude > 0.11f)
        {
            Vector3 euler_delta = m_EulerAngleVelocity.normalized * m_TurningSpeed * Time.deltaTime * Mathf.Sign(signed_angle);


            float signed_angle2 = Vector3.SignedAngle(m_Rigidbody.velocity, m_BabyView.transform.forward, Vector3.down);

            // Rotate velocity vector (around world up axis)
            // m_Rigidbody.velocity = Quaternion.Euler(0, m_TurningSpeed * Time.deltaTime * Mathf.Sign(signed_angle), 0) * m_Rigidbody.velocity;

            Debug.Log("Quaternion.Euler(0, signed_angle2, 0): " + Quaternion.Euler(0, signed_angle2, 0));

            // m_Rigidbody.velocity = Quaternion.Euler(0, signed_angle2, 0) * m_Rigidbody.velocity;

            // Vector3 velocity = m_Rigidbody.velocity;
            // m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.velocity = m_BabyView.transform.forward * m_Rigidbody.velocity.magnitude;

            // Rotate vector around its own up axis
            // vector = Quaternion.AngleAxis(-45, Vector3.up) * vector;

            Quaternion deltaRotation = Quaternion.Euler(euler_delta);

            // m_Rigidbody.velocity = (m_Rigidbody.rotation * deltaRotation).eulerAngles;

            // m_Rigidbody.rotation = Quaternion.SetLookRotation(velocity);

            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);

            

            Vector3 new_dir = Vector3.RotateTowards(m_BabyView.transform.forward, m_Rigidbody.velocity, m_TurningSpeed * GameManager.m_Instance.m_GameDeltaTime, 0f);

        }

        // :BUG: note that after the turn the prevous force should be zeroed, otherwise we keep walking to wrong direction
        if (m_Rigidbody.velocity.magnitude < m_WalkingSpeed)
        {
            // Debug.Log("WALKING");
            // m_Rigidbody.AddForce(m_CurrentTarget.normalized * 1000f);
            m_Rigidbody.AddForce(m_BabyView.transform.forward * 1000f);
        }


            //Vector3 target_dir = m_CurrentTarget - m_CowView.transform.position;
            //Vector3 new_dir = Vector3.RotateTowards(m_CowView.transform.forward, target_dir, m_TurningSpeed * GameManager.m_Instance.m_GameDeltaTime, 0f);

            //Debug.Log("m_CowView.transform.rotation: " + m_CowView.transform.rotation.eulerAngles + ", new_dir: " + new_dir + ", target_dir" + target_dir);

            //// m_Rigidbody.MoveRotation(Quaternion.Euler(target_dir));

            //m_Rigidbody.rotation = Quaternion.Euler(target_dir);


        // rotation
        //Vector3 target_dir = m_CurrentTarget - m_CowView.transform.position;
        //Vector3 new_dir = Vector3.RotateTowards(m_CowView.transform.forward, target_dir, m_TurningSpeed * GameManager.m_Instance.m_GameDeltaTime, 0f);
        //m_CowView.transform.rotation = Quaternion.LookRotation(new_dir);

        float dist = MagnitudeXZ(m_BabyView.transform.position, m_CurrentTarget);

        // are we there yet? 
        if (dist < 0.5) // || dist > m_PrevDist)
        {
            m_Rigidbody.velocity = Vector3.zero;

            m_PrevDist = float.MaxValue;
            m_CurrentTarget = Vector3.negativeInfinity;

            if (m_NextPathStep >= m_CurrentPath.corners.Length - 1)
            {
                // This was the last corner, we are finished
                return false;
            }

            m_NextPathStep++;
            return true;
        }
        else
        {
            m_PrevDist = dist;
            return true;
        }
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
    float Inferdrink() { return 0f; }
    float InferEat() { return m_Hungry.GetState(); }
    float InferEscape() { return 0f; }
    float InferExplore() { return 0f; }
    float InferIdle() { return 0f; }
    float InferSleep() { return 0f; }

    void GetPath(Vector3 target)
    {
        // :TEST: Navigation 
        // if (Input.GetMouseButtonDown(0))
        // {
        // Ray ray = m_Camera.ScreenPointToRay(target);
        // RaycastHit hit;

        // if (Physics.Raycast(ray, out hit))
        NavMesh.CalculatePath(m_BabyView.transform.position, target, NavMesh.AllAreas, m_CurrentPath);

        // Adjust the path to current baby height
        for (int i = 0; i < m_CurrentPath.corners.Length; i++)
        {
            m_CurrentPath.corners[i].Set(m_CurrentPath.corners[i].x, m_BabyHeight, m_CurrentPath.corners[i].z);
        }

        // }
    }

    //////////////////////////
    /// COLLISION HANDLING ///
    //////////////////////////

    /// <summary>
    /// Remove references to cow's own colliders from all lists.
    /// </summary>
    public void CleanColliders ()
    {
        Debug.Log("clean colliders");

        GameObject go;
        
        int len = m_ContactObjects.Count - 1;

        for (int i = len; i >= 0; i--)
        {
            go = m_ContactObjects[i];

            if (ReferenceEquals(go, m_BabyView))
            {
                m_ContactObjects.Remove(go);
            }
        }
    }

    internal float GetInjuryDepth()
    {
        return 0f;
    }


}
