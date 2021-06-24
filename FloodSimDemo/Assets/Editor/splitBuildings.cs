using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
public class union_set
{
    public List<int> parent;
    public union_set(int l)
    {
        parent = new List<int>();
        for (int i = 0; i < l; i++)
            parent.Add(i);
    }
    public int findParent(int i)
    {
        if(parent[i]!=i)
        {
            parent[i] = findParent(parent[i]);
        }
        return parent[i];
    }
    public void union(int i,int j,int k)
    {
        int fi = findParent(i), fj = findParent(j), fk = findParent(k);
        parent[fj] = fi;
        parent[fk] = fi;
    }
    public void union(int i, int j)
    {
        int fi = findParent(i), fj = findParent(j);
        parent[fj] = fi;
    }
}

public class boundset
{

    public List<Bounds> allBounds;
    //public List<int> parent;
    public boundset()
    {
        allBounds = new List<Bounds>();
 
    }
    
   public void add(Bounds bounds)
    {
        
        List<Bounds> newBounds=new List<Bounds> ();
        Vector3 aa = bounds.min;
        Vector3 bb = bounds.max;
        aa.z = bb.z = 0;
        bounds.SetMinMax(aa, bb);
        foreach(var b in allBounds)
        {
            
            if (b.Intersects(bounds) == true)
                bounds.SetMinMax(new Vector3(Mathf.Min(b.min.x, bounds.min.x), Mathf.Min(b.min.y, bounds.min.y), Mathf.Min(b.min.z, bounds.min.z)), new Vector3(Mathf.Max(b.max.x, bounds.max.x), Mathf.Max(b.max.y, bounds.max.y), Mathf.Max(b.max.z, bounds.max.z)));
            else
                newBounds.Add(b);
        }
        newBounds.Add(bounds);
        allBounds = newBounds;  
    }
    public int query(Bounds b)
    {
        Vector3 aaa = b.min;
        Vector3 bbb = b.max;
        aaa.z = bbb.z = 0;
        b.SetMinMax(aaa, bbb);
        int cnt = 0;
        foreach (var bb in allBounds)
        {
            if (bb.Intersects(b) == true)
                return cnt;
            cnt++;
        }
        return -1;
    }
}


public class splitBuildings : Editor
{

    [MenuItem("myTools/splitBuildings")]
    public static void splitWithBBox()
    {
        //var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var previousSelection = Selection.objects;
        // Selection.objects = allGos;
        //var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        //var selectedGameobj = Selection.gameObjects;
        var selectGameobjs = Selection.gameObjects;
        foreach (var obj in selectGameobjs)
        {
            splitAGameobjWithBBOX(obj);
        }

    }

    public static void splitWithBBoxNumber()
    {
        //var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var previousSelection = Selection.objects;
        // Selection.objects = allGos;
        //var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        //var selectedGameobj = Selection.gameObjects;
        var selectGameobjs = Selection.gameObjects;
        foreach (var obj in selectGameobjs)
        {
            splitAGameobjWithBBOXNumber(obj);
        }

    }

    static void splitAGameobjWithBBOXNumber(GameObject obj)
    {
        Debug.Log(obj.name);
        Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        Material[] materials = obj.GetComponent<MeshRenderer>().sharedMaterials;
        var allTriangles = mesh.triangles;
        int sz = mesh.vertices.Length;
        Vector3[] pos = mesh.vertices;

        boundset bs = new boundset();
        List<Bounds> allBounds = new List<Bounds>();
        for (int i = 0; i < allTriangles.Length; i += 3)
        {
            Vector3 a = pos[allTriangles[i]], b = pos[allTriangles[i + 1]], c = pos[allTriangles[i + 2]];
            Bounds bound = new Bounds(a, new Vector3(0, 0, 0));
            bound = include(bound, b);
            bound = include(bound, c);
            bs.add(bound);
            allBounds.Add(bound);
        }

        Debug.Log(bs.allBounds.Count);
    }

