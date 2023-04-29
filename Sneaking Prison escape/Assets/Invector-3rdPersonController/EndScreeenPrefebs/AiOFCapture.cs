using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiOFCapture : MonoBehaviour
{
    public bool iscar;
    public Transform target; 
    NavMeshAgent agent; 

    Animator animator;
    bool israndomvalue = false;

    //car drifiting tech 
    public Transform Carbody;
    public Quaternion CarBodyRotation;
    public float rotationSpeed = 5.0f;
    void Start()
    {
       // target = GameObject.Find("PlayerPOstion");
        target = FindObjectOfType<PlayerController>().transform;
      //  target = FindObjectOfType<Transform>().
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (!iscar)
        {
            agent.stoppingDistance = Random.Range(6, 9);
        }
        else
        {
            agent.stoppingDistance = Random.Range(13, 22);
        }
    }

    void Update()
    {
        // Set the destination of the NavMeshAgent to the target point
        agent.destination = target.transform.position;
        float distance = Vector3.Distance(this.transform.position, target.transform.position);

        if (!iscar)
        {
            if (distance <= agent.stoppingDistance && !israndomvalue)
            {
                animator.SetInteger("Aim", Random.Range(2, 20));
                israndomvalue = true;
            }
        }
        if(distance <= agent.stoppingDistance+5 && !israndomvalue)
        {
            Carbody.rotation = Quaternion.Lerp(Carbody.rotation, CarBodyRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
