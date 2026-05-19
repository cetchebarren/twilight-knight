using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BlazeAISpace;

namespace etchebarren
{
    public class NPC : MonoBehaviour
    {
        public enum NPCType
        {
            Stationary,
            AI
        }

        public enum IdleAnimation
        {
            Standard,
            SitGround,
            SitLow,
            SitMedium,
            SitHigh,
            Crouch,
            Gathering,
            Farming,
            Mining,
            Fishing,
            Hammering,
            PettingHorse,
            StandingAtBarOrTable,
            DrinkingStanding,
            Drunk,
            PushUps,
            FallenIdle
        }

        [Header("NPC Settings")]
        public NPCType npcType;

        [Header("Stationary Only Settings")]
        public IdleAnimation idleAnim;
        public AudioSource audioSource;
        public AudioClip[] hammerCollision;
        [Range(0f, 1f)]
        public float hammerVolume = 0.5f;
        public AudioClip[] pickaxeCollision;
        [Range(0f, 1f)]
        public float pickaxeVolume = 0.5f;
        public AudioClip[] farmingCollision;
        [Range(0f, 1f)]
        public float farmingVolume = 0.5f;
        public AudioClip[] drinkingSound;
        [Range(0f, 1f)]
        public float drinkingVolume = 0.5f;

        [Header("References")]
        public GameObject[] fishingRods;
        public GameObject[] hammers;
        public GameObject[] plows;
        public GameObject[] pickaxes;
        public GameObject[] cups;
        public Animator animator; // Reference to the Animator component
        [SerializeField] BlazeAI blaze;
        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] AudioSource blazeAudioSource;

        [Header("Misc. Settings")]
        public float rotationSpeed = 180f; // Speed of rotation in degrees per second

        [Header("Do Not Set Manually")]
        [SerializeField] bool stationary = true;
        [SerializeField] private Transform target; // The target to rotate towards
        // Private Data
        private Quaternion initialRotation;
        private Coroutine rotateTowardsTarget;
        private Coroutine rotateTowardsInitial;

        void OnValidate()
        {
            ApplyNPCTypeChange();
        }

        void Start()
        {
            switch (idleAnim)
            {
                case IdleAnimation.SitGround:
                    animator.SetBool("sitGround", true);
                    break;
                case IdleAnimation.SitLow:
                    animator.SetBool("sitLow", true);
                    break;
                case IdleAnimation.SitMedium:
                    animator.SetBool("sitMedium", true);
                    break;
                case IdleAnimation.SitHigh:
                    animator.SetBool("sitHigh", true);
                    break;
                case IdleAnimation.Crouch:
                    animator.SetBool("crouch", true);
                    break;
                case IdleAnimation.Gathering:
                    animator.SetBool("gathering", true);
                    break;
                case IdleAnimation.Hammering:
                    animator.SetBool("hammering", true);
                    foreach (GameObject hammer in hammers)
                    {
                        hammer.SetActive(true);
                    }
                    break;
                case IdleAnimation.Mining:
                    animator.SetBool("mining", true);
                    foreach (GameObject pickaxe in pickaxes)
                    {
                        pickaxe.SetActive(true);
                    }
                    break;
                case IdleAnimation.Farming:
                    animator.SetBool("farming", true);
                    foreach (GameObject plow in plows)
                    {
                        plow.SetActive(true);
                    }
                    break;
                case IdleAnimation.Fishing:
                    animator.SetBool("fishing", true);
                    foreach (GameObject fishingRod in fishingRods)
                    {
                        fishingRod.SetActive(true);
                    }
                    break;
                case IdleAnimation.PettingHorse:
                    animator.SetBool("pettingHorse", true);
                    break;
                case IdleAnimation.StandingAtBarOrTable:
                    animator.SetBool("standingAtBarOrTable", true);
                    break;
                case IdleAnimation.Drunk:
                    animator.SetBool("drunk", true);
                    break;
                case IdleAnimation.DrinkingStanding:
                    animator.SetBool("drinkingStanding", true);
                    foreach (GameObject cup in cups)
                    {
                        cup.SetActive(true);
                    }
                    break;
                case IdleAnimation.PushUps:
                    animator.SetBool("pushUps", true);
                    break;
                case IdleAnimation.FallenIdle:
                    animator.SetBool("fallenIdle", true);
                    break;
                default:
                    break;
            }

            stationary = !blaze.enabled;
            initialRotation = transform.rotation;
            // Store the initial rotation
        }

