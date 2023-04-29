using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckPushButtonObject : MonoBehaviour
{
    [ReadOnly] public ButtonPushAction currentButtonObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.forward, out hit, 3))
        {
            currentButtonObject = hit.collider.gameObject.GetComponent<ButtonPushAction>();
        }
        else
            currentButtonObject = null;
    }
}
