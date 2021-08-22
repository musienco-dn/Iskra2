using UnityEngine;
using System.Collections;

namespace SpawningFramework
{
    public class SpawnArea : MonoBehaviour
    {
        #region Variables
#if(UNITY_EDITOR)
        public Color gizmoColour = new Color(1, 0.72f, 0, 0.5f);
#endif

        /// <summary>
        /// The collider which controls where to spawn enemies. Note that this collider doesnt need to be active to spawn enemies!
        /// </summary>
        public Collider collider;

        public enum SphereSpawnType { WithinArea = 0, OnEdges, WithinFlatSphere, OnFlatEdges }
        public SphereSpawnType type;

        public float noRepeatInterval, lastTimeRan;
        #endregion

        void Start()
        {
            if(collider != null)
                collider.enabled = false;//we don't actually need the colliders themselves, only the shape
        }

        public Vector3 GetPosition()
        {
            lastTimeRan = Time.realtimeSinceStartup;//needed for the NoRepeatInterval functions

            if(collider is SphereCollider)
                return SpawnWithinSphere();
            else if(collider is BoxCollider)
                return SpawnWithinBox();
            else
                return transform.position;//if there is no collider just spawn in the same position
        }

        Vector3 SpawnWithinSphere()
        {
            Vector3 position;

            switch(type)
            {
                case SphereSpawnType.WithinArea:
                    return transform.position + Random.insideUnitSphere * ((SphereCollider)collider).radius;//spawn within the spheres radius

                case SphereSpawnType.OnEdges://spawns on the edge of the sphere
                    return transform.position + Random.onUnitSphere * ((SphereCollider)collider).radius;

                case SphereSpawnType.WithinFlatSphere:
                    position = transform.position + Random.insideUnitSphere * ((SphereCollider)collider).radius;//spawn within the spheres radius
                    position.y = transform.position.y;//flatten to a circle
                    return position;//this mode completely ignores the Y values and always spawns on the spheres center. You can think of this as spawn within circle

                case SphereSpawnType.OnFlatEdges://same as above but only spawns on the edge of the sphere rather than inside it
                    return RandomOnCircle(transform.position, ((SphereCollider)collider).radius);

                default:
                    Debug.LogError("Error whilst trying to spawn within circle/sphere");
                    return transform.position;//shouldn't happen
            }
        }

        Vector3 SpawnWithinBox()
        {
            BoxCollider box = (BoxCollider)collider;

            return this.transform.TransformPoint(Vector3.Lerp(box.center - box.size / 2, box.center + box.size / 2, Random.value));//basically calculate the boxes local position and transform it relative to world space once the point has been selected
        }

#if(UNITY_EDITOR)
        public void OnDrawGizmos()
        {
            Gizmos.color = gizmoColour;

            if(collider == null)
                Gizmos.DrawSphere(transform.position, 0.25f);
            else if(collider is SphereCollider)
                Gizmos.DrawWireSphere(transform.position, ((SphereCollider)collider).radius);
            else if(collider is BoxCollider)
            {
                BoxCollider box = (BoxCollider)collider;

                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
#endif

        /// <summary>
        /// Spawn Randomly ona  circle
        /// </summary>
        Vector3 RandomOnCircle(Vector3 center, float radius)
        {
            float angle = Random.Range(0, 360);//first select a random angle
            Vector3 position;
            position.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            position.y = center.y;
            position.z = center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad);

            return position;
        }

        /// <summary>
        /// This helper function is used to read the no repeat interval and spawn accordingly
        /// </summary>
        /// <returns></returns>
        public bool ReadyToSpawn()
        {
            return lastTimeRan == 0 || lastTimeRan + noRepeatInterval < Time.realtimeSinceStartup;//true if this value could be selected
        }
    }
}
