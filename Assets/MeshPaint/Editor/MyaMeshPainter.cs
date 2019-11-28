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
        SceneView.onSceneGUIDelegate += window.OnSceneGUI;
        window.Show();
    }
    string contolTexName = "";

    Texture[] brushTex;
    int selBrush = 0;
    Texture[] texLayer;
    int selTex = 0;

    GameObject editorGobj = null;
    bool isPaint;
    float brushSize = 16f;
    float brushStronger = 0.5f;

    int tab = 0;

    Texture2D MaskTex;
    string MaskPath = string.Empty;
    int brushSizeInPourcent;

    LayerMask groudLayer;
    void OnFocus()
    {
        //SceneView.onSceneGUIDelegate -= OnSceneGUI;
        //SceneView.onSceneGUIDelegate += OnSceneGUI;
        //Repaint();
    }
    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }
    bool ToggleF = false;

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
            EditorGUI.BeginChangeCheck();
            isPaint = GUILayout.Toggle(isPaint, EditorGUIUtility.IconContent("EditCollider"), boolBtnOn, GUILayout.Width(35), GUILayout.Height(25));//编辑模式开关
            if (EditorGUI.EndChangeCheck())
            {
                if (isPaint && editorGobj == null && Selection.activeGameObject != null)
                {
                    editorGobj = Selection.activeGameObject;
                }
                if (isPaint)
                {
                    endPainting = true;
                }
                else
                {
                    if (endPainting)
                    {

                        if (editorGobj != null && MaskPath != string.Empty)
                        {
                            SaveTexture();
                            editorGobj.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Control", (Texture2D)AssetDatabase.LoadAssetAtPath(GetAssetPath(MaskPath), typeof(Texture2D)));
                            MaskTex = null;
                            Debug.Log("Change Tex");
                        }
                    }
                    endPainting = false;
                }
            }

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

        groudLayer = EditorGUILayout.LayerField(groudLayer);
    }


    //获取材质球中的贴图
    void layerTex()
    {
        Transform Select = Selection.activeTransform;
        texLayer = new Texture[4];
        texLayer[0] = editorGobj == null? new Texture2D(85, 85) : AssetPreview.GetAssetPreview(editorGobj.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat0")) as Texture;
        texLayer[1] = editorGobj == null? new Texture2D(85, 85) : AssetPreview.GetAssetPreview(editorGobj.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat1")) as Texture;
        texLayer[2] = editorGobj == null? new Texture2D(85, 85) : AssetPreview.GetAssetPreview(editorGobj.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat2")) as Texture;
        texLayer[3] = editorGobj == null? new Texture2D(85, 85) : AssetPreview.GetAssetPreview(editorGobj.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat3")) as Texture;
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

    bool endPainting = false; 
    void OnSceneGUI(SceneView sceneView)
    {

        if (isPaint)
        {
            Painter();
        }


        //else
        //{
        //    endPainting = true;
        //}
        //if (endPainting)
        //{
        //    SaveTexture();
        //    if (editorGobj != null && MaskPath != string.Empty)
        //    {
        //        editorGobj.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Control", (Texture2D)AssetDatabase.LoadAssetAtPath(GetAssetPath(MaskPath), typeof(Texture2D)));
        //        Debug.Log("Change Tex");
        //    }
        //
        //    //MaskTex = null;
        //
        //    endPainting = false;
        //}
        //sceneView.Repaint();
    }


    void Painter()
    {
        if(editorGobj == null)
        {
            editorGobj = Selection.activeGameObject;
        }
        if (editorGobj == null)
            return;
        Transform CurrentSelect = editorGobj.transform;
        MeshFilter temp = CurrentSelect.GetComponent<MeshFilter>();//获取当前模型的MeshFilter
        float orthographicSize = (brushSize * CurrentSelect.localScale.x) * (temp.sharedMesh.bounds.size.x / 200);//笔刷在模型上的正交大小
        if(MaskTex == null)
        {
            MaskTex = UnlockTexture((Texture2D)CurrentSelect.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Control"));//从材质球中获取Control贴图
            CurrentSelect.gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Control" , MaskTex);
        }

        brushSizeInPourcent = (int)Mathf.Round((brushSize * MaskTex.width) / 100);//笔刷在模型上的大小
        
        Event e = Event.current;//检测输入
        HandleUtility.AddDefaultControl(0);
        RaycastHit raycastHit = new RaycastHit();
        Ray terrain = HandleUtility.GUIPointToWorldRay(e.mousePosition);//从鼠标位置发射一条射线
        if (Physics.Raycast(terrain, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("ground")))//射线检测名为"ground"的层
        {
            Handles.color = new Color(1f, 1f, 0f, 1f);//颜色
            Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, orthographicSize);//根据笔刷大小在鼠标位置显示一个圆

            //鼠标点击或按下并拖动进行绘制
            if ((e.type == EventType.MouseDrag && e.alt == false && e.control == false && e.shift == false && e.button == 0) || (e.type == EventType.MouseDown && e.shift == false && e.alt == false && e.control == false && e.button == 0 && ToggleF == false))
            {
                //选择绘制的通道
                Color targetColor = new Color(1f, 0f, 0f, 0f);
                switch (selTex)
                {
                    case 0:
                        targetColor = new Color(1f, 0f, 0f, 0f);
                        break;
                    case 1:
                        targetColor = new Color(0f, 1f, 0f, 0f);
                        break;
                    case 2:
                        targetColor = new Color(0f, 0f, 1f, 0f);
                        break;
                    case 3:
                        targetColor = new Color(0f, 0f, 0f, 1f);
                        break;

                }

                Vector2 pixelUV = raycastHit.textureCoord;

                //计算笔刷所覆盖的区域
                int PuX = Mathf.FloorToInt(pixelUV.x * MaskTex.width);
                int PuY = Mathf.FloorToInt(pixelUV.y * MaskTex.height);
                int x = Mathf.Clamp(PuX - brushSizeInPourcent / 2, 0, MaskTex.width - 1);
                int y = Mathf.Clamp(PuY - brushSizeInPourcent / 2, 0, MaskTex.height - 1);
                int width = Mathf.Clamp((PuX + brushSizeInPourcent / 2), 0, MaskTex.width) - x;
                int height = Mathf.Clamp((PuY + brushSizeInPourcent / 2), 0, MaskTex.height) - y;

                Color[] terrainBay = MaskTex.GetPixels(x, y, width, height, 0);//获取Control贴图被笔刷所覆盖的区域的颜色

                Texture2D TBrush = brushTex[selBrush] as Texture2D;//获取笔刷性状贴图
                float[] brushAlpha = new float[brushSizeInPourcent * brushSizeInPourcent];//笔刷透明度

                //根据笔刷贴图计算笔刷的透明度
                for (int i = 0; i < brushSizeInPourcent; i++)
                {
                    for (int j = 0; j < brushSizeInPourcent; j++)
                    {
                        brushAlpha[j * brushSizeInPourcent + i] = TBrush.GetPixelBilinear(((float)i) / brushSizeInPourcent, ((float)j) / brushSizeInPourcent).a;
                    }
                }

                //计算绘制后的颜色
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        int index = (i * width) + j;
                        float Stronger = brushAlpha[Mathf.Clamp((y + i) - (PuY - brushSizeInPourcent / 2), 0, brushSizeInPourcent - 1) * brushSizeInPourcent + Mathf.Clamp((x + j) - (PuX - brushSizeInPourcent / 2), 0, brushSizeInPourcent - 1)] * brushStronger;

                        terrainBay[index] = Color.Lerp(terrainBay[index], targetColor, Stronger);
                    }
                }
                Undo.RegisterCompleteObjectUndo(MaskTex, "meshPaint");//保存历史记录以便撤销

                MaskTex.SetPixels(x, y, width, height, terrainBay, 0);//把绘制后的Control贴图保存起来
                MaskTex.Apply();

                ToggleF = true;
                Debug.Log("Paint");
            }
        
            else if (e.type == EventType.MouseUp && e.alt == false && e.button == 0 && ToggleF == true)
            {
                Debug.Log("MouseUp");
                
                //绘制结束保存Control贴图
                ToggleF = false;
                
            }


        }
    }
    Texture2D UnlockTexture(Texture2D t)
    {
        MaskPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/") + 1) + AssetDatabase.GetAssetPath(t);
        Debug.Log(MaskPath);
        byte[] tBytes = System.IO.File.ReadAllBytes(MaskPath);
        Texture2D tampTex = new Texture2D(t.width,t.height);
        tampTex.LoadImage(tBytes);
        return tampTex;
    }
    string GetAssetPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return string.Empty;
        fullPath = fullPath.Replace("\\", "/");
        int index = fullPath.IndexOf("Assets/");
        if (index < 0)
            return fullPath;
        string ret = fullPath.Substring(index);
        return ret;
    }
    public void SaveTexture()
    {
        if (MaskPath == string.Empty)
            return;
        //var path = AssetDatabase.GetAssetPath(MaskTex);
        var bytes =  MaskTex.EncodeToPNG();
        Debug.Log(GetAssetPath(MaskPath));
        File.WriteAllBytes(MaskPath, bytes);
        AssetDatabase.Refresh();
        //AssetDatabase.ImportAsset(MaskPath, ImportAssetOptions.ForceUpdate);//刷新
        Debug.Log("SaveTexture");
    }
}
