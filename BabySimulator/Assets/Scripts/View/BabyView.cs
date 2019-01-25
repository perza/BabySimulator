using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CowVieW manages the visual cow model
/// </summary>
public class BabyView : MonoBehaviour
{
    public BabyModel m_BabyModel;

    public Animator m_Animator;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(BabyModel baby_model)
    {
        m_BabyModel = baby_model;
        m_BabyModel.mActionChange += HandleCustomEvent;
    }

    // Update is called once per frame
    public void Update()
    {
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