    public static void splitWithDst(float dst)
    {
        //var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var previousSelection = Selection.objects;
        // Selection.objects = allGos;
        //var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        //var selectedGameobj = Selection.gameObjects;
        var selectGameobjs = Selection.gameObjects;
        foreach (var obj in selectGameobjs)
        {
            splitAGameobjWithDistance(obj,dst);
        }

    }

    public static void splitWithDstNumber(float dst)
    {
        //var allGos = Resources.FindObjectsOfTypeAll(typeof(GameObject));
        //var previousSelection = Selection.objects;
        // Selection.objects = allGos;
        //var selectedTransforms = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
        //var selectedGameobj = Selection.gameObjects;
        var selectGameobjs = Selection.gameObjects;
        foreach (var obj in selectGameobjs)
        {
            splitAGameobjWithDistanceNumber(obj, dst);
        }

    }
    static void splitAGameobjWithDistanceNumber(GameObject obj, float dst)
    {
        Debug.Log(obj.name);
        Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        Material[] materials = obj.GetComponent<MeshRenderer>().sharedMaterials;
        var allTriangles = mesh.triangles;
        int sz = mesh.vertices.Length;

        Vector3[] pos = mesh.vertices;
        Dictionary<int, List<int>> newTris = new Dictionary<int, List<int>>();
        union_set us = new union_set(sz);
        float minx = pos[0].x, maxx = pos[0].x, miny = pos[0].y, maxy = pos[0].y, minz = pos[0].z, maxz = pos[0].z;
        for (int i = 0; i < sz; i++)
        {
            Vector3 tmp = pos[i];
            minx = Mathf.Min(minx, tmp.x);
            maxx = Mathf.Max(maxx, tmp.x);
            miny = Mathf.Min(miny, tmp.y);
            maxy = Mathf.Max(maxy, tmp.y);
            minz = Mathf.Min(minz, tmp.z);
            maxz = Mathf.Max(maxz, tmp.z);
        }

        Debug.Log(maxx - minx);
        Debug.Log(maxy - miny);
        Debug.Log(maxz - minz);

        for (int i = 0; i < allTriangles.Length; i += 3)
            us.union(allTriangles[i], allTriangles[i + 1], allTriangles[i + 2]);

        for (int i = 0; i < sz; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (Vector3Equal(pos[i], pos[j], dst) == true)
                    us.union(i, j);

            }
        }

