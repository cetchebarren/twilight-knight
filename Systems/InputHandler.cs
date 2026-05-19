using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace etchebarren
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler instance;

        [Header("References")]
        public PlayerMenuManager playerMenuManager;
        public SpellsHUDManager spellsHUDManager;
        public ShopMenuManager shopMenuManager;
        public DialogueMenu dialogueMenu;
        public OptionsManager optionsManager;
        public ClimbManager climbManager;
        public InteractPrompt interactPrompt;
        public PlayerControls inputActions;
        public PlayerStats playerStats;
        public PlayerLocomotion playerLocomotion;
        public CameraHandler cameraHandler;
        public NoteManager noteManager;
        public ControllerUIManager controllerUIManager;
        public QuestManager questManager;
        public SaveManager saveManager;
        public HelpMenu helpMenu;

        [Header("Flags")]
        public bool canRotate = true;
        public bool canMove = true;
        public bool isPerformingAction = false;
        public bool isJumping = false;
        public bool isGrounded = true;
        public bool isBlocking = false;
        public bool playerMenuOpen = false;
        public bool mapMenuOpen = false;
        public bool usingController = false;
        public bool lockOnTimerReady = true;
        public bool isAttacking = false;
        public bool canQueueAttack = false;
        public bool comboFlag;
        public bool canMountLeft = false;
        public bool canMountRight = false;
        public bool sheatheReady = true;
        public float testDurationSheathe = 1.0f;

        [SerializeField] private bool _LockedOn = false;
        public bool lockedOn
        {
            get { return _LockedOn; }
            set
            {
                if (_LockedOn != value)
                {
                    _LockedOn = value;
                    //Debug.Log("Locked On Changed");
                    playerLocomotion.SetAnimatorLockedOnBool(_LockedOn);
                }
            }
        }

        [Header("INPUT TYPE FLAGS")]
        public bool keyboardUsedForInput = false;

        [Header("MOVEMENT VALUES")]
        public float horizontal;
        public float vertical;
        public float moveAmount;

        [Header("CAMERA MOVEMENT VALUES")]
        public float mouseX;
        public float mouseY;
        public float lockOnSensitivity = 0.95f;
        public float lockOnDelay = 0.5f;

        [Header("INPUTS")]
        Vector2 movementInput;
        Vector2 cameraInput;
        [SerializeField] bool dodgeInput = false;
        [SerializeField] bool jumpInput = false;
        [SerializeField] bool sprintInput = false;
        [SerializeField] bool crouchInput = false;
        public bool blockInput = false;
        public bool lockOnInput = false;
        [SerializeField] bool lockOnLeftInput = false;
        [SerializeField] bool lockOnRightInput = false;
        [SerializeField] bool playerMenuInput = false;
        [SerializeField] bool mapMenuInput = false;
        public bool attackInput = false;
        public bool heavyAttackInput = false;
        public bool castSpellInput = false;
        [SerializeField] bool useItemInput = false;
        [SerializeField] bool switchItemLeftInput = false;
        [SerializeField] bool switchItemRightInput = false;
        public bool sheatheWeaponInput = false;
        public bool callHorseInput = false;
        public bool trackQuest = false;

        private Coroutine callHorseCoroutine;
        public bool testAnimationInput = false;

        [Header("UI Controller Inputs")]
        ///[SerializeField] bool openSpells = false;
        [SerializeField] bool switchSpellsHUDInput = false;
        [SerializeField] private bool _OpenSpells = false;

        public bool openSpells
        {
            get { return _OpenSpells; }
            set
            {
                if (_OpenSpells != value)
                {
                    _OpenSpells = value;
                    if (!playerMenuOpen && !shopWindow.activeSelf && !inMenu && !mapMenuOpen)
                    {
                        Debug.Log("Spell Menu Active triggered");
                        spellsHUDManager.SpellMenuActive(_OpenSpells, usingController);
                    }
                }
            }
        }
        [SerializeField] bool spellNorth = false;
        [SerializeField] bool spellWest = false;
        [SerializeField] bool spellEast = false;
        [SerializeField] bool spellSouth = false;
        [SerializeField] bool spell1 = false;
        [SerializeField] bool spell2 = false;
        [SerializeField] bool spell3 = false;
        [SerializeField] bool spell4 = false;
        [SerializeField] bool spell5 = false;
        [SerializeField] bool spell6 = false;
        [SerializeField] bool spell7 = false;
        [SerializeField] bool spell8 = false;
        public bool interactInput = false;

        [Header("UI Controller Inputs")]
        [SerializeField] bool menuLeftShoulder = false;
        [SerializeField] bool menuRightShoulder = false;
        [SerializeField] bool menuLeftTrigger = false;
        [SerializeField] bool menuRightTrigger = false;
        [SerializeField] bool buttonNorthInput = false;

        [Header("UI Controller Character Create Inputs")]
        [SerializeField] bool charMenuSpinLeft = false;
        [SerializeField] bool charMenuSpinRight = false;
        [SerializeField] bool charMenuZoomToggle = false;
        [SerializeField] bool charMenuRecenter = false;

        [Header("Character Create References")]
        private bool spinningFlag = false;
        public SpinManager spinManagerDefault;
        public SpinManager spinManagerConfirm;
        public ZoomCharCreate zoomDefault;
        public ZoomCharCreate zoomConfirm;
        public GameObject characterCreateCanvas;
        public bool inMenu = false;
        public Button hairColorSelect;
        public Slider redSlider_hairColor;
        public Slider greenSlider_hairColor;
        public Slider blueSlider_hairColor;
        public Button eyebrowColorSelect;
        public Slider redSlider_eyebrowColor;
        public Slider greenSlider_eyebrowColor;
        public Slider blueSlider_eyebrowColor;
        public Button facialHairColorSelect;
        public Slider redSlider_facialHairColor;
        public Slider greenSlider_facialHairColor;
        public Slider blueSlider_facialHairColor;

        [Header("UI Buttons and References")]
        public GameObject swapRingWindow;
        public Button closeSwapRingWindowButton;
        public GameObject swapSpellWindow;
        public Button closeSwapSpellWindowButton;
        //Char Create Windows
        public GameObject genderWindow;
        public GameObject hairWindow;
        public GameObject eyeColorWindow;
        public GameObject skinToneWindow;
        public GameObject detailsWindow;
        public GameObject finishWindow;
        public GameObject genderButton;
        public GameObject hairButton;
        public GameObject eyeColorButton;
        public GameObject skinToneButton;
        public GameObject detailsButton;
        public Button confirmNoButton;
        public GameObject levelUpMenuWindow;
        public GameObject statsBox;
        public LevelUpMenuManager levelUpManager;
        public GameObject confimLevelUpButton;
        // Save Menu window / buttons
        public GameObject saveMenu;
        public GameObject saveMenuConfirmationWindow;
        public GameObject saveGameButton;
        public GameObject loadGameButton;
        // Stables Shop
        public Button confirmStables;
        // Misc Menu Windows
        public GameObject optionsWindow;
        public GameObject questsWindow;
        // Interact Prompt
        public GameObject switchActionContainer;
        // Note UI
        public GameObject noteUI;
        // Map Menu
        public SpriteRenderer virtualCursor;
        // Controls 
        public GameObject viewControlsWindow;
        // Map Confirm Window
        public GameObject mapConfirmWindow;
        public Button mapConfirmCancelButton;
        public Image trackQuestFill;

        [Header("Combat References")]
        public BlockingCollider blockingCollider;
        public TargetIconPosition targetIconPosition;

        [Header("Mount References")]
        public MountLocomotion mountLocomotion;
        public GameObject horseAIVer;

        [Header("Shop References")]
        public GameObject quantityWindow;
        public GameObject purchaseWindow;
        public GameObject cancelButtonPurchase;
        public GameObject cancelButtonQuantity;
        public GameObject shopWindow;

        [Header("Mouse Movement Target Switching")]
        public bool mouseSwitchesTargets = true;
        public float switchTargetMouseSensitiity = 8f;
        private struct MouseSample {public float delta; public float time;}
        private List<MouseSample> mouseHistory = new List<MouseSample>();
        public float gestureWindow = 0.12f; // 120 ms

        public bool inCutscene = false;
        public bool pausedForNotice = false;
        public bool tutorialPopupActive = false;

        private float trackQuestHoldDuration = 1.5f;
        private float trackQuestCurrentHoldTime = 0f;

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

            inMenu = characterCreateCanvas.activeSelf || playerMenuOpen;
        }

        private void Update()
        {
            if (inCutscene || pausedForNotice) return;
           
            HandleSprinting();
            HandleCrouchInput();
            HandleLockOnInput();
            HandleBlocking();
            HandlePlayerMenuInput();
            HandleMapMenuInput();
            HandleHeavyAttackInput();
            HandleAttackInput();
            HandleCastSpellInput();
            HandleItemInput();
            HandleInteractInput();
            HandleSwitchSpellsHUDInput();
            HandleJumpInput();
            HandleSheatheWeapon();
            HandleCallHorse();
            HandleTestAnimationInput();
            HandleDodgeInput();

            if (usingController)
            {
                HandlePlayerMenuShouldersAndTriggers();
                HandleButtonNorthInput();

                if (characterCreateCanvas != null && characterCreateCanvas.activeSelf)
                {
                    HandleCharCreateControllerInputs();
                }
            }

        }

        private void LateUpdate()
        {
            if (inCutscene || pausedForNotice || playerMenuOpen || mapMenuOpen || tutorialPopupActive) return;

            cameraHandler.HandleAllCameraActions();
        }

        public void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.PlayerMovement.Movement.performed += inputActions => movementInput = inputActions.ReadValue<Vector2>();
                inputActions.PlayerCamera.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
                inputActions.PlayerActions.Dodge.performed += i => dodgeInput = true;

                inputActions.PlayerActions.Jump.performed += i => jumpInput = true;

                inputActions.PlayerActions.Sprint.performed += i => sprintInput = true;
                inputActions.PlayerActions.Sprint.canceled += i => sprintInput = false;

                inputActions.PlayerActions.Crouch.performed += i => crouchInput = true;
                inputActions.PlayerActions.Crouch.canceled += i => crouchInput = false;

                inputActions.PlayerActions.Block.performed += i => blockInput = true;
                inputActions.PlayerActions.Block.canceled += i => blockInput = false;

                inputActions.PlayerActions.Attack.performed += i => attackInput = true;
                inputActions.PlayerActions.Attack.canceled += i => attackInput = false;

                inputActions.PlayerActions.HeavyAttack.performed += i => heavyAttackInput = true;
                inputActions.PlayerActions.HeavyAttack.canceled += i => heavyAttackInput = false;

                inputActions.PlayerCamera.LockOn.performed += i => lockOnInput = true;
                inputActions.PlayerCamera.LockOnTargetLeft.performed += i => lockOnLeftInput = true;
                inputActions.PlayerCamera.LockOnTargetRight.performed += i => lockOnRightInput = true;

                //inputActions.PlayerActions.PlayerMenu.performed += i => playerMenuInput = true; // OLD
                inputActions.PlayerActions.PlayerMenu.performed += OnPlayerMenu; // NEW, updated to identify device used

                //inputActions.PlayerActions.MapMenu.performed += i => mapMenuInput = true; // OLD
                inputActions.PlayerActions.MapMenu.performed += OnMapMenu; // NEW, updated to identify device used

                //UI Controller Here
                inputActions.UIControls.LeftShoulder.performed += i => menuLeftShoulder = true;
                inputActions.UIControls.RightShoulder.performed += i => menuRightShoulder = true;
                inputActions.UIControls.LeftTrigger.performed += i => menuLeftTrigger = true;
                inputActions.UIControls.RightTrigger.performed += i => menuRightTrigger = true;

                inputActions.UIControls.ButtonNorth.performed += inputActions => buttonNorthInput = true;

                //UI Controller Exclusive to Character Creation
                inputActions.UIControlsCharCreate.RotateLeft.performed += i => charMenuSpinLeft = true;
                inputActions.UIControlsCharCreate.RotateRight.performed += i => charMenuSpinRight = true;
                inputActions.UIControlsCharCreate.RotateLeft.canceled += i => charMenuSpinLeft = false;
                inputActions.UIControlsCharCreate.RotateRight.canceled += i => charMenuSpinRight = false;

                inputActions.UIControlsCharCreate.Recenter.performed += i => charMenuRecenter = true;
                inputActions.UIControlsCharCreate.ZoomToggle.performed += i => charMenuZoomToggle = true;

                //Spells
                inputActions.PlayerActions.OpenSpells.performed += i => openSpells = true;
                inputActions.PlayerActions.OpenSpells.canceled += i => openSpells = false;

                inputActions.PlayerActions.SwitchSpellsHUD.performed += i => switchSpellsHUDInput = true;

                inputActions.PlayerActions.CastSpellNorth.performed += i => spellNorth = true;
                inputActions.PlayerActions.CastSpellWest.performed += i => spellWest = true;
                inputActions.PlayerActions.CastSpellEast.performed += i => spellEast = true;
                inputActions.PlayerActions.CastSpellSouth.performed += i => spellSouth = true;

                inputActions.PlayerActions.CastSpell1.performed += i => spell1 = true;
                inputActions.PlayerActions.CastSpell2.performed += i => spell2 = true;
                inputActions.PlayerActions.CastSpell3.performed += i => spell3 = true;
                inputActions.PlayerActions.CastSpell4.performed += i => spell4 = true;
                inputActions.PlayerActions.CastSpell5.performed += i => spell5 = true;
                inputActions.PlayerActions.CastSpell6.performed += i => spell6 = true;
                inputActions.PlayerActions.CastSpell7.performed += i => spell7 = true;
                inputActions.PlayerActions.CastSpell8.performed += i => spell8 = true;

                inputActions.PlayerActions.UseItem.performed += i => useItemInput = true;
                inputActions.PlayerActions.SwitchItemLeft.performed += i => switchItemLeftInput = true;
                inputActions.PlayerActions.SwitchItemRight.performed += i => switchItemRightInput = true;

                inputActions.PlayerActions.Interact.performed += OnInteract; // NEW, updated to identify device used

                inputActions.PlayerActions.SheatheWeapon.performed += i => sheatheWeaponInput = true;

                inputActions.PlayerActions.CallHorse.performed += i => callHorseInput = true;
                inputActions.PlayerActions.CallHorse.canceled += i => callHorseInput = false;

                inputActions.PlayerActions.TestAnimation.performed += i => testAnimationInput = true;
            }

            inputActions.Enable();
        }

        public void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            if (!inMenu)
                MoveInput(delta);
            else if (dialogueMenu.gameObject.activeSelf)
            {
                mouseX = cameraInput.x;
                mouseY = cameraInput.y;
            }
        }

        private void MoveInput(float delta)
        {
            float previousVertical = vertical;

            horizontal = movementInput.x;
            vertical = movementInput.y;

            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

            mouseX = cameraInput.x;
            mouseY = cameraInput.y;

            // Change Lock on Target, if controller or mouse switching is enabled
            if (lockedOn)
            {
                if (usingController)
                {
                    if (mouseX > lockOnSensitivity)
                    {
                        lockOnRightInput = true;
                    }
                    else if (mouseX < -lockOnSensitivity)
                    {
                        lockOnLeftInput = true;
                    }
                }
                else
                {
                    if (mouseSwitchesTargets)
                    {
                        // Add new sample
                        float now = Time.time;
                        mouseHistory.Add(new MouseSample { delta = mouseX, time = now });

                        // Remove old samples
                        mouseHistory.RemoveAll(s => now - s.time > gestureWindow);

                        // Sum movement inside the window
                        float total = 0f;
                        foreach (var s in mouseHistory)
                            total += s.delta;

                        if (total > switchTargetMouseSensitiity)
                        {
                            lockOnRightInput = true;
                            mouseHistory.Clear();
                        }
                        else if (total < -switchTargetMouseSensitiity)
                        {
                            lockOnLeftInput = true;
                            mouseHistory.Clear();
                        }
                    }
                }
            }
        }

        private void HandleInteractInput()
        {
            if (interactInput)
            {
                interactInput = false;

                if (playerStats.dead)
                {
                    if (interactPrompt.interactActive)
                    {
                        if (interactPrompt.interactType == "Rise, Knight")
                        {  
                            playerLocomotion.StandUp();
                            interactPrompt.RemoveMiscInteraction("Rise, Knight");
                            StartCoroutine(questManager.StartQuestAfterDelay(4, true, 11f));
                            StartCoroutine(cameraHandler.ResetCameraOffset(6f));     
                        }
                    }
                    else if (playerStats.canRespawn)
                    {
                        playerStats.canRespawn = false;
                        playerStats.Respawn();
                    }
                    return;
                }

                bool showCursor = keyboardUsedForInput;
                keyboardUsedForInput = false;

                /* Dialogue Menu is Open*/
                if (dialogueMenu.gameObject.activeSelf)
                {
                    dialogueMenu.ProgressDialogueMenu();
                    return;
                }

                if (noteUI.activeSelf)
                {
                    if (noteManager.currentReadable.canGrab)
                    {
                        noteManager.GrabNote();
                        return;
                    }
                }

                if (playerMenuOpen || inMenu || mapMenuOpen) return;

                else if (interactPrompt.interactActive)
                {
                    if (interactPrompt.interactType == "Chest")
                    {
                        if (!isPerformingAction && !isAttacking)
                        {
                            playerLocomotion.StartLooting(true);
                        }
                    }
                    if (interactPrompt.interactType == "Door")
                    {
                        if (!isPerformingAction && !isAttacking)
                        {
                            interactPrompt.currentDoor.Open();
                        }
                    }
                    else if (interactPrompt.interactType == "Item")
                    {
                        interactPrompt.PickUpItem();
                    }
                    else if (interactPrompt.interactType == "Climb")
                    {
                        if (isPerformingAction) return;

                        ClimbableEdge climbableEdge = interactPrompt.GetCurrentInteraction().climbableEdge;
                        climbManager.Climb(climbableEdge);
                        interactPrompt.RemoveInteraction(climbableEdge);
                    }
                    else if (interactPrompt.interactType == "Push")
                    {
                        if (isPerformingAction || !isGrounded || isAttacking) return;

                        PushableObject pushableObject = interactPrompt.GetCurrentInteraction().pushableObject;
                        pushableObject.SetTriggersEnabled(false);
                        playerLocomotion.StartPushingObject(pushableObject);
                        interactPrompt.RemoveInteraction(pushableObject);
                    }
                    else if (interactPrompt.interactType == "Read")
                    {
                        if (!isPerformingAction && !isAttacking)
                        {
                            noteManager.Display(interactPrompt.currentReadable);
                        }
                    }
                    else if (interactPrompt.interactType == "Stables Shop")
                    {
                        if (!MountManager.instance.mountUnlocked)
                        {
                            TextNotificationsManager.instance.NewTextNotifaction("Cannot open stables, no mount available.", true); //true prevents duplicate messages for 3 seconds
                            return;
                        }
                        interactPrompt.interactActive = false;
                        StablesShop.instance.gameObject.SetActive(true);
                        interactPrompt.interactContainer.SetActive(false);
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(StablesShop.instance.nameButton);
                        PlayerMenuManager.instance.controllerHUD.SetActive(true);
                        controllerUIManager.SetSelectText("Select");
                        controllerUIManager.SelectTextActive(true);
                        controllerUIManager.SetBackText("Leave");
                        controllerUIManager.BackTextActive(true);
                        if (showCursor)
                        {
                            Cursor.lockState = CursorLockMode.None;
                        }
                        else
                        {
                            Cursor.lockState = CursorLockMode.Locked;
                        }
                    }
                    else if (interactPrompt.interactType == "Shop")
                    {
                        interactPrompt.interactActive = false;
                        interactPrompt.interactContainer.SetActive(false);
                        inMenu = true;
                        string shopName = interactPrompt.interactions[interactPrompt.currentActionIndex].shopName;
                        if (shopName != "") shopMenuManager.OpenShopMenu(shopName);
                        if (showCursor)
                        {
                            Cursor.lockState = CursorLockMode.None;
                        }
                        else
                        {
                            Cursor.lockState = CursorLockMode.Locked;
                        }
                    }
                    else if (interactPrompt.interactType == "Dialogue")
                    {
                        interactPrompt.interactActive = false;
                        interactPrompt.interactContainer.SetActive(false);
                        inMenu = true;
                        dialogueMenu.OpenDialogueMenu();
                    }
                    else if (interactPrompt.interactType == "Activate Waypoint")
                    {
                        interactPrompt.interactActive = false;
                        interactPrompt.interactContainer.SetActive(false);
                        interactPrompt.currentTeleportWaypoint.UnlockTeleportWaypoint();
                    }
                    else if (interactPrompt.interactType == "Scene Exit")
                    {
                        if (saveManager.saving != null)
                        {
                            TextNotificationsManager.instance.NewTextNotifaction("Cannot leave while saving. Please wait.");
                            return;
                        }
                        interactPrompt.interactActive = false;
                        interactPrompt.interactContainer.SetActive(false);
                        Debug.Log("Scene Exit Triggered");
                        interactPrompt.RemoveInteraction(interactPrompt.currentSceneExit);
                        WorldStateManager.instance.ChangeScene(WorldStateManager.instance.newSceneName);
                    }
                    else if (interactPrompt.interactType == "InteractEvent")
                    {
                        if (interactPrompt.currentInteractEvent.canInteract)
                        {                       
                            Debug.Log("Interact Event Triggered");
                            interactPrompt.currentInteractEvent.InvokeEvent();
                            // Disable Prompt
                            if (interactPrompt.currentInteractEvent.disablePromptOnInteraction)
                            {
                                interactPrompt.interactActive = false;
                                interactPrompt.interactContainer.SetActive(false);
                                interactPrompt.RemoveEventInteraction(interactPrompt.currentInteractEvent);
                            }
                            else
                            {
                                // Refresh prompt text (may have changed)
                                interactPrompt.interactText.text = interactPrompt.currentInteractEvent.GetInteractText();
                                interactPrompt.ResizeTextBackground();
                            }
                        }                                            
                    }
                }
            }
        }

        private void HandleSwitchSpellsHUDInput()
        {
            if (switchSpellsHUDInput)
            {
                switchSpellsHUDInput = false;

                if (noteUI.activeSelf || playerStats.dead) return;

                Debug.Log("Double pressed left trigger!");

                spellsHUDManager.SwitchSpellMenu();
            }
        }

        private void HandleDodgeInput()
        {
            if (dodgeInput)
            {
                dodgeInput = false;

                if (playerStats.dead)
                {
                    return;
                }

                if (tutorialPopupActive)
                {
                    helpMenu.Close();
                    return;
                }

                if (mapMenuOpen)
                {
                    if (mapConfirmWindow.activeSelf)
                    {
                        mapConfirmCancelButton.onClick.Invoke();
                    }
                    else
                    {
                        playerMenuManager.CloseMapMenu();
                        if (!playerMenuOpen) Cursor.lockState = CursorLockMode.Locked;
                    }
                    return;
                }

                if (noteUI.activeSelf)
                {
                    noteManager.Close();
                    return;
                }

                if (!playerMenuOpen && !inMenu && !openSpells && !playerLocomotion.onMount)
                {
                    Debug.Log("Dodge/Back Input 1");
                    // Perform a dodge
                    blockingCollider.SetBlockingCollider(false);
                    playerStats.SetStaminaRegenScale(playerStats.defaultStaminaRegenAmount);
                    isBlocking = false; //added to try to make blocking resume after attack

                    playerLocomotion.AttemptToPerformDodge();
                }
                else if (swapRingWindow.activeSelf)
                {
                    Debug.Log("Dodge/Back Input 2");
                    // Close swap ring window and change selected button
                    closeSwapRingWindowButton.onClick.Invoke();
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(playerMenuManager.lastSelectedRingBeforeSwap);
                }
                else if (swapSpellWindow.activeSelf)
                {
                    Debug.Log("Dodge/Back Input 3");
                    // Close swap ring window and change selected button
                    closeSwapSpellWindowButton.onClick.Invoke();
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(playerMenuManager.lastSelectedSpellBeforeSwap);
                }
                else if (shopWindow.activeSelf)
                {
                    Debug.Log("Dodge/Back Input 4");

                    if (purchaseWindow.activeSelf)
                    {
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(cancelButtonPurchase);
                    }
                    else if (quantityWindow.activeSelf)
                    {
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(cancelButtonQuantity);
                    }
                    else
                    {
                        shopMenuManager.CloseShopMenu();
                    }
                }
                else if (characterCreateCanvas.activeSelf)
                {
                    if (genderWindow.activeSelf)
                    {
                        Debug.Log("Dodge/Back Input 5");
                        // Close gender window and change selected button
                        genderWindow.SetActive(false);
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(genderButton);
                    }
                    else if (hairWindow.activeSelf)
                    {
                        Debug.Log("Dodge/Back Input ");
                        // Close hair window and change selected button
                        if (hairColorSelect.interactable == false) // Means player is choosing hair color
                        {
                            redSlider_hairColor.interactable = false;
                            greenSlider_hairColor.interactable = false;
                            blueSlider_hairColor.interactable = false;
                            hairColorSelect.interactable = true;
                            hairColorSelect.Select();
                        }
                        else if (eyebrowColorSelect.interactable == false)
                        {
                            redSlider_eyebrowColor.interactable = false;
                            greenSlider_eyebrowColor.interactable = false;
                            blueSlider_eyebrowColor.interactable = false;
                            eyebrowColorSelect.interactable = true;
                            eyebrowColorSelect.Select();
                        }
                        else if (facialHairColorSelect.interactable == false)
                        {
                            redSlider_facialHairColor.interactable = false;
                            greenSlider_facialHairColor.interactable = false;
                            blueSlider_facialHairColor.interactable = false;
                            facialHairColorSelect.interactable = true;
                            facialHairColorSelect.Select();
                        }
                        else
                        {
                            hairWindow.SetActive(false);
                            UIChangeSelectedButton.instance.ChangeSelectedButtonTo(hairButton);
                        }
                    }
                    else if (eyeColorWindow.activeSelf)
                    {
                        Debug.Log("Dodge/Back Input 7");
                        // Close eye color window and change selected button
                        eyeColorWindow.SetActive(false);
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(eyeColorButton);
                    }
                    else if (skinToneWindow.activeSelf)
                    {
                        Debug.Log("Dodge/Back Input 8");
                        // Close skin tone window and change selected button
                        skinToneWindow.SetActive(false);
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(skinToneButton);
                    }
                    else if (detailsWindow.activeSelf)
                    {
                        Debug.Log("Dodge/Back Input 9");
                        // Close skin tone window and change selected button
                        detailsWindow.SetActive(false);
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(detailsButton);
                        //detailsWindow.GetComponent<MiscCharOptions>().ResetTextColors();
                    }
                    else if (finishWindow.activeSelf)
                    {
                        Debug.Log("Dodge/Back Input 10");
                        // Handle finish window action
                        confirmNoButton.onClick.Invoke();
                    }
                }
                else if (playerMenuOpen && saveMenu.activeSelf)
                {
                    Debug.Log("Dodge/Back Input 11");

                    if (saveMenuConfirmationWindow.activeSelf)
                    {
                        //saveMenu.GetComponent<SaveMenu>().SelectFirstSaveSlot();
                        UIChangeSelectedButton.instance.ReselectLastSelectedButton();
                    }
                    else if (saveMenu.GetComponent<SaveMenu>().loading)
                    {
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(loadGameButton);
                        saveMenu.SetActive(false);
                    }
                    else
                    {
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(saveGameButton);
                        saveMenu.SetActive(false);
                    }
                    saveMenuConfirmationWindow.SetActive(false);
                }
                else if (levelUpMenuWindow.activeSelf)
                {
                    Debug.Log("Dodge/Back Input 12");
                    levelUpManager.ResetValues();
                    levelUpMenuWindow.SetActive(false);
                    UIChangeSelectedButton.instance.ReselectLastSelectedButton();
                    PlayerMenuManager.instance.CheckAndSetLevelUpNotification();
                    statsBox.SetActive(true);
                }
                else if (StablesShop.instance.gameObject.activeSelf)
                {
                    Debug.Log("Dodge/Back Input 13");
                    confirmStables.onClick.Invoke();
                }
                else if (playerMenuOpen && optionsWindow.activeSelf)
                {
                    Debug.Log("Dodge/Back Input 14");
                    viewControlsWindow.SetActive(false);
                    optionsManager.BackToPreviousSelectedOption();
                }
                else if (!playerMenuOpen && !inMenu && !openSpells && playerLocomotion.onMount && !mapMenuOpen)
                {
                    Debug.Log("Dodge/Back Input 15");
                    playerLocomotion.Dismount();
                }
                else
                {
                    Debug.Log("Back/Dodge Input - Nothing Triggered");
                }
            }
        }

        private void HandleCrouchInput()
        {
            if (crouchInput)
            {
                crouchInput = false;

                if (!playerStats.dead && !playerMenuOpen && !inMenu && !mapMenuOpen && !tutorialPopupActive)
                {
                    if (playerLocomotion.onMount) return;

                    if (playerLocomotion.isSprinting)
                    {
                        playerLocomotion.AttemptToSlide();
                    }
                    else
                    {
                        playerLocomotion.AttemptToToggleCrouch();
                    }
                }
            }
        }

        private void HandleHeavyAttackInput()
        {
            if (!playerMenuOpen && !inMenu && !mapMenuOpen && !tutorialPopupActive)
            {
                // Start Charge Attack or Heavy Attack
                if (heavyAttackInput && !playerLocomotion.isHeavyAttacking)
                {
                    if (playerStats.dead)
                    {
                        heavyAttackInput = false;
                        return;
                    }

                    blockInput = false;
                    playerLocomotion.SetAnimBlockFalse();
                    if (canQueueAttack)
                    {
                        //Debug.Log("CALLED: HandleHeavyAttackQueue()");
                        comboFlag = true;
                        playerLocomotion.HandleHeavyAttackQueue();
                        comboFlag = false;
                    }
                    else
                    {
                        //Debug.Log("CALLED: HandleHeavyAttack()");
                        playerLocomotion.HandleHeavyAttack();
                    }
                }
                else if (!heavyAttackInput && playerLocomotion.isHeavyAttacking)// Stop Charge Attack
                {
                    playerLocomotion.isHeavyAttacking = false;
                }
            }
            else
            {
                heavyAttackInput = false;
            }
        }

        private void HandleAttackInput()
        {
            if (attackInput)
            {
                attackInput = false;

                if (playerStats.dead)
                {
                    if (playerStats.canRespawn)
                    {
                        playerStats.canRespawn = false;
                        playerStats.Respawn();
                    }
                    return;
                }

                if (dialogueMenu.gameObject.activeSelf)
                {
                    dialogueMenu.ProgressDialogueMenu();
                    return;
                }

                if (noteUI.activeSelf)
                {
                    if (noteManager.currentReadable.canGrab)
                    {
                        noteManager.GrabNote();
                        return;
                    }
                }

                if (!playerMenuOpen && !inMenu && !mapMenuOpen && !tutorialPopupActive)
                {
                    isBlocking = false; //added to try to make blocking resume after attack

                    blockingCollider.SetBlockingCollider(false);
                    playerStats.SetStaminaRegenScale(playerStats.defaultStaminaRegenAmount);

                    if (canQueueAttack && playerStats.currentStamina >= 0)
                    {
                        comboFlag = true;
                        playerLocomotion.HandleAttackQueue();
                        comboFlag = false;
                    }
                    else
                    {
                        if (canQueueAttack)
                            return;
                        playerLocomotion.AttemptToAttack();
                    }
                }
            }
        }

        private void HandleItemInput()
        {
            if (useItemInput)
            {
                useItemInput = false;

                if (playerMenuOpen || isPerformingAction || inMenu || mapMenuOpen || playerStats.dead || tutorialPopupActive) return;

                if (!openSpells)
                {
                    if (ConsumablesHUDManager.instance.canUseConsumable)
                    {
                        if (ConsumablesHUDManager.instance.selectedConsumable >= 0
                            && ConsumablesHUDManager.instance.selectedConsumable < PlayerInventory.instance.equippedConsumables.Count)
                        {
                            if (PlayerInventory.instance.equippedConsumables[ConsumablesHUDManager.instance.selectedConsumable] != null)
                            {
                                // valid index and non-null item.
                                Consumable selected = PlayerInventory.instance.equippedConsumables[ConsumablesHUDManager.instance.selectedConsumable];
                                playerLocomotion.AttemptToUseItem(selected);
                            }
                        }
                        else
                        {

                            //Debug.Log("No item selected");
                            TextNotificationsManager.instance.NewTextNotifaction("No item selected");
                        }
                    }
                    else
                    {
                        //Debug.Log("Consumables are on cooldown");
                        TextNotificationsManager.instance.NewTextNotifaction("Cannot use. Items on cooldown");
                    }
                }
            }

            if (switchItemLeftInput)
            {
                switchItemLeftInput = false;

                if (playerMenuOpen || inMenu || mapMenuOpen) return;

                if(interactPrompt.interactActive && switchActionContainer.activeSelf)
                {
                    interactPrompt.SwitchAction(false);
                    return;
                }

                Debug.Log("Switch to left Item");
                ConsumablesHUDManager.instance.UpdateSelectedConsumable(false);
            }

            if (switchItemRightInput)
            {
                switchItemRightInput = false;

                if (playerMenuOpen || inMenu || mapMenuOpen) return;

                if (interactPrompt.interactActive && switchActionContainer.activeSelf)
                {
                    interactPrompt.SwitchAction(true);
                    return;
                }

                Debug.Log("Switch to right Item");
                ConsumablesHUDManager.instance.UpdateSelectedConsumable(true);
            }
        }

        private void HandleCastSpellInput()
        {
            if (!playerMenuOpen && !inMenu && !mapMenuOpen && !playerStats.dead && !isPerformingAction && !tutorialPopupActive)
            {
                if (openSpells)
                {
                    bool primarySpellsHUD = true; // This is referring to which spell HUD is open (spells 0-3, or 4-7)
                    if (spellNorth)
                    {
                        spellNorth = false;
                        primarySpellsHUD = spellsHUDManager.PrimarySpellHUDisActive();
                        if (primarySpellsHUD) CastSpell(0);
                        else CastSpell(4);
                    }
                    else if (spellWest)
                    {
                        spellWest = false;
                        primarySpellsHUD = spellsHUDManager.PrimarySpellHUDisActive();
                        if (primarySpellsHUD) CastSpell(1);
                        else CastSpell(5);
                    }
                    else if (spellEast)
                    {
                        spellEast = false;
                        primarySpellsHUD = spellsHUDManager.PrimarySpellHUDisActive();
                        if (primarySpellsHUD) CastSpell(2);
                        else CastSpell(6);
                    }
                    else if (spellSouth)
                    {
                        spellSouth = false;
                        primarySpellsHUD = spellsHUDManager.PrimarySpellHUDisActive();
                        if (primarySpellsHUD) CastSpell(3);
                        else CastSpell(7);
                    }
                }
                else
                {
                    spellNorth = false;
                    spellSouth = false;
                    spellEast = false;
                    spellWest = false;
                }

                if (spell1)
                {
                    spell1 = false;
                    CastSpell(0);
                }
                else if (spell2)
                {
                    spell2 = false;
                    CastSpell(2);
                }
                else if (spell3)
                {
                    spell3 = false;
                    CastSpell(1);
                }
                else if (spell4)
                {
                    spell4 = false;
                    CastSpell(3);
                }
                else if (spell5)
                {
                    spell5 = false;
                    CastSpell(4);
                }
                else if (spell6)
                {
                    spell6 = false;
                    CastSpell(6);
                }
                else if (spell7)
                {
                    spell7 = false;
                    CastSpell(5);
                }
                else if (spell8)
                {
                    spell8 = false;
                    CastSpell(7);
                }
            }
            else
            {
                openSpells = false;
                spellNorth = false;
                spellSouth = false;
                spellEast = false;
                spellWest = false;
                spell1 = false;
                spell2 = false;
                spell3 = false;
                spell4 = false;
                spell5 = false;
                spell6 = false;
                spell7 = false;
                spell8 = false;

            }
        }

        private void CastSpell(int spellIndex)
        {
            isBlocking = false; //added to try to make blocking resume after attack

            blockingCollider.SetBlockingCollider(false);
            playerStats.SetStaminaRegenScale(playerStats.defaultStaminaRegenAmount);

            playerLocomotion.AttemptToCastSpell(spellIndex);
        }

        private void HandleButtonNorthInput()
        {
            if (buttonNorthInput)
            {
                buttonNorthInput = false;

                if (playerMenuOpen && !mapMenuOpen)
                    playerMenuManager.OpenLevelUpMenu();
            }
        }

        private void HandleSprinting()
        {
            if (playerMenuOpen || inMenu || mapMenuOpen || playerStats.dead || tutorialPopupActive)
            {
                sprintInput = false;
                return;
            }

            if (sprintInput)
            {
                sprintInput = false;
                blockInput = false;
                isBlocking = false;
                playerLocomotion.SetAnimBlockFalse();
                blockingCollider.SetBlockingCollider(false);

                if (!playerLocomotion.onMount)
                {
                    playerLocomotion.ToggleSprint();
                }
                else
                {
                    mountLocomotion.ToggleGallop();
                }

            }
        }

        private void HandleBlocking()
        {
            if (playerMenuOpen || inMenu || mapMenuOpen || playerStats.dead || tutorialPopupActive)
            {
                if (blockInput && optionsWindow.activeSelf && viewControlsWindow.activeInHierarchy)
                {
                    optionsManager.SwitchControlsLayout();
                }
                blockInput = false;
                return;
            }

            if (blockInput)
            {
                if (isPerformingAction)
                    return;
                //Debug.Log("Handle blocking Case 1");
                playerLocomotion.AttemptToBlock();
            }
            else if (isBlocking)
            {
                isBlocking = false;
                blockingCollider.SetBlockingCollider(false);
                playerStats.SetStaminaRegenScale(playerStats.defaultStaminaRegenAmount);
                playerLocomotion.SetAnimBlockFalse();
            }
        }

        private void HandleJumpInput()
        {
            if (jumpInput)
            {
                jumpInput = false;

                if (playerMenuOpen || inMenu || mapMenuOpen || playerStats.dead || tutorialPopupActive) return;

                if (!playerLocomotion.onMount)
                {
                    if (canMountLeft && !isPerformingAction)
                    {
                        canMountLeft = false;
                        canMountRight = false;
                        playerLocomotion.AttemptToMountHorse("left");
                    }
                    else if (canMountRight && !isPerformingAction)
                    {
                        canMountLeft = false;
                        canMountRight = false;
                        playerLocomotion.AttemptToMountHorse("right");
                    }
                    else if(!openSpells)
                    {
                        playerLocomotion.AttemptToJump();
                    }
                }
                else if (!openSpells)
                {
                    mountLocomotion.AttemptToJump();
                }
            }
        }

        private void HandleLockOnInput()
        {
            if (playerMenuOpen || inMenu || mapMenuOpen || mapMenuOpen || playerStats.dead || tutorialPopupActive)
            {
                lockOnInput = false;
                lockOnLeftInput = false;
                lockOnRightInput = false;
                cameraHandler.SetCameraHeight();
                return;
            }

            // CNot currently locked on, check if lock on or reorient camera based on available lock on targets
            if (lockOnInput && !lockedOn)
            {
                lockOnInput = false;
                cameraHandler.HandleLockOn();
                // Lock on Target found, lock on
                if (cameraHandler.nearestLockOnTarget != null)
                {
                    cameraHandler.currentLockOnTarget = cameraHandler.nearestLockOnTarget;
                    lockedOn = true;
                    targetIconPosition.StartUpdatingIconPosition();
                }
                // No lock on target found, reset camera position/rotation
                else
                {
                    cameraHandler.ReorientCamera();
                }
            }
            // Currently locked on, unlock
            else if (lockOnInput && lockedOn)
            {
                lockOnInput = false;
                lockedOn = false;
                targetIconPosition.StopUpdatingIconPosition();
                cameraHandler.ClearLockOnTargets();
            }

            if (lockOnLeftInput)
            {
                lockOnLeftInput = false;
                if (lockedOn)
                {
                    if (lockOnTimerReady)
                    {
                        StartCoroutine(ResetLockOnTimer(lockOnDelay));
                        cameraHandler.HandleLockOn();
                        //Debug.Log(cameraHandler.availableTargets.Count);
                        if (cameraHandler.leftLockTarget != null)
                        {
                            cameraHandler.currentLockOnTarget = cameraHandler.leftLockTarget;
                        }
                    }
                }
            }
            else if (lockOnRightInput)
            {
                lockOnRightInput = false;
                if (lockedOn)
                {
                    if (lockOnTimerReady)
                    {
                        StartCoroutine(ResetLockOnTimer(lockOnDelay));
                        cameraHandler.HandleLockOn();
                        //Debug.Log(cameraHandler.availableTargets.Count);
                        if (cameraHandler.rightLockTarget != null)
                        {
                            cameraHandler.currentLockOnTarget = cameraHandler.rightLockTarget;
                        }
                    }
                }

            }

            cameraHandler.SetCameraHeight();
        }

        private IEnumerator ResetLockOnTimer(float waitTime)
        {
            lockOnTimerReady = false;
            yield return new WaitForSeconds(waitTime);
            //Debug.Log("LOCK ON READY");
            lockOnTimerReady = true;
        }

        private void OnPlayerMenu(InputAction.CallbackContext context)
        {
            // Check if the ESC key was used on a Keyboard, if so flag input as PC to enable mouse with menu
            keyboardUsedForInput = context.control.device is Keyboard || context.control.name == "escape";
            playerMenuInput = true;
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            // Check if the F key was used on a Keyboard, if so flag input as PC to enable mouse with menu
            Debug.Log("context.control.name: " + context.control.name);
            keyboardUsedForInput = context.control.device is Keyboard || context.control.name == "f";
            interactInput = true;
        }

        private void HandlePlayerMenuInput()
        {
            if (playerMenuInput)
            {
                playerMenuInput = false;
                bool showCursor = keyboardUsedForInput;
                keyboardUsedForInput = false;

                if (playerStats.dead)
                {
                    return;
                }

                if (tutorialPopupActive)
                {
                    helpMenu.Close();
                    return;
                }

                if (mapMenuOpen)
                {
                    playerMenuManager.CloseMapMenu();
                    if (!playerMenuOpen) Cursor.lockState = CursorLockMode.Locked;
                    return;
                }

                if (noteUI.activeSelf)
                {
                    noteManager.Close();
                    return;
                }

                if (optionsManager.lastGameObjectSelected != null)
                {
                    optionsManager.SetLastSelectedGameObjectOptionToNull();
                }

                if (shopWindow.activeSelf)
                {
                    if (purchaseWindow.activeSelf)
                    {
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(cancelButtonPurchase);
                    }
                    else if (quantityWindow.activeSelf)
                    {
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(cancelButtonQuantity);
                    }
                    else
                    {
                        shopMenuManager.CloseShopMenu();
                        return;
                    }
                }

                if (StablesShop.instance.gameObject.activeSelf)
                {
                    confirmStables.onClick.Invoke();
                    return;
                }

                if (inMenu) return;

                playerMenuManager.FindInventorySlots();

                if (levelUpMenuWindow.activeSelf)
                {
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(confimLevelUpButton);
                    return;
                }

                playerMenuOpen = !playerMenuOpen;

                if (!mapMenuOpen)
                {
                    if (playerMenuOpen)
                    {
                        playerMenuManager.OpenPlayerMenu();
                        playerMenuOpen = true;
                        playerMenuManager.FindInventorySlots();
                        playerMenuManager.UpdateUI();
                        levelUpMenuWindow.SetActive(false);

                        Color originalColor = controllerUIManager.levelUpButton.color;
                        if (showCursor) // PC UI
                        {
                            Cursor.lockState = CursorLockMode.None;
                            controllerUIManager.levelUpButton.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1); //opaque
                            controllerUIManager.controllerLevelUpButtonDisplay.SetActive(false);
                        }
                        else // Controller UI
                        {
                            controllerUIManager.levelUpButton.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0); //transparent
                            controllerUIManager.controllerLevelUpButtonDisplay.SetActive(true);
                        }
                    }
                    else
                    {
                        swapRingWindow.SetActive(false);
                        swapSpellWindow.SetActive(false);
                        Cursor.lockState = CursorLockMode.Locked;
                        playerMenuManager.ClosePlayerMenu();
                        playerMenuOpen = false;
                    }
                }
            }
        }

        // Only called from TrackQuestPrompt.cs, track prompt is enabled in DisplayQuestUpdatedTewt() in QuestManager.cs
        public void UpdateTrackQuestListeners(bool enabled)
        {
            if (enabled)
            {
                trackQuest = false;
                inputActions.PlayerActions.TrackQuest.performed += i => trackQuest = true;
                inputActions.PlayerActions.TrackQuest.canceled += i => trackQuest = false;
            }
            else
            {
                inputActions.PlayerActions.TrackQuest.performed -= i => trackQuest = true;
                inputActions.PlayerActions.TrackQuest.canceled -= i => trackQuest = false;
                trackQuest = false;
            }
        }

        // Only called from TrackQuestPrompt.cs, track prompt is enabled in DisplayQuestUpdatedText() in QuestManager.cs
        public void HandleTrackQuestInput()
        {
            if(inputActions.PlayerActions.TrackQuest.ReadValue<float>() > 0)
            {
                trackQuestCurrentHoldTime += Time.deltaTime;
            }
            else
            {
                trackQuestCurrentHoldTime = 0;
            }

            trackQuestFill.fillAmount = trackQuestCurrentHoldTime / trackQuestHoldDuration;

            // Hold Duration met, trigger track quest
            if (trackQuest)
            {
                trackQuest = false;
                UpdateTrackQuestListeners(false);
                trackQuestFill.fillAmount = 0f;
                questManager.TrackPromptTriggered();
            }
        }

        private void HandlePlayerMenuShouldersAndTriggers()
        {
            if (inMenu && !shopWindow.activeSelf) return; //may change this for navigating menus, add more sub options

            if (menuLeftShoulder)
            {
                menuLeftShoulder = false;
                if (levelUpMenuWindow.activeSelf)
                    return;
                if (playerMenuOpen && !mapMenuOpen)
                {
                    if (swapRingWindow.activeSelf || swapSpellWindow.activeSelf) return;
                    playerMenuManager.SwitchInventoryTab("left");
                }
                else if (shopWindow.activeSelf)
                {
                    if (purchaseWindow.activeSelf)
                    {
                        return;
                    }
                    else if(quantityWindow.activeSelf)
                    {
                        shopMenuManager.AdjustSlider("leftShoulder");
                    }
                    else
                    {
                        shopMenuManager.SwitchInventoryTab("left");
                    }
                }

            }
            if (menuRightShoulder)
            {
                menuRightShoulder = false;
                if (levelUpMenuWindow.activeSelf)
                    return;
                if (playerMenuOpen && !mapMenuOpen)
                {
                    if (swapRingWindow.activeSelf || swapSpellWindow.activeSelf) return;
                    playerMenuManager.SwitchInventoryTab("right");
                }
                else if(shopWindow.activeSelf)
                {
                    if (purchaseWindow.activeSelf)
                    {
                        return;
                    }
                    else if (quantityWindow.activeSelf)
                    {
                        shopMenuManager.AdjustSlider("rightShoulder");
                    }
                    else
                    {
                        shopMenuManager.SwitchInventoryTab("right");
                    }
                }

            }
            if (menuLeftTrigger)
            {
                menuLeftTrigger = false;
                if (levelUpMenuWindow.activeSelf)
                    return;
                if (playerMenuOpen && !mapMenuOpen)
                {
                    if (swapRingWindow.activeSelf || swapSpellWindow.activeSelf) return;
                    playerMenuManager.SwitchMenuTab("left");
                }
                else if(shopWindow.activeSelf)
                {
                    if (purchaseWindow.activeSelf)
                    {
                        return;
                    }
                    else if (quantityWindow.activeSelf)
                    {
                        shopMenuManager.AdjustSlider("leftTrigger");
                    }
                    else
                    {
                        shopMenuManager.SwitchMenuTab();
                    }
                }

            }
            if (menuRightTrigger)
            {
                menuRightTrigger = false;
                if (levelUpMenuWindow.activeSelf)
                    return;
                if (playerMenuOpen && !mapMenuOpen)
                {
                    if (swapRingWindow.activeSelf || swapSpellWindow.activeSelf) return;
                    playerMenuManager.SwitchMenuTab("right");
                }
                else if (shopWindow.activeSelf)
                {
                    if (purchaseWindow.activeSelf)
                    {
                        return;
                    }
                    else if (quantityWindow.activeSelf)
                    {
                        shopMenuManager.AdjustSlider("rightTrigger");
                    }
                    else
                    {
                        shopMenuManager.SwitchMenuTab();
                    }
                }

            }
        }

        private void OnMapMenu(InputAction.CallbackContext context)
        {
            // Check if the ESC key was used on a Keyboard, if so flag input as PC to enable mouse with menu
            keyboardUsedForInput = context.control.device is Keyboard || context.control.name == "escape";
            mapMenuInput = true;
        }

        private void HandleMapMenuInput()
        {
            if (mapMenuInput)
            {
                mapMenuInput = false;
                bool showCursor = keyboardUsedForInput;
                keyboardUsedForInput = false;

                if (noteUI.activeSelf || playerStats.dead || tutorialPopupActive) return;

                if (shopWindow.activeSelf || dialogueMenu.gameObject.activeSelf || StablesShop.instance.gameObject.activeSelf || characterCreateCanvas.activeSelf)
                {
                    return;
                }

                if (mapMenuOpen)
                {
                    playerMenuManager.CloseMapMenu();

                    if(!playerMenuOpen) Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    playerMenuManager.OpenMapMenu();
                    if (showCursor) // Show cursor refers to the system cursor, hides virtual cursor's sprite renderer
                    {
                        Cursor.lockState = CursorLockMode.Confined;
                        virtualCursor.enabled = false;
                    }
                    else // Display the virutal cursor
                    {
                        virtualCursor.enabled = true;
                    }
                }             
            }
        }

        private void HandleCharCreateControllerInputs()
        {
            if (!characterCreateCanvas.activeSelf) return; //may change this for navigating menus, add more sub options

            if (charMenuSpinLeft && !spinningFlag)
            {
                spinningFlag = true;
                if (spinManagerDefault.gameObject.activeSelf)
                    spinManagerDefault.OnPress("left");
                else if (spinManagerConfirm.gameObject.activeSelf)
                    spinManagerConfirm.OnPress("left");
            }
            else if (charMenuSpinRight && !spinningFlag)
            {
                spinningFlag = true;
                if (spinManagerDefault.gameObject.activeSelf)
                    spinManagerDefault.OnPress("right");
                else if (spinManagerConfirm.gameObject.activeSelf)
                    spinManagerConfirm.OnPress("right");
            }
            else if (spinningFlag)
            {
                spinningFlag = false;
                if (spinManagerDefault.gameObject.activeSelf)
                {
                    spinManagerDefault.OnRelease("left");
                    spinManagerDefault.OnRelease("right");
                }
                else if (spinManagerConfirm.gameObject.activeSelf)
                {
                    spinManagerConfirm.OnRelease("left");
                    spinManagerConfirm.OnRelease("right");
                }
            }

            if (charMenuRecenter)
            {
                charMenuRecenter = false;
                if (spinManagerDefault.gameObject.activeSelf)
                    spinManagerDefault.Recenter();
                else if (spinManagerConfirm.gameObject.activeSelf)
                    spinManagerConfirm.Recenter();
            }

            if (charMenuZoomToggle)
            {
                charMenuZoomToggle = false;
                if (zoomDefault.gameObject.activeSelf)
                    zoomDefault.Zoom();
                else if (zoomConfirm.gameObject.activeSelf)
                    zoomConfirm.Zoom();
            }
        }

        private void HandleSheatheWeapon()
        {
            if (sheatheWeaponInput)
            {
                sheatheWeaponInput = false;

                if (playerMenuOpen || inMenu || isPerformingAction || isAttacking || mapMenuOpen || playerStats.dead || tutorialPopupActive) return;

                if (isGrounded || playerLocomotion.onMount)
                {
                    playerLocomotion.SheatheUnsheatheWeapon();
                }
            }
        }

        private void HandleCallHorse()
        {
            if (callHorseInput)
            {
                if (playerMenuOpen || inMenu || mapMenuOpen || playerStats.dead || tutorialPopupActive)
                {
                    callHorseInput = false;
                    return;
                }

                if(!horseAIVer.activeSelf) //horse is not active yet
                {
                    callHorseInput = false;
                    playerLocomotion.CallHorse(false);
                }
                else if(callHorseCoroutine == null)
                {
                    callHorseCoroutine = StartCoroutine(DetermineCallHorseAction());
                }
            }
        }

        private IEnumerator DetermineCallHorseAction()
        {
            float holdCheckTimer = 0.0f;
            float timeRequirement = 1.0f;
            bool forceFollow = false;
            bool dismissHorse = false;

            while (callHorseInput)
            {
                holdCheckTimer += Time.deltaTime;

                // First threshold for force follow
                if (!forceFollow && holdCheckTimer >= timeRequirement)
                {
                    forceFollow = true;
                    playerLocomotion.CallHorse(forceFollow, dismissHorse); // Call when force follow is reached
                }

                // Second threshold for dismissing the horse
                if (holdCheckTimer >= timeRequirement * 4f)
                {
                    dismissHorse = true;
                    break; // Exit loop after reaching the second threshold
                }

                yield return null; // Wait for the next frame
            }

            // Ensure dismissHorse is passed after the second threshold
            playerLocomotion.CallHorse(forceFollow, dismissHorse);
            callHorseInput = false;
            callHorseCoroutine = null;
        }

        private void HandleTestAnimationInput()
        {
            if (testAnimationInput)
            {
                testAnimationInput = false;
                redSlider_hairColor.interactable = false;
                greenSlider_hairColor.interactable = false;
                blueSlider_hairColor.interactable = false;
                hairColorSelect.interactable = true;
                hairColorSelect.Select();
            }
        }

        public void ResetInputs(bool resetMovement = true, bool resetCamera = true)
        {
            if(resetMovement) movementInput = Vector2.zero;
            if(resetCamera) cameraInput = Vector2.zero;
            dodgeInput = false;
            jumpInput = false;
            sprintInput = false;
            blockInput = false;
            lockOnInput = false;
            lockOnLeftInput = false;
            lockOnRightInput = false;
            playerMenuInput = false;
            mapMenuInput = false;
            attackInput = false;
            heavyAttackInput = false;
            castSpellInput = false;
            useItemInput = false;
            switchItemLeftInput = false;
            switchItemRightInput = false;
            sheatheWeaponInput = false;
            callHorseInput = false;
            testAnimationInput = false;
            _OpenSpells = false;
            openSpells = false;
            switchSpellsHUDInput = false;
            spellNorth = false;
            spellWest = false;
            spellEast = false;
            spellSouth = false;
            spell1 = false;
            spell2 = false;
            spell3 = false;
            spell4 = false;
            spell5 = false;
            spell6 = false;
            spell7 = false;
            spell8 = false;
            interactInput = false;
            menuLeftShoulder = false;
            menuRightShoulder = false;
            menuLeftTrigger = false;
            menuRightTrigger = false;
            buttonNorthInput = false;
            charMenuSpinLeft = false;
            charMenuSpinRight = false;
            charMenuZoomToggle = false;
            charMenuRecenter = false;
        }

        // DEBUG:
        public void InMenu()
        {
            inMenu = true;
        }
    }
}
