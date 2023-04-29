using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour, ICanTakeDamage
{
    [Header("--- SET UP ---")]
    public int hitsToKill = 1;
    public Animator anim;
    public float moveSpeed = 2;
    public float runSpeed = 4;
    public float gravity = -9.8f;
    public int horizontalInput = -1;
    public LayerMask layerAsGround;
    public LayerMask layerAsWall;
    public AudioClip soundDie, soundDetectPlayer;
    [ReadOnly] public bool isGrounded = false;
    CharacterController characterController;
    [ReadOnly] public Vector2 velocity;
    bool isDead = false;

    public bool allowCheckGroundAhead = false;

    [Header("*** PATROL ***")]
    public bool usePatrol = false;
    public float patrolWaitForNextPoint = 2f;
    [Range(-10, -1)]
    public float limitLocalLeft = -2;
    [Range(1, 10)]
    public float limitLocalRight = 2;
    [ReadOnly] public float limitLeft, limitRight;
    public float dismissPlayerDistance = 10;
    public float dismissPlayerWhenStandSecond = 5;

    public float checkDistance = 8;
    public LayerMask layerAsTarget;
    [ReadOnly] public bool isAttacking = false;

    [ReadOnly] public bool isDetectPlayer;
    bool isWaiting = false;

    bool isFacingRight { get { return transform.forward.x > 0.5f; } }
    [ReadOnly] public float countingStanding = 0;
    protected EnemyMeleeAttack meleeAttack;

    private void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meleeAttack = GetComponent<EnemyMeleeAttack>();

        StartCoroutine(CheckTargetCo());

        limitLeft = transform.position.x + limitLocalLeft;
        limitRight = transform.position.x + limitLocalRight;
        isWaiting = false;

        if (!usePatrol)
            moveSpeed = 0;
    }

    IEnumerator CheckTargetCo()
    {
        while (true)
        {
            while (isDetectPlayer || (GameManager.Instance.gameState != GameManager.GameState.Playing)) { yield return null; }

            RaycastHit hit;
            if (Physics.CapsuleCast(transform.position + Vector3.up * characterController.height * 0.5f, transform.position + Vector3.up * (characterController.height - characterController.radius),
           characterController.radius, horizontalInput > 0 ? Vector3.right : Vector3.left, out hit, checkDistance, layerAsTarget))
            {
               DetectPlayer(0.5f);
            }

            yield return null;
        }
    }


    //can call by Alarm action of other Enemy
    public virtual void DetectPlayer(float delayChase = 0)
    {
        if (isDetectPlayer)
            return;

        isDetectPlayer = true;
        SoundManager.PlaySfx(soundDetectPlayer);
        StartCoroutine(DelayBeforeChasePlayer(delayChase));
    }

    protected IEnumerator DelayBeforeChasePlayer(float delay)
    {
        isWaiting = true;
           yield return new WaitForSeconds(delay);

        isWaiting = false;
    }

    public virtual void DismissDetectPlayer()
    {
        if (!isDetectPlayer)
            return;

        isWaiting = false;
        isDetectPlayer = false;
    }

    private void OnDrawGizmos()
    {
        if (usePatrol)
        {
            if (Application.isPlaying)
            {
                var lPos = transform.position;
                lPos.x = limitLeft;
                var rPos = transform.position;
                rPos.x = limitRight;
                Gizmos.DrawWireCube(lPos, Vector3.one * 0.2f);
                Gizmos.DrawWireCube(rPos, Vector3.one * 0.2f);
                Gizmos.DrawLine(lPos, rPos);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position + Vector3.right * limitLocalLeft, Vector3.one * 0.2f);
                Gizmos.DrawWireCube(transform.position + Vector3.right * limitLocalRight, Vector3.one * 0.2f);
                Gizmos.DrawLine(transform.position + Vector3.right * limitLocalLeft, transform.position + Vector3.right * limitLocalRight);
            }
        }
    }

    private void Update()
    {
        if (isDead)
        {
            HandleAnimation();
            return;
        }
        transform.forward = new Vector3(horizontalInput, 0, 0);
        if (GameManager.Instance.gameState != GameManager.GameState.Playing || isAttacking)
            velocity.x = 0;
        else
            velocity.x = (isDetectPlayer ? runSpeed : moveSpeed) * horizontalInput;

        CheckGround();

        if (isGrounded && velocity.y < 0)
            velocity.y = 0;
        else
            velocity.y += gravity * Time.deltaTime;     //add gravity

        if (isWaiting)
            velocity.x = 0;

        if (isDead)
            velocity = Vector2.zero;

        Vector2 finalVelocity = velocity;
        if (isGrounded && groundHit.normal != Vector3.up)        //calulating new speed on slope
            GetSlopeVelocity(ref finalVelocity);

        characterController.Move(finalVelocity * Time.deltaTime);

        if (isDetectPlayer && velocity.x == 0)
        {
            countingStanding += Time.deltaTime;
            if (isDetectPlayer && countingStanding >= dismissPlayerWhenStandSecond)
                DismissDetectPlayer();
        }
        else
            countingStanding = 0;

        HandleAnimation();

        if (isDetectPlayer)
        {
            if ((isFacingRight && transform.position.x > GameManager.Instance.Player.transform.position.x) || (!isFacingRight && transform.position.x < GameManager.Instance.Player.transform.position.x))
            {
                //if (enemyState == ENEMYSTATE.WALK)
                Flip();
                isWaiting = false;
            }

            if (isWallAHead() || (allowCheckGroundAhead && !isGroundedAhead()))
            {
                isWaiting = true;
            }
            else
                isWaiting = false;
        }
        else
        {
            if (!isDead && !isWaiting)
            {
                if (isWallAHead())
                    StartCoroutine(ChangeDirectionCo());
                else if (usePatrol)
                {
                    if ((velocity.x < 0 && transform.position.x < limitLeft)
                        || (velocity.x > 0 && transform.position.x > limitRight))
                        StartCoroutine(ChangeDirectionCo());
                }

                if (allowCheckGroundAhead && !isGroundedAhead())
                    StartCoroutine(ChangeDirectionCo());
            }
        }

        if (isDetectPlayer)
        {
            if (Vector2.Distance(transform.position, GameManager.Instance.Player.transform.position) > dismissPlayerDistance)
                DismissDetectPlayer();
        }

        if (isDetectPlayer && GameManager.Instance.gameState == GameManager.GameState.Playing)
        {
            CheckAttack();
        }
    }

    Vector3 hitNormal;
    public float dot;
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
        dot = Vector3.Dot(transform.forward, hitNormal);
    }

    IEnumerator ChangeDirectionCo()
    {
        if (isWaiting)
            yield break;

        isWaiting = true;
        yield return new WaitForSeconds(patrolWaitForNextPoint);

        while (GameManager.Instance.gameState != GameManager.GameState.Playing) { yield return null; }

        Flip();
        isWaiting = false;
    }

    RaycastHit groundHit;
    void CheckGround()
    {
        isGrounded = false;
        if (Physics.SphereCast(transform.position + Vector3.up * 1, characterController.radius * 0.9f, Vector3.down, out groundHit, 1f, layerAsGround))
        {
            float distance = transform.position.y - groundHit.point.y;
            if (distance <= (characterController.skinWidth + 0.01f))
                isGrounded = true;
        }
    }

    bool isGroundedAhead()
    {
        var _isGroundAHead = Physics.Raycast(transform.position + Vector3.up * 0.5f + (isFacingRight ? Vector3.right : Vector3.left) * characterController.radius * 1.1f, Vector3.down, 0.75f);
        Debug.DrawRay(transform.position + Vector3.up * 0.5f + (isFacingRight ? Vector3.right : Vector3.left) * characterController.radius * 1.1f, Vector3.down * 1);
        return _isGroundAHead;
    }

    void GetSlopeVelocity(ref Vector2 vel)
    {
        var crossSlope = Vector3.Cross(groundHit.normal, Vector3.forward);
        vel = vel.x * crossSlope;

        Debug.DrawRay(transform.position, crossSlope * 10);
    }

    void Flip()
    {
        if (isAttacking)
            return;

        horizontalInput *= -1;
    }

    bool isWallAHead()
    {
        if (Physics.CapsuleCast(transform.position + Vector3.up * characterController.height * 0.5f, transform.position + Vector3.up * (characterController.height - characterController.radius),
            characterController.radius, horizontalInput > 0 ? Vector3.right : Vector3.left, 1f, layerAsWall))
        {
            return true;
        }
        else
            return false;
    }

    void HandleAnimation()
    {
        anim.SetFloat("speed", Mathf.Abs(velocity.x));
        anim.SetBool("isDead", isDead);
        anim.SetBool("isRunning", isDetectPlayer);
    }

    public void Kill()
    {
        if (isDead)
            return;

        StopAllCoroutines();
        isDead = true;
        SoundManager.PlaySfx(soundDie);
        gameObject.layer = LayerMask.NameToLayer("TriggerPlayer");
        gameObject.AddComponent<Rigidbody>();

        Destroy(characterController);
        gameObject.AddComponent<Rigidbody>();
        GetComponent<Rigidbody>().isKinematic = true;
        Destroy(gameObject, 5);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Deadzone")
        {
            Kill();
        }
    }


    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        hitsToKill--;

        if (hitsToKill > 0)
            DetectPlayer();
        else Kill();
    }

    void CheckAttack()
    {
        if (meleeAttack.AllowAction())
        {
            if (meleeAttack.CheckPlayer(isFacingRight))
            {
                meleeAttack.Action();
                anim.SetTrigger("melee");
                isAttacking = true;
                CancelInvoke("NoAttacking");
                Invoke("NoAttacking", 2);
            }
        }
        //else if (!meleeAttack.isAttacking && isAttacking)
        //{
        //    isAttacking = false;
        //}
    }

    void NoAttacking()
    {
        isAttacking = false;
    }

    public void AnimMeleeAttackStart()
    {
        meleeAttack.Check4Hit();
    }

    public void AnimMeleeAttackEnd()
    {
        meleeAttack.EndCheck4Hit();
    }
}
