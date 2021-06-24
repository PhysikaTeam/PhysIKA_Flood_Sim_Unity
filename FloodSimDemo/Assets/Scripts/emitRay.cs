using System.Collections;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System;
public class emitRay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    string folder = "C:\\Users\\user\\Desktop\\floodInitialstate\\shanghai\\4096";
    // Update is called once per frame
    void Update()
    {
        int n = 4096;
        //virtual
        //Vector3 terrPos = new Vector3(-5, 0, -5);
        //float offsetX = 20f / n;
        //float offsetZ = 22f / n;

        //shanghai
        Vector3 terrPos = new Vector3(1750, 0, 2800);
        float offsetX = 4500f / n;
        float offsetZ = 4000f / n;
        if (Input.GetMouseButtonDown(2))
            //getFence(terrPos, n, offsetX, n, offsetZ);
            getTerrainHeight(terrPos, n, offsetX, n, offsetZ);//
        //if (Input.GetMouseButtonDown(0))
        //    getBuildings(terrPos, n, offsetX, n, offsetZ);

        //if (Input.GetMouseButtonDown(1))
        //    getWaterSource(terrPos, n, offsetX, n, offsetZ);


        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//从摄像机发出到点击坐标的射线
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                Debug.DrawLine(ray.origin, hitInfo.point);//划出射线，在scene视图中能看到由摄像机发射出的射线
                GameObject gameObj = hitInfo.collider.gameObject;
                Debug.Log(gameObj.name + " " + hitInfo.point);
            }

        }
    }

    void getFence(Vector3 terrPos, int sizeX, float scaleX, int sizeZ, float scaleZ)
    {
        //float sizeX = myTerrain.GetComponent<TerrainCollider>().bounds.size.x;
        //float scaleX = myTerrain.transform.localScale.x;

        //float sizeZ = myTerrain.GetComponent<TerrainCollider>().bounds.size.z;
        //float scaleZ = myTerrain.transform.localScale.z;

        //Debug.Log("the X size = " + sizeX);
        //Debug.Log("the X scale = " + scaleX);
        //Debug.Log("the Z size = " + sizeZ);
        //Debug.Log("the Z scale = " + scaleZ);

        //Vector3 terrPos = myTerrain1.GetPosition();
        List<float> t = new List<float>();
        List<int> BuildingTag = new List<int>();
        Dictionary<string, int> dict = new Dictionary<string, int>();
        float maxHeight = float.MinValue;
        float minHeight = float.MaxValue;
        string maxBuildingName = "", minBuildingName = "";
        float riverHeight = 0;
        Debug.Log("the position: " + terrPos);
        Vector3 tmpPos;


        int numberOfBuildings = 1;

        for (int i = 0; i < sizeX; i++)
        {
            tmpPos = terrPos;
            tmpPos.y = -1; //取地表平面之下的点
            tmpPos.x += i * scaleX;
            for (int j = 0; j < sizeZ; j++)
            {

                tmpPos.z = terrPos.z + j * scaleZ;
                Vector3 origin = new Vector3(tmpPos.x, 10000000, tmpPos.z);

                Vector3 direction = tmpPos - origin;
                //Ray ray = new Ray(origin, direction);
                Ray ray = new Ray(origin, new Vector3(0, -1, 0));
                RaycastHit hit;
                bool isCollider = Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (isCollider)
                {
                    float tmp = 0.0f;
                    string nn = hit.transform.gameObject.name;
                    if (nn.StartsWith("Post ") || nn.StartsWith("Rail "))
                    {
                        tmp = 1.0f;
                    }
                    t.Add(tmp);
                    if (maxHeight < tmp)
                        maxHeight = tmp;
                    if (minHeight > tmp)
                        minHeight = tmp;

                  

                }
                else
                {
                    t.Add(0);
                    //                  BuildingTag.Add(0);
                    //print("what the fuck?");
                }
            }
            //   Debug.Log("part1:" + i + "/" + sizeX);
        }
        Debug.Log("maxHeight:" + maxHeight);
        Debug.Log("minHeight:" + minHeight);
        Debug.Log("maxHeight:" + maxBuildingName);
        Debug.Log("minHeight:" + minBuildingName);
        // Debug.Log("riverHeight:" + riverHeight);
        //  Debug.Log("all buildings:"+(numberOfBuildings - 1));
        //Texture2D heightMap = new Texture2D(sizeX, sizeZ, TextureFormat.RGBA32,false);
        //for(int i=0;i<sizeX;i++)
        //{
        //    for(int j=0;j<sizeZ;j++)
        //    {
        //        heightMap.SetPixel(i, j, new Color(t[i * sizeZ + j] / maxHeight, 0, 0, 0));
        //    }
        //}



        StreamWriter sw;
        FileInfo fi = new FileInfo(folder + "//" + "fenceHeightMap");
        if (!fi.Exists)
        {
            Debug.Log("写入 不存在");
            sw = fi.CreateText();
            //sw.WriteLine ("this is a line.");
        }
        else
        {
            Debug.Log("写入 存在");
            sw = fi.CreateText();
            // sw = fi.AppendText ();
            // sw.WriteLine ("this is a line.");
        }

        for (int i = 0; i < t.Count; i++)
        {

            sw.Write((t[i] - minHeight) / (maxHeight - minHeight));
            sw.Write(" ");
            //  if(i%sizeX==0) 
            //       Debug.Log("part1:" + i/sizeX + "/" + sizeX);
        }
        sw.Close();
        sw.Dispose();


    }

    void getTerrainHeight(Vector3 terrPos, int sizeX, float scaleX, int sizeZ, float scaleZ)
    {
        //float sizeX = myTerrain.GetComponent<TerrainCollider>().bounds.size.x;
        //float scaleX = myTerrain.transform.localScale.x;

        //float sizeZ = myTerrain.GetComponent<TerrainCollider>().bounds.size.z;
        //float scaleZ = myTerrain.transform.localScale.z;

        //Debug.Log("the X size = " + sizeX);
        //Debug.Log("the X scale = " + scaleX);
        //Debug.Log("the Z size = " + sizeZ);
        //Debug.Log("the Z scale = " + scaleZ);

        //Vector3 terrPos = myTerrain1.GetPosition();
        List<float> t = new List<float>();
        List<int> BuildingTag = new List<int>();
        Dictionary<string, int> dict = new Dictionary<string, int>();
        float maxHeight = float.MinValue;
        float minHeight = -6.0f;
        string maxBuildingName="", minBuildingName="";
        float riverHeight = 0;
        Debug.Log("the position: " + terrPos);
        Vector3 tmpPos;


        int numberOfBuildings = 1;

        for (int i = 0; i < sizeX; i++)
        {
            tmpPos = terrPos;
            tmpPos.y = -1; //取地表平面之下的点
            tmpPos.x += i * scaleX;
            for (int j = 0; j < sizeZ; j++)
            {
                
                tmpPos.z = terrPos.z+ j * scaleZ;
                Vector3 origin = new Vector3(tmpPos.x, 10000000, tmpPos.z);
              
                Vector3 direction = tmpPos - origin;
                //Ray ray = new Ray(origin, direction);
                Ray ray = new Ray(origin, new Vector3(0,-1,0));
                RaycastHit hit;
                bool isCollider = Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (isCollider)
                {
                    float tmp = hit.point.y;
                    string nn = hit.transform.gameObject.name;

                    //if (nn == "river")
                    //{
                    //    tmp =-4.0f;
                    //}
                    //else if (nn.StartsWith("Building")||nn.StartsWith("Terrain")||nn.StartsWith("Post ")||nn.StartsWith("Rail "))
                    //   tmp= hit.point.y;
                    //else 
                    //    tmp= -4.0f;
                    tmp = Mathf.Max(tmp, -6.0f);
                    //tmp = hit.point.y;
                    t.Add(tmp);
                    if (tmp > maxHeight)
                    {
                        maxHeight = tmp;
                        maxBuildingName = nn;
                    }
                        
                    if (tmp < minHeight)
                    {
                        minHeight = tmp;
                        minBuildingName = nn;
                    }
                    

                    //if(hit.collider.gameObject.name.StartsWith("barrier") ==true)
                    //{
                    //    Debug.Log(hit.collider.gameObject.name);
                    //    Debug.Log(hit.point.y);
                    //    riverHeight = hit.point.y;
                    //}

                    //string n = hit.collider.gameObject.name;
                    //if (n.StartsWith("Cube")||n.StartsWith("Cylinder"))
                    //{
                    //    if (dict.ContainsKey(n) == false)
                    //        dict[n] = numberOfBuildings++;
                    //    BuildingTag.Add(dict[n]);
                    //}
                    //else
                    //{
                    //    BuildingTag.Add(0);
                    //}

                }
                else
                {
                    t.Add(-6);
  //                  BuildingTag.Add(0);
                    //print("what the fuck?");
                }
            }
         //   Debug.Log("part1:" + i + "/" + sizeX);
        }
        Debug.Log("maxHeight:"+maxHeight);
        Debug.Log("minHeight:" + minHeight);
        Debug.Log("maxHeight:" + maxBuildingName);
        Debug.Log("minHeight:" + minBuildingName);
        // Debug.Log("riverHeight:" + riverHeight);
        //  Debug.Log("all buildings:"+(numberOfBuildings - 1));
        //Texture2D heightMap = new Texture2D(sizeX, sizeZ, TextureFormat.RGBA32,false);
        //for(int i=0;i<sizeX;i++)
        //{
        //    for(int j=0;j<sizeZ;j++)
        //    {
        //        heightMap.SetPixel(i, j, new Color(t[i * sizeZ + j] / maxHeight, 0, 0, 0));
        //    }
        //}



        StreamWriter sw;
        FileInfo fi = new FileInfo(folder + "//" + "heightMap1213");
        if (!fi.Exists)
        {
            Debug.Log("写入 不存在");
            sw = fi.CreateText();
            //sw.WriteLine ("this is a line.");
        }
        else
        {
            Debug.Log("写入 存在");
            sw = fi.CreateText();
            // sw = fi.AppendText ();
            // sw.WriteLine ("this is a line.");
        }

        for (int i = 0; i <t.Count; i++)
        {
           
            sw.Write((t[i]-minHeight)/ (maxHeight-minHeight));
            sw.Write(" ");
         //  if(i%sizeX==0) 
         //       Debug.Log("part1:" + i/sizeX + "/" + sizeX);
        }
        sw.Close();
        sw.Dispose();


    }
    void getBuildings(Vector3 terrPos, int sizeX, float scaleX, int sizeZ, float scaleZ)
    {
        List<int> BuildingTag = new List<int>();
        Dictionary<string, int> dict = new Dictionary<string, int>();
        Debug.Log("the position: " + terrPos);
        Vector3 tmpPos;
        int numberOfBuildings = 1;
        for (int i = 0; i < sizeX; i++)
        {
            tmpPos = terrPos;
            tmpPos.y = -1; //取地表平面之下的点
            tmpPos.x += i * scaleX;
            for (int j = 0; j < sizeZ; j++)
            {
                tmpPos.z = terrPos.z + j * scaleZ;
                Vector3 origin = new Vector3(tmpPos.x, 1000000, tmpPos.z);

                Vector3 direction = tmpPos - origin;
                //Ray ray = new Ray(origin, direction);
                Ray ray = new Ray(origin, new Vector3(0, -1, 0));
                RaycastHit hit;
                bool isCollider = Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (isCollider)
                {
                    string n = hit.collider.gameObject.name;
                    if (n.StartsWith("Building"))
                    {
                        string tt = n.Substring(9);

                        int space = n.IndexOf(' ', 9);
    
                        if (space == -1)
                            tt = n.Substring(9);
                        else
                            tt = n.Substring(9, space - 9);

                        int t = Convert.ToInt32(tt);
                        if (dict.ContainsKey(n) == false)
                        {
                            dict[n] = t;
                            Debug.Log(t);
                        }
                           
                        BuildingTag.Add(dict[n]);
                    }
                    else
                    {
                        BuildingTag.Add(0);
                    }
                }
                else
                {
                    BuildingTag.Add(0);
                }
            }
        }
       
        StreamWriter sw;
        FileInfo fi = new FileInfo(folder + "//" + "BuildingTag1118");
        if (!fi.Exists)
        {
            Debug.Log("写入 不存在");
            sw = fi.CreateText();
            //sw.WriteLine ("this is a line.");
        }
        else
        {
            Debug.Log("写入 存在");
            sw = fi.CreateText();
            // sw = fi.AppendText ();
            // sw.WriteLine ("this is a line.");
        }
        for (int i = 0; i < BuildingTag.Count; i++)
        {
            sw.Write(BuildingTag[i]);
            sw.Write(" ");
        }
        sw.Close();
        sw.Dispose();
    }

    void getWaterSource(Vector3 terrPos, int sizeX, float scaleX, int sizeZ, float scaleZ)
    {
        List<float> t = new List<float>();
        float maxHeight = 1.0f;
        Debug.Log("the position: " + terrPos);
        Vector3 tmpPos;
        float minH = float.MaxValue, maxH = float.MinValue;

        Debug.Log("minH " + minH);
        Debug.Log("maxH " + maxH);

        for (int i = 0; i < sizeX; i++)
        {
            tmpPos = terrPos;
            tmpPos.y = -1; //取地表平面之下的点
            tmpPos.x += i * scaleX;
            for (int j = 0; j < sizeZ; j++)
            {

                tmpPos.z = terrPos.z + j * scaleZ;
                Vector3 origin = new Vector3(tmpPos.x, 1000000, tmpPos.z);

                Vector3 direction = tmpPos - origin;
                //Ray ray = new Ray(origin, direction);
                Ray ray = new Ray(origin, new Vector3(0, -1, 0));
                RaycastHit hit;
                bool isCollider = Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (isCollider)
                {
                    if (hit.transform.gameObject.name == "river")
                    {
                        t.Add(1.0f);
                        minH = Mathf.Min(minH, hit.point.y);
                        maxH = Mathf.Max(maxH, hit.point.y);
                    }
                        
                    else
                        t.Add(0.0f);
                   
                    //t.Add(hit.point.y);
                    //if (hit.point.y > maxHeight)
                    //maxHeight = 1.0;
                    // if (hit.point.y != 0)
                    //  {
                    //     print("碰撞对象: " + hit.collider.name + " 碰撞点: " + hit.point);
                    // }
                }
                else
                {
                    t.Add(0);
                    //print("what the fuck?");
                }
            }
        }
        Debug.Log("minH "+minH);
        Debug.Log("maxH " + maxH);


        //Texture2D heightMap = new Texture2D(sizeX, sizeZ, TextureFormat.RGBA32, false);
        //for (int i = 0; i < sizeX; i++)
        //{
        //    for (int j = 0; j < sizeZ; j++)
        //    {
        //        heightMap.SetPixel(i, j, new Color(t[i * sizeZ + j] / maxHeight, 0, 0, 0));
        //    }
        //}



        StreamWriter sw;
        FileInfo fi = new FileInfo(folder + "//" + "waterSource");
        if (!fi.Exists)
        {
            Debug.Log("写入 不存在");
            sw = fi.CreateText();
            //sw.WriteLine ("this is a line.");
        }
        else
        {
            Debug.Log("写入 存在");
            sw = fi.CreateText();
            // sw = fi.AppendText ();
            // sw.WriteLine ("this is a line.");
        }

        for (int i = 0; i < t.Count; i++)
        {

            sw.Write(t[i] / maxHeight);
            sw.Write(" ");
        }
        Debug.Log(t.Count);
        sw.Close();
        sw.Dispose();
    }

    void  getXiHuLake(Vector3 terrPos, int sizeX, float scaleX, int sizeZ, float scaleZ)
    {
        List<float> t = new List<float>();
        float maxHeight = 1.0f;
        Debug.Log("the position: " + terrPos);
        Vector3 tmpPos;
        for (int i = 0; i < sizeX; i++)
        {
            tmpPos = terrPos;
            tmpPos.y = -1; //取地表平面之下的点
            tmpPos.x += i * scaleX;
            for (int j = 0; j < sizeZ; j++)
            {

                tmpPos.z = terrPos.z + j * scaleZ;
                Vector3 origin = new Vector3(tmpPos.x, 1000000, tmpPos.z);

                Vector3 direction = tmpPos - origin;
                //Ray ray = new Ray(origin, direction);
                Ray ray = new Ray(origin, new Vector3(0, -1, 0));
                RaycastHit hit;
                bool isCollider = Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (isCollider)
                {
                    if (hit.transform.gameObject.name == "Mesh10")
                        t.Add(1.0f);
                    else
                        t.Add(0.0f);
                    //t.Add(hit.point.y);
                    //if (hit.point.y > maxHeight)
                    //maxHeight = 1.0;
                    // if (hit.point.y != 0)
                    //  {
                    //     print("碰撞对象: " + hit.collider.name + " 碰撞点: " + hit.point);
                    // }
                }
                else
                {
                    t.Add(0);
                    //print("what the fuck?");
                }
            }
        }
        Debug.Log(maxHeight);

        //Texture2D heightMap = new Texture2D(sizeX, sizeZ, TextureFormat.RGBA32, false);
        //for (int i = 0; i < sizeX; i++)
        //{
        //    for (int j = 0; j < sizeZ; j++)
        //    {
        //        heightMap.SetPixel(i, j, new Color(t[i * sizeZ + j] / maxHeight, 0, 0, 0));
        //    }
        //}



        StreamWriter sw;
        FileInfo fi = new FileInfo(Application.streamingAssetsPath + "//" + "heightMapXihuLake");
        if (!fi.Exists)
        {
            Debug.Log("写入 不存在");
            sw = fi.CreateText();
            //sw.WriteLine ("this is a line.");
        }
        else
        {
            Debug.Log("写入 存在");
            sw = fi.CreateText();
            // sw = fi.AppendText ();
            // sw.WriteLine ("this is a line.");
        }

        for (int i = 0; i < t.Count; i++)
        {

            sw.Write(t[i] / maxHeight);
            sw.Write(" ");
        }
        Debug.Log(t.Count);
        sw.Close();
        sw.Dispose();
    }


}
