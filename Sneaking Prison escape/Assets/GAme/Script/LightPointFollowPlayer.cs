using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPointFollowPlayer : MonoBehaviour
{
    public Vector3 offsetToPlayer = new Vector3(0, 2, -1);
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = GameManager.Instance.Player.transform.position + offsetToPlayer;
    }
}
