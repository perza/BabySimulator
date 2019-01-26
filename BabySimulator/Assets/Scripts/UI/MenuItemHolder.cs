using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuItemHolder : MonoBehaviour
{
    private List<GameObject> CurrentItems = new List<GameObject>();

    public void AddMenuItem(GameObject prefab)
    {
        var go = GameObject.Instantiate(prefab);
        CurrentItems.Add(go);
    }

    public void RemoveItems()
    {
        CurrentItems.ForEach(go => Destroy(go));
        CurrentItems.Clear();
    }
}
