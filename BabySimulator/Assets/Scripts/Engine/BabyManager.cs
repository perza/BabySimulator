using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BabyManager : PersistentEngineSingleton<BabyManager>
{
    public List<BabyModel> m_Babies;

    // Start is called before the first frame update
    public BabyManager()
    {
        m_Babies = new List<BabyModel>();
    }

    public BabyModel AddBaby (GameObject baby_view)
    {
        m_Babies.Add(new BabyModel(baby_view));

        baby_view.GetComponent<HomeObjectView>().StartCoroutine (CleanBabyColliders(m_Babies[m_Babies.Count - 1]));

        return m_Babies[m_Babies.Count-1];
    }

    // Update is called once per frame
    public void Update()
    {
        foreach (BabyModel baby in m_Babies)
        {
            baby.Update();
        }
    }

    public IEnumerator CleanBabyColliders (HomeObject ho)
    {
        yield return new WaitForSeconds(1);

        ho.CleanColliders();
    }
}
