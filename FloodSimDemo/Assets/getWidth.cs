using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getWidth : MonoBehaviour
{
    // Start is called before the first frame update
    float size = 0;
    bool flag = false;
    void Start()
    {
        Terrain terrain = gameObject.GetComponent<Terrain>();
        size = Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(flag == false)
        {
            Debug.Log("Terrain Size = " + size);
            flag = true;
        }
    }
}
