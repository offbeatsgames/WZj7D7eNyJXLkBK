using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckLadderZone : MonoBehaviour
{
    public LayerMask layerAsLadder;
    public float ladderMoveSpeed = 2;
    public float droppingToLadderTime = 1;      //time dropping from the ledge to ladder below
    //public Transform checkLadderBelow;
    [Header("---Rootbone Rotate Controll---")]
    public Transform rootBone;
    public Vector3 rootBoneRotateOffset = new Vector3(-10, 0, 0);
    public Vector3 rootBonePositionOffset = new Vector3(0, 0, 0.12f);

    [ReadOnly] public RaycastHit ladderHit;
    [ReadOnly] public Vector2 ladderNormal;
    [ReadOnly] public bool isInLadderZone = false;
    [ReadOnly] public bool isHasLadderBelow = false;
    [ReadOnly] public RaycastHit ladderBelowHit;

    public bool isContactLadder(CharacterController characterController)
    {
        if (GameManager.Instance.Player.characterController == null)
            return false;
        //var _temp = isInLadderZone;

        if (isInLadderZone)
            return true;

        isInLadderZone = false;

        Vector3 p1 = transform.position + GameManager.Instance.Player.characterController.center + Vector3.up * -GameManager.Instance.Player.characterController.height * 0.5F + Vector3.up * 0.5f;
        Vector3 p2 = p1 + Vector3.up * GameManager.Instance.Player.characterController.height + Vector3.up * -0.5f;
        float distanceToObstacle = 0;

        // Cast character controller shape 10 meters forward to see if it is about to hit anything.
        if (Physics.CapsuleCast(p1, p2, GameManager.Instance.Player.characterController.radius, transform.forward, out ladderHit, 10, layerAsLadder))
        {
            ladderNormal = ladderHit.normal;
            //Debug.LogError("distance to ladder = " + ladderHit.distance);
            distanceToObstacle = ladderHit.distance;
            if (distanceToObstacle < 0.1f)
            {
                //if (Physics.Raycast(p2, transform.forward, 1, layerAsLadder))
                isInLadderZone = true;
            }
            Debug.DrawRay(ladderHit.point, ladderHit.normal * 10);
        }

        //if (_temp && !isInLadderZone)
        //{
        //    Debug.LogError()
        //    Debug.Break();
        //}

        return isInLadderZone;
    }

    public bool CheckLadderBelow(Vector3 checkPos, Vector3 direction, float distance)
    {

        isHasLadderBelow = false;
        Debug.DrawRay(checkPos, direction * distance);
        if (Physics.Raycast(checkPos, direction, out ladderBelowHit, distance, layerAsLadder))
        {
                isHasLadderBelow = true;
                Debug.DrawRay(ladderBelowHit.point, ladderBelowHit.normal * 10);
        }

        return isHasLadderBelow;
    }
}