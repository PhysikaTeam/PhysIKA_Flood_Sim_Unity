using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(2))
            ;
    }

    void getHeight(Vector3 terrPos,float sizeX,float scaleX,float sizeZ,float scaleZ)//起始点位置、x方向的尺寸、z方向的尺寸
    {
        //float sizeX = myTerrain.GetComponent<TerrainCollider>().bounds.size.x;
        //float scaleX = myTerrain.transform.localScale.x;

        //float sizeZ = myTerrain.GetComponent<TerrainCollider>().bounds.size.z;
        //float scaleZ = myTerrain.transform.localScale.z;

        Debug.Log("the X size = " + sizeX);
        Debug.Log("the X scale = " + scaleX);
        Debug.Log("the Z size = " + sizeZ);
        Debug.Log("the Z scale = " + scaleZ);

        //Vector3 terrPos = myTerrain.GetPosition();

        Debug.Log("the position: " + terrPos);
        Vector3 tmpPos;
        for (int i = 0; i < sizeX; i++)
        {
            tmpPos = terrPos;
            tmpPos.y = -1; //取地表平面之下的点
            tmpPos.x += i * scaleX;
            for (int j = 0; j < sizeZ; j++)
            {
                tmpPos.z += j * scaleZ;
                Vector3 origin = new Vector3(tmpPos.x, 10000, tmpPos.z);
                Vector3 direction = tmpPos - origin;
                Ray ray = new Ray(origin, direction);
                RaycastHit hit;
                bool isCollider = Physics.Raycast(ray, out hit, Mathf.Infinity);
                if (isCollider)
                {
                    if (hit.point.y != 0)
                    {
                        print("碰撞对象: " + hit.collider.name + " 碰撞点: " + hit.point);
                    }
                }
                else
                {
                    print("what the fuck?");
                }
            }
        }

    }
}
