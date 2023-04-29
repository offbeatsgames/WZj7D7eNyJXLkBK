using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRopeDetecter : MonoBehaviour
{
    public LayerMask layerAsRope;
    public Transform ropeCheckPoint;
    public float checkDistance = 1;
    public float ropeMoveUpSpeed = 2;
    public Vector2 offsetPlayer = new Vector2(-0.5f, 1.5f);
    public float swingForce = 10;
    public float jumpOutForce = 10;

    [ReadOnly] public RaycastHit ropeHit;
    [ReadOnly] public bool isHoldingRope = false;
    [ReadOnly] public bool isDetectedRope = false;
    [ReadOnly] public GameObject currentRope;
    [ReadOnly] public GameObject ropeRootParent;
    [ReadOnly] public GameObject catchKnot;
    public float allowCatchRopeDelay = 0.1f;
    [ReadOnly] public bool reachLimitUp = false;

    float cooldownRight = 0.75f;
    float cooldownRightCounter = 0;

    float cooldownLeft = 0.75f;
    float cooldownLeftCounter = 0;

    public void AddForceToRope(Vector3 force, float direction)
    {
        if (direction == 1 && cooldownRightCounter > 0)
        {
            currentRope.GetComponent<Rigidbody>().AddForce(force, ForceMode.Acceleration);
        }
        else if(direction == -1 && cooldownLeftCounter > 0)
        {
            currentRope.GetComponent<Rigidbody>().AddForce(force, ForceMode.Acceleration);
        }
    }

    public void SwingRight()
    {
        cooldownRightCounter = cooldownRight;
    }

    public void SwingLeft()
    {
        cooldownLeftCounter = cooldownLeft;
    }

    public void CheckRope(Vector2 direction)
    {
        if (Physics.Raycast(ropeCheckPoint.position, direction, out ropeHit, checkDistance, layerAsRope))
        {
            if (ropeRootParent == null || (ropeRootParent != null && (ropeRootParent != ropeHit.collider.transform.root.gameObject)))        //only catch the rope if it different with the last one
            {
                isDetectedRope = true;
                currentRope = ropeHit.collider.gameObject;
                currentRope.GetComponent<SphereCollider>().isTrigger = false;
                ropeRootParent = currentRope.transform.root.gameObject;
                SetKnot();
            }
        }
        else
            isDetectedRope = false;
    }

    private void Update()
    {
        if (cooldownRightCounter > 0)
            cooldownRightCounter -= Time.deltaTime;

        if (cooldownLeftCounter > 0)
            cooldownLeftCounter -= Time.deltaTime;
    }

    public void SetKnot()
    {
        if (catchKnot == null)
        {
            catchKnot = new GameObject();
            catchKnot.name = "Catch Knot";
        }

        catchKnot.transform.position = currentRope.transform.position;
        catchKnot.transform.parent = currentRope.transform;
        catchKnot.transform.up = currentRope.transform.up;
    }

    public void MoveUp()
    {
        //make the catch knot center of the rope
        var newPos = catchKnot.transform.localPosition;
        newPos.z = 0;
        catchKnot.transform.localPosition = newPos;

        if (currentRope.transform.parent != null && currentRope.transform.parent.GetComponent<HingeJoint>())
        {
            reachLimitUp = false;
            if (Vector2.Distance(catchKnot.transform.position, currentRope.transform.position) > Vector2.Distance(catchKnot.transform.position, currentRope.transform.parent.position))
            {
                currentRope.GetComponent<SphereCollider>().isTrigger = true;
                var newParent = currentRope.transform.parent.gameObject;
                currentRope = newParent;
                currentRope.GetComponent<SphereCollider>().isTrigger = false;
                currentRope.GetComponent<Rigidbody>().velocity = Vector2.zero ;

                catchKnot.transform.parent = currentRope.transform;
                catchKnot.transform.up = currentRope.transform.up;
            }
            else
                catchKnot.transform.Translate(0, ropeMoveUpSpeed * Time.deltaTime * -1, 0, Space.Self);
        }
        else
            reachLimitUp = true;
    }

    public void MoveDown()
    {
        catchKnot.transform.Translate(0, ropeMoveUpSpeed * Time.deltaTime, 0, Space.Self);
        reachLimitUp = false;
        //make the catch knot center of the rope
        var newPos = catchKnot.transform.localPosition;
        newPos.z = 0;
        catchKnot.transform.localPosition = newPos;

        if (currentRope.transform.childCount > 0 && currentRope.transform.GetChild(0).GetComponent<HingeJoint>()) {
            if (Vector2.Distance(catchKnot.transform.position, currentRope.transform.position) > Vector2.Distance(catchKnot.transform.position, currentRope.transform.GetChild(0).transform.position))
            {
                currentRope.GetComponent<SphereCollider>().isTrigger = true;
                var newParent = currentRope.transform.GetChild(0).gameObject;
                currentRope = newParent;
                currentRope.GetComponent<SphereCollider>().isTrigger = false;

                catchKnot.transform.parent = currentRope.transform;
                catchKnot.transform.up = currentRope.transform.up;
            }
        }
        else
        {
            if (Vector2.Distance(catchKnot.transform.position, currentRope.transform.position) > currentRope.GetComponent<SphereCollider>().radius)
            {
                GameManager.Instance.Player.DropOutClimbableRope();
                JumpOut();
            }
        }
    }

    public void JumpOut()
    {
        currentRope.GetComponent<SphereCollider>().isTrigger = true;
        isDetectedRope = false;
        isHoldingRope = false;
    }

    public void ResetRope()
    {
        ropeRootParent = null;
    }
}
