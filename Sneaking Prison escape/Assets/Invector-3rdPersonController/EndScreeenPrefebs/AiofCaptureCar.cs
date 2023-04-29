using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class AiofCaptureCar : MonoBehaviour
{
    public string playerPrebefName;

    public Transform target;
    NavMeshAgent agent;

    Animator animator;
    bool israndomvalue = false;

    //car drifiting tech 
    public Transform Carbody;
    public Quaternion CarBodyRotation;
    public float rotationSpeed = 5.0f;


    //for waypoints if u want

    public bool usingwaypoint;
    public Transform[] waypoints;
    public int currentWaypoint = 0;

    // Start is called before the first frame update
    void Start()
    {
        target = FindObjectOfType<PlayerController>().transform;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        

        
            agent.stoppingDistance = Random.Range(13, 22);
      
        if (usingwaypoint)
        {
            agent.destination = waypoints[currentWaypoint].position;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!usingwaypoint)
        {
            agent.destination = target.transform.position;
            float distance = Vector3.Distance(this.transform.position, target.transform.position);



            if (distance <= agent.stoppingDistance + 5 && !israndomvalue)
            {
                Carbody.rotation = Quaternion.Lerp(Carbody.rotation, CarBodyRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (usingwaypoint)
        {
            if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < agent.stoppingDistance)
            {
                currentWaypoint++;

                if (currentWaypoint >= waypoints.Length)
                {
                  //  currentWaypoint = 0;
                  usingwaypoint = false; 
                }
                if (usingwaypoint)
                    agent.destination = waypoints[currentWaypoint].position;
            }
        }
    }
   
}
