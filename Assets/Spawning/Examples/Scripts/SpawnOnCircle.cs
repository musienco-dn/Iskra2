using UnityEngine;
using System.Collections;

namespace SpawningFrameworkExamples
{
    public class SpawnOnCircle : MonoBehaviour
    {
        public Vector3 centerPoint;
        public float radius;

        public void Spawn()
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));//select a random angle
            transform.position = centerPoint + transform.forward * radius;//and project forward

            //transform.position = Random.onUnitSphere * radius;//use this to spawn on the edge of a sphere shape instead

            transform.LookAt(centerPoint);//now face the middle
        }
    }
}