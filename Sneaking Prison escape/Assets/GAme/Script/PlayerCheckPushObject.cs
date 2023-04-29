using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckPushObject : MonoBehaviour
{
    public float applyForce = 100;
    public LayerMask layerAsPushObject;
    [ReadOnly] public GameObject currentPushObject;
    RaycastHit hitObject;

    void Start()
    {

    }

    void Update()
    {
        Debug.DrawRay(GameManager.Instance.Player.transform.position + Vector3.up * 0.5f, transform.forward * (GameManager.Instance.Player.characterController.radius + 10));
        if (Physics.Raycast(GameManager.Instance.Player.transform.position + Vector3.up * 0.5f, transform.forward, out hitObject, GameManager.Instance.Player.characterController.radius + 0.3f, layerAsPushObject))
        {
            Debug.LogError(hitObject.collider.gameObject);
            if (hitObject.collider.gameObject.CompareTag("PushObject"))
                currentPushObject = hitObject.collider.gameObject;
            else
                currentPushObject = null;
        }
        else
            currentPushObject = null;
    }

    public void TryPushObject(Vector3 velocity)
    {
        velocity.y = 0;
        if (currentPushObject)
            currentPushObject.GetComponent<Rigidbody>().AddForce(velocity * applyForce);

        //currentPushObject.GetComponent<Rigidbody>().velocity = velocity;
    }
}
