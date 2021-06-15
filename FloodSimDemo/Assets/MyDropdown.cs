using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts;
/// <summary>
/// 将下拉菜单上的 Dropdown组件移除，替换为该脚本
/// </summary>
public class MyDropdown : Dropdown
{
    public bool AlwaysCallback = false;//是否开启 点击选项按钮总是回调
    public int SelectIndexBitMark = 0;

    public new void Show()
    {
        //Debug.Log("in show");
        base.Show();
        Transform toggleRoot = transform.Find("Dropdown List/Viewport/Content");
        Toggle[] toggleList = toggleRoot.GetComponentsInChildren<Toggle>(false);
        for (int i = 0; i < toggleList.Length; i++)
        {
     
            Toggle temp = toggleList[i];
            temp.onValueChanged.RemoveAllListeners();
            temp.isOn = ((1 << i) & SelectIndexBitMark) > 0; // 改造后
            /*if (temp.isOn == false)
            {
             
                Debug.Log("index false");
            }
            else Debug.Log("index true");*/
            temp.onValueChanged.AddListener(x => OnSelectItemEx(temp));
            
        }
        //Debug.Log("out show");
    }


    public override void OnPointerClick(PointerEventData eventData)
    {
        Show();
        //Debug.Log("OnPointerClick");
    }


    public override void OnPointerExit(PointerEventData eventData)
    {
        /*Transform toggleRoot = transform.Find("Dropdown List/Viewport/Content");
        Toggle[] toggleList = toggleRoot.GetComponentsInChildren<Toggle>(false);
        if (toggleList == null)
            return;
        for(int i = 0; i < toggleList.Length; i++)
        {
            Toggle temp = toggleList[i];
            if (i == 0)
            {
                CustomTerrain.earlyWarning = temp.isOn;
            }
            else if (i == 1)

            {
                CustomTerrain.displayWaterDepth = temp.isOn;
            }
            else if (i == 2)
            {
                CustomTerrain.displayMap = temp.isOn;
            }
        }*/
        //Debug.Log("OnPointerExit");
    }


    public void OnSelectItemEx(Toggle toggle)
    {
        /*Debug.Log("in listener");
        Debug.Log(toggle.GetComponentInChildren<Text>().text);*/
        
        if (!toggle.isOn)
        {
            toggle.isOn = true;
            if(toggle.GetComponentInChildren<Text>().text == "显示洪水预警等级")
            {
                CustomTerrain.earlyWarning = false;
            }
            else if(toggle.GetComponentInChildren<Text>().text == "显示洪水深度伪彩图")
            {
                CustomTerrain.displayWaterDepth = false;
            }
            else if(toggle.GetComponentInChildren<Text>().text == "显示地图")
            {
                CustomTerrain.displayMap = false;
            }
            return;
        }
        else{
            if (toggle.GetComponentInChildren<Text>().text == "显示洪水预警等级")
            {
                CustomTerrain.earlyWarning = true;
            }
            else if (toggle.GetComponentInChildren<Text>().text == "显示洪水深度伪彩图")
            {
                CustomTerrain.displayWaterDepth = true;
            }
            else if (toggle.GetComponentInChildren<Text>().text == "显示地图")
            {
                CustomTerrain.displayMap = true;
            }
        }
       
        int selectedIndex = -1;
        Transform tr = toggle.transform;
        Transform parent = tr.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i) == tr)
            {
                selectedIndex = i - 1;
                break;
            }
        }

        if (selectedIndex < 0)
            return;
        if (value == selectedIndex && AlwaysCallback)
            onValueChanged.Invoke(value);
        else
            value = selectedIndex;
        
        SelectIndexBitMark ^= 1 << value;
        Hide();
        //Debug.Log("out listener");
    }
}
