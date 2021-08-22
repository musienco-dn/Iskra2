using UnityEngine;
using System.Collections;

namespace SpawningFrameworkExamples
{
    public class DestroyAfterTime : MonoBehaviour
    {
        public float time;

        void Start()
        {
            StartCoroutine(DestroyMe());//basically destroy this gameobject after has been alive for X seconds
        }

        IEnumerator DestroyMe()
        {
            yield return new WaitForSeconds(time);

            Destroy(gameObject);
        }
    }
}