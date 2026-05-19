using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class AnimatorHandler : MonoBehaviour
    {
        public static AnimatorHandler instance;
        //[HideInInspector]
        public Animator anim;
        public Rigidbody rb;
        public CapsuleCollider playerSidesCol;
        public PlayerLocomotion playerLocomotion;

        public bool debugLogOn = false;

        [Header("Rigidbody Interpolation and Collisions")]
        public RigidbodyInterpolation defaultInterpolation;
        public RigidbodyInterpolation rootMotionInterpolation;
        public CollisionDetectionMode defaultCollisionDetection;
        public CollisionDetectionMode rootMotionCollisionDetection;

        [Header("Idle/Combat References")]
        public GameObject shieldHolder;
        public GameObject swordHolder;
        public GameObject shieldAttachmentPoint;
        public GameObject swordAttachmentPoint;
        public GameObject sword;
        public GameObject shield;

        [Header("Earrings")]
        [SerializeField] bool animateEarrings = false;
        public Animator leftEar;
        public Animator rightEar;
        private float lastVertical;
        private float lastHorizontal;
        private bool isSwinging;
        private bool wasMoving;
        public float fullSwingTime = 1f;
        public float moveTimer = 0f;

        [Header("Sheathe References")]
        public GameObject sheathIcon;
        public GameObject combatIcon;

        public bool loadedGame = false;

        [System.Serializable]
        public struct Action
        {
            public string targetAnimation;
            public bool isPerformingAction;
            public bool applyRootMotion;
            public bool canRotate;
            public bool canMove;
            public float crossfade;

            public Action(string anim, bool performing, bool rootMotion, bool rotate, bool move, float fade = 0.3f)
            {
                targetAnimation = anim;
                isPerformingAction = performing;
                applyRootMotion = rootMotion;
                canRotate = rotate;
                canMove = move;
                crossfade = fade;
            }
        }
        [Header("Queued Actions / Perform after current action")]
        public List<Action> queuedActions = new List<Action>();

        private void Awake()
        {
            // Do not Destroy new instances, instead replace, since player can change animators/models
            instance = this;
        }

        private void OnEnable()
        {
            // Do not Destroy new instances, instead replace, since player can change animators/models
            Initialize();
        }

        public void Initialize()
        {
            anim = GetComponent<Animator>();
            AnimatorHandler.instance = this;
            PlayerLocomotion.instance.playerSidesCollider = playerSidesCol;
            PlayerLocomotion.instance.primaryColliderStandingHeight = PlayerLocomotion.instance.playerFeetCollider.height;
            PlayerLocomotion.instance.sidesColliderStandingHeight = PlayerLocomotion.instance.playerSidesCollider.height;
            //PlayerLocomotion.instance.primaryColliderStandingCenter = PlayerLocomotion.instance.playerFeetCollider.center;
            //PlayerLocomotion.instance.sidesColliderStandingCEnter = PlayerLocomotion.instance.playerSidesCollider.center;

            if (loadedGame)
            {
                loadedGame = false;

                if (PlayerLocomotion.instance.inCombat)
                {
                    MoveSword("hands");
                    MoveShield("hands");
                }
                else
                {
                    MoveSword("back");
                    MoveShield("back");
                }
            }
        }

        private void OnAnimatorMove()
        {
            if (!anim.applyRootMotion) return;

            transform.parent.rotation = anim.rootRotation;
            transform.parent.position += anim.deltaPosition;


            /*
            Vector3 deltaPosition = anim.deltaPosition;
            Quaternion deltaRotation = anim.deltaRotation;

            // Use Rigidbody to move and rotate the character
            rb.MovePosition(rb.position + deltaPosition);
            rb.MoveRotation(rb.rotation * deltaRotation);    
            */
        }


        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement, bool isSprinting, bool isBlocking, bool lockedOn, bool isSwimming)
        {
            float v = 0;
            float h = 0;

            if (isSwimming)
            {
                InputHandler.instance.blockInput = false;
            }

            if (isSprinting)
            {
                if (debugLogOn) { Debug.Log("Sprinting"); }
                v = 3;
            }
            else if (!lockedOn)
            {
                if (debugLogOn) { Debug.Log("Not Locked On"); }
                #region Vertical

                if (verticalMovement > 0 && verticalMovement < 0.55f)
                {
                    v = 1.0f;
                }
                else if (verticalMovement > 0.55f)
                {
                    v = 2.0f;
                }
                else if (verticalMovement < 0 && verticalMovement > -0.55f)
                {
                    v = -1.0f;
                }
                else if (verticalMovement < -0.55f)
                {
                    v = -2.0f;
                }
                else
                {
                    v = 0;
                }
                #endregion

                #region Horizontal

                if (horizontalMovement > 0 && horizontalMovement < 0.55f)
                {
                    h = 0.5f;
                }
                else if (horizontalMovement > 0.55f)
                {
                    h = 1.0f;
                }
                else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
                {
                    h = -0.5f;
                }
                else if (horizontalMovement < -0.55f)
                {
                    h = -1.0f;
                }
                else
                {
                    h = 0;
                }
                #endregion

                if (isBlocking)
                {
                    v *= 0.5f;
                    h *= 0.5f;
                }
            }
            else //Locked On
            {
                if (debugLogOn) { Debug.Log("Locked On"); }

                Vector2 inputVector = new Vector2(horizontalMovement, verticalMovement);
                inputVector.Normalize(); // Normalize the input vector

                float magnitude = 1.0f; // Set your desired magnitude

                v = inputVector.y * magnitude;
                h = inputVector.x * magnitude;

                if (isBlocking)
                {
                    v *= 0.5f;
                    h *= 0.5f;
                }

            }

            anim.SetFloat("Vertical", v, 0.1f, Time.deltaTime);
            anim.SetFloat("Horizontal", h, 0.1f, Time.deltaTime);

            if (animateEarrings)
            {
                // Handle Earring Anim
                leftEar.SetFloat("Horizontal", h, 0.1f, Time.deltaTime);
                leftEar.SetFloat("Vertical", v, 0.1f, Time.deltaTime);
                rightEar.SetFloat("Horizontal", h, 0.1f, Time.deltaTime);
                rightEar.SetFloat("Vertical", v, 0.1f, Time.deltaTime);

                // Check if the character is moving
                bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

                if (isMoving)
                {
                    // Player is moving, update last velocity
                    lastVertical = v;
                    lastHorizontal = h;
                    isSwinging = false;
                    wasMoving = true;
                    moveTimer += Time.deltaTime;
                }
                else if (wasMoving && !isMoving && !isSwinging)
                {
                    // Player just stopped moving, start swing effect
                    isSwinging = true;
                    wasMoving = false;
                    float ratio = moveTimer / fullSwingTime;
                    moveTimer = 0f;

                    // Set blend tree parameters for animation
                    leftEar.SetFloat("HorizontalSwing", lastHorizontal * ratio);
                    leftEar.SetFloat("VerticalSwing", lastVertical * ratio);
                    rightEar.SetFloat("HorizontalSwing", lastHorizontal * ratio);
                    rightEar.SetFloat("VerticalSwing", lastVertical * ratio);

                    // Trigger the animation
                    leftEar.SetTrigger("PlaySwingAnimation");
                    rightEar.SetTrigger("PlaySwingAnimation");
                }
            }
        }

        public void CanRotate()
        {
            InputHandler.instance.canRotate = true;
        }

        public void StopRotation()
        {
            InputHandler.instance.canRotate = false;
        }

        public virtual void PlayTargetActionAnimation(string _targetAnimation, bool _isPerformingAction, bool _applyRootMotion = false, bool _canRotate = false, bool _canMove = false, float crossfade=0.3f)
        {
            Debug.Log("Action Triggered: " + _targetAnimation);

            anim.applyRootMotion = _applyRootMotion;
            ToggleRigidbodyModes(anim.applyRootMotion);

            if(crossfade > 0f)
            {
                anim.CrossFade(_targetAnimation, crossfade);
            }
            else
            {
                anim.Play(_targetAnimation, -1, 0f);
            }

            //is performaing action gatekeeping tool to decide if actions can be input
            InputHandler.instance.isPerformingAction = _isPerformingAction;
            InputHandler.instance.canRotate = _canRotate;
            InputHandler.instance.canMove = _canMove;
        }

        public void SetSelfCastSpeed(float speed)
        {
            anim.SetFloat("selfCastSpeed", speed);
        }

        public void MoveSword(string newLocation)
        {
            //Time.timeScale = 0;
            if (newLocation == "back")
            {
                PlayerLocomotion.instance.inCombat = false;

                // Make sword a child of swordHolder
                sword.transform.SetParent(swordHolder.transform);

                // Reset the position, rotation, and scale of the sword
                sword.transform.localPosition = Vector3.zero;
                sword.transform.localRotation = Quaternion.identity;
                sword.transform.localScale = Vector3.one;

                // Set stamina regen amount
                PlayerStats.instance.staminaRegenAmount = PlayerStats.instance.sheathedStaminaRegenAmount;
                // Set sprint stamina cost
                PlayerStats.instance.sprintStaminaCost = PlayerStats.instance.sheathedSprintStaminaCost;
            }
            else if (newLocation == "hands")
            {
                PlayerLocomotion.instance.inCombat = true;

                // Make sword a child of swordAttachmentPoint
                sword.transform.SetParent(swordAttachmentPoint.transform);

                // Reset the position, rotation, and scale of the sword
                sword.transform.localPosition = Vector3.zero;
                sword.transform.localRotation = Quaternion.identity;
                sword.transform.localScale = Vector3.one;

                // Set stamina regen amount
                PlayerStats.instance.staminaRegenAmount = PlayerStats.instance.defaultStaminaRegenAmount;
                // Set sprint stamina cost
                PlayerStats.instance.sprintStaminaCost = PlayerStats.instance.defaultSprintStaminaCost;
            }
            sheathIcon.SetActive(!PlayerLocomotion.instance.inCombat);
            combatIcon.SetActive(PlayerLocomotion.instance.inCombat);
            anim.SetBool("inCombat", PlayerLocomotion.instance.inCombat);
        }

        public void MoveShield(string newLocation)
        {
            //Time.timeScale = 0;
            if (newLocation == "back")
            {
                PlayerLocomotion.instance.inCombat = false;

                // Make shield a child of shieldHolder
                shield.transform.SetParent(shieldHolder.transform);

                // Reset the position, rotation, and scale of the shield
                shield.transform.localPosition = Vector3.zero;
                shield.transform.localRotation = Quaternion.identity;
                shield.transform.localScale = Vector3.one;

                // Set stamina regen amount
                PlayerStats.instance.staminaRegenAmount = PlayerStats.instance.sheathedStaminaRegenAmount;
                // Set sprint stamina cost
                PlayerStats.instance.sprintStaminaCost = PlayerStats.instance.sheathedSprintStaminaCost;

            }
            else if (newLocation == "hands")
            {
                PlayerLocomotion.instance.inCombat = true;

                // Make shield a child of shieldAttachmentPoint
                shield.transform.SetParent(shieldAttachmentPoint.transform);

                // Reset the position, rotation, and scale of the shield
                shield.transform.localPosition = Vector3.zero;
                shield.transform.localRotation = Quaternion.identity;
                shield.transform.localScale = Vector3.one;

                // Set stamina regen amount
                PlayerStats.instance.staminaRegenAmount = PlayerStats.instance.defaultStaminaRegenAmount;
                // Set sprint stamina cost
                PlayerStats.instance.sprintStaminaCost = PlayerStats.instance.defaultSprintStaminaCost;
            }
            sheathIcon.SetActive(!PlayerLocomotion.instance.inCombat);
            combatIcon.SetActive(PlayerLocomotion.instance.inCombat);
            anim.SetBool("inCombat", PlayerLocomotion.instance.inCombat);
        }

        public void SetAnimateEarrings(bool _animateEarrings, bool animateLeft, bool animateRight)
        {
            animateEarrings = _animateEarrings;
            leftEar.enabled = animateLeft;
            rightEar.enabled = animateRight;
        }

        public void ToggleRigidbodyModes(bool usingRootMotion)
        {
            if (playerLocomotion.onMount)
            {
                rb.interpolation = RigidbodyInterpolation.None;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            else
            {
                rb.interpolation = usingRootMotion ? rootMotionInterpolation : defaultInterpolation;
                rb.collisionDetectionMode = usingRootMotion ? rootMotionCollisionDetection : defaultCollisionDetection;
            }
        }

        public void TryDequeueAction()
        {
            if (queuedActions.Count > 0)
            {
                Action action = queuedActions[0];
                queuedActions.RemoveAt(0);
                PlayTargetActionAnimation(action.targetAnimation, action.isPerformingAction, action.applyRootMotion, action.canRotate, action.canMove, action.crossfade);
            }
        }

        public void QueueAction(
            string anim,
            bool performing,
            bool rootMotion,
            bool rotate,
            bool move,
            float fade = 0.3f)
        {
            Action a = new Action(anim, performing, rootMotion, rotate, move, fade);
            queuedActions.Add(a);
        }

    }
}
