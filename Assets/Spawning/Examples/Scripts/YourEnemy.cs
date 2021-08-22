using UnityEngine;
using System.Collections;
using SpawningFramework;//***       Don't forget this!       ***

namespace SpawningFrameworkExamples
{
    public class YourEnemy : Enemy
    {
        static int currentDepth = 10;

        public bool usePlacementManager;

        /// <summary>
        /// Its important to set the position of your object here rather than on start or awake. Otherwise the methods will run at the wrong times and the last enemy of a wave would try and spawn at the start of the next wave instead!
        /// </summary>
        public override void Initialise()
        {
            base.Initialise();

            transform.position = PlacementManager.Instance.GetSpawnPosition();

            ((SpriteRenderer)GetComponent<Renderer>()).sortingOrder = currentDepth;//bit of a hack to make sure the sprites don't Z fight
            currentDepth++;
        }

        void Update()
        {
            if(transform.position.x > 20)
                Destroy(gameObject);
        }
    }
}