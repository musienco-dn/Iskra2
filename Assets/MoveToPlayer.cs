using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveToPlayer: MonoBehaviour
{
    //Transform that NPC has to follow
    public Transform transformToFollow;
    //NavMesh Agent variable
    NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //Follow the player
        agent.destination = transformToFollow.position;
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("lel");

            agent.destination = Vector3.zero;
        }
    }
}