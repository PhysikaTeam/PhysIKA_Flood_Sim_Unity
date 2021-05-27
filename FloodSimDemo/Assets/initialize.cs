using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;

public class initialize : MonoBehaviour
{
    
    private int xRes = 1024;
    private int yRes = 1024;
    private float waterHeight = 7.0f;
    private float waterVelocity = 5.0f;

    public InputField xInput;
    public InputField yInput;
    public InputField wInput;
    public InputField vInput;

    public GameObject initPanel;

    // Start is called before the first frame update
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInitBtnClick()
    {
        String xString = xInput.text.ToString();
        String yString = yInput.text.ToString();
        String w = wInput.text.ToString();
        String v = vInput.text.ToString();
        int.TryParse(xString, out xRes);
        int.TryParse(yString, out yRes);
        waterHeight = Convert.ToSingle(w);
        waterVelocity = Convert.ToSingle(v);
        CustomTerrain.width = xRes;
        CustomTerrain.height = yRes;
        CustomTerrain.riverHeight = waterHeight;
        CustomTerrain.riverSpeed = waterVelocity;

        CustomTerrain.init = true;

        initPanel.SetActive(false);

        Debug.Log("(" + xRes + ", " + yRes + ")");
        Debug.Log("height: " + waterHeight + " speed: " + waterVelocity);
    }
}
