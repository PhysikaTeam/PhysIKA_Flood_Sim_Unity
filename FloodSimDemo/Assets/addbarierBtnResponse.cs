using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class addbarierBtnResponse : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isConstruct = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.L))
        {
            uiDrawLines.isDrawing = false;
            CustomTerrain.buildBarrier1 = CustomTerrain.buildBarrier2 = true;
            isConstruct = false;
            //CustomTerrain.InputMode = InputModes.ClickBuildingToGetSpline;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isConstruct == false)
            {
                uiDrawLines.isDrawing = true;
                isConstruct = true;
            }
            Camera.main.GetComponent<uiDrawLines>().drawOpen = true;
        }
    }
    
    public void onButtonClick()
    {
        CustomTerrain._brushRadius = 0;
        CustomTerrain.InputMode = InputModes.AddTerrain;
        CustomTerrain.barrirHei = settingButtonResponse.inputBhei;
        CustomTerrain.barrirWid = settingButtonResponse.inputBWid;
        
       
    }
}
