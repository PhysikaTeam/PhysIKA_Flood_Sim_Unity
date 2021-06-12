using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[CustomEditor(typeof(AutoFenceCreator))]
public class AutoFenceEditor : Editor {

	public AutoFenceCreator script;
	private SerializedProperty	 fenceHeight, postSize, postRotation, postHeightOffset;
	private SerializedProperty   numRails, railSize, railPositionOffset, railRotation, autoHideBuriedRails;
	private SerializedProperty 	 /*subsFixedOrProportionalSpacing,*/ subSpacing, showSubs, subSize, subPositionOffset, subRotation, useSubJoiners;
	private SerializedProperty   roundingDistance;
	private SerializedProperty   smooth, showControls, closeLoop, railGaps, frequency, amplitude, wavePosition, useWave;
	private SerializedProperty   interpolate, randomness, addColliders, obj;
	private SerializedProperty	 gs, scaleInterpolationAlso; //global scale
	private SerializedProperty   allowGaps, showDebugGapLine;

	bool oldCloseLoop = false; //doSaveMeshes = false;

	string presetName = "Fence Preset_001";
	public bool undone = false;
	public bool addedPostNow = false, deletedPostNow = false;
	Color	darkCyan = new Color(0, .5f, .75f);
	Color	darkRed = new Color(0.85f, .0f, .0f);
	GUIStyle warningStyle, infoStyle;
    void OnEnable()    
    {
		script = (AutoFenceCreator)target;

		gs = serializedObject.FindProperty("gs");
		scaleInterpolationAlso = serializedObject.FindProperty("scaleInterpolationAlso");

		railGaps = serializedObject.FindProperty("railGaps");
		numRails = serializedObject.FindProperty("numRails");
		railPositionOffset = serializedObject.FindProperty("railPositionOffset");
		railSize = serializedObject.FindProperty("railSize");
		railRotation = serializedObject.FindProperty("railRotation");
		autoHideBuriedRails = serializedObject.FindProperty("autoHideBuriedRails");

		fenceHeight = serializedObject.FindProperty("fenceHeight");
		postHeightOffset = serializedObject.FindProperty("postHeightOffset");
		postSize = serializedObject.FindProperty("postSize");
		postRotation = serializedObject.FindProperty("postRotation");
		randomness = serializedObject.FindProperty("randomness");
		smooth = serializedObject.FindProperty("smooth");
		roundingDistance = serializedObject.FindProperty("roundingDistance");
		interpolate = serializedObject.FindProperty("interpolate");

		//subsFixedOrProportionalSpacing = serializedObject.FindProperty("subsFixedOrProportionalSpacing");
		subSpacing = serializedObject.FindProperty("subSpacing");
		showSubs = serializedObject.FindProperty("showSubs");
		subPositionOffset = serializedObject.FindProperty("subPositionOffset");
		subSize = serializedObject.FindProperty("subSize");
		subRotation = serializedObject.FindProperty("subRotation");
		showControls = serializedObject.FindProperty("showControls");
		closeLoop = serializedObject.FindProperty("closeLoop");
		frequency = serializedObject.FindProperty("frequency");
		amplitude = serializedObject.FindProperty("amplitude");
		wavePosition = serializedObject.FindProperty("wavePosition");
		useWave = serializedObject.FindProperty("useWave");
		useSubJoiners = serializedObject.FindProperty("useSubJoiners");
		addColliders = serializedObject.FindProperty("addColliders");

		allowGaps = serializedObject.FindProperty("allowGaps");
		showDebugGapLine = serializedObject.FindProperty("showDebugGapLine");
		gs.floatValue = 1.0f;
    }
	//---------------
	void DebugInfo() 
	{
		//Debug.Log ("Num ClickPoints = " + script.clickPoints.Count() );
	}
//------------------------------------------
	public override void OnInspectorGUI() 
	{
		serializedObject.Update(); // updates serialized editor from the real script
		script.CheckFolders();

		/*if( GUILayout.Button("Debug Info", GUILayout.Width(100))){ 
			DebugInfo();
		}*/
		warningStyle = new GUIStyle(EditorStyles.label);
		infoStyle = new GUIStyle(EditorStyles.label);
		warningStyle.fontStyle = FontStyle.Bold;
		warningStyle.normal.textColor = darkRed;
		infoStyle.fontStyle = FontStyle.Italic;
		infoStyle.normal.textColor = darkCyan;

		if (Event.current.keyCode == KeyCode.Escape)// cancels a ClearAll
			script.clearAllFencesWarning = 0;
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Shift-Click to place new post on ground.  Insert: Control-Shift-Click", infoStyle);
		EditorGUILayout.LabelField("To delete: enable 'Show Move/Delete Controls and Control-Click", infoStyle);
		EditorGUILayout.Separator();

		//============================
		//		Tidy up after Undo
		//============================
		if ( Event.current.commandName == "UndoRedoPerformed"){
			script.ForceRebuildFromClickPoints();
		}

		//============================
		//	  Finish & Clear Buttons
		//============================
		GUILayout.BeginHorizontal("box");
		
		//bool saving = false;
		if( GUILayout.Button("Finish & Start New", GUILayout.Width(140)) && script.clickPoints.Count > 0){ 

			if(script.allPostsPositions.Count() >0){//Reposition handle at base of first post
				Vector3 currPos = script.fencesFolder.transform.position;
				Vector3 delta = script.allPostsPositions[0] - currPos;
				script.fencesFolder.transform.position = script.allPostsPositions[0];
				script.postsFolder.transform.position = script.allPostsPositions[0] - delta;
				script.railsFolder.transform.position = script.allPostsPositions[0] - delta;
				script.subsFolder.transform.position = script.allPostsPositions[0] - delta;
			}
			SaveProcRailMeshesAsPrefabs();
			script.FinishAndStartNew();
		}

		if(GUILayout.Button("Clear All", GUILayout.Width(140)) && script.clickPoints.Count > 0){ 
			if(script.clearAllFencesWarning == 1){
				script.ClearAllFences();
				script.clearAllFencesWarning = 0;
			}
			else
				script.clearAllFencesWarning = 1;
		}
		if(script.clearAllFencesWarning == 1){
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("   ** This will clear all the fence parts currently being built.", warningStyle);
			EditorGUILayout.LabelField("      Press again to continue or Escape Key to cancel **", warningStyle);
			script.clearAllFencesWarning = 1;
		}
		else /*if(! saving)*/
			GUILayout.EndHorizontal();
		EditorGUILayout.Separator();EditorGUILayout.Separator();
		//============================
		//		Show Controls
		//============================
		GUILayout.BeginHorizontal("box");
		//EditorGUI.BeginChangeCheck();
		showControls.boolValue = script.showControls = EditorGUILayout.BeginToggleGroup(" Show Move/Delete Controls", showControls.boolValue);
		/*if( EditorGUI.EndChangeCheck() ){
			Undo.RecordObject(script, "Show Controls");
		}*/
		EditorGUILayout.EndToggleGroup();
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		//============================
		//		Save Preset
		//============================
		GUILayout.BeginHorizontal("box");
		EditorGUILayout.LabelField("Set Preset Name: ", GUILayout.Width(92));
		presetName = EditorGUILayout.TextField(presetName);
		if(GUILayout.Button("Save Preset", GUILayout.Width(100))){ 
			if(presetName.Length > 4 && script.presetNames.Contains(presetName)) // if untitled, create a new unique name
			{
				string endOfCurrName = presetName.Substring(presetName.Length-4);
				if(endOfCurrName.StartsWith("_")){
					string endDigits = presetName.Substring(presetName.Length-3);
					int n;
					bool isNumeric = int.TryParse(endDigits, out n);
					if(isNumeric){
						int newN = n+1;
						presetName = presetName.Substring(0, presetName.Length-3);
						if(newN < 10) presetName += "00";
						else if(newN < 100) presetName += "0";
						presetName += newN.ToString();
					}
				}
			}
			if(presetName == ""){ // if blank,  name it
				presetName = "Untitled Fence Preset";
			}
			script.SavePresetFromCurrentSettings(presetName);
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();

		//============================
		//		Choose Preset
		//============================
		int oldPreset = script.currentPreset;
		script.currentPreset = EditorGUILayout.IntPopup("Choose Preset", script.currentPreset, script.presetNames.ToArray(), script.presetNums.ToArray());
		if(script.currentPreset != oldPreset){
			script.RebuildFenceFromPreset(script.currentPreset);
		}
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		//============================
		//		Choose Parts
		//============================
		//---------- Post Chooser -----------
		int oldPostType = script.currentPostType;
		script.currentPostType = EditorGUILayout.IntPopup("Choose Post Type", script.currentPostType, script.postNames.ToArray(), script.postNums.ToArray());
		if(script.currentPostType != oldPostType)
			script.SetPostType(script.currentPostType, true);

		//----   Rail Chooser ---------
		int oldRailType = script.currentRailType;
		script.currentRailType = EditorGUILayout.IntPopup("Choose Rail Type", script.currentRailType, script.railNames.ToArray(), script.railNums.ToArray());
		if(script.currentRailType != oldRailType)
			script.SetRailType(script.currentRailType, true);

		//------- SubPost Chooser ----------
		int oldSubType = script.currentSubType;
		script.currentSubType = EditorGUILayout.IntPopup("Choose Sub Type", script.currentSubType, script.subNames.ToArray(), script.subNums.ToArray());
		if(script.currentSubType != oldSubType)
			script.SetSubType(script.currentSubType, true);
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();

		//============================
		//		Randomize
		//============================
		if(GUILayout.Button("Randomize All", GUILayout.Width(110))){ 
			script.Randomize();
		}


		//========================================================
		//						Post OptionsidentifyGaps
		//========================================================
		EditorGUILayout.Separator();
		GUILayout.BeginVertical("box");
		GUIStyle style = new GUIStyle(EditorStyles.label);
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = darkCyan;
		EditorGUILayout.LabelField("Post Options: ", style);
		EditorGUILayout.PropertyField(fenceHeight);
		EditorGUILayout.PropertyField(postHeightOffset);
		EditorGUILayout.PropertyField(postSize); postSize.vector3Value = EnforceVectorMinimums(postSize.vector3Value, new Vector3(0.05f, 0.05f, 0.05f));
		EditorGUILayout.PropertyField(postRotation);
		GUILayout.EndVertical();

		//========================================================
		//						Rails
		//========================================================
		EditorGUILayout.Separator();EditorGUILayout.Separator();
		GUILayout.BeginVertical("box");
		style = new GUIStyle(EditorStyles.label);
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = darkCyan;
		GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Rail Options: ", style);

		if(GUILayout.Button(new GUIContent("Central Y", "Centralise the Rails")/*, GUILayout.Width(20)*/)){
			script.centralizeRails = true;
			script.ForceRebuildFromClickPoints();
		}

		GUILayout.EndHorizontal();
		int oldNumRails = script.numRails;
		EditorGUILayout.PropertyField(numRails);
		if(script.numRails != oldNumRails)
			script.CheckResizePools();
		EditorGUILayout.PropertyField(railGaps);
		EditorGUILayout.PropertyField(railPositionOffset);
		EditorGUILayout.PropertyField(railSize); //EnforceVectorMinimums(railSize.vector3Value, new Vector3(0.05f, 0.05f, 0.05f));
		EditorGUILayout.PropertyField(railRotation);
		EditorGUILayout.PropertyField(autoHideBuriedRails);

		//=============== Slope Mode ================
		string currentRailName = script.railNames[script.currentRailType];
		if(currentRailName.EndsWith("Panel_Rail")) // only make shear available on panel types for efficiency
		{
			AutoFenceCreator.FenceSlopeMode oldSlopeMode = script.slopeMode;
			string[] slopeModeNames = {"Normal Slope", "Stepped", "Sheared"};
			int[] slopeModeNums = {0,1,2};
			script.slopeMode = (AutoFenceCreator.FenceSlopeMode)EditorGUILayout.IntPopup("Slope Mode", (int)script.slopeMode, slopeModeNames, slopeModeNums);
			if(script.slopeMode != oldSlopeMode)
			{
				script.HandleSlopeModeChange();
				script.ForceRebuildFromClickPoints();
			}
		}
		else
			script.slopeMode = AutoFenceCreator.FenceSlopeMode.slope;
		GUILayout.EndVertical();
		EditorGUILayout.Separator();EditorGUILayout.Separator();

		//========================================================
		//						Subs
		//========================================================
		GUILayout.BeginVertical("box");
		style = new GUIStyle(EditorStyles.label);
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = darkCyan;
		EditorGUILayout.LabelField("SubPost Options: ", style);
		EditorGUILayout.PropertyField(showSubs);

		//----- SubPost Spacing Mode -------
		int oldSubsMode = script.subsFixedOrProportionalSpacing;
		string[] subModeNames = {"Fixed Number Between Posts", "Depends on Post Distance"};
		int[] subModeNums = {0,1};

		script.subsFixedOrProportionalSpacing = EditorGUILayout.IntPopup("SubPosts Spacing Mode", script.subsFixedOrProportionalSpacing, subModeNames, subModeNums);

		EditorGUILayout.PropertyField(subSpacing);
		if(script.subsFixedOrProportionalSpacing == 0){// Mode 0 = Fixed, so round the number.
			script.subSpacing = Mathf.Round(script.subSpacing); 
			if(script.subSpacing  < 1){script.subSpacing = 1;}
		}
		if(script.subsFixedOrProportionalSpacing != oldSubsMode)


		//-------------------------------
		EditorGUILayout.PropertyField(subPositionOffset);
		EditorGUILayout.PropertyField(subSize); //EnforceVectoscript.ForceRebuildFromClickPoints();rMinimums(subSize.vector3Value, new Vector3(0.05f, 0.05f, 0.05f));
		EditorGUILayout.PropertyField(subRotation);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("forceSubsToGroundContour"), new GUIContent("Force To Ground Contour"));
		//======= Sub Wave ==========
		EditorGUILayout.PropertyField(useWave);
		EditorGUILayout.PropertyField(frequency);
		EditorGUILayout.PropertyField(amplitude);
		EditorGUILayout.PropertyField(wavePosition);
		EditorGUILayout.PropertyField(useSubJoiners);
		GUILayout.EndVertical();

		EditorGUILayout.Separator(); EditorGUILayout.Separator();
		EditorGUILayout.Separator();EditorGUILayout.Separator();

		//============================
		//	   Global Options
		//============================
		GUILayout.BeginVertical("box");
		style = new GUIStyle(EditorStyles.label);
		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = darkCyan;
		EditorGUILayout.LabelField("Global Options: ", style);
		//============================
		//	   Interpolate
		//============================
		EditorGUILayout.PropertyField(interpolate);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("interPostDist"), new GUIContent("     Distance"));
		EditorGUILayout.Separator();EditorGUILayout.Separator();

