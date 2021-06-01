using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts;

public class getSettings : MonoBehaviour
{
    public MyDropdown myDropdown;
    // Start is called before the first frame update
    void Start()
    {
        //myDropdown = transform.parent.GetComponent<MyDropdown>();
    }

    // Update is called once per frame
    void Update()
    {
        //Transform toggleRoot = myDropdown.transform.Find("Dropdown List/Viewport/Content");
        //Toggle[] toggleList = toggleRoot.GetComponentsInChildren<Toggle>(false);
        //if (toggleList == null)
        //    return;
        //for (int i = 0; i < toggleList.Length; i++)
        //{
        //    Toggle temp = toggleList[i];
        //    if (i == 0)
        //    {
        //        CustomTerrain.earlyWarning = temp.isOn;
        //    }
        //    else if (i == 1)
        //    {
        //        CustomTerrain.displayWaterDepth = temp.isOn;
        //    }
        //    else if (i == 2)
        //    {
        //        CustomTerrain.displayMap = temp.isOn;
        //    }
        //}
    }
}