        for (int i = 0; i < allTriangles.Length; i += 3)
        {
            int f = us.findParent(allTriangles[i]);
            if (newTris.ContainsKey(f) == false)
                newTris.Add(f, new List<int>());
            newTris[f].Add(allTriangles[i]);
            newTris[f].Add(allTriangles[i + 1]);
            newTris[f].Add(allTriangles[i + 2]);
        }
        Debug.Log("can split to "+newTris.Count);



    }

    static bool Vector3Equal(Vector3 a,Vector3 b,float error)
    {
        //float error = 0.01f;
        return Vector3.Magnitude(a - b) < error;
       // return (Mathf.Abs(a.x - b.x) <= error) && (Mathf.Abs(a.y - b.y) <= error) && (Mathf.Abs(a.z - b.z) <= error);
    }

    static Bounds include(Bounds b,Vector3 p)
    {
        b.SetMinMax(new Vector3(Mathf.Min(b.min.x, p.x), Mathf.Min(b.min.y, p.y), Mathf.Min(b.min.z, p.z)), new Vector3(Mathf.Max(b.max.x, p.x), Mathf.Max(b.max.y, p.y), Mathf.Max(b.max.z, p.z)));
        return b;
    }

    static void splitAGameobjWithBBOX(GameObject obj)
    {
        Debug.Log(obj.name);
        Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        Material[] materials = obj.GetComponent<MeshRenderer>().sharedMaterials;
        var allTriangles = mesh.triangles;
        int sz = mesh.vertices.Length;
        Vector3[] pos = mesh.vertices;

        boundset bs = new boundset();
        List<Bounds> allBounds=new List<Bounds> ();
        for(int i=0;i<allTriangles.Length;i+=3)
        {
            Vector3 a = pos[allTriangles[i]], b = pos[allTriangles[i + 1]], c = pos[allTriangles[i + 2]];
            Bounds bound = new Bounds(a,new Vector3(0,0,0));
            bound=include(bound, b);
            bound=include(bound, c);
            bs.add(bound);
            allBounds.Add(bound);
        }
        Debug.Log(bs.allBounds.Count);
        
        Dictionary<int, List<int>> newTris = new Dictionary<int, List<int>>();

        for (int i = 0; i < allTriangles.Length; i += 3)
        {
            int index = bs.query(allBounds[i / 3]);
            if (newTris.ContainsKey(index) == false)
                newTris[index] = new List<int>();
            newTris[index].Add(allTriangles[i]);
            newTris[index].Add(allTriangles[i + 1]);
            newTris[index].Add(allTriangles[i + 2]);
        }
        // Debug.Log(newTris.Count);
        int cnt = 0;
        GameObject parent = new GameObject();
        parent.name = obj.name + "split";
        parent.transform.parent = obj.transform.parent.gameObject.transform;
        parent.transform.localPosition = obj.transform.localPosition;
        parent.transform.rotation = obj.transform.rotation;
        parent.transform.localScale = obj.transform.localScale;
        foreach (var t in newTris.Values)
        {
            Mesh mesh1 = new Mesh();

            Dictionary<int, int> dict = new Dictionary<int, int>();
            List<int> nnTris = new List<int>();
            List<Vector3> nVertex = new List<Vector3>();
            foreach (var i in t)
            {
                if (dict.ContainsKey(i) == false)
                {
                    dict[i] = nVertex.Count;
                    nVertex.Add(pos[i]);
                }
                nnTris.Add(dict[i]);
            }




            mesh1.vertices = nVertex.ToArray();
            //mesh1.uv = (Vector2[])mesh.uv.Clone();
            //mesh1.normals = (Vector3[])mesh.normals.Clone();
            mesh1.triangles = nnTris.ToArray();
            mesh1.RecalculateNormals();
            GameObject obj1 = new GameObject();
            obj1.name = obj.name + Convert.ToString(cnt++);
            Debug.Log(obj1.name);
            obj1.AddComponent<MeshFilter>();
            obj1.GetComponent<MeshFilter>().sharedMesh = mesh1;
            obj1.AddComponent<MeshRenderer>();
            //for(int i=0;i<materials.Length;i++)
            //       materials[i]= Resources.Load<Material>("White");
            obj1.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Red");

            obj1.transform.parent = parent.transform;

            obj1.transform.localPosition = new Vector3(0,0,0);
            obj1.transform.rotation = new Quaternion(0, 0, 0, 0);
            obj1.transform.localScale =new Vector3(1,1,1);


            //using (StreamWriter streamWriter = new StreamWriter(string.Format("{0}{1}.obj", "./Assets/models/", obj1.name)))
            //{
            //    streamWriter.Write(MeshToString(obj1.GetComponent<MeshFilter>(), new Vector3(-1f, 1f, 1f)));
            //    streamWriter.Close();
            //}
            

           // AssetDatabase.Refresh();
            //PrefabUtility.CreatePrefab(string.Format("{0}{1}.prefab", "./Assets/Prefab/", obj1.name),obj1);
           // PrefabUtility.SaveAsPrefabAsset(obj1, string.Format("{0}{1}.prefab", "./Assets/Prefab/", obj1.name));
        }
        AssetDatabase.Refresh();

    }
    static void splitAGameobjWithDistance(GameObject obj,float dst)
    {
        Debug.Log(obj.name);
        Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        Material[] materials = obj.GetComponent<MeshRenderer>().sharedMaterials;
        var allTriangles = mesh.triangles;
        int sz = mesh.vertices.Length;

        Vector3[] pos = mesh.vertices;
        Dictionary<int, List<int>> newTris = new Dictionary<int, List<int>>();
        union_set us = new union_set(sz);
        float minx = pos[0].x, maxx = pos[0].x, miny = pos[0].y, maxy = pos[0].y, minz = pos[0].z, maxz = pos[0].z;
        for (int i = 0; i < sz; i++)
        {
            Vector3 tmp = pos[i];
            minx = Mathf.Min(minx, tmp.x);
            maxx = Mathf.Max(maxx, tmp.x);
            miny = Mathf.Min(miny, tmp.y);
            maxy = Mathf.Max(maxy, tmp.y);
            minz = Mathf.Min(minz, tmp.z);
            maxz = Mathf.Max(maxz, tmp.z);
        }

        Debug.Log(maxx - minx);
        Debug.Log(maxy - miny);
        Debug.Log(maxz - minz);

        for (int i = 0; i < allTriangles.Length; i += 3)
            us.union(allTriangles[i], allTriangles[i + 1], allTriangles[i + 2]);

        for (int i = 0; i < sz; i++)
        {
            for (int j = 0; j < i; j++)
            {
                if (Vector3Equal(pos[i], pos[j], dst) == true)
                    us.union(i, j);

            }
        }

        for (int i = 0; i < allTriangles.Length; i += 3)
        {
            int f = us.findParent(allTriangles[i]);
            if (newTris.ContainsKey(f) == false)
                newTris.Add(f, new List<int>());
            newTris[f].Add(allTriangles[i]);
            newTris[f].Add(allTriangles[i + 1]);
            newTris[f].Add(allTriangles[i + 2]);
        }
        Debug.Log(newTris.Count);
        int cnt = 0;
        GameObject parent = new GameObject();
        parent.name = obj.name +"split";
        parent.transform.parent = obj.transform.parent.gameObject.transform;
        parent.transform.localPosition = obj.transform.localPosition;
        parent.transform.rotation = obj.transform.rotation;
        parent.transform.localScale = obj.transform.localScale;
        foreach (var t in newTris.Values)
        {
            Mesh mesh1 = new Mesh();

            Dictionary<int, int> dict = new Dictionary<int, int>();
            List<int> nnTris = new List<int>();
            List<Vector3> nVertex = new List<Vector3>();
            foreach (var i in t)
            {
                if (dict.ContainsKey(i) == false)
                {
                    dict[i] = nVertex.Count;
                    nVertex.Add(pos[i]);
                }
                nnTris.Add(dict[i]);
            }




            mesh1.vertices = nVertex.ToArray();
            //mesh1.uv = (Vector2[])mesh.uv.Clone();
            //mesh1.normals = (Vector3[])mesh.normals.Clone();
            mesh1.triangles = nnTris.ToArray();
            mesh1.RecalculateNormals();
            GameObject obj1 = new GameObject();
            obj1.name = obj.name + Convert.ToString(cnt++);
            obj1.AddComponent<MeshFilter>();
            obj1.GetComponent<MeshFilter>().sharedMesh = mesh1;
            obj1.AddComponent<MeshRenderer>();
            //for(int i=0;i<materials.Length;i++)
            //       materials[i]= Resources.Load<Material>("White");
            obj1.GetComponent<MeshRenderer>().material = Resources.Load<Material>("White");
            obj1.transform.parent = parent.transform;

            obj1.transform.localPosition = new Vector3(0, 0, 0);
            //obj1.transform.rotation = new Vector3(0,0,0);
            obj1.transform.rotation = new Quaternion(0, 0, 0, 0);
            obj1.transform.localScale = new Vector3(1, 1, 1);
            //using (StreamWriter streamWriter = new StreamWriter(string.Format("{0}{1}.obj", "./Assets/models/", obj1.name)))
            //{
            //    streamWriter.Write(MeshToString(obj1.GetComponent<MeshFilter>(), new Vector3(-1f, 1f, 1f)));
            //    streamWriter.Close();
            //}
           // AssetDatabase.Refresh();
           // obj1.GetComponent<MeshFilter>().mesh= AssetDatabase.LoadAssetAtPath<Mesh>(string.Format("{0}{1}.obj", "./Assets/models/", obj1.name));
            //PrefabUtility.CreatePrefab(string.Format("{0}{1}.prefab", "./Assets/Prefab/", obj1.name),obj1);
           // PrefabUtility.SaveAsPrefabAsset(obj1, string.Format("{0}{1}.prefab", "./Assets/Prefab/", obj1.name));
        }
  

     //   PrefabUtility.CreatePrefab(string.Format("{0}{1}.prefab", projectPath, this.meshGO.name), this.meshGO);


        AssetDatabase.Refresh();

       // AssetDatabase.Refresh();


    }



    private static string MeshToString(MeshFilter mf, Vector3 scale)
    {
        Mesh mesh = mf.mesh;
        Material[] sharedMaterials = mf.GetComponent<Renderer>().sharedMaterials;
        Vector2 textureOffset = mf.GetComponent<Renderer>().material.GetTextureOffset("_MainTex");
        Vector2 textureScale = mf.GetComponent<Renderer>().material.GetTextureScale("_MainTex");

        StringBuilder stringBuilder = new StringBuilder().Append("mtllib design.mtl")
            .Append("\n")
            .Append("g ")
            .Append(mf.name)
            .Append("\n");

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vector = vertices[i];
            stringBuilder.Append(string.Format("v {0} {1} {2}\n", vector.x * scale.x, vector.y * scale.y, vector.z * scale.z));
        }

        stringBuilder.Append("\n");

        Dictionary<int, int> dictionary = new Dictionary<int, int>();

        if (mesh.subMeshCount > 1)
        {
            int[] triangles = mesh.GetTriangles(1);

            for (int j = 0; j < triangles.Length; j += 3)
            {
                if (!dictionary.ContainsKey(triangles[j]))
                {
                    dictionary.Add(triangles[j], 1);
                }

                if (!dictionary.ContainsKey(triangles[j + 1]))
                {
                    dictionary.Add(triangles[j + 1], 1);
                }

                if (!dictionary.ContainsKey(triangles[j + 2]))
                {
                    dictionary.Add(triangles[j + 2], 1);
                }
            }
        }

        for (int num = 0; num != mesh.uv.Length; num++)
        {
            Vector2 vector2 = Vector2.Scale(mesh.uv[num], textureScale) + textureOffset;

            if (dictionary.ContainsKey(num))
            {
                stringBuilder.Append(string.Format("vt {0} {1}\n", mesh.uv[num].x, mesh.uv[num].y));
            }
            else
            {
                stringBuilder.Append(string.Format("vt {0} {1}\n", vector2.x, vector2.y));
            }
        }

        for (int k = 0; k < mesh.subMeshCount; k++)
        {
            stringBuilder.Append("\n");

            if (k == 0)
            {
                stringBuilder.Append("usemtl ").Append("Material_design").Append("\n");
            }

            if (k == 1)
            {
                stringBuilder.Append("usemtl ").Append("Material_logo").Append("\n");
            }

            int[] triangles2 = mesh.GetTriangles(k);

            for (int l = 0; l < triangles2.Length; l += 3)
            {
                stringBuilder.Append(string.Format("f {0}/{0} {1}/{1} {2}/{2}\n", triangles2[l] + 1, triangles2[l + 2] + 1, triangles2[l + 1] + 1));
            }
        }

        return stringBuilder.ToString();
    }


}

