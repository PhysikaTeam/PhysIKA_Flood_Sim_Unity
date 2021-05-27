using System;
using Assets.Scripts.Utils;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ChartAndGraph;
using System.Runtime.InteropServices;
//using XCharts;


namespace Assets.Scripts
{
    public enum InputModes : int
    {
        AddTerrain = 0,
        AddPaishuikou = 1,
        AddJinshuikou = 2,
        ClickBuildingToGetSpline = 3,
        AddWater = 4,
        AddWater2 = 5
    }

    public class CustomTerrain : MonoBehaviour
    {
        //PhysIKA data pointers
        [StructLayout(LayoutKind.Sequential)]
        unsafe struct PhysIKAPointers
        {
            public IntPtr solid;
            public IntPtr depth;
            public IntPtr uVel;
            public IntPtr wVel;
        }

        //Import PhysIKA Dll
        const int len = 1024 * 1024;
        [DllImport("App_SWEDlld")]
        public static extern void PhysIKAInit();

        [DllImport("App_SWEDlld")]
        public static extern int excuteOneStep(IntPtr p);

        PhysIKAPointers pData;

        private float[] solidRes = new float[len];
        private float[] depthRes = new float[len];
        private float[] uVelRes = new float[len];
        private float[] wVelRes = new float[len];

        public Material phyMat; //test PhysIKA material
        private RenderTexture _pRTex;

        //PhysIKA compute shader buffer
        private ComputeBuffer rBuffer;
        private ComputeBuffer gBuffer;
        private ComputeBuffer bBuffer;
        private ComputeBuffer aBuffer;

        private int phyKernel;

        [Header("Main settings")]
        public Material[] Materials;
        public Material buildingProcess;
        public ComputeShader ErosionComputeShader;
        public Texture2D InitialState;
        public Material InitHeightMap, InitBuildingOutlineMap,InitFlowInOutMap;
        public Texture2D RainMap;
        public Texture2D FlowInOutMap;
        public Texture2D BuildingOutlineMap;
        public Texture2D BuildingPointsMap1, BuildingPointsMap2, BuildingPointsMap3, BuildingPointsMap4;

        //public Texture2D SourceMap;
        [Range(0, 1)]
        public float InOutFlowScale = 0.01f;
        //[Range(32, 1024)]
        [Range(32, 4096)]
        public int Width = 4096;
        //[Range(32, 1024)]
        [Range(32, 4096)]
        public int Height = 4096;

        public int TrueWidth = 4500;
        public int TrueHeight = 4000;

        public Vector3 terrPos = new Vector3(1750, 0, 2800);
        public float offsetX = 4500f;
        public float offsetZ = 4000f;

        public static  float BrushAmount = 0f;
        public static InputModes InputMode = InputModes.ClickBuildingToGetSpline;
        public GraphChart graph;

        [Serializable]
        public class SimulationSettings
        {
            [Header("InitialSetting")]
            public int earlyWarning=0;
            public float riverHeight = 7.0f;

            [Range(0f, 10f)]
            public float TimeScale = 1.0f;

            [Range(0f, 40f)]
            public float SpeedUp = 1.0f;

            public float PipeLength = 1.0f / 256.0f;
            public Vector2 CellSize = new Vector2(1f / 256, 1f / 256);

            public Vector4 _BuildingOrProtectionSize = new Vector4(0, 0, 0, 0);

            [Range(0, 0.5f)]
            public float RainRate = 0.012f;

            [Range(0, 10.0f)]
            public float MaximalWaterDepth = 10.0f;

            [Range(0, 1f)]
            public float Evaporation = 0.015f;

            [Range(0.001f, 1000)]
            public float PipeArea = 20;

            [Range(0.1f, 20f)]
            public float Gravity = 9.81f;

            [Header("Visualize")]
            [Range(0,2)]
            //0 no 1 state 2 building state
            public int saveState=0;

            [Range(50,500)]
            public int framesGap = 200;

