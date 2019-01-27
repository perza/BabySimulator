using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Actionbar : MonoBehaviour
{
    public BuildView BuildView;

    private bool buildViewEnabled = false;
       

    public void EnableBuildView()
    {
        if (buildViewEnabled)
        {
            DisableViews();
            buildViewEnabled = false;
        }
            
        else
        {
            DisableViews();
            BuildView.EnableView();
            buildViewEnabled = true;
        }
    }


    public void DisableViews()
    {
        //Disable every view here
        BuildView.DisableView();
        buildViewEnabled = false;
    }
}
