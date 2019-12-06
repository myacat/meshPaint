using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FlagWaveShaderGUI : ShaderGUI
{
    MaterialEditor m_MaterialEditor;

    MaterialProperty color = null;
    MaterialProperty mainTex = null;

    MaterialProperty bumpMap = null;
    MaterialProperty bumpScale = null;

    MaterialProperty waveMask = null;
    MaterialProperty waveIntensity = null;

    MaterialProperty lightColor = null;
    MaterialProperty lightDir = null;
    MaterialProperty ambientCol = null;

    bool isEditor = false;
    bool starEditor = true;
    bool endEditor = true;

    GameObject selectGameObj;
    public void FindProperties(MaterialProperty[] props)
    {

        color = FindProperty("_Color", props);
        mainTex = FindProperty("_MainTex", props);

        bumpMap = FindProperty("_BumpMap", props);
        bumpScale = FindProperty("_BumpScale", props);

        waveMask = FindProperty("_WaveMask", props);
        waveIntensity = FindProperty("_WaveIntensity", props);

        lightColor = FindProperty("_LightColor", props);
        lightDir = FindProperty("_LightDir", props);
        ambientCol = FindProperty("_AmbientCol", props);
    }
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        materialEditor.PropertiesDefaultGUI(props);

        FindProperties(props);
        m_MaterialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        ShaderPropertiesGUI(material);
    }
    public void ShaderPropertiesGUI(Material material)
    {
        m_MaterialEditor.SetDefaultGUIWidths();

        //if (GUILayout.Button("Set"))
        //{
        //    selectGameObj = Selection.activeGameObject;
        //    SceneView.onSceneGUIDelegate += OnSenceGUI;
        //    
        //    //LightDirWindow lightWin = LightDirWindow.Init(material , lightDir.vectorValue);
        //}
        isEditor = GUILayout.Toggle( isEditor , "Set", "Button");
        if (isEditor)
        {
            if (starEditor)
            {
                selectGameObj = Selection.activeGameObject;
                Vector3 localDir = new Vector3(lightDir.vectorValue.x, lightDir.vectorValue.y, lightDir.vectorValue.z);
                Vector3 worldDir = selectGameObj.transform.rotation * localDir;
                rot  = Quaternion.FromToRotation(Vector3.forward, worldDir);
                //rot = Quaternion.identity;// Quaternion.FromToRotation(Vector3.forward, selectGameObj.transform.rotation * new Vector3(lightDir.vectorValue.x, lightDir.vectorValue.y, lightDir.vectorValue.z));
                SceneView.onSceneGUIDelegate += OnSenceGUI;
                starEditor = false;
                Debug.Log("starEditor");
            }  
        }
        else
        {
            if (!starEditor)
            {
                SceneView.onSceneGUIDelegate -= OnSenceGUI;
                selectGameObj = null;
                starEditor = true;
                Debug.Log("endEditor");
            }
        }
        



        EditorGUI.BeginChangeCheck();
        {


        }
        if (EditorGUI.EndChangeCheck())
        {

        }

        m_MaterialEditor.RenderQueueField();
        m_MaterialEditor.EnableInstancingField();
    }

    void OnSceneGUI()
    {

        Handles.Label(new Vector3(0,0,0), "Test");
    }
    public override void OnClosed(Material material)
    {
        Debug.Log("OnClosed");
    }

    public Material mat = null;

    public Quaternion rot = Quaternion.identity;
    void OnSenceGUI(SceneView senceView)
    {

        if(Selection.activeGameObject != selectGameObj)
            SceneView.onSceneGUIDelegate -= OnSenceGUI;

        Vector3 pos = selectGameObj.transform.position;
        Handles.Label(pos, "LightDir:" + lightDir.vectorValue);

        //rot = Handles.RotationHandle(rot, pos);
        ////Vector3 worldDir = rot * Vector3.forward;
        //Vector3 newDir = rot * new Vector3(lightDir.vectorValue.x, lightDir.vectorValue.y, lightDir.vectorValue.z);
        ////Vector3 newDir = Quaternion.FromToRotation(selectGameObj.transform.forward, worldDir) * Vector3.forward;
        //lightDir.vectorValue = new Vector4(newDir.x, newDir.y, newDir.z, lightDir.vectorValue.w);
        //Handles.color = Color.red;
        //Handles.DrawLine(pos, pos + selectGameObj.transform.rotation * newDir);
        //Handles.color = Color.green;
        //Handles.DrawLine(pos, pos + rot * Vector3.forward);

        //Vector3 localDir = new Vector3(lightDir.vectorValue.x, lightDir.vectorValue.y, lightDir.vectorValue.z);
        //Vector3 worldDir = selectGameObj.transform.rotation * localDir;
        //Quaternion worldRot = Quaternion.FromToRotation(Vector3.forward, worldDir);
        rot = Handles.RotationHandle(rot, pos);
        Vector3 newlocalDir = Quaternion.Inverse(selectGameObj.transform.rotation) * rot * Vector3.forward;

        lightDir.vectorValue = new Vector4(newlocalDir.x, newlocalDir.y, newlocalDir.z, lightDir.vectorValue.w);

        //Handles.color = Color.red;
        //Handles.DrawLine(pos, pos + worldDir);
        Handles.color = Color.green;
        Handles.DrawLine(pos, pos + rot * Vector3.forward);
        //Handles.color = Color.yellow;
        //Handles.DrawLine(pos, pos + Quaternion.Inverse (selectGameObj.transform.rotation) * worldDir * 2);
        //Handles.color = Color.white;
        //Handles.DrawLine(pos, pos + localDir);
        //Quaternion localRot = Quaternion.FromToRotation(selectGameObj.transform.forward, localDir);
        ////Quaternion rot = selectGameObj.transform.rotation * Quaternion.FromToRotation(Vector3.forward, localDir);
        //
        //Quaternion newRot = Quaternion.FromToRotation(selectGameObj.transform.forward,Handles.RotationHandle(rot, pos) * Vector3.forward);
        //
        //Vector3 newWorldDir = newRot * Vector3.forward;
        ////rot = Quaternion.FromToRotation(selectGameObj.transform.forward, worldDir);
        ////Vector3 dir = newrot * Vector3.forward;
        //
        //lightDir.vectorValue = new Vector4(newWorldDir.x, newWorldDir.y, newWorldDir.z, lightDir.vectorValue.w);
        //Handles.color = Color.green;
        //Handles.DrawLine(pos, pos + worldDir);
        //Handles.color = Color.red;
        //Handles.DrawLine(pos, pos + newWorldDir);



    }
}
