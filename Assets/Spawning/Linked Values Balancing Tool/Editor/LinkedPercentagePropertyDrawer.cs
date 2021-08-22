using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(LinkedPercentage))]
public class LinkedPercentagePropertyDrawer : PropertyDrawer
{
    static LinkedPercentageManager manager;

    const float SaveDelay = 0.25f;//if we save every time we move the slider you always see the loading icon which is a bit annoying and inneficient. This helps delay that
    float saveDelayTimer;

    LinkedPercentage data;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        #region Initialise
        manager = LinkedPercentageManager.Instance;

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        #region First Run Checks
        data = property.objectReferenceValue as LinkedPercentage;

        if(data == null || !data.variableName.Equals(property.propertyPath))//the second half is important to check if new valueshave been added to array objects
            data = LinkedPercentage.CreateInstance(property, false);

        #region Set First Group
        int popupIndex = data.popupIndex;//which group do you belong to

        if(popupIndex == -1 && manager.linkedGroups.Count > 0)//if this object doesnt belong to any groups (just created) and a group exists
        {
            popupIndex = data.popupIndex = manager.lastGroupIndex;//set to last used index

            if(manager.lastGroupIndex != manager.groupNames.Length - 1)//if the last index was actually a group (not creating a new one)
                SelectNewGroup(-1, manager.lastGroupIndex);//add the new data to that group
        }
        #endregion
        #endregion
        #endregion

        #region Create Group
        if(popupIndex == manager.groupNames.Length - 1 || popupIndex == -1)//if creating a new group
        {
            int newGroupIndex = manager.groupNames.Length - 1;//start by selecting the last item (New Group)
            newGroupIndex = EditorGUI.Popup(new Rect(position.x, position.y, position.width / 3, position.height), newGroupIndex, manager.groupNames);
            data.tempGroup = EditorGUI.TextField(new Rect(position.x + position.width / 3, position.y, position.width / 3, position.height), GUIContent.none, data.tempGroup);

            #region Selecting Group
            if(newGroupIndex != manager.groupNames.Length - 1)//if the player selected anything that wasnt a new group
                SelectNewGroup(-1, newGroupIndex);
            #endregion

            #region Button: Create New Group
            if(GUI.Button(new Rect(position.x + (position.width * 0.667f), position.y, position.width / 3, position.height), new GUIContent("Create")))
                AddNewGroup(data);//add the new group name to the list of current groups
            #endregion
        }
        #endregion

        #region Main Display
        else
        {
            float labelWidth = GUI.skin.label.CalcSize(new GUIContent("100%")).x;//this is the max size of the label, used to position properly

            #region Slider
            float previous = data.percentage;
            float percentage = GUI.HorizontalSlider(new Rect(position.x, position.y, position.width * 0.5f - 4 - labelWidth / 2, position.height), data.percentage, 0, 100);
            SetPercentage(ref data, percentage);

            if(percentage != previous)
                saveDelayTimer = SaveDelay;//delay saving until the user stops editing values
            #endregion

            EditorGUI.LabelField(new Rect(position.x + position.width * 0.5f - labelWidth / 2, position.y, labelWidth, position.height), Mathf.RoundToInt(percentage) + "%");

            int previousIndex = popupIndex;
            popupIndex = EditorGUI.Popup(new Rect(position.x + position.width * 0.5f + labelWidth / 2, position.y, position.width * 0.5f - labelWidth, position.height), popupIndex, manager.groupNames);

            if(popupIndex != previousIndex)//if the player selected something else
            {
                if(popupIndex == manager.groupNames.Length - 1)//if the player selected new group (the last index)
                    RemoveFromGroup(previousIndex);//remove this from any existing lists
                else
                    SelectNewGroup(previousIndex, popupIndex);
            }

            data.popupIndex = popupIndex;
        }
        #endregion

        EditorGUI.indentLevel = indent;// Set indent back to what it was

        #region Finalise and Save
        if(GUI.changed)
        {
            property.serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(data, "Linked Percentage");//very important! This marks the scene as dirty and records that changes need saved
        }
        #endregion

        EditorGUI.EndProperty();

        //property.serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(property.serializedObject.targetObject); // Repaint 
    }

    #region Set Percentage
    public static void SetPercentage(ref LinkedPercentage data, float percentage)
    {
        manager = LinkedPercentageManager.Instance;//this is important if called externally

        #region Set Current Value
        data.percentage = percentage;

        int index = data.popupIndex;
        #endregion

        try
        {
            float total = 0;

            for(int i = 0; i < manager.linkedGroups[index].dataList.Count; i++)
                total += manager.linkedGroups[index].dataList[i].percentage;

            if(total > 0)
                for(int i = 0; i < manager.linkedGroups[index].dataList.Count; i++)//loop again
                    manager.linkedGroups[index].dataList[i].percentage = ((manager.linkedGroups[index].dataList[i].percentage / total) * 100);//and set the relative value
        }
        catch(System.Exception e)
        {
            LinkedPercentageWindow.RefreshData();//if there is an error chances are its null data, so this removes it. This can happen after each build!
        }
    }
    #endregion

    #region Group Functions
    /// <summary>
    /// Called when the user selects a new group, switches the current instance onto the list of the new group
    /// </summary>
    void SelectNewGroup(int previousGroupIndex, int newGroupIndex)
    {
        if(previousGroupIndex != -1)//-1 isn't a new object that doesnt have a group yet
            manager.linkedGroups[previousGroupIndex].dataList.Remove(data);//remove this value from the data group

        manager.linkedGroups[newGroupIndex].dataList.Add(data);//add to the new group

        data.popupIndex = newGroupIndex;//make sure to store the new group index!

        manager.lastGroupIndex = newGroupIndex;//store the last group used to help UX flow
    }

    void RemoveFromGroup(int previousGroupIndex)
    {
        if(previousGroupIndex != -1)//-1 isn't a new object that doesnt have a group yet
            manager.linkedGroups[previousGroupIndex].dataList.Remove(data);//remove this value from the data group
    }

    void AddNewGroup(LinkedPercentage data)
    {
        LinkedPercentageData temp = new LinkedPercentageData();//create a new group
        temp.dataList.Add(data);//add the current item to the group

        System.Array.Resize<string>(ref manager.groupNames, manager.groupNames.Length + 1);//resize the list of group names

        manager.groupNames[manager.groupNames.Length - 2] = data.tempGroup;//add the new group to the list of groups
        manager.groupNames[manager.groupNames.Length - 1] = "New Group";

        manager.linkedGroups.Add(temp);//add our list to the list of groups

        manager.lastGroupIndex = manager.groupNames.Length - 2;//show this group was the last one used
        data.popupIndex = manager.lastGroupIndex;//set the group index to the last one used
        data.percentage = 100;//since we are adding a new group then we want a 100% chance
    }
    #endregion
}
