using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawBarrier : MonoBehaviour
{
    public bool beginDraw = false;
    public Material lineMaterial;
    public float interval = 0.02f;
    private List<Vector3> posList=new List<Vector3> ();
    private Vector3 curPos;
    void DrawLine(Vector3 start, Vector3 end)
    {
        if (!beginDraw)
            return;
        GL.PushMatrix();
        GL.LoadOrtho();

        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);

        GL.Vertex3(start.x, start.y, start.z);
        GL.Vertex3(end.x, end.y, end.z);

        GL.End();
        GL.PopMatrix();
    }

    void OnGUI()
    {
        Event e = Event.current;

        if (e != null && e.type != null)
        {
            if (e.type == EventType.MouseDown)
            {
                beginDraw = true;
                curPos = Input.mousePosition;
                Debug.Log(curPos);
            }
            if (e.type == EventType.MouseDrag)
            {

                if (Vector3.Distance(curPos, Input.mousePosition) > interval)
                {
                    curPos = Input.mousePosition;
                    posList.Add(new Vector3(curPos.x / Screen.width, curPos.y / Screen.height, 0));
                    //DrawLine();
                    Debug.Log(new Vector3(curPos.x / Screen.width, curPos.y / Screen.height, 0));
                }
            }

            if (e.type == EventType.MouseUp)
            {
                beginDraw = false;
                ClearLines();
            }
        }
    }


    void DrawLine()
    {
        if (!beginDraw)
            return;
        GL.PushMatrix();
        GL.LoadPixelMatrix();

        //lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        for (int i = 0; i < posList.Count - 1; i++)
        {
            Vector3 pos = posList[i];

            GL.Vertex3(pos.x, pos.y, pos.z);
            GL.Vertex3(posList[i + 1].x, posList[i + 1].y, posList[i + 1].z);
        }

        GL.End();
        GL.PopMatrix();
    }


        void ClearLines()
    {
        beginDraw = false;
        posList.Clear();
        curPos = Vector3.zero;
    }

    private void OnPostRender()
    {
        if (beginDraw)
        {
            DrawLine();
            Debug.Log("draw: " + posList.Count);
        }
           
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