        // Function to start the rotation towards the target
        public void StartRotation(Transform _target)
        {
            if (idleAnim != IdleAnimation.Standard) return;

            if (!stationary) // If we have a character that moves
            {
                blaze.enabled = false; // disable movement (logic)
                animator.SetTrigger("blazeDisabled"); // change to idle animation

                initialRotation = transform.rotation;
            }

            target = _target;
            if (rotateTowardsInitial != null) StopCoroutine(rotateTowardsInitial);
            if (rotateTowardsTarget != null) StopCoroutine(rotateTowardsTarget);
            rotateTowardsTarget = StartCoroutine(RotateToTarget());          
        }

        // Function to start the rotation back to the original rotation
        public void ReturnToInitialRotation()
        {
            if (idleAnim != IdleAnimation.Standard) return;

            if (rotateTowardsTarget != null) StopCoroutine(rotateTowardsTarget);
            if (rotateTowardsInitial != null) StopCoroutine(rotateTowardsInitial);
            rotateTowardsInitial = StartCoroutine(RotateBackToInitial());        
        }

        private IEnumerator RotateToTarget()
        {
            // Calculate the horizontal target rotation
            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0; // Ignore vertical component
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            float signedAngle = Vector3.SignedAngle(transform.forward, target.position - transform.position, Vector3.up);

            // Determine the direction of rotation
            if (signedAngle > 0)
            {
                animator.SetBool("isRotatingRight", true);
                animator.SetBool("isRotatingLeft", false);
            }
            else
            {
                animator.SetBool("isRotatingRight", false);
                animator.SetBool("isRotatingLeft", true);
            }

            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                // Rotate towards the target over time
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            // Ensure the final rotation is exact
            transform.rotation = targetRotation;

            animator.SetBool("isRotatingRight", false);
            animator.SetBool("isRotatingLeft", false);
        }

        private IEnumerator RotateBackToInitial()
        {
            Quaternion targetRotation = initialRotation;
            float signedAngle = Vector3.SignedAngle(transform.forward, initialRotation * Vector3.forward, Vector3.up);

            // Determine the direction of rotation
            if (signedAngle > 0)
            {
                animator.SetBool("isRotatingRight", true);
                animator.SetBool("isRotatingLeft", false);
            }
            else
            {
                animator.SetBool("isRotatingRight", false);
                animator.SetBool("isRotatingLeft", true);
            }

            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                // Rotate towards the target over time
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }

            // Ensure the final rotation is exact
            transform.rotation = targetRotation;

            animator.SetBool("isRotatingRight", false);
            animator.SetBool("isRotatingLeft", false);

            if (!stationary)
            {
                blaze.enabled = true;
                Debug.Log(blaze.state);

                if (!blaze.IsIdle()) // if NPC is moving
                {
                    if (blaze.state == BlazeAI.State.normal || blaze.state == BlazeAI.State.alert)
                    {
                        animator.SetTrigger("restoreWalking");
                    }
                    else
                    {
                        animator.SetTrigger("restoreRunning");
                    }
                }
            }
        }

        public void NPCAnimationEvent(int type)
        {
            switch (type)
            {
                case 1:                   
                    audioSource.PlayOneShot(hammerCollision[Random.Range(0,hammerCollision.Length)], hammerVolume);
                    break;
                case 2:
                    audioSource.PlayOneShot(pickaxeCollision[Random.Range(0, pickaxeCollision.Length)], pickaxeVolume);
                    break;
                case 3:
                    audioSource.PlayOneShot(farmingCollision[Random.Range(0, farmingCollision.Length)], farmingVolume);
                    break;
                case 4:
                    audioSource.PlayOneShot(drinkingSound[Random.Range(0, drinkingSound.Length)], drinkingVolume);
                    break;
                default:
                    break;

            }
        }

        private void ApplyNPCTypeChange()
        {
            if (npcType == NPCType.Stationary)
            {
                blaze.enabled = false;
                navMeshAgent.enabled = false;
                blazeAudioSource.enabled = false;
                stationary = true;
            }
            else if (npcType == NPCType.AI)
            {
                blaze.enabled = true;
                navMeshAgent.enabled = true;
                blazeAudioSource.enabled = true;
                stationary = false;
            }
            //Debug.Log($"NPC type changed to {npcType}");
        }
    }
}
