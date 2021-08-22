using System.Collections;
using UnityEngine;

namespace SpawningFramework
{
    public class Stats : MonoBehaviour
    {
        #region Stats
        public static Stats Instance;

        public int[] enemiesSpawned, enemiesKilled, enemiesKilledAllTime;
        #endregion

        void Start()
        {
            Instance = this;
        }

        /// <summary>
        /// Softly resets stats for each new play
        /// </summary>
        public void Reset()
        {
            enemiesSpawned = new int[(int)EnemyTypes.Type.Last];
            enemiesKilled = new int[(int)EnemyTypes.Type.Last];
            enemiesKilledAllTime = new int[(int)EnemyTypes.Type.Last];
        }

        public void LoadStats()
        {
            enemiesKilledAllTime = K2Saving.GetIntArray("enemiesKilledAllTime");
        }

        public void RegisterGameFinished()
        {
            for(int i = 0; i < enemiesKilledAllTime.Length; i++)
                enemiesKilledAllTime[i] += enemiesKilled[i];

            K2Saving.SetIntArray("enemiesKilledAllTime", enemiesKilledAllTime);
        }
    }
}