using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlayerAction : MonoBehaviour
{
    public GameObject targetObj;
    public string action = "action";

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == GameManager.Instance.Player.gameObject)
        {
            targetObj.SendMessage(action, SendMessageOptions.RequireReceiver);
        }
    }
}
