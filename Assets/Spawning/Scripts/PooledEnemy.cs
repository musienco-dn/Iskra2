using UnityEngine;
using System.Collections;
using SpawningFramework;//***       Don't forget this!       ***

namespace SpawningFrameworkExamples
{
    public class PooledEnemy : Enemy
    {
        public MeshFilter meshFilter;

        /// <summary>
        /// Called when this enemy has been spawned. Here is where the real pooling happens
        /// </summary>
        public virtual void Initialise()
        {
            switch(type)
            {
                case EnemyTypes.Type.BigBlueAsteroid:
                    meshFilter.mesh = PooledSpawnManager.Instance.blueEnemy;//and here we swap out the meshes as needed
                    break;

                case EnemyTypes.Type.BrownAsteroid:
                    meshFilter.mesh = PooledSpawnManager.Instance.redEnemy;
                    break;

                case EnemyTypes.Type.BlueAsteroid2:
                    meshFilter.mesh = PooledSpawnManager.Instance.bronzeEnemy;
                    break;

                case EnemyTypes.Type.BlueAsteroid:
                    meshFilter.mesh = PooledSpawnManager.Instance.defaultEnemy;
                    break;

                case EnemyTypes.Type.BigBrownAsteroid:
                    meshFilter.mesh = PooledSpawnManager.Instance.archerEnemy;
                    break;

                default:
                    Debug.LogError("Enemy Model not assigned: " + type);
                    break;
            } 
            
            gameObject.SetActive(true);//this way you avoided calling instantiate or destroy and instead just swap out the mesh of each enemy as needed. Thus avoiding a lot of garbage calls

            if(Stats.Instance != null)
                Stats.Instance.enemiesSpawned[(int)type]++;
        }
    }
}