using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class removeWaterResponse : MonoBehaviour
{
    private bool isRmove = false;
    // Start is called before the first frame update
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
            isRmove = false;
        }

    }

    public void removeBtnClick()
    {
        if (isRmove == false)
        {
            CustomTerrain.InputMode = InputModes.AddPaishuikou;
            CustomTerrain._brushRadius = settingButtonResponse.inputRadius;
            CustomTerrain.BrushAmount = settingButtonResponse.inputAmount;
            isRmove = true;
        }
        else
        {
            CustomTerrain.InputMode = InputModes.ClickBuildingToGetSpline;
            isRmove = false;
        }
    }
}
