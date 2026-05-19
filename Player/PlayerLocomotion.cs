using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlazeAISpace;

namespace etchebarren
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public static PlayerLocomotion instance;

        [Header("General Parameters")]
        public string gender = "mars";

        [Header("General References")]
        public PlayerStats playerStats;
        Transform cameraObject;
        CameraHandler cameraHandler;
        [SerializeField] InputHandler inputHandler;
        [SerializeField] Vector3 moveDirection;
        public PlayerAudioManager playerAudioManager;
        public Transform myTransform;
        public AnimatorHandler animatorHandler;
        public AnimatorHandler animatorHandlerMars;
        public AnimatorHandler animatorHandlerVenus;
        public WaypointManager waypointManager;
        public CompanionBehaviour companionBehaviour;
        private Coroutine attackFlagFailsafe;
        public MountAnimEvents mountAnimEvents;
        public PlayerInventory playerInventory;

        [Header("General Flags")]
        public bool isSprinting = false;
        public bool isDodging = false;
        public bool isCrouching = false;
        public bool isHeavyAttacking = false;
        public bool canHeavyAttack = true;
        public bool isAirComboing = false;
        public bool canStartNewAirCombo = true;
        public bool inCombat = true;
        public bool isSwimming = false;
        public bool canSmallSplash = true;
        public bool cancelMount = false;
        public bool isClimbing = false;
        public bool useUnarmedHeavyA = true;

        [Header("General Timers")]
        public float heavyChargeTimer = 0.0f;
        public float sprintTimer = 0.0f;

        [Header("Movement Settings")]
        Vector3 projectedVelocity;
        [HideInInspector] public new Rigidbody rigidbody;
        [SerializeField] float blockingSpeed = 3.0f; //seperate into walk/run
        [SerializeField] float crouchingSpeed = 2.0f; //seperate into walk/run
        [SerializeField] float walkingSpeed = 2.0f; //seperate into walk/run
        [SerializeField] float runningSpeed = 4.0f; //seperate into walk/run
        [SerializeField] float sprintingSpeed = 10f; //seperate into walk/run
        [SerializeField] float swimmingSpeed = 2.0f;
        [SerializeField] float swimmingFastSpeed = 3.0f;
        [SerializeField] float rotationSpeed = 10;
        [SerializeField] float lockedOnSpeed = 2.0f;
        public float maxRootMotionDistance = 1.2f;
        private Vector3 rollDirection;
        public CapsuleCollider mainCollider;
        private Vector3 smoothNormal = Vector3.zero; // added 4/6

        [Header("Ground Check & Jumping")]
        [SerializeField] Vector3 yVelocity;
        public float gravityScale = 1.5f;
        public float jumpForce = 550.0f;
        public float inAirTimer = 0;
        public float lastAirTimer = 0.0f;
        [SerializeField] float groundedCheckSphereRadius = 0.3f;
        [SerializeField] LayerMask groundLayers;
        public Transform groundCheckSphereLocation;
        public float airAttackUpwardForce = 10.0f;
        public int airDashTriggered = 0;
        public float airDashForwardForce = 30.0f;
        public bool isAirDashing = false;
        public float airDashingGravityScale = 0.25f;

        [Header("Swimming")]
        public GameObject swimCollider;
        public float targetHeight = 0f; // accounts for height of player object
        [SerializeField] GameObject waterPlane;
        public float playerOffset = 1.3f;
        public float buoyancy = 1f;
        public CapsuleCollider[] damageColliders;
        [SerializeField] float newYPosition = 0f;
        public GameObject bigSplashPrefab;
        public GameObject smallSplashPrefab;
        public GameObject ripplePrefab;
        public float minRippleInterval = 0.5f;
        public float maxRippleInterval = 1.0f;
        [Range(0f, 1f)]
        public float rippleChance = 0.8f;
        public Coroutine rippleSpawner;

        [Header("Steps and Stairs")]
        public bool touchingShallowWater = false;
        public bool touchingDeepWater = false;
        [SerializeField] GameObject stepRayUpper;
        [SerializeField] GameObject stepRayLower;
        public float stepHeight = 0.3f;
        public float stepSmooth = 2f;
        public float lowerStepRaycastLength = 0.255f;
        public float upperStepRaycastLength = 0.299f;
        public float lowerStepRaycastLength45 = 0.300f;
        public float upperStepRaycastLength45 = 0.344f;
        public LayerMask stepLayers;
        public Vector3 stepDirection;
        [SerializeField] Vector3 raycastHeightOffset; // Used to increase height per raycast and essentially create a raycast plane

        [Header("Fall Corrector / Snap To Ground")]
        public float snapDownDistance = 0.2f;
        public float jumpSnapDownDelay = 1.0f;
        public bool canSnapDown = true;

        [Header("Attack-Related Values")]
        public int attackCombo = 0;
        private Coroutine moveAfterAttackCoroutine;
        private Coroutine chargeAttackDelay;
        private Coroutine determineHeavyAttackInput;

        [Header("Combat References")]
        public BlockingCollider blockingCollider;
        public DamageTriggerCollider horseAIAttackHitBox;
        public int currentSpellIndex;
        public bool usingItem = false;

        [Header("Test Values")]
        public bool chargeAttackUnlocked = true;
        public bool airAttackUnlocked = true;
        private Vector3 previousEulerAngles;
        private int lockedOnRotateDirection = 0; //This is used for spinning(legs) animation when locked on
        public float spinAnimThreshold = 0.1f;
        public bool heavyAttackRootMotion = true;

        [Header("Mount")]
        public MountManager mountManager;
        public WhistleIconManager whistleIconManager;
        public Transform playerHolder;
        public Transform mountStartPos_left;
        public Transform mountStartPos_right;
        public bool onMount = false;
        public float cameraOverrideDuration = 3.0f;
        public MountLocomotion mountLocomotion;
        public GameObject mountPrompt;
        public GameObject mountTriggers;
        public GameObject mount;
        public MountStats mountStats;
        public bool mountingComplete = true;
        public bool dismountingComplete = true;
        public GameObject horseMountVersion;
        public GameObject horseAIVersion;
        public LayerMask layerMask;
        public Collider dismountLeftCollider;
        public Collider dismountRightCollider;
        public GameObject minimapPlayerIcon;
        public CapsuleCollider playerFeetCollider;
        public CapsuleCollider playerSidesCollider;
        public MountTrigger leftMountTrigger;
        public MountTrigger rightMountTrigger;

        [Header("Pushable Object")]
        public PushableObject currentPushable;
        public Transform handTransformM;
        public Transform handTransformF;

        [Header("Root Motion Sphere Check")]
        public Transform rootMotionSphereCheckLocation;
        public float rootMotionSphereCheckRadius = 1.0f;
        public LayerMask rootMotionSphereCheckLayers;
        public LayerMask rootMotionSphereCheckLayersDodge;

        [Header("Crouching Settings")]
        public LayerMask standingCheckLayers;
        [SerializeField] private float primaryColliderCrouchedHeight = 0.6f;
        [SerializeField] private float sidesColliderCrouchedHeight = 0.55f;
        //[SerializeField] private float primaryColliderCrouchedCenter = 0.6f;
        //[SerializeField] private float sidesColliderCrouchedCenter = 0.6f;
        [Header("Crouching References - Set Dynamically in AnimatorHandler")]
        public float primaryColliderStandingHeight;
        public float sidesColliderStandingHeight;
        //public Vector3 primaryColliderStandingCenter;
        //public Vector3 sidesColliderStandingCenter;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepRayLower.transform.position.y + stepHeight, stepRayUpper.transform.position.z);
        }

        void Start()
        {
            cameraHandler = CameraHandler.instance;
            rigidbody = GetComponent<Rigidbody>();
            SetGender(gender);
            cameraObject = Camera.main.transform;
            //myTransform = transform;

            previousEulerAngles = myTransform.rotation.eulerAngles; //used for determining if character is spinning while locked on

            playerOffset = swimCollider.transform.localPosition.y;

            animatorHandler.Initialize();
        }

        public void Update()
        {
            inputHandler.TickInput(Time.deltaTime); 
            // used to be in FixedUpdate, using FixedDeltaTime
            // caused some frame dependence in target switching, look out for new issues post change.
        }

        public void FixedUpdate()
        {
            // Having colliders (head, arms, etc) throws off the rigidbody's center of mass and tensor inertia rotaion because unity automatically includes all attached rigidbodies when
            // Calculating center of mass and tendor inertia rotation. So we set them manuallly to prevent throwing off our rigidbodies. This caused our player to slowly sink.
            //rigidbody.centerOfMass = mainCollider.center;
            //rigidbody.inertiaTensorRotation = Quaternion.identity;

            if (onMount) return;

            if (!onMount && !isSwimming && !isClimbing)
            {
                float _gravityScale = gravityScale;
                if (isAirDashing) _gravityScale = airDashingGravityScale;
                rigidbody.AddForce(Vector3.down * 9.81f * _gravityScale, ForceMode.Acceleration);
            }
            else if (isSwimming)
            {
                //if (waterPlane == null) SetSwimming(false, null);

                targetHeight = waterPlane.transform.position.y - playerOffset;

                if (inputHandler.isGrounded)
                {
                    // Smoothly interpolate between the current y position and the target height
                    newYPosition = Mathf.Lerp(myTransform.position.y, targetHeight, Time.fixedDeltaTime * buoyancy);
                }
                else
                {
                    newYPosition = targetHeight;
                }
                // Update the object's position with the interpolated y value OR the water level
                myTransform.position = new Vector3(myTransform.position.x, newYPosition, myTransform.position.z);
            }

            // Migrated from update:

            animatorHandler.anim.SetBool("isGrounded", inputHandler.isGrounded);
            float delta = Time.fixedDeltaTime; // changed from deltaTime to fixedDeltaTime on 4/6
            float speed = 0;

            //inputHandler.TickInput(delta); // moved to update on 4/25, watch for issues

            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;
            moveDirection.y = 0;
            moveDirection.Normalize();
            stepDirection = moveDirection;
            
            HandleSprinting();

            if (isSwimming)
            {
                if (isSprinting)
                {
                    speed = swimmingFastSpeed;
                }
                else
                {
                    speed = swimmingSpeed;
                }
            }
            else if (isSprinting)
            {
                speed = sprintingSpeed;
            }
            else if (isCrouching)
            {
                speed = crouchingSpeed;
            }
            else if (inputHandler.isBlocking)
            {
                speed = blockingSpeed;
            }
            else if (inputHandler.lockedOn)
            {
                speed = lockedOnSpeed;
            }
            else
            {
                if (inputHandler.moveAmount > 0.55f)
                {
                    speed = runningSpeed;
                }
                else if (inputHandler.moveAmount <= 0.55f)
                {
                    speed = walkingSpeed;
                }
            }

            //speed = movementSpeed;
            moveDirection *= speed;

            if (isSwimming)
            {
                if (inputHandler.canRotate && !onMount) // moved up 4/6
                {
                    HandleRotation(delta);
                }

                projectedVelocity = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
                projectedVelocity.y = 0f;
                rigidbody.velocity = projectedVelocity;

                if (inputHandler.lockedOn && !isSprinting)
                {
                    rigidbody.velocity = projectedVelocity;
                    animatorHandler.UpdateAnimatorValues(inputHandler.vertical, inputHandler.horizontal, isSprinting, inputHandler.isBlocking, inputHandler.lockedOn, isSwimming);
                }
                else
                {
                    rigidbody.velocity = projectedVelocity;
                    animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, isSprinting, inputHandler.isBlocking, inputHandler.lockedOn, isSwimming);
                }

                //if (inputHandler.canRotate && !onMount)
                //{
                //    HandleRotation(delta);
                //}

                HandleGroundCheck();
                return;
            }

            // 1. Start handling player movement, first get the normal plane of the ground underneath the player
            GetPlaneNormals();

            // 2. Project the moveDirection onto the plane defined by the slope's normal.
            // This ensures the movement is adjusted for the ground's slope angle, so the character moves along the surface according to environment.
            smoothNormal = Vector3.Slerp(smoothNormal, normalVector, 0.1f);
            Vector3 moveOnSlope = Vector3.ProjectOnPlane(moveDirection, smoothNormal); // was normalVector prior to smoothVector

            // 3. For horizontal movement, we want to align the movement with the desired direction of travel, which is represented by moveDirection.
            // However, we only care about the horizontal component of moveDirection. We ignore the vertical component (y-axis), 
            // because we don't want any unnecessary vertical movement, especially when dealing with slopes.
            Vector3 forwardDirection = moveDirection;
            forwardDirection.y = 0; // Set the y component to zero, effectively ignoring the vertical component for horizontal movement.

            // 5. Normalize the forward direction to ensure that the speed remains consistent, regardless of the slope's steepness.
            // Without normalizing, the movement could become erratic or slide in unexpected directions due to the varying magnitude of moveDirection.
            forwardDirection.Normalize();

            // 5. Now, we project the adjusted horizontal move direction (forwardDirection) onto the plane defined by the slope's normal.
            // This ensures that the character's movement stays aligned with the slope, while ignoring any unwanted vertical influence.
            Vector3 moveDirectionInSlope = Vector3.Project(moveOnSlope, forwardDirection);

            // 6. Multiply by the desired movement speed. This determines how fast the player moves along the slope in the desired direction.
            // The moveDirectionInSlope is normalized to ensure that the speed remains consistent and not affected by the steepness of the slope.
            Vector3 finalVelocity = moveDirectionInSlope.normalized * speed; // Normalize to keep movement speed constant.

            // 7. Preserve the vertical velocity (gravity, jumping, etc.) from the rigidbody's current velocity and apply it to the final velocity.
            // This keeps the player grounded and allows for proper interaction with gravity or jumping mechanics, preventing any unnatural movement or "floating."
            finalVelocity.y = rigidbody.velocity.y; // Keep the vertical velocity intact.

            if (inputHandler.canRotate && !onMount)
            {
                HandleRotation(delta);
            }

            if (inputHandler.canMove && inputHandler.isGrounded && !isClimbing) //added climbing condition 4/16
            {
                // Apply calculated velocitys
                rigidbody.velocity = finalVelocity;
                
                if (inputHandler.lockedOn && !isSprinting)
                {
                    animatorHandler.UpdateAnimatorValues(inputHandler.vertical, inputHandler.horizontal, isSprinting, inputHandler.isBlocking, inputHandler.lockedOn, isSwimming);
                }
                else
                {
                    animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, isSprinting, inputHandler.isBlocking, inputHandler.lockedOn, isSwimming);
                }
            }
            else if (inputHandler.isGrounded)
            {
                rigidbody.velocity *= 0.9f;   
                animatorHandler.UpdateAnimatorValues(0, 0, isSprinting, inputHandler.isBlocking, inputHandler.lockedOn, isSwimming);
            }

            HandleGroundCheck();

            // Falling
            if (!inputHandler.isGrounded && !inputHandler.isJumping && !isSwimming && canSnapDown)
            {
                //Check minimum fall distance and snap to ground if within snap range
                RaycastHit hit;
                Ray ray = new Ray(transform.position, Vector3.down);

                if (Physics.Raycast(ray, out hit, snapDownDistance) && !isClimbing)
                {
                    // Snap to the hit point
                    if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "DamageableEnemy" || LayerMask.LayerToName(hit.collider.gameObject.layer) == "Enemy")
                    {
                        // Define a LayerMask that includes the enemy layers to ignore
                        LayerMask ignoreLayers = LayerMask.GetMask("DamageableEnemy", "Enemy");
                        // Perform a second raycast ignoring the specified layers
                        RaycastHit secondHit;
                        if (Physics.Raycast(ray, out secondHit, snapDownDistance + 5.0f, ~ignoreLayers))
                        {
                            // Snap to the hit point of the second raycast
                            myTransform.position = new Vector3(myTransform.position.x, secondHit.point.y, myTransform.position.z);
                            inputHandler.isGrounded = true; // Update grounded status
                            airDashTriggered = 0;
                            isAirDashing = false;
                            inAirTimer = 0;
                            lastAirTimer = inAirTimer;
                            Debug.Log("Landed on enemy detected, snapping to ground");
                        }
                    }
                    else
                    {
                        myTransform.position = new Vector3(myTransform.position.x, hit.point.y, myTransform.position.z);
                        inputHandler.isGrounded = true; // Update grounded status
                        airDashTriggered = 0;
                        isAirDashing = false;
                        inAirTimer = 0;
                        lastAirTimer = inAirTimer;
                        Debug.Log("Fall detected, but snapped to ground as fall disance was too small");
                    }
                }
                else
                {
                    inAirTimer += Time.fixedDeltaTime; // was delta time
                    lastAirTimer = inAirTimer;
                }
            }
            else
            {
                if (!isSwimming && !inputHandler.isJumping)
                {
                    ClimbStep();
                }
                inAirTimer = 0;
            }

            if (isSprinting)
            {
                sprintTimer += Time.fixedDeltaTime; // was delta time
            }
            else
            {
                sprintTimer = 0.0f;
            }
        }

        public void SetGender(string _gender)
        {
            if (_gender == "mars")
            {
                gender = "mars";
                animatorHandler = animatorHandlerMars;
                animatorHandler.Initialize();
                Debug.Log("Gender has been set to Mars");
            }
            else if (_gender == "venus")
            {
                gender = "venus";
                animatorHandler = animatorHandlerVenus;
                animatorHandler.Initialize();
                Debug.Log("Gender has been set to Venus");
            }
            playerStats.gender = gender;
        }

        Vector3 normalVector;
        Vector3 targerPosition;

        private void HandleRotation(float delta)
        {
            if (inputHandler.lockedOn)
            {
                if (isSprinting) //add roll flag 
                {
                    Vector3 targetDirection = Vector3.zero;
                    targetDirection = cameraObject.transform.forward * inputHandler.vertical;
                    targetDirection += cameraObject.transform.right * inputHandler.horizontal;
                    targetDirection.y = 0;
                    targetDirection.Normalize();

                    if (targetDirection == Vector3.zero)
                    {
                        targetDirection = transform.forward;
                    }

                    Quaternion tr = Quaternion.LookRotation(targetDirection);
                    Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * delta);

                    //transform.rotation = targetRotation; // old
                    rigidbody.MoveRotation(targetRotation);
                }
                else
                {
                    //if (onMount) return;
                    if (cameraHandler.currentLockOnTarget == null) return;
                    Vector3 rotationDirection = moveDirection;
                    rotationDirection = cameraHandler.currentLockOnTarget.transform.position - transform.position;
                    rotationDirection.y = 0;
                    rotationDirection.Normalize();

                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, rotationSpeed * delta);

                    //transform.rotation = targetRotation; // old
                    rigidbody.MoveRotation(targetRotation);

                    // Update Animator Values if character is spinning while locked on to play leg animations
                    float deltaY = Mathf.DeltaAngle(previousEulerAngles.y, myTransform.rotation.eulerAngles.y);

                    if (Mathf.Abs(deltaY) < spinAnimThreshold)
                    {
                        lockedOnRotateDirection = 0;
                        animatorHandler.anim.SetInteger("lockedOnRotateDirection", lockedOnRotateDirection);
                    }
                    else
                    {
                        if (inputHandler.canRotate)
                        {
                            lockedOnRotateDirection = Mathf.RoundToInt(Mathf.Sign(deltaY) * -1);
                            animatorHandler.anim.SetInteger("lockedOnRotateDirection", lockedOnRotateDirection);
                            animatorHandler.anim.SetFloat("moveAmount", inputHandler.moveAmount);
                        }
                    }

                    // Always update this every frame (prevents stale angle spikes)
                    previousEulerAngles = myTransform.rotation.eulerAngles;
                }
            }
            else
            {
                Vector3 targetDir = Vector3.zero;
                float moveOverride = inputHandler.moveAmount;

                targetDir = cameraObject.forward * inputHandler.vertical;
                targetDir += cameraObject.right * inputHandler.horizontal;
                targetDir.y = 0;
                targetDir.Normalize();

                if (targetDir == Vector3.zero)
                {
                    targetDir = myTransform.forward;
                }

                float rs = rotationSpeed;

                Quaternion tr = Quaternion.LookRotation(targetDir);
                Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

                //myTransform.rotation = targetRotation; // old
                rigidbody.MoveRotation(targetRotation);
            }
        }

        public void AttemptToSlide()
        {
            if (!inputHandler.isGrounded) return;
            animatorHandler.PlayTargetActionAnimation("Slide", true, true);
            AttemptToToggleCrouch(true, false);
        }

        public void AttemptToToggleCrouch(bool forceCrouch=false, bool forceStand=false)
        {     
            // optional: check if grounded, for now doesn't seem necessary to me

            if (isCrouching || forceStand)
            {
                if (CanStand())
                {
                    isCrouching = false;
                    animatorHandler.anim.SetBool("isCrouching", false);
                    SetColliderHeight(false);
                }
                else
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Cannot stand here", true); //true prevents duplicate messages for 3 seconds
                }
            }
            else if (!isCrouching || forceCrouch)
            {
                isCrouching = true;
                animatorHandler.anim.SetBool("isCrouching", true);
                playerFeetCollider.height = primaryColliderCrouchedHeight;
                SetColliderHeight(true);
                isSprinting = false;
            }
        }

        public bool CanStand()
        {
            float radius = playerFeetCollider.radius;

            // 1. Get the real bottom of the collider in world space
            Vector3 bottom = playerFeetCollider.bounds.min + new Vector3(0, radius, 0);

            // 2. Compute the top of the STANDING capsule
            float standHeight = primaryColliderStandingHeight;
            Vector3 top = bottom + Vector3.up * (standHeight - radius * 2f);

            // 3. Only check the upper half
            Vector3 midpoint = (top + bottom) * 0.5f;

            Collider[] hits = Physics.OverlapCapsule(
                midpoint,
                top,
                radius,
                standingCheckLayers,
                QueryTriggerInteraction.Ignore
            );

            if (hits.Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetColliderHeight(bool crouching)
        {
            if (crouching)
            {
                playerFeetCollider.height = primaryColliderCrouchedHeight;
                playerSidesCollider.height = sidesColliderCrouchedHeight;
            }
            else
            {
                playerFeetCollider.height = primaryColliderStandingHeight;
                playerSidesCollider.height = sidesColliderStandingHeight;
            }

            playerFeetCollider.center = new Vector3(0f, playerFeetCollider.height / 2f, 0f);
            playerSidesCollider.center = playerFeetCollider.center;
        }

        public void AttemptToPerformDodge()
        {
            if (inputHandler.isPerformingAction || isSwimming)
            {
                //Override this by allowing dodge to cancel last parts of attack animations
                return;
            }

            if (playerStats.currentStamina <= 0)
            {
                //TextNotificationsManager.instance.NewTextNotifaction("Cannot dodge: out of Stamina!", true); //true prevents duplicate messages for 3 seconds
                return;
            }
            if(!inputHandler.isGrounded)
            {
                return;
            }

            if (inputHandler.moveAmount > 0) //roll
            {
                if (inputHandler.lockedOn)
                {
                    // Clamp Input values to fall on unit circle for clamped animations
                    float rollHorizontal = inputHandler.horizontal;
                    float rollVertical = inputHandler.vertical;

                    // Calculate the angle in radians
                    float angle = Mathf.Atan2(rollVertical, rollHorizontal);

                    // Calculate clamped values on the circumference
                    rollHorizontal = Mathf.Cos(angle);
                    rollVertical = Mathf.Sin(angle);

                    // Update animator parameters
                    animatorHandler.anim.SetFloat("rollHorizontal", rollHorizontal);
                    animatorHandler.anim.SetFloat("rollVertical", rollVertical);
                    animatorHandler.PlayTargetActionAnimation("RollBlendTree", true, !CheckRootMotionSphere(true));
                }
                else
                {
                    rollDirection = cameraHandler.cameraObject.transform.forward * inputHandler.vertical;
                    rollDirection += cameraHandler.cameraObject.transform.right * inputHandler.horizontal;
                    rollDirection.y = 0;
                    rollDirection.Normalize();

                    Quaternion playerRotation = Quaternion.LookRotation(rollDirection);
                    cameraHandler.playerTransform.rotation = playerRotation;

                    //HUGE FIX: OFFSET APPLIED TO ANIMATIONS ROOT TRANSFORM POSITION (Y) : True, Original, -0.05
                    if (inCombat)
                    {
                        animatorHandler.PlayTargetActionAnimation("Roll_Combat_F_0", true, !CheckRootMotionSphere(true));
                    }
                    else
                    {
                        animatorHandler.PlayTargetActionAnimation("Roll_F_0", true, !CheckRootMotionSphere(true));
                    }

                }
                playerStats.currentStamina -= playerStats.dodgeStaminaCost;

                SetColliderHeight(true);
            }
            else //backstep
            {
                animatorHandler.PlayTargetActionAnimation("Backstep01", true, true);
                playerStats.currentStamina -= playerStats.backstepStaminaCost;
            }

            isDodging = true;
            animatorHandler.anim.SetBool("isBlocking", false);      
        }

        public void AttemptToUseItem(Consumable selected)
        {
            if (inputHandler.isPerformingAction)
            {
                return;
            }
            if (inputHandler.isBlocking)
            {
                return;
            }
            if (inputHandler.isAttacking)
            {
                return;
            }
            if (!inputHandler.isGrounded)
            {
                return;
            }
            if(usingItem)
            {
                return;
            }

            if(ConsumablesHUDManager.instance.CanUseConsumable())
            {
                if (selected.useDefaultAnimation)
                {
                    usingItem = true;
                    animatorHandler.PlayTargetActionAnimation("UseItem", true, false, true, true);
                }
                else
                {
                    selected.UseConsumable();
                }
            }
        }

        public void HandleSprinting()
        {
            //PLAYER BUSY
            if(inputHandler.isPerformingAction)
            {
                isSprinting = false;
                return;
            }

            if (inputHandler.isAttacking || inputHandler.isBlocking || isDodging || (!inputHandler.isGrounded && !isSwimming))
            {
                isSprinting = false;
                return;
            }

            //INSUFFICIENT STAMINA
            if (playerStats.currentStamina <= 0)
            {
                isSprinting = false;
                return;
            }

            //HANDLE SPRINT
            Vector3 input = new Vector3(inputHandler.horizontal, 0f, inputHandler.vertical);
            //Debug.Log(input.magnitude);

            if (input.magnitude >= 0.75f && (inputHandler.isGrounded || isSwimming)) //if moving, set sprinting to true
            {
                sprintTimer += Time.fixedDeltaTime; // was delta time
            }
            else //not moving, sprinting false
            {
                isSprinting = false;
            }

            //COST STAMINA
            if(isSprinting)
            {
                if(inCombat)
                {
                    playerStats.currentStamina -= playerStats.sprintStaminaCost * Time.fixedDeltaTime;// was delta time
                }
                else
                {
                    playerStats.currentStamina -= playerStats.sheathedSprintStaminaCost * Time.fixedDeltaTime;// was delta time
                }

;           }
        }

        public void ToggleSprint()
        {
            //PLAYER BUSY
            if (inputHandler.isPerformingAction)
            {
                isSprinting = false;
                return;
            }

            if (inputHandler.isAttacking || inputHandler.isBlocking || isDodging)
            {
                isSprinting = false;
                return;
            }

            //INSUFFICIENT STAMINA
            if (playerStats.currentStamina <= 0)
            {
                isSprinting = false;
                return;
            }

            if (!inputHandler.isGrounded && !isSwimming)
            {
                isSprinting = false;
                return;
            }

            //HANDLE SPRINT
            if (inputHandler.moveAmount >= 0.5f && (inputHandler.isGrounded || isSwimming)) //if moving, set sprinting to true
            {
                isSprinting = true;
                AttemptToToggleCrouch(false, true); // force stop crouching
            }
            else //not moving or falling, sprinting false
            {
                isSprinting = false;
            }
        }

        public void AttemptToJump(bool overrideConditions=false)
        {
            if(!overrideConditions)
            {
                if (isSwimming || isClimbing)
                {
                    return;
                }

                if (playerStats.currentStamina <= 0)
                {
                    return;
                }

                if (!inputHandler.isGrounded)
                {
                    int airDashesAllowed = (int)playerStats.GetSkillEffectByID(8);
                    if (airDashTriggered < airDashesAllowed && !onMount)
                    {
                        airDashTriggered++; // increment air dash counter

                        // snap air dash direction
                        Vector3 dashDirection = cameraObject.forward * inputHandler.vertical;
                        dashDirection += cameraObject.right * inputHandler.horizontal;
                        dashDirection.y = 0;
                        dashDirection.Normalize();
                        transform.rotation = Quaternion.LookRotation(dashDirection);

                        animatorHandler.PlayTargetActionAnimation("Air Dash", true, false);
                        playerStats.currentStamina -= playerStats.jumpStaminaCost;
                    }
                    return;
                }

                if (inputHandler.isPerformingAction)
                {
                    return;
                }

                if (inputHandler.isJumping)
                {
                    return;
                }
            }

            isSprinting = false;
            inputHandler.isGrounded = false;
            inputHandler.isJumping = true;

            inputHandler.isAttacking = false;
            animatorHandler.anim.SetBool("isAttacking", false);
            StopBlocking();

            StartCoroutine(SnapDownDelayAfterJump()); // make sure snapping down for moments after jumping

            animatorHandler.PlayTargetActionAnimation("CombatJumpStart", true, false);

            if (!overrideConditions)
            {
                rigidbody.AddForce(transform.up * jumpForce);
                playerStats.currentStamina -= playerStats.jumpStaminaCost;
            }
        }

        private void HandleGroundCheck()
        {
            if (inputHandler.isJumping)
            {
                inputHandler.isGrounded = false;
            }
            else if (Physics.CheckSphere(groundCheckSphereLocation.position, groundedCheckSphereRadius, groundLayers))
            {
                inputHandler.isGrounded = true;
                airDashTriggered = 0;
                isAirDashing = false;
            }
            else 
            {
                inputHandler.isGrounded = false;
            }       
        }

        private void GetPlaneNormals()
        {
            float offset = 0.1f; // Increase height of raycast origin

            RaycastHit hit;
            Vector3 raycastOrigin = transform.position + Vector3.up * offset;

            if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, 2.0f, groundLayers)) // Last is raycast distance
            {
                normalVector = hit.normal;

                // Draw the ray for debugging
                //Debug.DrawRay(raycastOrigin, Vector3.down * hit.distance, Color.green);

                //// Check if the normal vector is not (0, 1, 0)
                //if (normalVector != Vector3.up)
                //{
                //    // Log the name of the object hit
                //    //Debug.Log("Hit object name: " + hit.collider.gameObject.name);
                //    //Debug.Log("Normal Plane: " + normalVector);
                //}
            }
            else
            {
                // Draw the ray for debugging (extend it to the maximum distance)
                //Debug.DrawRay(raycastOrigin, Vector3.down * 2.0f, Color.red);
            }
        }

        public void AttemptToBlock()
        {
            if (inputHandler.isPerformingAction || isSwimming)
            {
                return;
            }
            if (playerStats.currentStamina <= 0)
            {
                return;
            }
            if (playerStats.currentStamina <= 0)
            {
                return;
            }
            if(inputHandler.isBlocking)
            {
                return;
            }
            if(inputHandler.isAttacking)
            {
                return;
            }
            if (!inputHandler.isGrounded)
            {
                return;
            }
            if (playerInventory.equippedShield == playerInventory.emptyShield || !inCombat)
            {
                if (onMount) return;
                inputHandler.blockInput = false;
                attackCombo = 1;
                inputHandler.isAttacking = true;
                animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
                animatorHandler.anim.SetBool("isBlocking", false);
                playerStats.SpendAttackStamina(attackCombo, "UnarmedLight");
                animatorHandler.PlayTargetActionAnimation("UnarmedAttackLeft", true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false);
                return;
            }

            inputHandler.isBlocking = true;
            playerStats.SetStaminaRegenScale(playerStats.blockingStaminaRegen);
            blockingCollider.SetBlockingCollider(true);
            animatorHandler.anim.SetBool("isBlocking", inputHandler.isBlocking);
            if(!inputHandler.lockedOn) //lockedON blend tree already has upperbody blocking
            {
                animatorHandler.PlayTargetActionAnimation("StartBlock", false, false, true, true, 0.1f);
            }

            //Debug.Log("Start blocking");
        }

        public void StopBlocking()
        {
            inputHandler.isBlocking = false;
            playerStats.SetStaminaRegenScale(playerStats.defaultStaminaRegenAmount);
            blockingCollider.SetBlockingCollider(false);
            animatorHandler.anim.SetBool("isBlocking", inputHandler.isBlocking);
        }

        public void SetAnimBlockFalse()
        {
            animatorHandler.anim.SetBool("isBlocking", false);
        }

        public void HandleAttackQueue()
        {
            if (inputHandler.comboFlag)
            {
                // Reset Flag
                inputHandler.canQueueAttack = false;

                // Stop Blocking 
                animatorHandler.anim.SetBool("isBlocking", false);

                if (isSwimming || isClimbing) return;

                // Check Stamina
                if (playerStats.currentStamina <= 0)
                {
                    return;
                }

                // Update Combo Counter
                attackCombo++;

                // Initialize animationName for animator
                string animationName;

                // Aerial Attack
                if (!inputHandler.isGrounded)
                {
                    if (playerStats.GetSkillRankByID(6) > 0) // If Air Attacks are unlocked
                    {
                        animationName = "Combo_Attack_Air_0" + attackCombo.ToString();

                        if(attackCombo > playerStats.GetSkillRankByID(6) + 1)
                        {
                            //Debug.Log(attackCombo + " > " + (playerStats.skills[6].rank + 1) + " = true");
                            //Debug.Log("Max Air Combo reached, returning");
                            return;
                        }
                        else
                        {
                            animatorHandler.PlayTargetActionAnimation(animationName, true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove

                            playerStats.SpendAttackStamina(attackCombo);
                        }
                    }
                }
                // Ground Attack
                else
                {
                    // ADVANCED COMBO (FORWARD)
                    if (playerStats.GetSkillRankByID(10) > 0 && inputHandler.lockedOn && inputHandler.vertical >= 0.5f)
                    {
                        animationName = "ForwardAdvancedCombo" + attackCombo.ToString();
                        if(animationName == "ForwardAdvancedCombo3")
                        {
                            if(gender == "venus" || gender == "Venus")
                            {
                                animationName = "ForwardAdvancedCombo3F";
                            }
                            else 
                            {
                                animationName = "ForwardAdvancedCombo3M";
                            }
                        }
                    }
                    // ADVANCED COMBO (BACK)
                    else if (playerStats.GetSkillRankByID(10) > 0 && inputHandler.lockedOn && inputHandler.vertical <= -0.5f)
                    {
                        animationName = "BackAdvancedCombo" + attackCombo.ToString();
                    }
                    // STANDARD COMBO
                    else
                    {
                        animationName = "Attack" + attackCombo.ToString();
                    }

                    if (attackCombo > playerStats.GetSkillRankByID(0) + 1)
                    {
                        Debug.Log("Max Combo reached, returning");
                        return;
                    }
                    else
                    {
                        // Root Motion Note: if enemy is within 1.2f when attacking, root motion is disabed
                        animatorHandler.PlayTargetActionAnimation(animationName, true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false, 0.0f);
                        playerStats.SpendAttackStamina(attackCombo);
                    }                         
                }
            }
        }

        public void AttemptToAttack()
        {
            if (playerStats.currentStamina <= 0)
            {
                return;
            }

            if (inputHandler.isAttacking || isSwimming || isClimbing)
            {
                return;
            }

            if(onMount)
            {
                if (inputHandler.horizontal >= 0)
                {
                    animatorHandler.PlayTargetActionAnimation("MountedAttackRight", true, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                }
                else
                {
                    animatorHandler.PlayTargetActionAnimation("MountedAttackLeft", true, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                }

                inputHandler.isAttacking = true;
                animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
                playerStats.SpendAttackStamina(attackCombo);
                return;
            }

            if (!inputHandler.isGrounded)
            {
                if(playerStats.GetSkillRankByID(6) > 0 && canStartNewAirCombo)
                {
                    //Debug.Log("Light Air Attack Triggered");
                    animatorHandler.PlayTargetActionAnimation("Combo_Attack_Air_01", true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                    attackCombo = 1;
                    inputHandler.isAttacking = true;
                    animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
                    canStartNewAirCombo = false;
                    playerStats.SpendAttackStamina(attackCombo);
                }
                return;
            }

            if(sprintTimer >= 1.0f)
            {
                if(playerStats.GetSkillRankByID(3) > 0)
                {
                    Debug.Log("Swift Strike rank: " + playerStats.GetSkillRankByID(3));
                    animatorHandler.PlayTargetActionAnimation("LightSprintAttack", true, true, inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                    isSprinting = false;
                    playerStats.SpendAttackStamina(0, "LightSprintAttack");
                    return;
                }
            }

            if (inputHandler.isPerformingAction)
            {
                //Debug.Log("returning because performing action was true");
                return;
            }

            string animationName = "Attack1M";
            if (gender == "venus" || gender == "Venus") animationName = "Attack1F";

            // ADVANCED ATTACK (FORWARD)
            if (playerStats.GetSkillRankByID(10) > 0 && inputHandler.lockedOn && inputHandler.vertical >= 0.5f)
            {
                animationName = "ForwardAdvancedCombo1";
            }
            // ADVANCED ATTACK (BACK)
            else if (playerStats.GetSkillRankByID(10) > 0 && inputHandler.lockedOn && inputHandler.vertical <= -0.5f)
            {
                animationName = "BackAdvancedCombo1";
            }

            if (playerInventory.equippedWeapon == playerInventory.emptyWeapon || !inCombat)
            {
                animationName = "UnarmedAttackRight";
            }

            attackCombo = 1;
            inputHandler.isAttacking = true;
            animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
            animatorHandler.anim.SetBool("isBlocking", false);
            if (animationName == "UnarmedAttackRight")
            {
                playerStats.SpendAttackStamina(attackCombo, "UnarmedLight");
            }
            else
            {
                playerStats.SpendAttackStamina(attackCombo);
            }

            //Can rotate is set to TRUE if lockedOn, this corrects direction when attacking when locked on
            // Root Motion Note: if enemy is within 1.2f when attacking, root motion is disabed
            animatorHandler.PlayTargetActionAnimation(animationName, true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove
        }
        
        public void AttemptToCastSpell(int spellIndex)
        {

            if (playerInventory.equippedSpells[spellIndex] == null)
            {
                Debug.Log("No equipped spell");
                TextNotificationsManager.instance.NewTextNotifaction("No spell equipped", true); //true prevents duplicate messages for 3 seconds
                return;
            }

            currentSpellIndex = spellIndex; // Used by animevents to determine spell, example: CastSpellAOEStartingEffect()

            // Check action conditions for infusion spell
            if (playerInventory.equippedSpells[spellIndex].spell_ID == 10)
            {
                // Check if weapon is equipped before allowing spell cast
                if (playerInventory.equippedWeapon == playerInventory.emptyWeapon)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Cannot infuse, weapon required", true); //true prevents duplicate messages for 3 seconds
                    return;
                }

                // Check if weapon is drawn in order to infuse
                if (!inCombat)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Draw weapon to infuse", true); //true prevents duplicate messages for 3 seconds
                    return;
                }

                // Check if infusion spell is in progress to allow switching element (and if casted spell was the infusion spell)
                if (AnimEvents.instance.canSwitchInfusionType)
                {
                    AnimEvents.instance.SwitchInfusionElement();
                    return;
                }
            }

            if (inputHandler.isPerformingAction || isSwimming || isClimbing)
            {
                //Debug.Log("returning because performing action was true");
                return;
            }
            if (playerStats.currentMana < playerInventory.equippedSpells[spellIndex].manaCost) //change to spell cost
            {
                TextNotificationsManager.instance.NewTextNotifaction("Insufficient Mana", true); //true prevents duplicate messages for 3 seconds
                return;
            }
            if (inputHandler.isAttacking)
            {
                return;
            }

            inputHandler.isAttacking = true;
            animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
            animatorHandler.anim.SetBool("isBlocking", false);

            string castSpeed = playerInventory.equippedSpells[spellIndex].castSpeed;

            string animName;
            switch (playerInventory.equippedSpells[spellIndex].spellType)
            {
                case "Projectile":
                    //Can rotate is set to TRUE if lockedOn, this corrects direction when attacking when locked on
                    if (!onMount) animName = "CastingProj";
                    else animName = "CastingProjMounted";
                    animatorHandler.PlayTargetActionAnimation(animName, true, true, inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                    break;
                case "AOE":
                    // We use unique animations instead of adjusting the animator state speed with a multiplier because that can cause issues with animation events
                    animName = "CastingAOE_Fast";
                    if (playerInventory.equippedSpells[spellIndex].spell_ID == 10) // Infusion Spell
                    {        
                        AnimEvents.instance.infusionDuration = playerInventory.equippedSpells[spellIndex].duration;
                        animName = "Infuse Start";
                        animatorHandler.PlayTargetActionAnimation(animName, true, false, true, true); //target anim, isPerformingAction, root motion, canRotate, canMove
                        return;
                    }
                    else if (onMount)
                    {
                        animName = "CastingAOEMounted";
                    }
                    else if(castSpeed == "normal")
                    {
                        animName = "CastingAOE_Normal";
                    }
                    else if (castSpeed == "slow")
                    {
                        animName = "CastingAOE_Slow";
                    }
                    else if (castSpeed == "verySlow")
                    {
                        animName = "CastingAOE_VerySlow";
                    }
                    animatorHandler.PlayTargetActionAnimation(animName, true, true, inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                    break;
                case "Self":
                    //Can rotate is set to TRUE if lockedOn, this corrects direction when attacking when locked on
                    if (!onMount) animName = "CastingSelf";
                    else animName = "CastingSelfMounted";
                    animatorHandler.PlayTargetActionAnimation(animName, true, true, inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                    break;
                default:
                    break;
            }

        }

        public void PlayBlockedAnimation(bool success)
        {
            if (success)
            {
                animatorHandler.PlayTargetActionAnimation("BlockSuccessful", false, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
            }
            else
            {
                animatorHandler.PlayTargetActionAnimation("BlockFail", false, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                animatorHandler.anim.SetBool("isBlocking", false);
                inputHandler.blockInput = false;
                isDodging = false;
                animatorHandler.anim.SetBool("isAttacking", false);
                isHeavyAttacking = false;
            }
        }

        public void SetAnimatorLockedOnBool(bool status)
        {
            animatorHandler.anim.SetBool("isLockedOn", status);
            //Debug.Log("SET LOCKED ON ANIM: " + status);
        }

        public void PlayStaggerAnimation()
        {
            AnimEvents.instance.CancelAllAttacks();
            if(onMount)
            {
                if(mountManager.mountIsHealthy)
                {
                    animatorHandler.PlayTargetActionAnimation("DirectionalHitHorseback", true, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                }
            }
            else if(inputHandler.isGrounded)
            {

                animatorHandler.PlayTargetActionAnimation("DirectionalHit", true, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                // After this point in both this conditional and the else below, I added several flag resets that I believe fixed several issues.
                // However, for future references, these may be causing some issues.
                animatorHandler.anim.SetBool("isBlocking", false);
                inputHandler.blockInput = false;
                isDodging = false;
                isHeavyAttacking = false;
            }
            else
            {
                animatorHandler.PlayTargetActionAnimation("AerialDirectionalHit", true, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                animatorHandler.anim.SetBool("isBlocking", false);
                inputHandler.blockInput = false;
                isDodging = false;
                isHeavyAttacking = false;
            }


        }

        public void HandleHeavyAttack()
        {
            if (isSwimming || isClimbing) return;

            //INSUFFICIENT STAMINA
            if (playerStats.currentStamina <= 0)
            {
                isHeavyAttacking = false;
                inputHandler.isAttacking = false;
                return;
            }

            if (onMount)
            {
                if (inputHandler.horizontal >= 0)
                {
                    animatorHandler.PlayTargetActionAnimation("MountedAttackRight", true, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                }
                else
                {
                    animatorHandler.PlayTargetActionAnimation("MountedAttackLeft", true, false, false, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                }

                inputHandler.isAttacking = true;
                animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
                playerStats.SpendAttackStamina(attackCombo);
                return;
            }

            if (!inputHandler.isGrounded) // air attack
            {
                if (playerStats.GetSkillRankByID(6) > 0) //sufficient skill point
                {
                    Debug.Log("Plunge Attack Triggered");
                    inputHandler.isAttacking = true; //prevent other aerial attacks, gets reset by AnimEvents AirAttackEnd
                    animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
                    animatorHandler.anim.SetBool("airAttack", true);
                    playerStats.SpendAttackStamina(0, "AirAttack");
                    inputHandler.canRotate = inputHandler.lockedOn;
                    inputHandler.heavyAttackInput = false;
                    if(chargeAttackDelay != null)
                    {
                        StopCoroutine(chargeAttackDelay);
                    }
                    chargeAttackDelay = StartCoroutine(DelayHeavyAttackAfterPlungeAttack());
                }
                return;
            }

            if (sprintTimer >= 1.0f)
            {
                //Debug.Log(playerStats.skills[3].rank);
                if (playerStats.GetSkillRankByID(3) == 2)
                {
                    //Debug.Log("Heavy, rank: " + playerStats.skills[3].rank);
                    animatorHandler.PlayTargetActionAnimation("HeavySprintAttack", true, true, inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                    isSprinting = false;
                    playerStats.SpendAttackStamina(0, "HeavySprintAttack");
                    inputHandler.heavyAttackInput = false;
                    return;
                }
                else if (playerStats.GetSkillRankByID(3) == 1)
                {
                    //Debug.Log("Heavy->Light, rank: " + playerStats.skills[3].rank);
                    animatorHandler.PlayTargetActionAnimation("LightSprintAttack", true, true, inputHandler.lockedOn, false); //target anim, isPerformingAction, root motion, canRotate, canMove
                    isSprinting = false;
                    playerStats.SpendAttackStamina(0, "LightSprintAttack");
                    inputHandler.heavyAttackInput = false;
                    return;
                }
            }

            //PLAYER BUSY
            if (inputHandler.isPerformingAction)
            {
                isHeavyAttacking = false;
            }

            if (inputHandler.isAttacking || isDodging)
            {
                isHeavyAttacking = false;
                return;
            }

            if (isHeavyAttacking)
            {
                Debug.Log("Previous heavy attack still in progress, returned.");
                return;
            }

            //HANDLE ATTACK
            if(determineHeavyAttackInput == null)
            {
                if(inputHandler.isAttacking)
                {
                    if(inputHandler.canQueueAttack)
                    {
                        determineHeavyAttackInput = StartCoroutine(DetermineHeavyInputAction());
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    determineHeavyAttackInput = StartCoroutine(DetermineHeavyInputAction());
                }

            }
        }

        private IEnumerator DetermineHeavyInputAction()
        {
            if (canHeavyAttack)
            {
                float holdCheckTimer = 0.0f;
                float chargeAttackHoldThreshold = 0.25f;

                if (inputHandler.isAttacking) yield break;

                if(playerInventory.equippedWeapon == playerInventory.emptyWeapon || !inCombat)
                {
                    Debug.Log("Player triggered heavy unarmed attack");
                    // Stop Blocking 
                    animatorHandler.anim.SetBool("isBlocking", false);

                    attackCombo = 1;
                    inputHandler.isAttacking = true;
                    animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
                    animatorHandler.anim.SetBool("isBlocking", false);

                    playerStats.SpendAttackStamina(attackCombo, "UnarmedHeavy");

                    isHeavyAttacking = true;

                    if (useUnarmedHeavyA)
                    {
                        // Can rotate is set to TRUE if lockedOn, this corrects direction when attacking when locked on
                        animatorHandler.PlayTargetActionAnimation("UnarmedHeavyAttack", true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false); // target anim, isPerformingAction, root motion, canRotate, canMove
                    }
                    else
                    {
                        animatorHandler.PlayTargetActionAnimation("UnarmedHeavyAttackB", true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false); // target anim, isPerformingAction, root motion, canRotate, canMove
                    }
                    useUnarmedHeavyA = !useUnarmedHeavyA; // Toggle for next time

                    determineHeavyAttackInput = null;
                    yield break; // Exit the coroutine
                }

                // Check for continuous button holding
                while (inputHandler.heavyAttackInput && playerStats.GetSkillRankByID(1) >= 1)
                {
                    holdCheckTimer += Time.fixedDeltaTime; // was delta time
                    if (holdCheckTimer >= chargeAttackHoldThreshold)
                    {
                        Debug.Log("Player triggered charge attack");
                        StartCoroutine(StartChargingHeavyAttack());
                        determineHeavyAttackInput = null;
                        yield break; // Exit the coroutine
                    }
                    yield return null; // Wait for the next frame
                }

                // Check if the button was held for the required duration
                if (holdCheckTimer >= chargeAttackHoldThreshold)
                {
                    Debug.Log("Player triggered charge attack");
                    StartCoroutine(StartChargingHeavyAttack());
                }
                else
                {
                    Debug.Log("Player triggered heavy attack");
                    // Stop Blocking 
                    animatorHandler.anim.SetBool("isBlocking", false);

                    attackCombo = 1;
                    inputHandler.isAttacking = true;
                    animatorHandler.anim.SetBool("isAttacking", inputHandler.isAttacking);
                    animatorHandler.anim.SetBool("isBlocking", false);

                    playerStats.SpendAttackStamina(attackCombo, "HeavyAttack");

                    // Can rotate is set to TRUE if lockedOn, this corrects direction when attacking when locked on
                    animatorHandler.PlayTargetActionAnimation("HeavyAttack1", true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false); // target anim, isPerformingAction, root motion, canRotate, canMove
                }
            }

            determineHeavyAttackInput = null;
            yield break;
        }

        private IEnumerator StartChargingHeavyAttack()
        {
            // MAY NEED TO IMPEMENT A WAY TO INTERRUPTE THIS COROUTINE IF PLAYER IS STAGGERED!
            if (chargeAttackDelay != null)
            {
                StopCoroutine(chargeAttackDelay);
            }
            canHeavyAttack = false;
            animatorHandler.anim.SetBool("isCharging", true);
            //Debug.Log("Charged attack started");
            float maximumChargingTime = playerStats.GetSkillRankByID(1); // rank 1 = 1 second, and so on.
            heavyChargeTimer = 0.0f;
            /// REMEMBER to reset isHeavyAttacking in this script and animator if staggered/interrupted
            isHeavyAttacking = true;
            //animatorHandler.anim.SetBool("isCharging", isHeavyAttacking);
            animatorHandler.PlayTargetActionAnimation("Charge1", true, true, inputHandler.lockedOn);
            while (isHeavyAttacking)
            {
                heavyChargeTimer += Time.fixedDeltaTime;// was delta time

                // Check for a condition that allows you to exit the loop, e.g., a time limit
                if (heavyChargeTimer >= maximumChargingTime)
                {
                    break; // Exit the loop when the condition is met
                }

                playerStats.currentStamina -= playerStats.chargeAttackStaminaCostPerSecond * Time.fixedDeltaTime;// was delta time

                if (playerStats.currentStamina <= 0)
                {
                    break;
                }

                yield return null;
            }
            inputHandler.heavyAttackInput = false;
            attackCombo = 1;
            Debug.Log("Charged attack for: " + heavyChargeTimer.ToString("N2") + " seconds");
            //isHeavyAttacking = false; //may need to set this in Charge2 animiation to prevent player from canceling attack with anothe charge (keep holding)
            animatorHandler.anim.SetBool("isCharging", false);
            animatorHandler.PlayTargetActionAnimation("Charge2", true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), false, false, 0.0f);
            yield return new WaitForSeconds(1.0f); // Delay before allowing the next heavy attack
            canHeavyAttack = true;
        }

        public void HandleHeavyAttackQueue()
        {
            if (inputHandler.comboFlag)
            {
                // Reset Flag
                inputHandler.canQueueAttack = false;

                if(!inputHandler.isGrounded || isSwimming || isClimbing)
                {
                    return;
                }

                // Stop Blocking 
                animatorHandler.anim.SetBool("isBlocking", false);

                // Check Stamina
                if (playerStats.currentStamina <= 0)
                {
                    return;
                }

                // Update Combo Counter             
                attackCombo++;

                // Initialize animationName for animator
                string animationName = "HeavyAttack" + attackCombo.ToString();

                if (attackCombo > playerStats.GetSkillRankByID(1) + 1)
                {
                    Debug.Log("Max Combo reached, returning");
                    Debug.Log(attackCombo + " > " + (playerStats.GetSkillRankByID(1) + 1));
                    return;
                }
                else
                {
                    animatorHandler.PlayTargetActionAnimation(animationName, true, RootMotionBasedOnTargetDistance(maxRootMotionDistance), inputHandler.lockedOn, false, 0.2f);
                    playerStats.SpendAttackStamina(attackCombo, "HeavyAttack");
                }
            }
        }

        private IEnumerator DelayHeavyAttackAfterPlungeAttack()
        {
            canHeavyAttack = false;
            yield return new WaitForSeconds(1.5f); // Delay before allowing the next heavy attack, adjust as needed
            canHeavyAttack = true;
        }

        public void ImpulseOnAirAttack(float scale = 1.0f)
        {
            rigidbody.velocity = Vector3.zero;

            rigidbody.AddForce(Vector3.up * airAttackUpwardForce * scale, ForceMode.Impulse);
        }

        public void ImpulseOnAirDash(float scale = 1.0f)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.AddForce(myTransform.forward * airDashForwardForce * scale, ForceMode.Impulse);
        }

        public bool RootMotionBasedOnTargetDistance(float rootMotionAttackDistance)
        {
            if (CheckRootMotionSphere())
            {
                return false;
            }

            if(!inputHandler.lockedOn)
            {
                // Always use root motion if not locked on
                return true;
            }
            else
            {
                // distance serves as the threshold to dictate whether we allow root motion or not
                float enemyDistance = cameraHandler.GetDistanceToExteriorOfCurrentTarget();

                Debug.Log("Enemy Distance: " + enemyDistance + ", within distance(" + rootMotionAttackDistance + ") = " + (enemyDistance > rootMotionAttackDistance));

                // root motion IF enemy is far enough
                return enemyDistance > rootMotionAttackDistance;
            }
        }

        public void SheatheUnsheatheWeapon(bool isPerformingAction=true, bool canMove=true, bool canRotate=true)
        {
            if (!mountingComplete) return; // mounting in progress
            if (!dismountingComplete) return; // dismount in progress

            if (onMount || isCrouching)
            {
                if (inCombat)
                {
                    animatorHandler.PlayTargetActionAnimation("CombatToIdle_UpperBody", isPerformingAction, false, true, true, 0.3f);
                    if (onMount) StartCoroutine(DelayGrabReinsAfterSheathe());
                }
                else
                {
                    animatorHandler.PlayTargetActionAnimation("IdleToCombat_UpperBody", isPerformingAction, false, true, true, 0.3f);
                    mountStats.DetachReins();
                }
            }
            else
            {
                if (inCombat)
                {
                    if(inputHandler.moveAmount < 0.1f) animatorHandler.PlayTargetActionAnimation("CombatToIdleLegs", isPerformingAction, true, canRotate, canMove, 0.3f);
                    animatorHandler.PlayTargetActionAnimation("CombatToIdle_UpperBody", isPerformingAction, false, canRotate, canMove, 0.3f);
                }
                else
                {
                    if (inputHandler.moveAmount < 0.1f) animatorHandler.PlayTargetActionAnimation("IdleToCombatLegs", isPerformingAction, true, canRotate, canMove, 0.3f);
                    animatorHandler.PlayTargetActionAnimation("IdleToCombat_UpperBody", isPerformingAction, false, canRotate, canMove, 0.3f);
                }
            }
        }

        public void InstantSheatheUnsheathe(bool sheathe = true)
        {
            if (sheathe)
            {
                Debug.Log("InstantSheathe called");
                AnimatorHandler.instance.MoveSword("back");
                AnimatorHandler.instance.MoveShield("back");
                inCombat = false;
            }
            else
            {
                Debug.Log("Instant unSheathe called");
                AnimatorHandler.instance.MoveSword("hands");
                AnimatorHandler.instance.MoveShield("hands");
                inCombat = true;
            }
        }

        public void PlayTestAnimation()
        {

        }

        public bool CheckRootMotionSphere(bool dodge = false)
        {
            Vector3 checkPosition = rootMotionSphereCheckLocation.position;
            LayerMask checkLayers = dodge ? rootMotionSphereCheckLayersDodge : rootMotionSphereCheckLayers;

            if (inputHandler.lockedOn && !isSprinting)
            {
                Vector3 pointDirection = cameraObject.forward * inputHandler.vertical;
                pointDirection += cameraObject.right * inputHandler.horizontal;
                pointDirection.y = 0;
                pointDirection.Normalize();

                float distanceFromPlayer = rootMotionSphereCheckLocation.localPosition.z;
                float height = rootMotionSphereCheckLocation.localPosition.y;

                Vector3 newPoint = myTransform.position + pointDirection * distanceFromPlayer + Vector3.up * height;
                Debug.DrawRay(newPoint, Vector3.up * 0.1f, Color.green);

                checkPosition = newPoint;
            }

            if (Physics.CheckSphere(checkPosition, rootMotionSphereCheckRadius, checkLayers))
            {
                //Debug.Log("Object detected, root motion disabled");
                return true; // object detected
            }
            else
            {
                //Debug.Log("No object detected, root motion enabled");
                return false; // no object detected
            }
        }
        
        // This function serves to prevent any situtation where the isAttacking flag may not be reset on attack end
        // Since the attack flag is typically reset by animation events, some rare cases may cause the flag to get stuck
        // Currently this is not necessary as I have not seen this happen but I am including these functions so that 
        // If i ever encounter this, this solution may provide useful.
        private IEnumerator AttackResetFailsafe()
        {
            yield return new WaitForSeconds(5.0f);

            inputHandler.isAttacking = false;

        }
        // This function would be called whenever a new attack is triggered
        private void StartAttackFlagFailsafe()
        {
            if (attackFlagFailsafe != null)
            {
                StopCoroutine(attackFlagFailsafe);
            }
            attackFlagFailsafe = StartCoroutine(AttackResetFailsafe());
        }

        // ADDED FOR HORSE

        public void AttemptToMountHorse(string direction)
        {
            if (!dismountingComplete) return; // curently dismounting

            AttemptToToggleCrouch(false, true);

            mountingComplete = false;
            cancelMount = false;
            AnimatorHandler.instance.anim.SetBool("cancelMount", false);

            //Disable prompt
            mountPrompt.SetActive(false);
            mountTriggers.SetActive(false);

            Debug.Log("Mount horse from " + direction);

            onMount = true;
            rigidbody.isKinematic = true;

            playerSidesCollider.enabled = false;
            playerFeetCollider.enabled = false;

            animatorHandler.anim.SetBool("onMount", true);

            SwapHorseVersions(true);

            myTransform.SetParent(playerHolder);
            myTransform.localPosition = Vector3.zero;
            myTransform.localRotation = Quaternion.identity;

            // Play First Frames to set position
            if (direction == "left")
            {
                animatorHandler.PlayTargetActionAnimation("MountLeft", true, false, false, false, 0.0f);

            }
            else if (direction == "right")
            {
                animatorHandler.PlayTargetActionAnimation("MountRight", true, false, false, false, 0.0f);

            }

            // Set Camera Transition Target
            cameraHandler.StartCameraOverride(playerHolder, cameraOverrideDuration);

            waypointManager.HorseIconOff();

            whistleIconManager.SetIconToWhistle();

            horseAIAttackHitBox.enabled = false;

            minimapPlayerIcon.SetActive(false);

            InteractPrompt.instance.ClearInteractionList();

            leftMountTrigger.alreadyTriggered = false;
            rightMountTrigger.alreadyTriggered = false;
        }

        public void Dismount(bool forced=false)
        {
            if(mountingComplete || forced)
            {
                AttemptToToggleCrouch(false, true);
                string animationName = "DismountLeft";
                dismountingComplete = false;
                cameraHandler.StopOverrideCoroutine();
                if (!forced)
                {
                    /* Determine Dismount Direction */
                    // First, find what side of horse the camera is facing:
                    int dismountDirection = 0;
                    dismountDirection = DetermineDismountDirection();

                    // Next, check for collisions on both sides of horse
                    int leftCollisions = CheckDismountCollisions(0);
                    int rightCollisions = CheckDismountCollisions(1);

                    #region Debug
                    //if (dismountDirection == 0)
                    //{
                    //    Debug.Log("Camera is on the left side of the object.");
                    //}
                    //else
                    //{
                    //    Debug.Log("Camera is on the right side of the object.");
                    //}

                    //Debug.Log("Left Collisions:" + leftCollisions + ", Right Collisions:" + rightCollisions);
                    #endregion

                    // Objects detected on left
                    if(leftCollisions > rightCollisions)
                    {
                        animationName = "DismountRight";
                        inputHandler.canMountLeft = false;
                        inputHandler.canMountRight = true;
                    }
                    // Objects detected on right
                    else if(rightCollisions > leftCollisions)
                    {
                        animationName = "DismountLeft";
                        inputHandler.canMountRight = false;
                        inputHandler.canMountLeft = true;
                    }
                    // Zero or equal objects detected, use camera position
                    else if(rightCollisions == leftCollisions)
                    {
                        if(dismountDirection == 0)
                        {
                            animationName = "DismountLeft";
                            inputHandler.canMountRight = false;
                            inputHandler.canMountLeft = true;
                        }
                        else
                        {
                            animationName = "DismountRight";
                            inputHandler.canMountLeft = false;
                            inputHandler.canMountRight = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("Unexpected behavior with Dismount Direction, should never be reached.");
                    }

                    animatorHandler.PlayTargetActionAnimation(animationName, true, true, false, false, 0.0f);
                }
                else
                {
                    StartCoroutine(ForcedDismount());
                }

                //mountLocomotion.enabled = false;
                mountStats.DetachReins();

                minimapPlayerIcon.SetActive(true);
            }
        }

        int DetermineDismountDirection()
        {
            // Get the direction from the object to the camera
            Vector3 toCamera = cameraHandler.cameraObject.transform.position - horseMountVersion.transform.position;

            // Get the object's forward direction
            Vector3 objectForward = horseMountVersion.transform.forward;

            // Project the toCamera vector onto the object's right vector
            float dotProduct = Vector3.Dot(toCamera, horseMountVersion.transform.right);

            // Determine if the camera is on the left or right side of the object
            if (dotProduct > 0)
            {
                // Camera is on the right side
                return 1;
                Debug.Log("Camera is on the right side of the object.");
            }
            else if (dotProduct < 0)
            {
                return 0;
                // Camera is on the left side
                Debug.Log("Camera is on the left side of the object.");
            }
            else
            {
                return 0;
                // Camera is directly behind or in front of the object
                Debug.Log("Camera is directly behind or in front of the object.");
            }

        }

        int CheckDismountCollisions(int direction)
        {
            int numberOfObjectsTouching = 0;
            Collider collider;

            if (direction == 0)
            {
                collider = dismountLeftCollider;
            }
            else
            {
                collider = dismountRightCollider;
            }

            // Check if there are objects touching the collider
            if (Physics.CheckSphere(collider.transform.position, collider.bounds.size.x / 2f, layerMask))
            {
                // Get all colliders touching the dismountLeftCollider
                Collider[] colliders = Physics.OverlapBox(collider.bounds.center, collider.bounds.extents, Quaternion.identity, layerMask);

                // Tally the number of objects
                numberOfObjectsTouching = colliders.Length;

                // Process the colliders as needed
                foreach (Collider col in colliders)
                {
                    Debug.Log("Object touching: " + col.gameObject.name);
                }
            }
            else
            {
                // No objects touching, reset the tally
                numberOfObjectsTouching = 0;
            }

            return numberOfObjectsTouching;
        }

        public void FinishDismount()
        {
            cameraHandler.StopOverrideCoroutine();
            onMount = false;
            rigidbody.isKinematic = false;
            playerSidesCollider.enabled = true;
            playerFeetCollider.enabled = true;
            animatorHandler.anim.SetBool("onMount", false);
            myTransform.parent = ScenePersistentPlayerObject.instance.objectContainer.transform;
            mountTriggers.SetActive(true);
            myTransform.localScale = Vector3.one;
            StartCoroutine(CanMountDelay());
            minimapPlayerIcon.SetActive(true);
            //swap to Horse AI
            SwapHorseVersions(false);
            inputHandler.canRotate = true;
            inputHandler.canMove = true;
        }

        public void PlacePlayerOnMount()
        {
            if (cancelMount)
            {
                AnimatorHandler.instance.anim.SetBool("cancelMount", false);
                cancelMount = false;
                return;
            }

            AnimatorHandler.instance.anim.applyRootMotion = false; //unused atm

            mountManager.SetSaddlePosition();
            // Attach Player to Saddle
            myTransform.SetParent(playerHolder);

            myTransform.localPosition = Vector3.zero;
            myTransform.localRotation = Quaternion.identity;

            mountingComplete = true;

            mountStats.AttachReins(inCombat);

            inputHandler.isPerformingAction = false;        
        }

        public void CallHorse(bool forceFollow=false, bool dismissHorse=false)
        {
            // Be sure to play whistle sound here, or stay sound maybe if commanding to wait
            if (onMount) return;
            //waypointManager.StartHorseIconCoroutine();
            if (dismissHorse)
            {
                mountManager.DismissMount();
            }
            else if(!forceFollow)
            {
                mountManager.DetermineCallAction();
            }
            else
            {
                mountManager.ForceFollow();
            }
        }

        private IEnumerator CanMountDelay()
        {
            yield return new WaitForSeconds(0.6f);
            dismountingComplete = true;
        }

        private IEnumerator DelayGrabReinsAfterSheathe()
        {
            yield return new WaitForSeconds(1.5f);
            if(onMount && dismountingComplete && mountingComplete)
            {
                mountStats.AttachReins(inCombat);
            }
        }

        public void SwapHorseVersions(bool toMountVersion)
        {
            // If no horse is present, no need to swap. This can occur when forcing dismount on fast travel.
            if (!horseMountVersion.activeSelf && !horseAIVersion.activeSelf) return;

            mountAnimEvents.ResumeFootsteps();

            if (toMountVersion)
            {
                horseMountVersion.transform.position = horseAIVersion.transform.position;
                horseMountVersion.transform.rotation = horseAIVersion.transform.rotation;

                horseMountVersion.SetActive(true);
                horseAIVersion.SetActive(false);
            }
            else
            {
                horseAIVersion.transform.position = horseMountVersion.transform.position;
                horseAIVersion.transform.rotation = horseMountVersion.transform.rotation;
                horseAIVersion.SetActive(true);
                horseMountVersion.SetActive(false);

                whistleIconManager.DetermineAndSetIcon(true, companionBehaviour.follow);
            }
        }

        private IEnumerator ForcedDismount()
        {
            myTransform.parent = ScenePersistentPlayerObject.instance.objectContainer.transform;
            rigidbody.isKinematic = false;
            inputHandler.canMountLeft = false;
            inputHandler.canMountRight = false;
            AttemptToJump(true);

            yield return new WaitForSeconds(0.5f);

            onMount = false;
            rigidbody.isKinematic = false;
            playerSidesCollider.enabled = true;
            playerFeetCollider.enabled = true;
            animatorHandler.anim.SetBool("onMount", false);
            mountTriggers.SetActive(true);

            StartCoroutine(CanMountDelay());

            minimapPlayerIcon.SetActive(true);
        }

        public void StartLooting(bool chestVersion=false)
        {
            if (chestVersion) StartCoroutine(OpenChest(0.5f));

            if (!onMount)
            {
                animatorHandler.PlayTargetActionAnimation("LootStart", true, true, false, false);
            }
            else
            {
                animatorHandler.PlayTargetActionAnimation("MountedLootStart", true, true, false, false);
            }
        }

        private IEnumerator OpenChest(float delay)
        {
            Chest chest = InteractPrompt.instance.currentChest;
            yield return new WaitForSeconds(delay);
            chest.GetComponent<Chest>().Open(InteractPrompt.instance.keyCount);
        }

        public void StandUp(float delay = 0)
        {
            StartCoroutine(StandUpDelay(delay));
        }

        private IEnumerator StandUpDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            animatorHandler.PlayTargetActionAnimation("StandUp", true, true);
        }

        // SWIMMING:
        public void SetSwimming(bool _isSwimming, GameObject _waterPlane)
        {
            if (isClimbing) return;

            waterPlane = _waterPlane;
            isSwimming = _isSwimming;

            if (!isSwimming)
            {
                StopRipples();
            }
            else
            {
                if (!inputHandler.isGrounded)
                {
                    InstantiateBigSplash();
                }
                else
                {
                    InstantiateSmallSplash();
                }
            }

            foreach(CapsuleCollider cc in damageColliders)
            {
                cc.isTrigger = isSwimming;
            }
   
            animatorHandler.anim.SetBool("isSwimming", isSwimming);

            playerAudioManager.PlaySwimmingAudio(isSwimming);
        }

        public void InstantiateBigSplash()
        {
            // These prefabs have a script to delete themselves already
            var splash = Instantiate(bigSplashPrefab, new Vector3(myTransform.position.x, waterPlane.transform.position.y, myTransform.position.z), Quaternion.identity);
            splash.transform.parent = waterPlane.transform;
        }

        public void InstantiateSmallSplash()
        {
            if (!canSmallSplash) return;
            StartCoroutine(SmallSpawnCooldown());
            // These prefabs have a script to delete themselves already
            Vector3 pos = myTransform.position + myTransform.forward * 1.0f;
            //pos = new Vector3(pos.x, waterPlane.transform.position.y, mountStartPos_left.y);
            pos.y = waterPlane.transform.position.y;

            var splash = Instantiate(smallSplashPrefab, pos, Quaternion.identity);
            splash.transform.parent = waterPlane.transform;
        }

        private IEnumerator SmallSpawnCooldown()
        {
            canSmallSplash = false;
            yield return new WaitForSeconds(3.0f);
            canSmallSplash = true;
        }

        public void InstantiateRipple()
        {
            // These prefabs have a script to delete themselves already
            if(waterPlane == null)
            {
                StopRipples();
                return;
            }
            var ripple = Instantiate(ripplePrefab, new Vector3(myTransform.position.x, waterPlane.transform.position.y, myTransform.position.z), Quaternion.Euler(90f,0,0));
            ripple.transform.parent = waterPlane.transform;
        }

        public void StartRipples()
        {
            //ripples.Play();
            if (rippleSpawner == null)
            {
                rippleSpawner = StartCoroutine(RippleSpawner());
                playerAudioManager.SetSwimmingClip(false);
            }
        }

        public void StopRipples()
        {
            if (rippleSpawner != null)
            {
                StopCoroutine(rippleSpawner);
                rippleSpawner = null;
                playerAudioManager.SetSwimmingClip(true);
            }
        }

        private IEnumerator RippleSpawner()
        {
            InstantiateRipple();

            // Infinite loop for continuous ripple spawning
            while (true)
            {
                // Randomize the interval between spawns
                float interval = Random.Range(minRippleInterval, maxRippleInterval);

                // Wait for the randomized interval
                yield return new WaitForSeconds(interval);


                float rand = Random.Range(0f, 1.0f);
                // Call SpawnRipple to spawn a ripple
                if (rand <= rippleChance) InstantiateRipple();
            }
        }

        // STEPS / STAIRs:

        void ClimbStep()
        {
            // Set our step check diection to correlate to player input, so that steps are climbed even if not directly walking into them
            // For example, the player has the camera locked onto an enemy but is walking backwards into steps
            Vector3 stepCheckDirection = stepDirection;
            // If the player isn't moving (has no move direction), we will use the player's forward direction by default
            if(stepCheckDirection.magnitude == 0)
            {
                stepCheckDirection = transform.TransformDirection(Vector3.forward);
                return;
            }

            // Set up varirables
            RaycastHit hitLower; // Raycast in the forward direction of player input/movement
            RaycastHit hitLower45; // Raycast at a 45 degree angle of direction of player input/movement
            RaycastHit hitLowerMinus45; // Raycast at a -45 degree angle of direction of player input/movement

            // Set up step check directions for45 degree angle ray casts
            Vector3 stepCheckDirection45 = Quaternion.Euler(0f, 45f, 0f) * stepCheckDirection;
            Vector3 stepCheckDirectionMinus45 = Quaternion.Euler(0f, -45f, 0f) * stepCheckDirection;

            // Raycast in the forward direction of player input/movement
            if (Physics.Raycast(stepRayLower.transform.position, stepCheckDirection, out hitLower, lowerStepRaycastLength, stepLayers) //.05
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset), stepCheckDirection, out hitLower, lowerStepRaycastLength, stepLayers) //.1
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset * 2), stepCheckDirection, out hitLower, lowerStepRaycastLength, stepLayers) //.15
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset * 3), stepCheckDirection, out hitLower, lowerStepRaycastLength, stepLayers) //.20
                )
            {
                //Debug.Log("Step Triggered A1");
                //Debug.Log(hitLower.collider.gameObject);
                RaycastHit hitUpper;
                if (!Physics.Raycast(stepRayUpper.transform.position, stepCheckDirection, out hitUpper, upperStepRaycastLength, stepLayers))
                {
                    //Debug.Log("Step Taken A3");
                    rigidbody.position -= new Vector3(0f, -stepSmooth * Time.fixedDeltaTime, 0f); // was delta time
                    return;
                }
            }
            // Raycast at a 45 degree angle of direction of player input/movement       
            else if (Physics.Raycast(stepRayLower.transform.position, stepCheckDirection45, out hitLower45, lowerStepRaycastLength45, stepLayers) // .05
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset), stepCheckDirection45, out hitLower45, lowerStepRaycastLength45, stepLayers) //.1
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset * 2), stepCheckDirection45, out hitLower45, lowerStepRaycastLength45, stepLayers) //.15
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset * 3), stepCheckDirection45, out hitLower45, lowerStepRaycastLength45, stepLayers) //.20
                )
            {
                //Debug.Log("Step Triggered B");
                //Debug.Log(hitLower45.collider.gameObject);
                RaycastHit hitUpper45;
                if (!Physics.Raycast(stepRayUpper.transform.position, stepCheckDirection45, out hitUpper45, upperStepRaycastLength45, stepLayers))
                {
                   // Debug.Log("Step Taken B");
                    rigidbody.position -= new Vector3(0f, -stepSmooth * Time.fixedDeltaTime, 0f); // was delta time
                    return;
                }
            }
            // Raycast at a -45 degree angle of direction of player input/movement     
            else if (Physics.Raycast(stepRayLower.transform.position, stepCheckDirectionMinus45, out hitLowerMinus45, lowerStepRaycastLength45, stepLayers) // .05
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset), stepCheckDirectionMinus45, out hitLowerMinus45, lowerStepRaycastLength45, stepLayers) //.1
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset * 2), stepCheckDirectionMinus45, out hitLowerMinus45, lowerStepRaycastLength45, stepLayers) //.15
                || Physics.Raycast(stepRayLower.transform.position + (raycastHeightOffset * 3), stepCheckDirectionMinus45, out hitLowerMinus45, lowerStepRaycastLength45, stepLayers) //.20
                )
            {
                //Debug.Log("Step Triggered C");
                //Debug.Log(hitLowerMinus45.collider.gameObject);
                RaycastHit hitUpperMinus45;
                if (!Physics.Raycast(stepRayUpper.transform.position, stepCheckDirectionMinus45, out hitUpperMinus45, upperStepRaycastLength45, stepLayers))
                {
                    //Debug.Log("Step Taken C");
                    rigidbody.position -= new Vector3(0f, -stepSmooth * Time.fixedDeltaTime, 0f);  // was delta time
                    return;
                }
            }

            #region Debug Raycasts

            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset), stepCheckDirection * lowerStepRaycastLength, Color.red);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 2), stepCheckDirection * lowerStepRaycastLength, Color.red);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 3), stepCheckDirection * lowerStepRaycastLength, Color.red);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 4), stepCheckDirection * lowerStepRaycastLength, Color.red);

            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset), stepCheckDirection45 * lowerStepRaycastLength45, Color.blue);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 2), stepCheckDirection45 * lowerStepRaycastLength45, Color.blue);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 3), stepCheckDirection45 * lowerStepRaycastLength45, Color.blue);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 4), stepCheckDirection45 * lowerStepRaycastLength45, Color.blue);

            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset), stepCheckDirectionMinus45 * lowerStepRaycastLength45, Color.green);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 2), stepCheckDirectionMinus45 * lowerStepRaycastLength45, Color.green);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 3), stepCheckDirectionMinus45 * lowerStepRaycastLength45, Color.green);
            //Debug.DrawRay(stepRayLower.transform.position + (raycastHeightOffset * 4), stepCheckDirectionMinus45 * lowerStepRaycastLength45, Color.green);

            #endregion
        }

        private IEnumerator SnapDownDelayAfterJump()
        {
            canSnapDown = false;
            yield return new WaitForSeconds(jumpSnapDownDelay);
            canSnapDown = true;
        }

        // Pushing Objects

        public void StartPushingObject(PushableObject pushableObject)
        {
            currentPushable = pushableObject;

            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;           

            pushableObject.player = myTransform;
            if (gender == "mars")
            {
                pushableObject.playerHand = handTransformM;
            }
            else
            {
                pushableObject.playerHand = handTransformF;
            }

            // Set Flags
            inputHandler.canMove = false;
            inputHandler.canRotate = false;
            inputHandler.isPerformingAction = true;

            // Determine position/direction
            Transform startPosition;
            if(pushableObject.currentTrigger.direction == PushableTriggerCollider.Direction.Forward)
            {
                startPosition = pushableObject.startingPositionForward;
            }
            else
            {
                startPosition = pushableObject.startingPositionBackward;
            }

            // Move player
            myTransform.position = new Vector3(startPosition.position.x, myTransform.position.y, startPosition.position.z);
            myTransform.rotation = startPosition.rotation;

            StartCoroutine(PushDelay());
        }
        
        private IEnumerator PushDelay()
        {
            yield return new WaitForSeconds(0.3f);

            animatorHandler.PlayTargetActionAnimation("PushStart", true, true);
        }

        public void StartMovingObject()
        {
            currentPushable.StartMoving();
            StartPushTimer();
        }

        public void StopMovingObject()
        {
            currentPushable.StopMoving();
            rigidbody.isKinematic = false;
            currentPushable = null;
        }

        public void StartPushTimer()
        {
            StartCoroutine(PushTimer(currentPushable.pushDuration));
        }

        private IEnumerator PushTimer(float time)
        {
            yield return new WaitForSeconds(time);
            animatorHandler.anim.SetTrigger("stopPushing");
        }

        public IEnumerator RotateTowardsTarget(Rigidbody rb, Transform target, float rotateTime)
        {
            float elapsed = 0f;

            // Initial rotation
            Quaternion startRot = rb.rotation;

            // Compute final rotation (horizontal only)
            Vector3 dir = target.position - rb.position;
            dir.y = 0f; // flatten to horizontal plane

            if (dir.sqrMagnitude < 0.0001f)
                yield break; // no direction to rotate toward

            Quaternion endRot = Quaternion.LookRotation(dir);

            while (elapsed < rotateTime)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / rotateTime);

                // Frame-dependent interpolation
                Quaternion newRot = Quaternion.Slerp(startRot, endRot, t);

                rb.MoveRotation(newRot);

                Debug.Log($"Elapsed time: {elapsed}");

                yield return null;
            }

            // Ensure final rotation is exact
            rb.MoveRotation(endRot);
            Debug.Log("Rotation finished");
        }

        // TESTING:
        public void TestAnimHandler()
        {
            Debug.Log(AnimatorHandler.instance.anim.gameObject.name);
        }

    }
}

