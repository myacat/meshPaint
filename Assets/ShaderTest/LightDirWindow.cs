using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LightDirWindow : EditorWindow
{
    public Material mat = null;
    public Vector4 lightDir = Vector4.zero;
    public Quaternion rot = Quaternion.identity;
    Transform selectGameObj;
    static public LightDirWindow Init(Material m , Vector4 ld )
    {
        LightDirWindow window =  GetWindow<LightDirWindow>("Mya Mesh Painter");
        window.mat = m;
        window.lightDir = ld;
        window.rot = Quaternion.FromToRotation(Vector3.forward, new Vector3(ld.x, ld.y, ld.z));
        window.selectGameObj = Selection.activeGameObject.transform;
        SceneView.onSceneGUIDelegate += window.OnSenceGUI;
        Debug.Log("+");
        window.Show();
        return window;
    }
   

    void OnGUI()
    {
        lightDir = EditorGUILayout.Vector4Field("LightDir:" ,lightDir);
        if (GUILayout.Button("Set"))
        {
            if(mat != null)
            {
                mat.SetVector("_LightDir", lightDir);
                this.Close();
            }
            
        }
    }

    void OnSenceGUI(SceneView senceView)
    {
        Handles.Label(selectGameObj.position, "LightDir:" + lightDir);
        rot = Handles.RotationHandle(rot , selectGameObj.position);
        
        //Handles.FreeMoveHandle(selectGameObj.position , rot,1, Vector3.zero, Handles.RectangleHandleCap);
        //Transform t = new Transform();
        //Vector3 dir = new Vector3(lightDir.x, lightDir.y, lightDir.z);

        Vector3 dir = rot * Vector3.forward;
        lightDir = new Vector4(dir.x, dir.y, dir.z, lightDir.w);
        Handles.DrawLine(selectGameObj.position, selectGameObj.position + dir);
        mat.SetVector("_LightDir", lightDir);
        //dir = Vector3.Dot( dir , rot);
        //do something to senceView; 

    }

    void OnDestroy()
    {

        SceneView.onSceneGUIDelegate -= OnSenceGUI;
        Debug.Log("-");

    }

}