            [Range(20, 300)]
            public int capacity = 200;

            public Material interactive;

            //[Header("Hydraulic erosion")]
            //[Range(0.1f, 3f)]
            //public float SedimentCapacity = 1f;

            //[Range(0.1f, 2f)]
            //public float SoilSuspensionRate = 0.5f;

            //[Range(0.1f, 3f)]
            //public float SedimentDepositionRate = 1f;

            //[Range(0f, 10f)]
            //public float SedimentSofteningRate = 5f;

            //[Range(0f, 40f)]
            //public float MaximalErosionDepth = 10f;

            //[Header("Thermal erosion")]
            //[Range(0, 1000f)]
            //public float ThermalErosionTimeScale = 1f;

            //[Range(0, 1f)]
            //public float ThermalErosionRate = 0.15f;

            //[Range(0f, 1f)]
            //public float TalusAngleTangentCoeff = 0.8f;

            //[Range(0f, 1f)]
            //public float TalusAngleTangentBias = 0.1f;
        }

        public SimulationSettings Settings;
        // Computation stuff
        // State texture ARGBFloat
        // R - surface height  [0, +inf]
        // G - water over surface height [0, +inf]
        // B - Suspended sediment amount [0, +inf]
        // A - Hardness of the surface [0, 1]
        private RenderTexture _stateTexture;
        // Output water flux-field texture
        // represents how much water is OUTGOING in each direction
        // R - flux to the left cell [0, +inf]
        // G - flux to the right cell [0, +inf]
        // B - flux to the top cell [0, +inf]
        // A - flux to the bottom cell [0, +inf]
        private RenderTexture _waterFluxTexture;
        // Output terrain flux-field texture
        // represents how much landmass is OUTGOING in each direction
        // Used in thermal erosion process
        // R - flux to the left cell [0, +inf]
        // G - flux to the right cell [0, +inf]
        // B - flux to the top cell [0, +inf]
        // A - flux to the bottom cell [0, +inf]
        private RenderTexture _terrainFluxTexture;
        // Velocity texture
        // R - X-velocity [-inf, +inf]
        // G - Y-velocity [-inf, +inf]
        private RenderTexture _velocityTexture;
        private RenderTexture _FlowInOutTexture;
        // List of kernels in the compute shader to be dispatched
        // Sequentially in this order
        private RenderTexture _BuildingsAndProtectionTexture;
        private RenderTexture _BuildingPointsTexture1;
        private RenderTexture _BuildingPointsTexture2;
        private RenderTexture _BuildingPointsTexture3;
        private RenderTexture _BuildingPointsTexture4;
        private RenderTexture _BuildingOutlineTexture;
        private RenderTexture _BuildingColorTexture;
        private Material skybox;

        //private readonly string[] _kernelNames = {
        //    "RainAndControl",
        //    "FluxComputation",
        //    "FluxApply",
        //    "HydraulicErosion",
        //    "SedimentAdvection",
        //    "ThermalErosion",
        //    "ApplyThermalErosion"
        //};

        //Virtual Pip, not consider terrain erosion
        private readonly string[] VP = {
            "Edit",
            "FluxComputation",
            "FluxApply",
            "ComputeBuildingColor",
            "RainAndControl"
        };

        private readonly string[] SWE = {
            "Edit",
            "RainAndControl",
            "advect",
            "copy",
            "updateHeight",
            "updateVelocity",
            "copy2",
            "ComputeBuildingColor"
        };

        private bool mouseClicked=false;
        // Kernel-related data
        private int[] _kernels;
        private uint _threadsPerGroupX;
        private uint _threadsPerGroupY;
        private uint _threadsPerGroupZ;

        // Rendering stuff
        private const string StateTextureKey = "_StateTex";
        // Brush
        private Plane _floor = new Plane(Vector3.up, Vector3.zero);

        public static float _brushRadius = 0.001f;
        private Vector4 _inputControls;
        private bool stop = true;

