using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SpawningFramework
{
    [CustomEditor(typeof(SpawnArea))]
    [CanEditMultipleObjects]
    public class SpawnAreaEditor : Editor
    {
        SerializedProperty collider, type, gizmoColour, noRepeatInterval;

        PlacementManager linkedManager;//basically what manager does this group belong to
        int spawnGroup;//which group this spawn area belongs to

        void OnEnable()
        {
            collider = serializedObject.FindProperty("collider");
            type = serializedObject.FindProperty("type");
            gizmoColour = serializedObject.FindProperty("gizmoColour");
            noRepeatInterval = serializedObject.FindProperty("noRepeatInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            EditorGUILayout.PropertyField(gizmoColour, new GUIContent("Gizmo Colour", "Used to colour the scene gizmos"));
            EditorGUILayout.PropertyField(collider, new GUIContent("Collider", "Determines the shape for spawning enemies"));

            if(collider.objectReferenceValue is SphereCollider)//only display this for sphere colliders, doesnt matter otherwise
                EditorGUILayout.PropertyField(type, new GUIContent("Sphere Spawning Type", "How do you spawn within this sphere"));

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(noRepeatInterval, new GUIContent("No Repeat Interval", "This prevents spawning another enemy once one has spawned. Useful for challenging spawn areas to avoid spawning as many enemies there. This can be used to skip spawn areas for groups that spawn sequentially"));

            serializedObject.ApplyModifiedProperties();// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.

            if(GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}