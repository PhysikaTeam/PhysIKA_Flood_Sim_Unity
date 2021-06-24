using UnityEngine;
using ChartAndGraph;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
public class GraphChartSample: MonoBehaviour
{
	void Start ()
    {
        GraphChartBase graph = GetComponent<GraphChartBase>();
        horizontalAxisMap[0] = "00:00";
        horizontalAxisMap[1] = "";
        horizontalAxisMap[2] = "00:04";
        horizontalAxisMap[3] = "";
        horizontalAxisMap[4] = "00:08";
        horizontalAxisMap[5] = "";
        horizontalAxisMap[6] = "00:12";
        horizontalAxisMap[7] = "";
        horizontalAxisMap[8] = "00:16";
        horizontalAxisMap[9] = "";
        horizontalAxisMap[10] = "00:20";
        horizontalAxisMap[11] = "";

        horizontalAxisMap[12] = "00:24";
        horizontalAxisMap[13] = "";
        horizontalAxisMap[14] = "00:28";
        horizontalAxisMap[15] = "";
        horizontalAxisMap[16] = "00:32";
        horizontalAxisMap[17] = "";
        horizontalAxisMap[18] = "00:36";
        horizontalAxisMap[19] = "";
        horizontalAxisMap[20] = "00:40";

        horizontalAxisMap[21] = "";
        horizontalAxisMap[22] = "00:44";
        horizontalAxisMap[23] = "";
        horizontalAxisMap[24] = "00:48";
        horizontalAxisMap[25] = "";
        horizontalAxisMap[26] = "00:52";
        horizontalAxisMap[27] = "";
        horizontalAxisMap[28] = "00:56";
        horizontalAxisMap[29] = "";
        horizontalAxisMap[30] = "01:00";

        foreach (var k in horizontalAxisMap)
        {
            graph.HorizontalValueToStringMap[k.Key] = k.Value;
          
        }

        if (graph != null)
        {
            graph.DataSource.StartBatch();
           // graph.DataSource.ClearCategory("Player 1");
            graph.DataSource.ClearAndMakeBezierCurve("Building Depth");
            for (int i = 0; i <10; i++)
            {
                //graph.DataSource.AddPointToCategory("Player 1",Random.value*10f,Random.value*10f + 20f);
                if (i == 0)
                    graph.DataSource.SetCurveInitialPoint("Building Depth", 0f,UnityEngine.Random.value * 10f + 10f);
                else
                    graph.DataSource.AddLinearCurveToCategory("Building Depth", new DoubleVector2(i , Mathf.Abs(UnityEngine.Random.value) * 10f + 10f));
            }

            //graph.DataSource.MakeCurveCategorySmooth("Building Depth");
            graph.DataSource.EndBatch();
        }
    }

    Dictionary<double, string> horizontalAxisMap=new Dictionary<double, string>() ;
    public void changeData(List<float>newData,string title)
    {
      
        GraphChartBase graph = GetComponent<GraphChartBase>();
      
        graph.DataSource.StartBatch();
        graph.DataSource.ClearAndMakeBezierCurve("Building Depth");

        int len = newData.Count;
        for(int i=0;i<len;i++)
        {
            /*if (i == 12)
                newData[i] += 0.01f;*/
            if (i == 0)
                graph.DataSource.SetCurveInitialPoint("Building Depth", i, newData[i]);
            else
                graph.DataSource.AddLinearCurveToCategory("Building Depth", new DoubleVector2(i, newData[i]));
        }
        //graph.DataSource.MakeCurveCategorySmooth("Building Depth");
        graph.DataSource.EndBatch();
    }

    private void Update()
    {
       
    }


}
