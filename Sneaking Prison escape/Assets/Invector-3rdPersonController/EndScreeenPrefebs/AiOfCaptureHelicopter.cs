using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiOfCaptureHelicopter : MonoBehaviour
{
    public string playerPrebefName;
    public Transform target;
    NavMeshAgent agent;
    
    Animator animator;
    bool israndomvalue = false;

   

    // Start is called before the first frame update
    void Start()
    {
        target = FindObjectOfType<PlayerController>().transform;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.stoppingDistance = Random.Range(13, 22);

    }

    // Update is called once per frame
    void Update()
    {
        agent.destination = target.transform.position;
        float distance = Vector3.Distance(this.transform.position, target.transform.position);



    }
}
