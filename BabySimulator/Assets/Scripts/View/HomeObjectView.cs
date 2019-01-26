using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CowVieW manages the visual cow model
/// </summary>
public class HomeObjectView : MonoBehaviour
{
    public HomeObject m_HomeObjectModel;

    public Animator m_Animator;

    public List<GameObject> m_ProximityObjects;

    // Start is called before the first frame update
    void Start()
    {
        m_ProximityObjects = new List<GameObject>();
    }

    public void Init(HomeObject model)
    {
        m_HomeObjectModel = model;
        m_HomeObjectModel.mActionChange += HandleCustomEvent;
    }

    // Update is called once per frame
    public void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {

        HomeObjectView test = other.gameObject.GetComponent<HomeObjectView>();

        if (test && (test.m_HomeObjectModel.m_ObjectName.Equals("BabyModel") ||
                    test.m_HomeObjectModel.m_ObjectName.Equals("NannyModel")))
        {
            Debug.Log("TRIGGER ENTER");
            m_ProximityObjects.Add(other.gameObject);
        }

        // Keep list of babies and nannies within close range 
    }

    private void OnTriggerExit(Collider other)
    {
        HomeObjectView test = other.gameObject.GetComponent<HomeObjectView>();

        if (test && (test.m_HomeObjectModel.m_ObjectName.Equals("BabyModel") ||
                    test.m_HomeObjectModel.m_ObjectName.Equals("NannyModel")))
        {
            Debug.Log("TRIGGER EXIT");
            m_ProximityObjects.Remove(other.gameObject);
        }

        // Keep list of babies and nannies within close range 
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("COLLISION CONTACT");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="act">animation name</param>
    /// <param name="val">multipurpose parameter</param>
    void HandleCustomEvent(BabyModel.ConcreteAction act, float val)
    {
        m_Animator.SetTrigger(act.ToString());

        // StartCoroutine(AnimationChanger(act));

    }

    // :BUG: triggering animation from coroutine does not seem to work. Use booleans?
    IEnumerator AnimationChanger (BabyModel.ConcreteAction target_action)
    {
        while (true)
        {
            //Debug.Log("POSE: " + m_CowModel.m_CurrentBasePose.ToString());

            //if (m_CowModel.m_CurrentBasePose == CowModel.CurrentBasePose.STANDING)
            //    mAnimator.SetTrigger("STANDING");
            //else
            //    mAnimator.SetTrigger("LYING");

            yield return new WaitForSeconds(1);

            break;
        }

        // Debug.Log("ANIM: " + target_action.ToString());

        m_Animator.SetTrigger(target_action.ToString());
    }
}
