using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Assets.Scripts;
namespace operations
{


    public class interactiveWindow : EditorWindow
    {
        //static public CustomTerrain ct=null;
        public bool foldout1 = false, foldout0 = false, foldout2 = false;
        //public static int width = 4096, height = 4096;
        //public static float riverHeight;
        //public static int earlyWarning;
        private bool displayBuilding, displayTerrain, displayWater,displayMap;

        bool saveStates = false;

        [MenuItem("Tools/interactiveWindow")]
        public static void OpenWindow()
        {
            EditorWindow.GetWindow<interactiveWindow>();
        }

        private void OnGUI()
        {
            
            GUIStyle style = new GUIStyle
            {
                fontSize = 50,
                fontStyle = FontStyle.BoldAndItalic,
            };

            //var inputModes = new[] { "Add water", "Remove water", "Add Terrain", "Remove terrain" };
            //var inputModes = new[] { "抬高水位", "Remove water", "Add Terrain", "Remove terrain", "Add Rect Building", "Add Cyclinder Building","点击建筑物" ,"Add Rect Protection" ,"AddChushuikou",
            //"AddJinshuikou"};

            //var inputModes = new[] { "抬高水位", "降低水位", "修建挡板", "取消挡板" ,"添加排水口","添加进水口"};

            var inputModes = new[] {  "修建挡板", "添加排水口", "添加进水口","获取建筑物水位图","加水","加水2" };
            //GUILayout.BeginArea(new Rect(10, 10, 1000, 1000));
            GUILayout.BeginVertical();

            GUILayout.Space(30);

            foldout0 = EditorGUILayout.Foldout(foldout0, "初始化操作");
            if (foldout0)
            {
                GUILayout.Space(30);
                GUILayout.BeginHorizontal();
                GUILayout.Label("模拟网格分辨率");
                CustomTerrain.width = EditorGUILayout.IntField(CustomTerrain.width);
                CustomTerrain.height = EditorGUILayout.IntField(CustomTerrain.height);
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(30);

                GUILayout.BeginHorizontal();
                GUILayout.Label("河流水位(m)");
                //CustomTerrain.riverHeight = GUILayout.HorizontalSlider(CustomTerrain.riverHeight, 1f, 100f);
                GUILayout.Space(40);
                CustomTerrain.riverHeight = EditorGUILayout.FloatField(CustomTerrain.riverHeight);
                GUILayout.Space(30);
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(30);

                GUILayout.BeginHorizontal();
                GUILayout.Label("河流水速(m/s)");
                GUILayout.Space(30);
                //CustomTerrain.riverSpeed = GUILayout.HorizontalSlider(CustomTerrain.riverSpeed, 1f, 10f);
                CustomTerrain.riverSpeed = EditorGUILayout.FloatField(CustomTerrain.riverSpeed);
                GUILayout.Space(30);
                //GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(30);
                //GUILayout.BeginHorizontal();
                //GUILayout.Label("建筑物预警");
                //string[] tmp = { "开启", "关闭" };
                //CustomTerrain.earlyWarning = GUILayout.Toolbar(CustomTerrain.earlyWarning, tmp);
                //GUILayout.EndHorizontal();
                GUIStyle style1 = new GUIStyle
                {
                   // fontSize = 50,
                    fontStyle = FontStyle.BoldAndItalic,
                    //fixedWidth = 50
                };
                GUILayout.BeginHorizontal();
                GUILayout.Space(120);
                if (GUILayout.Button("开始模拟") && CustomTerrain.init == false)
                {
                    CustomTerrain.init = true;
                }
                GUILayout.Space(120);
                GUILayout.EndHorizontal();

            }

            GUILayout.Space(30);
            foldout1 = EditorGUILayout.Foldout(foldout1, "交互操作");
            if (foldout1)
            {
                GUILayout.BeginHorizontal();
                CustomTerrain.InputMode = (InputModes)GUILayout.Toolbar((int)CustomTerrain.InputMode, inputModes);
                GUILayout.EndHorizontal();

               // GUILayout.BeginHorizontal();
                if (CustomTerrain.InputMode == InputModes.AddTerrain)
                {
                    CustomTerrain._brushRadius = 0;

                    GUILayout.Space(30);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    CustomTerrain.barrirHei = EditorGUILayout.FloatField("挡板高度(m)",CustomTerrain.barrirHei);
                    GUILayout.Space(40);
                    
                    GUILayout.EndHorizontal();

                    GUILayout.Space(30);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    CustomTerrain.barrirWid = EditorGUILayout.FloatField("挡板宽度(m)", CustomTerrain.barrirWid);
                    GUILayout.Space(40);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(30);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    if (GUILayout.Button("开始建造"))
                    {
                        uiDrawLines.isDrawing = true;
                    }
                    GUILayout.Space(30);
                    if (GUILayout.Button("完成建造"))
                    {
                        uiDrawLines.isDrawing = false;
                        CustomTerrain.buildBarrier1= CustomTerrain.buildBarrier2 = true;
                    }
                    GUILayout.Space(40);
                    GUILayout.EndHorizontal();
                    Camera.main.GetComponent<uiDrawLines>().drawOpen = true;
                }
                else
                    //Camera.main.GetComponent<uiDrawLines>().drawOpen = false;

                // GUILayout.EndHorizontal();



                //if(CustomTerrain.InputMode == InputModes.AddWater|| CustomTerrain.InputMode == InputModes.RemoveWater||CustomTerrain.InputMode == InputModes.AddJinshuikou|| CustomTerrain.InputMode == InputModes.AddPaishuikou)
                if (CustomTerrain.InputMode == InputModes.AddJinshuikou || CustomTerrain.InputMode == InputModes.AddPaishuikou|| CustomTerrain.InputMode == InputModes.AddWater)
                {
                    GUILayout.Space(30);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUILayout.Label("半径圆尺寸(m)");
                    GUILayout.Space(20);
                    //CustomTerrain._brushRadius = GUILayout.HorizontalSlider(CustomTerrain._brushRadius, 0f, 80f);
                    GUILayout.Space(20);
                    CustomTerrain._brushRadius = EditorGUILayout.FloatField(CustomTerrain._brushRadius);
                    GUILayout.Space(40);
                    GUILayout.EndHorizontal();

                    GUILayout.Space(30);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUILayout.Label("水量数值(m3)");
                    GUILayout.Space(20);
                    //GUIStyle s1 = new GUIStyle { fixedWidth = 100 };
                    //CustomTerrain.BrushAmount = GUILayout.HorizontalSlider(CustomTerrain.BrushAmount, 0f, 10f);
                    GUILayout.Space(20);
                    CustomTerrain.BrushAmount = EditorGUILayout.FloatField(CustomTerrain.BrushAmount);
                    GUILayout.Space(40);
                    GUILayout.EndHorizontal();
                }
                
            }

            GUILayout.Space(30);
            foldout2 = EditorGUILayout.Foldout(foldout2, "绘制设置");
            if (foldout2)
            {
                //GUILayout.BeginHorizontal();

                // GUILayout.Label("显示建筑物");
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                CustomTerrain.earlyWarning = GUILayout.Toggle(CustomTerrain.earlyWarning, "          显示建筑物预警等级");
                GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //GUILayout.Space(30);
                //CustomTerrain.displayWater = GUILayout.Toggle(CustomTerrain.displayWater, "          显示洪水");
                //GUILayout.EndHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                CustomTerrain.displayWaterDepth = GUILayout.Toggle(CustomTerrain.displayWaterDepth, "          显示洪水深度伪彩图");
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                CustomTerrain.displayMap = GUILayout.Toggle(CustomTerrain.displayMap, "          显示地图");
                GUILayout.EndHorizontal();

                


                GUILayout.Space(30);
                //CustomTerrain.saveStates =GUILayout.Toggle(CustomTerrain.saveStates, "          保存过往洪水状态");
                //if (GUILayout.Button("保存当前状态") )
                //{
                //    CustomTerrain.saveButtonClick = true;
                //}


                //GUILayout.BeginHorizontal();
                //GUILayout.Label("时间戳");
                //CustomTerrain.timeIndex = (int)GUILayout.HorizontalSlider(CustomTerrain.timeIndex, 0, CustomTerrain.states.Count-1);
                //if (GUILayout.Button("回溯"))
                //{
                //    CustomTerrain.backwardButtonIsClick = true;
                //    //CustomTerrain.init = true;
                //}
                ////CustomTerrain._brushRadius = EditorGUILayout.FloatField(CustomTerrain._brushRadius);
                //GUILayout.EndHorizontal();
                //GUILayout.BeginHorizontal();
                //GUILayout.Label("数值");
                //CustomTerrain.BrushAmount = GUILayout.HorizontalSlider(CustomTerrain.BrushAmount, 1f, 10f);
                //CustomTerrain.BrushAmount = EditorGUILayout.FloatField(CustomTerrain.BrushAmount);
                //GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("尺寸");
                //CustomTerrain._brushRadius = GUILayout.HorizontalSlider(CustomTerrain._brushRadius, 0.001f * 4000, 0.02f * 4000);
                //CustomTerrain._brushRadius = EditorGUILayout.FloatField(CustomTerrain._brushRadius);
                //GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();


            

            // GUILayout.EndArea();
        }
    }
}
