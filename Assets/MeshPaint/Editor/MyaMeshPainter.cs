using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MyaMeshPainter : EditorWindow
{

    [MenuItem("MyaTools/MyaMeshPainter")]
    static void Init()
    {
        var window = GetWindow<MyaMeshPainter>("Mya Mesh Painter");
        window.Show();
    }
    string contolTexName = "";

    Texture[] brushTex;
    int selBrush = 0;
    Texture[] texLayer;
    int selTex = 0;


    bool isPaint;
    float brushSize = 16f;
    float brushStronger = 0.5f;

    int tab = 0;

    void OnGUI()
    {
        EditorGUILayout.PrefixLabel("Mya Mesh Painter ");
        tab = GUILayout.Toolbar(tab, new string[] { "Painter", "Option", "Info" });
        switch (tab)
        {
            case 0:
                DrawPainterTeb();
                break;
            case 1:
                break;

            case 2:
                break;
            default:
                DrawPainterTeb();
                break;
        }


    }

    void DrawPainterTeb()
    {

        GUIStyle boolBtnOn = new GUIStyle(GUI.skin.GetStyle("Button"));//得到Button样式
        GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            isPaint = GUILayout.Toggle(isPaint, EditorGUIUtility.IconContent("EditCollider"), boolBtnOn, GUILayout.Width(35), GUILayout.Height(25));//编辑模式开关
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();

        brushSize = (int)EditorGUILayout.Slider("Brush Size", brushSize, 1, 36);//笔刷大小
        brushStronger = EditorGUILayout.Slider("Brush Stronger", brushStronger, 0, 1f);//笔刷强度


        layerTex();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal("box", GUILayout.Width(340));
        selTex = GUILayout.SelectionGrid(selTex, texLayer, 4, "gridlist", GUILayout.Width(340), GUILayout.Height(86));
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        IniBrush();


        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal("box", GUILayout.Width(318));
        selBrush = GUILayout.SelectionGrid(selBrush, brushTex, 9, "gridlist", GUILayout.Width(340), GUILayout.Height(70));
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }


    //获取材质球中的贴图
    void layerTex()
    {
        Transform Select = Selection.activeTransform;
        texLayer = new Texture[4];
        texLayer[0] = new Texture2D(85,85);// AssetPreview.GetAssetPreview(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat0")) as Texture;
        texLayer[1] = new Texture2D(85, 85);//ssetPreview.GetAssetPreview(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat1")) as Texture;
        texLayer[2] = new Texture2D(85, 85);//ssetPreview.GetAssetPreview(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat2")) as Texture;
        texLayer[3] = new Texture2D(85, 85);//AssetPreview.GetAssetPreview(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat3")) as Texture;
    }
    //获取笔刷  
    void IniBrush()
    {
        string MeshPaintEditorFolder = "Assets/MeshPaint/Editor/";
        ArrayList BrushList = new ArrayList();
        Texture BrushesTL;
        int BrushNum = 0;
        do
        {
            BrushesTL = (Texture)AssetDatabase.LoadAssetAtPath(MeshPaintEditorFolder + "Brushes/Brush" + BrushNum + ".png", typeof(Texture));

            if (BrushesTL)
            {
                BrushList.Add(BrushesTL);
            }
            BrushNum++;
        } while (BrushesTL);
        brushTex = BrushList.ToArray(typeof(Texture)) as Texture[];
    }
}
