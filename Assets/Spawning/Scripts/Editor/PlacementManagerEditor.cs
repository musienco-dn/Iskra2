using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SpawningFramework
{
    [CustomEditor(typeof(PlacementManager))]
    public class PlacementManagerEditor : Editor
    {
        #region Variables
        SerializedProperty spawnManager;
        SerializedProperty[] types;//per wave group
        SerializedObject current;
        SerializedObject[] groups, typeObjects;

        SpawningManager spawning;
        PlacementManager placement;

        string tooltip;
        #endregion

        #region Methods
        void OnEnable()
        {
            spawnManager = serializedObject.FindProperty("spawnManager");

            if (spawnManager.objectReferenceValue != null)
                spawning = (SpawningManager)spawnManager.objectReferenceValue;
            else
                return;

            placement = (PlacementManager)target;

            if (placement.spawnGroups == null)
            {
                placement.spawnGroups = new SpawnGroup[spawning.groups.Length];

                for (int i = 0; i < placement.spawnGroups.Length; i++)
                {
                    placement.spawnGroups[i] = placement.gameObject.AddComponent<SpawnGroup>();
                    placement.spawnGroups[i].spawnAreas = new SpawnArea[0];
                }
            }

            types = new SerializedProperty[placement.spawnGroups.Length];
            typeObjects = new SerializedObject[placement.spawnGroups.Length];
            groups = new SerializedObject[placement.spawnGroups.Length];
            current = new SerializedObject(placement);//handled differently because its an array
        }

        public override void OnInspectorGUI()
        {
            #region Styles
            GUIStyle richText = new GUIStyle();
            richText.richText = true;

            if (EditorGUIUtility.isProSkin)
                richText.normal.textColor = new Color(225, 225, 225);
            #endregion

            serializedObject.Update();

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(spawnManager, new GUIContent("Spawn Manager", "This maanger is used to link spawn areas to specific waves"));

            if (spawning != null)//cant do anything without a spawn manager!
            {
                if (placement == null)//if there is no placement object
                {
                    OnEnable();//then attempt to find it
                    return;
                }

                if (placement.spawnGroups.Length != spawning.groups.Length)
                {
                    if (placement.spawnGroups.Length > spawning.groups.Length)//if we have more groups than needed
                    {
                        for (int i = spawning.groups.Length; i < placement.spawnGroups.Length; i++)
                            DestroyImmediate(placement.spawnGroups[i]);//remove the component from the gameobject to avoid clutter

                        Debug.LogError("*** Cleaned up unreferenced SpawnGroups. This will produce MissingReferenceException errors, this is normal ***");
                    }

                    System.Array.Resize<SpawnGroup>(ref placement.spawnGroups, spawning.groups.Length);
                }

                for (int i = 0; i < spawning.groups.Length; i++)
                {
                    if (spawning.groups[i] == null)
                    {
                        Debug.LogError("Found null spawn group: " + spawning.gameObject.name);
                        break;
                    }

                    #region Create New Group
                    if (placement.spawnGroups[i] == null)
                    {
                        placement.spawnGroups[i] = placement.gameObject.AddComponent<SpawnGroup>();
                        placement.spawnGroups[i].spawnAreas = new SpawnArea[0];
                    }
                    #endregion

                    if (i == 0)
                        GUILayout.Space(5);
                    else
                        GUILayout.Space(25);

                    EditorGUI.indentLevel = 0;

                    #region Details
                    EditorGUILayout.LabelField(new GUIContent(spawning.groups[i].gameObject.name, "The spawning areas associated with this group"), EditorStyles.boldLabel);

                    EditorGUILayout.LabelField(new GUIContent("<b>Waves:</b> " + spawning.groups[i].waves.Length + "   " + spawning.groups[i].RoughDetails(), "How many waves will be spawned. takes into consideration Random and Sequential waves"), richText);

                    #region Spawn Type
                    if (types.Length <= i)
                    {
                        typeObjects = new SerializedObject[placement.spawnGroups.Length];
                        types = new SerializedProperty[typeObjects.Length];

                        groups = new SerializedObject[typeObjects.Length];
                    }

                    if (types[i] == null)
                    {
                        typeObjects[i] = new SerializedObject(placement.spawnGroups[i]);
                        types[i] = typeObjects[i].FindProperty("type");//find the spawn type

                    }

                    if (placement.spawnGroups[i].spawnAreas.Length > 1)//only bother showing this if there is more than one type of spawn area
                    {
                        typeObjects[i].Update();

                        switch ((PlacementManager.Type)types[i].intValue)
                        {
                            case PlacementManager.Type.SpawnRandomlyPerObject:
                                tooltip = "\n\nRandom Per Object\n\nIn this mode each object will spawn at a new random area";
                                break;

                            case PlacementManager.Type.SpawnRandomlyPerWave:
                                tooltip = "\n\nRandom Per Wave\n\nIn this mode an area will be selected randomly and all objects in this wave will spawn in the same area";
                                break;

                            case PlacementManager.Type.SpawnSequentiallyPerObject:
                                tooltip = "\n\nSequentially Per Object\n\nIn this mode objects will spawn at each area in a pattern\nE.G object 1 will spawn within area 1, object 2 area 2";
                                break;

                            case PlacementManager.Type.SpawnSequentiallyPerWave:
                                tooltip = "\n\nRandom Per Wave\n\nIn this mode each wave will spawn all its objects per area.\nE.G wave 1 will spawn within area 1, 2 area 2";
                                break;
                        }

                        EditorGUILayout.PropertyField(types[i], new GUIContent("Cycle Type", "Switch from either spawning in a Sequential pattern or to Randomly choose an area to spawn within" + tooltip));

                        //if (types[i].intValue == (int)PlacementManager.Type.SpawnRandomlyPerWave || types[i].intValue == (int)PlacementManager.Type.SpawnRandomlyPerObject)//only show this for randomly spawning types
                        //    EditorGUILayout.PropertyField(noRepeatIntervals.GetArrayElementAtIndex(i), new GUIContent("No Repeat Interval", "This helps ensure the same spawn area isn't choosen too often. Basically once this area has spawned something this much time must pass before anything can spawn in it again"));

                        typeObjects[i].ApplyModifiedProperties();
                    }
                    #endregion

                    GUILayout.Space(5);
                    #endregion

                    #region Spawn Area Array
                    EditorGUI.indentLevel = 3;

                    if (placement.spawnGroups[i] == null)
                        placement.spawnGroups[i] = placement.gameObject.AddComponent<SpawnGroup>();

                    if (placement.spawnGroups[i].spawnAreas == null)
                        placement.spawnGroups[i].spawnAreas = new SpawnArea[0];

                    if (groups[i] == null)
                        groups[i] = new SerializedObject(placement.spawnGroups[i]);

                    GUIHelper.ArrayGUI(groups[i], groups[i].FindProperty("spawnAreas"));//Display the array of spawn instructions
                    #endregion
                }
            }
            else if (spawnManager != null)
                spawning = (SpawningManager)spawnManager.objectReferenceValue;

            if (GUI.changed)
                EditorUtility.SetDirty(target);

            serializedObject.ApplyModifiedProperties();// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        }
        #endregion
    }
}
