#define DebugButtons

using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;
using System.Reflection;
using System;

public class LinkedPercentageWindow : EditorWindow
{
    #region Variables
    const float SaveDelay = 0.25f;//if we save every time we move the slider you always see the loading icon which is a bit annoying and inneficient. This helps delay that
    float saveDelayTimer;

    LinkedPercentageManager current;
    AnimBool[] dropDowns;
    AnimBool[][] subDropDowns;//this is for the sub groups (Object, Class Name etc)

    int dirtyIndex = -1;//which group is dirty and needs saved
    GUIStyle centeredLabel;
    Vector2 scrollPosition;
    #endregion

    #region Methods
    public static void RefreshData()
    {
        RemoveNullData();

        LinkedPercentage[] allValues = GameObject.FindObjectsOfType<LinkedPercentage>();

        for(int i = 0; i < allValues.Length; i++)
            allValues[i].Update();
    }

    static void RemoveNullData()
    {
        for(int i = 0; i < LinkedPercentageManager.Instance.linkedGroups.Count; i++)
            for(int ii = 0; ii < LinkedPercentageManager.Instance.linkedGroups[i].dataList.Count; ii++)
                if(LinkedPercentageManager.Instance.linkedGroups[i].dataList[ii] == null)//remove any null data
                {
                    LinkedPercentageManager.Instance.linkedGroups[i].dataList.RemoveAt(ii);
                    ii--;
                }
    }

    [MenuItem("Tools/K2 Games/Linked Percentages")]
    public static void Initialise()
    {
        LinkedPercentageWindow window = (LinkedPercentageWindow)EditorWindow.GetWindow(typeof(LinkedPercentageWindow), false, "Linked Percentages");
        window.Show();
    }

