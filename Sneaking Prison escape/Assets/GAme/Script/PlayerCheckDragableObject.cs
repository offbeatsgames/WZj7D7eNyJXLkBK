using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckDragableObject : MonoBehaviour
{
    [Tooltip("Distance to detect the object")]
    public float distanceDetect = 0.5f;
    public float dragPushMoveSpeed = 2;
    [Tooltip("Set player offset with the detected point")]
    public float grabOffsetX = 0.2f;
    [ReadOnly] public bool isGrabbingTheDragableObject = false;
    [ReadOnly] public DragableObject detectedDragableObject;
    [ReadOnly] public  RaycastHit hit;
    [ReadOnly] public float offsetBoxAndPlayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbingTheDragableObject)
            return;     //stop checking when player is dragging

        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, distanceDetect))
        {
            if (hit.collider.gameObject.GetComponent<DragableObject>() && hit.collider.gameObject.GetComponent<DragableObject>().isDragable)
                detectedDragableObject = hit.collider.gameObject.GetComponent<DragableObject>();
            else
                detectedDragableObject = null;
        }
        else
            detectedDragableObject = null;
    }
}
