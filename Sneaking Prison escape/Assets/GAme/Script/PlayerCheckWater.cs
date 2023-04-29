using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckWater : MonoBehaviour
{
    [ReadOnly] public Transform currentWaterZone;

    public LayerMask layerAsWater;

    public GameObject waterInFX;
    public GameObject waterRippleFX;

    public AudioClip soundJumpIn, soundSwim;
    [Range(0f,1f)]
    public float soundVolume = 0.35f;

    [Header("---PLAYER ABILITY---")]
    public float offsetPlayerWithSurfaceSwimming = -1.5f;       //player floating in the water offset

    [Header("---SPEED---")]
    [Range(0f,0.9f)]
    public float lowSpeedPercent = 0.5f;
    public float lowSpeedHeight = 1;

    [Header("DIVING")]
    [ReadOnly] public bool isUnderWater = false;
    public float drivingSpeed = 2f;
    public float oxygenDrainTimeOut = 5f;
    [ReadOnly] public float oxygenRemainTime;

    private void Start()
    {
        StartCoroutine(DoRippleFXCo());
    }

    // Update is called once per frame
    void Update()
    {
        var hitWater = Physics.OverlapSphere(transform.position, 0.1f, layerAsWater);
        if (hitWater.Length > 0)
        {
            if (currentWaterZone == null && GameManager.Instance.Player.velocity.y < -5)
            {
                SoundManager.PlaySfx(soundJumpIn, soundVolume);
                Instantiate(waterInFX, new Vector3(GameManager.Instance.Player.transform.position.x, hitWater[0].gameObject.transform.position.y, 0), waterInFX.transform.rotation);
            }
            currentWaterZone = hitWater[0].gameObject.transform;
        }
        else
            currentWaterZone = null;

        if (!GameManager.Instance.Player.isDead && isUnderWater)
        {
            oxygenRemainTime -= Time.deltaTime;
            oxygenRemainTime = Mathf.Max(0, oxygenRemainTime);
            if (oxygenRemainTime <= 0)
                GameManager.Instance.Player.Die();
        }
        else
            oxygenRemainTime = oxygenDrainTimeOut;
    }

    IEnumerator DoRippleFXCo()
    {
        while (true)
        {
            while(GameManager.Instance.gameState!= GameManager.GameState.Playing) { yield return null; }

            yield return new WaitForSeconds(Random.Range(1.5f, 2f));
            if (isInWaterZone())
            {
                Instantiate(waterRippleFX, new Vector3(GameManager.Instance.Player.transform.position.x + GameManager.Instance.Player.input.x, currentWaterZone.transform.position.y, 0), waterInFX.transform.rotation);
            }
        }
    }

    //Animation event
    public void SwimAction()
    {
        if (GameManager.Instance.Player.input.x != 0)
        {
            SoundManager.PlaySfx(soundSwim, soundVolume);
            Instantiate(waterRippleFX, new Vector3(GameManager.Instance.Player.transform.position.x + GameManager.Instance.Player.input.x, currentWaterZone.transform.position.y, 0), waterInFX.transform.rotation);
        }
    }

    public bool isInWaterZone()
    {
        return currentWaterZone != null;
    }

    public bool isActiveLowSpeed(Vector3 playerPosition)
    {
        return Mathf.Abs(playerPosition.y - currentWaterZone.position.y) >= lowSpeedHeight;
    }

    public bool isSwimming(Vector3 playerPosition)
    {
        if (!isInWaterZone())
            return false;

        return Mathf.Abs(playerPosition.y - currentWaterZone.position.y) >= (Mathf.Abs( offsetPlayerWithSurfaceSwimming)-0.1f);
    }
}
