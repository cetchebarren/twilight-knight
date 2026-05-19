using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace etchebarren
{
    public class SaveSlot : MonoBehaviour
    {
        public RawImage screenshot;
        public TextMeshProUGUI nameAndLevel;
        public TextMeshProUGUI location;
        public TextMeshProUGUI playtime;
        public TextMeshProUGUI dateAndTime;
        public TextMeshProUGUI saveType;

        public Color autosaveTextColor;
        public Color manualSaveTextColor;

        public Image buttonImage;
        public SelectButtonFade selectButtonFade;
        public EventTrigger eventTrigger;

        public GameObject buttonGameObject; // used to select button in SaveMenu SelectFirstSaveSlot()
        public bool newSave = false;

        [Header("Do Not Set Manually - Set by Save Slot instantiation")]
        public string filepath;
        private bool load = true;

        public void SetNameAndLevel(string s)
        {
            nameAndLevel.text = s;
        }

        public void SetLocation(string s)
        {
            location.text = s;
        }

        public void SetPlaytime(string s)
        {
            playtime.text = s;
        }

        public void SetDateAndTime(string s)
        {
            dateAndTime.text = s;
        }

        public void SetSaveType(string s)
        {
            saveType.text = s;

            if(s == "Autosave")
            {
                saveType.color = autosaveTextColor;
            }
            else
            {
                saveType.color = manualSaveTextColor;
            }
        }

        public void SetScreenshot(Texture2D texture)
        {
            screenshot.texture = texture;
        }

        public void SetFilepath(string s)
        {
            filepath = s;
        }

        /// <summary>
        /// When the button is selected, this event is called from the selected event assigned in the inspector and the viewsport autoscrolls
        /// </summary>
        public void AdjustAutoscroll()
        {
            if(SaveMenu.instance != null)
            {
                SaveMenu.instance.saveMenuAutoscroll.ScrollToSelectedElement();
            }
        }

        /// <summary>
        /// This is called when a save slot is selected for overwriting a current save OR loading a save file.
        /// Attached to the save slot prefab's button on click event.
        /// </summary>
        public void OpenConfirmationWindow()
        {
            // We need to check if saving is already in progress before allowing this
            if (!SaveMenu.instance.mainMenu)
            {
                if (SaveMenu.instance.saveManager.saving != null)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Please wait until saving is complete.", true); //true prevents duplicate messages for 3 seconds
                    return;
                }
            }

            SetLastSelectedGameObject();
            SaveMenu.instance.selectedSaveSlot = this;
            SaveMenu.instance.OpenConfirmationWindow();
        }

        /// <summary>
        /// This is called when the button for a new save file is clicked in the menu;
        /// Attached to the NEW save slot prefab's button on click event.
        /// </summary>
        public void NewSave()
        {
            ScenePersistentPlayerObject.instance.saveManager.NewSave();
        }

        public void LoadThisSlot()
        {
            MainMenuManager.instance.LoadSave(filepath);
        }

        public void OverwriteThisSlot()
        {
            Debug.Log("Overwritten!");
        }

        public void SetLastSelectedGameObject()
        {
            // Save this as the last selected gameobject in the event system
            if(UIChangeSelectedButton.instance != null)
            {
                UIChangeSelectedButton.instance.RememberLastButton();
            }           
        }
    }
}
