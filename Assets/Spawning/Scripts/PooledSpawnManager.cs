using UnityEngine;
using System.Collections;
using SpawningFramework;//***       Don't forget this!       ***

namespace SpawningFrameworkExamples
{
    /// <summary>
    /// This class serves as an example of how you can implement pooling within your system. It is untested and serves as a theoretical example. 
    /// In order to best optimise/pool your enemies you must consider an individual games needs yourself!
    /// </summary>
    public class PooledSpawnManager : SpawningManager
    {
        public static PooledSpawnManager Instance;

        public GameObject enemyBase;//our defualt prefab for all enemies
        public Mesh defaultEnemy, bronzeEnemy, blueEnemy, redEnemy, archerEnemy;//so by storing the mesh of each enemy here it saves storing a copy for each enemy

        public Transform enemyParent;

        void Start()
        {
            Instance = this;

            InitialiseNewSector();//start running straight away
        }

        protected override void SpawnObject(EnemyTypes.Type type, int index)
        {
            if(enemies[index] == null)//if there is no enemy here
            {
                enemies[index] = (GameObject.Instantiate(enemyBase) as GameObject).GetComponent<PooledEnemy>();
                enemies[index].transform.parent = enemyParent;//its good practise to parent things in your scene to avoid making a mess and make debugging easier
            }

            enemies[index].Initialise();
        }
    }
}