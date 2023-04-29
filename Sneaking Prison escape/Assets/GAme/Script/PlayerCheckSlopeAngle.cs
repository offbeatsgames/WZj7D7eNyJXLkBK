using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckSlopeAngle : MonoBehaviour
{
    public LayerMask layerAsGround;
    public float slideSpeed = 10;
    public float accSpeed = 1;
    public float angleAsSlope = 45;

    [Header("---Rootbone Rotate Controll---")]
    public Transform rootBone;
    public Vector3 rotateBoneOnSliding = new Vector3(0, 90, 10);

    [ReadOnly] public float currentAngle;
    [ReadOnly] public bool isStandOnTheSlope;
    public  RaycastHit hitGround;

    // Update is called once per frame
    void Update()
    {
        isStandOnTheSlope = false;

        if (!GameManager.Instance.Player.playerCheckWater.isUnderWater && Physics.Raycast(transform.position + Vector3.up * 1, Vector3.down, out hitGround, 2, layerAsGround))
        {
            Debug.DrawRay(hitGround.point, hitGround.normal * 2);
            currentAngle = Vector3.Angle(hitGround.normal, Vector3.right);
            if (currentAngle <= angleAsSlope)
            {
                isStandOnTheSlope = true;
            }
        }
    }
}