using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class settingButtonResponse : MonoBehaviour
{
    public GameObject SettingsPanel;
    public static float inputBWid = 10.0f, inputBhei = 2.0f;
    public static float inputRadius = 3.0f;
    public static float inputAmount = 2.0f;

    public InputField wInput;
    public InputField hInput;
    public InputField rInput;
    public InputField aInput;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        String wString = wInput.text.ToString();
        String hString = hInput.text.ToString();
        String rString = rInput.text.ToString();
        String aString = aInput.text.ToString();

        float.TryParse(wString, out inputBWid);
        float.TryParse(hString, out inputBhei);
        float.TryParse(rString, out inputRadius);
        float.TryParse(aString, out inputAmount);
        
    }

    public void ButtonOnClickEvent()
    {
        if (SettingsPanel.active == false)
        {
            SettingsPanel.SetActive(true);


        }
        else
        {
            SettingsPanel.SetActive(false);
        }
    }
}
