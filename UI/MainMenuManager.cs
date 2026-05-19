using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace etchebarren
{
    [Serializable]
    public struct Scenery
    {
        public GameObject camera;
        public GameObject sceneryGroup;
    }

    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager instance;

        bool loading = false;
        public Slider loadingSlider;
        public GameObject loadingScreen;
        public GameObject loadingCameraPrefab;

        [Header("Main Menu Only")]
        public bool mainMenu = false;
        public string mostRecentSaveFile;
        public GameObject continueButton;
        public GameObject loadGameButton;
        public GameObject newGameButton;
        public SaveSlot continueSlot;
        public GameObject continueConfirmWindow;
        public GameObject cancelContinueButton;
        public GameObject credits;
        public RectTransform creditsContent;
        public ScrollRect creditsScrollRect;
        public Scrollbar creditsScrollBar;
        public float scrollDuration = 30f;
        public float scrollStart = 586.5f;
        public float scrollStop = 0.6f;
        private Coroutine creditsScrolling;
        public List<Scenery> sceneries = new List<Scenery>();
        public int currentScenery = -1;
        public OptionsManager optionsManager;
        public AudioSource mainMenuMusic;
        public GameObject playIcon;
        public GameObject muteIcon;
        public TextMeshProUGUI creditsButtonText;
        public TextMeshProUGUI creditsButtonPressedText;
        public float creditsElapsedTime = 0f;

        private void Awake()
        {
            if(SceneManager.GetActiveScene().buildIndex < 2)
            {
                Debug.Log("Main Menu Manager set Cursor mode to None");
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 1f;
            }

            if (mainMenu)
            {
                HandleOptions();

                ChooseScenery();

                HandleMainMenuMusic();

                SelectStartingButton();
            }

            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadSave(string filepath)
        {
            // Set filepath for that save slot
            //string filepath = "SaveData/save" + saveSlot.ToString() + ".es3";

            if (ES3.FileExists(filepath))
            {
                // Get target scene index for that save 
                string targetScene = ES3.Load<string>("currentScene", filepath);

                // Save the target save data's file path so that it can be loaded in new scene
                PlayerPrefs.SetString("currentSaveFilepath", filepath);

                // Start loading new scene and enable loading screen / progress
                ChangeScene(targetScene);
            }
            else
            {
                Debug.LogError("Filepath \"" + filepath + "\" not found. Data not loaded.");
            }
        }

        public void ChangeScene(string scene)
        {
            if(scene == "MainMenu" || scene == "CharCreate")
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            loadingScreen.SetActive(true);
            ///Debug.Log(loadingScreen.activeSelf);
            loading = true;
            loadingSlider.value = 0f;

            // Destroy current player so that data is set properly
            if (ScenePersistentPlayerObject.instance != null)
            {
                Instantiate(loadingCameraPrefab);
                Destroy(ScenePersistentPlayerObject.instance.player); 
                Destroy(ScenePersistentPlayerObject.instance.gameObject);
                ScenePersistentPlayerObject.instance = null; // Ensure instance reference is cleared
            }

            StartCoroutine(LoadAsyncScene(scene));
        }

        IEnumerator LoadAsyncScene(string sceneName)
        {
            yield return new WaitForEndOfFrame();
            // The Application loads the Scene in the background as the current Scene runs.
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                loadingSlider.value = asyncLoad.progress;
                yield return null;
            }

            loadingSlider.value = 1f;
            Debug.Log("DONE LOADING SAVE FROM MAIN MENU MANAGER!");
            loading = false;
            loadingScreen.SetActive(false);
        }

        public bool SaveFileExists()
        {
            string relativePath = "saveData/";

            // Create directory if it doesn't exist
            if (!ES3.DirectoryExists(relativePath))
            {
                Debug.Log("Directory not found. No saves assumed.");
                return false; // No saves yet
            }
            
            string[] fileList = ES3.GetFiles(relativePath)
                                  .Where(file => file.EndsWith(".es3"))
                                  .ToArray();

            Debug.Log("File List Size: " + fileList.Length);
            return fileList.Length > 0;
        }


        public string GetMostRecentSaveFile()
        {
            // Get all files in the "saveData/" directory
            string[] fileList = ES3.GetFiles("saveData/");

            // Filter out files that do not end in .es3 and convert to array
            fileList = fileList.Where(file => file.EndsWith(".es3")).ToArray();

            // Sort the filtered fileList by the saved date and time
            fileList = fileList.OrderByDescending(file => {
                string filepath = "saveData/" + file;
                string dateString = ES3.Load<string>("date", filepath);
                string timeString = ES3.Load<string>("time", filepath);
                string dateTimeString = dateString + " " + timeString;
                DateTime dateTime = DateTime.ParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                return dateTime;
            }).ToArray();

            return fileList[0];
        }

        public void OpenContinueConfirmationWindow()
        {
            // Match the continue slot to the information from the recent save
            string filepath = "saveData/" + mostRecentSaveFile;

            continueSlot.SetFilepath(filepath);

            // Set Character Name Text
            string name = ES3.Load<string>("playerName", filepath);
            string level = "<color=#A8A8A8> - Lv." + ES3.Load<int>("playerLevel", filepath).ToString() + "</color>";
            continueSlot.SetNameAndLevel(name + level);

            // Set Level and Location Text
            string location = ES3.Load<string>("currentScene", filepath);
            location = System.Text.RegularExpressions.Regex.Replace(location, "(\\B[A-Z])", " $1");
            location = "Location: " + location;
            continueSlot.SetLocation(location);

            // Set total playtime text
            int totalSeconds = ES3.Load<int>("playtimeInSeconds", filepath);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            string formattedTime = $"{hours}h {minutes}m {seconds}s";
            continueSlot.SetPlaytime("Playtime: " + formattedTime);

            // Set Date and Time
            string dateAndTime = "Saved: " + ES3.Load<string>("date", filepath) + " at " + ES3.Load<string>("time", filepath);
            continueSlot.SetDateAndTime(dateAndTime);
            // Is autosave if file index (fileX.es3) is 0, which is reserved for auto saves
            if (filepath == "saveData/save0.es3")
            {
                continueSlot.SetSaveType("Autosave");
            }
            else
            {
                continueSlot.SetSaveType("Manual");
            }

            // Load a Texture2D from a PNG file and Set Screenshot image
            string screenshotFilepath = Path.ChangeExtension(filepath, ".jpg");
            if (ES3.FileExists(screenshotFilepath)) // added this to prevent errors, check if its still working properly next session!
            {
                Texture2D screenshot = ES3.LoadImage(screenshotFilepath);
                continueSlot.SetScreenshot(screenshot);
            }

            // Open the window now that everything has been set
            continueConfirmWindow.SetActive(true);

            // Auto select the cancel button
            UIChangeSelectedButton.instance.ChangeSelectedButtonTo(cancelContinueButton);
        }

        public void LoadMostRecentSave()
        {
            LoadSave("saveData/" + mostRecentSaveFile);
        }

        public void ExitGame()
        {
            Application.Quit();
        }


        public void DetermineCreditsAction()
        {
            // If credits window is open
            if (credits.activeInHierarchy)
            {
                // If we are currently scrolling
                if (creditsScrolling != null)
                {
                    Debug.Log("Skipping Credits...");
                    StopCoroutine(creditsScrolling);
                    creditsScrolling = null;
                    creditsScrollBar.value = scrollStop;
                    creditsButtonText.text = creditsButtonPressedText.text = "Close Credits";
                }
                // Scroll is complete
                else
                {
                    Debug.Log("Closing Credits...");
                    credits.SetActive(false);
                    creditsButtonText.text = creditsButtonPressedText.text = "Credits";
                }
            }
            // If credits window is closed
            else
            {
                Debug.Log("Starting Credits...");
                OpenCredits();
            }
        }

        public void OpenCredits()
        {
            credits.SetActive(true);

            creditsButtonText.text = creditsButtonPressedText.text = "Skip";

            if(creditsScrolling != null)
            {
                StopCoroutine(creditsScrolling);
                creditsScrollBar.value = scrollStart;
                Debug.Log("Scrolling coroutine stopped.");
            }
            
            creditsScrolling = StartCoroutine(ScrollCredits());       
        }

        private IEnumerator ScrollCredits()
        {
            creditsElapsedTime = 0f;
            float totalDuration = scrollDuration; // desired duration for the scroll
            Debug.Log("Scroll Duration: " + totalDuration);

            float startValue = scrollStart;
            float endValue = scrollStop;

            creditsScrollBar.value = startValue;

            while (creditsElapsedTime < totalDuration)
            {
                // Calculate the current value based on elapsed time
                creditsScrollBar.value = Mathf.Lerp(startValue, endValue, creditsElapsedTime / totalDuration);

                // Increase elapsed time
                creditsElapsedTime += Time.deltaTime;
                //creditsElapsedTime += Time.unscaledDeltaTime;

                // Wait until the next frame
                yield return null;
            }

            // Ensure the final value is exactly the stop value
            creditsScrollBar.value = endValue;

            // Reset the coroutine reference
            creditsScrolling = null;

            creditsButtonText.text = creditsButtonPressedText.text = "Close Credits";

            Debug.Log("Scroll credits coroutine finished...");
        }

        public void HandleMainMenuMusic()
        {
            int playMusic = 1;
            if (PlayerPrefs.HasKey("mainMenuMusic"))
            {
                playMusic = PlayerPrefs.GetInt("mainMenuMusic");
            }
            if (playMusic == 1) mainMenuMusic.mute = false;
            PlayerPrefs.SetInt("mainMenuMusic", playMusic);
            SetMuteIcon(mainMenuMusic.mute);      
        }

        public void ChooseScenery()
        {
            // Disable all sceneries
            for(int i = 0; i < sceneries.Count; i++)
            {
                sceneries[i].camera.SetActive(false);
                sceneries[i].sceneryGroup.SetActive(false);
            }
            //Enable Next Scenery
            int sceneryIndex = 0;
            if (PlayerPrefs.HasKey("mainMenuScenery"))
            {
                sceneryIndex = (PlayerPrefs.GetInt("mainMenuScenery") + 1) % sceneries.Count;             
            }

            PlayerPrefs.SetInt("mainMenuScenery", sceneryIndex);

            // Enable next menu scenery
            sceneries[sceneryIndex].camera.SetActive(true);
            sceneries[sceneryIndex].sceneryGroup.SetActive(true);
        }

        public void ToggleMainMenuMusic()
        {
            bool muted = mainMenuMusic.mute;

            if (muted)
            {
                PlayerPrefs.SetInt("mainMenuMusic", 1);
                mainMenuMusic.mute = false;
                SetMuteIcon(false);
            }
            else
            {
                PlayerPrefs.SetInt("mainMenuMusic", 0);
                mainMenuMusic.mute = true;
                SetMuteIcon(true);
            }
        }

        public void SetMuteIcon(bool muted)
        {
            muteIcon.SetActive(muted);
            playIcon.SetActive(!muted);
        }

        public void SelectStartingButton()
        {
            if (SaveFileExists())
            {
                mostRecentSaveFile = GetMostRecentSaveFile();
                continueButton.SetActive(true);
                loadGameButton.SetActive(true);
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(continueButton);
            }
            else
            {
                continueButton.SetActive(false);
                loadGameButton.SetActive(false);
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(newGameButton);
            }
        }

        public void HandleOptions()
        {
            StartCoroutine(DelayOptions());
            optionsManager.LoadGraphicSettings();
        }

        // When updating volume on start, Mixer may not be fully initialized which makes our changes not apply
        // Delaying this update by one frame fixes the issue and guarantees that the changes are applied.
        private IEnumerator DelayOptions()
        {
            yield return null;
            optionsManager.LoadVolumeSettings();
        }

    }
}
