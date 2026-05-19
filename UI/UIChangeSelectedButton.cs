using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace etchebarren
{
    public class UIChangeSelectedButton : MonoBehaviour
    {
        public static UIChangeSelectedButton instance;

        public OptionsManager optionsManager;
        public ControllerUIManager controllerUIManager;

        [Header("Last Selected Button")]
        [SerializeField] GameObject lastSelectedButton;

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

        public void ChangeSelectedButtonTo(GameObject _newSelectedButton, bool setRegardlessOfControls=false)
        {
            if (!EventSystem.current.alreadySelecting)
            {
                bool controller;
                // In Game
                
                if(controllerUIManager != null)
                {
                    controller = controllerUIManager.isUsingController();
                }
                else
                {
                    controller = Gamepad.all.Count > 0;

                    int controllerType = 0;
                    if (PlayerPrefs.HasKey("controllerType"))
                    {
                        controllerType = PlayerPrefs.GetInt("controllerType");
                    }

                    if (controllerType == 1) controller = true;
                    else if (controllerType == 2) controller = false;
                }


                if (controller || setRegardlessOfControls)
                {
                    if(_newSelectedButton != null)
                    {
                        EventSystem.current.SetSelectedGameObject(_newSelectedButton);
                    }
                    else
                    {
                        Debug.Log("Avoided error from null selected button");
                    }
                }
                if(PlayerMenuManager.instance != null)
                {
                    //This setting is to check if level up after closing menus (spell, ring)
                    PlayerMenuManager.instance.CheckAndSetLevelUpNotification();
                }
            }
            else
            {
                Debug.Log("Cannot select " + _newSelectedButton.name + " because EventSystem is already selecting a gameobject at this time");
            }
        }

        public void ChangeSelectedButtonToFirstChildOf(GameObject _parent)
        {
            //This Actually grabs the child of the child... Finger Section -> Ring Slot -> Button
            ChangeSelectedButtonTo(_parent.transform.GetChild(0).transform.GetChild(0).gameObject);
        }

        public void OverrideChangeSelectedButtonTo(GameObject newSelectedButton, bool overrideControllerParameter = false)
        {
            if (!EventSystem.current.alreadySelecting)
            {
                if (Gamepad.all.Count > 0 || overrideControllerParameter)
                {
                    EventSystem.current.SetSelectedGameObject(newSelectedButton);
                }
                if (PlayerMenuManager.instance != null)
                {
                    PlayerMenuManager.instance.CheckAndSetLevelUpNotification();
                }
            }
        }

        public void RememberLastButton()
        {
            if (Gamepad.all.Count > 0)
            {
                lastSelectedButton = EventSystem.current.currentSelectedGameObject;
            }
        }

        public void ReselectLastSelectedButton()
        {
            if (Gamepad.all.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedButton);
            }
        }
    }
}
