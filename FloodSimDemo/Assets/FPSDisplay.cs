using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ChartAndGraph;
using System.IO;

public class FPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;

    private float m_ClientFrameTime;
    private float m_RenderFrameTime;
    private int m_FrameCounter;
    private float m_MaxTimeAccumulator;
    private float m_MaxFrameTime;

    private float m_ClientTimeAccumulator;
    private float m_RenderTimeAccumulator;
    private bool isStart = false;
    private int count = 0;

    float start, curr;
    List<float> FPSList = new List<float>();
    public GraphChart graph;
    public Camera FPSCamera;


    // Start is called before the first frame update
    void Start()
    {
        start = Time.time;
        FPSCamera.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
            isStart = !isStart;
        //deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        if (Input.GetKeyDown(KeyCode.F))
        {
            var t = graph.GetComponent<GraphChartSample>();
            t.changeData(FPSList, "FPS Curve");
            ExportFPS();
            FPSCamera.enabled = true;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            FPSCamera.enabled = false;
        }

    }
    void FixedUpdate()
    {
        count++;
        if (isStart)
        {
            if (count % 120 == 0)
            {
                float outFPS = 1f / (m_MaxFrameTime + 0.003f);
                FPSList.Add(outFPS);
            }
        }
    }


    public void UpdateFrameTime()
    {
        float frameTime = 0.0f;
        float renderTime = 0.0f;
#if UNITY_EDITOR
        frameTime = UnityEditor.UnityStats.frameTime;
        renderTime = UnityEditor.UnityStats.renderTime;
#endif

        m_ClientTimeAccumulator += frameTime;
        m_RenderTimeAccumulator += renderTime;
        m_MaxTimeAccumulator += Mathf.Max(frameTime, renderTime);
        m_FrameCounter++;
        bool flag = m_ClientFrameTime == 0f && m_RenderFrameTime == 0f;
        bool flag2 = m_FrameCounter > 30 || m_ClientTimeAccumulator > 0.3f || m_RenderTimeAccumulator > 0.3f;
        if (flag || flag2)
        {
            m_ClientFrameTime = m_ClientTimeAccumulator / (float)m_FrameCounter;
            m_RenderFrameTime = m_RenderTimeAccumulator / (float)m_FrameCounter;
            m_MaxFrameTime = m_MaxTimeAccumulator / (float)m_FrameCounter;
        }
        if (flag2)
        {
            m_ClientTimeAccumulator = 0f;
            m_RenderTimeAccumulator = 0f;
            m_MaxTimeAccumulator = 0f;
            m_FrameCounter = 0;
        }

    }


    private void OnGUI()
    {
        UpdateFrameTime();
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.MiddleCenter;

        style.normal.textColor = Color.black;
        float msec = deltaTime * 1000.0f;
        //float fps = 1.0f / deltaTime;
        

        string text = string.Format("{0:F1} FPS ({1:F1}ms)", 1f / (m_MaxFrameTime+0.003f), (m_MaxFrameTime+0.003f) * 1000f);
        //string text = "FPS  :  " + string.Format("{0:F0}",fps);
        //Debug.Log("(" + Screen.width + ", " + Screen.height + ")");

        style.normal.background = null;
        style.normal.textColor = new Color(0, 0, 0);
        style.fontSize = 18;
        GUI.Label(new Rect(1290, 75, 100, 30), text, style);


        string gridText = "当前中心场景网格数：1024 x 1024";
        GUI.Label(new Rect(1175, 105, 200, 30), gridText, style);

    }

    void ExportFPS()
    {
       
        using (StreamWriter sw = new StreamWriter("FPS results.txt"))
        {
            foreach (float fps in FPSList)
            {
                sw.WriteLine(fps);

            }
        }
        
    }
}
