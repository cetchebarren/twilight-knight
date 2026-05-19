using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace etchebarren
{
    public class SaveMenu : MonoBehaviour
    {
        public static SaveMenu instance;

        [Header("Main Menu (Only) Settings")]
        public bool mainMenu = false;
        public UnityEvent cancelLoadGame;
        private InputAction cancelAction;

        public TextMeshProUGUI saveMenuTitle;

        public GameObject saveSlotPrefab;
        public GameObject newSaveSlotPrefab;
        public Transform saveSlotsParent;

        // UI
        public AutoScroll saveMenuAutoscroll;
        public Scrollbar saveMenuScrollbar;

        // Confirmation
        public SaveSlot confirmationSlot;
        public TextMeshProUGUI confirmationText;
        public GameObject confirmationWindow;
        public GameObject cancelButton;

        // Back To Main Menu / Exit Game
        public GameObject backToMainMenuWindow;
        public TextMeshProUGUI backToMainMenuText;
        public string backToMainMenuExitGameOption;

        [Header("In Game Menu Only")]
        public SaveManager saveManager;
        public Button saveTab;

        [Header("Do Not Set Manually")]
        public SaveSlot selectedSaveSlot;
        public bool loading = true;

        private void Awake()
        {
            instance = this;
            Debug.Log("Created instance of Save Menu: " + SaveMenu.instance);
        }

        private void OnEnable()
        {
            if (mainMenu)
            {
                var uiModule = EventSystem.current?.currentInputModule as InputSystemUIInputModule;
                if (uiModule != null)
                {
                    cancelAction = uiModule.cancel; // grab the bound Cancel action
                    cancelAction.performed += OnCancelPerformed;
                    cancelAction.Enable();
                }
            }
        }

        private void OnDisable()
        {         
            if (cancelAction != null )
            {
                cancelAction.performed -= OnCancelPerformed;
                cancelAction.Disable();
            }
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            OnCancelLoadGame();
        }

        public void OpenSaveMenu()
        {
            loading = false;
            saveMenuTitle.text = "Save Game";
            UpdateSaveSlotList(loading);
            gameObject.SetActive(true);
            SelectFirstSaveSlot();
        }

        public void OpenLoadMenu()
        {
            if(mainMenu && gameObject.activeInHierarchy)
            {
                cancelLoadGame.Invoke();
            }

            loading = true;
            saveMenuTitle.text = "Load Game";
            UpdateSaveSlotList(loading);
            gameObject.SetActive(true);
            SelectFirstSaveSlot();
        }

        public void OpenConfirmationWindow()
        {
            string message;
            if (loading)
            {
                message = "Load this save?";
                if (!mainMenu) message += " All unsaved progress will be lost.";
            }
            else
            {
                message = "Overwrite this save?";
            }
            confirmationText.text = message;

            // Match the confirmation slot to the information of the selected save slot
            confirmationSlot.SetNameAndLevel(selectedSaveSlot.nameAndLevel.text);
            confirmationSlot.SetLocation(selectedSaveSlot.location.text);
            confirmationSlot.SetPlaytime(selectedSaveSlot.playtime.text);
            confirmationSlot.SetDateAndTime(selectedSaveSlot.dateAndTime.text);
            confirmationSlot.SetSaveType(selectedSaveSlot.saveType.text);
            Texture2D screenshot = ES3.LoadImage(Path.ChangeExtension(selectedSaveSlot.filepath, ".jpg"));
            confirmationSlot.SetScreenshot(screenshot);

            // Open the window now that everything has been set
            confirmationWindow.SetActive(true);

            // Auto select the cancel button
            UIChangeSelectedButton.instance.ChangeSelectedButtonTo(cancelButton);
        }

        public void Confirm()
        {
            if (loading)
            {
                selectedSaveSlot.LoadThisSlot();
            }
            else
            {
                saveManager?.SaveData(selectedSaveSlot.filepath);
                saveTab?.onClick.Invoke();
            }
        }

        public void SelectFirstSaveSlot()
        {
            if(gameObject.activeInHierarchy) StartCoroutine(DelayedSelect());
        }

        private IEnumerator DelayedSelect(int frames=5)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }

            // Check if the parent object has any children
            if (saveSlotsParent.childCount > 0)
            {
                GameObject targetButton = saveSlotsParent.GetChild(0).GetComponent<SaveSlot>().buttonGameObject;
                //GameObject targetButton = saveSlotsParent.GetChild(0).transform.GetChild(1).gameObject;
                Debug.Log("Select First Save Slot: " + targetButton.name);
                Debug.Log("UIChangeSelectedButton obj: " + UIChangeSelectedButton.instance.gameObject.name);
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(targetButton);
            }
        }

        public void UpdateSaveSlotList(bool loading)
        {
            // Destroy all children of saveSlotsParent (to refresh the list)
            foreach (Transform child in saveSlotsParent)
            {
                Destroy(child.gameObject);
            }

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

            // Remove saveData/save0.es3 from fileList if loading is false (can't save over autosave)
            if (!loading)
            {
                fileList = fileList.Where(file => file != "save0.es3").ToArray();
            }

            List<Button> saveSlotButtons = new List<Button>();

            foreach (string fileName in fileList)
            {
                string filepath = "saveData/" + fileName;

                if (ES3.FileExists(filepath))
                {
                    var saveSlot = Instantiate(saveSlotPrefab, saveSlotsParent);
                    SaveSlot slot = saveSlot.GetComponent<SaveSlot>();
                    Button slotButton = saveSlot.transform.GetChild(1).GetComponent<Button>();
                    saveSlotButtons.Add(slotButton);

                    // Record filepath into slot (to be referenced when button is pressed)
                    slot.SetFilepath(filepath);

                    // Set Character Name Text
                    string name = ES3.Load<string>("playerName", filepath);
                    string level = "<color=#A8A8A8> - Lv." + ES3.Load<int>("playerLevel", filepath).ToString() + "</color>";
                    slot.SetNameAndLevel(name + level);

                    // Set Level and Location Text
                    string location = ES3.Load<string>("currentScene", filepath);
                    location = System.Text.RegularExpressions.Regex.Replace(location, "(\\B[A-Z])", " $1");
                    location = "Location: " + location;
                    slot.SetLocation(location);

                    // Set background alpha if main menu slot
                    if (mainMenu)
                    {
                        slot.buttonImage.color = new Color(0f, 0f, 0f, 0.93f);
                        slot.selectButtonFade.minAlpha = 0.75f;
                        slot.selectButtonFade.maxAlpha = 1.00f;
                    }

                    // Set total playtime text
                    int totalSeconds = ES3.Load<int>("playtimeInSeconds", filepath);
                    int hours = totalSeconds / 3600;
                    int minutes = (totalSeconds % 3600) / 60;
                    int seconds = totalSeconds % 60;
                    string formattedTime = $"{hours}h {minutes}m {seconds}s";
                    slot.SetPlaytime("Playtime: " + formattedTime);
                    // Set Date and Time
                    string dateAndTime = "Saved: " + ES3.Load<string>("date", filepath) + " at " + ES3.Load<string>("time", filepath);
                    slot.SetDateAndTime(dateAndTime);
                    // Is autosave if file index (fileX.es3) is 0, which is reserved for auto saves
                    if (fileName == "save0.es3")
                    {
                        slot.SetSaveType("Autosave");
                    }
                    else
                    {
                        slot.SetSaveType("Manual");
                    }
                    // Load a Texture2D from a PNG file and Set Screenshot image
                    string screenshotFilepath = Path.ChangeExtension(filepath, ".jpg");
                    if (ES3.FileExists(screenshotFilepath)) // added this to prevent errors, check if its still working properly next session!
                    {
                        Texture2D screenshot = ES3.LoadImage(screenshotFilepath);
                        slot.SetScreenshot(screenshot);
                    }
                }
            }

            if (!loading)
            {
                var newSave = Instantiate(newSaveSlotPrefab, saveSlotsParent);
                Button slotButton = newSave.transform.GetChild(1).GetComponent<Button>();
                saveSlotButtons.Add(slotButton);
            }

            SetButtonNavigations(saveSlotButtons);

            // Reselect the top slot
            if (!mainMenu)
            {
                SelectFirstSaveSlot();
            }
        }

        public void OpenMainMenuExitGameWindow(string type)
        {
            if (saveManager.saving == null)
            {
                backToMainMenuExitGameOption = type;

                switch (type)
                {
                    case "BackToMainMenu":
                        backToMainMenuText.text = "Back to Main Menu?\nAll unsaved progress will be lost.";
                        backToMainMenuWindow.SetActive(true);
                        break;
                    case "ExitGame":
                        backToMainMenuText.text = "Exit Game?\nAll unsaved progress will be lost.";
                        backToMainMenuWindow.SetActive(true);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                TextNotificationsManager.instance.NewTextNotifaction("Saving in progress. Please wait.", false); //true prevents duplicate messages for 3 seconds
            }
        }

        public void ConfirmMainMenuExitGame()
        {
            if (saveManager.saving == null)
            {
                switch (backToMainMenuExitGameOption)
                {
                    case "BackToMainMenu":
                        BackToMainMenu();
                        break;
                    case "ExitGame":
                        ExitGame();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                TextNotificationsManager.instance.NewTextNotifaction("Saving in progress. Please wait.", false); //true prevents duplicate messages for 3 seconds
            }
        }

        private void BackToMainMenu()
        {
            Debug.Log("BACK TO MAIN MENU SELECTED");
            MainMenuManager.instance.ChangeScene("MainMenu");
        }

        private void ExitGame()
        {
            Debug.Log("EXIT GAME SELECTED");
            Application.Quit();                
        }

        private void OnCancelLoadGame()
        {
            cancelLoadGame.Invoke();
        }

        public void ResetSaveMenuScrollbar()
        {
            StartCoroutine(ResetScrollbar());
        }

        private IEnumerator ResetScrollbar()
        {
            yield return null;
            saveMenuScrollbar.value = 1f;
        }

        private void SetButtonNavigations(List<Button> buttons)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                Button button = buttons[i];

                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.Explicit;

                // Up button: previous in list, or loop to last
                nav.selectOnUp = i > 0 ? buttons[i - 1] : buttons[buttons.Count - 1];

                // Down button: next in list, or loop to first
                nav.selectOnDown = i < buttons.Count - 1 ? buttons[i + 1] : buttons[0];

                // Left/right: optional
                nav.selectOnLeft = null;
                nav.selectOnRight = null;

                button.navigation = nav;
            }

        }
    }
}
