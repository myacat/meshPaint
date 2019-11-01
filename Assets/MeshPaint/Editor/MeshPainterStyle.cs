using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

[CustomEditor(typeof(MeshPainter))]
[CanEditMultipleObjects]
public class MeshPainterStyle : Editor
{
    string contolTexName = "";

    bool isPaint;

    float brushSize = 16f;
    float brushStronger = 0.5f;

    Texture[] brushTex;
    Texture[] texLayer;

    int selBrush = 0;
    int selTex = 0;


    int brushSizeInPourcent;
    Texture2D MaskTex;
    void OnSceneGUI()
    {
        if (isPaint)
        {
            Painter();
        }

    }
    public override void OnInspectorGUI()
        
    {
        if (Cheak())
        {
            GUIStyle boolBtnOn = new GUIStyle(GUI.skin.GetStyle("Button"));//得到Button样式
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                isPaint = GUILayout.Toggle(isPaint, EditorGUIUtility.IconContent("EditCollider"), boolBtnOn, GUILayout.Width(35), GUILayout.Height(25));//编辑模式开关
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            brushSize = (int)EditorGUILayout.Slider("Brush Size", brushSize, 1, 36);//笔刷大小
            brushStronger = EditorGUILayout.Slider("Brush Stronger", brushStronger, 0, 1f);//笔刷强度

            IniBrush();
            layerTex();
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal("box", GUILayout.Width(340));
                    selTex = GUILayout.SelectionGrid(selTex, texLayer, 4, "gridlist", GUILayout.Width(340), GUILayout.Height(86));
                    GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal("box", GUILayout.Width(318));
                    selBrush = GUILayout.SelectionGrid(selBrush, brushTex, 9, "gridlist", GUILayout.Width(340), GUILayout.Height(70));
                    GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
    }

    //获取材质球中的贴图
    void layerTex()
    {
        Transform Select = Selection.activeTransform;
        texLayer = new Texture[4];
        texLayer[0] = AssetPreview.GetAssetPreview(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat0")) as Texture;
        texLayer[1] = AssetPreview.GetAssetPreview(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat1")) as Texture;
        texLayer[2] = AssetPreview.GetAssetPreview(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat2")) as Texture;
        texLayer[3] = AssetPreview.GetAssetPreview(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Splat3")) as Texture;
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

    //检查
    bool Cheak()
    {
        bool Cheak = false;
        Transform Select = Selection.activeTransform;
        Texture ControlTex = Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Control");
        if(Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.shader == Shader.Find("Mya/texBlend/mya_4tex_blend_diffuce") || Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.shader == Shader.Find("Mya/texBlend/mya_4tex_blend_normal"))
        {
            if(ControlTex == null)
            {
                EditorGUILayout.HelpBox("当前模型材质球中未找到Control贴图，绘制功能不可用！", MessageType.Error);
                if (GUILayout.Button("创建Control贴图"))
                {
                    creatContolTex();
                    //Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Control", creatContolTex());
                }
            }
            else
            {
                Cheak = true;
            }
        }
        else 
        {
            EditorGUILayout.HelpBox("当前模型shader错误！请更换！", MessageType.Error);
        }
        return Cheak;
    }

    //创建Contol贴图
    void creatContolTex()
    {

        //创建一个新的Contol贴图
        string ContolTexFolder = "Assets/MeshPaint/Controler/";
        Texture2D newMaskTex = new Texture2D(512, 512, TextureFormat.ARGB32, true);
        Color[] colorBase = new Color[512 * 512];
        for(int t = 0; t< colorBase.Length; t++)
        {
            colorBase[t] = new Color(1, 0, 0, 0);
        }
        newMaskTex.SetPixels(colorBase);

        //判断是否重名
        bool exporNameSuccess = true;
        for(int num = 1; exporNameSuccess; num++)
        {
            string Next = Selection.activeTransform.name +"_"+ num;
            if (!File.Exists(ContolTexFolder + Selection.activeTransform.name + ".png"))
            {
                contolTexName = Selection.activeTransform.name;
                exporNameSuccess = false;
            }
            else if (!File.Exists(ContolTexFolder + Next + ".png"))
            {
                contolTexName = Next;
                exporNameSuccess = false;
            }

        }

        string path = ContolTexFolder + contolTexName + ".png";
        byte[] bytes = newMaskTex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);//保存


        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);//导入资源
        //Contol贴图的导入设置
        TextureImporter textureIm = AssetImporter.GetAtPath(path) as TextureImporter;
        textureIm.textureFormat = TextureImporterFormat.ARGB32;
        textureIm.isReadable = true;
        textureIm.anisoLevel = 9;
        textureIm.mipmapEnabled = false;
        textureIm.wrapMode = TextureWrapMode.Clamp;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);//刷新


        setContolTex(path);//设置Contol贴图

    }

    //设置Contol贴图
    void setContolTex(string peth)
    {
        Texture2D ControlTex = (Texture2D)AssetDatabase.LoadAssetAtPath(peth, typeof(Texture2D));
        Selection.activeTransform.gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Control", ControlTex);
    }

    void Painter()
    {
        
        
        Transform CurrentSelect = Selection.activeTransform;
        MeshFilter temp = CurrentSelect.GetComponent<MeshFilter>();//获取当前模型的MeshFilter
        float orthographicSize = (brushSize * CurrentSelect.localScale.x) * (temp.sharedMesh.bounds.size.x / 200);//笔刷在模型上的正交大小
        MaskTex = (Texture2D)CurrentSelect.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Control");//从材质球中获取Control贴图

        brushSizeInPourcent = (int)Mathf.Round((brushSize * MaskTex.width) / 100);//笔刷在模型上的大小
        bool ToggleF = false;
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
            }

            else if (e.type == EventType.MouseUp && e.alt == false && e.button == 0 && ToggleF == true)
            {

                SaveTexture();//绘制结束保存Control贴图
                ToggleF = false;
            }
        }
    }
    public void SaveTexture()
    {
        var path = AssetDatabase.GetAssetPath(MaskTex);
        var bytes = MaskTex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);//刷新
    }
}
