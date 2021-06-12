using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class AddCollider: Editor
{
    [MenuItem("AddCollider/add")]
    static void addColliders()
    {
        var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        var previousSelection = Selection.objects;
        Selection.objects = allGos;
        var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        var selectedGameobj = Selection.gameObjects;
        Selection.objects = previousSelection;
        foreach(var obj in selectedGameobj)
        {
            if(obj.GetComponent<MeshFilter>()!=null)
            obj.AddComponent<MeshCollider>();
        }

    }

    [MenuItem("myTools/uniteBuildingNames")]
    static void uniteBuildingNames()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        var previousSelection = Selection.objects;
        Selection.objects = allGos;
        var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        var selectedGameobj = Selection.gameObjects;
        Selection.objects = previousSelection;
        int index = 1;
        foreach (var obj in selectedGameobj)
        {
            //if(obj.name.StartsWith("building_matchBuilding")&&obj.name.EndsWith("split")==false)
            if (obj.name.StartsWith("Building ") && obj.name.EndsWith("split") == false)
            {
                if(dict.ContainsKey(obj.name)==false)
                {
                    int cnt = dict.Count;
                    dict[obj.name] = "Building " + Convert.ToString(cnt + 1);
                }
                obj.name = dict[obj.name];
            }
        }

    }

    [MenuItem("myTools/higherPostAndRails")]
    static void higherPostAndRails()
    {

        //var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var previousSelection = Selection.objects;
        //Selection.objects = allGos;
        var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        var selectedGameobj = Selection.gameObjects;
        //Selection.objects = previousSelection;
        int index = 1;
        foreach (var obj in selectedGameobj)
        {
            //if(obj.name.StartsWith("building_matchBuilding")&&obj.name.EndsWith("split")==false)
            if (obj.name.StartsWith("Post ") || obj.name.StartsWith("Rail "))
            {
                Vector3 old = obj.transform.localScale;
                //old.y = 2 * old.y;
                if (obj.name.StartsWith("Rail "))
                    old.y =2.4f;
                else
                    old.y = 2.6f;
                obj.transform.localScale = old;
            }
        }

    }


    [MenuItem("myTools/widenPostAndRails")]
    static void widenPostAndRails()
    {
       
        //var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var previousSelection = Selection.objects;
        //Selection.objects = allGos;
        var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        var selectedGameobj = Selection.gameObjects;
        //Selection.objects = previousSelection;
        int index = 1;
        foreach (var obj in selectedGameobj)
        {
            //if(obj.name.StartsWith("building_matchBuilding")&&obj.name.EndsWith("split")==false)
            if (obj.name.StartsWith("Post ") || obj.name.StartsWith("Rail "))
            {
                Vector3 old = obj.transform.localScale;
                //old.y = 2 * old.y;
                if (obj.name.StartsWith("Rail "))
                    old.z = 2 * old.z;
                else
                    old.x = 2 * old.x;
                obj.transform.localScale = old;
            }
        }

    }

    [MenuItem("myTools/narrowPostAndRails")]
    static void narrowPostAndRails()
    {

       // var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var previousSelection = Selection.objects;
        //Selection.objects = allGos;
        var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        var selectedGameobj = Selection.gameObjects;
       // Selection.objects = previousSelection;
        int index = 1;
        foreach (var obj in selectedGameobj)
        {
            //if(obj.name.StartsWith("building_matchBuilding")&&obj.name.EndsWith("split")==false)
            if (obj.name.StartsWith("Post ") || obj.name.StartsWith("Rail "))
            {
                Vector3 old = obj.transform.localScale;
                //old.y = 2 * old.y;
                if (obj.name.StartsWith("Rail "))
                    old.z = 0.5f * old.z;
                else
                    old.x = 0.5f * old.x;
                obj.transform.localScale = old;
            }
        }

    }


    [MenuItem("myTools/addColliderForFence")]
    static void addColliderForFence()
    {

        //var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var previousSelection = Selection.objects;
        //Selection.objects = allGos;
        var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        var selectedGameobj = Selection.gameObjects;
       // Selection.objects = previousSelection;
        int index = 1;
        foreach (var obj in selectedGameobj)
        {
            //if(obj.name.StartsWith("building_matchBuilding")&&obj.name.EndsWith("split")==false)
            if (obj.name.StartsWith("Post ") || obj.name.StartsWith("Rail "))
            {
                if (obj.GetComponent<BoxCollider>()!=null)
                    DestroyImmediate(obj.GetComponent<BoxCollider>());
                if (obj.GetComponent<MeshCollider>() == null)
                    obj.AddComponent<MeshCollider>();
            }
        }

    }

    [MenuItem("myTools/addMeshRenderForBuilding")]
    static void addMeshRenderForBuilding()
    {
        var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        var previousSelection = Selection.objects;
        Selection.objects = allGos;
        var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        var selectedGameobj = Selection.gameObjects;
        Selection.objects = previousSelection;
        int index = 1;
        foreach (var obj in selectedGameobj)
        {

            if (obj.name.StartsWith("Building ") && obj.name.EndsWith("split") == false)
            {
                //Debug.Log(obj.name);
                //int space = obj.name.IndexOf(' ', 9);
                //string tt;
                //if (space == -1)
                //    tt = obj.name.Substring(9);
                //else
                //    tt = obj.name.Substring(9, space - 9);
                //int t = Convert.ToInt32(tt);
                //Material m = new Material(Shader.Find("Custom/buildingColor"));
                //m.SetInt("_Index", t);
                //m.SetTexture(StateTextureKey, _BuildingColorTexture);
                //m.SetTexture("_MainTex", _BuildingOutlineTexture);
                if (obj.GetComponent<MeshRenderer>() == null)
                    obj.AddComponent<MeshRenderer>();
                //   var sz = obj.GetComponent<MeshRenderer>().materials.Length;
                //    Debug.Log("size: " + sz);

                //   Material[] mm = new Material[sz];
                //    for (int i = 0; i < sz; i++)
                //       mm[i] = m;
                //   obj.GetComponent<MeshRenderer>().sharedMaterials = mm;
             //   obj.GetComponent<MeshRenderer>().sharedMaterial = m;

            }
        }

    }
}