		//============================
		//		Smoothing 
		//============================
		GUILayout.BeginVertical("box");
		EditorGUILayout.PropertyField(smooth);
		EditorGUILayout.PropertyField(roundingDistance);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("tension"), new GUIContent("   Corner Tightness"));
		GUIStyle s = new GUIStyle(EditorStyles.label);
		s.fontStyle = FontStyle.Italic; s.normal.textColor = new Color(0.6f, 0.4f, 0.2f);
		GUILayout.Label("Use these to reduce the number of Smoothing posts for performance:", s);
		GUILayout.Label("(It helps to temporarily disable 'Interpolate' to judge the effect of these)", s);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("removeIfLessThanAngle"), new GUIContent("Remove Where Straight"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("stripTooClose"), new GUIContent("Remove Vey Close Posts"));
		GUILayout.EndVertical(); // end smoothing box
		//============================
		//		Close Loop 
		//============================
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.PropertyField(closeLoop);
		if(script.closeLoop != oldCloseLoop)
		{
			Undo.RecordObject(script, "Change Loop Mode");
			script.ManageLoop(script.closeLoop);
			SceneView.RepaintAll();
		}
		oldCloseLoop = script.closeLoop;
		//============================
		//		Randomness
		//============================
		EditorGUILayout.Separator();
		EditorGUILayout.PropertyField(randomness);
		//============================
		//		Add Colliders
		//============================
		EditorGUILayout.Separator();
		EditorGUILayout.PropertyField(addColliders);
		EditorGUILayout.Separator();
		//============================
		//		Gaps
		//============================
		EditorGUILayout.Separator();
		GUILayout.BeginHorizontal("box");
		EditorGUILayout.PropertyField(allowGaps);
		EditorGUILayout.PropertyField(showDebugGapLine);
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		//============================
		//		Global Scale
		//============================
		EditorGUILayout.Separator();
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("gs"), new GUIContent("Global Scale"));
		if (EditorGUI.EndChangeCheck()){
			if(gs.floatValue > .95f && gs.floatValue < 1.05f)
				gs.floatValue = 1.0f;
		}
		EditorGUILayout.PropertyField(scaleInterpolationAlso);
		//-----------------------------
		GUILayout.EndVertical(); // Global Options box

		//================================
		//		Apply Modified Properties
		//================================
		if( serializedObject.ApplyModifiedProperties()){
			script.ForceRebuildFromClickPoints();
		}
	 }
	//-------------------------------------	
	//Saves the procedurally generated Rail meshes produced when using Sheared mode as prefabs, in order to create a working prefab from the Finished AutoFence
	void SaveProcRailMeshesAsPrefabs(){

		List<Mesh> meshBuffers = script.railMeshBuffers;
		List<Transform> rails = script.rails;
		int numRails = script.railCounter;
		string dateStr = script.GetPartialTimeString(true);
		string path, folderName = "TempFenceMeshes " + dateStr;

		if(script.slopeMode == AutoFenceCreator.FenceSlopeMode.shear &&  numRails > 0 && meshBuffers[0] != null && rails[0] != null){

			if(!Directory.Exists("Assets/Auto Fence Builder/TempMeshesForFinished")){
				AssetDatabase.CreateFolder("Assets/Auto Fence Builder", "TempMeshesForFinished");
			}
			AssetDatabase.CreateFolder("Assets/Auto Fence Builder/TempMeshesForFinished", folderName);
			path = "Assets/Auto Fence Builder/TempMeshesForFinished/" + folderName + "/";
		}
		else
			return;

		for(int i=0; i<numRails; i++){
			Mesh mesh = meshBuffers[i];
			EditorUtility.DisplayProgressBar("Saving Meshes...", i.ToString() + " of " + numRails, (float)i/numRails );
			if(rails[i] != null && mesh != null){
				//GameObject thisRail = rails[i].gameObject;
				AssetDatabase.CreateAsset(mesh, path + mesh.name);
				//Object  emptyPrefab = PrefabUtility.CreateEmptyPrefab(path + mesh.name + ".prefab"); // ** uncomment if prefabs are missing
				//PrefabUtility.ReplacePrefab(thisRail, emptyPrefab);  // ** uncomment if prefabs are missing
			}	
		}
		AssetDatabase.SaveAssets();
		//AssetDatabase.Refresh();
		EditorUtility.ClearProgressBar();
	}
	//-------------------------------------	
	void OnSceneGUI()
	{
		script.CheckFolders();
		Event currentEvent = Event.current;
		if(currentEvent.alt)
			return;  	// It's not for us!
		Vector3 clickPoint = Vector3.zero;
		int controlRightClickAddGap = 0; // use 0 instead of a boolean so we can store int flags in clickPointFlags

		//============= Delete Post==============
		if(script.showControls && currentEvent.control && currentEvent.type == EventType.MouseDown && currentEvent.button == 0) // showControls + control-left-click
		{
			Ray ray  = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 2000.0f)){
				string name = hit.collider.gameObject.name;
				if(name.StartsWith("FenceManagerMarker_"))
				{
					Undo.RecordObject(script, "Delete Post");
					string indexStr = name.Remove(0,19);
					int index = Convert.ToInt32(indexStr);
					script.DeletePost(index);
					//deletedPostNow = true;
				}	   
			}
		}
		//============= Toggle Gap Stautus of Post==============
		bool togglingGaps = false;
		if(script.showControls && currentEvent.control && currentEvent.type == EventType.MouseDown && currentEvent.button == 1)// showControls + control-right-click
		{
			Ray ray  = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 2000.0f)){
				string name = hit.collider.gameObject.name;
				if(name.StartsWith("FenceManagerMarker_"))
				{
					Undo.RecordObject(script, "Toggle Gap Status Of Post");
					string indexStr = name.Remove(0,19);
					int index = Convert.ToInt32(indexStr);
					int oldStatus = script.clickPointFlags[index];
					script.clickPointFlags[index] =  1 - oldStatus; // flip 0/1
					script.ForceRebuildFromClickPoints();
					togglingGaps = true;
				}	   
			}
		}
		// I know, some redundant checking, but need to make this extra visible for maintainence, as control-click has two very different effects. 
		if(togglingGaps == false && currentEvent.button == 1 && !currentEvent.shift && currentEvent.control && currentEvent.type == EventType.MouseDown)
		{
			controlRightClickAddGap = 1;// we're inserting a new clickPoint, but as a break/gap
		}
		//============== Add Post ============
		if((!currentEvent.control && currentEvent.shift && currentEvent.type == EventType.MouseDown) || controlRightClickAddGap == 1)
		{
			Ray ray  = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			RaycastHit hit;
			if( Physics.Raycast (ray, out hit, 2000.0f)) {
				if(currentEvent.button == 0 || controlRightClickAddGap == 1){
					Undo.RecordObject(script, "Add Post");
					script.endPoint = Handles.PositionHandle(script.endPoint, Quaternion.identity);
					script.endPoint = hit.point;
					clickPoint = hit.point - new Vector3(0, 0.00f, 0); //bury it in ground as little
					oldCloseLoop = script.closeLoop = false;
					RepositionFolderHandles(clickPoint);
					script.clickPoints.Add(clickPoint); script.clickPointFlags.Add(controlRightClickAddGap); // 0 if normal, 1 if break
					script.keyPoints.Add(clickPoint); // ?? 15/12/2014
					script.ForceRebuildFromClickPoints();
					// copy click points to handle points
					script.handles.Clear();
					for(int i=0; i< script.clickPoints.Count; i++)
					{
						script.handles.Add (script.clickPoints[i] );
					}
					//addedPostNow = true;
					//deletedPostNow = false;
				}
			}
		}
		Selection.activeGameObject = script.gameObject;
		//============= Insert Post ===============
		if(currentEvent.shift && currentEvent.control && currentEvent.type == EventType.MouseDown)
		{
			Ray ray  = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 2000.0f)){
				Undo.RecordObject(script, "Insert Post");
				script.InsertPost(hit.point);
			}
		}
		//======== Handle dragging & controls ============
		if(script.showControls && script.clickPoints.Count > 0)
		{
			bool wasDragged = false;
			// Create handles at every click point
			if(currentEvent.type == EventType.MouseDrag)
			{
				script.handles.Clear();
				script.handles.AddRange(script.clickPoints); // copy them to the handles
				wasDragged = true;
				Undo.RecordObject(script, "Move Post");
			}
			for(int i=0; i < script.handles.Count; i++)
			{
				if(script.closeLoop && i == script.handles.Count-1)// don't make a handle for the last point if it's a closed loop
					continue;
				script.handles[i] = Handles.PositionHandle(script.handles[i] , Quaternion.identity); //allows movement of the handles
				script.clickPoints[i] = script.handles[i];// set new clickPoint position
				script.Ground(script.clickPoints); // re-ground the clickpoints
				script.handles[i] = new Vector3(script.handles[i].x, script.clickPoints[i].y, script.handles[i].z); // set the y position back to the clickpoint (grounded)
			}
			if(wasDragged){
				//Undo.RecordObject(script, "Move Post");
				script.ForceRebuildFromClickPoints();
			}
		}
	}
	//---------------------------------------------------------------------
	// move the folder handles out of the way of the real moveable handles
	void RepositionFolderHandles(Vector3 clickPoint)
	{
		Vector3 pos = clickPoint;
		if(script.clickPoints.Count > 0)
		{
			//pos = (script.clickPoints[0] + script.clickPoints[script.clickPoints.Count-1])/2;
			pos = script.clickPoints[0];
		}
		script.gameObject.transform.position = pos + new Vector3(0,4,0);
		/*script.fencesFolder.transform.position = pos + new Vector3(0,4,0);
		script.postsFolder.transform.position = pos + new Vector3(0,4,0);
		script.railsFolder.transform.position = pos + new Vector3(0,4,0);
		script.subsFolder.transform.position = pos + new Vector3(0,4,0);*/
	}
	//------------------------------------------
	Vector3  EnforceVectorMinimums(Vector3 inVec, Vector3 mins)
	{
		if(inVec.x < mins.x) inVec.x = mins.x;
		if(inVec.y < mins.y) inVec.y = mins.y;
		if(inVec.z < mins.z) inVec.z = mins.z;
		return inVec;
	}
}