        private int frames = 0;
        public static List<Texture2D> states = new List<Texture2D>();
        public static List<Texture2D> buildingStates = new List<Texture2D>();

        private bool firstClick = false;
        public static bool init =  false;

        public static int width = 1024, height = 1024;
        public static float riverHeight = 7.0f;
        public static float riverSpeed = 5.0f;
        public static bool earlyWarning;
        public  Material waterShader,terrainShader,barrirShader;
        public static bool displayBuilding = true, displayTerrain = true, displayWater = true, displayMap = false;
        public static bool displayWaterDepth = false;
        public static bool backwardButtonIsClick = false;
        public static bool saveButtonClick = false;
        public static int timeIndex = 0;
        public static int st = 0, ed = 0;
        public static float barrirWid = 0, barrirHei = 0;
        public static Vector3 barrirSt,barrirEd;
        public static bool buildBarrier1 = false;
        public static bool buildBarrier2 = false;

        void Start()
        {
            Camera.main.depthTextureMode = DepthTextureMode.Depth;
            // Set skybox
            string skyBoxName = "skybox/Blue Lagoon";
            skybox = Resources.Load<Material>(skyBoxName);
            RenderSettings.skybox = skybox;
            PhysIKAInit();
        }
        
        void Update()
        {
            if (init == false && firstClick == false)
                return;
            if (init == true && firstClick == false)
            {
                Initialize();
                firstClick = true;
            }

            rBuffer.SetData(solidRes);
            gBuffer.SetData(depthRes);
            bBuffer.SetData(uVelRes);
            aBuffer.SetData(wVelRes);
            if (Input.GetMouseButtonDown(2))
                stop = !stop;
            // Controls
            //_brushRadius = Mathf.Clamp(_brushRadius + Input.mouseScrollDelta.y * Time.deltaTime * 0.2f, 0.001f*4000, 1f*4000);
            _brushRadius = Mathf.Clamp(_brushRadius, 0.000f * 4000, 1f * 4000);
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var amount = 0f;
            var brushX = 0f;
            var brushY = 0f;
            float enter;
            if (_floor.Raycast(ray, out  enter))
            {
                var hitPoint = ray.GetPoint(enter);
                //Debug.Log(hitPoint);
                brushX = (hitPoint.x - terrPos.x) / offsetX;
                brushY = (hitPoint.z - terrPos.z) / offsetZ;
                mouseClicked = false;
                //Debug.Log("X: " + brushX + "  Y: " + brushY);
                Settings.interactive.SetFloat("_a", _brushRadius);
                Settings.interactive.SetVector("_FocusPos", hitPoint);
                if (Input.GetMouseButton(0))
                {
                    amount = BrushAmount;
                    mouseClicked = true;
                   // Debug.Log(InputMode);
                }      
            }
            else
            {
                amount = 0f;
            }
            _inputControls = new Vector4(brushX, brushY, _brushRadius/4000.0f, amount);
            Shader.SetGlobalVector("_InputControls", _inputControls);
            
            if (displayWater == false)
                waterShader.SetInt("_WaterMode", 0);
            else
            {
                if (displayWaterDepth == false)
                    waterShader.SetInt("_WaterMode", 1);
                else
                    waterShader.SetInt("_WaterMode", 2);
            }

            if (displayMap == false)
                terrainShader.SetInt("_Display", 0);
            else
                terrainShader.SetInt("_Display", 1);


            if (Input.GetMouseButtonDown(0) && InputMode == InputModes.ClickBuildingToGetSpline)
            {
                RaycastHit hit;
                bool isCollider = Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (isCollider == true && hit.transform.gameObject.name.StartsWith("Building "))
                {
                    string n = hit.transform.gameObject.name;
                    string tt = n.Substring(9);
                    int space = n.IndexOf(' ', 9);
                    if (space == -1)
                        tt = n.Substring(9);
                    else
                        tt = n.Substring(9, space - 9);
                    int t = Convert.ToInt32(tt);
                    //Debug.Log(hit.point);
                    getSplineWaterOfBuilding(t);
                }
            }
            if(saveButtonClick)
            {
                saveState();
                saveButtonClick = false;
            }
            if (backwardButtonIsClick == true)
            {
                if(stop)
                {
                    backward(timeIndex);
                    backwardButtonIsClick = false;
                }
            }
            if(buildBarrier1)
            {
                buildBarrierFunc(uiDrawLines.posList, barrirWid, barrirHei);
                buildBarrier1 = false;
            }
        }

