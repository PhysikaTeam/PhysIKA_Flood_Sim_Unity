using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float m_LastUpdateShowTime = 0f;  
    private float m_UpdateShowDeltaTime = 0.01f;
    private int m_FrameUpdate = 0; 

    private float m_FPS = 0;
    private bool isStart = false;

    // Start is called before the first frame update
    void Start()
    {
        m_LastUpdateShowTime = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        m_FrameUpdate++;
        if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
        {
            m_FPS = m_FrameUpdate / (Time.realtimeSinceStartup - m_LastUpdateShowTime);
            m_FrameUpdate = 0;
            m_LastUpdateShowTime = Time.realtimeSinceStartup;
        }

        if (Input.GetMouseButtonDown(2))
            isStart = !isStart;

    }
    private void OnGUI()
    {

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.MiddleCenter;

        style.normal.textColor = Color.black;
  
        string text = string.Format("FPS: {0:F1} ", m_FPS);

        style.normal.background = null;
        style.normal.textColor = new Color(0, 0, 0);
        style.fontSize = 18;
        GUI.Label(new Rect(1290, 75, 100, 30), text, style);


        string gridText = "当前中心场景网格数：1024 x 1024";
        GUI.Label(new Rect(1175, 105, 200, 30), gridText, style);

    }

}
