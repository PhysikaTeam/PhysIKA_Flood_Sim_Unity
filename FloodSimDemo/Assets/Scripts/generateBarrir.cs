using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateBarrir : MonoBehaviour
{
    // Start is called before the first frame update

    //顶点数组
    Vector3[] _vertices =
    {
            // front
            new Vector3(-5.0f, 10.0f, -5.0f),
            new Vector3(-5.0f, 0.0f, -5.0f),
            new Vector3(5.0f, 0.0f, -5.0f),
            new Vector3(5.0f, 10.0f, -5.0f),


            // left  
            new Vector3(-5.0f, 10.0f, -5.0f),
            new Vector3(-5.0f, 0.0f, -5.0f),
            new Vector3(-5.0f, 0.0f, 5.0f),//
            new Vector3(-5.0f, 10.0f, 5.0f),

            // back
            new Vector3(-5.0f, 10.0f, 5.0f),
            new Vector3(-5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 10.0f, 5.0f),


            // right  
            new Vector3(5.0f, 10.0f, 5.0f),
            new Vector3(5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 0.0f, -5.0f),
            new Vector3(5.0f, 10.0f, -5.0f),


            // Top
            new Vector3(-5.0f, 10.0f, 5.0f),
            new Vector3(5.0f, 10.0f, 5.0f),
            new Vector3(5.0f, 10.0f, -5.0f),
            new Vector3(-5.0f, 10.0f, -5.0f),

           // Bottom
            new Vector3(-5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 0.0f, 5.0f),
            new Vector3(5.0f, 0.0f, -5.0f),
            new Vector3(-5.0f, 0.0f, -5.0f),

        };

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
