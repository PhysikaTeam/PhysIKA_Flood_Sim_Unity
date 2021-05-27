using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;

[ExecuteInEditMode]
//------------------------------------
[System.Serializable]
public class AutoFenceCreator : MonoBehaviour {

	int objectsPerFolder = 100; // lower this number if using high-poly meshes. Only 65k can be combined, so objectsPerFolder * [number of tris in mesh] must be less than 65,000
	public const float  DEFAULT_RAIL_LENGTH = 3.0f;

	[Range(0.1f, 10.0f)]
	public float gs = 1.0f; //global scale, avoided long name as it occurs so often and takes up space!
	public bool  scaleInterpolationAlso = true; // this can be annoying if you want your posts to stay where they are.

	public enum SplineFillMode {fixedNumPerSpan = 0, equiDistant, angleDependent};
	public enum FencePrefabType {postPrefab = 0, railPrefab};
	public enum FenceSlopeMode {slope = 0, step, shear};

	public Vector3  startPoint = Vector3.zero;
	public Vector3  endPoint =  Vector3.zero;
	Vector3 gapStart = Vector3.zero, gapEnd = Vector3.zero;
	List<Vector3> gaps = new List<Vector3>(); // stores the location of gap start & ends: {start0, end0, start1, end1} etc.
	public bool allowGaps = true, showDebugGapLine = true; // draws a blue line to fill gaps, only in Editor

	int defaultPoolSize = 100;

	public GameObject fencesFolder, postsFolder, railsFolder, subsFolder;
	public	List<GameObject>	folderList = new List<GameObject>();

	// The lists of clones
	private	List<Transform>  posts = new List<Transform>();
	public  List<Transform> rails = new List<Transform>();
	private List<Transform> subs = new List<Transform>();
	private List<Transform> subJoiners = new List<Transform>();
	private	List<Transform>  markers = new List<Transform>();

	public List<Mesh> railMeshBuffers = new List<Mesh>();
	//public List<GameObject> clickMarkers = new List<GameObject>(); // used to select clickpoint posts
	private	List<Vector3>  interPostPositions = new List<Vector3>(); // temp for calculating linear interps
	public	List<Vector3>  clickPoints = new List<Vector3>(); // the points the user clicked, pure.
	public	List<int> 	   clickPointFlags = new List<int>(); //to hold potential extra info about the click points. Not used in v1.1 and below
	public	List<Vector3>  keyPoints = new List<Vector3>(); // the clickPoints + some added primary curve-fitting points
	public	List<Vector3>  allPostsPositions = new List<Vector3>(); // all
	public List<Vector3> handles = new List<Vector3>(); // the positions of the transform handles

	public List<GameObject> postPrefabs = new List<GameObject>();
	public List<GameObject> railPrefabs = new List<GameObject>();
	public List<GameObject> subPrefabs = new List<GameObject>();
	public List<GameObject> subJoinerPrefabs = new List<GameObject>();
	GameObject clickMarkerObj;

	public List<string> presetNames = new List<string>();
	public List<string> postNames = new List<string>();
	public List<string> railNames = new List<string>();
	public List<string> subNames = new List<string>();
	public List<int> presetNums = new List<int>();
	public List<int> postNums = new List<int>();
	public List<int> railNums = new List<int>();
	public List<int> subNums = new List<int>();

	int postCounter = 0, subCounter = 0,  subJoinerCounter = 0;
	public int railCounter = 0;

	public int currentPostType = 0;
	public int currentRailType = 0;
	public int currentRailBType = 0;
	public int currentSubType = 0;
	public int currentSubJoinerType = 0;

	//===== Posts =====
	[Range(0.2f, 10.0f)]
	public float fenceHeight = 2f;
	public Vector3 postSize = Vector3.one;
	[Range(-1.0f, 2.0f)]
	public float postHeightOffset = 0;
	public Vector3 postRotation = Vector3.zero;
	//public Material postMat;// for v2.0
	List<Material> originalPostMaterials = new List<Material>();
	
	//===== Rails =======
	[Range(0, 12)]
	public int numRails = 3;

	bool useSecondaryRails = true;
	[Range(0.1f, 1.0f)]
	public float railGaps = 1.0f,minGap = 0.1f, maxGap = 1.0f;

	public  Vector3 railPositionOffset = Vector3.zero;
	public  Vector3 railSize = Vector3.one;
	public  Vector3 railRotation = Vector3.zero;
	public bool centralizeRails = false;
	List<Material> originalRailMaterials = new List<Material>();
	public bool	autoHideBuriedRails = true;
	

	//======= Subs ========
	public bool showSubs = false;
	public int subsFixedOrProportionalSpacing = 1;
	[Range(0.1f, 20)]
	public float subSpacing = 0.5f;
	public Vector3 subPositionOffset = Vector3.zero;
	public Vector3 subSize = Vector3.one;
	public Vector3 subRotation = Vector3.zero;
	public bool forceSubsToGroundContour =false;
	List<Material> originalSubMaterials = new List<Material>();
	
	public bool useWave = false;
	[Range(0.01f, 10.0f)]
	public float frequency = 1;
	[Range(0.0f, 2.0f)]
	public float amplitude = 0.25f;
	[Range(-Mathf.PI*4, Mathf.PI*4)]
	public float wavePosition = Mathf.PI/2;
	public bool useSubJoiners = false;
	
	//===== Interpolate =========
	public bool  interpolate = true;
	[Range(0.25f, 20.0f)]
	public float interPostDist = 4f;

	//===== Smoothing =========
	public bool smooth = false;
	[Range(0.0f, 1.0f)]
	public float tension = 0.0f;
	[Range(1, 50)]
	public int roundingDistance = 6;
	[Range(0, 45)]
	public float removeIfLessThanAngle = 4.5f;
	[Range(0.02f, 10)]
	public float stripTooClose = 0.35f;
	
	public bool closeLoop = false;
	[Range(0, 0.2f)]
	public float randomness = 0.0f;
	Vector3 preCloseEndPost;
	public bool showControls = false;

	public List<AutoFencePreset> presets = new List<AutoFencePreset>();
	public int currentPreset = 0;
	string presetFilePath  = "Assets/Auto Fence Builder/Editor/AutoFencePresetFiles";
	public Vector3 lastDeletedPoint = Vector3.zero;
	public int lastDeletedIndex = 0;
	public bool addColliders = false;

	List< List<Vector3> > meshOrigVerts = new List< List<Vector3> >();
	List< List<Vector3> > meshOrigNormals = new List< List<Vector3> >();
	List< List<Vector4> > meshOrigTangents = new List< List<Vector4> >();
	List< List<Vector2> > meshOrigUVs = new List< List<Vector2> >();
	List< List<Vector2> > meshOrigUV2s = new List< List<Vector2> >();
	List< List<int> > meshOrigTris = new List< List<int> >();
	List<string> railMeshNames = new List<string>();
	List<Mesh> origRailMeshes = new List<Mesh>();

	public FenceSlopeMode slopeMode = FenceSlopeMode.slope;
	public int 	clearAllFencesWarning = 0;

	//=====================================================
	//					Reset
	//=====================================================
	public void Reset () {

		DestroyPools();
		keyPoints.Clear ();
		LoadAllParts();
		SetupFolders();
		DestroyPools();
		CreatePools();

		fenceHeight = 2.4f;
		numRails = 2;
		railGaps = 0.3f;
		railPositionOffset.y = 0.36f;
		currentPostType = FindPrefabByName(FencePrefabType.postPrefab, "Angled_Post");
		currentRailType = FindPrefabByName(FencePrefabType.railPrefab,"CylinderSlim_Rail");
		currentSubType = FindPrefabByName(FencePrefabType.postPrefab,"SlimCylinder_Post");
		subSize.y = 0.36f;
		roundingDistance = 6;
		SetPostType(currentPostType, false);
		SetRailType(currentRailType, false);
		SetSubType(currentSubType, false);
		centralizeRails = true;
		slopeMode = FenceSlopeMode.slope;
		interpolate = true;
		interPostDist = 4;
		autoHideBuriedRails = true;

		ReadPresetFiles();

		RebuildFenceFromPreset(0);
	}
	//--------------------------
	public void FinishAndStartNew()
	{
		GameObject currentFolder = GameObject.Find("Current Fences Folder");
		if(currentFolder != null)
			currentFolder.name = "Finished AutoFence";
		DestroyUnused();

		FenceMeshMerge fenceMeshMerger = currentFolder.AddComponent<FenceMeshMerge>();

		//-- Clear the references to the old parts ---
		clickPoints.Clear(); clickPointFlags.Clear();
		keyPoints.Clear ();
		posts.Clear();
		rails.Clear();
		subs.Clear();
		subJoiners.Clear();
		closeLoop = false;
		gaps.Clear();

		fencesFolder = null; // break the reference to the old folder

		SetupFolders();
		railMeshBuffers = new List<Mesh>(); //so that we don't destroy or overwrite the old ones
		CreatePools();
	}
	//--------------------------
	public void ClearAllFences()
	{
		clickPoints.Clear(); clickPointFlags.Clear();
		keyPoints.Clear ();
		DestroyPools();
		CreatePools();
		DestroyMarkers();
		closeLoop = false;
	}
	//--------------------------
	void Awake()
	{
		GameObject existingFolder = GameObject.Find("Current Fences Folder");
		if(existingFolder != null)
		{
			if (Application.isEditor)
			{
				fencesFolder = existingFolder;
				DestroyImmediate(existingFolder);
				SetupFolders();
				LoadAllParts();
				DestroyPools();
				CreatePools();
				SetMarkersActiveStatus(showControls);
				ForceRebuildFromClickPoints();
			}
			else if (Application.isPlaying){
					SetMarkersActiveStatus(false);
			}
		}
	}
	//=================================================
	//				Create/Destroy Folders
	//=================================================
	public void SetupFolders(){
		
		// Make the Current Fences folder 
		if(fencesFolder == null){
			fencesFolder = new GameObject("Current Fences Folder");
			folderList.Add(fencesFolder);
			//?Selection.activeGameObject = this.gameObject;
		}
		if(fencesFolder != null){ // if it's already there, destroy sub-folders before making new ones
			int childs = fencesFolder.transform.childCount;
			for (int i = childs - 1; i >= 0; i--)
			{
				GameObject subFolder = fencesFolder.transform.GetChild(i).gameObject;
				int grandChilds = subFolder.transform.childCount;
				for (int j = grandChilds - 1; j >= 0; j--)
				{
					GameObject.DestroyImmediate(subFolder.transform.GetChild(j).gameObject);
				}
				DestroyImmediate(subFolder);
			}
		}
		postsFolder = new GameObject("Posts");
		postsFolder.transform.parent = fencesFolder.transform;
		railsFolder = new GameObject("Rails");
		railsFolder.transform.parent = fencesFolder.transform;
		subsFolder = new GameObject("Subs");
		subsFolder.transform.parent = fencesFolder.transform;
	}
	//--------------------------
	//Do this when necessary to check the user hasn't deleted the current working folder
	public void CheckFolders(){
		if(fencesFolder == null){
			SetupFolders();
			ClearAllFences();
		}
		else{
			if(postsFolder == null){
				postsFolder = new GameObject("Posts");
				postsFolder.transform.parent = fencesFolder.transform;
				ResetPostPool();
			}
			if(railsFolder == null){
				railsFolder = new GameObject("Rails");
				railsFolder.transform.parent = fencesFolder.transform;
				ResetRailPool();
			}
			if(subsFolder == null){
				subsFolder = new GameObject("Subs");
				subsFolder.transform.parent = fencesFolder.transform;
				ResetSubPool();
			}
		}
	}
	//---------------------------
	// Reads all the preset files and converts each one in to a string array
	// We save the presets as individual files to make it easier to transfer a preset (one simple .txt file) to another project
	public void ReadPresetFiles() 
	{
		string[] filePaths = null;
		try
		{
			filePaths = Directory.GetFiles(presetFilePath);
		}
		catch (System.Exception e)
		{
			print("Missing Presets Folder. Have you moved or renamed the Auto Fence Builder Folder or its contents?\n The Auto Fence Builder folder must be in the top level of your assets folder, " +
				"with the scripts and its Editor folder inside. This Editor folder should contain the AutoFencePresetFiles folder.  " + e.ToString());
			return;
		}
		foreach(string filePath in filePaths)
		{
			if( filePath.Contains("AutoFencePreset_")  && filePath.EndsWith(".txt")  )
			{
				string[] values = new string[50];
				values = File.ReadAllLines(filePath);
				CreatePresetFromStringValuesArray(values);
			}
		}
		presets = presets.OrderBy(o=>o.name).ToList();
		CreatePresetStringsForMenus();
	}

