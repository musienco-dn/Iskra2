using UnityEngine;
using System.Collections;
using SpawningFramework;

namespace SpawningFrameworkExamples
{
    public class ResumeOnCollide : MonoBehaviour
    {
       public SpawningManager manager;

        void OnTriggerEnter(Collider other)
        {
            manager.Resume();
            gameObject.SetActive(false);//only run once
            //Debug.Log("Triggered");
        }
    }
}