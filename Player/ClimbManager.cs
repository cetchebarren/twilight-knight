using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class ClimbManager : MonoBehaviour
    {
        public PlayerLocomotion playerLocomotion;
        public InputHandler inputHandler;
        public InteractPrompt interactPrompt;

        [Header("Trigger Flags")]
        public bool alreadyTriggered = false;

        [Header("Height Setting")]
        public Transform heightToSet;
        public bool setPlayerHeight = false;
        public float heightOffset = 0.0f;

        [Header("Debugging")]
        public bool spawnDebugBall = false;
        public GameObject prefabTest;

        // 
        private Vector3 newPos;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Climbable" && !alreadyTriggered)
            {
                if (PlayerLocomotion.instance.isClimbing) return;

                if (PlayerLocomotion.instance.isDodging) return;

                alreadyTriggered = true;

                // If approaching climbable edge while on ground, enable interact prompt to climb
                if (/*!inputHandler.isPerformingAction && */!inputHandler.isJumping && inputHandler.isGrounded)
                {
                    if (inputHandler.isAttacking || inputHandler.isBlocking) return;
                    ClimbableEdge climbableEdge = other.GetComponent<ClimbableEdge>();
                    if(climbableEdge != null)
                    {
                        interactPrompt.AddInteraction(climbableEdge);
                    }                   
                }
                // If colliding with climbable edge while jumping or in the air
                else if (inputHandler.isJumping || !inputHandler.isGrounded)
                {
                    if (inputHandler.isAttacking) AnimEvents.instance.CancelAllAttacks();
                    ClimbableEdge climbableEdge = other.GetComponent<ClimbableEdge>();
                    Climb(climbableEdge);                   
                }
                else
                {
                    Debug.Log("Collided, but no action taken.");
                    Debug.Log("isPerformingAction: " + inputHandler.isPerformingAction);
                    Debug.Log("isJumping: " + inputHandler.isJumping);
                    Debug.Log("isGrounded: " + inputHandler.isGrounded);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Climbable" && alreadyTriggered)
            {
                alreadyTriggered = false;
                ClimbableEdge climbableEdge = other.GetComponent<ClimbableEdge>();
                if (climbableEdge != null)
                {
                    interactPrompt.RemoveInteraction(climbableEdge);
                }
            }
        }

        public void SetPlayerPosition()
        {
            playerLocomotion.myTransform.position = newPos;
        }

        private IEnumerator ClimbNextFrame()
        {
            //yield return null;
            if (inputHandler.isGrounded)
            {
                yield return new WaitForSeconds(0.1f);
            }
            playerLocomotion.animatorHandler.PlayTargetActionAnimation("Climb", true, true);
        }

        public void Climb(ClimbableEdge climbableEdge)
        {
            playerLocomotion.isClimbing = true;
            playerLocomotion.rigidbody.isKinematic = true;
            playerLocomotion.rigidbody.velocity = Vector3.zero;
            inputHandler.canMove = false;
            inputHandler.canRotate = false;
            inputHandler.isPerformingAction = true;
            inputHandler.isJumping = false;

            if (climbableEdge != null)
            {
                Vector3 newHorizontalPosition = climbableEdge.GetClosestPointOnLine(playerLocomotion.myTransform.position);

                newPos = new Vector3(
                    newHorizontalPosition.x,
                    climbableEdge.heightSet.position.y - playerLocomotion.playerOffset,
                    newHorizontalPosition.z);

                SetPlayerPosition();

                Physics.SyncTransforms();

                if (spawnDebugBall)
                {
                    Vector3 testPos = new Vector3(
                        newHorizontalPosition.x,
                        newHorizontalPosition.y,
                        newHorizontalPosition.z);
                    Instantiate(prefabTest, testPos, Quaternion.identity);
                }
            }
            //Debug.Log("Climb triggered 1");
            playerLocomotion.SetSwimming(false, null);
            //Debug.Log(playerLocomotion.myTransform.position);

            StartCoroutine(ClimbNextFrame());
            //playerLocomotion.animatorHandler.PlayTargetActionAnimation("Climb", true, true);
        }
    }
}
