using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public float speed = 100;
    void Update()
    {
        transform.RotateAround(transform.position, Vector3.up, speed * Time.deltaTime);
    }

}
