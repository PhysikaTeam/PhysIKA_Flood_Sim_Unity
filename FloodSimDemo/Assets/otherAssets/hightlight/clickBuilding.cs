using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HighlightingSystem;

public class clickBuilding : MonoBehaviour
{
    public GameObject building;
    public Camera ca;
    public Vector2 lastPos;
    public Color highlightColor=Color.red;
    public GUIStyle gs;
    public Text Title;
    //public LineChart lChart;
    private void OnGUI()
    {
        if(building!=null)
        {
           // canvas.enabled = true;
            Vector3 worldPos = building.transform.parent.TransformPoint(building.transform.position);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            //Debug.Log(screenPos);
            var ca = Camera.main;
            
            //Debug.Log(rc);
            
            GUI.color = Color.red;
            //GUI.Label(new Rect(new Vector2(0,0), new Vector2(50, 20)), building.name,gs);
            //hightlightObj();
        }
        //else
           // canvas.enabled = false;

    }

    private void hightlightOn(GameObject obj)
    {
        if (obj == null)
            return;
        var h= obj.AddComponent<Highlighter>();
        //var h = obj.GetComponent<Highlighter>();
        h.ConstantOnImmediate(highlightColor);

    }

    private void hightlightOff(GameObject obj)
    {
        if (obj == null)
            return;
        //var h = obj.GetComponent<Highlighter>();
        //h.ConstantOffImmediate();
        DestroyImmediate(obj.GetComponent<Highlighter>());


    }


    // Start is called before the first frame update
    void Start()
    {
        ca.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        { //首先判断是否点击了鼠标左键
            // Debug.Log(Input.mousePosition);
            /*int fingerId = Input.GetTouch(0).fingerId;
            if (EventSystem.current.IsPointerOverGameObject(fingerId))
            {
                Debug.Log("点击到UI");
            }*/
           
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //定义一条射线，这条射线从摄像机屏幕射向鼠标所在位置
            RaycastHit hit; //声明一个碰撞的点(暂且理解为碰撞的交点)
            if (Physics.Raycast(ray, out hit)) //如果真的发生了碰撞，ray这条射线在hit点与别的物体碰撞了
            {
                var obj = hit.collider.gameObject;
                hightlightOff(building);

                if (obj.name.StartsWith("Building "))
                {
                    ca.enabled = true;
                    hightlightOn(obj);
                    lastPos = Input.mousePosition;
                    building = obj;
                    Title.text = building.name;
                    //lChart.title.text = building.name;
                }
                else
                {
                    building = null;
                    ca.enabled = false;
                }
                //GetComponent<Transform>().pos
            }
            
        }
    }
}
