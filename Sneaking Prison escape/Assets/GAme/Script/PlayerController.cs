
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerState { Ground, Water, Jetpack, Windy }
[System.Serializable]
public class PlayerParameter
{
    public float moveSpeed = 4;
    public float runSpeed = 6;
    public float sneakingSpeed = 2.5f;
    public float maxJumpHeight = 3;
    public float minJumpHeight = 1;
    public float gravity = -35f;
}

public class PlayerController : MonoBehaviour, ICanTakeDamage
{
    [ReadOnly] public float gravity = -35f;
    [ReadOnly] public PlayerState PlayerState = PlayerState.Ground;        //set what state the player in

    [Header("---SETUP LAYERMASK---")]
    public LayerMask layerAsGround;
    public LayerMask layerAsWall;
    public LayerMask layerCheckHitHead;

    [Header("---AIR JUMP OPTION---")]
    [Range(0, 2)]
    public int numberOfAirJump = 0;
    [ReadOnly] public int numberAirJumpCounter = 0;

    [Header("---WALL SLIDE---")]
    public float wallSlidingSpeed = 0.5f;
    [Tooltip("Player only can stick on the wall a little bit, then fall")]
    public float wallStickTime = 0.25f;
    [ReadOnly] public float wallStickTimeCounter;
    public Vector2 wallSlidingJumpForce = new Vector2(6, 3);
    [ReadOnly] public bool isWallSliding = false;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    //[Tooltip("When look to other side, sliding with this speed")]
    //public float wallSlideSpeedHold = 0.15f;
    //[Tooltip("When look to other side, sliding with this speed")]
    //public float wallSlideSpeedNoHold = 0.5f;
    //[Tooltip("When look to other side, sliding with this speed")]
    //public float wallSlideSpeedLookOtherSide = 0.3f;
    int wallDirX;

    public CharacterController characterController { get; set; }
    [ReadOnly] public Vector2 velocity;
    [ReadOnly] public Vector2 input;
    [ReadOnly] public Vector2 inputLastTime = Vector2.right;
    [ReadOnly] public bool isGrounded = false;
    [ReadOnly] public bool ignoreInputControl = false;      //use for rope jump
    Animator anim;
    bool isPlaying = true;
    [ReadOnly] public bool isDead = false;

    float velocityXSmoothing;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGroundedRun = .3f;
    public float accelerationTimeGroundedSliding = 1f;

    [Header("---AUDIO---")]
    public AudioClip soundFootStep;
    public AudioClip soundJump, soundHit, soundDie, soundLanding, soundSlideSlope;
    public AudioClip soundGrap, soundRopeJump;
    [Range(0f, 1f)]
    public float soundFootStepVolume = 0.5f;
    [Range(0f, 1f)]
    public float soundJumpVolume = 0.2f;

    AudioSource audioSourceSliding;

    public bool isInJumpZone { get; set; }

    public float accGrounedOverride { get; set; }

    [Header("Setup parameter on ground")]
    public PlayerParameter GroundParameter;     //Ground parameters

    private float moveSpeed;        //the moving speed, changed evey time the player on ground or in water
    private float runSpeed;
    private float sneakingSpeed;
    private float maxJumpHeight;
    private float minJumpHeight;

    PlayerCheckLadderZone playerCheckLadderZone;

    PlayerRopeDetecter playerRopeDetecter;
    PlayerCheckSmallBridge playerCheckSmallBridge;
    PlayerCheckSneakingWall playerCheckSneakingWall;
    PlayerCheckSlopeAngle playerCheckSlopeAngle;
    [ReadOnly] public PlayerCheckWater playerCheckWater;
    PlayerCheckDragableObject playerCheckDragableObject;
    [HideInInspector] public PlayerCheckPushButtonObject playerCheckPushButtonObject;

    public bool isFacingRight { get { return inputLastTime.x > 0; } }
    [ReadOnly] public float lastGroundPos;
    [ReadOnly] public Vector2 deltaPosition;
    public GameObject unableObject;

    protected PlayerParameter overrideZoneParameter = null; //null mean no override
    protected bool useOverrideParameter = false;
    PlayerOverrideParametersChecker playerOverrideParametersChecker;
    public void SetOverrideParameter(PlayerParameter _override, bool isUsed, PlayerState _zone = PlayerState.Ground)
    {
        overrideZoneParameter = _override;
        useOverrideParameter = isUsed;
        PlayerState = _zone;
    }

    bool isUsingPatachute = false;

    public void SetParachute(bool useParachute)
    {
        isUsingPatachute = useParachute;
    }

    public void ExitZoneEvent()
    {
        PlayerState = PlayerState.Ground;
        if (isUsingPatachute)
            SetParachute(false);
    }

    public bool forcePlayerStanding { get; set; }
    public void ForcePlayerStanding(bool stop)
    {
        forcePlayerStanding = stop;
        if (stop)
            input = Vector2.zero;
    }

    bool forceIdle = false;
    public void ForceIdle(float time)
    {
        StartCoroutine(ForceIdleCo(time));
    }

    IEnumerator ForceIdleCo(float time)
    {
        forceIdle = true;
        yield return new WaitForSeconds(time);
        forceIdle = false;
    }

    void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        transform.forward = new Vector3(isFacingRight ? 1 : -1, 0, 0);
        originalCharHeight = characterController.height;
        originalCharCenterY = characterController.center.y;

        audioSourceSliding = gameObject.AddComponent<AudioSource>();
        audioSourceSliding.clip = soundSlideSlope;
        audioSourceSliding.volume = 0;
        audioSourceSliding.loop = true;
        audioSourceSliding.Play();

        jetpackObj.SetActive(false);

        jetpackAScr = jetpackObj.AddComponent<AudioSource>();
        jetpackAScr.clip = jetpackSound;
        jetpackAScr.volume = 0;
        jetpackAScr.loop = true;

        rangeAttack = GetComponent<RangeAttack>();
        meleeAttack = GetComponent<MeleeAttack>();
        playerCheckLadderZone = GetComponent<PlayerCheckLadderZone>();
        playerRopeDetecter = GetComponent<PlayerRopeDetecter>();
        playerCheckSmallBridge = GetComponent<PlayerCheckSmallBridge>();
        playerCheckSneakingWall = GetComponent<PlayerCheckSneakingWall>();
        playerCheckSlopeAngle = GetComponent<PlayerCheckSlopeAngle>();
        playerCheckWater = GetComponent<PlayerCheckWater>();
        playerCheckDragableObject = GetComponent<PlayerCheckDragableObject>();
        playerCheckPushButtonObject = GetComponent<PlayerCheckPushButtonObject>();
        ropeRenderer = GetComponent<LineRenderer>();

