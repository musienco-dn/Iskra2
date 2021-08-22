using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SpawningFramework
{
    [System.Serializable]
    public class PlacementManager : MonoBehaviour
    {
        #region Variables
        public static PlacementManager Instance;

        public enum Type { SpawnRandomlyPerObject, SpawnRandomlyPerWave, SpawnSequentiallyPerObject, SpawnSequentiallyPerWave }

        public SpawningManager spawnManager;//which manager is this linked with

        public SpawnGroup[] spawnGroups;//where to spawn enemies broken down to groups and waves

        int currentSpawner = -1;
        #endregion

        void Start()
        {
            Instance = this;

            spawnManager.waveFinished += WaveFinished;
            spawnManager.waveGroupFinished += WaveGroupFinished;//hook up the delegates to call these methods at the right times

            Reset();

#if(UNITY_EDITOR)
            for (int i = 0; i < spawnGroups.Length; i++)
                if (spawnGroups[i].spawnAreas.Length == 0)
                    Debug.LogError("No valid spawn areas for group " + (i + 1));

            if (spawnGroups.Length != spawnManager.groups.Length)
                Debug.LogError("The number of spawn groups doesnt match the number of placement ones! Check your placement groups as this will produce errors!: " + gameObject);
#endif
        }

        /// <summary>
        /// Add any other reset logic here. Most of the work is done in the spawn manager since its values are read directly
        /// </summary>
        public void Reset()
        {
            currentSpawner = -1;

            if (spawnGroups[spawnManager.currentGroupIndex].type == Type.SpawnRandomlyPerWave || spawnGroups[spawnManager.currentGroupIndex].type == Type.SpawnSequentiallyPerWave)
                WaveFinished();//selects the first spawn index properly
        }

        public void WaveGroupFinished()
        {
            currentSpawner = -1;//this is important for sequential waves
        }

        public void WaveFinished()
        {
            switch (spawnGroups[spawnManager.currentGroupIndex].type)
            {
                case Type.SpawnRandomlyPerWave:
                    currentSpawner = SelectRandomIndex();//choose the next spawn area randomly
                    spawnGroups[spawnManager.currentGroupIndex].spawnAreas[currentSpawner].lastTimeRan = Time.realtimeSinceStartup;
                    break;

                case Type.SpawnSequentiallyPerWave:
                    currentSpawner = (currentSpawner + 1) % spawnGroups[spawnManager.currentGroupIndex].spawnAreas.Length;//loop through all spawners
                    break;
            }
        }

        /// <summary>
        /// Calculates the next spawn position. Only call this once per enemy or you will get varying results
        /// </summary>
        /// <returns></returns>
        public Vector3 GetSpawnPosition()
        {
            if (spawnGroups[spawnManager.currentGroupIndex].spawnAreas.Length == 0)
                return spawnGroups[spawnManager.currentGroupIndex].spawnAreas[0].GetPosition();//if there is just one spawner then ignore the type and spawn everything inside it

            switch (spawnGroups[spawnManager.currentGroupIndex].type)
            {
                case Type.SpawnRandomlyPerObject:
                    currentSpawner = SelectRandomIndex();//choose the next spawn area randomly
                    break;

                case Type.SpawnSequentiallyPerObject:
                    int runCount = 0;

                    do
                    {
                        currentSpawner = (currentSpawner + 1) % spawnGroups[spawnManager.currentGroupIndex].spawnAreas.Length;//loop through all spawners
                        runCount++;

                        if (spawnGroups[spawnManager.currentGroupIndex].spawnAreas[currentSpawner].ReadyToSpawn())
                           return spawnGroups[spawnManager.currentGroupIndex].spawnAreas[currentSpawner].GetPosition();//if ready to spawn then pass back the positions
                    }
                    while (runCount < spawnGroups[spawnManager.currentGroupIndex].spawnAreas.Length);//only run for as many groups as we have

                    Debug.LogError("No valid spawn area found! Check your NoRepeatIntervals are not too high: " + gameObject);
                    break;
            }

            return spawnGroups[spawnManager.currentGroupIndex].spawnAreas[currentSpawner].GetPosition();
        }

        /// <summary>
        /// A helper to avoid stack overflows whilst attempting to select from a random number range
        /// </summary>
        public int SelectRandomIndex()
        {
            List<int> possibleValues = new List<int>();
            int returnValue;

            for (int i = 0; i < spawnGroups[spawnManager.currentGroupIndex].spawnAreas.Length; i++)
                if (spawnGroups[spawnManager.currentGroupIndex].spawnAreas[i].ReadyToSpawn())//if this value could be selected
                    possibleValues.Add(i);//show we can select this value

            if (possibleValues.Count == 0)
            {
                Debug.LogError("No possible values could be selected! Try lowering your no repeat intervals on: " + spawnGroups[spawnManager.currentGroupIndex].gameObject + "\nSpawning the first group as a fallback");
                return 0;
            }

            returnValue = possibleValues[Random.Range(0, possibleValues.Count)];
            spawnGroups[spawnManager.currentGroupIndex].spawnAreas[returnValue].lastTimeRan = Time.realtimeSinceStartup;//record this area has been selected

            return returnValue;
        }
    }
}