        private List<Vector3> pos2 = new List<Vector3> ();
        private void buildBarrierFunc(List<Vector3>pos,float wid,float hei)
        {
            Camera c = Camera.main;
            List<Vector3> hitPoints = new List<Vector3>();
            for(int i = 0; i < pos.Count; i++)
            {
                var p = pos[i];
                p.x *= Screen.width;
                p.y *= Screen.height;
                var ray1 = Camera.main.ScreenPointToRay(p);
                float enter;
                Vector3 a = new Vector3(0, 0, 0), b = new Vector3(0, 0, 0);
                if (_floor.Raycast(ray1, out enter))
                {
                    var hitPoint = ray1.GetPoint(enter);
                    a = hitPoint;
                    hitPoints.Add(a);
                    //Debug.Log(hitPoint);
                    float pixelX = (hitPoint.x - terrPos.x) / offsetX;
                    float pixelZ = (hitPoint.z - terrPos.z) / offsetZ;
                    barrirSt = new Vector3(pixelX, 0, pixelZ);
                    pos2.Add(barrirSt);
                }
                else
                    barrirHei = 0;
            }
            if(barrirHei > 0)
            {
                for(int i=0;i<pos.Count-1;i++)
                {
                    createWall(hitPoints[i], hitPoints[i + 1], barrirWid, barrirHei);
                }
            }
        }

        public GameObject barrirPrefab;
        private void createWall(Vector3 st,Vector3 ed,float w,float h)
        {
            Vector3 center = (st + ed) / 2;
            float l = Vector3.Magnitude(ed - st);
            var wall = Instantiate(barrirPrefab);
            wall.transform.position = new Vector3(center.x, center.y - h /2, center.z);
            wall.transform.localScale = new Vector3(l, h, w);
            Vector3 forward = Vector3.Cross(ed - st, new Vector3(0, 1, 0));
            wall.transform.forward = forward;
        }
        void FixedUpdate()
        {
            //PhysIKA compute one step
            IntPtr passPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pData));
            int pRet = excuteOneStep(passPtr);
            pData = (PhysIKAPointers)Marshal.PtrToStructure(passPtr, typeof(PhysIKAPointers));

            //update PhysIKA data to compute shader buffer
            Marshal.Copy(pData.solid, solidRes, 0, len);
            Marshal.Copy(pData.depth, depthRes, 0, len);
            Marshal.Copy(pData.uVel, uVelRes, 0, len);
            Marshal.Copy(pData.wVel, wVelRes, 0, len);
            Marshal.FreeHGlobal(passPtr);
            /**/
            
