using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Helicopter : MonoBehaviour
{
  
    private Transform target;
  


    void Start()
    {
        target = FindObjectOfType<PlayerController>().transform;

       
        
    }

    void Update()
    {
        if(target != null)
            transform.position = Vector3.MoveTowards(transform.position,target.position, 5 * Time.deltaTime);
        else
            target = FindObjectOfType<PlayerController>().transform;
        
    }


    

}
