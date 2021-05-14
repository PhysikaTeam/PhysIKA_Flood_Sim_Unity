using UnityEngine;
using System.Collections;
using UnityEditor;
public class AutoFenceManagerMenu : MonoBehaviour {

	AutoFenceCreator creator;

	static public int globalNumFences = 0;

	[MenuItem ("GameObject/Create Auto Fence Builder #&f")]

	static void CreateFenceManager() {
		GameObject go = new GameObject("Auto Fence Builder" /*+ ++globalNumFences*/);
		go.transform.position = Vector3.zero;
		go.AddComponent(typeof(AutoFenceCreator));
		Selection.activeGameObject = go;
	}

}