            // Compute dispatch
            if (ErosionComputeShader != null)
            {
                if (Settings != null)
                {
                    // General parameters
                    ErosionComputeShader.SetFloat("_TimeDelta", Time.fixedDeltaTime * Settings.TimeScale);
                    //Debug.Log("_TimeDelta: " + Time.fixedDeltaTime * Settings.TimeScale);
                    // Debug.Log(Time.fixedDeltaTime * Settings.TimeScale);
                    ErosionComputeShader.SetFloat("_PipeArea", Settings.PipeArea);
                    ErosionComputeShader.SetFloat("_Gravity", Settings.Gravity);
                    ErosionComputeShader.SetFloat("_PipeLength", Settings.PipeLength);
                    ErosionComputeShader.SetVector("_CellSize", Settings.CellSize);
                    ErosionComputeShader.SetFloat("_Evaporation", Settings.Evaporation);
                    ErosionComputeShader.SetFloat("_RainRate", Settings.RainRate);
                    ErosionComputeShader.SetVector("_BuildingOrProtectionSize", Settings._BuildingOrProtectionSize);

                    //// Hydraulic erosion
                    //ErosionComputeShader.SetFloat("_SedimentCapacity", Settings.SedimentCapacity);
                    //ErosionComputeShader.SetFloat("_MaxErosionDepth", Settings.MaximalErosionDepth);
                    //ErosionComputeShader.SetFloat("_SuspensionRate", Settings.SoilSuspensionRate);
                    //ErosionComputeShader.SetFloat("_DepositionRate", Settings.SedimentDepositionRate);
                    //ErosionComputeShader.SetFloat("_SedimentSofteningRate", Settings.SedimentSofteningRate);

                    //// Thermal erosion
                    //ErosionComputeShader.SetFloat("_ThermalErosionRate", Settings.ThermalErosionRate);
                    //ErosionComputeShader.SetFloat("_TalusAngleTangentCoeff", Settings.TalusAngleTangentCoeff);
                    //ErosionComputeShader.SetFloat("_TalusAngleTangentBias", Settings.TalusAngleTangentBias);
                    //ErosionComputeShader.SetFloat("_ThermalErosionTimeScale", Settings.ThermalErosionTimeScale);

                    // Inputs
                    ErosionComputeShader.SetVector("_InputControls", _inputControls);
                    barrirSt = barrirEd = new Vector3(0, 0, 0);

                    if(uiDrawLines.isDrawing == false && buildBarrier2 == true)
                    {
                        if (pos2.Count < 2)
                        {
                            pos2.Clear();
                            uiDrawLines.complete = true;
                            buildBarrier2 = false;
                            Debug.Log("UIDRAWLINES COMPLETE");
                        }
                        if (pos2.Count >= 2)
                        {
                            //ErosionComputeShader.SetVector("_BarrirSt", pos2[0]);
                            //ErosionComputeShader.SetVector("_BarrirEd", pos2[1]);
                            barrirSt = pos2[0];
                            barrirEd = pos2[1];
                            mouseClicked = true;
                            Debug.Log("pos2.count: " + pos2.Count);
                            Debug.Log("st: " + barrirSt);
                            Debug.Log("ed: " + barrirEd);
                            pos2.RemoveAt(0);
                        }
                        
                    }
                    ErosionComputeShader.SetVector("_BarrirSt", barrirSt);
                    ErosionComputeShader.SetVector("_BarrirEd", barrirEd);

                    ErosionComputeShader.SetFloat("_BarrirHei", barrirHei);
                    ErosionComputeShader.SetFloat("_BarrirWid", barrirWid / 4000.0f);

                    ErosionComputeShader.SetVector("_InputControls", _inputControls);
                    ErosionComputeShader.SetInt("_InputMode", (int)InputMode);
                    if(mouseClicked)
                        Debug.Log("ErosionCompute: " + (int)InputMode);
                    ErosionComputeShader.SetBool("_mouseClicked", mouseClicked);
                    ErosionComputeShader.SetBool("_earlyWarning", earlyWarning);
                    ErosionComputeShader.SetFloat("_InOutFlowScale", InOutFlowScale);
                }
                //float t = Time.fixedDeltaTime * Settings.TimeScale;
                if (stop == true) //暂停时只有负责交互的核函数运行
                {
                    ErosionComputeShader.Dispatch(_kernels[0], _stateTexture.width / (int)_threadsPerGroupX, _stateTexture.height / (int)_threadsPerGroupY, 1);
                }
                else
                {
                    for (int i = 0; i < Settings.SpeedUp; i++) // for speed up the simulation
                    {
                        // Dispatch all passes sequentially
                        foreach (var kernel in _kernels)
                        {
                            ErosionComputeShader.Dispatch(kernel, _stateTexture.width / (int)_threadsPerGroupX, _stateTexture.height / (int)_threadsPerGroupY, 1);
                        }
                    }
                    ErosionComputeShader.Dispatch(phyKernel, 1024 / (int)_threadsPerGroupX, 1024 / (int)_threadsPerGroupY, 1);
                    frames++;
                    if(frames % Settings.framesGap == 0)
                    {
                        int tmp = Settings.framesGap * 12;
                        if (Settings.saveState == 1) //state
                            saveState();
                        else if (Settings.saveState == 2)
                        {
                            //save BuildingState, for the curve
                            if (frames > tmp)
                            {
                                Debug.Log("building state is save");
                                saveBuildingState();
                            }
                        }
                    }
                }
                mouseClicked = false;
            } 
        }
        private void getSplineWaterOfBuilding(int _Index)
        {
            Debug.Log("Building " + _Index);
            Debug.Log("buildingStates.size(): " + buildingStates.Count);
            List<float> l = new List<float>();
            _Index = _Index - 1;
            int a =  (_Index % Width) ;
            int b =  (_Index / Width);
            foreach (var png in buildingStates)
            {
                float depth=png.GetPixel(a, Width - 1 - b).r;
                Debug.Log("a and b: "+ a + ", " + b);
                Debug.Log("The depth: " + depth);

                l.Add(depth);
            }
            var t = graph.GetComponent<GraphChartSample>();
            t.changeData(l, "Building " + _Index);
        }
        private void saveState()
        {
            if (states.Count >= Settings.capacity)
            {
                states.RemoveAt(0);
            }
            RenderTexture prev = RenderTexture.active;
            var rt = _stateTexture;
            RenderTexture.active = rt;
            Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
            png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = prev;
            states.Add(png);
            Debug.Log(states.Count);
        }
        private void backward(int index)
        {
            if (states.Count <= index)
                return;
            if (ErosionComputeShader != null)
            {
                _kernels = new int[SWE.Length];
                var i = 0;
                foreach (var kernelName in SWE)
                {
                    var kernel = ErosionComputeShader.FindKernel(kernelName);
                    _kernels[i++] = kernel;
                   Graphics.Blit(states[index], _stateTexture);
                   ErosionComputeShader.SetTexture(kernel, "HeightMap", _stateTexture);
                }
            }
        }
        
        //save building state to memory
        private void saveBuildingState()
        {
            if (buildingStates.Count >= Settings.capacity)
                return;
            RenderTexture prev = RenderTexture.active;
            RenderTexture rt = _BuildingColorTexture;
            RenderTexture.active = rt;
            Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
            png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            buildingStates.Add(png);
            RenderTexture.active = prev;
            Debug.Log(buildingStates.Count);
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            /* ========= Setup computation =========== */
            // If there are already existing textures - release them
            Debug.Log("Start Initialize");

            if (_stateTexture != null)
                _stateTexture.Release();

            if (_waterFluxTexture != null)
                _waterFluxTexture.Release();

            if (_velocityTexture != null)
                _velocityTexture.Release();

            Debug.Log("the init w and h is " + Width + "  " + Height);
            // Initialize texture for storing height map
            _stateTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                //filterMode = FilterMode.Trilinear,
                filterMode = FilterMode.Bilinear,
                //filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap=true
            };

            // Initialize texture for storing flow
            _waterFluxTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            // Initialize texture for storing flow for thermal erosion
            _terrainFluxTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };

            // Velocity texture
            _velocityTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.RGFloat)
            {
                enableRandomWrite = true,
                //filterMode = FilterMode.Point,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            _FlowInOutTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.RGFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            _BuildingOutlineTexture= new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            _BuildingColorTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            _BuildingPointsTexture1 = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _BuildingPointsTexture2 = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _BuildingPointsTexture3 = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _BuildingPointsTexture4 = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _BuildingsAndProtectionTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            _pRTex = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = true
            };

            if (!_pRTex.IsCreated())
            {
                _pRTex.Create();
            }

            if (!_stateTexture.IsCreated())
                _stateTexture.Create();

            if (!_waterFluxTexture.IsCreated())
                _waterFluxTexture.Create();

            if (!_terrainFluxTexture.IsCreated())
                _terrainFluxTexture.Create();

            if (!_velocityTexture.IsCreated())
                _velocityTexture.Create();

            if (!_FlowInOutTexture.IsCreated())
                _FlowInOutTexture.Create();
            if (!_BuildingOutlineTexture.IsCreated())
                _BuildingOutlineTexture.Create();
            if (!_BuildingColorTexture.IsCreated())
                _BuildingColorTexture.Create();
            if (!_BuildingsAndProtectionTexture.IsCreated())
                _BuildingsAndProtectionTexture.Create();

            if (InitHeightMap != null)
            {
                InitHeightMap.SetFloat("_InitWaterHeight", riverHeight);
                Graphics.Blit(null, _stateTexture, InitHeightMap);
            }       
            else
                Graphics.Blit(InitialState, _stateTexture);
        
            if(InitFlowInOutMap != null)
                Graphics.Blit(null, _FlowInOutTexture, InitFlowInOutMap);
         
            Graphics.Blit(BuildingPointsMap1, _BuildingPointsTexture1);
            Graphics.Blit(BuildingPointsMap2, _BuildingPointsTexture2);
            Graphics.Blit(BuildingPointsMap3, _BuildingPointsTexture3);
            Graphics.Blit(BuildingPointsMap4, _BuildingPointsTexture4);

            int stride = sizeof(float);
            int count = 1024 * 1024;
            rBuffer = new ComputeBuffer(count, stride);
            gBuffer = new ComputeBuffer(count, stride);
            bBuffer = new ComputeBuffer(count, stride);
            aBuffer = new ComputeBuffer(count, stride);

            // Setup computation shader
            if (ErosionComputeShader != null)
            {
                _kernels = new int[SWE.Length];
                int i = 0;
                foreach (var kernelName in SWE)
                {
                    var kernel = ErosionComputeShader.FindKernel(kernelName);
                    _kernels[i++] = kernel;
                    // Set all textures
                    ErosionComputeShader.SetTexture(kernel, "HeightMap", _stateTexture);
                    ErosionComputeShader.SetTexture(kernel, "VelocityMap", _velocityTexture);
                    ErosionComputeShader.SetTexture(kernel, "FluxMap", _waterFluxTexture);
                    ErosionComputeShader.SetTexture(kernel, "TerrainFluxMap", _terrainFluxTexture);
                    ErosionComputeShader.SetTexture(kernel, "InAndOutFlowMap", _FlowInOutTexture);
                    ErosionComputeShader.SetTexture(kernel, "BuildingOutlineMap", _BuildingOutlineTexture);
                    ErosionComputeShader.SetTexture(kernel, "BuildingColorMap", _BuildingColorTexture);
                    ErosionComputeShader.SetTexture(kernel, "BuildingPoints1", _BuildingPointsTexture1);
                    ErosionComputeShader.SetTexture(kernel, "BuildingPoints2", _BuildingPointsTexture2);
                    ErosionComputeShader.SetTexture(kernel, "BuildingPoints3", _BuildingPointsTexture3);
                    ErosionComputeShader.SetTexture(kernel, "BuildingPoints4", _BuildingPointsTexture4);
                    ErosionComputeShader.SetTexture(kernel, "BuildingsAndProtectionMap", _BuildingsAndProtectionTexture);
                }
                //PhysIKA Compute Shader initialize
                phyKernel = ErosionComputeShader.FindKernel("CSMain");
                Debug.Log("The kernel is " + phyKernel);
                ErosionComputeShader.SetBuffer(phyKernel, "solid", rBuffer);
                ErosionComputeShader.SetBuffer(phyKernel, "depth", gBuffer);
                ErosionComputeShader.SetBuffer(phyKernel, "uVel", bBuffer);
                ErosionComputeShader.SetBuffer(phyKernel, "wVel", aBuffer);
                ErosionComputeShader.SetTexture(phyKernel, "Result", _pRTex);

                ErosionComputeShader.SetFloat("_InOutFlowScale", InOutFlowScale);
                ErosionComputeShader.SetInt("_Width", Width);
                ErosionComputeShader.SetInt("_Height", Height);
                ErosionComputeShader.GetKernelThreadGroupSizes(_kernels[0], out _threadsPerGroupX, out _threadsPerGroupY, out _threadsPerGroupZ);
            }

            // Debug information
            Debugger.Instance.Display("BuildingColorMap", _BuildingColorTexture);
            Debugger.Instance.Display("BuildingOutlineMap", _BuildingOutlineTexture);
            Debugger.Instance.Display("Width", Width);
            Debugger.Instance.Display("Height", Height);
            Debugger.Instance.Display("HeightMap", _stateTexture);
            Debugger.Instance.Display("FluxMap", _waterFluxTexture);
            Debugger.Instance.Display("TerrainFluxMap", _terrainFluxTexture);
            Debugger.Instance.Display("VelocityMap", _velocityTexture);
            /* ========= Setup Rendering =========== */
            // Assign state texture to materials, including physIKA into it 
            foreach (var material in Materials)
            {
                //Debug.Log(material.name);
                if (material.name == "WaterTessWithWave")
                {
                    Debug.Log("PhysIKA, go let's go");
                    material.SetTexture("_StateTex", _pRTex);
                }
                if (material.name == "Water")
                {
                    material.SetFloat("MaxWaterDepth", Settings.MaximalWaterDepth);
                }
                material.SetTexture(StateTextureKey, _stateTexture);
                
                if (material.name.StartsWith("buildingColor"))
                {
                    material.SetTexture(StateTextureKey, _BuildingColorTexture);
                    material.SetTexture("_MainTex", _BuildingOutlineTexture);
                }
                if (material.name == "WaterWithTess")
                {
                    material.SetTexture("_StateTex2", _stateTexture);
                    material.SetTexture("_StateTex", _stateTexture);
                }
                if (material.name == "GeoShader"||material.name == "TiledDirectinalFlow"|| material.name == "WaterTessWithWave")
                    material.SetTexture("_flowVelocity", _velocityTexture);

              
            }
            phyMat.SetTexture("_ShowTex", _pRTex); // for test PhysIKA
            if (barrirShader != null)
                barrirShader.SetTexture("_StateTex", _BuildingsAndProtectionTexture);
            addBuildingColorMaterial(); //测试丰富细节
        }

        public void addBuildingColorMaterial()
        {
            GameObject[] allObArray = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
            foreach (var obj in allObArray)
            {
                if (obj.name.StartsWith("Building ")&& obj.name.EndsWith("split")==false)
                {
                    //Debug.Log(obj.name);
                    int space = obj.name.IndexOf(' ', 9);
                    string tt;
                    if (space == -1)
                        tt = obj.name.Substring(9);
                    else
                        tt = obj.name.Substring(9, space - 9);
                    int t = Convert.ToInt32(tt);
                    Material m = new Material( Shader.Find("Custom/buildingColor"));
                    m.SetInt("_Index", t);
                    m.SetTexture(StateTextureKey, _BuildingColorTexture);
                    m.SetTexture("_MainTex", _BuildingOutlineTexture);
                    if (obj.GetComponent<MeshRenderer>() == null)
                        obj.AddComponent<MeshRenderer>();
                    obj.GetComponent<MeshRenderer>().sharedMaterial= m;
                }
            }
        }
    }
}
