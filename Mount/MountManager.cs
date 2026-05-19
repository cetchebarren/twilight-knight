using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using MalbersAnimations;
using BlazeAISpace;

namespace etchebarren
{
    public class MountManager : MonoBehaviour
    {
        public static MountManager instance;

        // Mount Spawning
        [Header("References")]
        public MountLocomotion mountLocomotion;
        public PlayerAudioManager playerAudioManager;
        public GameObject mountObject;
        public GameObject horseAIObject;
        public MountStats mountStats;
        public Transform playerChestTransform;
        public Transform mountSpawnCenter;
        public CompanionBehaviour companionBehaviour;
        public WhistleIconManager whistleIconManager;
        public PlayerLocomotion playerLocomotion;
        public WaypointManager waypointManager;
        public Animator mountAnim;
        public Animator horseAIAnim;
        public BlazeAI horseAI;
        public GameObject mountStatusHUD;
        public GameObject healthSlider;
        public GameObject recoverySliderObj;
        public Slider recoverySlider;
        public GameObject navMeshAgent;
        public MountAnimEvents mountAnimEvents;
        public StepsManager stepsManagerMount;
        public StepsManager stepsManagerAI;
        public Transform saddleTransform;

        [Header("Mount Spawn Availability Flags")]
        public bool mountUnlocked = false;
        public bool mountCanSpawnInThisScene = false;
        public bool canCurrentlySpawn = true;

        [Header("Coroutines")]
        public Coroutine forceFollowCoroutine;

        [Header("Settings")]
        public float spawnRadius;
        public float ignoreEnemiesDuration = 10.0f;
        public Vector3 venusSaddlePosition;
        public Vector3 marsSaddlePosition;

        [Header("Recovery Timer")]
        public bool mountIsHealthy = true;
        public float recoveryDuration = 10.0f;
        public float elapsedTime = 0.0f;

        [Header("Audio")]
        public bool horseCanRespondVocally = true;
        public AudioSource audioSourceHorseAI;
        public AudioClip[] horseWhistleResponse;

        [Header("Debugging")]
        public bool debugSpawnPoint = false;
        public GameObject spawnTestValid;
        public GameObject spawnTestInvalid;

        [Header("References for Feed Event")]
        public GameObject carrotModelM;
        public GameObject carrotModelF;
        public GameObject carrotModelH;

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

        public void DetermineCallAction()
        {
            if (playerLocomotion.onMount) return;
            // Mount is spawned
            if (horseAIObject.activeSelf)
            {
                StopForceFollow();
                horseAI.friendly = false;
                // Toggle Follow/Wait
                companionBehaviour.follow = !companionBehaviour.follow;

                if(companionBehaviour.follow)
                {
                    playerAudioManager.PlayCallHorseWhistle();
                    HorseWhistleResponse();
                    TextNotificationsManager.instance.NewTextNotifaction(mountStats.horseName + " will follow", false); //true prevents duplicate messages for 3 seconds
                    waypointManager.StartHorseIconCoroutine();
                }
                else
                {
                    playerAudioManager.PlayStopHorseWhistle();
                    HorseWhistleResponse();
                    TextNotificationsManager.instance.NewTextNotifaction(mountStats.horseName + " will wait", false); //true prevents duplicate messages for 3 seconds
                    waypointManager.StartHorseIconCoroutine();
                }
                // Update UI Whistle Icon
                whistleIconManager.DetermineAndSetIcon(true, companionBehaviour.follow); // (mount is active, mount is following)
            }
            // Mount is not spawned
            else
            {
                playerAudioManager.PlayCallHorseWhistle();
                if (mountUnlocked)
                {
                    if (!mountCanSpawnInThisScene)
                    {
                        string notification = "Cannot call " + mountStats.horseName + " from this location";
                        TextNotificationsManager.instance.NewTextNotifaction(notification, true); //true prevents duplicate messages for 3 seconds
                        return;
                    }

                    if (!canCurrentlySpawn)
                    {
                        string notification = "Cannot call " + mountStats.horseName + " at this time";
                        TextNotificationsManager.instance.NewTextNotifaction(notification, true); //true prevents duplicate messages for 3 seconds
                        return;
                    }
                 
                    HorseWhistleResponse();
                    FindMountSpawn();
                }
            }
        }

        public void ForceFollow()
        {
            if (forceFollowCoroutine != null) return;

            companionBehaviour.follow = true;
            whistleIconManager.DetermineAndSetIcon(true, companionBehaviour.follow); // (mount is active, mount is following)
            forceFollowCoroutine = StartCoroutine(IgnoreEnemies());
            playerAudioManager.PlayIgnoreHorseClick();
            waypointManager.StartHorseIconCoroutine();
            HorseWhistleResponse();
        }

