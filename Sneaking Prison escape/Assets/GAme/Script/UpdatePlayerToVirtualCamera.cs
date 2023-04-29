using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class UpdatePlayerToVirtualCamera : MonoBehaviour
{
    CinemachineVirtualCamera virtualCamera;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        if (GameManager.Instance.Player)
        {
            if (virtualCamera.Follow == null)
                virtualCamera.Follow = GameManager.Instance.Player.transform;
        }
    }
}
