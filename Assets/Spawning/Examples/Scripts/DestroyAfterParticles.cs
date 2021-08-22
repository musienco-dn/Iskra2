using UnityEngine;
using System.Collections;

namespace SpawningFrameworkExamples
{
    /// <summary>
    /// Used to destroy a particle effect but only once all of its particles have faded naturally
    /// </summary>
    public class DestroyAfterParticles : MonoBehaviour
    {
        public ParticleSystem particles;

        void Awake()
        {
            enabled = false;
        }

        public void Initialise()
        {
            enabled = true;
            particles.Stop();
            StartCoroutine(DestroyMe());
        }

        IEnumerator DestroyMe()
        {
            yield return new WaitForSeconds(particles.startLifetime);
            Destroy(gameObject);
        }
    }
}