        private IEnumerator IgnoreEnemies()
        {
            TextNotificationsManager.instance.NewTextNotifaction(mountStats.horseName + " will briefly ignore enemies", false); //true prevents duplicate messages for 3 seconds
            mountStats.attackHitBox.enabled = false;
            horseAI.friendly = true;
            yield return new WaitForSeconds(ignoreEnemiesDuration);
            horseAI.friendly = false;
            forceFollowCoroutine = null;
        }

        public void StopForceFollow()
        {
            if(forceFollowCoroutine != null)
            {
                StopCoroutine(forceFollowCoroutine);
                forceFollowCoroutine = null;
            }
        }

        public void HorseWhistleResponse()
        {
            if (horseCanRespondVocally)
            {
                horseCanRespondVocally = false;
                StartCoroutine(HorseResponseDelay());
            }
        }

        private IEnumerator HorseResponseDelay()
        {
            yield return new WaitForSeconds(0.5f);
            audioSourceHorseAI.PlayOneShot(horseWhistleResponse[Random.Range(0, horseWhistleResponse.Length)], 0.5f);
            yield return new WaitForSeconds(1.5f);
            horseCanRespondVocally = true;
        }

        public void FindMountSpawn()
        {
            int maxAttempts = 100; // how many spawn changes (not counting offset attempts)
            int offsetAttempts = 3; // how many times to increase height and try again if point is underneath terrain (hits nothing on down raycast)
            float heightOffset = 3f; // height to add to raycast start position each time a downward raycast hits nothing (most likely under terrain)
            float colliderCheckOffset = 0.7f; // When checking if hit point is within a collider (would be invalid position) we check above the hit point
            bool validPointFound = false;
            RaycastHit hit = new RaycastHit();

            for (int i = 0; i < maxAttempts && !validPointFound; i++)
            {
                // Get Random point within circle to use as spawn point
                Vector3 randomPoint = GetRandomPointInRadius(mountSpawnCenter.position, spawnRadius);
                Vector3 hitPoint = randomPoint;

                // Perform a raycast downwards from spawn point
                if (Physics.Raycast(randomPoint, Vector3.down, out hit, 20f))
                {
                    // Check if the hit point is on the NavMesh
                    if (hit.collider != null)
                    {
                        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, 1.0f, NavMesh.AllAreas))
                        {
                            hitPoint = navMeshHit.position;
                            // Check if the hit position with offset is within any collider using OverlapSphere
                            Vector3 offsetHitPoint = hitPoint + Vector3.up * colliderCheckOffset;
                            Collider[] nearbyColliders = Physics.OverlapSphere(offsetHitPoint, 0.5f); // Adjust the sphere radius as needed
                            if (nearbyColliders.Length == 0)
                            {
                                // Hit point with offset is NOT within a collider : Valid Spawn Position
                                validPointFound = true;
                                //Debug.Log($"Hit Object: {hit.collider.gameObject.name}, Tag: {hit.collider.gameObject.tag}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, "On NavMesh: {true}");
                            }
                            //else Debug.Log("Hit point with offset IS within a collider : Invalid Spawn Position");
                        }
                        //else Debug.Log("Hit point not on a navmesh");
                    } 
                    //else Debug.Log("Raycast did not hit anything");
                }
                else
                {
                    //Debug.Log("Downward raycast did not hit anything. Attempting with offset.");
                    // Apply the offset and retry the downward raycast
                    randomPoint = randomPoint + Vector3.up * heightOffset;
                    //Debug.DrawRay(randomPoint, Vector3.down * (20f + heightOffset), Color.blue, 5000f); // Visualize the downward raycast with offset

                    for (int j = 0; j < offsetAttempts; j++)
                    {
                        if (Physics.Raycast(randomPoint, Vector3.down, out hit, 20f + heightOffset))
                        {
                            // Check if the hit point is on the NavMesh
                            if (hit.collider != null)
                            {
                                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navMeshHit, 1.0f, NavMesh.AllAreas))
                                {
                                    hitPoint = navMeshHit.position;
                                    // Check if the hit position with offset is within any collider using OverlapSphere
                                    Vector3 offsetHitPoint = hitPoint + Vector3.up * colliderCheckOffset;
                                    Collider[] nearbyColliders = Physics.OverlapSphere(offsetHitPoint, 0.5f); // Adjust the sphere radius as needed
                                    if (nearbyColliders.Length == 0)
                                    {
                                        // Hit point with offset is NOT within a collider : Valid Spawn Position
                                        validPointFound = true;
                                        //Debug.Log($"Hit Object: {hit.collider.gameObject.name}, Tag: {hit.collider.gameObject.tag}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, "On NavMesh: {true}");
                                    }
                                    //else Debug.Log("Hit point with offset IS within a collider : Invalid Spawn Position");
                                }
                                //else Debug.Log("Hit point not on a navmesh");
                            }
                            //else Debug.Log("Raycast did not hit anything");
                        }
                        //else Debug.Log("Downward raycast with offset did not hit anything on attempt " + j+1);
                    }
                }

                if (debugSpawnPoint && ScenePersistentPlayerObject.instance.debugShowColliders)
                {
                    if (validPointFound)
                    {
                        var spawnedObj = Instantiate(spawnTestValid, randomPoint, Quaternion.identity);
                        var spawnedObj2 = Instantiate(spawnTestValid, hitPoint, Quaternion.identity);
                    }
                    else
                    {
                        var spawnedObj = Instantiate(spawnTestInvalid, randomPoint, Quaternion.identity);
                        var spawnedObj2 = Instantiate(spawnTestInvalid, hitPoint, Quaternion.identity);
                    }
                }
            }

            // After Loop
            if (validPointFound)
            {
                SpawnMount(hit.point);
                waypointManager.StartHorseIconCoroutine();
            }
            else
            {
                string notification = mountStats.horseName + " cannot reach you from this position";
                TextNotificationsManager.instance.NewTextNotifaction(notification, true); //true prevents duplicate messages for 3 seconds
            }
        }

        private Vector3 GetRandomPointInRadius(Vector3 center, float radius)
        {
            #region Note on Shape
            /*Note: To get points within a square instead of circle,
                // Generate random offsets within the square
                float xOffset = Random.Range(-radius, radius);
                float zOffset = Random.Range(-radius, radius);

                // Calculate the position within the circle
                float x = center.x + xOffset;
                float y = playerChestTransform.position.y;
                float z = center.z + zOffset;

                return new Vector3(x, y, z); 
            */
            #endregion

            // Generate random values for polar coordinates
            float randomRadius = radius * Mathf.Sqrt(Random.Range(0f, 1f));
            float angle = Random.Range(0f, 2f * Mathf.PI);

            // Calculate the position within the circle
            float x = center.x + randomRadius * Mathf.Cos(angle);
            float y = playerChestTransform.position.y;
            //float y = center.y;
            float z = center.z + randomRadius * Mathf.Sin(angle);

            return new Vector3(x, y, z);
        }

        private void SpawnMount(Vector3 spawnPosition)
        {
            string notification = mountStats.horseName + " is on the way!";
            TextNotificationsManager.instance.NewTextNotifaction(notification, true); //true prevents duplicate messages for 3 seconds

            stepsManagerMount.inDeepWater = false;
            stepsManagerMount.inShallowWater = false;
            stepsManagerAI.inDeepWater = false;
            stepsManagerAI.inShallowWater = false;
            mountLocomotion.SetSwimming(false, null);

            // We need to resync the NavMeshAgent position, so temporarily turn off update position
            navMeshAgent.GetComponent<NavMeshAgent>().updatePosition = false;
            // Reset position for navMeshAgent gameObject (from of the horse's body)
            navMeshAgent.transform.localPosition = new Vector3(0, 0, 1.33f);
            // Set the position of the horse 
            horseAIObject.transform.position = spawnPosition;
            // Enable the horse gameObject
            horseAIObject.SetActive(true);
            // Reenable update position for Nav Mesh Agent
            navMeshAgent.GetComponent<NavMeshAgent>().updatePosition = true;

            // Calculate the rotation to make the mount face the player's chest
            Vector3 lookAtPosition = new Vector3(playerChestTransform.position.x, horseAIObject.transform.position.y, horseAIObject.transform.position.z);
            horseAIObject.transform.LookAt(lookAtPosition);

            companionBehaviour.follow = true;
            whistleIconManager.DetermineAndSetIcon(true, companionBehaviour.follow); // (mount is active, mount is following)

            healthSlider.SetActive(true);
            recoverySliderObj.SetActive(false);
            mountStatusHUD.SetActive(true);

            horseAI.friendly = false;
            StopForceFollow();

            mountAnimEvents.ResumeFootsteps();
        }

        public void DismissMount()
        {
            string notification = mountStats.horseName + " dismissed.";
            TextNotificationsManager.instance.NewTextNotifaction(notification, true); //true prevents duplicate messages for 3 seconds
            horseAIObject.SetActive(false);
            whistleIconManager.SetIconToWhistle();
            mountStatusHUD.SetActive(false);
            waypointManager.HorseIconOff();
            horseAI.friendly = false;
            StopForceFollow();
        }

        // Recovery

        public void StartRecoveryTimer()
        {
            StartCoroutine(RecoveryTimer());
        }

        private IEnumerator RecoveryTimer()
        {
            recoverySlider.value = 0;
            healthSlider.SetActive(false);
            recoverySliderObj.SetActive(true);

            string triggerName;

            while (elapsedTime < recoveryDuration)
            {
                // Increment the timer
                elapsedTime += Time.deltaTime;

                recoverySlider.value = elapsedTime / recoveryDuration;

                // perform other actions based on the timer's progress if needed

                yield return null; // Wait for the next frame
            }

            // Timer complete, let's determine where to play animation
            horseAIAnim.SetTrigger("Recover");
            mountAnim.SetTrigger("Recover");

            elapsedTime = 0;
        }

        public  void RecoveryComplete() //called from MountAnimEvents
        {
            recoverySliderObj.SetActive(false);
            healthSlider.SetActive(true);

            mountIsHealthy = true;
            mountStats.canRegenHealth = true;
            mountStats.SetHealth(mountStats.maxHealth);
            TextNotificationsManager.instance.NewTextNotifaction(mountStats.horseName + " has recovered!", false); //true prevents duplicate messages for 3 seconds

            //horseAIObject.SetActive(true);
            //mountObject.SetActive(false);
            playerLocomotion.SwapHorseVersions(false);

            // Prevent Mount from being pushed around while dead
            //horseAIObject.GetComponent<Rigidbody>().isKinematic = false;
            mountObject.GetComponent<Rigidbody>().isKinematic = false;

            mountObject.tag = "Animal";
            horseAIObject.tag = "Animal";
            horseAI.enabled = true;
            companionBehaviour.enabled = true;
            //horseAI.SetState(Blaze.AI.State.normal);
            //Debug.Log("Horse Target On Recover: " + horseAI.enemyToAttack);
        }

        public void DeadOnLoad()
        {
            horseAI.enabled = false;  

            //added to fix reference error when loading game with dead horse      
            if (horseAI.overrideAgent != null)
            {
                horseAI.navmeshAgent = horseAI.overrideAgent; //added
            }
          
            mountStats.SetDead();
            mountIsHealthy = false;
            horseAIAnim.SetTrigger("DeadOnLoad");
            StartRecoveryTimer();
        }

        public void ResetMount()
        {
            recoverySliderObj.SetActive(false);
            healthSlider.SetActive(true);

            mountIsHealthy = true;
            mountStats.SetHealth(mountStats.maxHealth);
            //TextNotificationsManager.instance.NewTextNotifaction(mountStats.horseName + " has recovered!", false); //true prevents duplicate messages for 3 seconds

            horseAIObject.SetActive(false);
            mountObject.SetActive(false);

            // Prevent Mount from being pushed around while dead
            //horseAIObject.GetComponent<Rigidbody>().isKinematic = false;
            mountObject.GetComponent<Rigidbody>().isKinematic = false;

            mountObject.tag = "Animal";
            horseAIObject.tag = "Animal";
            horseAI.enabled = true;
            companionBehaviour.enabled = true;
            //horseAI.SetState(Blaze.AI.State.normal);
            //Debug.Log("Horse Target On Recover: " + horseAI.enemyToAttack);
        }

        public void SetSaddlePosition()
        {
            bool venus = PlayerStats.instance.gender == "venus" || PlayerStats.instance.gender == "Venus";

            if (venus)
            {
                saddleTransform.localPosition = venusSaddlePosition;
            }
            else
            {
                saddleTransform.localPosition = marsSaddlePosition;
            }
        }

        public bool CheckForCombat()
        {
            return horseAI.gameObject.activeInHierarchy && !companionBehaviour.isActiveAndEnabled;
        }


        public void FeedMount()
        {
            StartCoroutine(FeedMountCoroutine());
        }

        private IEnumerator FeedMountCoroutine()
        {
            carrotModelM.SetActive(true);
            carrotModelF.SetActive(true);
            playerLocomotion.animatorHandler.PlayTargetActionAnimation("Give", true);
            StartCoroutine(playerLocomotion.RotateTowardsTarget(playerLocomotion.rigidbody, carrotModelH.transform, 0.3f));
            yield return new WaitForSeconds(1f);
            carrotModelM.SetActive(false);
            carrotModelF.SetActive(false);
            carrotModelH.SetActive(true);
            horseAI.enabled = false;
            horseAI.animManager.Play("Eat", 0.2f);
            yield return new WaitForSeconds(3f);
            KeyItem carrot = PlayerInventory.instance.GetItem(PlayerInventory.instance.keyItemsInventory, 824);
            PlayerInventory.instance.RemoveFromInventory(carrot, 1);
            mountStats.GainExp(mountStats.baseExpCost);
            carrotModelH.SetActive(false);
        }
    }
}
