using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceMat : MonoBehaviour
{
    public Terrain terr;
    public PhysicMaterial cellMat;
    public PhysicMaterial roadNet;
    private int cnt = 0;
    // Start is called before the first frame update
    void Start()
    {
        cellMat = Resources.Load("resTest") as PhysicMaterial;
        roadNet = Resources.Load("roadNet") as PhysicMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            cnt++;
            if(cnt % 2 == 1)
            {
                
                terr.GetComponent<TerrainCollider>().material = cellMat;
            }
            else
            {
                terr.GetComponent<TerrainCollider>().material = roadNet;
            }
        }
    }
}
