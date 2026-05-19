using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class CameraHandler : MonoBehaviour
    {
        public static CameraHandler instance;

        public Camera cameraObject;
        public InputHandler inputHandler;
        public ControllerUIManager controllerUIManager;

        [SerializeField] private Transform cameraPivotTransform;

        public Transform playerTransform;
        public PlayerLocomotion playerLocomotion;

        [Header("Camera Settings")]
        public bool freezeCamera = false;
        /* Prevent unwanted camera rotation:
        * set to true on player only in CharCreate scene, false on player prefab
        * set to true when loading new scene in WorldStateManager.cs
        * set to false when loaded scene finish in WorldStateManager.cs*/

        public float cameraSmoothSpeed = 1; //bigger = longer for cam to catch up
        //public float controllerCameraMultiplier = 3.0f; // not a setting
        public float mouseCameraMultiplier = 0.01f; // not a setting

        public float leftAndRightSpeedSetting = 20.0f; //Default 40
        public float upAndDownSpeedSetting = 10.0f; //Default 20

        [SerializeField] private float leftAndRightRotationSpeed = 20.0f; //Default 120
        [SerializeField] private float upAndDownRotationSpeed = 10.0f; //Default 60

        [SerializeField] private float rotationSmoothing = 1.0f;
        [SerializeField] private float minimumPivot = -30.0f;
        [SerializeField] private float minimumPivotSwimming = 0f;
        [SerializeField] private float maximumPivot = 45.0f;
        [SerializeField] private float cameraCollisionRadius = 0.2f;
        [SerializeField] private LayerMask collideWithLayers;
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private LayerMask raycastIgnoreLayers;
        public Vector3 cameraOffset = Vector3.zero;
        public float distanceToCurrentTarget = 0.0f;

        [Header("Lock On Settings")]
        public LockOn currentLockOnTarget;
        public float maxLockOnDistance = 30.0f;
        public List<LockOn> availableTargets = new List<LockOn>();
        public LockOn nearestLockOnTarget;
        public LockOn leftLockTarget;
        public LockOn rightLockTarget;
        public float lockedPivotPosition = 2.25f;
        public float unlockedPivotPosition = 1.65f;
        public float minLockedOnVerticalAngle = -50.0f;
        public float maxLockedOnVerticalAngle = 50.0f;
        [SerializeField] private float lockOnRadius = 70.0f;
        public float lockOnBreakOffset = 10.0f; //the amount of distance added to maxLockOnDistance to determine unlocking

        [Header("Camera Values")]
        private Vector3 cameraVelocity;
        private Vector3 cameraObjectPosition; //used for camera collisions (moves the camera object to this pos when colliding)
        public float leftAndRightLookAngle;
        public float upAndDownLookAngle;
        // SET DEF AND MPUNT CAMERA POS Z (CAM DISTANCE) in player settings
        public float defaultCameraPositionZ = -2.372f; //used for collision and default camera distance
        public float mountedCameraPositionZ = -3.0f; //used for collision and mounted camera distance
        private float targetCameraPositionZ; //ditto
        public float cameraDistanceMultiplier = 1.0f;

        [Header("Event Related Camera Modifiers")]
        public float cameraDistanceModifier = 1.0f; //Distance
        public float lockedPivotModifier = 1.0f; // Height
        public float unlockedPivotModifier = 1.0f; // Height
        private Coroutine cameraTransition;

        [Header("Player Setting")]
        public float reorientationDuration = 0.15f; // 0 is instantly
        public bool targetOverride = false;
        private Coroutine overrideCoroutine;

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

        void Start()
        {
            defaultCameraPositionZ = cameraObject.transform.localPosition.z;
            ConfigureSensitivityBasedOnInput();
        }

        public void HandleAllCameraActions()
        {
            if(playerTransform != null)
            {
                if (!targetOverride)
                {
                    FollowTarget();
                }

                HandleRotations();
                
                CameraCollisions();
            }
        }

        public void FollowTarget()
        {
            Vector3 targetCameraPositionZ = Vector3.SmoothDamp(
                transform.position,
                playerTransform.position + cameraOffset,
                ref cameraVelocity,
                cameraSmoothSpeed // removed deltatime, attempting to fix mount issues
            );
            transform.position = targetCameraPositionZ;
        }

        private void HandleRotations()
        {
            float horizontal = inputHandler.mouseX;
            float vertical = inputHandler.mouseY;
            if (freezeCamera) horizontal = vertical = 0f;

            // NOT LOCKED ON
            if (inputHandler.lockedOn == false && currentLockOnTarget == null)
            {
                if (inputHandler.usingController)
                {
                    leftAndRightLookAngle = Mathf.Lerp(leftAndRightLookAngle, leftAndRightLookAngle + (horizontal * leftAndRightRotationSpeed), rotationSmoothing * Time.deltaTime);
                    upAndDownLookAngle = Mathf.Lerp(upAndDownLookAngle, upAndDownLookAngle - (vertical * upAndDownRotationSpeed), rotationSmoothing * Time.deltaTime);
                }
                else
                {
                    leftAndRightLookAngle = Mathf.Lerp(leftAndRightLookAngle, leftAndRightLookAngle + (horizontal * leftAndRightRotationSpeed), rotationSmoothing);
                    upAndDownLookAngle = Mathf.Lerp(upAndDownLookAngle, upAndDownLookAngle - (vertical * upAndDownRotationSpeed), rotationSmoothing);
                }

                if(!playerLocomotion.isSwimming)
                {
                    upAndDownLookAngle = Mathf.Clamp(upAndDownLookAngle, minimumPivot, maximumPivot);
                }
                else
                {
                    upAndDownLookAngle = Mathf.Clamp(upAndDownLookAngle, minimumPivotSwimming, maximumPivot);
                }

                Vector3 cameraRotation = Vector3.zero;
                Quaternion targetRotation;

                cameraRotation.y = leftAndRightLookAngle;
                targetRotation = Quaternion.Euler(cameraRotation);
                transform.rotation = targetRotation;

                cameraRotation = Vector3.zero;
                cameraRotation.x = upAndDownLookAngle;
                targetRotation = Quaternion.Euler(cameraRotation);
                cameraPivotTransform.localRotation = targetRotation;
            }
            //LOCKED ON
            else
            {
                if (currentLockOnTarget == null) return;

                Vector3 dir = currentLockOnTarget.transform.position - transform.position;
                dir.Normalize();
                dir.y = 0;

                Quaternion targetRotation = Quaternion.LookRotation(dir);
                transform.rotation = targetRotation;

                dir = currentLockOnTarget.transform.position - cameraPivotTransform.position;
                dir.Normalize();

                targetRotation = Quaternion.LookRotation(dir);
                Vector3 eulerAngle = targetRotation.eulerAngles;
                eulerAngle.y = 0;

                // Normalize the angle to be between -180 and 180 degrees
                float normalizedAngle = Mathf.Repeat(eulerAngle.x + 180f, 360f) - 180f;

                // Clamp the vertical rotation within the specified range
                if(!playerLocomotion.isSwimming)
                {
                    eulerAngle.x = Mathf.Clamp(normalizedAngle, minLockedOnVerticalAngle, maxLockedOnVerticalAngle);
                }
                else
                {
                    eulerAngle.x = Mathf.Clamp(normalizedAngle, minimumPivotSwimming, maxLockedOnVerticalAngle);
                }


                // Set the clamped angle to the camera pivot's local rotation
                cameraPivotTransform.localEulerAngles = eulerAngle;

                // Remember info
                leftAndRightLookAngle = transform.eulerAngles.y;
                upAndDownLookAngle = eulerAngle.x;

                //break out of lock on
                float distanceFromTarget = Vector3.Distance(playerTransform.position, currentLockOnTarget.transform.position);

                if(distanceFromTarget >= maxLockOnDistance + lockOnBreakOffset)
                {
                    inputHandler.lockedOn = false;
                    inputHandler.targetIconPosition.StopUpdatingIconPosition();
                    ClearLockOnTargets();
                }
            }
        }

        private void CameraCollisions()
        {
            if(!PlayerLocomotion.instance.onMount)
            {
                targetCameraPositionZ = defaultCameraPositionZ * cameraDistanceMultiplier * cameraDistanceModifier;
            }
            else
            {
                targetCameraPositionZ = mountedCameraPositionZ * cameraDistanceMultiplier * cameraDistanceModifier;
            }

            RaycastHit hit;
            Vector3 direction = cameraObject.transform.position - cameraPivotTransform.position;
            direction.Normalize();

            if (Physics.SphereCast(cameraPivotTransform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetCameraPositionZ), collideWithLayers))
            {
                float distancefromHitObject = Vector3.Distance(cameraPivotTransform.position, hit.point);
                targetCameraPositionZ = -(distancefromHitObject - cameraCollisionRadius);
            }

            if (Mathf.Abs(targetCameraPositionZ) < cameraCollisionRadius)
            {
                targetCameraPositionZ = -cameraCollisionRadius;
            }

            cameraObjectPosition.z = Mathf.Lerp(cameraObject.transform.localPosition.z, targetCameraPositionZ, 0.2f);
            cameraObject.transform.localPosition = cameraObjectPosition;
        }

        public void HandleLockOn()
        {
            availableTargets.Clear();

            float shortestDistance = Mathf.Infinity;
            float shortestDistanceOfLeftTarget = -Mathf.Infinity;
            float shortestDistanceOfRightTarget = Mathf.Infinity;

            // Args: starting position, radius, layers to check (~0 is ALL), and whether to check Trigger Colliders: https://docs.unity3d.com/ScriptReference/QueryTriggerInteraction.html
            Collider[] colliders = Physics.OverlapSphere(playerTransform.position, 26, ~0, QueryTriggerInteraction.Collide);
            if (colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    LockOn lockOn = colliders[i].GetComponent<LockOn>();
                    if (lockOn == null || !lockOn.enabled)
                        continue;

                    if (lockOn != null)
                    {
                        Vector3 lockTargetDirection = lockOn.transform.position - playerTransform.position;
                        float distanceFromTarget = Vector3.Distance(playerTransform.position, lockOn.transform.position);
                        float viewableAngle = Vector3.Angle(lockTargetDirection, cameraObject.transform.forward);
                        RaycastHit hit;
                        //Debug.Log("---------------------------------------");
                        //Debug.Log("Targets before cast #"+ i + ": " + availableTargets.Count);
                        if (viewableAngle > -lockOnRadius && viewableAngle < lockOnRadius && distanceFromTarget <= maxLockOnDistance)
                        {
                            if(Physics.Linecast(cameraObject.transform.position, lockOn.lockOnPoint.position, out hit, ~raycastIgnoreLayers.value, QueryTriggerInteraction.Collide)) //old: playerTransform.position
                            {
                                Debug.DrawLine(cameraObject.transform.position, lockOn.lockOnPoint.position, Color.red, 2f);
                                Transform root = hit.collider.transform.root; // check root to prevent enemy's internal objects from blocking raycast
                                //Debug.Log("Target Raycast from: " + cameraObject.transform.position);

                                if ((environmentLayer.value & (1 << hit.collider.gameObject.layer)) != 0 && !root.CompareTag("Enemy"))
                                {
                                    // blocked by ennvironment
                                    Debug.Log("Raycast to " + lockOn.gameObject.name + " Blocked by environment layer -> " + hit.collider.name);
                                    continue;
                                }

                                /*Debug.Log(
                                    "Valid raycast to target " + lockOn.gameObject.name + " HIT-> " + hit.collider.name +
                                    " | LayerIndex: " + hit.collider.gameObject.layer +
                                    " | LayerName: " + LayerMask.LayerToName(hit.collider.gameObject.layer) +
                                    " | InEnvironmentMask: " + ((environmentLayer.value & (1 << hit.collider.gameObject.layer)) != 0) +
                                    " | InIgnoreMask: " + ((raycastIgnoreLayers.value & (1 << hit.collider.gameObject.layer)) != 0)
                                );*/

                                availableTargets.Add(lockOn);
                                Enemy enemy = lockOn.GetComponent<Enemy>();
                                if (enemy != null) enemy.ShowStatusOnLockOn();                     
                            }
                        }
                    }
                }

                if (availableTargets.Count > 0)
                {
                    for (int j = 0; j < availableTargets.Count; j++)
                    {
                        float distanceFromTarget = Vector3.Distance(playerTransform.position, availableTargets[j].transform.position);

                        if (distanceFromTarget < shortestDistance)
                        {
                            shortestDistance = distanceFromTarget;
                            nearestLockOnTarget = availableTargets[j];
                        }

                        if (inputHandler.lockedOn)
                        {
                            Vector3 relativeEnemyPosition = inputHandler.transform.InverseTransformPoint(availableTargets[j].transform.position);
                            var distanceFromLeftTarget = relativeEnemyPosition.x;
                            var distanceFromRightTarget = relativeEnemyPosition.x;

                            if (relativeEnemyPosition.x <= 0.00 && distanceFromLeftTarget > shortestDistanceOfLeftTarget && availableTargets[j] != currentLockOnTarget)
                            {
                                shortestDistanceOfLeftTarget = distanceFromLeftTarget;
                                leftLockTarget = availableTargets[j];
                            }
                            else if (relativeEnemyPosition.x >= 0.00 && distanceFromRightTarget < shortestDistanceOfRightTarget && availableTargets[j] != currentLockOnTarget)
                            {
                                shortestDistanceOfRightTarget = distanceFromRightTarget;
                                rightLockTarget = availableTargets[j];
                            }
                        }
                    }
                }
            }
        }

        public void ClearLockOnTargets()
        {
            availableTargets.Clear();
            nearestLockOnTarget = null;
            leftLockTarget = null;
            rightLockTarget = null;
            currentLockOnTarget = null;
        }

        public void CancelLockOn()
        {
            inputHandler.lockedOn = false;
            inputHandler.targetIconPosition.StopUpdatingIconPosition();
            ClearLockOnTargets();
        }

        public void SetCameraHeight()
        {
            Vector3 velocity = Vector3.zero;
            Vector3 newLockedPosition = new Vector3(0, lockedPivotPosition * lockedPivotModifier);
            Vector2 newUnlockedPosition = new Vector3(0, unlockedPivotPosition * unlockedPivotModifier);

            if(currentLockOnTarget != null)
            {
                cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(cameraPivotTransform.transform.localPosition, newLockedPosition, ref velocity, Time.deltaTime);
            }
            else
            {
                cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(cameraPivotTransform.transform.localPosition, newUnlockedPosition, ref velocity, Time.deltaTime);
            }
        }

        public void ConfigureSensitivityBasedOnInput()
        {
            //Debug.Log("Configure sensitivity for controller: " + usingController);
            bool usingController = controllerUIManager.isUsingController();

            if (usingController)
            {
                upAndDownRotationSpeed = upAndDownSpeedSetting;
                leftAndRightRotationSpeed = leftAndRightSpeedSetting;
            }
            else
            {
                upAndDownRotationSpeed = upAndDownSpeedSetting * mouseCameraMultiplier;
                leftAndRightRotationSpeed = leftAndRightSpeedSetting * mouseCameraMultiplier;
            }
        }

        public void SetTargetToClosest()
        {
            //Debug.Log("SetTargetClosest triggered");
            ClearLockOnTargets();

            HandleLockOn();

            if (nearestLockOnTarget != null) // Check if there is a new target
            {
                Debug.Log("Acquired New Target: " + nearestLockOnTarget.name);

                inputHandler.targetIconPosition.StopUpdatingIconPosition();
                ClearLockOnTargets();

                HandleLockOn();
                currentLockOnTarget = nearestLockOnTarget;
                inputHandler.lockedOn = true;
                inputHandler.targetIconPosition.StartUpdatingIconPosition();
            }
            else
            {
                Debug.Log("No New Target Available");
                CancelLockOn();
            }
        }

        public void ReorientCamera  (bool instantly=false)
        {
            StartCoroutine(ReorientCoroutine(instantly));
            //Debug.Log("PlayerTransform Y euler angle: " + playerTransform.eulerAngles.y);
        }

        private IEnumerator ReorientCoroutine(bool instantly=false)
        {
            //Debug.Log("Reorienting Camera!");

            CancelLockOn(); // If reorienting via scene load, for example. Reorienting through input typically requires being unlocked anyway.

            float reorientDur = reorientationDuration;
            if (instantly) reorientDur = 0.0f;
           
            if (reorientDur == 0.0f)
            {
                leftAndRightLookAngle = Mathf.Repeat(playerTransform.eulerAngles.y, 360.0f);
                upAndDownLookAngle = 0.0f;
            }
            else
            {
                float initialAngle_LR = Mathf.Repeat(leftAndRightLookAngle, 360.0f);
                float targetAngle_LR = Mathf.Repeat(playerTransform.eulerAngles.y, 360.0f);

                float initialAngle_UD = upAndDownLookAngle;
                float targetAngle_UD = 0.0f;

                // Use a timer to track the progress of the transition
                float timer = 0.0f;

                while (timer < reorientDur)
                {
                    if (inputHandler.lockedOn) //if player locks on, cancel reorientation
                        yield break;

                    // Increment the timer
                    timer += Time.deltaTime;

                    // Calculate the interpolation factor based on the timer and duration
                    float t = Mathf.Clamp01(timer / reorientDur);

                    // Use Quaternion.Slerp to smoothly interpolate between initial and target angle
                    leftAndRightLookAngle = Mathf.LerpAngle(initialAngle_LR, targetAngle_LR, t);
                    upAndDownLookAngle = Mathf.LerpAngle(initialAngle_UD, targetAngle_UD, t);

                    // Wait for the end of the frame
                    yield return null;
                }

                // Ensure the final rotation is exactly the target rotation
                leftAndRightLookAngle = targetAngle_LR;
                upAndDownLookAngle = 0.0f;
            }
        }

        public float GetDistanceToExteriorOfCurrentTarget()
        {
            // Project the positions onto the horizontal plane by setting their Y-coordinates to the same value
            Vector3 playerPos = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
            Vector3 targetPos = new Vector3(currentLockOnTarget.transform.position.x, 0f, currentLockOnTarget.transform.position.z);

            // Calculate the distance in the horizontal plane
            float distanceToCenter = Vector3.Distance(playerPos, targetPos);

            // Next subtract the character's radius to determine the distance to the enemy character's
            // capsule collider. This is an accurate distance to the enemy's collider.
            float distanceToExteriorOfCurrentTarget = distanceToCenter - currentLockOnTarget.characterRadius;

            return distanceToExteriorOfCurrentTarget;
        }

        public void StartCameraOverride(Transform newTarget, float duration)
        {
            if(overrideCoroutine != null)
            {
                StopCoroutine(overrideCoroutine);
            }
            overrideCoroutine = StartCoroutine(FollowTargetOverride(newTarget, duration));
        }

        public void StopOverrideCoroutine()
        {
            if (overrideCoroutine != null)
            {
                StopCoroutine(overrideCoroutine);
            }
            targetOverride = false;
        }

        private IEnumerator FollowTargetOverride(Transform newTarget, float duration)
        {
            targetOverride = true;

            Vector3 initialPosition = transform.position;
            float timer = 0f;

            // Initialize velocity for SmoothDamp
            Vector3 velocity = Vector3.zero;

            while (timer < duration)
            {
                float t = timer / duration;

                // Calculate the target position based on Lerp for smooth interpolation
                Vector3 targetCameraPosition = Vector3.Lerp(initialPosition, newTarget.position + cameraOffset, t);

                // Update the camera position
                transform.position = targetCameraPosition;

                timer += Time.deltaTime;
                yield return null; // Wait for the next frame before the next iteration
            }

            // Ensure the final position is exactly as intended
            transform.position = newTarget.position;

            targetOverride = false;

            overrideCoroutine = null;
        }

        public void SetCameraSensitivityH(float leftAndRight)
        {
            leftAndRightSpeedSetting = leftAndRight;
            ConfigureSensitivityBasedOnInput();
        }

        public void SetCameraSensitivityV(float upAndDown)
        {
            //Debug.Log("BEFORE upAndDownSpeedSetting:" + upAndDownSpeedSetting);
            /// Debug.Log("SET TO upAndDown: " + upAndDown);
            upAndDownSpeedSetting = upAndDown;
            //Debug.Log("AFTER upAndDownSpeedSetting:" + upAndDownSpeedSetting);
            ConfigureSensitivityBasedOnInput();
        }

        public void SetCameraDistanceMutliplier(float multiplier)
        {
            cameraDistanceMultiplier = multiplier;
        }

        public void SetReorientDuration(float duration)
        {
            reorientationDuration = duration;
        }

        /// <summary>
        /// Transitions the camera height from startingHeight to 0 over the specified duration.
        /// </summary>
        /// <param name="duration">The total time for the height transition.</param>
        public IEnumerator ResetCameraOffset(float duration, float delay=0)
        {
            yield return new WaitForSeconds(delay);

            Vector3 startOffset = cameraOffset;
            Vector3 endOffset = new Vector3(0, 0, 0);
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                // Interpolate the camera offset height
                cameraOffset = Vector3.Lerp(startOffset, endOffset, t);

                yield return null;
            }

            // Ensure the final offset is exactly (0, 0, 0)
            cameraOffset = endOffset;
        }

        public void SetCameraModifiers(float distanceMod, float lockedHeightMod, float unlockedHeightMod, float t)
        {
            if(cameraTransition != null)
            {
                StopCoroutine(cameraTransition);
            }

            cameraTransition = StartCoroutine(TransitionCameraModifiers(distanceMod, lockedHeightMod, unlockedHeightMod, t));
        }

        public IEnumerator TransitionCameraModifiers(float distanceMod, float lockedHeightMod, float unlockedHeightMod, float t)
        {
            // Store the initial values before transitioning
            float initialCameraDistanceModifier = cameraDistanceModifier;
            float initialLockedPivotModifier = lockedPivotModifier;
            float initialUnlockedPivotModifier = unlockedPivotModifier;

            // Initialize a timer
            float elapsedTime = 0f;

            // Smoothly transition over the time 't'
            while (elapsedTime < t)
            {
                // Calculate the interpolation factor (0 to 1)
                float progress = elapsedTime / t;

                // Interpolate the modifiers between the initial and target values
                cameraDistanceModifier = Mathf.Lerp(initialCameraDistanceModifier, distanceMod, progress);
                lockedPivotModifier = Mathf.Lerp(initialLockedPivotModifier, lockedHeightMod, progress);
                unlockedPivotModifier = Mathf.Lerp(initialUnlockedPivotModifier, unlockedHeightMod, progress);

                // Increment elapsed time by the frame duration
                elapsedTime += Time.deltaTime;

                // Wait for the next frame
                yield return null;
            }

            // Ensure the values are exactly set to the target at the end
            cameraDistanceModifier = distanceMod;
            lockedPivotModifier = lockedHeightMod;
            unlockedPivotModifier = unlockedHeightMod;
            cameraTransition = null;
        }

        public void ResetCameraModifiers()
        {
            cameraDistanceModifier = 1.0f;
            lockedPivotModifier = 1.0f;
            unlockedPivotModifier = 1.0f;
        }

    }
}
