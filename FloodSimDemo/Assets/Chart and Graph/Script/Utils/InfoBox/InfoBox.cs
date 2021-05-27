using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace ChartAndGraph
{
    /// <summary>
    /// this class demonstrates the use of chart events
    /// </summary>
    public class InfoBox : MonoBehaviour
    {
        public PieChart[] PieChart;
        public BarChart[] BarChart;
        public GraphChartBase[] GraphChart;
        public RadarChart[] RadarChart;
        public CandleChart[] CandleChart;
        public Text infoText; 
         
        void BarHovered(BarChart.BarEventArgs args)
        {
            
            infoText.text = string.Format("({0},{1}) : {2}", args.Category, args.Group, args.Value);
        }

        void RadarHovered(RadarChart.RadarEventArgs args)
        {
            infoText.text = string.Format("{0},{1} : {2}", args.Category, args.Group, ChartAdancedSettings.Instance.FormatFractionDigits(2, args.Value));
        }
        void CandleClicked(CandleChart.CandleEventArgs args)
        {
            if(args.IsBodyEvent)
                infoText.text = string.Format("{0} : Candle Body Clicked , O:{1},C:{2}", args.Category, args.CandleValue.Open, args.CandleValue.Close);
            if (args.IsHighEvent)
                infoText.text = string.Format("{0} : Candle High Clicked , H:{1}", args.Category, args.CandleValue.High);
            if(args.IsLowEvent)
                infoText.text = string.Format("{0} : Candle Low Clicked , L:{1}", args.Category, args.CandleValue.Low);
        }
        void CandleHovered(CandleChart.CandleEventArgs args)
        {
            if (args.IsBodyEvent)
                infoText.text = string.Format("{0} : Candle Body  , O:{1},C:{2}", args.Category, args.CandleValue.Open, args.CandleValue.Close);
            if (args.IsHighEvent)
                infoText.text = string.Format("{0} : Candle High  , H:{1}", args.Category, args.CandleValue.High);
            if (args.IsLowEvent)
                infoText.text = string.Format("{0} : Candle Low , L:{1}", args.Category, args.CandleValue.Low);
        }

        void GraphClicked(GraphChartBase.GraphEventArgs args)
        {
            if (args.Magnitude < 0f)
                infoText.text = string.Format("{0} : {1},{2} Clicked", args.Category, args.XString, args.YString);
            else
                infoText.text = string.Format("{0} : {1},{2} : Sample Size {3} Clicked", args.Category, args.XString, args.YString, args.Magnitude);
        }

        void GraphHoverd(GraphChartBase.GraphEventArgs args)
        {
            if (args.Magnitude < 0f)
                infoText.text = string.Format("{0} : {1},{2}", args.Category, args.XString, args.YString);
            else
                infoText.text = string.Format("{0} : {1},{2} : Sample Size {3}", args.Category, args.XString, args.YString, args.Magnitude);
        }

        void PieHovered(PieChart.PieEventArgs args)
        {
            infoText.text = string.Format("{0} : {1}", args.Category, args.Value);
        }

        void NonHovered()
        {
            infoText.text = "";
        }

        public void HookChartEvents()
        {
            if (PieChart != null)
            {
                foreach (PieChart pie in PieChart)
                {
                    if (pie == null)
                        continue;
                    pie.PieHovered.AddListener(PieHovered);        // add listeners for the pie chart events
                    pie.NonHovered.AddListener(NonHovered);
                }
            }

            if (BarChart != null)
            {
                foreach (BarChart bar in BarChart)
                {
                    if (bar == null)
                        continue;
                    bar.BarHovered.AddListener(BarHovered);        // add listeners for the bar chart events
                    bar.NonHovered.AddListener(NonHovered);
                }
            }

            if(GraphChart  != null)
            {
                foreach(GraphChartBase graph in GraphChart)
                {
                    if (graph == null)
                        continue;
                    graph.PointClicked.AddListener(GraphClicked);
                    graph.PointHovered.AddListener(GraphHoverd);
                    graph.NonHovered.AddListener(NonHovered);
                }
            }
            if(CandleChart != null)
            {
                foreach(CandleChart candle in CandleChart)
                {
                    if (candle == null)
                        return;
                    candle.CandleHovered.AddListener(CandleHovered);
                    candle.CandleClicked.AddListener(CandleClicked);
                    candle.NonHovered.AddListener(NonHovered);
                }
            }
            if (RadarChart != null) 
            {
                foreach (RadarChart radar in RadarChart)
                {
                    if (radar == null)
                        continue;
                    radar.PointHovered.AddListener(RadarHovered);
                    radar.NonHovered.AddListener(NonHovered);
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            HookChartEvents();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}