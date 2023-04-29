using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchLight : MonoBehaviour
{
    public Transform lightHead;
    public float rotateSpeed = 1;
    public float maxAngle = 30;
    public float offset = 0;

    Vector3 originalRotation;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RotateCo());
    }

    IEnumerator RotateCo()
    {
        float targetAngle = maxAngle;
        originalRotation = lightHead.rotation.eulerAngles;
        while (true)
        {
            while(GameManager.Instance && (GameManager.Instance.gameState != GameManager.GameState.Playing)) { yield return null; }

            lightHead.rotation = Quaternion.Euler(originalRotation.x, Mathf.PingPong(Time.time * rotateSpeed, maxAngle * 2) + offset, originalRotation.z);
            yield return null;
        }
    }
}
