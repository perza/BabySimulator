using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MenuItemHolder : MonoBehaviour
{
    private List<GameObject> CurrentItems = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CurrentItems.Add(transform.GetChild(i).gameObject);
        }
    }


    public void HideItems()
    {
        CurrentItems.ForEach(go => go.SetActive(false));
    }
}
