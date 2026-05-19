using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace etchebarren
{
    public class PushableObject : MonoBehaviour
    {
        [Header("Settings")]
        public float pushDuration = 2f;//time spent in pushing animation
        public int currentPosition = 0;
        public int maxPosition = 1;
        public int minPosition = -1;

        [Header ("References")]
        public Transform startingPositionForward;
        public Transform startingPositionBackward;
        public BoxCollider forwardTrigger;
        public BoxCollider backwardTrigger;
        public PushableTriggerCollider forwardPTC;
        public PushableTriggerCollider backwardPTC;

        [Header("Debug Meshes to Hide in Game")]
        public MeshRenderer[] meshes;
        public bool hideDebug = true;

        [Header("Flags and Temp Values")]
        public PushableTriggerCollider currentTrigger;
        public bool isMoving = false;
        public Coroutine movingCoroutine;
        public Transform player;
        public Transform playerHand;
        private Vector3 playerLastPosition;
        public bool alreadyPushedOnce = false;

        [Header("Optional References")]
        public Animator animator;
        public AudioSource audioSource;

        [Header("Optional Events")]
        public UnityEvent OnFirstPush;

        void Start()
        {
            if (hideDebug)
            {
                foreach(MeshRenderer mesh in meshes)
                {
                    mesh.enabled = false;
                }
            }
        }

        public void StartMoving()
        {
            if(movingCoroutine == null)
            {
                // Play Audio (If there is any)
                if(audioSource != null)
                {
                    audioSource.enabled = true;
                    audioSource.Play();
                }

                // Determine animation and update position counter
                bool forward;
                if (currentTrigger.direction == PushableTriggerCollider.Direction.Forward)
                {
                    currentPosition++;
                    forward = true;
                }
                else
                {
                    currentPosition--;
                    forward = false;
                }    
                
                if(animator != null)
                {
                    animator.enabled = true;
                    animator.SetBool("Forward", forward);
                    animator.SetBool("Backward", !forward);
                }

                movingCoroutine = StartCoroutine(Moving());

                // Check for event
                if (!alreadyPushedOnce)
                {
                    alreadyPushedOnce = true;
                    OnFirstPush.Invoke();
                }
            }
        }

        public void StopMoving()
        {
            isMoving = false;

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.enabled = false;
            }

            if (animator != null)
            {
                animator.SetBool("Forward", false);
                animator.SetBool("Backward", false);
                animator.enabled = false;
            }
        }

        private IEnumerator Moving()
        {
            isMoving = true;
            playerLastPosition = playerHand.position;

            // Store the initial position of the pushable object
            Vector3 initialPosition = transform.position;

            while (isMoving)
            {
                // Calculate movement since the last frame
                Vector3 movement = playerHand.position - playerLastPosition;

                // Project the movement onto the pushable object's forward/backward direction
                Vector3 directionVector = (currentTrigger.direction == PushableTriggerCollider.Direction.Forward)
                    ? transform.forward
                    : -transform.forward;

                // Get the movement along the direction vector
                float movementMagnitude = Vector3.Dot(movement, directionVector);

                // Apply the forward/backward movement to the pushable object
                transform.position += directionVector * movementMagnitude;

                // Update the last position for the next frame
                playerLastPosition = playerHand.position;

                // Wait for the next frame before continuing the loop
                yield return null;
            }

            movingCoroutine = null;
            SetTriggersEnabled(true);
        }

        public void SetTriggersEnabled(bool status)
        {
            forwardTrigger.enabled = status;
            backwardTrigger.enabled = status;
            forwardPTC.alreadyTriggered = false;
            backwardPTC.alreadyTriggered = false;
        }
    }
}
