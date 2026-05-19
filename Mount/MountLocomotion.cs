using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BlazeAISpace;

namespace etchebarren
{
    public class MountLocomotion : MonoBehaviour
    {
        public static MountLocomotion instance;

        [Header("References")]
        InputHandler inputHandler;
        Transform cameraObject;
        Vector3 moveDirection;
        Vector3 projectedVelocity;
        Vector3 normalVector;
        Animator playerAnim;
        public Animator anim;
        public new Rigidbody rigidbody;
        private Coroutine smoothStopCoroutine;
        public PlayerAudioManager playerAudioManager;
        public MountStats mountStats;

        [Header("Flags")]
        public bool isGrounded = true;
        public bool isSprinting = false;
        public bool isJumping = false;
        public bool isSwimming = false;
        public bool canSmallSplash = true;

        [Header("Values")]
        public float gravityScale = 1.5f;
        [SerializeField] float walkingSpeed = 1.0f;
        [SerializeField] float runningSpeed = 2.0f;
        [SerializeField] float sprintingSpeed = 5.0f;
        [SerializeField] float rotationSpeed = 10;

        [Header("Input Smoothing")]
        public float smoothSpeed = 5.0f;
        private float smoothedHorizontalInput;
        private float smoothedVerticalInput;
        private float currentSpeed = 0;
        private float speedChangeRate = 5.0f;

        [Header("Ground Check & Jumping")]
        public float inAirTimer = 0;
        public float lastAirTimer = 0.0f;
        [SerializeField] float groundedCheckSphereRadius = 0.3f;
        [SerializeField] LayerMask groundLayers;
        public Transform groundCheckSphereLocation;
        private Vector3 smoothedNormalVector;
        public float smoothingFactor = 5.0f;
        public float minAngle = 5.0f;
        public float maxAngle = 45.0f;
        public float slopeAnimThreshold = 15f; //the positive and negative threshhold for a slope angle to reach -1 to 1 in the animator
        public Transform raycastStartPosition;
        public bool downhill = false;

        [Header("Swimming")]
        public GameObject swimCollider;
        public float targetHeight = 0f; // accounts for height of player object
        [SerializeField] GameObject waterPlane;
        public float swimmingOffset = 1.3f;
        public float buoyancy = 0.5f;
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
        public float rippleForwardOffset = 1.0f;
        public float smallSplashForwardOffset = 1.0f;
        public float bigSplashForwardOffset = 1.0f;

        private float smoothVelX;
        private float smoothVelY;
        private float speedVel;
        public float smoothTime = 0.075f;
        private Quaternion targetRotation;
        private Vector3 cachedWorldInput;



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
        }

        // Start is called before the first frame update
        void Start()
        {
            inputHandler = InputHandler.instance;
            rigidbody = GetComponent<Rigidbody>();
            cameraObject = Camera.main.transform;
            playerAnim = AnimatorHandler.instance.anim;
            cachedWorldInput = transform.forward;
            targetRotation = transform.rotation;
        }

        void FixedUpdate()
        {
            rigidbody.MoveRotation(targetRotation);

            if (isSwimming)
            {
                targetHeight = waterPlane.transform.position.y - swimmingOffset;

                if (isGrounded)
                {
                    newYPosition = Mathf.Lerp(
                        transform.position.y,
                        targetHeight,
                        Time.fixedDeltaTime * buoyancy
                    );
                }
                else
                {
                    newYPosition = targetHeight;
                }

                Vector3 newPos = new Vector3(
                    transform.position.x,
                    newYPosition,
                    transform.position.z
                );

                rigidbody.MovePosition(newPos);
            }
            else
            {
                rigidbody.AddForce(
                    Vector3.down * 9.81f * gravityScale,
                    ForceMode.Acceleration
                );
            }
        }