    void OnGUI()
    {
        #region Initialise
        current = LinkedPercentageManager.Instance;
        centeredLabel = EditorStyles.boldLabel;
        centeredLabel.alignment = TextAnchor.MiddleCenter;

        #region Drop Down Groups
        if(dropDowns == null)
            dropDowns = new AnimBool[current.groupNames.Length];

        if(dropDowns.Length != current.groupNames.Length)
            System.Array.Resize<AnimBool>(ref dropDowns, current.groupNames.Length);

        if(subDropDowns == null)
        {
            subDropDowns = new AnimBool[current.groupNames.Length][];

            for(int i = 0; i < subDropDowns.Length; i++)
                subDropDowns[i] = new AnimBool[4];//one drop down for each column
        }

        if(subDropDowns.Length != current.groupNames.Length)
        {
            System.Array.Resize<AnimBool[]>(ref subDropDowns, current.groupNames.Length);

            for(int i = 0; i < subDropDowns.Length; i++)
                if(subDropDowns[i] == null)
                    subDropDowns[i] = new AnimBool[4];
        }

        for(int i = 0; i < subDropDowns.Length; i++)
            for(int ii = 0; ii < subDropDowns[i].Length; ii++)
                if(subDropDowns[i][ii] == null)
                    subDropDowns[i][ii] = new AnimBool(true);
        #endregion

        RemoveNullData();
        #endregion

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width));

        for(int i = 0; i < current.linkedGroups.Count; i++)
            DrawGroup(current.groupNames[i], current.linkedGroups[i], i);

        EditorGUILayout.EndScrollView();

        #region Bottom Controls
        GUILayout.FlexibleSpace();

        using(new Vertical(GUIHelper.LayoutStyle.Box))
        {
            EditorGUILayout.LabelField("Controls", centeredLabel);

            using(new Horizontal())
            {
                if(GUILayout.Button("Show Object"))
                    for(int i = 0; i < subDropDowns.Length; i++)
                        subDropDowns[i][0].target = true;

                if(GUILayout.Button("Show Class Name"))
                    for(int i = 0; i < subDropDowns.Length; i++)
                        subDropDowns[i][1].target = true;

                if(GUILayout.Button("Show Variable Name"))
                    for(int i = 0; i < subDropDowns.Length; i++)
                        subDropDowns[i][2].target = true;
            }

            using(new Horizontal())
            {
                #region Scan For Data Changes
                if(GUILayout.Button("Scan For New Variables"))
                {
                    SerializedObject linkedObject;

                    for(int i = 0; i < current.linkedGroups.Count; i++)
                        for(int ii = 0; ii < current.linkedGroups[i].dataList.Count; ii++)
                        {
                            linkedObject = new SerializedObject(current.linkedGroups[i].dataList[ii].linkedObject);//grab an instance to the class already holding LinkedPercentages

                            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                            FieldInfo[] fields = current.linkedGroups[i].dataList[ii].linkedObject.GetType().GetFields(flags);

                            for(int j = 0; j < fields.Length; j++)
                                if(fields[j].FieldType.Equals(typeof(LinkedPercentage)))
                                    if(fields[j].GetValue(current.linkedGroups[i].dataList[ii].linkedObject) == null)//basically use reflection to find new variables
                                    {
                                        SerializedObject finder = new SerializedObject(current.linkedGroups[i].dataList[ii].linkedObject);
                                        SerializedProperty property = finder.FindProperty(fields[j].Name);

                                        fields[j].SetValue(current.linkedGroups[i].dataList[ii].linkedObject, LinkedPercentage.CreateInstance(property, true));
                                    }
                        }
                }

                if(GUILayout.Button("Scan For Invalid Data"))
                {
                    SerializedObject temp;
                    LinkedPercentage foundValue;
                    ScriptableObject foundObject;
                    int itemsFound = 0;

                    for(int i = 0; i < current.linkedGroups.Count; i++)
                        for(int ii = 0; ii < current.linkedGroups[i].dataList.Count; ii++)
                        {
                            foundValue = current.linkedGroups[i].dataList[ii];

                            if(foundValue.scriptableObjectPath.Length > 0)//if this is linked to a scriptable object
                            {
                                foundObject = AssetDatabase.LoadAssetAtPath(foundValue.scriptableObjectPath, typeof(ScriptableObject)) as ScriptableObject;

                                if(foundObject == null)
                                {
                                    current.linkedGroups[i].dataList.RemoveAt(ii);
                                    DestroyImmediate(foundValue, true);
                                    ii--;
                                    itemsFound++;
                                }
                            }
                            else
                            {
                                temp = new SerializedObject(foundValue.linkedObject);

                                if(temp.FindProperty(foundValue.variableName) == null)//if the variable name cannot be found
                                {
                                    current.linkedGroups[i].dataList.RemoveAt(ii);
                                    DestroyImmediate(foundValue);
                                    ii--;
                                    itemsFound++;
                                }
                            }
                        }

                    Debug.Log("Scanned and removed: " + itemsFound + " items");
                }
                #endregion
            }

            #region Debugging Methods
#if(DebugButtons)
            using(new Horizontal())
            {
                if(GUILayout.Button("Show All Components"))
                {
                    LinkedPercentage[] foundValues = GameObject.FindObjectsOfType<LinkedPercentage>();

                    for(int i = 0; i < foundValues.Length; i++)
                        foundValues[i].hideFlags = HideFlags.None;

                    foundValues = LinkedPercentageManager.instance.gameObject.GetComponents<LinkedPercentage>();

                    for(int i = 0; i < foundValues.Length; i++)
                        foundValues[i].hideFlags = HideFlags.None;
                }

                if(GUILayout.Button("Hide All Components"))
                {
                    LinkedPercentage[] foundValues = GameObject.FindObjectsOfType<LinkedPercentage>();

                    for(int i = 0; i < foundValues.Length; i++)
                        foundValues[i].hideFlags = HideFlags.HideInInspector;

                    foundValues = LinkedPercentageManager.instance.gameObject.GetComponents<LinkedPercentage>();

                    for(int i = 0; i < foundValues.Length; i++)
                        foundValues[i].hideFlags = HideFlags.HideInInspector;
                }

                if(GUILayout.Button("Delete All Components"))
                {
                    LinkedPercentage[] foundValues = GameObject.FindObjectsOfType<LinkedPercentage>();

                    Debug.LogError(foundValues.Length);

                    for(int i = 0; i < foundValues.Length; i++)
                        DestroyImmediate(foundValues[i]);
                }
            }
#endif
            #endregion

        }
        #endregion

        #region Finalise and Save
        if(GUI.changed)
            saveDelayTimer = SaveDelay;//show new data needs saved but not instantly
        else if(saveDelayTimer > 0)
        {
            saveDelayTimer -= 0.032f;//reduce the timer

            if(saveDelayTimer <= 0)//if the timer has finished, actually save!
                Save();
        }
        #endregion
    }

    void Save()
    {
        if(dirtyIndex == -1)
            return;

        for(int i = 0; i < current.linkedGroups[dirtyIndex].dataList.Count; i++)
        {
            EditorUtility.SetDirty(current.linkedGroups[dirtyIndex].dataList[i]);
            Undo.RegisterUndo(current.linkedGroups[dirtyIndex].dataList[i], "Linked Percentage");
        }

        EditorUtility.SetDirty(current);

        dirtyIndex = -1;//show everything is up to date
    }

    void Update()
    {
        Repaint();
    }

    void DrawGroup(string heading, LinkedPercentageData data, int groupIndex)
    {
        GUIHelper.DrawCenteredToggle(ref dropDowns[groupIndex], new GUIContent(heading.Length > 0 ? heading : "*Empty*"));

        float smallColumnsWidth = position.width * 0.2f;
        float mainColumnWidth = position.width * 0.4f;

        smallColumnsWidth *= 0.9f;//helps prevent the scroll view from showing the horizontal bar at the bottom when its not needed
        mainColumnWidth *= 0.9f;

        for(int i = 0; i < subDropDowns[groupIndex].Length - 1; i++)
            if(!subDropDowns[groupIndex][i].value)//if any headings are hidden
                mainColumnWidth += smallColumnsWidth;//expand the main column

        if(EditorGUILayout.BeginFadeGroup(dropDowns[groupIndex].faded))
        {
            float percentage, previous;
            LinkedPercentage currentData;

            using(new Vertical(GUIHelper.LayoutStyle.Box))
            {
                using(new Horizontal())
                using(new FixedWidthLabel("Group:"))
                    current.groupNames[groupIndex] = EditorGUILayout.TextField(current.groupNames[groupIndex]);

                using(new Horizontal())
                {
                    #region Percentage Slider
                    using(new Vertical(GUILayout.Width(mainColumnWidth)))
                    {
                        GUIHelper.DrawToggle(ref subDropDowns[groupIndex][3], new GUIContent("Percentage"));

                        if(EditorGUILayout.BeginFadeGroup(subDropDowns[groupIndex][3].faded))
                        {
                            for(int i = 0; i < data.dataList.Count; i++)
                            {
                                currentData = data.dataList[i];

                                if(currentData.linkedObject == null)//if the linked object has been destroyed
                                {
                                    DestroyImmediate(data.dataList[i]);
                                    data.dataList.RemoveAt(i);//then also remove the linked percentage
                                    return;
                                }

                                previous = data.dataList[i].percentage;
                                percentage = EditorGUILayout.Slider(previous, 0, 100);
                                LinkedPercentagePropertyDrawer.SetPercentage(ref currentData, percentage);

                                #region Saving
                                if(previous != percentage)//if the value changed
                                {
                                    saveDelayTimer = SaveDelay;//show a save needs triggered

                                    if(groupIndex != dirtyIndex)
                                    {
                                        if(dirtyIndex != -1)//if the index is a valid group
                                            Save();//save the old group instantly since the user has switched editing a group
                                        else
                                            dirtyIndex = groupIndex;//record which group needs saved
                                    }
                                }
                                #endregion
                            }

                            if(GUILayout.Button("Equalise Values"))
                            {
                                for(int i = 0; i < data.dataList.Count; i++)
                                    data.dataList[i].percentage = 100f / data.dataList.Count;

                                dirtyIndex = groupIndex;
                                Save();
                            }
                        }

                        EditorGUILayout.EndFadeGroup();
                    }
                    #endregion

                    #region Object
                    if(EditorGUILayout.BeginFadeGroup(subDropDowns[groupIndex][0].faded))
                        using(new Vertical(GUILayout.Width(smallColumnsWidth)))
                        {
                            GUIHelper.DrawToggle(ref subDropDowns[groupIndex][0], new GUIContent("Object"));

                            for(int i = 0; i < data.dataList.Count; i++)
                                if(data.dataList[i] != null)
                                    EditorGUILayout.ObjectField(data.dataList[i].gameObject, typeof(GameObject));
                        }

                    EditorGUILayout.EndFadeGroup();
                    #endregion

                    #region Class Name
                    if(EditorGUILayout.BeginFadeGroup(subDropDowns[groupIndex][1].faded))
                        using(new Vertical(GUILayout.Width(smallColumnsWidth)))
                        {
                            GUIHelper.DrawToggle(ref subDropDowns[groupIndex][1], new GUIContent("Class Name"));

                            for(int i = 0; i < data.dataList.Count; i++)
                            {
                                currentData = data.dataList[i];

                                EditorGUILayout.LabelField(currentData.linkedObject.GetType().ToString(), GUILayout.Width(smallColumnsWidth));
                            }
                        }

                    EditorGUILayout.EndFadeGroup();
                    #endregion

                    #region Variable Name
                    if(EditorGUILayout.BeginFadeGroup(subDropDowns[groupIndex][2].faded))
                        using(new Vertical(GUILayout.Width(smallColumnsWidth)))
                        {
                            GUIHelper.DrawToggle(ref subDropDowns[groupIndex][2], new GUIContent("Variable"));

                            for(int i = 0; i < data.dataList.Count; i++)
                            {
                                currentData = data.dataList[i];
                                EditorGUILayout.LabelField(data.dataList[i].variableName, GUILayout.Width(smallColumnsWidth));
                            }
                        }

                    EditorGUILayout.EndFadeGroup();
                    #endregion

                    #region Delete Button
                    using(new Vertical(GUILayout.Width(25)))
                    {
                        GUILayout.Space(16);

                        for(int i = 0; i < data.dataList.Count; i++)
                            if(GUILayout.Button("X", GUILayout.Width(25)))
                            {
                                DestroyImmediate(data.dataList[i], true);
                                data.dataList.RemoveAt(i);
                            }
                    }
                    #endregion
                }
            }
        }

        EditorGUILayout.EndFadeGroup();
    }
    #endregion
}
