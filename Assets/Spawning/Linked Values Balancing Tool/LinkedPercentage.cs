using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if(UNITY_EDITOR)
using UnityEditor;

[ExecuteInEditMode]
#endif

[System.Serializable]
public class LinkedPercentage : MonoBehaviour
{
#if(UNITY_EDITOR)
    public int popupIndex = -1;//for selecting an existing group
    public string tempGroup, variableName, scriptableObjectPath = "";
    public Object linkedObject;

    static bool quitting = false;
#endif

    public float percentage;

#if(UNITY_EDITOR)
    public void Update()
    {
        if(Application.isPlaying || BuildPipeline.isBuildingPlayer)
            return;//only run in the editor!

        #region Deleted Variables
        if(!linkedObject.Equals(LinkedPercentageManager.instance))//if not attached to the Manager object (as is the case for ScriptableObject's)
        {
            SerializedObject temp = new SerializedObject(linkedObject);

            if(temp.FindProperty(variableName) == null)//basically if you edited code and removed a variable that was a linked percentage
            {
                LinkedPercentageManager.instance.RemoveData(this, popupIndex);//then remove any data stored for it
                Debug.LogError("Removing data for deleted variable: " + variableName + " on object: " + gameObject);
                DestroyImmediate(this);
            }
        }
        #endregion

        if(popupIndex >= 0)
            if(LinkedPercentageManager.instance != null)//this means if there is no manager, don't make a new one (important whilst building)
                if(popupIndex != LinkedPercentageManager.instance.groupNames.Length - 1)//don't run this when trying to add new groups!
                    if(LinkedPercentageManager.Instance.linkedGroups.Count <= popupIndex ||
                        !LinkedPercentageManager.Instance.linkedGroups[popupIndex].dataList.Contains(this))//this checks for copy pasting by inserting new values into the database
                        LinkedPercentageManager.Instance.AddData(this);
    }

    /// <summary>
    /// Called by editor functions to facilitie destroying this object
    /// </summary>
    public void DestroyMeNow()
    {
        LinkedPercentageManager.Instance.RemoveData(this, popupIndex);
        DestroyImmediate(this, true);
    }

    void OnDestroy()
    {
        if(Application.isEditor && !quitting && !EditorApplication.isPlayingOrWillChangePlaymode && !BuildPipeline.isBuildingPlayer)//only run in the editor!
            LinkedPercentageManager.Instance.RemoveData(this, popupIndex);
    }

    void OnApplicationQuit()
    {
        quitting = true;
    }

    public static LinkedPercentage CreateInstance(SerializedProperty property, bool addToGroup)
    {
        MonoBehaviour monoObject = (property.serializedObject.targetObject as MonoBehaviour);
        LinkedPercentage data;

        if(monoObject != null)
        {
            data = (property.serializedObject.targetObject as MonoBehaviour).gameObject.AddComponent<LinkedPercentage>();//adding to the current object
            data.linkedObject = property.serializedObject.targetObject as MonoBehaviour;
        }
        else
        {
            data = LinkedPercentageManager.Instance.gameObject.AddComponent<LinkedPercentage>();//chances are the current object is a scriptable object. So add the data to the manager instead
            data.linkedObject = LinkedPercentageManager.Instance as MonoBehaviour;
            data.scriptableObjectPath = AssetDatabase.GetAssetPath(property.serializedObject.targetObject);

            if(!LinkedPercentageManager.instance.registeredScriptableObjects.Contains(data.scriptableObjectPath))//only add once..
                LinkedPercentageManager.instance.registeredScriptableObjects.Add(data.scriptableObjectPath);//register the scriptable object. This means its values can be updated if it is moved or deleted
        }

        data.variableName = property.propertyPath;//this is important to also record the position within array types

        data.hideFlags = HideFlags.HideInInspector;
        property.objectReferenceValue = data;
        EditorUtility.SetDirty(property.serializedObject.targetObject);

        if(addToGroup)
        {
            LinkedPercentageManager manager = LinkedPercentageManager.Instance;

            if(manager.linkedGroups.Count > 0)//if this object doesnt belong to any groups (just created) and a group exists
            {
                data.popupIndex = data.popupIndex = manager.lastGroupIndex;//set to last used index

                if(manager.lastGroupIndex != manager.groupNames.Length - 1)//if the last index was actually a group (not creating a new one)
                {
                    manager.linkedGroups[manager.lastGroupIndex].dataList.Add(data);//add to the new group
                    data.popupIndex = manager.lastGroupIndex;//make sure to store the new group index!
                }
            }
        }

        return data;
    }

    public static LinkedPercentage CreateInstance(MonoBehaviour monoObject)
    {
        LinkedPercentage data;

        if(monoObject != null)
        {
            data = monoObject.gameObject.AddComponent<LinkedPercentage>();//adding to the current object
            data.linkedObject = monoObject;
        }
        else
        {
            data = LinkedPercentageManager.Instance.gameObject.AddComponent<LinkedPercentage>();//chances are the current object is a scriptable object. So add the data to the manager instead
            data.linkedObject = LinkedPercentageManager.Instance as MonoBehaviour;
            data.scriptableObjectPath = AssetDatabase.GetAssetPath(monoObject);

            if(!LinkedPercentageManager.instance.registeredScriptableObjects.Contains(data.scriptableObjectPath))//only add once..
                LinkedPercentageManager.instance.registeredScriptableObjects.Add(data.scriptableObjectPath);//register the scriptable object. This means its values can be updated if it is moved or deleted
        }

        //data.variableName = property.propertyPath;//this is important to also record the position within array types

        //data.hideFlags = HideFlags.HideInInspector;
        //property.objectReferenceValue = data;
        //EditorUtility.SetDirty(property.serializedObject.targetObject);

        //if(addToGroup)
        //{
        //    LinkedPercentageManager manager = LinkedPercentageManager.Instance;

        //    if(manager.linkedGroups.Count > 0)//if this object doesnt belong to any groups (just created) and a group exists
        //    {
        //        data.popupIndex = data.popupIndex = manager.lastGroupIndex;//set to last used index

        //        if(manager.lastGroupIndex != manager.groupNames.Length - 1)//if the last index was actually a group (not creating a new one)
        //        {
        //            manager.linkedGroups[manager.lastGroupIndex].dataList.Add(data);//add to the new group
        //            data.popupIndex = manager.lastGroupIndex;//make sure to store the new group index!
        //        }
        //    }
        //}

        return data;
    }
#endif
}