        void Update()
        {
            float delta = Time.deltaTime;

            // -----------------------
            // INPUT
            // -----------------------
            inputHandler.TickInput(delta);

            float rawHorizontalInput = inputHandler.horizontal;
            float rawVerticalInput = inputHandler.vertical;

            smoothedHorizontalInput = Mathf.SmoothDamp(
                smoothedHorizontalInput, rawHorizontalInput, ref smoothVelX, smoothTime
            );

            smoothedVerticalInput = Mathf.SmoothDamp(
                smoothedVerticalInput, rawVerticalInput, ref smoothVelY, smoothTime
            );

            Vector3 input = new Vector3(smoothedHorizontalInput, 0f, smoothedVerticalInput);

            // -----------------------
            // WORLD SPACE (camera-relative)
            // -----------------------
            Vector3 groundNormal = Vector3.up;
            cachedWorldInput = CameraRelativeFlatten(input, groundNormal);

            // -----------------------
            // SPEED
            // -----------------------
            float inputMagnitude = Mathf.Clamp01(input.magnitude);

            float targetSpeed;
            if (inputMagnitude < 0.5f)
            {
                isSprinting = false;
                targetSpeed = walkingSpeed;
            }
            else if (isSprinting)
            {
                targetSpeed = sprintingSpeed;
            }
            else
            {
                targetSpeed = runningSpeed;
            }

            currentSpeed = Mathf.SmoothDamp(
                currentSpeed, targetSpeed, ref speedVel, 0.15f
            );

            anim.SetFloat("moveAmount", inputMagnitude);

            // -----------------------
            // ROTATION (CALCULATE ONLY)
            // -----------------------
            GetPlaneNormals();
            HandleGroundCheck();

            if (isGrounded || isSwimming)
            {
                if (isSwimming)
                {
                    smoothedNormalVector = waterPlane.transform.up;
                }

                Quaternion currentRotation = transform.rotation;

                Quaternion forwardRotation = Quaternion.FromToRotation(Vector3.up, smoothedNormalVector);

                Quaternion yRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y, 0);
                Vector3 localForward = yRotation * Vector3.forward;

                Vector3 adjustedForward = forwardRotation * localForward;

                if (cachedWorldInput.sqrMagnitude > 0.0001f)
                {
                    adjustedForward = Vector3.ProjectOnPlane(
                        cachedWorldInput,
                        smoothedNormalVector
                    ).normalized;
                }

                Quaternion finalRotation = Quaternion.LookRotation(adjustedForward, Vector3.up);

                float maxStep = rotationSpeed * 100f * delta;

                targetRotation = Quaternion.RotateTowards(
                    currentRotation,
                    finalRotation,
                    maxStep
                );

                inAirTimer = 0;

                // stamina
                if (isSprinting)
                {
                    if (mountStats.stamina <= 0)
                    {
                        isSprinting = false;
                    }
                    else
                    {
                        mountStats.stamina -= mountStats.sprintStaminaCost;
                        mountStats.staminaBar.SetStat(mountStats.stamina, false, false);
                    }
                }
            }
            else
            {
                inAirTimer += delta;
            }

            // -----------------------
            // LOCAL SPACE INPUT (AFTER ROTATION TARGET IS DECIDED)
            // -----------------------
            Vector3 localSpaceInput = transform.InverseTransformDirection(cachedWorldInput);

            float turn = localSpaceInput.x;

            // slightly stronger dead zone now that rotation is stable
            if (Mathf.Abs(turn) < 0.1f)
                turn = 0f;

            float forward = localSpaceInput.z;

            float targetX = turn * 2f;
            float targetY = forward * currentSpeed;

            // -----------------------
            // ANIMATOR
            // -----------------------
            anim.SetFloat("Horizontal", targetX, 0.1f, delta);
            anim.SetFloat("Vertical", targetY, 0.1f, delta);

