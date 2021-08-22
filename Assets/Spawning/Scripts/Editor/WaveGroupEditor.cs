using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SpawningFramework
{
    [CustomEditor(typeof(WaveGroup))]
    public class WaveGroupEditor : Editor
    {
        SerializedProperty wavesToSpawn, type, noRepeatInterval;

        SerializedObject current;//used to display arrays correctly

        SpawningManager linkedManager;//which manager this instruction belongs to

        bool infiniteWaves, searchedForManager;

        void OnEnable()
        {
            wavesToSpawn = serializedObject.FindProperty("wavesToSpawn");
            type = serializedObject.FindProperty("type");
            noRepeatInterval = serializedObject.FindProperty("noRepeatInterval");

            current = new SerializedObject(((WaveGroup)target));//handled differently because its an array

            infiniteWaves = wavesToSpawn.intValue == WaveType.InfiniteWaves;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            FindManager();

            EditorGUILayout.PropertyField(type, new GUIContent("Wave Spawn Type", "Determines if waves should spawn randomly or in sequence"));

            #region Waves to Spawn
            if(type.intValue == (int)WaveType.Type.SpawnRandomly)
            {
                EditorGUILayout.BeginHorizontal();

                if(infiniteWaves)//if the user wants infinite waves then disable manual wave entry
                    GUI.enabled = false;

                EditorGUILayout.PropertyField(wavesToSpawn, new GUIContent("Waves to Spawn", "How many waves do we spawn"));//when you are randomly spawning waves there is no way to know when to stop. this helps determine that

                GUI.enabled = true;//enable the rest of the controls

                infiniteWaves = EditorGUILayout.Toggle("Infinite", infiniteWaves);

                EditorGUILayout.EndHorizontal();

                if(!infiniteWaves && wavesToSpawn.intValue == WaveType.InfiniteWaves)//if the user unchecked the infinite box
                    wavesToSpawn.intValue = 0;
                else if(infiniteWaves && wavesToSpawn.intValue != WaveType.InfiniteWaves)//if teh box is checked and the value doesnt match infinite
                    wavesToSpawn.intValue = WaveType.InfiniteWaves;
            }
            #endregion

            if(linkedManager != null)
                if(linkedManager.type == RestrictedType.Type.SpawnRandomly)//only show this option if the group is spawning waves randomly
                    EditorGUILayout.PropertyField(noRepeatInterval, new GUIContent("No Repeat Interval", "Determines if waves should spawn randomly or in sequence"));

            GUILayout.Space(25);

            GUIHelper.ArrayGUI(current, "waves");//Display the array of waves

            if(GUI.changed)
                EditorUtility.SetDirty(target);

            serializedObject.ApplyModifiedProperties();// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        }

        /// <summary>
        /// Attempts to find a wave manager compenent either on ths same gameobject or its parent. Edit this is your hierarchy requires it to be somewhere else, or delete this script all together
        /// </summary>
        void FindManager()
        {
            if(linkedManager == null && !searchedForManager)
            {
                WaveGroup current = (WaveGroup)target;

                SpawningManager[] managers = FindObjectsOfType<SpawningManager>();//find all spawning managers

                for(int i = 0; i < managers.Length; i++)
                    if(managers[i].groups != null)
                        for(int ii = 0; ii < managers[i].groups.Length; ii++)
                            if(managers[i].groups[ii] != null && managers[i].groups[ii].Equals(current))//if we have found this instruction, and thus the parent spawn manager
                            {
                                linkedManager = managers[i];
                                searchedForManager = true;
                                return;//dont bother looping through the rest of the hierarchy!
                            }


                searchedForManager = true;
            }
        }
    }
}