using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;

public class SettingEvent : MonoBehaviour
{
    private MyDropdown myDropdown;
    Transform toggleRoot;
    Toggle[] toggleList;
    // Start is called before the first frame update
    void Start()
    {
        myDropdown = transform.parent.GetComponent<MyDropdown>();
        toggleRoot = transform.parent.gameObject.transform.Find("Dropdown List/Viewport/Content");
        toggleList = toggleRoot.GetComponentsInChildren<Toggle>(false);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < toggleList.Length; i++)
        {
            Toggle temp = toggleList[i];
            if(i == 0)
            {
                CustomTerrain.earlyWarning = temp.isOn;
            }
            else if(i == 1)
            {
                CustomTerrain.displayWaterDepth = temp.isOn;
            }
            else if(i == 2)
            {
                CustomTerrain.displayMap = temp.isOn;
            }
        }
    }
}
