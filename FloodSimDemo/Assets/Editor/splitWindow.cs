using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
public class splitWindow : EditorWindow
{
    static float dst = 0.0f;
    static string s = "0";
    [MenuItem("splitWindow/Demo")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow<splitWindow>();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        s = GUILayout.TextField(s);
       //  dst =EditorGUI.FloatField(new Rect(0, 0, 50, 50), dst);
        GUILayout.EndHorizontal();
        if(GUILayout.Button("splitWithBBOXToGetNumber"))
        {
            splitBuildings.splitWithBBoxNumber();
        }
        if (GUILayout.Button("splitWithBBOX"))
        {
            splitBuildings.splitWithBBox();
        }
        if (GUILayout.Button("splitWithDistanceNumber"))
        {
            splitBuildings.splitWithDstNumber(Convert.ToSingle(s));
        }
        if (GUILayout.Button("splitWithDistance"))
        {
            splitBuildings.splitWithDst(Convert.ToSingle(s));
        }
    }
}