	//---------------------------
	// Reads from an array of strings and creates an AutoFence preset
	public void CreatePresetFromStringValuesArray(string[] readValues)
	{
		presets.Add ( new AutoFencePreset(	readValues[0], //name
		                                  FindPrefabByName(FencePrefabType.postPrefab, readValues[1]), 
		                                  FindPrefabByName(FencePrefabType.railPrefab, readValues[2]), 
		                                  FindPrefabByName(FencePrefabType.postPrefab, readValues[3]), 
		                                  float.Parse(readValues[4]), float.Parse(readValues[5]), ParseVector3(readValues[6]), ParseVector3(readValues[7]), //post
		                                  int.Parse(readValues[8]), float.Parse(readValues[9]),  ParseVector3(readValues[10]), ParseVector3(readValues[11]), ParseVector3(readValues[12]), //rails
		                                  bool.Parse(readValues[13]), int.Parse(readValues[14]),  float.Parse(readValues[15]), //subs
		                                  ParseVector3(readValues[16]), ParseVector3(readValues[17]), ParseVector3(readValues[18]), //suns
		                                  bool.Parse(readValues[19]), float.Parse(readValues[20]),  float.Parse(readValues[21]), float.Parse(readValues[22]), bool.Parse(readValues[23]),//subs
		                                  bool.Parse(readValues[24]), float.Parse(readValues[25]), // interpolate
		                                  bool.Parse(readValues[26]), float.Parse(readValues[27]),  int.Parse(readValues[28]), //smooth
		                                  bool.Parse(readValues[29]), float.Parse(readValues[30]),  //forceSubsToGroundContour, randomness
		                                  float.Parse(readValues[31]), float.Parse(readValues[32])
		                                  /*readValues[33], readValues[34], readValues[35]*/ //material names for v2.0
		                                  )
		             );
	}
	//---------------------------
	// Fiest save it as an internal preset, then save it to disk
	public void SavePresetFromCurrentSettings(string name)
	{
		if(presetNames.Contains(name))
			name += "-";
		
		presets.Add ( new AutoFencePreset(name, currentPostType, currentRailType, currentSubType, 
		                                  fenceHeight, postHeightOffset, postSize , postRotation,
		                                  numRails, railGaps, railPositionOffset, railSize, railRotation,
		                                  showSubs, subsFixedOrProportionalSpacing, subSpacing,
		                                  subPositionOffset, subSize, subRotation,
		                                  useWave, frequency, amplitude, wavePosition, useSubJoiners,
		                                  interpolate, interPostDist,
		                                  smooth, tension, roundingDistance,
		                                  forceSubsToGroundContour, randomness,
		                                  removeIfLessThanAngle, stripTooClose
		                                  /*postMat.name, railMat.name, subMat.name*/ // for v2.0
		                                  ) );

		presets = presets.OrderBy(o=>o.name).ToList();



		SavePresetToPresetsFolder(name);
		if(presets.Count < 5){
			presets.Clear();
			ReadPresetFiles();
		}

		CreatePresetStringsForMenus();
		currentPreset = FindPresetByName(name); // sets this as current
	}
	//---------------
	int	FindPresetByName(string name)
	{
		for(int i=0; i<presetNames.Count; i++){
			if(presetNames[i] == name)
				return i;
		}
		return -1;
	}
	//---------------
	void	CreatePresetStringsForMenus()
	{
		presetNames.Clear (); presetNums.Clear ();
		for(int i=0; i<presets.Count; i++){
			
			if(presetNames.Contains(presets[i].name))
				presets[i].name += "+";
			presetNames.Add (presets[i].name);
			presetNums.Add(i);
		}
	}
	//---------------------------
	// Saves a single preset to a .txt file
	public void SavePresetToPresetsFolder(string name)
	{
		List<string> parameters = new List<string>();
		
		parameters.Add(name);
		parameters.Add(postPrefabs[currentPostType].name);
		parameters.Add(railPrefabs[currentRailType].name);
		parameters.Add(subPrefabs[currentSubType].name);
		//---- Posts -----
		parameters.Add(fenceHeight.ToString("F3"));
		parameters.Add(postHeightOffset.ToString("F3"));
		parameters.Add(VectorToString(postSize));
		parameters.Add(VectorToString(postRotation));
		//---- Rails ---
		parameters.Add(numRails.ToString());
		parameters.Add(railGaps.ToString());
		parameters.Add(VectorToString(railPositionOffset));
		parameters.Add(VectorToString(railSize));
		parameters.Add(VectorToString(railRotation));
		//---- Subs -----
		parameters.Add(showSubs.ToString());
		parameters.Add(subsFixedOrProportionalSpacing.ToString());
		parameters.Add(subSpacing.ToString("F3"));
		parameters.Add(VectorToString(subPositionOffset));
		parameters.Add(VectorToString(subSize));
		parameters.Add(VectorToString(subRotation));
		parameters.Add(useWave.ToString());
		parameters.Add(frequency.ToString("F3"));
		parameters.Add(amplitude.ToString("F3"));
		parameters.Add(wavePosition.ToString("F3"));
		parameters.Add(useSubJoiners.ToString());
		//---- Global -----
		parameters.Add(interpolate.ToString());
		parameters.Add(interPostDist.ToString());
		parameters.Add(smooth.ToString());
		parameters.Add(tension.ToString("F3"));
		parameters.Add(roundingDistance.ToString());
		
		parameters.Add(forceSubsToGroundContour.ToString());
		
		parameters.Add(randomness.ToString("F3"));
		
		parameters.Add(removeIfLessThanAngle.ToString("F3"));
		parameters.Add(stripTooClose.ToString("F3"));

		string presetText = "";
		for(int i = 0; i < parameters.Count; i++)
		{
			presetText += parameters[i] + "\r";
		}
		if(!Directory.Exists(presetFilePath))
			Directory.CreateDirectory(presetFilePath);

#if !UNITY_WEBPLAYER 
		string timeString = GetPartialTimeString(); // add a time in case a duplicate filename already exists
		string path = presetFilePath + "/AutoFencePreset_" + name + "_" + timeString + ".txt";
		File.WriteAllText(path, presetText);
#endif
	}
	//==================================================
	//		Assign Meshes & Materials To Game Objects
	//==================================================
	// re-wrote 3/12/14 for compatibility with web player
	void	LoadAllParts()
	{
		// Load posts, rails & Subposts
		GameObject go;
		UnityEngine.Object[] allPrefabs = Resources.LoadAll("FencePrefabs");
		foreach(UnityEngine.Object obj in allPrefabs)
		{	
			go = obj as GameObject;
			if(go != null && go.GetComponent<Renderer>() != null)
			{
				//print (obj.name); // useful if adding custom parts to check they're loading
				if(go.GetComponent<MeshFilter>() != null &&  go.GetComponent<MeshFilter>().sharedMesh != null)
				{
					if(obj.name.EndsWith("_Post"))
					{	
						postPrefabs.Add(go);
						originalPostMaterials.Add (go.GetComponent<Renderer>().sharedMaterial);
						subPrefabs.Add(go);
						originalSubMaterials.Add(go.GetComponent<Renderer>().sharedMaterial);
					}
					else if(obj.name.EndsWith("_Rail"))
					{	
						railPrefabs.Add(go);
						originalRailMaterials.Add(go.GetComponent<Renderer>().sharedMaterial);
					}
					else if(obj.name.EndsWith("_SubJoiner"))
					{	
						subJoinerPrefabs.Add(go);
					}
				}
			}
		}
		SaveRailMeshes();
		CreatePartStringsForMenus();

		string markerPath = "FencePrefabs/ClickMarkerObj";
		clickMarkerObj = Resources.Load<GameObject>(markerPath);
		if(clickMarkerObj == null)
			print ("Can't load clickMarkerObj");
	}
	//---------------
	void	CreatePartStringsForMenus()
	{
		postNames.Clear ();
		postNums.Clear ();
		int numPostTypes = postPrefabs.Count;
		for(int i=0; i<numPostTypes; i++){
			postNames.Add ( postPrefabs[i].name);
			postNums.Add (i);
		}
		railNames.Clear ();
		railNums.Clear ();
		int numRailTypes = railPrefabs.Count;
		for(int i=0; i<numRailTypes; i++){
			railNames.Add ( railPrefabs[i].name);
			railNums.Add (i);
		}
		subNames.Clear ();
		subNums.Clear ();
		int numSubTypes = subPrefabs.Count;
		for(int i=0; i<numSubTypes; i++){
			subNames.Add ( subPrefabs[i].name);
			subNums.Add (i);
		}
	}
	//---------------------------
	public void RebuildFenceFromPreset(int presetIndex)
	{
		// if presets are missing, try reloading them, or give up
		if(presetIndex < 0 || presetIndex >= presets.Count){
			presets.Clear ();
			ReadPresetFiles(); 
			if(presetIndex < 0 || presetIndex >= presets.Count){
				print ("Presets missing. Have they been deleted or moved from Assets/Auto Fence Builder/Editor/AutoFencePresetFiles/ ?");
				return;
			}
		}
		AutoFencePreset preset = presets[presetIndex];
		if(preset == null) return;
		currentPostType = preset.postType;
		currentRailType = preset.railType;
		currentSubType = preset.subType;
		fenceHeight = preset.fenceHeight;
		
		postHeightOffset = preset.postHeightOffset;
		postSize = preset.postSize;
		postRotation = preset.postRotation;
		
		numRails = preset.numRails;
		railGaps = preset.railGaps;
		railPositionOffset = preset.railPositionOffset;
		railSize = preset.railSize;
		railRotation = preset.railRotation;
		
		showSubs = preset.showSubs;
		subsFixedOrProportionalSpacing = preset.subsFixedOrProportionalSpacing;
		subSpacing = preset.subSpacing;
		subSize = preset.subSize;
		subPositionOffset = preset.subPositionOffset;
		subRotation = preset.subRotation;
		useWave = preset.useWave;
		frequency = preset.frequency;
		amplitude = preset.amplitude;
		wavePosition = preset.wavePosition;
		useSubJoiners = preset.useJoiners;
		
		interPostDist = preset.interPostDistance;

		forceSubsToGroundContour = preset.forceSubsToGroundContour;
		randomness = preset.randomness;

		SetPostType(currentPostType, false);
		SetRailType(currentRailType, false);
		SetSubType(currentSubType, false);
		ForceRebuildFromClickPoints();
	}
	//---------------------------
	public void Randomize()
	{
		currentPostType = (int)(UnityEngine.Random.value * postPrefabs.Count);
		currentRailType = (int)(UnityEngine.Random.value * railPrefabs.Count);
		string railName = railPrefabs[currentRailType].name;
		currentSubType = (int)(UnityEngine.Random.value * subPrefabs.Count);

		fenceHeight = (UnityEngine.Random.Range(0.5f, 3.5f)+UnityEngine.Random.Range(0.5f, 4.0f)+UnityEngine.Random.Range(0.5f, 4.0f)+UnityEngine.Random.Range(0.5f, 4.0f))/4;

		railSize.y = 1;
		if(railName.EndsWith("_Panel_Rail")){
		  	numRails = 1;
			railSize.y = fenceHeight * 0.85f;
		}
		else
			numRails = ((int) (((UnityEngine.Random.value * 4) + (UnityEngine.Random.value * 4) + (UnityEngine.Random.value * 4))/3))+1; //psuedo-central distribution 

		railGaps = UnityEngine.Random.value * 0.6f + 0.2f;
		//------ Centralize ---------
		float gap = fenceHeight;
		if(numRails > 1)
			gap = fenceHeight/(numRails-1);
		gap *= railGaps;
		float maxY = (1 - railGaps) * 0.9f;
		railPositionOffset.y = UnityEngine.Random.Range (0.1f, maxY);
		if(railName.EndsWith("_Panel_Rail"))
			railPositionOffset.y = 0.51f;
		//-----------------------------------------
		showSubs = false;
		if( Mathf.Round(UnityEngine.Random.value) > 0.5f)
		{
			showSubs = true;
			subSpacing = UnityEngine.Random.Range (0.4f, 5);
			subSize.y = UnityEngine.Random.Range (0.5f, 1);
		}
		SetPostType(currentPostType, false);
		SetRailType(currentRailType, false);
		SetSubType(currentSubType, false);
		ForceRebuildFromClickPoints();
	}
	//-------------------------------
	public void SetPostType(int type, bool doRebuild)
	{
		currentPostType = type;
		//postMat = originalPostMaterials[type]; // for v2.0
		DeactivateEntirePool();
		ResetPostPool();

		if(doRebuild)
			ForceRebuildFromClickPoints();
	}
	//---------------
	public void SetRailType(int railType, bool doRebuild)
	{
		FenceSlopeMode oldSlopeMode = slopeMode;
		currentRailType = railType;
		if(railNames[currentRailType].EndsWith("Panel_Rail") == false)
			slopeMode = FenceSlopeMode.slope;
		else{ // always change to 'shear' for panel fences
			slopeMode = FenceSlopeMode.shear;
			if(slopeMode != oldSlopeMode){
				HandleSlopeModeChange();
				doRebuild = true;
			}
		}
		DeactivateEntirePool();
		ResetRailPool();
		if(doRebuild)
			ForceRebuildFromClickPoints();

		//HandleSlopeModeChange();//refresh the meshes in case we're changing from sheared to non-sheared 
	}
	//-----------------
	public void SetSubType(int type, bool doRebuild)
	{
		currentSubType = type;
		DeactivateEntirePool();
		ResetSubPool();
		if(doRebuild)
			ForceRebuildFromClickPoints();
	}
	//------------------
	// Backup ALL possible mesh data in case any of it gets mangled
	// These are saved in Lists of Lists. We're not using all of them now, but could as editing features increase.
	void SaveRailMeshes()
	{
		meshOrigVerts.Clear();
		meshOrigNormals.Clear();
		meshOrigTangents.Clear();
		meshOrigUVs.Clear();
		meshOrigUV2s.Clear();
		meshOrigTris.Clear();
		railMeshNames.Clear();
		origRailMeshes.Clear();
		for(int i=0; i< railPrefabs.Count(); i++)
		{
			List<Vector3> vertList = new List<Vector3>();
			vertList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.vertices);
			meshOrigVerts.Add (vertList);
			
			List<Vector3> normalList = new List<Vector3>();
			normalList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.normals);
			meshOrigNormals.Add (normalList);
			
