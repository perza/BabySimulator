using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BuildView : MonoBehaviour
{
    public GameObject CategoryGO;
    
    List<BuildCategory> Categories = new List<BuildCategory>();

    // Start is called before the first frame update
    void Start()
    {
        if(CategoryGO != null)
        {
            Categories = CategoryGO.GetComponentsInChildren<BuildCategory>().ToList();
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CategoryClicked(BuildCategory caller)
    {

    }
}
