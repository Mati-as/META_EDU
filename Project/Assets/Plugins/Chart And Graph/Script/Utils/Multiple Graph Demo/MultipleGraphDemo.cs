#define Graph_And_Chart_PRO
using UnityEngine;
using System.Collections;
using ChartAndGraph;
using System.Collections.Generic;
using System;

public class MultipleGraphDemo : MonoBehaviour
{
    public GraphChart Graph;
    //  public GraphAnimation Animation;
    public int TotalPoints = 5;

    private float x_1;
    private float x_2;

    private float y_1;
    private float y_2;

    void Start()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;

        List<DoubleVector2> animationPoints = new List<DoubleVector2>();
        x_1 = 1;
        x_2 = 1;
        Graph.HorizontalValueToStringMap.Add(10, "Ten");
        Graph.VerticalValueToStringMap.Add(100, "$$");
        Graph.DataSource.StartBatch(); // calling StartBatch allows changing the graph data without redrawing the graph for every change
        Graph.DataSource.ClearCategory("SW data"); // clear the "Player 2" category. this category is defined using the GraphChart inspector

        //for (int i = 0; i < TotalPoints; i++)  //add random points to the graph
        //{
        //    Graph.DataSource.AddPointToCategory("OX data", 0, 0); // each time we call AddPointToCategory 
        //    Graph.DataSource.AddPointToCategory("SW data", 0, 0); // each time we call AddPointToCategory 

        //    Graph.DataSource.AddPointToCategory("OX data", 5, 7); // each time we call AddPointToCategory 
        //    Graph.DataSource.AddPointToCategory("SW data", 5, 3); // each time we call AddPointToCategory 

        //    Graph.DataSource.AddPointToCategory("OX data", 10, 2); // each time we call AddPointToCategory 
        //    Graph.DataSource.AddPointToCategory("SW data", 10, 9); // each time we call AddPointToCategory 



        //    //Graph.DataSource.AddPointToCategory("OX data", x, y+10); // each time we call AddPointToCategory 
        //    //Graph.DataSource.AddPointToCategory("SW data", x, y); // each time we call AddPointToCategory 
        //    //Graph.DataSource.AddPointToCategory("Player 2", System.DateTime.Now + System.TimeSpan.FromDays(x), Random.value * 20f + 10f); // each time we call AddPointToCategory 
        //    //animationPoints.Add(new DoubleVector2(ChartDateUtility.DateToValue(System.DateTime.Now + System.TimeSpan.FromDays(x)), Random.value * 10f));
        //    x += 1; 
        //    y += 50;
        //}

        Graph.DataSource.EndBatch(); // finally we call EndBatch , this will cause the GraphChart to redraw itself
                                     //if (Animation != null)
                                     //{
                                     //if (Graph.DataSource.HasCategory("Player 2"))
                                     //    Animation.Animate("Player 2", animationPoints, 6f);
                                     //}
    }

    public void Add_Data(Stack<string> result_1, Stack<string> result_2)
    {
        x_1 = 0;
        x_2 = 0;
        Graph.DataSource.StartBatch();
        Graph.DataSource.ClearCategory("SW data");
        Graph.DataSource.ClearCategory("OX data");

        //Debug.Log("Number of Each data" + result_1.Count + result_2.Count);
        int num_stack= result_1.Count;

        for (int i = 0; i < num_stack; i++)
        {
            //Debug.Log("데이터 add for문 갯수 " + i);
            y_1 = Convert.ToInt32(result_1.Pop());
            y_2 = Convert.ToInt32(result_2.Pop());

            //Debug.Log("Number of Each data" + y_1 + y_2);
            Graph.DataSource.AddPointToCategory("OX data", x_1, y_1);
            Graph.DataSource.AddPointToCategory("SW data", x_2, y_2);
            x_1 += 2.5f;
            x_2 += 2.5f;
        }
        Graph.DataSource.EndBatch();
    }
    //public void Add_Data_1()
    //{
    //    Graph.DataSource.StartBatch(); // calling StartBatch allows changing the graph data without redrawing the graph for every change
    //    Graph.DataSource.ClearCategory("SW data"); // clear the "Player 2" category. this category is defined using the GraphChart inspector
    //    Graph.DataSource.ClearCategory("OX data"); // clear the "Player 2" category. this category is defined using the GraphChart inspector

    //    Graph.DataSource.AddPointToCategory("OX data", 1, 0); // each time we call AddPointToCategory 
    //    Graph.DataSource.AddPointToCategory("SW data", 1, 50); // each time we call AddPointToCategory 

    //    Graph.DataSource.AddPointToCategory("OX data", 4.5, 20); // each time we call AddPointToCategory 
    //    Graph.DataSource.AddPointToCategory("SW data", 4.5, 10); // each time we call AddPointToCategory 

    //    Graph.DataSource.AddPointToCategory("OX data", 9, 35); // each time we call AddPointToCategory 
    //    Graph.DataSource.AddPointToCategory("SW data", 9, 100); // each time we call AddPointToCategory 

    //    Graph.DataSource.EndBatch(); // finally we call EndBatch , this will cause the GraphChart to redraw itself
    //}
}
