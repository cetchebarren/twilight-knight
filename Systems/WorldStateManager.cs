using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace etchebarren
{
    public class WorldStateManager : MonoBehaviour
    {
        public static WorldStateManager instance;

        public InputHandler inputHandler;
        public MapInputHandler mapInputHandler;
        public CameraHandler cameraHandler;
        public OptionsManager optionsManager;
        public PlayerLocomotion playerLocomotion;
        public GameObject horseMountVersion;
        public GameObject horseAIVersion;
        public GameObject interactPrompt;
        public Transform playerTransform;

        public bool newGame = false;

        public bool autosaveEnabled = false;

        // Used to record which chests in world (between scenes) have been opened already.
        public Dictionary<int, bool> chestDictionary = new Dictionary<int, bool>();

        // Used to record which quesy items in world (between scenes) have been picked up already;
        public Dictionary<int, bool> questItemDictionary = new Dictionary<int, bool>();

        // Used to record which world items have been grabbed, to prevent spawning them again when scene reloads
        public List<int> obtainedWorldItemIDs = new List<int>();

        // Used to record which doors have already been unlocked, to prevent needing to unlock twice
        public List<int> unlockedDoorIDs = new List<int>();

        // Used to record which notes/readables have already been read, to prevent obtaining attached items twice
        public List<int> readReadableIDs = new List<int>();

        // Used to record which notes/readables have been grabbed, to prevent spawning them in the world
        public List<int> grabbedReadableIDs = new List<int>();

        /* When the player speaks to a character for the first time, they will play their introduction dialogue 
         * then they will be recorded here so that the next time they speak, that dialogue will not be used */
        public List<string> dialogueAgentsAlreadyMet = new List<string>();

        // Used to record which teleport waypoints have been locked so that they can be properly initialized when loading scene
        public List<int> unlockedTeleportWaypointIDs;

        //contains a list of all teleport waypoints in the current scene as well as useful functions for them (such as scaling with UI zoom)
        public SceneTeleportWaypointsManager currentTeleportWaypointsManager; 

        // For setting scale in SceneTelortWaypointsManager.cs
        public WorldMapTeleportButton[] allWorldMapTeleportWaypoints; 

        public List<string> worldLocationsUnlocked;

        public List<int> deadEnemyIDs;

        public List<string> worldEventsAlreadyTriggered;

        public string lastSceneExitName; // This is assigned when exiting a scene, upon loading a scene, that scene's Scene Spawn Manager will find the corresponding scene entrance using this data
        public string newSceneName;
        public SceneTransitionWeb sceneWeb;
        public bool worldTravel = false; // flag used to determine if world teleport was used, helps determine spawn point in SceneSpawnManager
        public bool respawn = false;

        [Header("Loading Screen")]
        public bool loading = false;
        public GameObject loadingScreen;
        public Slider loadingSlider;
        public Image[] loadingScreens;

        [Header("Eclipse Event")]
        public bool eclipseActive = false;

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

            // Build web of scene transitions used for determining navigation between scenes for quest markers
            sceneWeb = new SceneTransitionWeb();
            sceneWeb.AddTransition("TestingGrounds", "ViridescentWoodlands");
            sceneWeb.AddTransition("ViridescentWoodlands", "Ravenport");
            sceneWeb.AddTransition("FellrockGlen", "TestingGrounds");
            sceneWeb.AddTransition("FellrockGlen", "FellrockCatacombs");
            sceneWeb.AddTransition("FellrockCatacombs", "ScorchedHollow");
            sceneWeb.AddTransition("ScorchedHollow", "OldMine");
            sceneWeb.AddTransition("OldMine", "ViridescentWoodlands");
            sceneWeb.AddTransition("FellrockGlen", "BuriedRuins");
        }

        public void Start()
        {
            // Set Audio Settings on Start
            // Previously this set all settings to default, now if player settings exist it will load those,
            // and if they do not, default settings will be set and saved
            //optionsManager.RestoreDefaultAudioSettings();
            optionsManager.LoadVolumeSettings();
            optionsManager.LoadControlSettings();
            optionsManager.LoadVisualSettings();
            optionsManager.LoadGraphicSettings();
            optionsManager.LoadMiscSettings();
        }

        public void AddNewWorldTeleportPoint(string newScene)
        {
            worldLocationsUnlocked.Add(newScene);
            foreach(WorldMapTeleportButton wmtb in allWorldMapTeleportWaypoints)
            {
                // We will compare the location name with the new scene's name
                bool areEqual = (newScene == wmtb.locationName);
                //Debug.Log(newScene);
                //Debug.Log(wmtb.locationName);
                //Debug.Log(areEqual);
                if (areEqual) wmtb.gameObject.SetActive(true);
            }
        }

        public bool AlreadyMetDialogueAgent(string agentName, bool meetingNow=true)
        {
            foreach(string name in dialogueAgentsAlreadyMet)
            {
                if(agentName == name)
                {
                    return true;
                }
            }
            if(meetingNow) MetDialogueAgent(agentName);
            return false;
        }

        public void MetDialogueAgent(string agentName)
        {
            dialogueAgentsAlreadyMet.Add(agentName);
        }

        public void ChangeScene(object scene, bool _worldTravel = false, bool useLoadingScreen = true)
        {
            // Determine scene type and get the scene name or index
            int sceneIndex = -1;
            string sceneName = null;

            if (scene is int index)
            {
                sceneIndex = index;
            }
            else if (scene is string name)
            {
                sceneName = name;
            }
            else
            {
                Debug.LogError("Invalid scene parameter! Must be an int (scene index) or string (scene name).");
                return;
            }

            // Free cursor if scene is main menu or character creator / new game
            if (sceneIndex == 0 || sceneIndex == 1 || sceneName == "MainMenu" || sceneName == "CharCreate")
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            // Respawn dead enemies on new scene load
            ClearDeadEnemies();

            // Reset progress for relevant quests
            QuestManager.instance.ResetProgessForResettingQuestSteps();

            // Restrict camera movement during load
            cameraHandler.freezeCamera = true;

            // Enable loading screen
            if (useLoadingScreen) loadingScreen.SetActive(true);

            // Set loading flag and loading slider value
            loading = true;
            loadingSlider.value = 0f;

            // Disable inputs
            inputHandler.enabled = false;
            mapInputHandler.enabled = false;

            // Set player sheathe animation state to true
            playerLocomotion.InstantSheatheUnsheathe(true);

            // Set world travel flag based on argument
            worldTravel = _worldTravel;

            // Reset any existing camera modifiers (e.g. boss arena zoom out)
            CameraHandler.instance.ResetCameraModifiers();

            // set eclipse off
            eclipseActive = false;

            // Start Load coroutine based on scene type
            if (sceneName != null)
            {
                StartCoroutine(LoadAsyncScene(sceneName));
            }
            else
            {
                StartCoroutine(LoadAsyncScene(sceneIndex));
            }
        }

        IEnumerator LoadAsyncScene(object scene)
        {
            yield return new WaitForEndOfFrame();

            // Reset inputs, interact UI Element, and Horse
            InputHandler.instance.ResetInputs();
            interactPrompt.SetActive(false);
            ResetHorse();

            // Create asynchronous operation
            AsyncOperation asyncLoad;

            // Determine whether scene is an index (int) or name (string)
            if (scene is int sceneIndex)
            {
                asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            }
            else if (scene is string sceneName)
            {
                asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            }
            else
            {
                Debug.LogError("Invalid scene parameter! Must be an int (scene index) or string (scene name).");
                yield break;
            }

            // Wait until the asynchronous scene fully loads and update status bar
            while (!asyncLoad.isDone)
            {
                loadingSlider.value = asyncLoad.progress;
                yield return null;
            }

            // Ensure loading completion
            loadingSlider.value = 1f;
            Debug.Log("DONE!");

            // Reset UI and allow inputs again, after loading
            loading = false;
            loadingScreen.SetActive(false);
            inputHandler.enabled = true;
            mapInputHandler.enabled = true;

            yield return new WaitForSeconds(0.2f);
            Cursor.lockState = CursorLockMode.Locked;

            yield return new WaitForSeconds(0.3f);
            cameraHandler.freezeCamera = false; // allow camera movement
        }

        public void ResetHorse()
        {
            playerLocomotion.FinishDismount();
            horseAIVersion.SetActive(false);
            horseMountVersion.SetActive(false);
            playerLocomotion.whistleIconManager.SetIconToWhistle();
        }

        public void SaveGame()
        {
            QuestManager.instance.SaveQuestData();
        }

        public void LoadGame()
        {
            QuestManager.instance.LoadQuestData();
        }

        public void ReloadScene()
        {
            respawn = true;
            ChangeScene(GetCurrentSceneIndex());
        }

        public int GetCurrentSceneIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        public void ClearDeadEnemies()
        {
            deadEnemyIDs.Clear();
        }

        public void EclipseActive(bool enabled)
        {
            eclipseActive = enabled;
        }

    }
}
