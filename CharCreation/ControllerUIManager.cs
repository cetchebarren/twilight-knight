using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace etchebarren
{
    public class ControllerUIManager : MonoBehaviour
    {
        public static ControllerUIManager instance;

        private CameraHandler cameraHandler;

        public bool controllerTypePlaystation = false;
        public OptionsManager optionsManager;
        public GameObject controlsHUD;

        public float rectTransformAdditionalWidth = 40f;
        public float rectTransformAdditionalHeight = 10f;

        [Header("CONTROLLER-ONLY ELEMENTS")]
        public GameObject[] controller_UI_elements;
        public GameObject controllerLevelUpButtonDisplay;

        [Header("KEYBOARDMOUSE-ONLY ELEMENTS")]
        public GameObject[] keyboardAndMouse_UI_elements;
        public Image levelUpButton;

        [Header("ICON REFERENCES")]
        public Image[] selectIcons;
        public Image[] backIcons;
        public Image[] y_triangleIcons;
        public Image[] leftBumperIcons;
        public Image[] rightBumperIcons;
        public Image[] leftTriggerIcons;
        public Image[] rightTriggerIcons;
        public Image[] rightStickPressIcons;
        public Image[] view_touchpadIcons;
        public Image[] start_optionsIcons;

        [Header("Canvas UI GameObject References")]
        public GameObject selectElement;
        public TextMeshProUGUI selectText;

        public GameObject backElement;
        public TextMeshProUGUI backText;

        public GameObject leftTriggerElement;
        public TextMeshProUGUI leftTriggerText;

        public GameObject rightTriggerElement;
        public TextMeshProUGUI rightTriggerText;

        public GameObject touchpadElement;
        public TextMeshProUGUI touchpadText;

        public GameObject optionsElement;
        public TextMeshProUGUI optionsText;

        public GameObject scrollWheelElement;
        public TextMeshProUGUI scrollWheelText;

        public GameObject rightStickElement;
        public TextMeshProUGUI rightStickText;

        [Header("Interact/Mount GameObjects")]
        public GameObject mountInput_PS;
        public GameObject mountInput_PC;
        public GameObject mountInput_XBOX;

        [Header("GamePad/Xbox Button Icons")]
        //BUTTONS:
        public Sprite A;
        public Sprite B;
        public Sprite Y;
        public Sprite xboxX;
        //SHOULDERS|TRIGGERS
        public Sprite LB;
        public Sprite RB;
        public Sprite LT;
        public Sprite RT;
        //STICKS
        public Sprite RS;
        //OPTIONS BUTTONS
        public Sprite start;
        public Sprite view;

        [Header("PlayStation Button Icons")]
        //BUTTONS:
        public Sprite X;
        public Sprite O;
        public Sprite triangle;
        public Sprite square;
        //SHOULDERS|TRIGGERS
        public Sprite L1;
        public Sprite R1;
        public Sprite L2;
        public Sprite R2;
        //STICKS
        public Sprite R3;
        //OPTIONS BUTTONS
        public Sprite options;
        public Sprite touchpad;

        [Header("PC Icons")]
        public Sprite computerKey;
        public Sprite leftClick;
        public Sprite rightClick;
        public Sprite escape;
        public Sprite scroll;
        public Sprite M_key;
        public Sprite F_key;

        [Header("Controller State Values")]
        private float lastInputTime = 0f;
        private float timeout = 2f;
        private bool controllerAlive = false;

        [Header("Debug & Test Values")]
        public bool displayGamepadInfoOnStart = true;
        public int gamepadCount = 0;


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

        private void Start()
        {
            cameraHandler = CameraHandler.instance;

            InputHandler.instance.usingController = isUsingController();

            ChangeControllerButtonIcons();

            SetUIBasedOnInputType();

            cameraHandler.ConfigureSensitivityBasedOnInput();

            optionsManager.SetImageTransparencies();

            // Debugging:

            gamepadCount = Gamepad.all.Count;

            if (displayGamepadInfoOnStart)
            {
                Debug.Log("Detected gamepads on game start: ");
                DebugLogGamepads();
            }
        }

        private void OnEnable()
        {
            // Subscribe to controller connection and disconnection events
            Debug.Log($"START: controller connected: {isControllerConnected()}, using controller? {isUsingController()}");
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDisable()
        {
            // Unsubscribe from events to prevent memory leaks
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            InputHandler.instance.usingController = isUsingController();

            SetUIBasedOnInputType();

            cameraHandler.ConfigureSensitivityBasedOnInput();

            gamepadCount = Gamepad.all.Count;

            // Check if a controller was added or removed
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
            {             
                Debug.Log($"controller {change}: " + Gamepad.all.Count + " controller(s) detected");
                //TextNotificationsManager.instance.NewTextNotifaction("Controller connected", false);
                optionsManager.EnableGamepads(isUsingController());
                DebugLogGamepads();
            }
            else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected)
            {
                Debug.Log($"controller {change}: " + Gamepad.all.Count + " controller(s) detected");
                TextNotificationsManager.instance.NewTextNotifaction("Controller disconnected", false);
                optionsManager.EnableGamepads(isUsingController());
                DebugLogGamepads();
            }
        }

        private void DebugLogGamepads()
        {
            if (Gamepad.all.Count > 0)
            {
                int count = 0;
                foreach (var gamepad in Gamepad.all)
                {
                    count++;
                    Debug.Log($"Gamepad {count}: {gamepad.displayName}, ID: {gamepad.deviceId}, Desc: {gamepad.description}, Added: {gamepad.added}, Enabled: {gamepad.enabled}");
                }
            }
        }

        // Should not be used in most cases, as this ingores player's settings
        private bool isControllerConnected()
        {
            return (Gamepad.all.Count > 0);
        }


        // Standard function to use for checking whether to use controller or not
        public bool isUsingController()
        {
            bool usingController = isControllerConnected();
            if (optionsManager.inputType == 1) usingController = true;
            else if (optionsManager.inputType == 2) usingController = false;
            InputHandler.instance.usingController = usingController;
            return usingController;
        }

        public void ChangeControllerButtonIcons()
        {
            //BUTTONS:
            Sprite select;
            Sprite back;
            Sprite y_triangle;
            //SHOULDERS|TRIGGERS
            Sprite leftBumper;
            Sprite rightBumper;
            Sprite leftTrigger;
            Sprite rightTrigger;
            //STICKS
            Sprite rightStickPress;
            //OPTIONS
            Sprite start_options;
            Sprite view_touchpad;

            bool controller = isUsingController();

            if (controller)
            {
                if (controllerTypePlaystation)
                {
                    //BUTTONS:
                    select = X;
                    back = O;
                    y_triangle = triangle;
                    //SHOULDERS|TRIGGERS
                    leftBumper = L1;
                    rightBumper = R1;
                    leftTrigger = L2;
                    rightTrigger = R2;
                    //STICKS
                    rightStickPress = R3;
                    //OPTIONS
                    start_options = start;
                    view_touchpad = touchpad;
                }
                else
                {
                    //BUTTONS:
                    select = A;
                    back = B;
                    y_triangle = Y;
                    //SHOULDERS|TRIGGERS
                    leftBumper = LB;
                    rightBumper = RB;
                    leftTrigger = LT;
                    rightTrigger = RT;
                    //STICKS
                    rightStickPress = RS;
                    //OPTIONS
                    start_options = start;
                    view_touchpad = view;
                }
            }
            else //PC:
            {
                //BUTTONS:
                select = leftClick;
                back = escape;
                y_triangle = triangle;
                //SHOULDERS|TRIGGERS
                leftBumper = L1;
                rightBumper = R1;
                leftTrigger = scroll;
                rightTrigger = scroll;
                //STICKS
                rightStickPress = rightClick;
                //OPTIONS
                start_options = start;
                view_touchpad = M_key;
            }

            SetIconSprites(selectIcons, select);
            SetIconSprites(backIcons, back);
            SetIconSprites(y_triangleIcons, y_triangle);
            SetIconSprites(leftBumperIcons, leftBumper);
            SetIconSprites(rightBumperIcons, rightBumper);
            SetIconSprites(leftTriggerIcons, leftTrigger);
            SetIconSprites(rightTriggerIcons, rightTrigger);
            SetIconSprites(rightStickPressIcons, rightStickPress);
            SetIconSprites(view_touchpadIcons, view_touchpad);
            SetIconSprites(start_optionsIcons, start_options);
        }

        private void SetIconSprites(Image[] icons, Sprite sprite)
        {
            foreach (Image icon in icons)
            {
                icon.sprite = sprite;
            }
        }

        public void SetUIBasedOnInputType()
        {
            bool controller = isUsingController();

            foreach (GameObject elem in controller_UI_elements)
            {
                if(elem != null)
                    elem.SetActive(controller);
            }

            foreach (GameObject elem in keyboardAndMouse_UI_elements)
            {
                if (elem != null)
                    elem.SetActive(!controller);
            }          
            SpellsHUDManager.instance.UpdateSpellHUD();
            InteractPrompt.instance.SetInteractIcon(controller);
            SetMountIcon(controller);
            ChangeControllerButtonIcons();
        }

        #region X / A Button
        public void SetSelectText(string input)
        {
            selectText.text = input;
            StartCoroutine(ResizeRectTransformToFitText(selectText));
            SelectTextActive(true);
        }

        public void SetSelectTextColor(Color color)
        {
            selectText.color = color;
        }

        public void SelectTextActive(bool input)
        {
            selectElement.SetActive(input);
        }
        #endregion

        #region Circle / B Button
        public void SetBackText(string input)
        {
            backText.text = input;
            StartCoroutine(ResizeRectTransformToFitText(backText));
            BackTextActive(true);
        }

        public void BackTextActive(bool input)
        {
            backElement.SetActive(input);
        }
        #endregion

        #region R3 / RS / Right Click Button (for Map)
        public void SetRightStickText(string input)
        {
            rightStickText.text = input;
            StartCoroutine(ResizeRectTransformToFitText(rightStickText));
            RightStickTextActive(true);
        }

        public void RightStickTextActive(bool input)
        {
            rightStickElement.SetActive(input);
        }
        #endregion

        #region Triggers
        public void SetLeftTriggerText(string input)
        {
            leftTriggerText.text = input;
            StartCoroutine(ResizeRectTransformToFitText(leftTriggerText, true));
            LeftTriggerTextActive(true);
        }

        public void SetRightTriggerText(string input)
        {
            rightTriggerText.text = input;
            StartCoroutine(ResizeRectTransformToFitText(rightTriggerText));
            RightTriggerTextActive(true);
        }

        public void LeftTriggerTextActive(bool input)
        {
            leftTriggerElement.SetActive(input);
        }


        public void RightTriggerTextActive(bool input)
        {
            rightTriggerElement.SetActive(input);
        }
        #endregion

        #region Touchpad / Select (Xbox)
        public void SetTouchpadText(string input)
        {
            touchpadText.text = input;
            StartCoroutine(ResizeRectTransformToFitText(touchpadText));
            TouchpadTextActive(true);
        }

        public void TouchpadTextActive(bool input)
        {
            touchpadElement.SetActive(input);
        }
        #endregion

        #region Options / Start (Xbox)
        public void SetOptionsText(string input)
        {
            optionsText.text = input;
            StartCoroutine(ResizeRectTransformToFitText(optionsText));
            OptionsTextActive(true);
        }

        public void OptionsTextActive(bool input)
        {
           optionsElement.SetActive(input);
        }
        #endregion

        #region Scroll Wheel (PC)
        public void SetScrollWheelText(string input)
        {
            scrollWheelText.text = input;
            StartCoroutine(ResizeRectTransformToFitText(scrollWheelText));
            ScrollWheelTextActive(true);
        }

        public void ScrollWheelTextActive(bool input)
        {
            scrollWheelElement.SetActive(input);
        }
        #endregion

        private IEnumerator ResizeRectTransformToFitText(TextMeshProUGUI text, bool delay=false)
        {
            if (delay)
            {
                yield return null;
            }

            if (text != null)
            {
                controlsHUD.SetActive(true);

                // Get the preferred width of the TextMeshProUGUI element based on its content
                float preferredWidth = text.preferredWidth;

                // Get the RectTransform component of the same GameObject
                RectTransform rectTransform = text.gameObject.transform.GetComponent<RectTransform>();

                // Set the width of the RectTransform to fit the text
                rectTransform.sizeDelta = new Vector2(preferredWidth, rectTransform.sizeDelta.y);

                // Force an immediate layout rebuild
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

                // Get the RectTransform component of the parent container
                RectTransform parentRectTransform = text.transform.parent.GetComponent<RectTransform>();

                if (parentRectTransform.gameObject.name == "Left Trigger")
                {
                    //Debug.Log("L2 Rect Size Before: " + parentRectTransform.sizeDelta);
                }

                //Debug.Log(parentRectTransform.gameObject.name);

                // Set the width of the parent RectTransform to fit the text and icon
                parentRectTransform.sizeDelta = new Vector2(preferredWidth + rectTransformAdditionalWidth, rectTransform.sizeDelta.y + rectTransformAdditionalHeight); //rectTransformAdditionalWidth is from the button icon

                // Force an immediate layout rebuild
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);


                if (parentRectTransform.gameObject.name == "Left Trigger")
                {
                    //Debug.Log("L2 Rect Size After: " + parentRectTransform.sizeDelta);
                }
            }

            yield return null;
        }

        public void SetPlayStationController(bool isPlayStationController)
        {
            controllerTypePlaystation = isPlayStationController;
            ChangeControllerButtonIcons();
            optionsManager.SetImageTransparencies();
        }

        public void SetMountIcon(bool controller)
        {
            mountInput_PS.SetActive(false);
            mountInput_PC.SetActive(false);
            mountInput_XBOX.SetActive(false);

            if (controller)
            {
                if(controllerTypePlaystation)
                {
                    mountInput_PS.SetActive(true);
                }
                else
                {
                    mountInput_XBOX.SetActive(true);
                }
            }
            else
            {
                mountInput_PC.SetActive(true);
            }
        }

        public void SetAllInactive()
        {
            SelectTextActive(false);
            BackTextActive(false);
            OptionsTextActive(false);
            LeftTriggerTextActive(false);
            RightTriggerTextActive(false);
            RightStickTextActive(false);
            ScrollWheelTextActive(false);
            TouchpadTextActive(false);
            Canvas.ForceUpdateCanvases();
        }
    }
}
