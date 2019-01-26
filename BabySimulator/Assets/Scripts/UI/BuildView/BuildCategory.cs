using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildCategory : MonoBehaviour
{
    public List<GameObject> MyButtons = new List<GameObject>();
    public MenuItemHolder MenuItemHolder;

    public void EnableMyButtons()
    {
        MenuItemHolder.HideItems();
        MyButtons.ForEach(go => go.SetActive(true));
    }
    
}
