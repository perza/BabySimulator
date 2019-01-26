using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Condition
{
    protected BabyModel m_Baby;
    protected FuzzyMembership m_Condition = new FuzzyMembership();
    protected float m_LatestValue = 0f;

    protected int m_Priority = 0;               // if two primary actions have equal value, the priority decides the selected primary action
    public float m_PriorityEnhancement = 0f; // Boost higher priority values so that they are preferred in close call cases
    protected float m_MinimumTreshold = 0f;     // Return 0 if below a threshold

    // Start is called before the first frame update
    public Condition(BabyModel baby)
    {
        m_Baby = baby;
    }

    public void SetParams (int prio, float prio_enhance = 0f, float min_treshold = 0f)
    {
        m_Priority = prio;
        m_PriorityEnhancement = prio_enhance;
        m_MinimumTreshold = min_treshold;       
    }

    /// <summary>
    /// Updates the fuzzy input value m_LatestValue for this condition
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Allow derived classes modify m_LatestValue before usage
    /// </summary>
    /// <returns></returns>
    protected virtual float GetScaledLatestValue()
    {
        return m_LatestValue;
    }

    /// <summary>
    /// Method used to reset the condition to default state
    /// </summary>
    public virtual void Reset()
    {
        m_LatestValue = 0f;
    }


    /// <summary>
    /// Returns the current state of this condition
    /// </summary>
    public float GetState()
    {
        float urgency = m_Condition.GetMembership(GetScaledLatestValue());

        if (urgency < m_MinimumTreshold)
            return 0f;

        return urgency;
    }
}
