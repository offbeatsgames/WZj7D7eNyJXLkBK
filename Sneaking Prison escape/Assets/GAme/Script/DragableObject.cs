using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragableObject : MonoBehaviour
{
    [ReadOnly] public bool isDragable = true;
    [ReadOnly] public float checkGroundDistance = 1.5f;
    Rigidbody rig;

    private IEnumerator Start()
    {
        rig = GetComponent<Rigidbody>();
        StartCoroutine(CheckGroundCo());

        //allow the object on the ground correctly
        rig.isKinematic = false;
        yield return new WaitForSeconds(1);
        rig.isKinematic = true;
    }

    IEnumerator CheckGroundCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            isDragable = Physics.Raycast(transform.position, Vector3.down, checkGroundDistance);
            if (!isDragable)
            {
                rig.isKinematic = false;
                rig.freezeRotation = false;
                StopAllCoroutines();
            }
        }
    }

    public void Pull(Vector3 deltaPos)
    {
        if (!isDragable)
            return;

        rig.isKinematic = true;
        rig.freezeRotation = true;
        transform.position += deltaPos;
        //rig.velocity = deltaPos;
    }

    public void Push(Vector3 velocity)
    {
        if (!isDragable)
            return;
        //rig.isKinematic = true;
        //transform.position += velocity;
        rig.isKinematic = false;
        rig.freezeRotation = true;
        rig.velocity = velocity;
    }
}
