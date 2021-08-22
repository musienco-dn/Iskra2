#if(UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class LinkedPercentageManager : MonoBehaviour
{
    public static LinkedPercentageManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = Resources.Load("Linked Percentage Database", typeof(LinkedPercentageManager)) as LinkedPercentageManager;

                if(instance == null)//if there is no manager!
                {
                    Debug.LogError("Failed to find Linked Percentage Database! Creating new one");//if you see this frequently then make sure the path below is correct

                    //"Assets/Spawning/Linked Values Balancing Tool/Resources/Linked Percentage Database.prefab"

                    CreateDataBase("Assets/Spawning/Linked Values Balancing Tool/Resources/Linked Percentage Database.prefab");//*** this path *** Once your database is created it can sit in any 'Resources' folder
                }
            }

            return instance;
        }
    }

    public static LinkedPercentageManager instance;

    [SerializeField]
    public List<LinkedPercentageData> linkedGroups = new List<LinkedPercentageData>();
    public string[] groupNames = new string[] { "New Group" };
    public int lastGroupIndex;

    public List<string> registeredScriptableObjects = new List<string>();

    /// <summary>
    /// Attempts to create the database object in a given resources folder
    /// </summary>
    /// <param name="path"></param>
    static void CreateDataBase(string path)
    {
        GameObject temp = new GameObject();
        temp.name = "Linked Percentage Manager";
        temp.AddComponent<LinkedPercentageManager>();

        //if(!Directory.Exists(path))//This seems to cause some serious issues! Throws up lots of moving file failed errors. If anyone knows a less glitchy way of creating directories please let us know!
        //    Directory.CreateDirectory(path);//if the directory doesn't exist, make it

        PrefabUtility.CreatePrefab(path, temp);

        DestroyImmediate(temp);

        instance = Resources.Load("Linked Percentage Database", typeof(LinkedPercentageManager)) as LinkedPercentageManager;
    }

    public void RemoveData(LinkedPercentage data, int previousPopUp)
    {
        if(previousPopUp > -1 && previousPopUp < linkedGroups.Count)
            linkedGroups[previousPopUp].dataList.Remove(data);//remove the value
    }

    public void AddData(LinkedPercentage data)
    {
        if(data.popupIndex < linkedGroups.Count && linkedGroups.Count > 0)
        {
            linkedGroups[data.popupIndex].dataList.Add(data);
            lastGroupIndex = data.popupIndex;
        }
        else//if actually part of a group that doesnt exist yet
            data.popupIndex = -1;
    }

    public void AddNewGroup(string newgroup)
    {
        System.Array.Resize<string>(ref groupNames, groupNames.Length + 1);//resize the list of group names

        groupNames[groupNames.Length - 2] = newgroup;//add the new group to the list of groups
        groupNames[groupNames.Length - 1] = "New Group";
    }
}

[System.Serializable]
public class LinkedPercentageData
{
    /// <summary>
    /// This is important for serialization since Unity struggles with nested arrays/lists
    /// </summary>
    public List<LinkedPercentage> dataList = new List<LinkedPercentage>();
}
#endif