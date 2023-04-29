using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckSneakingWall : MonoBehaviour
{
    public LayerMask layerAsSneakingWall;
    public float considerHighWallPos = 1.5f;        //if the wall reach to this local height, set it as the high wall
    [ReadOnly] public bool isHighWall;
    [ReadOnly] public bool isInTheSneakingZone = false;

    void Update()
    {
        if (Physics.CheckSphere(transform.position + Vector3.up * 0.3f, 0.02f, layerAsSneakingWall))
        {
            isInTheSneakingZone = true;
            if (Physics.CheckSphere(transform.position + Vector3.up * considerHighWallPos, 0.01f, layerAsSneakingWall))
                isHighWall = true;
            else
                isHighWall = false;
        }
        else
            isInTheSneakingZone = false;
    }
}
