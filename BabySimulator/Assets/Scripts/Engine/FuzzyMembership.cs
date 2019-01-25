using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FuzzyMembership
{
    SortedList<float, float> m_DistributionPoints;

    public FuzzyMembership(int priority=0, float prio_enhance=0.0f)
    {
        m_DistributionPoints = new SortedList<float, float>();
    }

    public void Add(float key, float val)
    {
        if (m_DistributionPoints.ContainsKey(key))
            throw new System.Exception("FuzzyMembership: Inserting same key twice");

        m_DistributionPoints.Add(key, val);
    }

    public float GetMembership(float key)
    {
        int index = FindFirstIndexGreaterThanOrEqualTo(key); // BinarySearch(m_DistributionPoints.Keys, key);

        // The end values are used for all outliers
        if (0 == index || index == m_DistributionPoints.Keys.Count-1)
            return m_DistributionPoints.Values[index];
        else
        {
            int prev_ind = index - 1;

            float delta = m_DistributionPoints.Keys[index] - m_DistributionPoints.Keys[prev_ind];
            float diff = m_DistributionPoints.Keys[index] - key;

            float pos = 1f - diff / delta;

            float mem_range = m_DistributionPoints.Values[index] - m_DistributionPoints.Values[prev_ind];

            return m_DistributionPoints.Values[prev_ind] + pos * mem_range;
        }
    }

    /// <summary>
    /// Get the closest index by binary search
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int FindFirstIndexGreaterThanOrEqualTo(float key)
    {
        int begin = 0;
        int end = m_DistributionPoints.Keys.Count-1;
        while (end > begin)
        {
            int index = (begin + end) / 2;
            float el = m_DistributionPoints.Keys[index];
            if (el.CompareTo(key) >= 0)
                end = index;
            else
                begin = index + 1;
        }
        return end;
    }
}