			List<Vector4> tangentList = new List<Vector4>();
			tangentList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.tangents);
			meshOrigTangents.Add (tangentList);
			
			List<Vector2> uvList = new List<Vector2>();
			uvList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.uv);
			meshOrigUVs.Add (uvList);

			List<Vector2> uv2List = new List<Vector2>();
			uv2List.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.uv2);
			meshOrigUV2s.Add (uv2List);
			
			List<int> triList = new List<int>();
			triList.AddRange(railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.triangles);
			meshOrigTris.Add (triList);

			railMeshNames.Add (railPrefabs[i].GetComponent<MeshFilter>().sharedMesh.name);
			origRailMeshes.Add (railPrefabs[i].GetComponent<MeshFilter>().sharedMesh);
		}
	}
	//----------------
	public void HandleSlopeModeChange()
	{
		if(meshOrigVerts.Count == 0)
			SaveRailMeshes();
		// Get all the mesh data from the pre-saved copies
		for(int i=0; i<railCounter; i++)
		{
			if(rails[i] != null)
			{
				rails[i].GetComponent<MeshFilter>().sharedMesh = origRailMeshes[currentRailType];
				rails[i].GetComponent<MeshFilter>().sharedMesh.vertices = meshOrigVerts[currentRailType].ToArray();
				rails[i].GetComponent<MeshFilter>().sharedMesh.normals = meshOrigNormals[currentRailType].ToArray();
				rails[i].GetComponent<MeshFilter>().sharedMesh.tangents = meshOrigTangents[currentRailType].ToArray();
				rails[i].GetComponent<MeshFilter>().sharedMesh.uv = meshOrigUVs[currentRailType].ToArray();
				rails[i].GetComponent<MeshFilter>().sharedMesh.uv2 = meshOrigUV2s[currentRailType].ToArray();
				rails[i].GetComponent<MeshFilter>().sharedMesh.triangles = meshOrigTris[currentRailType].ToArray();
			}
		}
	}
	//--------------------
	// Called when the layout is required to change
	// Creates, the interpolated pots, the smoothing curve and
	// then calls RebuildFromFinalList where the fence gets put together
	public void	ForceRebuildFromClickPoints()
	{
		if(clickPoints.Count == 0){
			DeactivateEntirePool();
			return;
		}
		if(clickPoints.Count == 1) // the first post doesn't need anything else
		{
			DeactivateEntirePool();
			allPostsPositions.Clear();
			keyPoints.Clear ();
			keyPoints.AddRange(clickPoints);
			AddNextPostAndInters(keyPoints[0], false, true);
			return;
		}
		MergeClickPointGaps();
		DeactivateEntirePool();
		allPostsPositions.Clear();
		keyPoints.Clear ();
		keyPoints.AddRange(clickPoints);
		MakeSplineFromClickPoints();
		startPoint = keyPoints[0];
		AddNextPostAndInters(keyPoints[0], false, false);
		
		for(int i=1; i<keyPoints.Count; i++)
		{
			endPoint = keyPoints[i];
			AddNextPostAndInters(keyPoints[i], true, false);
			startPoint = keyPoints[i];
		}
		RemoveDiscontinuityBreaksFromAllPostPosition();
		RebuildFromFinalList();
		centralizeRails = false;
	}
	//--------------------
	// If there are multiple contiguous gaps found, merge them in to 1 gap by deleting the previous point
	void MergeClickPointGaps(){

		for(int i=2; i<clickPointFlags.Count(); i++){

			if(clickPointFlags[i]== 1 && clickPointFlags[i-1] == 1) // tow together so keep the last one, deactivate the first one
				DeletePost(i-1, false);
		}
	}
	//-----------------------------------------------
	// Where the use asked for a break, we remove all inter/spline posts between the break-clickPoint and the previous clickPoint
	void RemoveDiscontinuityBreaksFromAllPostPosition(){

		if(allowGaps == false || clickPoints.Count < 3) return;

		Vector3 breakPoint, previousValidClickPoint = clickPoints[2];
		int clickPointIndex=0, breakPointIndex=-1, previousValidIndex= 1;

		List<int> removePostsIndices = new List<int>() ;

		for(int i=2; i<allPostsPositions.Count; i++){ // the first two can not be break points, as they are the minimum 1 single section of fence
			Vector3 thisPostPos = allPostsPositions[i];
			clickPointIndex = clickPoints.IndexOf(thisPostPos);
			if( clickPointIndex != -1) { // it's a clickPoint!
				if(clickPointFlags[clickPointIndex] == 1){ // it's a break point!
					breakPointIndex = i; // we will remove all the post between this and previousValidIndex
					for(int r=previousValidIndex+1; r <breakPointIndex; r++){
						if(removePostsIndices.Contains(r) == false)
							removePostsIndices.Add (r);
					}
				}
				else
					previousValidIndex = i;
			}
		}

		for(int i=removePostsIndices.Count-1; i>=0;  i--){ // comment this out to disable breakPoints
			allPostsPositions.RemoveAt(removePostsIndices[i]);
		}
	}
	//------------------------
	bool IsBreakPoint(Vector3 pos){
		
		int clickPointIndex = clickPoints.IndexOf(pos);
		if( clickPointIndex != -1) { // it's a clickPoint!
			if(clickPointFlags[clickPointIndex] == 1){ // it's a break point!
				return true;
			}
		}
		return false;
	}
	//------------------------
	// This is where the gameobjects actually get built
	public void	RebuildFromFinalList()
	{ 
		postCounter = 0;
		railCounter = 0;
		subCounter = 0;
		subJoinerCounter = 0;
		Vector3 A = Vector3.zero, B;
		//Check if we need to increase the pool size before we do any building
		CheckResizePools(); // this will also rebuild the sheared railMeshBuffers via RereateRailMeshes()
		SetMarkersActiveStatus(showControls);
		gaps.Clear ();
		
		// clean up memory of sheared meshes
		if(slopeMode != FenceSlopeMode.shear)
			DestroyBufferedRailMeshes();
		else
			RereateRailMeshes(); // already been created in CheckResizePools()
		
		for(int i=0; i<allPostsPositions.Count; i++)
		{
			if(i>0){
				A = allPostsPositions[i-1];
			}
			B = allPostsPositions[i];
			SetupPost(i, B); // Posts are created here
			if(i > 0){
				if(A == B){
					print ("Warning: Posts A & B are in identical positions");
				}
				else if(IsBreakPoint(allPostsPositions[i]) == false || allowGaps == false){
					CreateRailsAndSubPostsForSection(A, B);
				}
				else{
					gaps.Add(A);
					gaps.Add(B);
				}
			}
			postCounter++;
		}
		RotatePostsFinal(); //rotate each post to correctly follow the fence direction
		// post colliders only get built if the single rail in each section has not already built a collider for the whole fence section
		for(int i=0; i<allPostsPositions.Count; i++)
		{
			if(posts[i] != null)
				CreateCollider(posts[i].gameObject); // we need to do this after rotating the posts to ensure correct orientation
		}
	}
	//------------
	void OnDrawGizmos(){

		Color lineColor = new Color(.1f, .1f, 1.0f, 0.4f);
		Gizmos.color = lineColor;
		Vector3 a = Vector3.zero, b = Vector3.zero;
		if(showDebugGapLine && allowGaps){
			for(int i=0; i< gaps.Count(); i += 2){
				a = gaps[i]; a.y += 0.8f;
				b = gaps[i+1]; b.y += 0.8f;
				Gizmos.DrawLine(a, b); // draw a line to show user gaps
				a.y += 0.3f;
				b.y += 0.3f;
				Gizmos.DrawLine(a, b); 
				a.y += 0.3f;
				b.y += 0.3f;
				Gizmos.DrawLine(a, b); 
			}
		}
	}
	//------------
	public void	CreateCollider(GameObject postA)
	{
		BoxCollider postCollider = postA.GetComponent<BoxCollider>();
		
		if(postCollider == null && addColliders == false) // not needed, so return
			return;
		else if(postCollider != null && (addColliders == false || numRails >0)){ // not needed, but exist, so deactivate and return
			DestroyImmediate(postCollider);
			return;
		}
		if(postCollider != null)
			DestroyImmediate(postCollider);
		
		postCollider = (BoxCollider)postA.AddComponent(typeof(BoxCollider));
	}
	//------------
	public void	CreateRailCollider(GameObject rail)
	{
		BoxCollider railCollider = rail.GetComponent<BoxCollider>();

		if(railCollider == null && addColliders == false) // not needed, so return
			return;
		else if(railCollider != null && addColliders == false){ // not needed, but exist, so deactivate and return
			DestroyImmediate(railCollider);
			return;
		}

		if(railCollider != null)
			DestroyImmediate(railCollider);

		railCollider = (BoxCollider)rail.AddComponent(typeof(BoxCollider));
		if(railCollider != null){
			railCollider.enabled = true;	
			Vector3 newCenter = railCollider.center;
			newCenter.y = (fenceHeight/2) - (railPositionOffset.y * fenceHeight);
			newCenter.y *= gs;
			railCollider.center = newCenter;
			Vector3 newSize = railCollider.size;
			newSize.y = fenceHeight * gs;
			railCollider.size = newSize;
		}
	}
	//------------
	public Mesh CombineRailMeshes(){

		CombineInstance[] combiners = new CombineInstance[railCounter];

		for(int i=0; i< railCounter; i++){

			GameObject thisRail = rails[i].gameObject;
			MeshFilter mf = thisRail.GetComponent<MeshFilter>();
			Mesh mesh = (Mesh) Instantiate( mf.sharedMesh );

			Vector3[] vertices = mesh.vertices;
			Vector3[] newVerts = new Vector3[vertices.Length];
			int v = 0;
			while (v < vertices.Length) {

				newVerts[v] = vertices[v];
				v++;
			}
			mesh.vertices = newVerts;

			combiners[i].mesh = mesh;

			Transform finalTrans = Instantiate(thisRail.transform) as Transform;
			finalTrans.position += thisRail.transform.parent.position;
			combiners[i].transform = finalTrans.localToWorldMatrix;
			DestroyImmediate(finalTrans.gameObject);
		}

		Mesh finishedMesh = new Mesh();
		finishedMesh.CombineMeshes(combiners);

		return finishedMesh;
	}
	//------------
	// This the real meat of the thing where the fence get's assembled
	public void	CreateRailsAndSubPostsForSection(Vector3 A, Vector3 B)
	{
		Vector3 eulerDirection = CalculateDirection(A, B), vectorDir = A-B;
		float distance = Vector3.Distance(A, B);
		float r = randomness;
		float gap = fenceHeight *gs;
		Vector3 P = A, Q = B;

		P.y = Q.y = 0;
		float groundDistance  = Vector3.Distance(P, Q);
		float heightDelta = A.y - B.y; //ground position delta

		if(numRails > 1)
			gap = (fenceHeight*gs)/(numRails-1);
		gap *= railGaps;
		float heightOffset = fenceHeight * railPositionOffset.y * gs;
		
		if(centralizeRails == true) // did the user click the centralizeY button
		{
			if(numRails > 1){
				float totalRailsheight = fenceHeight * railGaps * gs;
				float d = ((fenceHeight-totalRailsheight)/2) * gs;
				railPositionOffset.y  = heightOffset =  d;
				railPositionOffset.y /= (fenceHeight * gs);
			}
			else{
				railPositionOffset.y =  0.5f;
				heightOffset = (fenceHeight * gs) * railPositionOffset.y;
			}
		}
		else
			heightOffset += 0.01f;// to keep the lowest one off the ground with default settings

		//Start looping through each Rail in the section
		for(int i=0; i<numRails; i++)
		{
			bool	omit = false;
			GameObject thisRail = rails[railCounter].gameObject;
			if(thisRail == null){
				print ("Missing Rail " + i + " Have you deleted one?");
				continue;
			}
			thisRail.gameObject.layer = 2; //raycast ignore colliders, we turn it on again at the end
			thisRail.hideFlags = HideFlags.None;
			thisRail.SetActive(true);
			thisRail.transform.rotation = Quaternion.identity;
			thisRail.transform.position = B + new Vector3(0, (gap*i)+heightOffset, 0);
			if(slopeMode == FenceSlopeMode.step)
				thisRail.transform.position += new Vector3(0, heightDelta, 0);// for stepped rails, use the previous post's height instead;

			thisRail.transform.Rotate(new Vector3(0, -90, 0));// varies depending on which software created the mesh!!
			thisRail.transform.Rotate(new Vector3(0, eulerDirection.y, 0));
			if(slopeMode == FenceSlopeMode.slope)
				thisRail.transform.Rotate(new Vector3(0,0,-eulerDirection.x)); //Incline. Er... z = x. What? v2.0 Make this cutomizable to support all modeller software.

			thisRail.transform.Translate(railPositionOffset.x, 0, railPositionOffset.z*gs);
			
			float zRot = railRotation.z/distance;
			thisRail.transform.Rotate(new Vector3(railRotation.x, railRotation.y, zRot), Space.Self);
			
			Vector3 scale = Vector3.one;
			if(randomness > 0.0001f)
				scale += new Vector3(0, UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r));
			
			if(slopeMode == FenceSlopeMode.slope)
				scale.x *= (distance/3.0f) * railSize.x;
			else
				scale.x *= (groundDistance/3.0f) * railSize.x;
			if(slopeMode != FenceSlopeMode.shear)//don't scale raillSize.y if sheared, as the vertices are explicitly set instead
				scale.y *= railSize.y * gs;

			//If it's a panel type, scale it with the fence
			if(railPrefabs[currentRailType].name.EndsWith("_Panel_Rail") && slopeMode != FenceSlopeMode.shear)
			   scale.y *= (fenceHeight*gs/2);
			scale.z *= railSize.z * gs;
			thisRail.transform.localScale = scale;
			
			float gain = (distance * railSize.x) -distance;
			if(railSize.x != 1.0f)
				thisRail.transform.Translate(gain/2, 0, 0);
			thisRail.GetComponent<Renderer>().enabled = true;
			//----------- Omit rails that would intersect with ground/other objects ------
			RaycastHit hit;
			if(autoHideBuriedRails && Physics.Raycast(thisRail.transform.position, vectorDir, out hit, distance) ){
				if(hit.collider.gameObject.name.StartsWith("Rail") == false && hit.collider.gameObject.name.StartsWith("Post") == false
				   && hit.collider.gameObject.name.StartsWith("FenceManagerMarker") == false){
					thisRail.hideFlags = HideFlags.HideInHierarchy;
					thisRail.SetActive(false);
					omit = true;
				}
			}
			thisRail.name = "Rail "+ railCounter.ToString();
			thisRail.transform.parent = railsFolder.transform;
			//========= Shear the mesh if it's a Panel type, to fit slopes =========
			if(omit == false){
				if(slopeMode == FenceSlopeMode.shear)
				{
					float relativeDistance;
					MeshFilter mf = thisRail.GetComponent<MeshFilter>();
					if(mf == null){ print ("MeshFilter missing. Have you deleted one? Disable shear."); continue;}
					Mesh newMesh = railMeshBuffers[railCounter]; // this is a List of spares that we can modify in place of the sharedMesh
					if(newMesh == null){ print ("Mesh missing. Have you deleted one? Disable shear."); continue;}
					newMesh.name = railMeshNames[currentRailType] + " sheared " + railCounter;
					if(meshOrigVerts.Count == 0)
						SaveRailMeshes();

					Vector3[] origVerts = meshOrigVerts[currentRailType].ToArray();
					newMesh.vertices = meshOrigVerts[currentRailType].ToArray();
					newMesh.normals = meshOrigNormals[currentRailType].ToArray();
					newMesh.uv = meshOrigUVs[currentRailType].ToArray();
					newMesh.uv2 = meshOrigUV2s[currentRailType].ToArray();
					newMesh.tangents = meshOrigTangents[currentRailType].ToArray();
					newMesh.triangles = meshOrigTris[currentRailType].ToArray();

					Vector3[] vertices = newMesh.vertices;
					for (int v=0; v < vertices.Length; v++) {
						relativeDistance = ( Mathf.Abs (vertices[v].x))/DEFAULT_RAIL_LENGTH;
						vertices[v].y = (origVerts[v].y * railSize.y * fenceHeight/2 * gs)+ (relativeDistance*heightDelta);
					}
					newMesh.vertices = vertices;
					mf.sharedMesh = newMesh;
				}
				//=========== Make/scale collider on first rail & remove on others ===========
				if(i == 0)
					CreateRailCollider(thisRail);
				else{
					BoxCollider railCollider = thisRail.GetComponent<BoxCollider>();
					if(railCollider != null)
						DestroyImmediate(railCollider);
				}
			}
			thisRail.gameObject.layer = 0; //normal layer
			railCounter++;

			//====== Organize into subfolders so we can combine for drawcalls, but don't hit the mesh combine limit of 65k ==========
			int numRailsFolders = (railCounter/objectsPerFolder)+1;
			string railsDividedFolderName = "railsDividedFolder" + (numRailsFolders-1);
			GameObject railsDividedFolder = GameObject.Find("Current Fences Folder/Rails/" + railsDividedFolderName);
			if(railsDividedFolder == null){
				railsDividedFolder = new GameObject(railsDividedFolderName);
				railsDividedFolder.transform.parent = railsFolder.transform;
				railsDividedFolder.transform.localPosition = Vector3.zero;
				CombineChildrenPlus combineChildren = railsDividedFolder.AddComponent<CombineChildrenPlus>();
				if(combineChildren != null)
					combineChildren.combineAtStart = true;
			}
			thisRail.transform.parent =  railsDividedFolder.transform;
		}
		//========== Sub-Posts ==============
		int intNumSubs = 1;
		GameObject thisSubJoiner = null;
		float actualSubSpacing = 1;
		if(subsFixedOrProportionalSpacing == 1) // depends on distance between posts
		{
			float idealSubSpacing = subSpacing;
			intNumSubs = (int)Mathf.Round(distance/idealSubSpacing);
			if(idealSubSpacing > distance)
				intNumSubs = 1;
			actualSubSpacing = distance/(intNumSubs+1);
		}
		else if(subsFixedOrProportionalSpacing == 0)
		{
			intNumSubs = (int)subSpacing;
			actualSubSpacing = distance/(intNumSubs+1);
		}
		if(showSubs)
		{
			for(int s=0; s<intNumSubs; s++)
			{
				GameObject thisSub = RequestSub(subCounter).gameObject;
				if(thisSub == null){
					print ("Missing Sub " + s + " Have you deleted one?");
					continue;
				}
				thisSub.hideFlags = HideFlags.None;
				thisSub.SetActive(true);
				thisSub.name = "Sub "+ subCounter.ToString();
				thisSub.transform.parent = subsFolder.transform;
				thisSub.transform.position = B;
				
				thisSub.transform.rotation = Quaternion.identity;
				thisSub.transform.Rotate(new Vector3(0, eulerDirection.y, 0), Space.Self);
				
				thisSub.transform.Translate(0, 0, subPositionOffset.z);
				thisSub.transform.Rotate(new Vector3(subRotation.x, subRotation.y, subRotation.z), Space.Self);
				
				Vector3 move = (B-A).normalized * actualSubSpacing * (s+1);
				thisSub.transform.position += new Vector3(-move.x, -move.y, -move.z); 
				thisSub.transform.position += new Vector3(0, subPositionOffset.y * fenceHeight * gs, 0); 
				
				float subFinalLength = subSize.y * gs;
				//===================== Apply sine to height of subposts =======================
				if(useWave) 
				{
					float realMoveForward = move.magnitude;
					float sinValue = Mathf.Sin (   (((realMoveForward/distance)* Mathf.PI * 2)+wavePosition) * frequency);
					sinValue *= amplitude * gs;
					subFinalLength = (subSize.y * gs) + sinValue + (amplitude * gs);
					//==== Create Sub Joiners ====
					if(s > 0 && useSubJoiners)
					{
						thisSubJoiner = RequestSubJoiner(subJoinerCounter++);
						if(thisSubJoiner != null){
							thisSubJoiner.transform.position = thisSub.transform.position + new Vector3(0, (subFinalLength*fenceHeight)-.01f, 0);
							thisSubJoiner.transform.rotation = Quaternion.identity;
						}
					}
				}
				//---------------------------------			
				thisSub.transform.Translate(subPositionOffset.x, 0, 0);
				Vector3 scale = Vector3.one;
				float subScaleRand = r;
				if(subScaleRand < .002f) subScaleRand =.002f; // helps with batching as Unity treats non-uniform scaled copies more effeciently. Bizzare but true!
				scale += new Vector3(UnityEngine.Random.Range(-subScaleRand, subScaleRand), UnityEngine.Random.Range(-subScaleRand, subScaleRand));
				scale.x *= subSize.x * gs;
				scale.y *= subFinalLength * fenceHeight;
				scale.z *= subSize.z * gs;
				thisSub.transform.localScale = scale;
				//=============== Sub Joinsers ================
				if(s > 0  && useSubJoiners && thisSubJoiner != null) // need to do this after the final sub calculations
				{
					Vector3 a = subs[subCounter].transform.position + new Vector3(0, subs[subCounter].transform.localScale.y, 0);
					Vector3 b = subs[subCounter-1].transform.position + new Vector3(0, subs[subCounter-1].transform.localScale.y, 0);
					float joinerDist = Vector3.Distance(b,a);
					thisSubJoiner.transform.localScale = new Vector3(joinerDist, thisSubJoiner.transform.localScale.y, thisSubJoiner.transform.localScale.z);
					Vector3 subJoinerDirection = CalculateDirection(a, b);
					thisSubJoiner.transform.Rotate(new Vector3(0, subJoinerDirection.y-90, -subJoinerDirection.x + 180));
					thisSubJoiner.GetComponent<Renderer>().sharedMaterial = thisSub.GetComponent<Renderer>().sharedMaterial;
				}
				//=============== Force Subs to Ground ================
				if(forceSubsToGroundContour)
				{
					SetIgnoreColliders(true);
					Vector3 currPos = thisSub.transform.position;
					float rayStartHeight = fenceHeight*2.0f;
					currPos.y += rayStartHeight;
					RaycastHit hit;
					if(Physics.Raycast(currPos, Vector3.down, out hit, 500) )
					{
						if(hit.collider.gameObject != null)
						{
							float distToGround = hit.distance +0.04f; //in the ground a little
							thisSub.transform.Translate(0, -(distToGround-rayStartHeight), 0);
							scale.y += (distToGround-rayStartHeight);
							thisSub.transform.localScale = scale;
						}
					}
					SetIgnoreColliders(false);
				}
				subCounter++;
				//====== Organize into subfolders (pun not intended) so we don't hit the mesh combine limit of 65k ==========
				int numSubsFolders = (subCounter/objectsPerFolder)+1;
				string subsDividedFolderName = "subsDividedFolder" + (numSubsFolders-1);
				GameObject subsDividedFolder = GameObject.Find("Current Fences Folder/Subs/" + subsDividedFolderName);
				if(subsDividedFolder == null){
					subsDividedFolder = new GameObject(subsDividedFolderName);
					subsDividedFolder.transform.parent = subsFolder.transform;
					CombineChildrenPlus combineChildren = subsDividedFolder.AddComponent<CombineChildrenPlus>();
					if(combineChildren != null)
						combineChildren.combineAtStart = true;
				}
				thisSub.transform.parent =  subsDividedFolder.transform;
			}
		}
	}

	//==================================================================
	//		Create a Pool of Posts and Rails
	//    	We only need the most basic psuedo-pool to allocate enough GameObjects (and resize when needed)
	//	 	They get activated/deactivated when necessary
	//		As memory isn't an issue at runtime (once the fence is built/finalized, there is NO pool), allocating 25% more 
	//      GOs each time reduces the need for constant pool-resizing and laggy performance in the editor
	//===================================================================
	void	CreatePools()
	{
		CreatePostPool();
		CreateRailPool();
		CreateSubPool();
	}
	//-------------
	void	CreatePostPool(int n=0, bool append = false)
	{
		// Make sure the post type is valid
		if(currentPostType == -1 || currentPostType >= postPrefabs.Count || postPrefabs[currentPostType] == null)
			currentPostType = 0;
		if(n == 0)
			n = defaultPoolSize;
		int start=0;
		if(append){
			start = posts.Count;
			n = start + n;
		}

		for(int i=start; i< n; i++)
		{
			GameObject post = Instantiate(postPrefabs[currentPostType], Vector3.zero, Quaternion.identity)as GameObject;
			post.SetActive(false);
			post.hideFlags = HideFlags.HideInHierarchy;
			posts.Add ( post.transform );
			post.transform.parent = postsFolder.transform;
		}

		n = 8;
		if(clickPoints.Count() > 8) n = (int)(clickPoints.Count()*1.25f);
		markers.Clear();

		if(clickMarkerObj == null){ print ("Reloading clickMarkerObj");
			clickMarkerObj = Resources.Load<GameObject>("FencePrefabs/ClickMarkerObj");
			if(clickMarkerObj == null) print ("Can't load clickMarkerObj");
		}
		if(clickMarkerObj != null){
			for(int i=0; i< n; i++)
			{
				GameObject marker = Instantiate(clickMarkerObj, Vector3.zero, Quaternion.identity)as GameObject;
				marker.SetActive(false);
				marker.hideFlags = HideFlags.HideInHierarchy;
				markers.Add ( marker.transform );
				marker.transform.parent = postsFolder.transform;
			}
		}
	}
	//-----------------------------
	void	CreateRailPool(int n=0, bool append = false)
	{
		if(currentRailType == -1 || currentRailType >= postPrefabs.Count || postPrefabs[currentRailType] == null)
			currentRailType = 0;
		if(n == 0)
			n = defaultPoolSize*2;
		int start=0;
		if(append){
			start = rails.Count;
			n = start + n;
		}
		for(int i=start; i< n; i++)
		{
			GameObject rail = Instantiate(railPrefabs[currentRailType], Vector3.zero, Quaternion.identity)as GameObject;
			rail.SetActive(false);
			rail.hideFlags = HideFlags.HideInHierarchy;
			rails.Add ( rail.transform );
			rail.transform.parent = railsFolder.transform;
		}
		RereateRailMeshes();
	}
	//---------------
	void	RereateRailMeshes()
	{
		// We need meshes to make modified versions if they become modified (e.g. when sheared)
		// we can't just modify the shared mesh, else all of them would be changed identically
		// Could have just new()'d t each mesh as needed, but there's a bug in unity when saving scene, 'cleaning up leaked objects, no scene is using them'
		DestroyBufferedRailMeshes();
		for(int i=0; i< (allPostsPositions.Count-1)*numRails; i++)
		{
			railMeshBuffers.Add (new Mesh());
		}
	}
	//---------------
	// called before rebuilding if not no longer using sheared
	// called before rebuilding from RereateRailMeshes()
	void	DestroyBufferedRailMeshes()
	{
		for(int i=0; i< railMeshBuffers.Count; i++)
		{
			if(railMeshBuffers[i] != null)
				DestroyImmediate( railMeshBuffers[i] );
		}
		railMeshBuffers.Clear ();
	}
	//-----------------------------
	void	CreateSubPool(int n=0, bool append = false)
	{
		// Make sure the post type is valid
		if(currentSubType == -1 || currentSubType >= postPrefabs.Count || postPrefabs[currentSubType] == null)
			currentSubType = 0;
		if(n == 0)
			n = defaultPoolSize*2;
		int start=0;
		if(append){
			start = subs.Count;
			n = start + n;
		}
		for(int i=start; i< n; i++){
			GameObject sub = Instantiate(subPrefabs[currentSubType], Vector3.zero, Quaternion.identity)as GameObject;
			sub.SetActive(false);
			sub.hideFlags = HideFlags.HideInHierarchy;
			subs.Add ( sub.transform );
			sub.transform.parent = subsFolder.transform;
		}
		for(int i=start; i< n; i++){
			if(subJoinerPrefabs[0] == null) continue;
			GameObject subJoiner = Instantiate(subJoinerPrefabs[0], Vector3.zero, Quaternion.identity)as GameObject;
			subJoiner.SetActive(false);
			subJoiner.hideFlags = HideFlags.HideInHierarchy;
			subJoiners.Add ( subJoiner.transform );
			subJoiner.transform.parent = subsFolder.transform;
		}
	}
	//-----------------------------
	// Increase pool size by 25% more than required if necessary
	public void	CheckResizePools()
	{
		//-- Posts---
		if(allPostsPositions.Count >= posts.Count-1)
		{
			CreatePostPool((int)(allPostsPositions.Count * 1.25f) -posts.Count , true); // add 25% more than needed, append is true
		}
		//-- Rails---
		if(allPostsPositions.Count * numRails >= rails.Count-1)
		{
			CreateRailPool((int)((allPostsPositions.Count * numRails * 1.25f)-rails.Count), true);
		}
		//-- Markers---
		if(clickPoints.Count >= markers.Count-1)
		{
			CreatePostPool((int)(allPostsPositions.Count * 1.25f) -posts.Count , true); // add 25% more than needed, append is true
		}
	}
	//---------------------- it's harder to predict how many subs there might be, so better to adjust storage when one is needed
	Transform RequestSub(int index)
	{
		if(index >= subs.Count-1)
		{
			CreateSubPool((int)(subs.Count * 0.25f) , true); // add 25% more, append is true
		}
		return subs[index];
	}
	//---------------------- Allocation is handled by Subs ---------
	GameObject RequestSubJoiner(int index)
	{
		if(subJoiners[index] == null || subJoiners[index].gameObject == null) return null;
		GameObject thisSubJoiner = subJoiners[index].gameObject;
		thisSubJoiner.hideFlags = HideFlags.None;
		thisSubJoiner.SetActive(true);
		thisSubJoiner.name = "SubJoiner "+ index.ToString();
		thisSubJoiner.transform.parent = subsFolder.transform;
		return thisSubJoiner;
	}
	//-----------------------
	// resetting is necessary when a part has been swapped out, we need to banish all the old ones
	void	ResetPostPool()
	{
		DestroyPosts();
		DestroyMarkers();
		CreatePostPool(posts.Count);
	}
	//---------
	void	ResetRailPool()
	{
		DestroyRails();
		CreateRailPool(rails.Count);
	}
	//---------
	void	ResetSubPool()
	{
		DestroySubs();
		CreateSubPool(subs.Count);
	}
	//---------
	void	DestroyPosts()
	{
		for(int i = 0; i< posts.Count; i++){
			if(posts[i] != null)
				DestroyImmediate(posts[i].gameObject);
		}
		posts.Clear();
	}
	//---------
	void	DestroyMarkers()
	{
		for(int i = 0; i< markers.Count; i++){
			if(markers[i] != null)
				DestroyImmediate(markers[i].gameObject);
		}
		markers.Clear();
	}
	//---------
	void	DestroyRails()
	{
		for(int i = 0; i< rails.Count; i++){
			if(rails[i] != null)
				DestroyImmediate(rails[i].gameObject);
		}
		rails.Clear();
	}
	//---------
	void	DestroySubs()
	{
		for(int i = 0; i< subs.Count; i++){
			if(subs[i] != null)	
				DestroyImmediate(subs[i].gameObject);
			if(subJoiners[i] != null)
				DestroyImmediate(subJoiners[i].gameObject);
		}
		subs.Clear();
		subJoiners.Clear();
	}
	//---------
	void	DestroyPools()
	{
		DestroyPosts();
		DestroyMarkers();
		DestroyRails();
		DestroySubs();
	}
	//--------------------------
	public void DestroyUnused()
	{
		for(int i=0; i<posts.Count; i++)
		{
			if(posts[i].gameObject != null){
				if(posts[i].gameObject.hideFlags == HideFlags.HideInHierarchy && posts[i].gameObject.activeSelf == false)
					DestroyImmediate(posts[i].gameObject);
			}
		}
		for(int i=0; i<rails.Count; i++)
		{
			if(rails[i].gameObject != null){
				if(rails[i].gameObject.hideFlags == HideFlags.HideInHierarchy && rails[i].gameObject.activeSelf == false)
					DestroyImmediate(rails[i].gameObject);
			}
		}
		for(int i=0; i<subs.Count; i++)
		{
			if(subs[i].gameObject != null){
				if(subs[i].gameObject.hideFlags == HideFlags.HideInHierarchy && subs[i].gameObject.activeSelf == false){
					DestroyImmediate(subs[i].gameObject);
					if(subJoiners[i].gameObject != null)
						DestroyImmediate(subJoiners[i].gameObject);
				}
			}
		}

		DestroyMarkers();
	}
	//-------------
	public void CheckStatusOfAllClickPoints()
	{
		for(int i=0; i<postCounter+1; i++)
		{
			if( clickPoints.Contains(posts[i].position) ) {
				int index = clickPoints.IndexOf(posts[i].position);
				if(posts[i].gameObject.activeInHierarchy == false){
					DeletePost(index);
				}
			}
		}
	}
	//--------------
	public void DeactivateEntirePool()
	{
		for(int i=0; i< posts.Count; i++)
		{
			if(posts[i] != null){
				posts[i].gameObject.SetActive(false);
				posts[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
				posts[i].position = Vector3.zero;
			}
		}
		for(int i=0; i< rails.Count; i++)
		{
			if(rails[i] != null){
				rails[i].gameObject.SetActive(false);
				rails[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
		
		}
		for(int i=0; i< subs.Count; i++)
		{
			if(subs[i] != null){
				subs[i].gameObject.SetActive(false);
				subs[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
			if(subJoiners[i] != null){
				subJoiners[i].gameObject.SetActive(false);
				subJoiners[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
		}
		// markers
		for(int i=0; i< markers.Count; i++)
		{
			if(markers[i] != null){
				markers[i].gameObject.SetActive(false);
				markers[i].gameObject.hideFlags = HideFlags.HideInHierarchy;
				markers[i].position = Vector3.zero;
			}
		}
	}
	//------------
	// we sometimes need to disable these when raycasting posts to the ground
	// but we need them back on when control-click-deleting them
	public void	SetIgnoreClickMarkers(bool inIgnore)
	{
		int layer = 0; //default layer
		if(inIgnore)
			layer = 2;// 'Ignore Raycast' layer
		/*for(int i=0; i< clickMarkers.Count; i++)
		{
			if(clickMarkers[i] != null)
				clickMarkers[i].gameObject.layer = layer; 
		}*/
		for(int i=0; i< markers.Count; i++)
		{
			if(markers[i] != null)
				markers[i].gameObject.layer = layer; 
		}
	}

	//-----------
	public void	SetIgnoreColliders(bool inIgnore)
	{
		int layer = 0; //default layer
		if(inIgnore)
			layer = 2;// 'Ignore Raycast' layer
		for(int i=0; i< posts.Count; i++)
		{
			if(posts[i] != null)
				posts[i].gameObject.layer = layer; 
		}

		for(int i=0; i< rails.Count; i++)
		{
			if(rails[i] != null)
				rails[i].gameObject.layer = layer; 
		}
	}
	//----------------
	// set on each rebuild
	public void SetMarkersActiveStatus(bool newState)
	{
		for(int i=0; i< clickPoints.Count; i++)
		{
			if(markers[i] != null)
			{
				markers[i].GetComponent<Renderer>().enabled = newState;
				markers[i].gameObject.SetActive(newState);
				if(newState == true)
					markers[i].hideFlags = HideFlags.None;
				else
					markers[i].hideFlags = HideFlags.HideInHierarchy;
			}
		}
	}
	//------------------
	public void ManageLoop(bool loop)
	{
		if(loop)
			CloseLoop();
		else
			OpenLoop();
	}
	//------------------
	public void CloseLoop()
	{
		if(clickPoints.Count < 3){// prevent user from closing if less than 3 points
			closeLoop = false;
		}
		if(clickPoints.Count >= 3 && clickPoints[clickPoints.Count-1] != clickPoints[0]){
			clickPoints.Add(clickPoints[0]); // copy the first clickPoint
			clickPointFlags.Add(clickPointFlags[0]);
			ForceRebuildFromClickPoints();
			//?SceneView.RepaintAll();
		}
	}
	//------------------
	public void OpenLoop()
	{
		if(clickPoints.Count >= 3){
			clickPoints.RemoveAt(clickPoints.Count-1); // remove the last clickPoint (the closer)
			ForceRebuildFromClickPoints();
			//?SceneView.RepaintAll();
		}
	}
	//---------------
	public void DeletePost(int index, bool rebuild = true)
	{
		if(clickPoints.Count > 0 && index < clickPoints.Count)
		{
			lastDeletedIndex = index;
			lastDeletedPoint = clickPoints[index];
			clickPoints.RemoveAt(index); clickPointFlags.RemoveAt(index);
			handles.RemoveAt(index);
			ForceRebuildFromClickPoints();
		}
	}
	//---------------------
	public void InsertPost(Vector3 clickPosition)
	{
		// Find the nearest post and connecting lines to the click position
		float nearest = 1000000;
		int insertPosition = -1;
		for(int i=0; i<clickPoints.Count-1; i++)
		{
			float distToLine = CalcDistanceToLine(clickPoints[i], clickPoints[i+1], clickPosition);
			if(distToLine < nearest)
			{
				nearest = distToLine;
				insertPosition = i+1;
			}
		}
		if(insertPosition != -1)
		{
			clickPoints.Insert(insertPosition, clickPosition);
			clickPointFlags.Insert(insertPosition, clickPointFlags[0]);

			ForceRebuildFromClickPoints();
			//-- Update handles ----
			handles.Clear();
			for(int i=0; i< clickPoints.Count; i++)
			{
				handles.Add (clickPoints[i] );
			}
		}
	}
	//-------------------
	public float GetAngleAtPost(int i, List<Vector3> posts)
	{
		if(i >= posts.Count-1 || i <= 0) return 0;
		
		Vector3 vecA = posts[i] - posts[i-1];
		Vector3 vecB = posts[i+1] - posts[i];
		float angle = Vector3.Angle(vecA, vecB);
		return angle;
	}
	//------------------
	float CalcDistanceToLine(Vector3 lineStart, Vector3 lineEnd,  Vector3 pt)
	{
		Vector3 direction = lineEnd - lineStart;
		Vector3 startingPoint = lineStart;
		
		Ray ray = new Ray(startingPoint, direction);
		float distance = Vector3.Cross(ray.direction, pt - ray.origin).magnitude;

		if(((lineStart.x > pt.x && lineEnd.x > pt.x)  ||  (lineStart.x < pt.x && lineEnd.x < pt.x)) && // it's before or after both x's
		   ((lineStart.z > pt.z && lineEnd.z > pt.z ) ||  (lineStart.z < pt.z && lineEnd.z < pt.z))  ) // it's before or after both z's
		{
			return float.MaxValue;
		}
		return distance;
	}
	//---------------------
	// Called from a loop of clicked array points [Rebuild()] or from a Click in OnSceneGui
	public void	AddNextPostAndInters(Vector3 keyPoint, bool interpolateThisPost = true, bool doRebuild = true)
	{ 
		interPostPositions.Clear();
		float distance = Vector3.Distance(startPoint, keyPoint);
		//float distance = CalculateGroundDistance(startPoint, keyPoint);
		float interDist = interPostDist;
		if(scaleInterpolationAlso)
			interDist *= gs; 
		if(interpolate && distance > interDist && interpolateThisPost)
		{
			int numSpans = (int)Mathf.Round(distance/interDist);
			float fraction = 1.0f/numSpans;
			float x, dx = (keyPoint.x - startPoint.x) * fraction;
			float y, dy = (keyPoint.y - startPoint.y) * fraction;
			float z, dz = (keyPoint.z - startPoint.z) * fraction;
			for(int i=0; i<numSpans-1; i++)
			{
				x = startPoint.x + (dx * (i+1));
				y = startPoint.y + (dy * (i+1));
				z = startPoint.z + (dz * (i+1));
				Vector3 interPostPos = new Vector3(x, y, z);
				interPostPositions.Add (interPostPos);
			}
			Ground(interPostPositions);
			allPostsPositions.AddRange(interPostPositions);
		}
		//Create last post where user clicked
		allPostsPositions.Add(keyPoint); // make a copy so it's independent of the other being destroyed
		if(doRebuild)
			RebuildFromFinalList();
	}
	//---------------------
	// often we need to know the flat distance, ignoring any height difference
	float	CalculateGroundDistance(Vector3 a, Vector3 b)
	{
		a.y = 0;
		b.y = 0;
		float distance = Vector3.Distance(a,b);

		return distance;
	}

	//--------------------
	// this is done at the end because depending on the settings the post rotation/direction need updating
	void RotatePostsFinal()
	{
		if(postCounter < 2) return;
		Vector3 A = Vector3.zero, B = Vector3.zero;
		Vector3 eulerDirection = Vector3.zero;

		if(posts[0] == null) return;
		// first
		A = posts[0].transform.position;
		B = posts[1].transform.position;
		eulerDirection = CalculateDirection(A, B);
		posts[0].transform.rotation = Quaternion.identity;
		posts[0].transform.Rotate(0, eulerDirection.y + 180, 0);
		posts[0].transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
		// main
		for(int i=1; i<postCounter-1; i++)
		{
			A = posts[i].transform.position;
			B = posts[i-1].transform.position;
			eulerDirection = CalculateDirection(A, B);
			posts[i].transform.rotation = Quaternion.identity;
			posts[i].transform.Rotate(0, eulerDirection.y, 0);
			float angle = GetRealAngle(posts[i].transform, posts[i+1].transform);
			posts[i].transform.Rotate(0, angle/2 - 90, 0);
			posts[i].transform.Rotate(postRotation.x, postRotation.y, postRotation.z);
		}
		// last
		A = posts[postCounter-1].transform.position;
		B = posts[postCounter-2].transform.position;
		eulerDirection = CalculateDirection(A, B);
		posts[postCounter-1].transform.rotation = Quaternion.identity;
		posts[postCounter-1].transform.Rotate(0, eulerDirection.y, 0);
		posts[postCounter-1].transform.Rotate(postRotation.x, postRotation.y, postRotation.z);

	}
	//-------------------
	//- this is the real angle (0-360) as opposed to -180/0/+180 that the Unity methods give.
	float GetRealAngle(Transform postA, Transform postB)
	{
		Vector3 referenceForward = postA.forward;
		Vector3 newDirection = postB.position - postA.transform.position;
		float angle = Vector3.Angle(newDirection, referenceForward);
		float sign = (Vector3.Dot(newDirection, postA.right) > 0.0f) ? 1.0f: -1.0f;
		float finalAngle = sign * angle;
		if(finalAngle <0) finalAngle = finalAngle +360;
		return finalAngle;
	}
	//------------
	// Sets the post in the pool with all the correct attributes
	void	SetupPost(int n,  Vector3 postPoint)
	{
		if(posts[n] == null) 
			return;
		GameObject thisPost = posts[n].gameObject;
		bool isClickPoint = false;
		if(postNames[currentPostType] != "_None_Post")
			thisPost.SetActive(true);
		thisPost.hideFlags = HideFlags.None;
		thisPost.name = "Post "+ n.ToString();
		// Name it if it is a click point
		if( clickPoints.Contains(postPoint)){
			thisPost.name += "_click";
			isClickPoint = true;
		}
		
		thisPost.layer =  8;
		float r = UnityEngine.Random.Range(-randomness, randomness);
		r = UnityEngine.Random.Range(-randomness/5, randomness);
		thisPost.transform.position = postPoint;
		thisPost.transform.position += new Vector3(0, postHeightOffset*gs, 0);
		thisPost.transform.localScale = new Vector3(postSize.x*gs, fenceHeight*(1+r)*postSize.y*gs, postSize.z*gs);
		//thisPost.transform.parent = postsFolder.transform;
		//====== Organize into subfolders (pun not intended) so we don't hit the mesh combine limit of 65k ==========
		int numPostsFolders = (postCounter/objectsPerFolder)+1;
		string postsDividedFolderName = "postsDividedFolder" + (numPostsFolders-1);
		GameObject postsDividedFolder = GameObject.Find("Current Fences Folder/Posts/" + postsDividedFolderName);
		if(postsDividedFolder == null){
			postsDividedFolder = new GameObject(postsDividedFolderName);
			postsDividedFolder.transform.parent = postsFolder.transform;
			CombineChildrenPlus combineChildren = postsDividedFolder.AddComponent<CombineChildrenPlus>();
			if(combineChildren != null)
				combineChildren.combineAtStart = true;
		}
		thisPost.transform.parent =  postsDividedFolder.transform;

		//====== Set Up Yellow Click Markers =======
		if(isClickPoint){
			int clickIndex = clickPoints.IndexOf(postPoint);
			if(clickIndex != -1){
				GameObject marker = markers[clickIndex].gameObject;
				marker.SetActive(true);
				//marker.hideFlags = HideFlags.None;
				marker.hideFlags = HideFlags.HideInHierarchy;

				Vector3 markerPos = postPoint;
				markerPos.y += thisPost.transform.localScale.y;
				marker.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
				marker.transform.position = markerPos;
				marker.name = "FenceManagerMarker_" + clickIndex.ToString();
			}
		}
	}
	//-------------------
	// we have to do this recursively one at a time because removing one will alter the angles of the others
	void ThinByAngle(List<Vector3> posList)
	{
		if(removeIfLessThanAngle < 0.01f) return;

		float minAngle = 180;
		int minAngleIndex = -1;
		for(int i=1; i<posList.Count-1; i++)
		{
			Vector3 vecA = posList[i] - posList[i-1];
			Vector3 vecB = posList[i+1] - posList[i];
			float angle = Vector3.Angle(vecA, vecB);
			if(!clickPoints.Contains (posList[i]) && angle < minAngle)
			{
				minAngle = angle;
				minAngleIndex = i;
			}
		}
		if(minAngleIndex != -1 && minAngle < removeIfLessThanAngle) // we found one
		{
			posList.RemoveAt(minAngleIndex);
			ThinByAngle(posList);
		}
	}
	//-------------------
	// we have to do this recursively one at a time because removing one will alter the distances of the others
	void ThinByDistance(List<Vector3> posList)
	{
		float minDist = 10000;
		int minDistIndex = -1;
		float distToPre, distToNext, distToNextNext;
		for(int i=1; i<posList.Count-1; i++)
		{
			if(!IsClickPoint(posList[i]))
			{
				distToNext = Vector3.Distance(posList[i], posList[i+1]);
				if(distToNext < stripTooClose)
				{
					// close to neighbour, do we strip this one or the neighbour? Strip the one that has the other closest neighbour
					// but only if it is not a clickpoint
					if(!IsClickPoint(posList[i+1]))
					{
						distToPre = Vector3.Distance(posList[i], posList[i-1]);
						distToNextNext = Vector3.Distance(posList[i+1], posList[i+2]);
						
						if(distToPre < distToNextNext)
						{
							minDist = distToNext;
							minDistIndex = i;
						}
						else
						{
							minDist = distToNext;
							minDistIndex = i+1;
						}
					}
					else
					{
						minDist = distToNext;
						minDistIndex = i;
					}
				}
			}
		}
		if(minDistIndex != -1 && minDist < stripTooClose) // we found one
		{
			posList.RemoveAt(minDistIndex);
			ThinByDistance(posList);
		}
	}
	//-------------------
	int FindClickPointIndex(Vector3 pos)
	{
		return clickPoints.IndexOf(pos);
	}
	//-------------------
	bool IsClickPoint(Vector3 pos)
	{
		if(clickPoints.Contains(pos))
			return true;
		return false;
	}
	//-------------------
	void MakeSplineFromClickPoints()
	{
		// SplineFillMode {fixedNumPerSpan = 0, equiDistant, angleDependent};
		if(smooth == false || roundingDistance == 0 || clickPoints.Count <3) 
			return; //abort
		//-- Add 2 at each end before interpolating
		List<Vector3> splinedList = new List<Vector3>();
		Vector3 dirFirstTwo = (clickPoints[1] - clickPoints[0]).normalized;
		Vector3 dirLastTwo = (clickPoints[clickPoints.Count-1] - clickPoints[clickPoints.Count-2]).normalized;
		
		if(closeLoop)
		{
			splinedList.Add (clickPoints[clickPoints.Count-3]);
			splinedList.Add (clickPoints[clickPoints.Count-2]);
		}
		else{
			splinedList.Add (clickPoints[0] - (2 * dirFirstTwo));
			splinedList.Add (clickPoints[0] - (1 * dirFirstTwo));
		}
		
		splinedList.AddRange(clickPoints);
		if(closeLoop)
		{
			splinedList.Add (clickPoints[1]);
			splinedList.Add (clickPoints[2]);
		}
		else{
			splinedList.Add (clickPoints[clickPoints.Count-1] + (2 * dirLastTwo));
			splinedList.Add (clickPoints[clickPoints.Count-1] + (1 * dirLastTwo));
		}
		//int points = 51 - roundingDistance;
		splinedList =  CreateCubicSpline3D(splinedList, roundingDistance, SplineFillMode.equiDistant, tension);
		ThinByAngle(splinedList);
		ThinByDistance(splinedList);
		//---------------------------
		keyPoints.Clear ();
		keyPoints.AddRange(splinedList);

		Ground(keyPoints);
	}
	//--------------------
	// lower things to ground level
	public void Ground(List<Vector3> vec3List)
	{
		RaycastHit hit;
		Vector3 pos, highPos;
		float extraHeight = 4;//((fenceHeight * postSize.y)/2) + postHeightOffset;
		SetIgnoreClickMarkers(true); // switch them off so we can raycasy down from above them
		SetIgnoreColliders(true);

		for(int i=0; i<vec3List.Count; i++ )
		{
			highPos = pos = vec3List[i];
			highPos.y += extraHeight;
			if(Physics.Raycast(highPos, Vector3.down, out hit, 500) ) // First check from above, looking down
			{
				if(hit.collider.gameObject != null){
						pos += new Vector3(0, -(hit.distance-extraHeight), 0);
				}
			}
			else if(Physics.Raycast(pos, Vector3.up, out hit, 500) ) // maybe we've gone below... check upwards
			{
				if(hit.collider.gameObject != null){
						pos += new Vector3(0, +hit.distance, 0);
				}
			}
			vec3List[i] = pos;
		}
		SetIgnoreClickMarkers(false); // switch them back on so we can detect clicks on them later
		SetIgnoreColliders(false);
	}
	//--------------------------------------------
	public Vector3  CalculateDirection(Vector3 A, Vector3 B) {
		
		if(postCounter < 1) return Vector3.zero;
		Quaternion q2 = Quaternion.LookRotation(B - A);
		Vector3 euler = q2.eulerAngles;
		return euler;
	}
	//----------------------------------
	List<Vector3>  CreateCubicSpline3D(List<Vector3> inNodes, int numInters,  
	                                                 SplineFillMode fillMode = SplineFillMode.fixedNumPerSpan , 
	                                                 float tension = 0, float bias = 0, bool addInputNodesBackToList = true)
	{
		int numNodes = inNodes.Count;
		if(numNodes < 4) return inNodes;
		
		float mu, interpX, interpZ;
		int numOutNodes = (numNodes-1) * numInters;
		List<Vector3> outNodes = new List<Vector3>(numOutNodes);
		
		int numNewPoints = numInters;
		for(int j=2; j<numNodes-3; j++) // don't build first  fake ones
		{
			outNodes.Add(inNodes[j]);
			Vector3 a,b,c,d;
			a = inNodes[j-1];
			b = inNodes[j];
			c = inNodes[j+1];
			if(j<numNodes-2)
				d = inNodes[j+2];
			else
				d = inNodes[numNodes-1];
			
			if(fillMode == SplineFillMode.equiDistant) //equidistant posts, numInters now refers to the requested distance between the new points
			{
				float dist = Vector3.Distance(b,c);
				numNewPoints = (int)Mathf.Round(dist/numInters);
				if(numNewPoints < 1) numNewPoints = 1;
			}

			float t= tension;
			if( IsBreakPoint(inNodes[j]) || IsBreakPoint(inNodes[j+2])  ){ // this will prevent falsely rounding in to gaps/breakPoints
				t = 1.0f;
			}

			for(int i=0; i<numNewPoints; i++)
			{
				mu = (1.0f/(numNewPoints+1.0f))*(i+1.0f);
				interpX = HermiteInterpolate(a.x, b.x, c.x, d.x, mu, t, bias);
				interpZ = HermiteInterpolate(a.z, b.z, c.z, d.z, mu, t, bias);
				outNodes.Add( new Vector3(interpX, b.y, interpZ));
			}
		}
		if(addInputNodesBackToList)
		{
			outNodes.Add(inNodes[numNodes-3]);
		}
		return outNodes;
	}
	float HermiteInterpolate(float y0,float y1,float y2,float y3,float mu,float tension,float bias)
	{
		float mid0,mid1,mid2,mid3;
		float a0,a1,a2,a3;
		mid2 = mu * mu;
		mid3 = mid2 * mu;
		mid0  = (y1-y0)*(1+bias)*(1-tension)/2;
		mid0 += (y2-y1)*(1-bias)*(1-tension)/2;
		mid1  = (y2-y1)*(1+bias)*(1-tension)/2;
		mid1 += (y3-y2)*(1-bias)*(1-tension)/2;
		a0 =  2*mid3 - 3*mid2 + 1;
		a1 =    mid3 - 2*mid2 + mu;
		a2 =    mid3 -   mid2;
		a3 = -2*mid3 + 3*mid2;
		return(a0*y1+a1*mid0+a2*mid1+a3*y2);
	}
	//---------------------------
	int FindPrefabByName(FencePrefabType prefabType,  string prefabName) 
	{
		List<GameObject> prefabList = postPrefabs;
		if(prefabType == FencePrefabType.railPrefab)
			prefabList = railPrefabs;

		for(int i=0; i< prefabList.Count; i++)
		{
			string name = prefabList[i].name;
			if(name == prefabName)
				return i;
		}
		print ("Warning: Couldn't find prefab with name: " + prefabName + ". Have you deleted it, or is its mesh missing? Replace it, or reimport AutoFence to restore it.\n" +
		       "A default prefab will be used instead now.");
		return -1;
	}
	//---------------------------
	// custom, beacuse by default .ToString() only writes 1 decimal place, we want 3
	string VectorToString(Vector3 vec)
	{
		string vecString = "(";
		vecString += vec.x.ToString("F3") + ", ";
		vecString += vec.y.ToString("F3") + ", ";
		vecString += vec.z.ToString("F3") + ")";
		return vecString;
	}
	//---------------------------
	Vector3 ParseVector3(string vecStr) 
	{
		vecStr = vecStr.Trim(new Char[] { '(', ')' } );
		string[] values = vecStr.Split(',');
		Vector3 vec3 = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]) );
		return vec3;
	}
	//---------------------------
	public string GetPartialTimeString(bool includeDate = false)
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
