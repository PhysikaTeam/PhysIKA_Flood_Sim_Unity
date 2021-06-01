using ChartAndGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GraphVis : MonoBehaviour
{
    // Start is called before the first frame update
    public GraphChart chart;
    void Start()
    {

        chart.DataSource.StartBatch();
        chart.DataSource.ClearCategory("depth");

        chart.DataSource.AddPointToCategory("depth", 0, 0.0017);

        chart.DataSource.AddPointToCategory("depth", 1, 0.002);
 
        chart.DataSource.AddPointToCategory("depth", 2, 0.004);

        chart.DataSource.AddPointToCategory("depth", 3, 0.0056);

        chart.DataSource.AddPointToCategory("depth",4, 0.007);

        chart.DataSource.AddPointToCategory("depth", 5, 0.0085);


 
        chart.DataSource.EndBatch();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
