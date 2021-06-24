using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
public class uiDrawLines : MonoBehaviour
{
    public bool drawOpen = false;
    private Material lineMaterial;
    private bool beginDraw;
    public Vector3 curPos;
    public static List<Vector3> posList=new List<Vector3> ();
    public  float interval=0.2f;
    public bool isStraight = true;
    public static Vector3 beg, ed;
    public static bool complete = false;
    public static bool isDrawing = false;

    // Start is called before the first frame update
    [System.Obsolete]
    void Start()
    {
        lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +

"SubShader { Pass {" +

"   BindChannels { Bind \"Color\",color }" +

"   Blend SrcAlpha OneMinusSrcAlpha" +

"   ZWrite Off Cull Off Fog { Mode Off }" +

"} } }")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPostRender()
    {
        DrawLine();
    }
    void drawSphere(Vector3 p,float r)
    {
        GL.Begin(GL.TRIANGLES);
        GL.Color(Color.cyan);
        int n = 36;
        float pi = 3.1415926f;
       for(int i=0;i<n;i++)
       {
            GL.Vertex(p);
            Vector3 a = new Vector3(Mathf.Sin(i * 2 * pi / n), Mathf.Cos(i * 2 * pi / n), 0);
            Vector3 b = new Vector3(Mathf.Sin((i+1) * 2 * pi / n), Mathf.Cos((i+1) * 2 * pi / n), 0);
            GL.Vertex(p + r * a);
            GL.Vertex(p + r * b);
       }
        GL.End();
    }
    void DrawLine()
    {
        if (!drawOpen||!beginDraw)
            return;
        if (posList.Count < 2)
            return;

        GL.PushMatrix();
       // GL.LoadPixelMatrix();
        lineMaterial.SetPass(0);
        GL.LoadOrtho();

        //GL.Color(Color.red);
        //GL.Begin(GL.QUADS);

        GL.Begin(GL.LINES);

        //GL.Begin(GL.QUADS);
        //if (isStraight == false)
        //{

        GL.Color(Color.blue);

        float sphereR = 0.006f;
        float quardW = 0.002f;
        Debug.Log("List Count = " + posList.Count);
            for (int i = 0; i < posList.Count - 1; i++)
            {
                Vector3 a = posList[i];
                Vector3 b = posList[i + 1];

                Vector3 n = Vector3.Normalize(a - b);
                Debug.Log(n);
                Vector3 n1 = new Vector3(-n.y, n.x, 0);
                n1 = n1 * quardW;

                GL.Vertex3(a.x-n1.x, a.y-n1.y, a.z-n1.z);
                GL.Vertex3(a.x + n1.x, a.y + n1.y, a.z + n1.z);

                GL.Vertex3(a.x + n1.x, a.y + n1.y, a.z + n1.z);
                GL.Vertex3(b.x + n1.x, b.y + n1.y, b.z + n1.z);

                GL.Vertex3(b.x+n1.x, b.y+n1.y, b.z+n1.z);
                GL.Vertex3(b.x - n1.x, b.y - n1.y, b.z - n1.z);

            GL.Vertex3(b.x - n1.x, b.y - n1.y, b.z - n1.z);
            GL.Vertex3(a.x - n1.x, a.y - n1.y, a.z - n1.z);
        }
        GL.End();

        for (int i = 0; i < posList.Count; i++)
            drawSphere(posList[i], sphereR);

        //}
        //else
        //{
        //    if(posList.Count>=2)
        //    {
        //        beg = posList[0];
        //        ed = posList[posList.Count - 1];
        //        GL.Vertex3(beg.x, beg.y, beg.z);
        //        GL.Vertex3(ed.x, ed.y, ed.z);
        //    }
        //}
       
        GL.PopMatrix();
    }

    bool mouseDrag = false;
    void OnGUI()
    {
        if (complete)
        {
            ClearLines();
            complete = false;
        }
        if (!drawOpen||!isDrawing)
            return;

        if (Event.current != null) 
        {

            if (Event.current.type == EventType.MouseDown)
            {
                beginDraw = true;
                mouseDrag = true;
                curPos = Input.mousePosition;
                Debug.Log("curPos = " + curPos);
                posList.Add(new Vector3(curPos.x / Screen.width, curPos.y / Screen.height, 0));

            } 
        }

            //if (e.type == EventType.MouseDrag)
            //{

            //    if (Vector3.Distance(curPos, Input.mousePosition) > interval)
            //    {
            //        curPos = Input.mousePosition;
            //        posList.Add(new Vector3(curPos.x / Screen.width, curPos.y / Screen.height, 0));
            //    }
            //    DrawLine();
            //}
            //if (e.type == EventType.MouseUp)
            //{
            //    //beginDraw = false;
            //    //CustomTerrain.buildBarrier = true;
            //   // ClearLines();
            //}
        
        //Debug.Log("List Count = " + posList.Count);

    }
    void ClearLines()
    {
        posList.Clear();
        Debug.Log("Complete: " + posList.Count);
    }



}
