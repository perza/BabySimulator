using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BuildView : MonoBehaviour, IView
{
    public GameObject CategoryGO;
    public MenuItemHolder menuItemHolder;

    public void EnableView()
    {
        gameObject.SetActive(true);
        menuItemHolder.HideItems();
    }

    public void BuildWall()
    {

    }

    public void DisableView()
    {
       if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
