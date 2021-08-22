using UnityEngine;
using System.Collections;
using SpawningFramework;//***       Don't forget this!       ***

namespace SpawningFrameworkExamples
{
    public class YourSpawnManager : SpawningManager
    {
        public GameObject blueAsteroid, blueAsteroid2, brownAsteroid, bigBlueAsteroid, bigBrownAsteroid;

        public Transform enemyParent;

        void Start()
        {
#if(UNITY_EDITOR)//this ensures the following method only runs in the editor and wont run for your final builds
            CheckValues();//this run a series of checks to ensure your waves have the correct values set. Helps avoid errors!
#endif

            waveGroupFinished += GroupFinished;//this hooks up your method to the delegate. Essentially whatever method you pass here will run when a wave has finished
            waveFinished += WaveFinished;
            allGroupsFinished += AllGroupsFinished;

            InitialiseNewSector();//start running straight away
        }

        void OnEnable()
        {
            InitialiseNewSector();//start running again when awoken
        }

        /// <summary>
        /// An example delegate called when waves are finished
        /// </summary>
        void WaveFinished()
        {
            Debug.Log("Wave Finished");
            //Your code here!
        }

        void GroupFinished()
        {
            Debug.Log("Wave group finished");
            //Your code here!
        }

        void AllGroupsFinished()
        {
            Debug.Log("All waves finished");
            //Your code here!

            TestPlayerSkill.runFinished = true;//this is a hack for the web demo. Feel free to remove it (it lets the GUI know to display the restart button)
        }

        protected override void SpawnObject(EnemyTypes.Type type, int index)
        {
            //Debug.Log("Spawning: " + type);

            GameObject enemy = null;//Example code!

            switch(type)
            {
                case EnemyTypes.Type.BigBlueAsteroid:
                    enemy = (GameObject)GameObject.Instantiate(bigBlueAsteroid);//note this code is not efficient as is only inteded for learning the framework. Once youa re more familiar with it have a look at the pooling example.
                    break;

                case EnemyTypes.Type.BrownAsteroid:
                    enemy = (GameObject)GameObject.Instantiate(brownAsteroid);
                    break;

                case EnemyTypes.Type.BlueAsteroid2:
                    enemy = (GameObject)GameObject.Instantiate(blueAsteroid2);
                    break;

                case EnemyTypes.Type.BlueAsteroid:
                    enemy = (GameObject)GameObject.Instantiate(blueAsteroid);
                    break;

                case EnemyTypes.Type.BigBrownAsteroid:
                    enemy = (GameObject)GameObject.Instantiate(bigBrownAsteroid);
                    break;

                default:
                    Debug.LogError("Enemy Model not assigned: " + type);
                    break;
            }

            enemies[index] = (YourEnemy)enemy.GetComponent(typeof(YourEnemy));//this part is important if you want to track enemies outwith standard collisions. E.G casting a spell that effects all enemies
            //its also important for linking with the Placement manager and your own methods. Make sure this is YourEnemy and not the base class enemy!

            enemy.transform.parent = enemyParent;//its good practise to parent things in your scene to avoid making a mess and make debugging easier
        }
    }
}