using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class LinkedPercentagePostProcessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        #region Deleting
        for(int i = 0; i < deletedAssets.Length; i++)
        {
            List<int> emptyGroups = new List<int>();
            List<int> destroyedIndexes = new List<int>();
            List<int> destroyedGroupIndex = new List<int>();

            if(LinkedPercentageManager.instance == null)
                return;

            for(int ii = 0; ii < LinkedPercentageManager.Instance.registeredScriptableObjects.Count; ii++)
                if(LinkedPercentageManager.Instance.registeredScriptableObjects[ii].Equals(deletedAssets[i]))//if we have just moved a scriptable object with instances of Linked Percentage
                    for(int j = 0; j < LinkedPercentageManager.instance.linkedGroups.Count; j++)
                        for(int jj = 0; jj < LinkedPercentageManager.instance.linkedGroups[j].dataList.Count; jj++)//loop for all objects
                            if(LinkedPercentageManager.instance.linkedGroups[j].dataList[jj].scriptableObjectPath.Equals(LinkedPercentageManager.Instance.registeredScriptableObjects[ii]))//if any of them had the old path
                            {
                                GameObject.DestroyImmediate(LinkedPercentageManager.instance.linkedGroups[j].dataList[jj], true);//destroy the LinkedPercentage

                                if(!destroyedIndexes.Contains(jj))
                                {
                                    destroyedIndexes.Add(jj);//list all things updated so the registry can be updated
                                    destroyedGroupIndex.Add(j);
                                }

                                if(!emptyGroups.Contains(ii))
                                    emptyGroups.Add(ii);
                            }

            for(int ii = destroyedIndexes.Count - 1; ii > -1; ii--)
                LinkedPercentageManager.instance.linkedGroups[destroyedGroupIndex[ii]].dataList.RemoveAt(destroyedIndexes[ii]);//remove the old data as well

            for(int ii = emptyGroups.Count - 1; ii > -1; ii--)
                LinkedPercentageManager.Instance.registeredScriptableObjects.RemoveAt(emptyGroups[ii]);//and the entry in the list of objects

            if(destroyedIndexes.Count > 0)
                EditorUtility.SetDirty(LinkedPercentageManager.instance);
        }
        #endregion

        #region Moving Assets
        for(var i = 0; i < movedAssets.Length; i++)
        {
            List<int> namesToUpdate = new List<int>();

            for(int ii = 0; ii < LinkedPercentageManager.Instance.registeredScriptableObjects.Count; ii++)
                if(LinkedPercentageManager.Instance.registeredScriptableObjects[ii].Equals(movedFromAssetPaths[i]))//if we have just moved a scriptable object with instances of Linked Percentage
                    for(int j = 0; j < LinkedPercentageManager.instance.linkedGroups.Count; j++)
                        for(int jj = 0; jj < LinkedPercentageManager.instance.linkedGroups[j].dataList.Count; jj++)//loop for all objects
                            if(LinkedPercentageManager.instance.linkedGroups[j].dataList[jj].scriptableObjectPath.Equals(LinkedPercentageManager.Instance.registeredScriptableObjects[ii]))//if any of them had the old path
                            {
                                LinkedPercentageManager.instance.linkedGroups[j].dataList[jj].scriptableObjectPath = movedAssets[i];//update the paths

                                if(!namesToUpdate.Contains(ii))
                                    namesToUpdate.Add(ii);//list all things updated to the registry can be updated
                            }


            for(int ii = 0; ii < namesToUpdate.Count; ii++)
                LinkedPercentageManager.Instance.registeredScriptableObjects[namesToUpdate[ii]] = movedAssets[i];//update the names!

            if(namesToUpdate.Count > 0)
                EditorUtility.SetDirty(LinkedPercentageManager.instance);
        }
        #endregion
    }
}
