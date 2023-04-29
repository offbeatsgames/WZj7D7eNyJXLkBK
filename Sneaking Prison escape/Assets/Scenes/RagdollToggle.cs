using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollToggle : MonoBehaviour {

    protected Animator Animator;
    protected Rigidbody Rigidbody;

    public Collider MainCollider;
   
    public Transform Root;
    Quaternion NormalRotation;
    

    protected Collider[] ChildrenCollider;
    protected Rigidbody[] ChildrenRigidbody;

    

    public bool Dead;

    // Use this for initialization
    void Awake ()
    {
        NormalRotation = Root.rotation;
        Animator = GetComponent<Animator>();
       
       
        //Bear = GetComponent<Bear>();

        ChildrenCollider = GetComponentsInChildren<Collider>();
        ChildrenRigidbody = GetComponentsInChildren<Rigidbody>();
        
        MainCollider = GetComponent<Collider>();
    }

    void Start()
    {
        RagdollActive(false);
    }

    // Update is called once per frame

    public void RagdollActive(bool active)
    {
        if(MainCollider != null)
        {
            MainCollider.enabled = !active;

        //children
            foreach (var collider in ChildrenCollider)
                collider.enabled = active;
            foreach (var rigidbody in ChildrenRigidbody)
            {
                rigidbody.detectCollisions = active;
                rigidbody.isKinematic = !active;
            }
           
            //root
            Animator.enabled = !active;
        
            MainCollider.enabled = !active;

        }

        
       // Rigidbody.detectCollisions = !active;
      //  Rigidbody.isKinematic = active;
        //BoxCollider.enabled = !active;

    }


    public void ResetPostion()
    {
        Root.rotation = Quaternion.Slerp(Root.rotation,NormalRotation,2f * Time.deltaTime);
    }

   




}
