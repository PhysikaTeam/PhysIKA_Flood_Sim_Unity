using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class getWorldPos : Editor
{
    [MenuItem("myTools/GetPos")]
    static void GetPosAndOther()
    {
        var previousSelection = Selection.gameObjects; // Start is called before the first frame update
        foreach(var obj in previousSelection)
        {
            Debug.Log(obj.name);
            Transform transform = obj.transform;
           // Transform parent = transform.parent.transform;
           // Debug.Log(parent.TransformPoint(transform.localPosition));
            Debug.Log(transform.position);
            Debug.Log(transform.GetComponent<Bounds>());
        }
        
    }
    
   
}