            playerAnim.SetFloat("mountVertical", targetY);
            playerAnim.SetFloat("mountHorizontal", targetX);
        }

        Vector3 CameraRelativeFlatten(Vector3 input, Vector3 localUp)
        {
            Quaternion flatten = Quaternion.LookRotation(-localUp, cameraObject.forward) * Quaternion.Euler(-90f, 0, 0);

            return flatten * input;
        }

        private void GetPlaneNormals()
        {
            raycastStartPosition.rotation = Quaternion.Euler(0, raycastStartPosition.rotation.eulerAngles.y, raycastStartPosition.rotation.eulerAngles.z);///////
            RaycastHit hit;
            //if (Physics.Raycast(raycastStartPosition.position, Vector3.down, out hit, 2.0f, groundLayers))
            if (Physics.Raycast(raycastStartPosition.position, -raycastStartPosition.up, out hit, 2.0f, groundLayers))
            {
                // Debug the hit point and the normal vector
                Debug.DrawLine(raycastStartPosition.position, hit.point, Color.blue);

                // Log the raw (unsmoothed) normal vector
                //Debug.Log("Raw Normal Vector: " + hit.normal);

                // Check if the character is facing uphill or downhill
                float dotProduct = Vector3.Dot(hit.normal, raycastStartPosition.forward);
                downhill = dotProduct >= 0 && hit.normal != new Vector3(0, 1f, 0);

                //Debug.Log("Downhill: " + downhill);

                if (Vector3.Angle(hit.normal, Vector3.up) < maxAngle)
                {
                    smoothedNormalVector = Vector3.Lerp(smoothedNormalVector, hit.normal, 1 - Mathf.Exp(-smoothingFactor * Time.deltaTime));
                    //if (minAngle < Vector3.Angle(hit.normal, Vector3.up))
                    //{
                    //    // Smooth the normal vector using a damping approach
                    //    smoothedNormalVector = Vector3.Lerp(smoothedNormalVector, hit.normal, 1 - Mathf.Exp(-smoothingFactor * Time.deltaTime));
                    //}
                }

                //Debug.Log(smoothedNormalVector);
            }
        }


        public void ToggleGallop()
        {
            if (!isGrounded && !isSwimming)
            {
                isSprinting = false;
                return;
            }

            //HANDLE SPRINT
            if (inputHandler.moveAmount >= 0.5f && (isGrounded || isSwimming)) //if moving, set sprinting to true
            {
                if(isSprinting == false)
                {
                    playerAudioManager.PlaySpeedUpHorseClick();      
                }
                isSprinting = true;
            }
            else //not moving or falling, sprinting false
            {
                isSprinting = false;
            }
        }

        private void HandleGroundCheck()
        {
            isGrounded = Physics.CheckSphere(groundCheckSphereLocation.position, groundedCheckSphereRadius, groundLayers);
            anim.SetBool("Grounded", isGrounded);
        }

        public void AttemptToJump()
        {
            anim.SetTrigger("Jump");
        }

        // SWIMMING:
        public void SetSwimming(bool _isSwimming, GameObject _waterPlane)
        {
            isSwimming = _isSwimming;
            if (!isSwimming)
            {
                if(waterPlane != null)
                {
                    waterPlane.GetComponent<WaterPlane>().alreadyTriggeredSwimMount = false;
                }
            }

            waterPlane = _waterPlane;
            
            if (!isSwimming)
            {
                StopRipples();
            }
            else
            {
                if (!isGrounded)
                {
                    InstantiateBigSplash();
                }
                else
                {
                    InstantiateSmallSplash();
                }
            }

            foreach (CapsuleCollider cc in damageColliders)
            {
                cc.isTrigger = isSwimming;
            }

            anim.SetBool("isSwimming", isSwimming);

            playerAudioManager.PlaySwimmingAudio(isSwimming);
        }

        public void InstantiateBigSplash()
        {
            Vector3 offset = transform.forward * bigSplashForwardOffset;
            // These prefabs have a script to delete themselves already
            var splash = Instantiate(
                bigSplashPrefab,
                new Vector3(
                transform.position.x + offset.x,
                waterPlane.transform.position.y,
                transform.position.z + offset.z),
            Quaternion.identity);
            splash.transform.parent = waterPlane.transform;
        }

        public void InstantiateSmallSplash()
        {
            if (!canSmallSplash) return;
            StartCoroutine(SmallSpawnCooldown());
            // These prefabs have a script to delete themselves already
            Vector3 pos = transform.position + transform.forward * smallSplashForwardOffset;
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
            if (waterPlane == null)
            {
                StopRipples();
                return;
            }
            Vector3 offset = transform.forward * rippleForwardOffset;
            Vector3 spawnPosition = new Vector3(
                transform.position.x + offset.x,
                waterPlane.transform.position.y,
                transform.position.z + offset.z
            );


            // Spawn the ripple facing upward (90° X)
            var ripple = Instantiate(ripplePrefab, spawnPosition, Quaternion.Euler(90f, 0f, 0f));
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
    }
}
