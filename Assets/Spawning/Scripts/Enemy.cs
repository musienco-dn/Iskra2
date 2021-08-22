using UnityEngine;
using System.Collections;

namespace SpawningFramework
{
    /// <summary>
    /// An example enemy class
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        public EnemyTypes.Type type;

        public float MaxHealth;
        float health;

        [HideInInspector]
        public bool dying;

        /// <summary>
        /// Called when this enemy has been spawned
        /// </summary>
        public virtual void Initialise()
        {
            health = MaxHealth * SpawningManager.Instance.CustomValue;//adjusts health according to the given custom value

            if(Stats.Instance != null)
                Stats.Instance.enemiesSpawned[(int)type]++;
        }

        /// <summary>
        /// Called when the player loses
        /// </summary>
        public virtual void OnGameOver()
        {
        }

        #region Taking Damage
        /// <summary>
        /// Hurts the enemy and returns if it dies or not
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public virtual bool Hurt(float damage)
        {
            if(dying)
                return false;//already run 

            health -= damage;//hurt the enemy

            if(health < 0.1)
            {
                dying = true;
                SpawningManager.Instance.enemiesAlive--;

                gameObject.SetActive(false);//this tells the pool that this enemy is now ready to be replaced

                //Here is where you handle your dying animation

                return true;
            }

            return health < 0.1;//return if this will die
        }

        /// <summary>
        /// Hurts the enemy from the given center. Returns if it died
        /// </summary>
        /// <param name="epicenter"></param>
        /// <returns></returns>
        public bool HurtWithExplosion(Vector3 epicenter, float radiusSquared, float power)
        {
            float distSq = Vector3.SqrMagnitude(epicenter - transform.position);//distance squared is more efficient than distance since it avoids calling square root!

            if(distSq < radiusSquared)//if in the explosions range
            {
                float timeTillHurt = K2Maths.Percentage(distSq, radiusSquared) * 0.25f;//The final number represents the time it takes to fully expand
                float explosionDamage = power * K2Maths.Percentage(distSq, radiusSquared);//hurt the enemy in relation to its distance from the epi-center

                if(Hurt(explosionDamage))//hurt returns true if the enemy has died
                    return dying;//dying will now be true
            }

            return false;
        }

        public bool CheckIfInRadius(Vector3 epicenter, float radiusSquared)
        {
            float distSq = Vector3.SqrMagnitude(epicenter - (transform.position + ((SphereCollider)gameObject.GetComponent<Collider>()).center));//bounding sphere center is important for asteroids since they project their transform.position!

            return distSq < radiusSquared;
        }
        #endregion
    }
}
