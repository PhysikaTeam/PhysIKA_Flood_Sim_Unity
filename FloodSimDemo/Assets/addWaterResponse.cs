using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class addWaterResponse : MonoBehaviour
{
    // Start is called before the first frame update
    private bool isAdd = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            CustomTerrain.InputMode = InputModes.ClickBuildingToGetSpline;
            CustomTerrain._brushRadius = 0;
            CustomTerrain.BrushAmount = 0;
            isAdd = false;
        }
    }

    public void OnAddWaterBtnClick()
    {
        if (isAdd == false)
        {
            CustomTerrain.InputMode = InputModes.AddJinshuikou;
            CustomTerrain._brushRadius = settingButtonResponse.inputRadius;
            CustomTerrain.BrushAmount = settingButtonResponse.inputAmount;
            isAdd = true;
        }
        else
        {
            CustomTerrain.InputMode = InputModes.ClickBuildingToGetSpline;
            isAdd = false;
        }
    }


}
