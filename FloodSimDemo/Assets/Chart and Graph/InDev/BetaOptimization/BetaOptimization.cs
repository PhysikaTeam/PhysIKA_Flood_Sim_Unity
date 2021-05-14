using UnityEngine;
using System.Collections;
using ChartAndGraph;

public class BetaOptimization : MonoBehaviour
{

    public GraphChart Graph;
    public int TotalPoints = 5;
    float lastTime = 0f;
    float lastX = 0f;
    int count = 0;
    public float PointInterval = 0.1f;
    void Start()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;
        float x = 3f * TotalPoints;
        Graph.DataSource.StartBatch(); // calling StartBatch allows changing the graph data without redrawing the graph for every change
        Graph.DataSource.ClearCategory("Player 1"); // clear the "Player 1" category. this category is defined using the GraphChart inspector
        Graph.DataSource.ClearCategory("Player 2"); // clear the "Player 2" category. this category is defined using the GraphChart inspector
        lastX = 0;
        for (int i = 0; i < TotalPoints; i++)  //add random points to the graph
        {
            Graph.DataSource.AddPointToCategory("Player 1", lastX, Random.value * 20f + 10f); // each time we call AddPointToCategory 
            Graph.DataSource.AddPointToCategory("Player 2", lastX, Random.value * 10f); // each time we call AddPointToCategory 
            x -= Random.value * 3f;
            lastX++;
            count+=2;
        }

        Graph.DataSource.EndBatch(); // finally we call EndBatch , this will cause the GraphChart to redraw itself
    }

    void Update()
    {
        float time = Time.time;
        if (lastTime + PointInterval < time)
        {
            lastTime = time;
            lastX += Random.value * 3f;
            //            System.DateTime t = ChartDateUtility.ValueToDate(lastX);
            Graph.DataSource.AddPointToCategoryRealtime("Player 1", lastX, Random.value * 20f + 10f, PointInterval); // each time we call AddPointToCategory 
            Graph.DataSource.AddPointToCategoryRealtime("Player 2", lastX, Random.value * 10f, PointInterval); // each time we call AddPointToCategory
            count += 2;
            if (count % 100 == 0)
                Debug.Log("Point count is :" + count);
        }

    }
}