        playerOverrideParametersChecker = GetComponent<PlayerOverrideParametersChecker>();
        SetupParameter();
        unableObject.SetActive(false);
    }

    public void SetupParameter()
    {
        PlayerParameter _tempParameter;

        switch (PlayerState)
        {
            case PlayerState.Ground:
                _tempParameter = GroundParameter;
                break;
            default:
                _tempParameter = GroundParameter;
                break;
        }

        if (useOverrideParameter)
            _tempParameter = overrideZoneParameter;

        isRunning = false;
        moveSpeed = _tempParameter.moveSpeed;
        runSpeed = _tempParameter.runSpeed;
        sneakingSpeed = _tempParameter.sneakingSpeed;
        maxJumpHeight = _tempParameter.maxJumpHeight;
        minJumpHeight = _tempParameter.minJumpHeight;
        gravity = _tempParameter.gravity;
    }

    public void animSetSpeed(float value)
    {
        if (anim)
            anim.speed = value;
    }

    void SetCheckPoint(Vector3 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 100, layerAsGround))
        {
            GameManager.Instance.SetCheckPoint(hit.point);
        }
    }

    void Update()
    {
        if (GameManager.Instance.gameState != GameManager.GameState.Playing)
        {
            velocity.x = 0;
            if (playerCheckWater.isUnderWater)
                velocity.y -= Time.deltaTime;
            else
                velocity.y += gravity * Time.deltaTime;

            input.x = 0;
            characterController.Move(velocity * Time.deltaTime);
            CheckGround();
            if (isGrounded && velocity.y < 0)
                velocity.y = 0;

            HandleAnimation();
            return;
        }

        if (isMovingInPipe)
            return;

        audioSourceSliding.volume = (playerCheckSlopeAngle.isStandOnTheSlope && isGrounded) ? 1 : 0;

        if (meleeAttack.isWeaponShowing() && (isDead || playerCheckLadderZone.isInLadderZone || climbingState == ClimbingState.ClimbingLedge
            || playerCheckDragableObject.isGrabbingTheDragableObject || playerRopeDetecter.isHoldingRope || isHangingTopPipe || playerCheckPushButtonObject.currentButtonObject != null
            || playerCheckWater.isInWaterZone()))
            meleeAttack.ShowWeapon(false);

        if (rangeAttack.isWeaponShowing() && (isDead || playerCheckLadderZone.isInLadderZone || climbingState == ClimbingState.ClimbingLedge
          || playerCheckDragableObject.isGrabbingTheDragableObject || playerRopeDetecter.isHoldingRope || isHangingTopPipe || playerCheckPushButtonObject.currentButtonObject != null
          || playerCheckWater.isInWaterZone()))
            rangeAttack.ShowWeapon(false);

        if (!isDead && !playerRopeDetecter.isHoldingRope)
        {
            playerRopeDetecter.CheckRope(isFacingRight ? Vector2.right : Vector2.left);
        }

        if (playerRopeDetecter.isDetectedRope && !playerRopeDetecter.isHoldingRope)
        {
            if (!isGrounded || (isGrounded && (input.y == 1)))
            {
                if (isUsingJetpack)
                {
                    UseJetpack(false);
                    UpdateJetPackStatus();
                }

                playerRopeDetecter.isHoldingRope = true;
                velocity = Vector2.zero;
                AllowInputControlAgainCo();
                //apply the force when jump hold the rope
                playerRopeDetecter.currentRope.GetComponent<Rigidbody>().AddForce(playerRopeDetecter.currentRope.transform.forward * input.x * 500);
            }
        }
        if (isGrabingRope)
        {
            transform.RotateAround(currentAvailableRope.transform.position, rotateAxis, inputLastTime.x * speed * Time.deltaTime);


            transform.up = currentAvailableRope.transform.position - transform.position;
            transform.Rotate(0, inputLastTime.x > 0 ? 90 : -90, 0);

            ropeRenderer.SetPosition(0, transform.position + transform.forward * grabOffset.x + transform.up * grabOffset.y);
            ropeRenderer.SetPosition(1, currentAvailableRope.transform.position);

            if (transform.position.y >= releasePointY)
            {
                if ((inputLastTime.x > 0 && transform.position.x > currentAvailableRope.transform.position.x) || (inputLastTime.x < 0 && transform.position.x < currentAvailableRope.transform.position.x))
                    //GrabRelease();      //disconnect grab if player reach to the limit position
                    Flip();
            }
        }
        else if (playerRopeDetecter.isHoldingRope)
        {
            if (input.y != 0)
            {
                if (input.y > 0)
                    playerRopeDetecter.MoveUp();
                else
                    playerRopeDetecter.MoveDown();
            }

            transform.position = playerRopeDetecter.catchKnot.transform.position +( playerRopeDetecter.catchKnot.transform.up * playerRopeDetecter.offsetPlayer.y ) + (playerRopeDetecter.catchKnot.transform.right * playerRopeDetecter.offsetPlayer.x) * -1;

            transform.up = playerRopeDetecter.currentRope.transform.up * -1;
            transform.Rotate(0, isFacingRight ? 90 : -90, 0);

            //add force to the holding rope object
            //playerRopeDetecter.currentRope.GetComponent<Rigidbody>().AddForce(playerRopeDetecter.currentRope.transform.forward * playerRopeDetecter.swingForce * input.x, ForceMode.Acceleration);
            playerRopeDetecter.AddForceToRope(playerRopeDetecter.currentRope.transform.forward * playerRopeDetecter.swingForce * input.x, input.x);
        }
        else if (climbingState != ClimbingState.ClimbingLedge && !isDroppingToLadder)      //stop move when climbing
        {
            transform.forward = new Vector3(isFacingRight ? 1 : -1, 0, 0);

            float targetX = 0;
            if (isHangingTopPipe)
                targetX = hangingMoveSpeed;
            else if (isGrounded && (!playerCheckSmallBridge.isHasGroundOnBothSide || playerCheckSneakingWall.isInTheSneakingZone))
                targetX = sneakingSpeed;
            else if (IgnoreControllerInput())
                targetX = 0;
            else if (playerCheckDragableObject.isGrabbingTheDragableObject)
            {
                targetX = playerCheckDragableObject.dragPushMoveSpeed;

                if (input.x == 0)
                    velocity.x = 0;
            }
            else
                targetX = isRunning ? runSpeed : moveSpeed;

            float targetVelocityX = input.x * targetX;

            if (isSliding || isWallSliding || forcePlayerStanding)
                targetVelocityX = 0;

            if (playerCheckWater.isInWaterZone() && !playerCheckWater.isSwimming(transform.position) && playerCheckWater.isActiveLowSpeed(transform.position))
                targetVelocityX *= playerCheckWater.lowSpeedPercent;

            if (playerCheckSlopeAngle.isStandOnTheSlope && isGrounded && !playerCheckWater.isUnderWater)
                targetVelocityX = playerCheckSlopeAngle.slideSpeed * (playerCheckSlopeAngle.hitGround.normal.x > 0 ? 1 : -1);


            if (forceStandingRemain > 0)
            {
                targetVelocityX = 0;
                forceStandingRemain -= Time.deltaTime;
            }

            if (!ignoreInputControl && !isJumpingOutFromTheRope)
            {
                velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, isGrounded ?
                    ((isSliding ? accelerationTimeGroundedSliding : accelerationTimeGroundedRun)) : (isHangingTopPipe ? 0 : accelerationTimeAirborne));
            }

            CheckGround();

            if (isGrounded && groundHit.collider.gameObject.tag == "Deadzone")
                HitAndDie();

            if (!isDead && isUsingJetpack)
            {
                jetpackRemainTime -= Time.deltaTime;
                jetpackRemainTime = Mathf.Max(0, jetpackRemainTime);
                if (jetpackRemainTime > 0)
                    velocity.y += jetForce * Time.deltaTime;
                else
                {
                    ActiveJetpack(false);
                }
            }

            //if (isGrounded && playerCheckLadderZone.CheckLadderBelow(transform.position + Vector3.down * 0.5f, transform.forward, characterController.radius * 2))
            //{
            //    StartCoroutine(DropToLadderCo());
            //}

            if (isGrounded)
                lastGroundPos = transform.position.y;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0;
                lastJumpZoneObj = null;
                CheckEnemy();
                CheckSpring();
                isIgnorePlayerMovenmentInput = false;
                AllowInputControlAgainCo();
                isFallingFromWall = false;
                allowGrabWall = true;
                isJumpingOutFromTheRope = false;
                lastRopePointObj = null;
                firstContactWall = true;
                allowGrapNextWall = false;
                AllowCheckWall();

                playerRopeDetecter.ResetRope();
                playerCheckLadderZone.isInLadderZone = false;

                if (isFallingFromHeight)
                {
                    isFallingFromHeight = false;
                    SoundManager.PlaySfx(soundLanding, 0.5f);
                    ForceIdle(hardFallingIdleTime);
                }
            }
            else if (isWallSliding)
                velocity.y = -wallSlidingSpeed;
            else if (isHangingTopPipe)
                velocity.y = 0;
            else if (climbingState != ClimbingState.ClimbingLedge && playerCheckWater.isInWaterZone())
            {

                if (isFallingFromHeight)
                    isFallingFromHeight = false;

                float distanceTopTopWater = Mathf.Abs(transform.position.y - playerCheckWater.currentWaterZone.transform.position.y);

                if (playerCheckWater.isUnderWater)
                {
                    if (allowUnderSwimming)
                    {
                        velocity.y = input.y * playerCheckWater.drivingSpeed;
                    }

                    //velocity.y += gravity * Time.deltaTime;     //add gravity
                    if (distanceTopTopWater < Mathf.Abs(playerCheckWater.offsetPlayerWithSurfaceSwimming))
                    {
                        playerCheckWater.isUnderWater = false;
                        allowUnderSwimming = false;
                        velocity.y = 0;
                    }
                }
                else if ((distanceTopTopWater > Mathf.Abs(playerCheckWater.offsetPlayerWithSurfaceSwimming)))
                {
                    if (velocity.y < 0)
                    {
                        float _speed = 6;
                        velocity.y = Mathf.Max(velocity.y, -3);
                        velocity.y = Mathf.MoveTowards(velocity.y, 0, _speed * Time.deltaTime);
                    }
                    else
                    {
                        float _speed = 2;
                        SetPosition(new Vector2(transform.position.x, Mathf.MoveTowards(transform.position.y, playerCheckWater.currentWaterZone.position.y + playerCheckWater.offsetPlayerWithSurfaceSwimming, _speed * Time.deltaTime)));
                        velocity.y = 0;
                    }

                    if (input.y == -1)
                    {
                        velocity.y = -2f;
                        playerCheckWater.isUnderWater = true;
                        allowUnderSwimming = false;
                        Invoke("AllowUnderSwimming", 0.5f);
                    }
                } else if ((distanceTopTopWater <= Mathf.Abs(playerCheckWater.offsetPlayerWithSurfaceSwimming)))
                {
                    if (input.y == -1)
                    {
                        velocity.y = -2f;
                        playerCheckWater.isUnderWater = true;
                        allowUnderSwimming = false;
                        Invoke("AllowUnderSwimming", 0.5f);
                    }
                }
            }
            else
                velocity.y += gravity * Time.deltaTime;     //add gravity

            if (playerCheckLadderZone && climbingState != ClimbingState.ClimbingLedge && playerCheckLadderZone.isContactLadder(characterController))
            {
                if (input.y == 0)
                {
                    if ((isFacingRight && input.x == 1) || (!isFacingRight && input.x == -1))
                        velocity.x = input.x * playerCheckLadderZone.ladderMoveSpeed;     //X axis only move up
                    else
                        velocity.x = 0;

                }
                else
                    velocity.x = input.y * (isFacingRight ? 1 : -1) * playerCheckLadderZone.ladderMoveSpeed;       //Y axis can move up and down

                var crossSlope = Vector3.Cross(playerCheckLadderZone.ladderNormal, Vector3.forward);        //get the slope of the ladder
                velocity = velocity.x * crossSlope;     //get the final velocity with the ladder's slope
                isRunning = false;
            }
            else
                playerCheckLadderZone.isInLadderZone = false;

            if (!isUsingJetpack)
                CheckWallSliding();

            CheckEnemyAHead();

            if (PlayerState == PlayerState.Windy)
            {

                if (isUsingPatachute)
                {
                    if (velocity.y < playerOverrideParametersChecker.currentZone.forceVeritcalWithParachute)
                    {
                        velocity.y = playerOverrideParametersChecker.currentZone.forceVeritcalWithParachute;
                    }
                }
                else
                {
                    if (velocity.y < playerOverrideParametersChecker.currentZone.forceVertical)
                    {
                        velocity.y = playerOverrideParametersChecker.currentZone.forceVertical;
                    }
                }
            }

            if (!isPlaying || isDead || isFallingFromWall || isWaitingDropToGrabTheLadder)
                velocity.x = 0;

            //Prevent stuck in the air
            if (!isGrounded && input.x == 0 && deltaPosition.y == 0 && velocity.y < -10 && !playerCheckLadderZone.isInLadderZone && playerCheckLadderZone.CheckLadderBelow(transform.position, isFacingRight ? Vector3.left : Vector3.right, 1))
            {
                velocity.x = (isFacingRight ? moveSpeed : -moveSpeed);
            }

            Vector2 finalVelocity = velocity + new Vector2(extraForceSpeed, 0);
            if (isGrounded && groundHit.normal != Vector3.up)        //calulating new speed on slope
                GetSlopeVelocity(ref finalVelocity);

            characterController.Move(finalVelocity * Time.deltaTime);

            if (isJetpackActived)
                UpdateJetPackStatus();
        }

        HandleAnimation();
        CheckDoubleTap();

        if (isGrounded)
        {
            isInJumpZone = false;
            wallStickTimeCounter = wallStickTime;       //set reset wall stick timer when on ground
            CheckStandOnEvent();
        }

        ropeRenderer.enabled = isGrabingRope;
        CheckRopeInZone();      //only checking rope when in jump status

        if (!isUsingJetpack && PlayerState == PlayerState.Ground && forceStandingRemain <= 0)
        {
            if (climbingState == ClimbingState.None && isGrounded)
                CheckLowerLedge();

            if (climbingState == ClimbingState.None && input.x == (isFacingRight ? 1 : -1) && (playerCheckWater.isInWaterZone() || (!isGrounded && velocity.y < 0)))
                CheckLedge(verticalChecker.position);
            else if (climbingState == ClimbingState.None && playerCheckLadderZone && playerCheckLadderZone.isInLadderZone && climbingState == ClimbingState.None)
            {
                if ((isFacingRight && input.x == 1) || (!isFacingRight && input.x == -1) || input.y == 1)
                    CheckLedge(verticalCheckerClimbLadder.position);
            }
        }

        if (isHangingTopPipe)
            CheckIfInTopPipe();
        else
            CheckGrabHangingTopPipe();

        if (climbingState == ClimbingState.ClimbingLedge)
            UpdatePositionOnMovableObject(ledgeTarget);
        else if (isGrounded || input.x == 0)
            UpdatePositionOnMovableObject(groundHit.transform);

        deltaPosition = (Vector2)transform.position - lastPos;

        lastPos = transform.position;

        //Move the dragable object
        if (playerCheckDragableObject.isGrabbingTheDragableObject && playerCheckDragableObject.detectedDragableObject)
        {
            if ((isFacingRight && input.x == 1) || (!isFacingRight && input.x == -1))
            {
                playerCheckDragableObject.detectedDragableObject.Push(velocity);
                if (Mathf.Abs(transform.position.x - playerCheckDragableObject.detectedDragableObject.transform.position.x) < Mathf.Abs(playerCheckDragableObject.offsetBoxAndPlayer))
                {
                    SetPosition(new Vector3(playerCheckDragableObject.detectedDragableObject.transform.position.x - playerCheckDragableObject.offsetBoxAndPlayer, transform.position.y, transform.position.z));
                    lastPos = transform.position;
                }
            }
            else if ((isFacingRight && input.x != 1) || (!isFacingRight && input.x != -1))
                playerCheckDragableObject.detectedDragableObject.Pull(deltaPosition);
        }
    }

    [ReadOnly] public bool allowUnderSwimming = false;
    void AllowUnderSwimming()
    {
        allowUnderSwimming = true;
    }

    float extraForceSpeed = 0;
    public void AddHorizontalForce(float _speed)
    {
        extraForceSpeed = _speed;
    }

    [ReadOnly] public Vector3 m_LastGroundPos = Vector3.zero;
    private float m_LastAngle = 0;
    [ReadOnly] public Transform m_CurrentTarget;
    public Vector3 DeltaPos { get; private set; }
    public float DeltaYAngle { get; private set; }
    public void UpdatePositionOnMovableObject(Transform target)
    {
        if (target == null)
        {
            m_CurrentTarget = null;
            return;
        }

        if (m_CurrentTarget != target)
        {
            m_CurrentTarget = target;
            DeltaPos = Vector3.zero;
            DeltaYAngle = 0;
        }
        else
        {
            DeltaPos = target.transform.position - m_LastGroundPos;
            DeltaYAngle = target.transform.rotation.eulerAngles.y - m_LastAngle;

            Vector3 direction = transform.position - target.transform.position;
            direction.y = 0;

            float FinalAngle = Vector3.SignedAngle(Vector3.forward, direction.normalized, Vector3.up) + DeltaYAngle;

            float xMult = Vector3.Dot(Vector3.forward, direction.normalized) > 0 ? 1 : -1;
            float zMult = Vector3.Dot(Vector3.right, direction.normalized) > 0 ? -1 : 1;

            float cosine = Mathf.Abs(Mathf.Cos(FinalAngle * Mathf.Deg2Rad));
            Vector3 deltaRotPos = new Vector3(cosine * xMult, 0,
                 Mathf.Abs(Mathf.Sin(FinalAngle * Mathf.Deg2Rad)) * zMult) * Mathf.Abs(direction.magnitude);

            DeltaPos += deltaRotPos * (DeltaYAngle * Mathf.Deg2Rad);
        }

        if (DeltaPos.magnitude > 3f)
            DeltaPos = Vector3.zero;


        characterController.enabled = false;
        transform.Rotate(0, DeltaYAngle, 0);
        characterController.enabled = true;


        m_LastGroundPos = target.transform.position;
        m_LastAngle = target.transform.rotation.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (isMovingInPipe)
            return;
        if (playerRopeDetecter.isHoldingRope)
            return;

        if (playerCheckLadderZone.isInLadderZone && !isGrounded)
        {
            var crossSlope = Vector3.Cross(playerCheckLadderZone.ladderNormal, Vector3.forward);        //get the slope of the ladder
            playerCheckLadderZone.rootBone.up = crossSlope;
            playerCheckLadderZone.rootBone.transform.Rotate(0, isFacingRight ? 90 : -90, 0);

            var rot = playerCheckLadderZone.rootBone.localRotation.eulerAngles;

            if (isFacingRight)
                rot += playerCheckLadderZone.rootBoneRotateOffset;
            else
                rot += (Vector3.right * 180 - playerCheckLadderZone.rootBoneRotateOffset);

            playerCheckLadderZone.rootBone.localRotation = Quaternion.Euler(rot);

            playerCheckLadderZone.rootBone.transform.localPosition += playerCheckLadderZone.rootBonePositionOffset;
        }

        if (playerCheckSlopeAngle.isStandOnTheSlope && isGrounded && !playerCheckWater.isUnderWater)
        {
            if (playerCheckSlopeAngle.hitGround.normal.x > 0 && !isFacingRight)      //force player facing down to the Slope
                Flip();
            else if (playerCheckSlopeAngle.hitGround.normal.x < 0 && isFacingRight)      //force player facing down to the Slope
                Flip();

            playerCheckSlopeAngle.rootBone.localRotation = Quaternion.Euler(playerCheckSlopeAngle.rotateBoneOnSliding);
        }

        var finalPos = new Vector3(transform.position.x, transform.position.y, 0);
        transform.position = finalPos;    //keep the player stay 0 on Z axis

        if (!isHangingTopPipe && input.x != 0)
        {
            if ((!isGrounded && playerCheckLadderZone.isInLadderZone) || (playerCheckSlopeAngle.isStandOnTheSlope && isGrounded)
                || playerCheckDragableObject.isGrabbingTheDragableObject || forcePlayerStanding || forceIdle || isSliding || isWallSliding || isGrabingRope || isJumpingOutFromTheRope)
                ;
            else
                inputLastTime = input;
        }
    }

    public void AddPosition(Vector2 pos)
    {
        characterController.enabled = false;
        transform.position += (Vector3)pos;
        characterController.enabled = true;
    }

    public void SetPosition(Vector2 pos)
    {
        characterController.enabled = false;
        transform.position = (Vector3)pos;
        characterController.enabled = true;
    }

    public void TeleportTo(Vector3 pos)
    {
        if (!isPlaying)
            return;

        StartCoroutine(TeleportCo(pos));
    }

    IEnumerator TeleportCo(Vector3 pos)
    {
        isPlaying = false;

        ControllerInput.Instance.ShowController(false);
        yield return new WaitForSeconds(0.5f);
        characterController.enabled = false;
        transform.position = pos;
        characterController.enabled = true;
        isPlaying = true;
        ControllerInput.Instance.ShowController(true);
    }

    private void CheckStandOnEvent()
    {
        var hasEvent = (IPlayerStandOn)groundHit.collider.gameObject.GetComponent(typeof(IPlayerStandOn));
        if (hasEvent != null)
            hasEvent.OnPlayerStandOn();
    }

    void CheckEnemy()
    {
        var isEnemy = groundHit.collider.GetComponent<SimpleEnemy>();
        if (isEnemy)
        {
            HitAndDie();
        }
    }

    void CheckEnemyAHead()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up * characterController.height * 0.5f,
            characterController.radius,
            isFacingRight ? Vector3.right : Vector3.left,
            out hit, 0.1f, 1 << LayerMask.NameToLayer("Enemy")))
        {
            HitAndDie();
        }
    }

    void CheckSpring()
    {
        var isSpring = groundHit.collider.GetComponent<TheSpring>();
        if (isSpring)
        {
            isSpring.Action();
            Jump(isSpring.pushHeight);
        }
    }

    void Flip()
    {
        if (isSliding || isHangingTopPipe || isIgnorePlayerMovenmentInput || forceIdle)
            return;

        inputLastTime *= -1;
    }

    void GetSlopeVelocity(ref Vector2 vel)
    {
        var crossSlope = Vector3.Cross(groundHit.normal, Vector3.forward);
        vel = vel.x * crossSlope;

        Debug.DrawRay(transform.position, crossSlope * 10);
    }

    [ReadOnly] public bool allowGrabWall = true;
    bool allowCheckWall = true;
    bool firstContactWall = true;
    bool allowGrapNextWall = false;

    void CheckWallSliding()
    {
        isWallSliding = false;

        if (playerCheckWater && playerCheckWater.isInWaterZone())
            return;

        if (!allowCheckWall)
            return;

        if (!isFacingRight)
            wallDirX = -1;
        else if (isFacingRight)
            wallDirX = 1;
        else
            wallDirX = 0;

        if (allowGrabWall)
        {
            if (PlayerState != PlayerState.Ground || isUsingJetpack /* || input.x != (isFacingRight ? 1 : -1) */|| isGrounded || velocity.y >= 0)
            {
                return;
            }

            if (isWallAHead())
            {
                velocity.x = 0;
                if (isFallingFromWall)
                    return;     //stop checking if player in falling state

                if (!isGrounded && !isDead && CanSlidingWall())
                {
                    isWallSliding = true;
                    wallStickTimeCounter -= Time.deltaTime;

                    //if (wallStickTimeCounter < 0)
                    //{
                    //    isWallSliding = false;
                    //    isFallingFromWall = true;
                    //}
                    if (wallStickTimeCounter > 0)
                    {
                        velocityXSmoothing = 0;
                        //velocity.x = 0;

                        if (input.x != wallDirX)
                        {
                            if (input.x == 0)
                            {
                                wallStickTimeCounter -= Time.deltaTime;
                                if (wallStickTimeCounter <= 0)
                                {
                                    isWallSliding = false;
                                    isFallingFromWall = true;
                                    Invoke("AllowCheckWall", 0.2f);
                                    Flip();
                                }

                                //if (velocity.y < -wallSlideSpeedNoHold)
                                //{
                                //    velocity.y = -wallSlideSpeedNoHold;
                                //}
                            }
                            else
                            {
                                wallStickTimeCounter = wallStickTime;      //
                                //                                        //wallSlidingHoldPosition = true;
                                //if (velocity.y < -wallSlideSpeedLookOtherSide)
                                //{
                                //    velocity.y = -wallSlideSpeedLookOtherSide;
                                //}
                            }
                        }
                        else
                        {
                            wallStickTimeCounter = wallStickTime;
                            //if (velocity.y < -wallSlideSpeedHold)
                            //{
                            //    velocity.y = -wallSlideSpeedHold;
                            //}
                        }
                    }
                    else
                    {
                        wallStickTimeCounter = wallStickTime;
                    }
                }
            }
        }
    }

    void AllowCheckWall()
    {
        allowCheckWall = true;
        firstContactWall = true;
        allowGrabWall = true;
    }

    bool CanSlidingWall()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + Vector3.up * (characterController.height - characterController.radius), (isFacingRight ? Vector3.right : Vector3.left) * (characterController.radius + 0.1f));
        if (Physics.Raycast(transform.position + Vector3.up * (characterController.height - characterController.radius), isFacingRight ? Vector3.right : Vector3.left, out hit, characterController.radius + 0.1f, layerAsWall))
            return true;
        else
            return false;
    }

    bool isFallingFromWall = false;

    bool isWallAHead()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up * characterController.height * 0.5f,
            characterController.radius,
            isFacingRight ? Vector3.right : Vector3.left,
            out hit, 0.1f, layerAsWall))
        {
            return true;
        }
        else
            return false;
    }

    public void Victory()
    {
        isRunning = false;
        isPlaying = false;
        GameManager.Instance.FinishGame();
        if (isUsingJetpack)
        {
            UseJetpack(false);
            UpdateJetPackStatus();
        }
    }

    void HitAndDie()
    {
        GetComponent<RagdollToggle>().RagdollActive(true);
 
        if (isDead)
            return;

        SoundManager.PlaySfx(soundHit);
        Die();
    }



    public void Die(bool disappear = false)
    {
        if (isDead)
            return;
        if (playerRopeDetecter.isHoldingRope)
            playerRopeDetecter.JumpOut();

        SoundManager.PlaySfx(soundDie);
        isDead = true;
        velocity.x = 0;
        velocity.y = 0;
       allowUnderSwimming = false;
        allowCheckWall = true;
        firstContactWall = true;
        isRunning = false;
        forceStandingRemain = 0;
        anim.applyRootMotion = true;
        if (isHangingTopPipe)
            DropHangingTopPipe();
        if (isJetpackActived)
            ActiveJetpack(false);

        GameManager.Instance.GameOver();

        if (disappear)
            gameObject.SetActive(false);
    }

    [Header("---HARD FALLING FROM HEIGHT---")]
    [ReadOnly] public bool isFallingFromHeight = false;
    [Tooltip("If player velocity Y lower this value, active the hard falling")]
    public float hardFallingDistance = 10;
    public float hardFallingIdleTime = 1;


    void HandleAnimation()
    {
        if (PlayerState == PlayerState.Ground)
            anim.speed = isRunning ? 1.8f : 1;
        else if (PlayerState == PlayerState.Water)
            anim.speed = 0.75f;

        anim.SetInteger("inputX", (int)input.x);
        anim.SetInteger("inputY", (int)input.y);
        anim.SetBool("isFacingRight", isFacingRight);
        anim.SetFloat("speed", Mathf.Abs(velocity.x));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("height speed", velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
        anim.SetBool("isDead", isDead);
        anim.SetBool("isFallingFromWall", isFallingFromWall);
        anim.SetBool("isInWindZone", PlayerState == PlayerState.Windy);
        anim.SetBool("isHangingTopPipe", isHangingTopPipe);
        anim.SetBool("isInWaterZone", playerCheckWater.isInWaterZone() && !isGrounded);
        anim.SetBool("isHoldingRope", playerRopeDetecter.isHoldingRope);
        anim.SetBool("isSlidingSlope", playerCheckSlopeAngle.isStandOnTheSlope && isGrounded);
        anim.SetInteger("climbingRopeDirection", playerRopeDetecter.reachLimitUp ? 0 : (int)input.y);
        anim.SetBool("isGrabbingTheDragableObject", playerCheckDragableObject.isGrabbingTheDragableObject);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("isSneaking", !playerCheckSmallBridge.isHasGroundOnBothSide);
        anim.SetBool("isInTheSneakingZoneLowWall", playerCheckSneakingWall.isInTheSneakingZone && !playerCheckSneakingWall.isHighWall);
        anim.SetBool("isInTheSneakingZoneHighWall", playerCheckSneakingWall.isInTheSneakingZone && playerCheckSneakingWall.isHighWall);
        anim.SetBool("isFallingFromHeight", isFallingFromHeight);
        anim.SetBool("isUsingJetpack", isUsingJetpack);
        anim.SetBool("isGrabingRope", isGrabingRope);
        anim.SetInteger("numberAirJump", numberAirJumpCounter);
        anim.SetBool("swimming", playerCheckWater.isInWaterZone() && !isGrounded && (velocity != Vector2.zero));

        if (input.x == 0)
            anim.SetInteger("swingDirection", 0);
        else
            anim.SetInteger("swingDirection", ((isFacingRight && input.x == 1) || (!isFacingRight && input.x == -1)) ? 1 : -1);

        if (playerCheckLadderZone)
        {
            anim.SetBool("isInLadderZone", playerCheckLadderZone.isInLadderZone);
        }

        //Check Hard Falling
      
        if (!isFallingFromHeight && (transform.position.y <(lastGroundPos - 0.5f)) && !isGrounded && !isDroppingToLadder && !playerCheckLadderZone.isInLadderZone && climbingState != ClimbingState.ClimbingLedge)
        {
            //Debug.LogError(lastGroundPos);
            RaycastHit hit;
            if (!Physics.Raycast(transform.position, Vector3.down, out hit, hardFallingDistance))      //don't hit anything in this distance => Active hard falling
                isFallingFromHeight = true;
        }

        if (!playerCheckSmallBridge.isHasGroundOnBothSide && isGrounded)
        {
            SetCharacterControllerSlidingSize();
        }
        else if (!isSliding)
            SetCharacterControllerOriginalSize();
    }

    #region ATTACK
    [ReadOnly] public RangeAttack rangeAttack;

    public void RangeAttack()
    {
        if (!isPlaying)
            return;
        if (playerCheckDragableObject.isGrabbingTheDragableObject || isSliding)
            return;

        if (IgnoreControllerInput())
        {
            unableObject.SetActive(true);
            return;
        }

        if (isHangingTopPipe || climbingState == ClimbingState.ClimbingLedge || PlayerState == PlayerState.Windy
            || playerCheckLadderZone.isInLadderZone
            || playerCheckDragableObject.isGrabbingTheDragableObject || playerRopeDetecter.isHoldingRope || playerCheckPushButtonObject.currentButtonObject != null
            || playerCheckWater.isInWaterZone()
            //|| !playerCheckSmallBridge.isHasGroundOnBothSide || playerCheckSneakingWall.isInTheSneakingZone
            )
        {
            unableObject.SetActive(true);
            return;
        }

        if (rangeAttack != null)
        {
            if (rangeAttack.Fire(isFacingRight))
            {
                if(meleeAttack.isWeaponShowing())
                    meleeAttack.ShowWeapon(false);

                anim.SetTrigger("shoot");

                if (!playerCheckSmallBridge.isHasGroundOnBothSide || playerCheckSneakingWall.isInTheSneakingZone)
                {
                    forceStandingRemain = rangeAttack.standingTimeWhenSneaking;
                }
            }
        }
    }

    [HideInInspector] public MeleeAttack meleeAttack;
    public void MeleeAttack()
    {
        if (!isPlaying)
            return;
        if (isSliding || playerCheckDragableObject.isGrabbingTheDragableObject || isUsingJetpack)
            return;

        if (IgnoreControllerInput())
        {
            unableObject.SetActive(true);
            return;
        }

        if (isHangingTopPipe || climbingState == ClimbingState.ClimbingLedge || PlayerState == PlayerState.Windy
            || playerCheckLadderZone.isInLadderZone
            || playerCheckDragableObject.isGrabbingTheDragableObject || playerRopeDetecter.isHoldingRope || playerCheckPushButtonObject.currentButtonObject != null
            || playerCheckWater.isInWaterZone())
        {
            unableObject.SetActive(true);
            return;
        }

        if (meleeAttack != null)
        {
            if (meleeAttack.Attack())
            {
                if (rangeAttack.isWeaponShowing())
                    rangeAttack.ShowWeapon(false);

                anim.SetTrigger("meleeAttack");
                forceStandingRemain = meleeAttack.standingTime;
            }
        }
    }

    float forceStandingRemain = 0;

    #endregion

    [HideInInspector] public RaycastHit groundHit;
    void CheckGround()
    {
       

        isGrounded = false;
        if (velocity.y > 0.1f)
            return;

        if (playerCheckWater.isInWaterZone())
            return;

        if (Physics.SphereCast(transform.position + Vector3.up * 1, characterController.radius * 0.99f, Vector3.down, out groundHit, 2, layerAsGround))
        {
            float distance = transform.position.y - groundHit.point.y;

            if (distance <= (characterController.skinWidth + 0.01f + (DeltaPos.y != 0 ? 0.1 : 0)))      //if standing on the moving platform (deltaPos.y != 0), increase the delect ground to avoid problem
            {
                isGrounded = true;
                numberAirJumpCounter = 0;
                //check if standing on small ledge then force play move
                if (!Physics.Raycast(transform.position, Vector3.down, 1, layerAsGround))
                {
                    if (input.x == 0 && groundHit.point.y > (transform.position.y - 0.1f))
                    {
                        var forceMoveOnLedge = Vector3.zero;
                        if (groundHit.point.x < transform.position.x)
                            forceMoveOnLedge = (transform.position - groundHit.point) * Time.deltaTime * 10 * (isFacingRight ? 1 : -1);
                        else
                            forceMoveOnLedge = (transform.position - groundHit.point) * Time.deltaTime * 10 * (isFacingRight ? -1 : 1);
                        characterController.Move(forceMoveOnLedge);
                    }
                }
            }
        }
    }

    public void GragTheDragableObject(bool grabbing)
    {
        if (isGrounded && grabbing)
        {
            if (playerCheckDragableObject.detectedDragableObject != null)
            {
                playerCheckDragableObject.isGrabbingTheDragableObject = true;
                velocity.x = 0;
                //set player offset position with the contact point to fit with the handler
                SetPosition(new Vector3(playerCheckDragableObject.hit.point.x + (characterController.radius * 2 + playerCheckDragableObject.grabOffsetX) * (isFacingRight ? -1 : 1), transform.position.y, transform.position.z));
                lastPos = transform.position;

                playerCheckDragableObject.offsetBoxAndPlayer = playerCheckDragableObject.detectedDragableObject.transform.position.x - transform.position.x;

            }
        }
        else
            playerCheckDragableObject.isGrabbingTheDragableObject = false;
    }

    [ReadOnly] public Vector3 moveDirection;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead)
            return;

        moveDirection = hit.moveDirection;
        var isTrigger = hit.gameObject.GetComponent<TriggerEvent>();
        if (isTrigger)
        {
            isTrigger.OnContactPlayer();
        }

        if (velocity.y > 1 && hit.moveDirection.y == 1)     //check hit object from below
        {
            velocity.y = 0;
            CheckBlockBrick(hit.gameObject);  //check if hit block with head

        }
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3)
        {
            return;
        }
    }

    void CheckBlockBrick(GameObject obj)
    {
        Block isBlock;
        isBlock = obj.GetComponent<Block>();
        if (isBlock)
        {
            isBlock.BoxHit();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
    }

    private void OnDrawGizmosSelected()
    {
        if (isGrounded)
        {
            Gizmos.DrawWireSphere(groundHit.point, characterController.radius * 0.9f);
        }
    }

    public void Jump(float newForce = -1)
    {

        if (!isPlaying)
            return;

        if (isWallSliding)
        {
            allowCheckWall = false;
            isWallSliding = false;
            //numberOfJumpLeft = 0;
            wallStickTimeCounter = 0;

            if (wallDirX == input.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
                Invoke("AllowCheckWall", 0.35f);
            }
            else if (input.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
                Flip();
                Invoke("AllowCheckWall", 0.1f);
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
                Flip();
                allowGrapNextWall = true;
                Invoke("AllowCheckWall", 0.05f);
            }
            SoundManager.PlaySfx(soundJump, soundJumpVolume);
        }
        else
        {
            if (playerCheckLadderZone.isInLadderZone && !isGrounded)
            {
                if ((isFacingRight && input.x == -1) || (!isFacingRight && input.x == 1))
                {
                    if (newForce == -1)
                        SoundManager.PlaySfx(soundJump, soundJumpVolume);

                    isGrounded = false;
                    numberAirJumpCounter = 0;
                    var _height = newForce != -1 ? newForce : minJumpHeight;
                    velocity.y += Mathf.Sqrt(_height * -2 * gravity);
                    velocity.x = moveSpeed * input.x;
                    characterController.Move(velocity * Time.deltaTime);

                    playerCheckLadderZone.isInLadderZone = false;
                    return;
                }
                else
                    return;
            }

            if (IgnoreControllerInput())
                return;

            if (playerCheckDragableObject.isGrabbingTheDragableObject)
                return;

            //playerCheckLadderZone.isInLadderZone = false;

            if (playerRopeDetecter.isHoldingRope)
            {
                JumpOutFromClimbableRope();
                return;
            }

            if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
                return;

            if (GameManager.Instance.gameState != GameManager.GameState.Playing)
                return;

            if (isUsingJetpack)
                return;

            UpdatePositionOnMovableObject(null);

            wallStickTimeCounter = wallStickTime;

            if (isWallSliding)
            {
                if (newForce == -1)
                    SoundManager.PlaySfx(soundJump, soundJumpVolume);

                isWallSliding = false;
                allowGrabWall = false;
                velocity.y += Mathf.Sqrt(wallSlidingJumpForce.y * -2 * gravity);
            }
            else if (isGrounded && (PlayerState == PlayerState.Ground || PlayerState == PlayerState.Windy))
            {
                if (newForce == -1)
                    SoundManager.PlaySfx(soundJump, soundJumpVolume);

                if (isSliding)
                    SlideOff();

                isGrounded = false;
                var _height = newForce != -1 ? newForce : maxJumpHeight;
                velocity.y += Mathf.Sqrt(_height * -2 * gravity);
                velocity.x = characterController.velocity.x;

                characterController.Move(velocity * Time.deltaTime);

            }
            else if (isInJumpZone)
            {
                if (newForce == -1)
                    SoundManager.PlaySfx(soundJump, soundJumpVolume);

                var _height = maxJumpHeight;
                velocity.y = Mathf.Sqrt(_height * -2 * gravity);
                //velocity.x = characterController.velocity.x;

                characterController.Move(velocity * Time.deltaTime);
                isInJumpZone = false;
                Time.timeScale = 1;
            }
            else if(!isGrounded && (numberAirJumpCounter < numberOfAirJump))
            {
                numberAirJumpCounter++;
                if (numberAirJumpCounter == 1)
                    anim.SetTrigger("doubleJump");
                else if (numberAirJumpCounter == 2)
                    anim.SetTrigger("tripleJump");

                if (newForce == -1)
                    SoundManager.PlaySfx(soundJump, soundJumpVolume);
                var _height = newForce != -1 ? newForce : maxJumpHeight;
                velocity.y = Mathf.Sqrt(_height * -2 * gravity);
                velocity.x = characterController.velocity.x;

                characterController.Move(velocity * Time.deltaTime);
            }
        }
    }

    public void JumpOff()
    {
        if (!isPlaying)
            return;

        var _minJump = Mathf.Sqrt(minJumpHeight * -2 * gravity);
        if (velocity.y > _minJump)
        {
            velocity.y = _minJump;
        }
    }

    //jump and do not allow change the direction until hit the Ground or Grab something
    [HideInInspector] public bool isIgnorePlayerMovenmentInput = false;
    void JumpAndIgnoreInput()
    {
        isIgnorePlayerMovenmentInput = true;
    }

    JumpZoneObj lastJumpZoneObj;
    private void OnTriggerEnter(Collider other)
    {
        if (!isPlaying)
            return;

        if (isDead)
            return;

        var isTrigger = other.GetComponent<TriggerEvent>();
        if (isTrigger)
        {
            isTrigger.OnContactPlayer();
        }

        if (other.gameObject.tag == "Finish")
            Victory();
        else if (other.gameObject.tag == "Deadzone")
        {
            GetComponent<RagdollToggle>().RagdollActive(true);
            print("Dei");
            Die();
        }
        else if (other.gameObject.tag == "Ball")
        {
            GetComponent<RagdollToggle>().RagdollActive(true);
            print("yo");
            Die();
        }



        if (other.gameObject.tag == "Checkpoint")
            SetCheckPoint(other.transform.position);

        if (other.gameObject.tag == "TurnAround")
            Flip();

        var isJumpZone = other.GetComponent<JumpZoneObj>();
        if (lastJumpZoneObj != isJumpZone && isJumpZone != null)
        {
            isInJumpZone = true;
            isJumpZone.SetState(true);

            if (isJumpZone.slowMotion)
            {
                Time.timeScale = 0.1f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Check Jump Zone
        var isJumpZone = other.GetComponent<JumpZoneObj>();
        if (isJumpZone != null)
        {
            isInJumpZone = false;
            isJumpZone.SetState(false);
            if (velocity.y > 0)
                isJumpZone.SetStateJump();
            lastJumpZoneObj = isJumpZone;
            Time.timeScale = 1f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Deadzone")
        {
            GetComponent<RagdollToggle>().RagdollActive(true);
            print("Dead");
            HitAndDie();
        }
        if (collision.gameObject.tag == "Ball")
        {
            GetComponent<RagdollToggle>().RagdollActive(true);
            Die();
        }


    }

    //Call by walk animation
    public void FootStep()
    {
        if (playerCheckDragableObject.isGrabbingTheDragableObject || !playerCheckSmallBridge.isHasGroundOnBothSide
            || playerCheckLadderZone.isInLadderZone
            || (playerCheckSneakingWall.isInTheSneakingZone && !playerCheckSneakingWall.isHighWall) || (playerCheckSneakingWall.isInTheSneakingZone && playerCheckSneakingWall.isHighWall))
            return;

        SoundManager.PlaySfx(soundFootStep, soundFootStepVolume);
    }

    #region MOVE
    float leftButtonCoolerTime = 0.5f;
    float rightButtonCoolerTime = 0.5f;
    int leftButtonCount, rightButtonCount;

    void CheckDoubleTap()
    {
        if (leftButtonCoolerTime > 0)
        {

            leftButtonCoolerTime -= 1 * Time.deltaTime;

        }
        else
        {
            leftButtonCount = 0;
        }

        if (rightButtonCoolerTime > 0)
        {

            rightButtonCoolerTime -= 1 * Time.deltaTime;

        }
        else
        {
            rightButtonCount = 0;
        }
    }

    bool IgnoreControllerInput()
    {
        return forceIdle|| isIgnorePlayerMovenmentInput || climbingState == ClimbingState.ClimbingLedge
            || isDroppingToLadder || (playerCheckSlopeAngle.isStandOnTheSlope && isGrounded) || forceIdle || forcePlayerStanding;
    }

    public void MoveLeft()
    {
        if (isDroppingToLadder)
            return;

        if (isPlaying)
        {
            //input = new Vector2(-1, 0);
            input.x = -1;

            if (playerRopeDetecter.isHoldingRope)
            {
                //playerRopeDetecter.AddForceToRope(playerRopeDetecter.currentRope.transform.forward * playerRopeDetecter.swingForce * input.x, input.x);
                playerRopeDetecter.SwingLeft();
            }
        }
    }

    public void MoveLeftTap()
    {
        if (IgnoreControllerInput())
            return;
        if (!isGrounded)
            return;

        if (leftButtonCoolerTime > 0 && leftButtonCount == 1/*Number of Taps you want Minus One*/)
        {
            isRunning = true;
        }
        else
        {
            leftButtonCoolerTime = 0.5f;
            leftButtonCount += 1;
        }

    }

    //This action is called by the Input/ControllerInput
    public void MoveRight()
    {
        if (isDroppingToLadder)
            return;

        if (isPlaying)
        {
            //input = new Vector2(1, 0);
            input.x = 1;

            if (playerRopeDetecter.isHoldingRope)
            {
                //playerRopeDetecter.AddForceToRope(playerRopeDetecter.currentRope.transform.forward * playerRopeDetecter.swingForce * input.x, input.x);
                playerRopeDetecter.SwingRight();
            }
        }
    }

    public void MoveRightTap()
    {
        if (IgnoreControllerInput())
            return;
        if (!isGrounded)
            return;

        if (rightButtonCoolerTime > 0 && rightButtonCount == 1/*Number of Taps you want Minus One*/)
        {
            isRunning = true;
        }
        else
        {
            rightButtonCoolerTime = 0.5f;
            rightButtonCount += 1;
        }

    }

    //This action is called by the Input/ControllerInput
    public void MoveUp()
    {
        if (playerRopeDetecter.isHoldingRope && input.x != 0)
            return;

        input.y = 1;
    }


    //This action is called by the Input/ControllerInput
    public void MoveDown()
    {
        if (playerRopeDetecter.isHoldingRope && input.x != 0)
            return;

        if (isHangingTopPipe)
            DropHangingTopPipe();

        input.y = -1;

        if (isGrounded && playerCheckLadderZone.CheckLadderBelow(transform.position + Vector3.down * 0.5f, transform.forward, characterController.radius * 3))
        {
            StartCoroutine(DropToLadderCo());
        }
    }

    public void StopMove(int fromDirection = 0)
    {
        //if (input.x != 0 && input.x != fromDirection)
        //    return;

        //input = Vector2.zero;
        //isRunning = false;

        if (fromDirection == 0)     //mean release Up/Down button
        {
            input.y = 0;
        }
        else
        {
            if (input.x != 0 && input.x != fromDirection)
                return;

            //input = Vector2.zero;
            input.x = 0;
            isRunning = false;
        }
    }

    public void Action(bool active)
    {
        if (IgnoreControllerInput())
            return;

        //check and grap the rope point
        if (currentAvailableRope != null)
        {
            if(active)
                GrabRope();
            else
                GrabRelease();
        }

        GragTheDragableObject(active);

        if (active)
        {
            CheckPushButtonObject();
        }
    }

    #endregion

    void CheckPushButtonObject()
    {
        if (playerCheckPushButtonObject.currentButtonObject != null)
        {
            anim.SetTrigger("pushButton");
            var _newPos = new Vector2(playerCheckPushButtonObject.currentButtonObject.transform.position.x, transform.position.y);
            SetPosition(_newPos);

            //ForcePlayerStanding(true);
            playerCheckPushButtonObject.currentButtonObject.Action();
        }
    }

    #region LEDGE
    public enum ClimbingState { None, ClimbingLedge }
    public LayerMask layersCanGrab;
    [Header("------CLIBMING LEDGE------")]
    [ReadOnly] public ClimbingState climbingState;
    [Tooltip("Ofsset from ledge to set character position")]
    public Vector3 climbOffsetPos = new Vector3(0, 1.3f, 0);
    [Tooltip("Adjust to fit with the climbing animation length")]
    public float climbingLedgeTime = 1;
    public Transform verticalChecker;
    public Transform verticalCheckerClimbLadder;
    public float verticalCheckDistance = 0.5f;

    [Header("---CHECK LOW CLIMB 1m---")]
    [Tooltip("Ofsset from ledge to set character position")]
    public Vector3 climbLCOffsetPos = new Vector3(0, 1f, 0);
    public float climbingLBObjTime = 1;


    Transform ledgeTarget;      //use to update ledge moving/rotating
    Vector3 ledgePoint;

    bool CheckLowerLedge()      //check lower ledge
    {
        RaycastHit hitVertical;
        RaycastHit hitGround;
        RaycastHit hitHorizontal;

        if (Physics.Linecast(verticalChecker.position, new Vector3(verticalChecker.position.x, transform.position.y + characterController.stepOffset, transform.position.z), out hitVertical, layersCanGrab, QueryTriggerInteraction.Ignore))
        {
            if (hitVertical.normal == Vector3.up)
            {
                if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y, verticalChecker.position.z), Vector3.down, out hitGround, 3, layersCanGrab, QueryTriggerInteraction.Ignore))
                {
                    if ((int)hitGround.distance <= 1)
                    {
                        if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), isFacingRight ? Vector3.right : Vector3.left, out hitHorizontal, 2, layersCanGrab, QueryTriggerInteraction.Ignore))
                        {
                            ledgePoint = new Vector3(hitHorizontal.point.x, hitVertical.point.y, transform.position.z);
                            //check if the top of ledge is clear or not
                            var hits = Physics.OverlapSphere(ledgePoint + (isFacingRight ? Vector3.right : Vector3.left) * 0.5f + Vector3.up * 0.5f,
                                0.1f, layersCanGrab);
                            if (hits.Length == 0)
                            {
                                ledgeTarget = hitVertical.transform;
                                velocity = Vector2.zero;
                                characterController.Move(velocity);
                                transform.position = CalculatePositionOnLedge(climbOffsetPos);
                                //reset other value
                                isWallSliding = false;
                                if (isSliding)
                                    SlideOff();

                                StartCoroutine(ClimbingLedgeCo(true));
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    bool CheckLedge(Vector3 checkPosition)       //check higher ledge
    {
        RaycastHit hitVertical;
        RaycastHit hitHorizontal;

        //Debug.DrawRay(checkPosition, Vector3.down * (verticalCheckDistance + (playerCheckLadderZone.isInLadderZone ? 1 : 0)));
        //Debug.LogError("CheckLedge");
        if (Physics.Raycast(checkPosition, Vector2.down, out hitVertical, verticalCheckDistance + (playerCheckLadderZone.isInLadderZone? 1: 0), layersCanGrab, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, checkPosition.z), (isFacingRight ? Vector3.right : Vector3.left) * 2);
            if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, checkPosition.z), isFacingRight ? Vector3.right : Vector3.left, out hitHorizontal, 2, layersCanGrab, QueryTriggerInteraction.Ignore))
            {
                ledgePoint = new Vector3(hitHorizontal.point.x, hitVertical.point.y, transform.position.z);
                ;
                //check if the top of ledge is clear or not
                var hits = Physics.OverlapSphere(ledgePoint + (isFacingRight ? Vector3.right : Vector3.left) * 0.5f + Vector3.up * 0.5f,
                    0.1f, layersCanGrab);
                if (hits.Length == 0)
                {
                    ledgeTarget = hitVertical.transform;

                    velocity = Vector2.zero;
                    characterController.Move(velocity);
                    transform.position = CalculatePositionOnLedge(climbOffsetPos);
                    //reset other value
                    isWallSliding = false;
                    if (isSliding)
                        SlideOff();

                    playerCheckLadderZone.isInLadderZone = false;
                    if (isFallingFromHeight)
                        isFallingFromHeight = false;

                    StartCoroutine(ClimbingLedgeCo(false));
                    return true;
                }
            }
        }
        return false;
    }

    private Vector3 CalculatePositionOnLedge(Vector3 offset)
    {
        Vector3 newPos = new Vector3(ledgePoint.x - (characterController.radius * (isFacingRight ? 1 : -1)) - offset.x, ledgePoint.y - offset.y, transform.position.z);

        return newPos;
    }

    IEnumerator ClimbingLedgeCo(bool lowClimb)
    {
        isRunning = false;
        climbingState = ClimbingState.ClimbingLedge;

        if (lowClimb)
            anim.SetBool("lowLedgeClimbing", true);
        else
            anim.SetBool("ledgeClimbing", true);

        HandleAnimation();
        yield return new WaitForSeconds(Time.deltaTime);
        characterController.enabled = false;
        anim.applyRootMotion = true;
        transform.position = CalculatePositionOnLedge(lowClimb ? climbLCOffsetPos : climbOffsetPos);
        yield return new WaitForSeconds(Time.deltaTime);
        transform.position = CalculatePositionOnLedge(lowClimb ? climbLCOffsetPos : climbOffsetPos);

        yield return new WaitForSeconds(lowClimb ? climbingLBObjTime : climbingLedgeTime);

        SetPosition(ledgePoint + Vector3.right * characterController.radius * (isFacingRight ? 1 : -1));    //make sure the player stand safe on the ground
        LedgeReset();
    }

    void LedgeReset()
    {
        characterController.enabled = true;
        anim.applyRootMotion = false;
        climbingState = ClimbingState.None;
        anim.SetBool("ledgeClimbing", false);
        anim.SetBool("lowLedgeClimbing", false);
        ledgeTarget = null;
    }

    #endregion

    #region ROPE SYSTEM

    //apply for the climbable rope
    Vector2 lastPos;
    public void JumpOutFromClimbableRope()
    {
        playerRopeDetecter.JumpOut();
        deltaPosition.Normalize();
        velocity = new Vector2(playerRopeDetecter.jumpOutForce * input.x, minJumpHeight);

        //Debug.LogError("ROPE JUMP = " + velocity);
        characterController.Move(velocity * Time.deltaTime);
        Time.timeScale = 1;
        SoundManager.PlaySfx(soundRopeJump);

        ignoreInputControl = true;
        Invoke("AllowInputControlAgainCo", 0.5f);
    }

    void AllowInputControlAgainCo()
    {
        ignoreInputControl = false;
    }

    [ReadOnly] public bool isDroppingToLadder = false;
    bool isWaitingDropToGrabTheLadder = false;

    IEnumerator DropToLadderCo()
    {
        if (isDroppingToLadder)
            yield break;

        bool _isFacingRight = isFacingRight;        //store the facing to calculating the ladder after droping
        input.x = 0;
        isDroppingToLadder = true;
        isWaitingDropToGrabTheLadder = true;
        if (isSliding)
            SlideOff();
        if (isRunning)
            isRunning = false;

        velocity = Vector2.zero;
        characterController.Move(velocity);
        //get the ledge X position
        RaycastHit hitWall;
        if (Physics.Raycast(playerCheckLadderZone.ladderBelowHit.point, isFacingRight ? Vector3.left : Vector3.right, out hitWall, 2, layerAsWall))
        {
            SetPosition(new Vector2(hitWall.point.x, transform.position.y));
        }

        isGrounded = false;


        anim.SetBool("isDropToLadder", true);

        HandleAnimation();
        yield return new WaitForSeconds(Time.deltaTime);

        characterController.enabled = false;
        anim.applyRootMotion = true;
      
        yield return new WaitForSeconds(playerCheckLadderZone.droppingToLadderTime);

        anim.applyRootMotion = false;
        characterController.enabled = true;
        anim.SetBool("isDropToLadder", false);

        //make sure player hold the ladder
        RaycastHit hitLadder;
       
        if (Physics.Raycast(transform.position + Vector3.right * (isFacingRight ? 1 : -1), isFacingRight ? Vector3.left : Vector3.right, out hitLadder, 3, playerCheckLadderZone.layerAsLadder))
        {
            //Debug.LogError("h = " + hitLadder.point + "/" + (hitLadder.point + Vector3.right * (characterController.radius + 0.05f) * (isFacingRight ? 1 : -1)));
            SetPosition(hitLadder.point + Vector3.right * (characterController.radius + 0.0f) * (isFacingRight ? 1 : -1));
        }

        Flip();
        transform.forward = new Vector3(isFacingRight ? 1 : -1, 0, 0);
       
        isDroppingToLadder = false;

        isWaitingDropToGrabTheLadder = false;
    }

    public void DropOutClimbableRope()
    {
        velocity = Vector2.zero;
    }

    #endregion

    #region PIPE
    public void SetInThePipe(bool state, Vector2 direction)
    {
        if (MovingInPipeCoWork != null)
            StopCoroutine(MovingInPipeCoWork);
        if (state)
        {
            isMovingInPipe = true;
            MovingInPipeCoWork = MovingInPipeCo(direction);
            StartCoroutine(MovingInPipeCoWork);
        }
        else
        {
            isMovingInPipe = false;
            ControllerInput.Instance.StopMove(0);
            velocity.y = -10;
        }
    }

    bool isMovingInPipe = false;
    float movingPipeSpeed = 1.5f;
    IEnumerator MovingInPipeCoWork;
    IEnumerator MovingInPipeCo(Vector2 direction)
    {

        Vector2 movingTarget = Vector2.zero;
        if (direction.y == -1)
        {
            movingTarget = transform.position + Vector3.down * 2;

        }
        else if (direction.x == 1)
        {
            movingTarget = transform.position + Vector3.right;
        }

        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, movingTarget, movingPipeSpeed * Time.deltaTime);
            yield return null;
        }
    }
    #endregion

    #region HANGING TOP PIPE
    [Header("---HANGING TOP PIPE---")]
    public LayerMask layerTopPipe;
    [ReadOnly] public bool isHangingTopPipe = false;
    public Vector2 hangingOffset = new Vector2(0, -1.2f);
    public float hangingMoveSpeed = 3;

    void CheckGrabHangingTopPipe()
    {
        if (isHangingTopPipe)
            return;
        if (velocity.y <= 0)
            return;

        RaycastHit hit;

        if (Physics.SphereCast(transform.position + Vector3.up * characterController.radius, characterController.radius, Vector3.up, out hit, characterController.height - characterController.radius, layerTopPipe))
        {
            if (!isFacingRight)
                Flip();
            isRunning = false;
            isHangingTopPipe = true;
            characterController.enabled = false;
            transform.position = hit.point + (Vector3)hangingOffset;
            characterController.enabled = true;
        }
    }

    void CheckIfInTopPipe()
    {
        RaycastHit hit;

        if (!Physics.SphereCast(transform.position + Vector3.up * 0.5f + Vector3.right * 0.2f * input.x, characterController.radius, Vector3.up, out hit, characterController.height, layerTopPipe))
        {
            DropHangingTopPipe();
        }
    }

    public void DropHangingTopPipe()
    {
        isHangingTopPipe = false;
    }
    #endregion

    private void OnAnimatorMove()
    {
        // Vars that control root motion
        if (!anim || !anim.applyRootMotion)
            return;

        bool useRootMotion = true;
        bool verticalMotion = true;
        bool rotationMotion = true;

        Vector3 multiplier = Vector3.one;

        if (Mathf.Approximately(Time.deltaTime, 0f) || !useRootMotion) { return; } // Conditions to avoid animation root motion

        Vector3 delta = anim.deltaPosition;

        delta.z = 0;
        delta = transform.InverseTransformVector(delta);
        delta = Vector3.Scale(delta, multiplier);
        delta = transform.TransformVector(delta);

        Vector3 vel = (delta) / Time.deltaTime; // Get animator movement

        if (!verticalMotion)
            vel.y = characterController.velocity.y; // Preserve vertical velocity

        characterController.Move(vel * Time.deltaTime);

        Vector3 deltaRot = anim.deltaRotation.eulerAngles;

        if (rotationMotion)
            transform.rotation *= Quaternion.Euler(deltaRot);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
       
        Die(instigator.GetComponent<EnemyFlowerMonster>() != null);
       
    }

    public void GetShoot()
    {
        isPlaying = false;
        GetComponent<RagdollToggle>().RagdollActive(true);

        anim.SetTrigger("die-getshoot");
        Die();
        GameManager.Instance.GameOver();

       
    }

    [ReadOnly] public bool isRunning = false;

    #region JET PACK
    [Header("---JET PACK---")]
    public float jetForce = 5;
    public float jetpackDrainTimeOut = 5f;
    [ReadOnly] public float jetpackRemainTime;

    public GameObject jetpackObj;
    public AudioClip jetpackSound;
    [Range(0f, 1f)]
    public float jetpackSoundVolume = 0.5f;
    AudioSource jetpackAScr;
    public ParticleSystem[] jetpackEmission;
    public GameObject jetpackReleaseObj;
    [ReadOnly] public bool isJetpackActived = false;
    [ReadOnly] public bool isUsingJetpack = false;

    public void ActiveJetpack(bool active)
    {
        if (active)
        {
            if (isSliding)
                SlideOff();

            jetpackRemainTime = jetpackDrainTimeOut;
            isJetpackActived = true;
            jetpackObj.SetActive(true);
        }
        else if (isJetpackActived)
        {
            isJetpackActived = false;
            isUsingJetpack = false;
            jetpackObj.SetActive(false);

            var obj = Instantiate(jetpackReleaseObj, jetpackObj.transform.position, jetpackReleaseObj.transform.rotation);
            obj.GetComponent<Rigidbody>().velocity = new Vector2(isFacingRight ? -1 : 1, 2);
            Destroy(obj, 2);
            lastGroundPos = transform.position.y;
        }
    }

    public void AddJetpackFuel()
    {
        jetpackRemainTime = jetpackDrainTimeOut;
    }

    public void UseJetpack(bool use)
    {
        if (!isJetpackActived)
            return;

        if (climbingState != ClimbingState.None)
            return;

        if (jetpackRemainTime <= 0)
            return;

        if (isWallSliding)
            isWallSliding = false;

        if (isSliding)
            SlideOff();

        isFallingFromWall = false;      //reset falling state if it is happing
        wallStickTimeCounter = wallStickTime;       //set reset wall stick timer when on ground

        if (isUsingJetpack)
            isRunning = false;

        isUsingJetpack = use;
    }

    void UpdateJetPackStatus()
    {
        for (int i = 0; i < jetpackEmission.Length; i++)
        {
            var emission = jetpackEmission[i].emission;
            emission.enabled = isUsingJetpack;
            jetpackAScr.volume = (isUsingJetpack & GlobalValue.isSound) ? jetpackSoundVolume : 0;
        }
    }

    #endregion

    #region SLIDING
    [Header("---SLIDING---")]
    public float slidingTime = 1;
    public float slidingCapsultHeight = 0.8f;
    float originalCharHeight, originalCharCenterY;
    [ReadOnly] public bool isSliding = false;
    public AudioClip soundSlide;

    public void SlideOn()
    {
        if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
            return;

        if (GameManager.Instance.gameState != GameManager.GameState.Playing)
            return;

        if (!isGrounded)
            return;

        if (isSliding)
            return;

        if (isUsingJetpack)
            return;

        SoundManager.PlaySfx(soundSlide);
        isSliding = true;

        //characterController.height = slidingCapsultHeight;
        //var _center = characterController.center;
        //_center.y = slidingCapsultHeight * 0.5f;
        //characterController.center = _center;

        SetCharacterControllerSlidingSize();

        Invoke("SlideOff", slidingTime);
    }

    void SlideOff()
    {
        if (!isSliding)
            return;

        if (isUsingJetpack)
            return;

        if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
            return;

        //characterController.height = originalCharHeight;
        //var _center = characterController.center;
        //_center.y = originalCharCenterY;
        //characterController.center = _center;

        SetCharacterControllerOriginalSize();

        isSliding = false;
    }
    #endregion

    void SetCharacterControllerSlidingSize()
    {
        
        characterController.height = slidingCapsultHeight;
        var _center = characterController.center;
        _center.y = slidingCapsultHeight * 0.5f;
        characterController.center = _center;
    }

    void SetCharacterControllerOriginalSize()
    {
        characterController.height = originalCharHeight;
        var _center = characterController.center;
        _center.y = originalCharCenterY;
        characterController.center = _center;
    }

    #region
    [Header("---ROPE--")]
    public Vector3 rotateAxis = Vector3.forward;
    public float speed = 100;
    public float releaseForce = 10;
    float distance, releasePointY;
    public float ropeCheckRadius = 6;
    [Tooltip("draw rope offset")]
    public Vector2 grabOffset = new Vector2(0, 1.6f);
    LineRenderer ropeRenderer;
    [ReadOnly] public bool isGrabingRope = false;
    [ReadOnly] public RopePoint currentAvailableRope;
    [ReadOnly] public bool isJumpingOutFromTheRope = false;
    public LayerMask layerAsRope;
    RopePoint lastRopePointObj;

    void CheckRopeInZone()
    {
        if (isGrabingRope)
            return;

        var hits = Physics.OverlapSphere(transform.position + Vector3.up * characterController.height * 0.5f, ropeCheckRadius, layerAsRope);

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (inputLastTime.x > 0)
                {
                    if (hits[i].transform.position.x > transform.position.x)
                    {
                        currentAvailableRope = hits[i].GetComponent<RopePoint>();
                        if (lastRopePointObj != currentAvailableRope)
                        {
                            if (currentAvailableRope.slowMotion)
                                Time.timeScale = 0.1f;
                        }
                        else
                            currentAvailableRope = null;
                    }
                    else
                        currentAvailableRope = null;
                }
                else
                {
                    if (hits[i].transform.position.x < transform.position.x)
                    {
                        currentAvailableRope = hits[i].GetComponent<RopePoint>();
                        if (lastRopePointObj != currentAvailableRope)
                        {
                            if (currentAvailableRope.slowMotion)
                                Time.timeScale = 0.1f;
                        }
                        else
                            currentAvailableRope = null;
                    }
                    else
                        currentAvailableRope = null;
                }
            }
        }
        else
        {
            if (currentAvailableRope != null)       //set time scale back to normal if it active the slow motion before but player don't grab
            {
                if (currentAvailableRope.slowMotion)
                    Time.timeScale = 1;
            }

            currentAvailableRope = null;
        }
    }

    public void GrabRope()
    {
        if (isGrabingRope)
            return;

        if (isGrounded)
            return;     //don't allow grab rope when standing on ground

        if (lastRopePointObj != currentAvailableRope)
        {
            if (currentAvailableRope.slowMotion)
                Time.timeScale = 1;

            lastRopePointObj = currentAvailableRope;
            isGrabingRope = true;
            SoundManager.PlaySfx(soundGrap);
            distance = Vector2.Distance(transform.position, currentAvailableRope.transform.position);
            releasePointY = currentAvailableRope.transform.position.y - distance / 10f;
        }
    }

    public void GrabRelease()
    {
        if (!isGrabingRope)
            return;

        velocity = releaseForce * transform.forward;
        characterController.Move(velocity * Time.deltaTime);
        Time.timeScale = 1;
        SoundManager.PlaySfx(soundRopeJump);
        isGrabingRope = false;

        isJumpingOutFromTheRope = true;
        Invoke("DisableJumpOutFormTheRopeBoolean", 0.8f);
    }

    void DisableJumpOutFormTheRopeBoolean()
    {
        isJumpingOutFromTheRope = false;
        lastRopePointObj = null;
    }

    #endregion
}