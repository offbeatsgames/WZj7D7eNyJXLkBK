using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleElevator : MonoBehaviour, IPlayerSpawnCheckpoint
{
    [Tooltip("Force player no move when this elevator moving")]
    public bool forcePlayerStandWhenMoving = true;
    public Vector2 localTargetPos = new Vector3(0, 8);
    public float moveSpeed = 2;
    public float delay = 1f;
    public AudioClip elevatorOperateSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;

    AudioSource audioSource;

    //public Material lightWorkMat;
    public Color colorIdle, colorActive;
    [ReadOnly] public bool isMoving = false;
    Vector3 PosA;
    Vector3 PosB;

    bool movingA2B = false;
    MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

           PosA = transform.position;
        PosB = PosA + (Vector3)localTargetPos;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = elevatorOperateSound;
        audioSource.volume = 0;
        audioSource.loop = true;
    }

    void FixedUpdate()
    {
        meshRenderer.materials[1].SetColor("_EmissionColor", isMoving ? colorActive : colorIdle);

        if (isMoving)
        {
            Vector3 targetPos = movingA2B ? PosB : PosA;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) <= 0.01f)
            {
                transform.position = targetPos;
                isMoving = false;
                audioSource.Stop();
                if (forcePlayerStandWhenMoving)
                {
                    GameManager.Instance.Player.ForcePlayerStanding(false);
                    GameManager.Instance.Player.transform.parent = null;
                }
            }
        }

        audioSource.volume = isMoving ? soundVolume : 0;
    }

    public void Active()
    {
        if (isMoving)
            return;

        if (forcePlayerStandWhenMoving)
        {
            GameManager.Instance.Player.ForcePlayerStanding(true);
            GameManager.Instance.Player.transform.parent = transform;
        }
        Invoke("ActiveCo", delay);
    }

    void ActiveCo()
    {
        movingA2B = !movingA2B;

        isMoving = true;
        audioSource.Play();
    }

    public void OnPlayerRespawnCheckpoint()
    {
        transform.position = PosA;

        //Reset data
        movingA2B = false;
        isMoving = false;
        audioSource.Stop();
    }

    protected virtual void OnDrawGizmos()
    {
        float size = .3f;


        Gizmos.color = Color.red;
        Vector3 globalWaypointPos = (Application.isPlaying) ? PosB : (Vector3) localTargetPos + transform.position;

        Gizmos.DrawWireSphere(globalWaypointPos, size);

        Gizmos.color = Color.green;
        if (Application.isPlaying)
            Gizmos.DrawLine(PosA, PosB);
        else
            Gizmos.DrawLine(transform.position, globalWaypointPos);

    }
}