using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.Instance.Player.gameObject)
        {
            other.gameObject.SetActive(false);
        }
    }
}
