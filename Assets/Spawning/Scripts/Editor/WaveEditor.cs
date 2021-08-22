using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SpawningFramework
{
    [CustomEditor(typeof(Wave))]

    public class WaveEditor : Editor
    {
        SerializedProperty totalWaveTime, paddingTime, type, noRepeatInterval;
        SerializedObject current;

        WaveGroup linkedGroup;

        void OnEnable()
        {
            totalWaveTime = serializedObject.FindProperty("totalWaveTime");
            paddingTime = serializedObject.FindProperty("paddingTime");
            current = new SerializedObject(((Wave)target));//handled differently because its an array
            type = serializedObject.FindProperty("type");
            noRepeatInterval = serializedObject.FindProperty("noRepeatInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            FindGroup();

            if(linkedGroup == null)
            {
                EditorGUILayout.LabelField("Failed to find WaveGroup!");
                EditorGUILayout.LabelField("Make sure the componenet is attached to either this object or its parent");
            }

            EditorGUILayout.PropertyField(type, new GUIContent("Wave Spawn Type", "Determines if waves should spawn randomly or in sequence"));

            if(type.intValue != (int)WaveType.Type.SpawnSequentially)
                EditorGUILayout.PropertyField(totalWaveTime, new GUIContent("Total Run Time", "How long to run this wave for"));
            else
                totalWaveTime.floatValue = float.MaxValue;//basically for sequential waves run them until told otherwise

            if(linkedGroup != null)
            {
                if(linkedGroup.type == RestrictedType.Type.SpawnRandomly)//only show this option if the group is spawning waves randomly
                    EditorGUILayout.PropertyField(noRepeatInterval, new GUIContent("No Repeat Interval", "Determines if waves should spawn randomly or in sequence"));
            }

            EditorGUILayout.PropertyField(paddingTime, new GUIContent("Padding Time", "This time is used to create a pause in game play before spawning the next block.\nE.G it can be useful if you have spawned a lot of enemies to have some padding so the player has time to kill them"));

            GUILayout.Space(25);

            GUIHelper.ArrayGUI(current, "spawnInstructions");//Display the array of spawn instructions

            serializedObject.ApplyModifiedProperties();// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            
            if(GUI.changed)
                EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Attempts to find a wave group compenent either on ths same gameobject or its parent. Edit this is your hierarchy requires it to be somewhere else, or delete this script all together
        /// </summary>
        void FindGroup()
        {
            if(linkedGroup == null)
            {
                Wave current = (Wave)target;

                linkedGroup = current.gameObject.GetComponent<WaveGroup>();

                if(linkedGroup == null && current.transform.parent != null)
                    linkedGroup = current.gameObject.transform.parent.GetComponent<WaveGroup>();
            }
        }
    }
}