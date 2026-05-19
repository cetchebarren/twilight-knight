using System.Collections;
using System.Collections.Generic;
using MalbersAnimations;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class SceneSpawnManager : MonoBehaviour
    {
        [Header("Spawn References")]
        public Transform defaultSpawnLocation;
        public Transform worldTravelSpawnLocation;

        public List<SceneEntrance> sceneEntrances;

        public bool spawnedFromSaveSlot = false;

        public bool mountCanBeCalledInThisScene = false;

        // Start is called before the first frame update
        void Start()
        {
            // Set Mount Spawn Availability based on scene
            MountManager.instance.mountCanSpawnInThisScene = mountCanBeCalledInThisScene;
            MountManager.instance.canCurrentlySpawn = true; // Reset this value

            // Disable all interaction prompts
            InteractPrompt.instance.ClearInteractionList();

            // Determine starting animation, if any
            if (PlayerStats.instance.dead)
            {
                // Spawned dead from New Game
                if (WorldStateManager.instance.newGame)
                {
                    WorldStateManager.instance.newGame = false;
                    PlayerLocomotion.instance.animatorHandler.PlayTargetActionAnimation("LayingDown", true, true, false, false, 0f);
                    // Fade in from black screen over 8 seconds
                    StartCoroutine(FadeInFromBlackScreen(8f, 3.5f));
                    // Spawn life light effect, and descend towards dead player
                    StartCoroutine(SelfEffectsManager.instance.PlayLifeLightEffect(15f));
                    // Around the time the life light touches the player, fill up their health over time
                    StartCoroutine(PlayerStats.instance.FillHealthOverTime(16f, 20f)); // (delay before filling, amount filled per second)
                    // Enable prompt to allow player to get up, shortly after health has been filled
                    StartCoroutine(EnableInteractPrompt(20f, "Rise, Knight"));
                    // Set camera offset lower to the ground, to be restored after Rise, Knight is triggered
                    CameraHandler.instance.cameraOffset = new Vector3(0f, -0.3f, 0f);
                }
                // Spawn dead after dying in game and respawning / reloading scene
                else
                {
                    PlayerLocomotion.instance.animatorHandler.PlayTargetActionAnimation("LayingDown", true, true, false, false, 0f);
                    // Fade in from black screen over 8 seconds
                    StartCoroutine(FadeInFromBlackScreen(4f, 3.5f));
                    // Spawn life light effect, and descend towards dead player
                    StartCoroutine(SelfEffectsManager.instance.PlayLifeLightEffect(6f));
                    // Around the time the life light touches the player, fill up their health over time
                    StartCoroutine(PlayerStats.instance.FillHealthOverTime(6.5f, 20f)); // (delay before filling, amount filled per second)

                    // Enable prompt to allow player to get up, shortly after health has been filled
                    PlayerLocomotion.instance.StandUp(7f);

                    // Set camera offset lower to the ground, to be restored after Rise, Knight is triggered
                    CameraHandler.instance.cameraOffset = new Vector3(0f, -0.3f, 0f);

                    StartCoroutine(CameraHandler.instance.ResetCameraOffset(6f, 7f));

                }

            }
            else
            {
                StartCoroutine(FadeInFromBlackScreen(2f, 3.5f));
            }

            // Reinitialize animations and flags, cancel swimming, falling, etc.
            InputHandler.instance.interactInput = false;
            PlayerLocomotion.instance.SetSwimming(false, null);
            InputHandler.instance.isGrounded = true;

            DetermineSpawn();

            CheckTerrain();

            StartCoroutine(HandleCameraOnSpawn());

            Cursor.lockState = CursorLockMode.Locked;
        }

        private IEnumerator HandleCameraOnSpawn()
        {
            CameraHandler.instance.freezeCamera = true;
            CameraHandler.instance.ReorientCamera(true);
            yield return new WaitForSeconds(2.0f);
            CameraHandler.instance.freezeCamera = false;
        }

        void DetermineSpawn()
        {
            if (defaultSpawnLocation == null) Debug.LogError("MAKE SURE YOU ASSIGN DEFAULT SPAWN LOCATION");
            if (worldTravelSpawnLocation == null) Debug.LogError("MAKE SURE YOU ASSIGN WORLD TRAVEL SPAWN LOCATION");

            // Close any menus
            PlayerMenuManager.instance.CloseMapMenu(false);
            PlayerMenuManager.instance.ClosePlayerMenu(false);
            WorldStateManager.instance.interactPrompt.SetActive(false);

            // For scene transitions or respawning at the closest waypoint that is also a scene exit
            string lastExitName = WorldStateManager.instance.lastSceneExitName;

            // If respawning, set in WorldStateManager 
            if (WorldStateManager.instance.respawn)
            {
                // Reset respawn flag
                WorldStateManager.instance.respawn = false;

                // Find all GameObjects with the TeleportWaypoint script
                List<GameObject> waypointObjects = new List<GameObject>();
                foreach (TeleportWaypoint waypoint in FindObjectsOfType<TeleportWaypoint>())
                {
                    if (waypoint.enabled)
                    {
                        waypointObjects.Add(waypoint.gameObject);
                    }
                }

                // Find the closest GameObject to the player's transform
                Transform playerTransform = PlayerLocomotion.instance.myTransform;
                GameObject closestWaypoint = null;
                float closestDistance = Mathf.Infinity;
                foreach (GameObject waypoint in waypointObjects)
                {
                    float distance = Vector3.Distance(playerTransform.position, waypoint.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestWaypoint = waypoint;
                    }
                }

                if (closestWaypoint != null)
                {
                    Debug.Log("Closest TeleportWaypoint is: " + closestWaypoint.name + " at a distance of: " + closestDistance);

                    TeleportWaypoint closestTeleportWaypoint = closestWaypoint.GetComponent<TeleportWaypoint>();

                    // Closest waypoint is a scene transition
                    if (closestTeleportWaypoint.sceneExit != null)
                    {
                        lastExitName = closestTeleportWaypoint.sceneExit.sceneToGoTo;
                    }
                    // Closest waypoint is just a teleport waypoint
                    else
                    {
                        Vector3 spawnPos = closestTeleportWaypoint.spawnPoint.position;
                        float rot = closestTeleportWaypoint.spawnPoint.rotation.eulerAngles.y;
                        WorldStateManager.instance.playerTransform.position = spawnPos;
                        WorldStateManager.instance.playerTransform.rotation = Quaternion.Euler(0f, rot, 0f);
                        CameraHandler.instance.ReorientCamera(true); // True means instantly reorient, no animation
                        return;
                    }

                }
                else
                {
                    Debug.Log("No valid teleport waypoint found for respawn, reverting to default spawn point");
                    UseDefaultSpawnLocation();
                    return;
                }
            }

            // If spawned in from loading a save file
            if (spawnedFromSaveSlot)
            {
                spawnedFromSaveSlot = false;
                return;
            }

            // If used fast travel (world) to get here
            if (WorldStateManager.instance.worldTravel && worldTravelSpawnLocation != null)
            {
                Vector3 spawnPos = worldTravelSpawnLocation.position;
                float rot = worldTravelSpawnLocation.rotation.eulerAngles.y;
                WorldStateManager.instance.playerTransform.position = spawnPos;
                WorldStateManager.instance.playerTransform.rotation = Quaternion.Euler(0f, rot, 0f);
                CameraHandler.instance.ReorientCamera(true); // True means instantly reorient, no animation
                ScenePersistentPlayerObject.instance.saveManager.Autosave();
                return;
            }

            // if traversed here through scene entrance/exit
            foreach (SceneEntrance sceneEntrance in sceneEntrances)
            {
                if (sceneEntrance.sceneExit.sceneToGoTo == lastExitName)
                {
                    Vector3 spawnPos = sceneEntrance.spawnPoint.position;
                    float rot = sceneEntrance.spawnPoint.rotation.eulerAngles.y;
                    WorldStateManager.instance.playerTransform.position = spawnPos;
                    WorldStateManager.instance.playerTransform.rotation = Quaternion.Euler(0f, rot, 0f);
                    CameraHandler.instance.ReorientCamera(true); // True means instantly reorient, no animation
                    ScenePersistentPlayerObject.instance.saveManager.Autosave();
                    return;
                }
            }

            UseDefaultSpawnLocation();
        }

        public void UseDefaultSpawnLocation()
        {
            // No match found, spawn at default spawn point
            if (defaultSpawnLocation != null)
            {
                Debug.Log("Default spawn used");
                Vector3 spawnPos = defaultSpawnLocation.position;
                float rot = defaultSpawnLocation.rotation.eulerAngles.y;
                WorldStateManager.instance.playerTransform.position = spawnPos;
                WorldStateManager.instance.playerTransform.rotation = Quaternion.Euler(0f, rot, 0f);
                CameraHandler.instance.ReorientCamera(true); // True means instantly reorient, no animation
            }
            else
            {
                WorldStateManager.instance.playerTransform.position = new Vector3(0f, 0f, 0f);
                WorldStateManager.instance.playerTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
                CameraHandler.instance.ReorientCamera(true); // True means instantly reorient, no animation
            }
        }



        void CheckTerrain()
        {
           //Init terrain for footsteps player
           AnimEvents.instance.playerFootstepSoundManager.FindTerrain();

           //Init for mount and AI horse
           MountStats.instance.mountStepsManager.FindTerrain();
           MountStats.instance.aiStepsManager.FindTerrain();
        }

        /// <summary>
        /// Fades in the black screen from fully opaque (alpha 1) to fully transparent (alpha 0) over the specified duration.
        /// The bias parameter controls the fade speed: a higher bias makes the fade slow at the start and faster towards the end.
        /// </summary>
        /// <param name="duration">The total time for the fade-in effect.</param>
        /// <param name="bias">Controls the fade speed curve. A higher bias value results in a slower start and a faster finish.</param>
        private IEnumerator FadeInFromBlackScreen(float duration, float bias)
        {
            Image blackScreen = PlayerMenuManager.instance.blackScreen;

            blackScreen.gameObject.SetActive(true);
            float elapsedTime = 0f;
            Color screenColor = blackScreen.color;
            float startAlpha = 1f;
            float endAlpha = 0f;

            // Ensure the bias is within a reasonable range (e.g., 0.1 to 5.0)
            bias = Mathf.Clamp(bias, 0.1f, 5.0f);

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                // Apply quadratic easing-out function
                float adjustedT = Mathf.Pow(t, 2f * bias) / (Mathf.Pow(t, 2f * bias) + Mathf.Pow(1 - t, 2f * bias));

                // Interpolate alpha value
                screenColor.a = Mathf.Lerp(startAlpha, endAlpha, adjustedT);
                blackScreen.color = screenColor;

                yield return null;
            }

            // Ensure the final alpha is exactly 0
            screenColor.a = endAlpha;
            blackScreen.color = screenColor;
            blackScreen.gameObject.SetActive(false);
        }

        private IEnumerator EnableInteractPrompt(float delay, string interactionName)
        {
            yield return new WaitForSeconds(delay);

            InteractPrompt.instance.AddMiscInteraction(interactionName);
        }

    }
}
