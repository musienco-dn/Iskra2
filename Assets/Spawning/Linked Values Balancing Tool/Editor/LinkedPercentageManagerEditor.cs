using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LinkedPercentageManager))]
public class LinkedPercentageManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("This is best edited with the window view");

        if(GUILayout.Button("Open Window"))
            LinkedPercentageWindow.Initialise();

        EditorGUILayout.HelpBox("When you make a build this object becomes redundant and can be safely deleted during a custom build phase.", MessageType.Info);

        //GUILayout.Space(20);

        //if(GUILayout.Button("Spawn Test ScriptableObject"))//this spawns an instance of ScriptableObjectExample
        //{
        //    ScriptableObjectExample newObject = ScriptableObject.CreateInstance<ScriptableObjectExample>();
        //    AssetDatabase.CreateAsset(newObject, "Assets/Linked Values/Examples/New Object.asset");
        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.Refresh();

        //    Debug.LogError("Spawned at: Assets/Linked Values/Examples/New Object.asset");
        //}

        //if(GUILayout.Button("Refresh"))
        //    LinkedPercentageWindow.RefreshData();//if there is an error chances are its null data, so this removes it. This can happen after each build!
    }
}
