using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

[CustomEditor(typeof(FenceMeshMerge))]
public class FenceMeshMergeEditor : Editor {

	public FenceMeshMerge 		 fenceMeshMerge;
	//private SerializedProperty	 fenceHeight;
	bool showHelp = false;
	void OnEnable()    
	{
		fenceMeshMerge = (FenceMeshMerge)target;
	}

	//------------------------------------------
	public override void OnInspectorGUI() 
	{
		serializedObject.Update();
		List<GameObject> finishedMergedObjects = new List<GameObject>();

		if( GUILayout.Button("Create Merged-Mesh Copy", GUILayout.Width(200)) ){ 

			//-- Cretae New Folder for the copy -----
			GameObject mergedCopyFolder = new GameObject(fenceMeshMerge.gameObject.name + " Merged Mesh Copy");
			GameObject moveableFolder= new GameObject("Merged");
			moveableFolder.transform.parent = mergedCopyFolder.transform;
			Vector3 adjustedPosition = Vector3.zero; // we'll set the final position of everything to the first post position
			//=========== Rails ==============
			Transform mainRailsFolder = fenceMeshMerge.gameObject.transform.Find("Rails");
			List<Transform> railsDividedFolders = GetAllDividedFolders("Rails");
			for(int i=0; i< railsDividedFolders.Count; i++){
				List<GameObject> allRails = GetAllGameObjectsFromDividedFolder(railsDividedFolders[i]);
	
				GameObject mergedObj = CombineMeshes(allRails, "Rails Merged " + i);
				mergedObj.transform.parent = moveableFolder.transform;
				finishedMergedObjects.Add (mergedObj);
				for(int j=0; j<allRails.Count; j++){

					GameObject thisRail = allRails[j];
					BoxCollider coll = thisRail.GetComponent<BoxCollider>(); // does the original have a collider
					if(coll != null){

						GameObject colliderDummy = Instantiate (thisRail) as GameObject;
						colliderDummy.name = thisRail.name + "_BoxCollider";
						colliderDummy.transform.parent = mergedObj.transform;
						Vector3 pos = colliderDummy.transform.position;
						colliderDummy.transform.position = pos;
						MeshFilter mf = colliderDummy.GetComponent<MeshFilter>();
						DestroyImmediate(mf);
						MeshRenderer mr = colliderDummy.GetComponent<MeshRenderer>();
						DestroyImmediate(mr);
					}
				}
			}
			//=========== Posts ==============
			List<Transform> postsDividedFolders = GetAllDividedFolders("Posts");
			for(int i=0; i< postsDividedFolders.Count; i++){
				List<GameObject> allPosts = GetAllGameObjectsFromDividedFolder(postsDividedFolders[i]);
				GameObject mergedObj = CombineMeshes(allPosts, "Posts Merged " + i);
				mergedObj.transform.parent = moveableFolder.transform;
				finishedMergedObjects.Add (mergedObj);
				if(i==0) adjustedPosition = allPosts[0].transform.position;
			}
			//=========== Subs ==============
			List<Transform> subsDividedFolders = GetAllDividedFolders("Subs");
			for(int i=0; i< subsDividedFolders.Count; i++){
				List<GameObject> allSubs = GetAllGameObjectsFromDividedFolder(subsDividedFolders[i]);
				GameObject mergedObj = CombineMeshes(allSubs, "Subs Merged " + i);
				mergedObj.transform.parent = moveableFolder.transform;
				finishedMergedObjects.Add (mergedObj);
			}
			moveableFolder.transform.position = -adjustedPosition;
			mergedCopyFolder.transform.position = adjustedPosition;
			SaveMergedMeshes(finishedMergedObjects);
		}
		EditorGUILayout.Separator();
		showHelp = EditorGUILayout.Foldout(showHelp, "Show Merged-Mesh-Copy Help");
		if(showHelp){
			EditorGUILayout.LabelField("This will create a copy of the Finished folder with each sub-group");
			EditorGUILayout.LabelField("of meshes merged in to a single mesh. Each merged mesh will take 1 drawcall.");
			EditorGUILayout.LabelField("(As with any Game Object, this can increase with Lights/Shadows/Quality-Settings)");
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("In most cases you don't need to use this, as each group has a Combine script");
			EditorGUILayout.LabelField("which will achieve the same thing, while retaining the flexibility of separate meshes.");
			EditorGUILayout.LabelField("However, it can be useful depending on your dynamic/static batching settings,");
			EditorGUILayout.LabelField("and when needing a particular setup for prefab creation or lighmapping.");
		}
	}
	//------------------------------------------
	GameObject CombineMeshes(List<GameObject> allGameObjects, string name){

		CombineInstance[] combiners = new CombineInstance[allGameObjects.Count];
		for(int i=0; i< allGameObjects.Count; i++){
			
			GameObject go = allGameObjects[i].gameObject;
			MeshFilter mf = go.GetComponent<MeshFilter>();
			Mesh mesh = (Mesh) Instantiate( mf.sharedMesh );
			
			Vector3 findMeshCentre = Vector3.zero;
			Vector3[] vertices = mesh.vertices;
			Vector3[] newVerts = new Vector3[vertices.Length];
			//Vector2[] UV2 = mesh.uv2;
			//Vector2[] newUV2 = new Vector2[vertices.Length];
			int v = 0;
			while (v < vertices.Length) {
				
				newVerts[v] = vertices[v];
				//newUV2[v] = UV2[v];
				v++;
			}
			mesh.vertices = newVerts;
			//mesh.uv2 = newUV2;
			combiners[i].mesh = mesh;
			Transform finalTrans = Instantiate(go.transform) as Transform;
			finalTrans.position += go.transform.parent.position;
			combiners[i].transform = finalTrans.localToWorldMatrix;

			findMeshCentre /= vertices.Length;
			DestroyImmediate(finalTrans.gameObject);
			
		}
		Mesh finishedMesh = new Mesh();
		finishedMesh.CombineMeshes(combiners);
		finishedMesh.name = name;
		//Unwrapping.GenerateSecondaryUVSet(selection[i].sharedMesh);
		Unwrapping.GenerateSecondaryUVSet(finishedMesh);

		Material mat = allGameObjects[0].GetComponent<Renderer>().sharedMaterial;
		GameObject mergedObj = new GameObject (name);
		mergedObj.AddComponent<MeshRenderer>();
		mergedObj.GetComponent<Renderer>().sharedMaterial = mat;
		MeshFilter meshFilter = mergedObj.AddComponent<MeshFilter>();
		meshFilter.mesh = finishedMesh;

		return mergedObj;
	}
	//-------------------------------------	
	//Saves the procedurally generated Rail meshes produced when using Sheared mode as prefabs, in order to create a working prefab from the Finished AutoFence
	void SaveMergedMeshes(List<GameObject> finishedGameObjects){
		
		string dateStr = GetPartialTimeString(true);
		string path, folderName = "Meshes-Merged  " + dateStr;
		
		if(!Directory.Exists("Assets/Auto Fence Builder/TempMeshesForFinished")){
			AssetDatabase.CreateFolder("Assets/Auto Fence Builder", "TempMeshesForFinished");
		}
		AssetDatabase.CreateFolder("Assets/Auto Fence Builder/TempMeshesForFinished", folderName);
		path = "Assets/Auto Fence Builder/TempMeshesForFinished/" + folderName + "/";

		int numObjects = finishedGameObjects.Count;
		for(int i=0; i<numObjects; i++){
			Mesh mesh = finishedGameObjects[i].GetComponent<MeshFilter>().sharedMesh;
			if(finishedGameObjects[i] != null && mesh != null){
				if(Directory.Exists(path) ){
					AssetDatabase.CreateAsset(mesh, path + mesh.name);
				}
			}	
		}
		AssetDatabase.SaveAssets();
	}
	//------------------------------------------
	List<GameObject> GetAllGameObjectsFromDividedFolder(Transform dividedFolder){

		int numChildren = dividedFolder.childCount;
		List<GameObject> goList = new List<GameObject>(); 
		for(int i=0; i<numChildren; i++){
			goList.Add (dividedFolder.GetChild(i).gameObject);
		}
		return goList;
	}
	//------------------------------------------
	List<Transform> GetAllDividedFolders(string folderName) 
	{
		GameObject masterFolder = fenceMeshMerge.gameObject;

		Transform mainFolder = masterFolder.transform.Find(folderName);

		if(mainFolder == null) return null;
		
		int numChildren = mainFolder.childCount;
		
		Transform thisChild;
		List<Transform> dividedFolders = new List<Transform>(); 
		for(int i=0; i<numChildren; i++){
			
			thisChild = mainFolder.GetChild(i);
			if(folderName == "Rails" &&  thisChild.name.StartsWith("railsDividedFolder") ){
				
				dividedFolders.Add(thisChild);
			}
			else if(folderName == "Posts" &&  thisChild.name.StartsWith("postsDividedFolder") ){
				
				dividedFolders.Add(thisChild);
			}
			else if(folderName == "Subs" &&  thisChild.name.StartsWith("subsDividedFolder") ){
				
				dividedFolders.Add(thisChild);
			}
		}

		return dividedFolders;
	}
	//---------------------------
	string GetPartialTimeString(bool includeDate = false)
	{
		DateTime currentDate = System.DateTime.Now;
		string timeString = currentDate.ToString();
		timeString = timeString.Replace("/", "-"); // because the / in that will upset the path
		timeString = timeString.Replace(":", "-"); // because the / in that will upset the path
		if (timeString.EndsWith (" AM") || timeString.EndsWith (" PM")) { // windows??
			timeString = timeString.Substring (0, timeString.Length - 3 );
		}
		if(includeDate == false)
			timeString = timeString.Substring (timeString.Length - 8);
		return timeString;
	}

}
