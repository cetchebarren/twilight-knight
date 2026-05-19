using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace etchebarren
{
    public class ScenePersistentPlayerObject : MonoBehaviour
    {
        public static ScenePersistentPlayerObject instance;

        public SaveManager saveManager;

        public AchievementManager achievementManager;

        public CutsceneManager cutsceneManager;

        public GameObject objectContainer;

        public GameObject player;

        public bool debugShowColliders = false;

        private void Awake()
        {
            if (instance == null)
            {
                // New Game or Game Loaded
                instance = this;
                objectContainer.SetActive(true);

                // Load Game Only
                if (SceneManager.GetActiveScene().name != "CharCreate")
                {
                    Debug.Log("Loading data: Load game selected");

                    string filepath = PlayerPrefs.GetString("currentSaveFilepath", "error");

                    if (ES3.FileExists(filepath))
                    {
                        saveManager.LoadData(filepath);
                    }
                    else
                    {
                        Debug.LogError("Filepath \"" + filepath + "\" not found. Data not loaded.");
                    }         
                }
                else
                {
                    Debug.Log("NOT loading data: New Game selected");
                    // Start recording play time (not an active process, actually just setting start time to use later for calulating play time)
                    // This saves some function calls in an update function, since we don't need to constantly have a record of playtime in-game
                    //achievementManager.SetStartTime(Time.time);
                }
            }
            else
            {
                // Scene Transition (Character already exists, deleting this gameobject)
                Debug.Log("NOT loading data: Scene transition");
                Destroy(gameObject);
            }
        }
    }
}
