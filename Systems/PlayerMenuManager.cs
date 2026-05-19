using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace etchebarren
{
    public class PlayerMenuManager : MonoBehaviour
    {
        public static PlayerMenuManager instance;

        [Header("Script References")]
        public PlayerInventory playerInventory;
        public GameObject playerMenuWindow;
        public GameObject mapMenuWindow;
        public GameObject controllerHUD;
        public PlayerStats playerStats;
        public UIAudioManager uiAudioManager;
        public QuestManager questManager;
        public MapInputHandler mapInputHandler;
        public InputHandler inputHandler;
        public NoteManager noteManager;
        public OptionsManager optionsManager;
        public ControlsNotice controlsNotice;
        public SkillTreeManager skillTreeManager;

        [Header("References for Refreshing Menu")]
        public GameObject itemInfoBox;
        [HideInInspector] public GameObject lastSelectedBorder;
        public GameObject equipRatingDescription;
        public GameObject primaryStatDescription;
        public GameObject secondaryStatsDescription;
        private Color hidden = new Color(0.37f, 0.27f, 0.15f, 0.0f);
        [HideInInspector] public Image lastStatButtonImage;
        public TextMeshProUGUI playerNameAndLevelText;

        [Header("References for switching menu tabs")]
        [HideInInspector] public int currentTabIndex;
        public GameObject[] menuTabs;
        [HideInInspector] public int currentInventoryTabIndex;
        public GameObject[] inventoryTabs;
        public GameObject[] inventoryContents;
        public GameObject inventoryWindow;
        public GameObject questInfoBox;
        public GameObject questWindow;
        public GameObject activeQuestsContents;
        public GameObject[] questTabs;
        public GameObject[] questContents;
        public GameObject optionsWindow;
        public GameObject[] optionsTabs;
        public GameObject[] optionsContents;
        [HideInInspector] public int currentOptionsTabIndex = 0;
        [HideInInspector] public int currentSkillTabIndex;
        public GameObject[] skillTabs;
        public GameObject[] skillContents;
        public GameObject skillWindow;
        public Scrollbar[] scrollbars;

        [Header("UI elements to hide/show on close/open playerMenu")]
        //public GameObject textNotifications;
        public GameObject acquiredNotifications;
        public GameObject sheathIcon;
        public GameObject whistleIcon;
        public GameObject horseStatusHUD;
        public GameObject horseAIVer;
        public GameObject horseMountVer;
        public GameObject goldHUDObj;
        public GameObject minimapWindow;
        public Canvas waypointCanvas; // Used for horse icon, target icon, etc. 
        public GameObject interactContainer; // special case, only renable if it was active
        public bool interactContainerWasActive = false;
        public bool wasQuestHUDActive;
        public GameObject levelUpIconInGame;

        //Move position of these objects
        public RectTransform trackedQuestHUD;
        public VerticalLayoutGroup trackedQuestVerticalLayoutGroup; // set this false/true when moving tracked quest hud
        public RectTransform questUpdateText;
        [SerializeField] private Vector2 originalPositionTrackedQuestHUD;
        [SerializeField] private Vector2 originalPositionQuestUpdateText;

        [Header("UI elements for controller HUD")]
        public GameObject select;
        public TextMeshProUGUI selectText;
        public GameObject back;
        public TextMeshProUGUI backText;

        [Header("References for taking camera snapshot for menu")]
        public Camera mainCamera;
        public RenderTexture preMenuSnapshot;
        private LayerMask originalCullingMask;

        [Header("References for opening Level Up window")]
        //public GameObject levelUpMenu;
        public Button levelUpButton;
        public GameObject levelUpNotification;

        [Header("Reference for moving UI on open/close player menu")]
        public GameObject background;
        public RectTransform statusBars;
        public RectTransform xpBar;
        public RectTransform consumablesHUD;
        public RectTransform statusHUD;
        public RectTransform spellsHUD;
        public RectTransform goldHUD;
        public RectTransform textNotifications;
        public Vector3 originalTextNotificationsPosition; // = new Vector3(-856f, 251f, 0f); // Overwriten in Start
        public Vector3 inMenuTextNotificationsPosition = new Vector3(359, 431, 0);
        public RectTransform acquNotifications;
        public Vector3 originalAcquNotificationsPosition; // = new Vector3(-856f, 251f, 0f); // Overwriten in Start
        public Vector3 inMenuAcquNotificationsPosition = new Vector3(-777, 504, 0);

        [Header("Reference for displaying and updating notes in player menu")]
        public GameObject noteDisplayPlayerMenu;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI bodyText;
        public Image noteImage;
        public TextMeshProUGUI noteNameText;
        public GameObject statsBox;

        [Header("Reference for UI related to scene transisitons or animations")]
        public Image blackScreen;

        [Header("References for going back to selected ring")]
        [HideInInspector] public GameObject lastSelectedRingBeforeSwap;

        [Header("References for going back to selected spell")]
        [HideInInspector] public GameObject lastSelectedSpellBeforeSwap;

        [Header("References for going back to selected button when opening/closing map/helpmenu when player menu is open. Do not set.")]
        public bool playerMenuWasOpen = false;
        public GameObject lastSelectedObjectBeforeMap;
        public GameObject lastSelectedObjectBeforeHelpMenu;

        [Header("Weapon Inventory")]
        public GameObject weaponInventorySlotPrefab;
        public Transform weaponInventorySlotsParent;
        WeaponInventorySlot[] weaponInventorySlots;

        [Header("Shield Inventory")]
        public GameObject shieldInventorySlotPrefab;
        public Transform shieldInventorySlotsParent;
        ShieldInventorySlot[] shieldInventorySlots;

        [Header("Torso Armor Inventory")]
        public GameObject torsoArmorInventorySlotPrefab;
        public Transform torsoArmorInventorySlotsParent;
        TorsoArmorInventorySlot[] torsoArmorInventorySlots;

        [Header("Hands Armor Inventory")]
        public GameObject handsArmorInventorySlotPrefab;
        public Transform handsArmorInventorySlotsParent;
        HandsArmorInventorySlot[] handsArmorInventorySlots;

        [Header("Legs Armor Inventory")]
        public GameObject legsArmorInventorySlotPrefab;
        public Transform legsArmorInventorySlotsParent;
        LegsArmorInventorySlot[] legsArmorInventorySlots;

        [Header("Ring Inventory")]
        public GameObject ringInventorySlotPrefab;
        public Transform ringInventorySlotsParent;
        RingInventorySlot[] ringInventorySlots;

        [Header("Amulet Inventory")]
        public GameObject amuletInventorySlotPrefab;
        public Transform amuletInventorySlotsParent;
        AmuletInventorySlot[] amuletInventorySlots;

        [Header("Spell Inventory")]
        public GameObject spellInventorySlotPrefab;
        public Transform spellInventorySlotsParent;
        SpellInventorySlot[] spellInventorySlots;

        [Header("Consumable Inventory")]
        public GameObject consumableInventorySlotPrefab;
        public Transform consumableInventorySlotsParent;
        ConsumableInventorySlot[] consumableInventorySlots;

        [Header("Key Item Inventory")]
        public GameObject keyItemInventorySlotPrefab;
        public Transform keyItemInventorySlotsParent;
        KeyItemInventorySlot[] keyItemInventorySlots;

        [Header("Active Quest Slots")]
        List<Quest> activeQuests = new List<Quest>();
        public GameObject activeQuestSlotPrefab;
        public Transform activeQuestSlotsParent;
        QuestMenuSlot[]activeQuestMenuSlots;

        [Header("Completed Quest Slots")]
        List<Quest> completedQuests = new List<Quest>();
        public GameObject completedQuestSlotPrefab;
        public Transform completedQuestSlotsParent;
        QuestMenuSlot[] completedQuestMenuSlots;

        [Header("Temporary Values")]
        public GameObject[] bossStatusCanvases;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if(instance != this)
            {
                Destroy(gameObject);
            }
            originalCullingMask = mainCamera.cullingMask;
            // Record original positions (they're moved in menus)
            originalPositionTrackedQuestHUD = trackedQuestHUD.anchoredPosition;
            originalPositionQuestUpdateText = questUpdateText.anchoredPosition;
            originalTextNotificationsPosition = textNotifications.anchoredPosition;
            originalAcquNotificationsPosition = acquNotifications.anchoredPosition;
            gameObject.SetActive(false); // Disable this gameObject when gameStarts
        }

        public void Start()
        {
            FindInventorySlots();
        }

        public void FindInventorySlots()
        {
            weaponInventorySlots = weaponInventorySlotsParent.GetComponentsInChildren<WeaponInventorySlot>();
            shieldInventorySlots = shieldInventorySlotsParent.GetComponentsInChildren<ShieldInventorySlot>();
            torsoArmorInventorySlots = torsoArmorInventorySlotsParent.GetComponentsInChildren<TorsoArmorInventorySlot>();
            handsArmorInventorySlots = handsArmorInventorySlotsParent.GetComponentsInChildren<HandsArmorInventorySlot>();
            legsArmorInventorySlots = legsArmorInventorySlotsParent.GetComponentsInChildren<LegsArmorInventorySlot>();
            ringInventorySlots = ringInventorySlotsParent.GetComponentsInChildren<RingInventorySlot>();
            amuletInventorySlots = amuletInventorySlotsParent.GetComponentsInChildren<AmuletInventorySlot>();
            spellInventorySlots = spellInventorySlotsParent.GetComponentsInChildren<SpellInventorySlot>();
            consumableInventorySlots = consumableInventorySlotsParent.GetComponentsInChildren<ConsumableInventorySlot>();
            keyItemInventorySlots = keyItemInventorySlotsParent.GetComponentsInChildren<KeyItemInventorySlot>();
            activeQuestMenuSlots = activeQuestSlotsParent.GetComponentsInChildren<QuestMenuSlot>();
            completedQuestMenuSlots = completedQuestSlotsParent.GetComponentsInChildren<QuestMenuSlot>();
        }

        public void UpdateUI()
        {
            //Update Player Name & Level in Stats Box
            playerNameAndLevelText.text = PlayerStats.instance.playerName + " - Lv." + PlayerStats.instance.playerLevel;

            #region WEAPON INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < weaponInventorySlots.Length; i++)
            {
                weaponInventorySlots[i].ClearInventorySlot();
                weaponInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.weaponsInventory.Count > 0) // If there is inventory, create the slots
            {
                weaponInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.weaponsInventory.Count; i++)
                {
                    if (i < weaponInventorySlots.Length)
                    {
                        // Reuse existing slot
                        weaponInventorySlots[i].gameObject.SetActive(true);
                        weaponInventorySlots[i].AddWeaponItem(playerInventory.weaponsInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(weaponInventorySlotPrefab, weaponInventorySlotsParent);
                        WeaponInventorySlot newSlotComponent = newSlot.GetComponent<WeaponInventorySlot>();
                        newSlotComponent.AddWeaponItem(playerInventory.weaponsInventory[i]);

                        // Update the slots array
                        List<WeaponInventorySlot> slotsList = new List<WeaponInventorySlot>(weaponInventorySlots);
                        slotsList.Add(newSlotComponent);
                        weaponInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                weaponInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region SHIELD INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < shieldInventorySlots.Length; i++)
            {
                shieldInventorySlots[i].ClearInventorySlot();
                shieldInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.shieldsInventory.Count > 0) // If there is inventory, create the slots
            {
                shieldInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.shieldsInventory.Count; i++)
                {
                    if (i < shieldInventorySlots.Length)
                    {
                        // Reuse existing slot
                        shieldInventorySlots[i].gameObject.SetActive(true);
                        shieldInventorySlots[i].AddShieldItem(playerInventory.shieldsInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(shieldInventorySlotPrefab, shieldInventorySlotsParent);
                        ShieldInventorySlot newSlotComponent = newSlot.GetComponent<ShieldInventorySlot>();
                        newSlotComponent.AddShieldItem(playerInventory.shieldsInventory[i]);

                        // Update the slots array
                        List<ShieldInventorySlot> slotsList = new List<ShieldInventorySlot>(shieldInventorySlots);
                        slotsList.Add(newSlotComponent);
                        shieldInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                shieldInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region TORSO ARMOR INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < torsoArmorInventorySlots.Length; i++)
            {
                torsoArmorInventorySlots[i].ClearInventorySlot();
                torsoArmorInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.torsoArmorInventory.Count > 0) // If there is inventory, create the slots
            {
                torsoArmorInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.torsoArmorInventory.Count; i++)
                {
                    if (i < torsoArmorInventorySlots.Length)
                    {
                        // Reuse existing slot
                        torsoArmorInventorySlots[i].gameObject.SetActive(true);
                        torsoArmorInventorySlots[i].AddTorsoArmorItem(playerInventory.torsoArmorInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(torsoArmorInventorySlotPrefab, torsoArmorInventorySlotsParent);
                        TorsoArmorInventorySlot newSlotComponent = newSlot.GetComponent<TorsoArmorInventorySlot>();
                        newSlotComponent.AddTorsoArmorItem(playerInventory.torsoArmorInventory[i]);

                        // Update the slots array
                        List<TorsoArmorInventorySlot> slotsList = new List<TorsoArmorInventorySlot>(torsoArmorInventorySlots);
                        slotsList.Add(newSlotComponent);
                        torsoArmorInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                torsoArmorInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region HANDS ARMOR INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < handsArmorInventorySlots.Length; i++)
            {
                handsArmorInventorySlots[i].ClearInventorySlot();
                handsArmorInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.handsArmorInventory.Count > 0) // If there is inventory, create the slots
            {
                handsArmorInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.handsArmorInventory.Count; i++)
                {
                    if (i < handsArmorInventorySlots.Length)
                    {
                        // Reuse existing slot
                        handsArmorInventorySlots[i].gameObject.SetActive(true);
                        handsArmorInventorySlots[i].AddHandsArmorItem(playerInventory.handsArmorInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(handsArmorInventorySlotPrefab, handsArmorInventorySlotsParent);
                        HandsArmorInventorySlot newSlotComponent = newSlot.GetComponent<HandsArmorInventorySlot>();
                        newSlotComponent.AddHandsArmorItem(playerInventory.handsArmorInventory[i]);

                        // Update the slots array
                        List<HandsArmorInventorySlot> slotsList = new List<HandsArmorInventorySlot>(handsArmorInventorySlots);
                        slotsList.Add(newSlotComponent);
                        handsArmorInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                handsArmorInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region LEGS ARMOR INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < legsArmorInventorySlots.Length; i++)
            {
                legsArmorInventorySlots[i].ClearInventorySlot();
                legsArmorInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.legsArmorInventory.Count > 0) // If there is inventory, create the slots
            {
                legsArmorInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.legsArmorInventory.Count; i++)
                {
                    if (i < legsArmorInventorySlots.Length)
                    {
                        // Reuse existing slot
                        legsArmorInventorySlots[i].gameObject.SetActive(true);
                        legsArmorInventorySlots[i].AddLegsArmorItem(playerInventory.legsArmorInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(legsArmorInventorySlotPrefab, legsArmorInventorySlotsParent);
                        LegsArmorInventorySlot newSlotComponent = newSlot.GetComponent<LegsArmorInventorySlot>();
                        newSlotComponent.AddLegsArmorItem(playerInventory.legsArmorInventory[i]);

                        // Update the slots array
                        List<LegsArmorInventorySlot> slotsList = new List<LegsArmorInventorySlot>(legsArmorInventorySlots);
                        slotsList.Add(newSlotComponent);
                        legsArmorInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                legsArmorInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region RING INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < ringInventorySlots.Length; i++)
            {
                ringInventorySlots[i].ClearInventorySlot();
                ringInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.ringsInventory.Count > 0) // If there is inventory, create the slots
            {
                ringInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.ringsInventory.Count; i++)
                {
                    if (i < ringInventorySlots.Length)
                    {
                        // Reuse existing slot
                        ringInventorySlots[i].gameObject.SetActive(true);
                        ringInventorySlots[i].AddRingItem(playerInventory.ringsInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(ringInventorySlotPrefab, ringInventorySlotsParent);
                        RingInventorySlot newSlotComponent = newSlot.GetComponent<RingInventorySlot>();
                        newSlotComponent.AddRingItem(playerInventory.ringsInventory[i]);

                        // Update the slots array
                        List<RingInventorySlot> slotsList = new List<RingInventorySlot>(ringInventorySlots);
                        slotsList.Add(newSlotComponent);
                        ringInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                ringInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region AMULETS INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < amuletInventorySlots.Length; i++)
            {
                amuletInventorySlots[i].ClearInventorySlot();
                amuletInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.amuletsInventory.Count > 0) // If there is inventory, create the slots
            {
                amuletInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.amuletsInventory.Count; i++)
                {
                    if (i < amuletInventorySlots.Length)
                    {
                        // Reuse existing slot
                        amuletInventorySlots[i].gameObject.SetActive(true);
                        amuletInventorySlots[i].AddAmuletItem(playerInventory.amuletsInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(amuletInventorySlotPrefab, amuletInventorySlotsParent);
                        AmuletInventorySlot newSlotComponent = newSlot.GetComponent<AmuletInventorySlot>();
                        newSlotComponent.AddAmuletItem(playerInventory.amuletsInventory[i]);

                        // Update the slots array
                        List<AmuletInventorySlot> slotsList = new List<AmuletInventorySlot>(amuletInventorySlots);
                        slotsList.Add(newSlotComponent);
                        amuletInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                amuletInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region SPELLS INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < spellInventorySlots.Length; i++)
            {
                spellInventorySlots[i].ClearInventorySlot();
                spellInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.spellsInventory.Count > 0) // If there is inventory, create the slots
            {
                spellInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.spellsInventory.Count; i++)
                {
                    // Fixed Bug where If a spell was added to inventory while on spell tab, switching back to tab included an empty slot
                    spellInventorySlots = spellInventorySlotsParent.GetComponentsInChildren<SpellInventorySlot>(); 

                    if (i < spellInventorySlots.Length)
                    {
                        // Reuse existing slot
                        spellInventorySlots[i].gameObject.SetActive(true);
                        spellInventorySlots[i].AddSpell(playerInventory.spellsInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(spellInventorySlotPrefab, spellInventorySlotsParent);
                        SpellInventorySlot newSlotComponent = newSlot.GetComponent<SpellInventorySlot>();
                        newSlotComponent.AddSpell(playerInventory.spellsInventory[i]);

                        // Update the slots array
                        List<SpellInventorySlot> slotsList = new List<SpellInventorySlot>(spellInventorySlots);
                        slotsList.Add(newSlotComponent);
                        spellInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                spellInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region CONSUMABLES INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < consumableInventorySlots.Length; i++)
            {
                consumableInventorySlots[i].ClearInventorySlot();
                consumableInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.consumablesInventory.Count > 0) // If there is inventory, create the slots
            {
                consumableInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.consumablesInventory.Count; i++)
                {
                    if (i < consumableInventorySlots.Length)
                    {
                        // Reuse existing slot
                        consumableInventorySlots[i].gameObject.SetActive(true);
                        consumableInventorySlots[i].AddConsumableItem(playerInventory.consumablesInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(consumableInventorySlotPrefab, consumableInventorySlotsParent);
                        ConsumableInventorySlot newSlotComponent = newSlot.GetComponent<ConsumableInventorySlot>();
                        newSlotComponent.AddConsumableItem(playerInventory.consumablesInventory[i]);

                        // Update the slots array
                        List<ConsumableInventorySlot> slotsList = new List<ConsumableInventorySlot>(consumableInventorySlots);
                        slotsList.Add(newSlotComponent);
                        consumableInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                consumableInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region KEY ITEMS INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < keyItemInventorySlots.Length; i++)
            {
                keyItemInventorySlots[i].ClearInventorySlot();
                keyItemInventorySlots[i].gameObject.SetActive(false);
            }

            if (playerInventory.keyItemsInventory.Count > 0) // If there is inventory, create the slots
            {
                keyItemInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < playerInventory.keyItemsInventory.Count; i++)
                {
                    if (i < keyItemInventorySlots.Length)
                    {
                        // Reuse existing slot
                        keyItemInventorySlots[i].gameObject.SetActive(true);
                        keyItemInventorySlots[i].AddKeyItem(playerInventory.keyItemsInventory[i]);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(keyItemInventorySlotPrefab, keyItemInventorySlotsParent);
                        KeyItemInventorySlot newSlotComponent = newSlot.GetComponent<KeyItemInventorySlot>();
                        newSlotComponent.AddKeyItem(playerInventory.keyItemsInventory[i]);

                        // Update the slots array
                        List<KeyItemInventorySlot> slotsList = new List<KeyItemInventorySlot>(keyItemInventorySlots);
                        slotsList.Add(newSlotComponent);
                        keyItemInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                keyItemInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region ACTIVE QUEST MENU SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < activeQuestMenuSlots.Length; i++)
            {
                activeQuestMenuSlots[i].ClearSlot();
                activeQuestMenuSlots[i].gameObject.SetActive(false);
            }

            // Clear our last snapshot of active quests
            activeQuests.Clear();

            // Iterate over the array and add active quests to capture a snapshot of active quests
            foreach (Quest quest in questManager.quests)
            {
                if (quest.questActive && !quest.completed)
                {
                    activeQuests.Add(quest);
                }
            }

            // Instantiate quest slots based on active quests list
            if (activeQuests.Count > 0) // If there are active quests, create the slots
            {
                activeQuestSlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < activeQuests.Count; i++)
                {
                    if (i < activeQuestMenuSlots.Length)
                    {
                        // Reuse existing slot
                        activeQuestMenuSlots[i].gameObject.SetActive(true);
                        activeQuestMenuSlots[i].AddQuestData(activeQuests[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(activeQuestSlotPrefab, activeQuestSlotsParent);
                        QuestMenuSlot newSlotComponent = newSlot.GetComponent<QuestMenuSlot>();
                        newSlotComponent.AddQuestData(activeQuests[i], true);

                        // Update the slots array
                        List<QuestMenuSlot> slotsList = new List<QuestMenuSlot>(activeQuestMenuSlots);
                        slotsList.Add(newSlotComponent);
                        activeQuestMenuSlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                activeQuestSlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region COMPLETED QUEST MENU SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < completedQuestMenuSlots.Length; i++)
            {
                completedQuestMenuSlots[i].ClearSlot();
                completedQuestMenuSlots[i].gameObject.SetActive(false);
            }

            // Clear our last snapshot of completed quests
            completedQuests.Clear();

            // Iterate over the array and add completed quests to capture a snapshot of completed quests
            foreach (Quest quest in questManager.quests)
            {
                if (quest.completed)
                {
                    completedQuests.Add(quest);
                }
            }

            // Instantiate quest slots based on completed quests list
            if (completedQuests.Count > 0) // If there are completed quests, create the slots
            {
                completedQuestSlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < completedQuests.Count; i++)
                {
                    if (i < completedQuestMenuSlots.Length)
                    {
                        // Reuse existing slot
                        completedQuestMenuSlots[i].gameObject.SetActive(true);
                        completedQuestMenuSlots[i].AddQuestData(completedQuests[i], false);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(completedQuestSlotPrefab, completedQuestSlotsParent);
                        QuestMenuSlot newSlotComponent = newSlot.GetComponent<QuestMenuSlot>();
                        newSlotComponent.AddQuestData(completedQuests[i], false);

                        // Update the slots array
                        List<QuestMenuSlot> slotsList = new List<QuestMenuSlot>(completedQuestMenuSlots);
                        slotsList.Add(newSlotComponent);
                        completedQuestMenuSlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                completedQuestSlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

        }

        public void SortInventory()
        {
            // Sorting the weapon list of items primarily by level and then by rarity
            playerInventory.weaponsInventory.Sort((a, b) =>
            {
                if (a.equipped && !b.equipped)
                {
                    return -1; // Move 'a' to the front if 'a' is equipped and 'b' is not
                }
                else if (!a.equipped && b.equipped)
                {
                    return 1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // If both or neither are equipped, sort by level and then by rarity
                    int levelComparison = b.level.CompareTo(a.level);
                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });
            // Sorting the shields list of items primarily by level and then by rarity
            playerInventory.shieldsInventory.Sort((a, b) =>
            {
                if (a.equipped && !b.equipped)
                {
                    return -1; // Move 'a' to the front if 'a' is equipped and 'b' is not
                }
                else if (!a.equipped && b.equipped)
                {
                    return 1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // If both or neither are equipped, sort by level and then by rarity
                    int levelComparison = b.level.CompareTo(a.level);
                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });
            // Sorting the torso armor list of items primarily by level and then by rarity
            playerInventory.torsoArmorInventory.Sort((a, b) =>
            {
                if (a.equipped && !b.equipped)
                {
                    return -1; // Move 'a' to the front if 'a' is equipped and 'b' is not
                }
                else if (!a.equipped && b.equipped)
                {
                    return 1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // If both or neither are equipped, sort by level and then by rarity
                    int levelComparison = b.level.CompareTo(a.level);
                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });
            // Sorting the hands armor list of items primarily by level and then by rarity
            playerInventory.handsArmorInventory.Sort((a, b) =>
            {
                if (a.equipped && !b.equipped)
                {
                    return -1; // Move 'a' to the front if 'a' is equipped and 'b' is not
                }
                else if (!a.equipped && b.equipped)
                {
                    return 1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // If both or neither are equipped, sort by level and then by rarity
                    int levelComparison = b.level.CompareTo(a.level);
                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });
            // Sorting the legs armor list of items primarily by level and then by rarity
            playerInventory.legsArmorInventory.Sort((a, b) =>
            {
                if (a.equipped && !b.equipped)
                {
                    return -1; // Move 'a' to the front if 'a' is equipped and 'b' is not
                }
                else if (!a.equipped && b.equipped)
                {
                    return 1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // If both or neither are equipped, sort by level and then by rarity
                    int levelComparison = b.level.CompareTo(a.level);
                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });
            // Sorting the rings list of items primarily by level and then by rarity
            playerInventory.ringsInventory.Sort((a, b) =>
            {
                if (a.equipped && !b.equipped)
                {
                    return -1; // Move 'a' to the front if 'a' is equipped and 'b' is not
                }
                else if (!a.equipped && b.equipped)
                {
                    return 1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // If both or neither are equipped, sort by level and then by rarity
                    int levelComparison = b.level.CompareTo(a.level);
                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });
            // Sorting the amulets list of items primarily by level and then by rarity
            playerInventory.amuletsInventory.Sort((a, b) =>
            {
                if (a.equipped && !b.equipped)
                {
                    return -1; // Move 'a' to the front if 'a' is equipped and 'b' is not
                }
                else if (!a.equipped && b.equipped)
                {
                    return 1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // If both or neither are equipped, sort by level and then by rarity
                    int levelComparison = b.level.CompareTo(a.level);
                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });

            // Sorting the list of items primarily by rarity
            playerInventory.spellsInventory.Sort((a, b) => b.rarity.CompareTo(a.rarity));

            // Sorting the list of items primarily by consumable ID
            playerInventory.consumablesInventory.Sort((a, b) => b.consumable_ID.CompareTo(a.consumable_ID));

            // Sorting the list of items primarily by rarity
            playerInventory.keyItemsInventory.Sort((a, b) => b.rarity.CompareTo(a.rarity));

            // Sorting the quest list by tracked then by main/side
            questManager.SortQuestList();
        }

        public void OpenPlayerMenu()
        {
            //Take snapshot, display snapshot as background, stop rendering to help performance in menu
            SetMenuBackground(true);

            // Pause and Hide any tutorial notices
            controlsNotice.SetPause(true);
            HelpMenu.instance.Close();

            //open controller HUD
            controllerHUD.SetActive(true);
            //pause game
            Time.timeScale = 0;
            //Reset menu tabs
            currentTabIndex = 0;
            currentInventoryTabIndex = 0;
            menuTabs[currentTabIndex].GetComponent<Button>().onClick.Invoke();
            inventoryTabs[currentInventoryTabIndex].GetComponent<Button>().onClick.Invoke();

            currentSkillTabIndex = 0;
            currentOptionsTabIndex = 0;

            CheckAndSetLevelUpNotification();
            PlayerStats.instance.levelUpText.SetActive(false);

            SortInventory();
            SpellsHUDManager.instance.UpdateSpellHUD();
            UpdateUI();
            PlayerStats.instance.UpdateStatsScreen();
            playerMenuWindow.SetActive(true);
            questInfoBox.SetActive(false);
            SetSelectedInventorySlot();

            PlayerStats.instance.ScaleStatusBars(true);
            //move Health bars

            //textNotifications.SetActive(false);
            //acquiredNotifications.SetActive(false);

            goldHUDObj.SetActive(true);

            sheathIcon.SetActive(false);
            whistleIcon.SetActive(false);
            horseStatusHUD.SetActive(false);
            minimapWindow.SetActive(false);
            levelUpIconInGame.SetActive(false);

            waypointCanvas.enabled = false;

            MoveQuestHUD(false); // False means out of frame

            MoveUIElements(true);

            uiAudioManager.PlayOpenMenuAudio();
        }

        public void ClosePlayerMenu(bool playAudio = true)
        {
            inputHandler.playerMenuOpen = false;

            inputHandler.viewControlsWindow.SetActive(false);

            // Disable background, and restore camera rendering
            SetMenuBackground(false);

            // Unpause and Display any tutorial notices
            controlsNotice.SetPause(false);
            HelpMenu.instance.Close();

            //close controller HUD
            //controllerHUD.SetActive(false);
            ControllerUIManager.instance.SetAllInactive();

            Time.timeScale = 1;

            playerMenuWindow.SetActive(false);
            questInfoBox.SetActive(false);

            itemInfoBox.SetActive(false);
            equipRatingDescription.SetActive(false);
            primaryStatDescription.SetActive(false);
            secondaryStatsDescription.SetActive(false);
            if(lastSelectedBorder != null)
                lastSelectedBorder.SetActive(false);
            if (lastStatButtonImage != null)
                lastStatButtonImage.color = hidden;

            SortInventory();

            SpellsHUDManager.instance.UpdateSpellHUD();

            PlayerStats.instance.ScaleStatusBars(false);

            goldHUDObj.SetActive(false);

           // acquiredNotifications.SetActive(true);

            sheathIcon.SetActive(true);
            whistleIcon.SetActive(true);
            minimapWindow.SetActive(true);
            if (PlayerStats.instance.remainingPoints > 0) levelUpIconInGame.SetActive(true);

            bool horseHUD = horseAIVer.activeSelf || horseMountVer.activeSelf;
            horseStatusHUD.SetActive(horseHUD);

            trackedQuestHUD.gameObject.SetActive(QuestManager.instance.aQuestIsTracked);
            MoveQuestHUD(true); // true means in frame

            waypointCanvas.enabled = true;

            MoveUIElements(false);

            selectText.text = "";
            select.SetActive(false);
            backText.text = "";
            back.SetActive(false);

            if(playAudio) uiAudioManager.PlayCloseMenuAudio();

            // Clear stat previews
            playerStats.DisableAllStatPreviews();

            // Reset this gameobject (useful for when we fast travel and need to make sure the map menu knows it)
            lastSelectedObjectBeforeMap = null;

            skillTreeManager.Notification("OFF");

            HelpMenu.instance.DisplayQueued(); // if any
        }

        public void OpenMapMenu()
        {
            // Get Potential Boss Status Canvas to Hide
            bossStatusCanvases = GameObject.FindGameObjectsWithTag("BossStatusCanvas");
            for (int i = 0; i < bossStatusCanvases.Length; i++)
            {
                Canvas canvas = bossStatusCanvases[i].GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.enabled = false;
                }
                else
                {
                    Debug.LogWarning($"GameObject {bossStatusCanvases[i].name} does not have a Canvas component.");
                }
            }

            // Hide still frame from player menu
            background.SetActive(false);
            HelpMenu.instance.Close();

            // Pause and Hide any tutorial notices
            controlsNotice.SetPause(true);

            // Stop rendering on main camera to help performance
            mainCamera.cullingMask = 0;

            interactContainerWasActive = interactContainer.activeSelf;
            interactContainer.SetActive(false);

            inputHandler.mapMenuOpen = true;

            mapInputHandler.enabled = true;
            mapInputHandler.SetMapCameraActive(true);

            // instead of closing the plaeyr menu, we want to hide it and keep a record of the last button that was selected
            if (playerMenuWindow.activeSelf)
            {
                playerMenuWasOpen = true;
                lastSelectedObjectBeforeMap = EventSystem.current.currentSelectedGameObject;
                playerMenuWindow.SetActive(false);
            }
            else
            {
                playerMenuWasOpen = false;
                lastSelectedObjectBeforeMap = null;
            }

            //open controller HUD
            controllerHUD.SetActive(true);
            //pause game
            Time.timeScale = 0;

            statusHUD.gameObject.SetActive(false);

            mapMenuWindow.SetActive(true);

            SetSelectedInventorySlot();

            goldHUDObj.SetActive(false);
            consumablesHUD.gameObject.SetActive(false);
            horseStatusHUD.SetActive(false);
            minimapWindow.SetActive(false);
            spellsHUD.gameObject.SetActive(false);
            levelUpIconInGame.SetActive(false);

            waypointCanvas.enabled = false;

            skillTreeManager.Notification("OFF");

            trackedQuestHUD.gameObject.SetActive(QuestManager.instance.aQuestIsTracked);
            MoveQuestHUD(true, true, true); // true means in frame

            uiAudioManager.PlayOpenMenuAudio();
        }

        public void CloseMapMenu(bool playAudio = true)
        {
            // Renable any boss canvases
            if (bossStatusCanvases != null)
            {
                for (int i = 0; i < bossStatusCanvases.Length; i++)
                {
                    Canvas canvas = bossStatusCanvases[i]?.GetComponent<Canvas>();
                    if (canvas != null)
                    {
                        canvas.enabled = true;
                    }
                    else
                    {
                        Debug.LogWarning($"GameObject {bossStatusCanvases[i].name} does not have a Canvas component.");
                    }
                }
                bossStatusCanvases = null;
            }

            // Stop rendering on main camera to help performance
            mainCamera.cullingMask = originalCullingMask;

            // Set interact container (the UI for interacting such as "Activate",etc) based on its state prior to opening mapm
            interactContainer.SetActive(interactContainerWasActive);

            mapInputHandler.SetMapCameraActive(false);
            mapInputHandler.enabled = false;

            // Pause and Hide any tutorial notices
            controlsNotice.SetPause(false);
            HelpMenu.instance.Close();

            // HUD changes to perform regardless of returning to player menu or not
            mapMenuWindow.SetActive(false);
            statusHUD.gameObject.SetActive(true);
            spellsHUD.gameObject.SetActive(true);
            consumablesHUD.gameObject.SetActive(true);

            if(playAudio) uiAudioManager.PlayCloseMenuAudio();

            // If player menu was open when we opened the map, resume where we were
            if (playerMenuWasOpen)
            {
                // Redisplay player menu still frame
                background.SetActive(true);
                playerMenuWindow.SetActive(true);
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(lastSelectedObjectBeforeMap);
                lastSelectedObjectBeforeMap = null;
                MoveQuestHUD(false); // False means out of frame
            }
            else // back to normal gameplay
            {
                // Hide still frame from player menu
                background.SetActive(false);
                waypointCanvas.enabled = true;
                Time.timeScale = 1;
                selectText.text = "";
                select.SetActive(false);
                backText.text = "";
                back.SetActive(false);
                minimapWindow.SetActive(true);
                bool horseHUD = horseAIVer.activeSelf || horseMountVer.activeSelf;
                horseStatusHUD.SetActive(horseHUD);
                if (PlayerStats.instance.remainingPoints > 0) levelUpIconInGame.SetActive(true);

                //SpellsHUDManager.instance.UpdateSpellHUD();

                controllerHUD.SetActive(false);
                //acquiredNotifications.SetActive(true);
                goldHUDObj.SetActive(false);
                MoveQuestHUD(true, false, false);
            }
            inputHandler.mapMenuOpen = false;
        }

        public void SetMenuBackground(bool enabled)
        {
            if (enabled)
            {
                //Take snapshot, display snapshot as background, stop rendering to help performance in menu
                CaptureCameraSnapshot();
                background.SetActive(true);
                mainCamera.cullingMask = 0;

                // Set width of background. Added to make background aspect ratio compatible with different ratios that aren't 16:9
                // this basically maintains the ratio of the 16:9 image, but expands it beyond the game window to fit perfectly
                RectTransform backgroundRect = background.GetComponent<RectTransform>();
                //Debug.Log("Height:" + backgroundRect.rect.height);
                float width = backgroundRect.rect.height * (16f / 9f);
                //Debug.Log("Calculated Width:" + width);
                backgroundRect.sizeDelta = new Vector2(width, backgroundRect.sizeDelta.y);
            }
            else
            {
                // Disable background, and restore camera rendering
                mainCamera.cullingMask = originalCullingMask;
                background.SetActive(false);
            }
        }

        /* Used to update background from inside the player menu, mainly when changing game resolution / aspect ratio*/
        public void CaptureCameraSnapshotFromMenu()
        {
            // Enable camera rendering
            mainCamera.cullingMask = originalCullingMask;
            // Take snaphot
            CaptureCameraSnapshot();
            // Disable rendering
            mainCamera.cullingMask = 0;
            // Set width of background. Added to make background aspect ratio compatible with different ratios that aren't 16:9
            // this basically maintains the ratio of the 16:9 image, but expands it beyond the game window to fit perfectly
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            //Debug.Log("Height:" + backgroundRect.rect.height);
            float width = backgroundRect.rect.height * (16f / 9f);
            //Debug.Log("Calculated Width:" + width);
            backgroundRect.sizeDelta = new Vector2(width, backgroundRect.sizeDelta.y);

        }

        public void SwitchMenuTab(string direction)
        {

            if (direction == "left")
            {
                if (currentTabIndex - 1 > -1)
                {
                    menuTabs[currentTabIndex - 1].GetComponent<Button>().onClick.Invoke();
                }

            }
            else if (direction == "right")
            {
                if (currentTabIndex + 1 < 4)
                {
                    menuTabs[currentTabIndex + 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else
            {
                Debug.Log("Direction from SwitchMenuTab(string direction) via PlayerMenuManager.cs not recgomized");
            }
        }

        public void UpdateMenuTab(int newIndex)
        {
            playerStats.DisableAllStatPreviews();
            currentTabIndex = newIndex;
            if (currentTabIndex == 0)
            {
                SetSelectedInventorySlot();
            }
            else if (currentTabIndex == 1)
            {
                //Debug.Log("currentSkillTabIndex:" + currentSkillTabIndex);
                skillTabs[currentSkillTabIndex].GetComponent<Button>().onClick.Invoke();
                SetSelectedSkillSlot();
            }
            uiAudioManager.PlaySwitchMenuTabAudio();

        }

        public void SwitchInventoryTab(string direction)
        {
            if (direction == "left" && inventoryWindow.activeSelf)
            {
                if (currentInventoryTabIndex - 1 > -1)
                {
                    inventoryTabs[currentInventoryTabIndex - 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else if (direction == "left" && skillWindow.activeSelf)
            {
                if (currentSkillTabIndex - 1 > -1)
                {
                    skillTabs[currentSkillTabIndex - 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else if (direction == "left" && questWindow.activeSelf)
            {
                int index = 0;
                questTabs[index].GetComponent<Button>().onClick.Invoke();
            }
            else if (direction == "left" && optionsWindow.activeSelf)
            {
                if (currentOptionsTabIndex - 1 > -1)
                {
                    optionsTabs[currentOptionsTabIndex - 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else if (direction == "right" && inventoryWindow.activeSelf)
            {
                if (currentInventoryTabIndex + 1 < 6)
                {
                    inventoryTabs[currentInventoryTabIndex + 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else if (direction == "right" && skillWindow.activeSelf)
            {
                if (currentSkillTabIndex + 1 < 3)
                {
                    skillTabs[currentSkillTabIndex + 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else if (direction == "right" && questWindow.activeSelf)
            {
                int index = 1;
                questTabs[index].GetComponent<Button>().onClick.Invoke();
            }
            else if (direction == "right" && optionsWindow.activeSelf)
            {
                if (currentOptionsTabIndex + 1 < 6)
                {
                    optionsTabs[currentOptionsTabIndex + 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else
            {
                Debug.Log("Inv Tab not active or issue with Direction from SwitchMenuTab(string direction) via PlayerMenuManager.cs not recgomized");
                return;
            }
        }

        public void UpdateInventoryTab(int newIndex)
        {
            playerStats.DisableAllStatPreviews();
            currentInventoryTabIndex = newIndex;
            SortInventory();
            UpdateUI();
            SetSelectedInventorySlot();
            ResetScrollbars();
            uiAudioManager.PlaySwitchSubMenuTabAudio();
        }

        public void UpdateSkillsTab(int newIndex)
        {
            playerStats.DisableAllStatPreviews();
            currentSkillTabIndex = newIndex;
            SetSelectedSkillSlot();
            uiAudioManager.PlaySwitchSubMenuTabAudio();
        }

        public void UpdateQuestsTab(int newIndex)
        {
            SortInventory();
            UpdateUI();
            SetSelectedQuestSlot(newIndex);
            ResetScrollbars();
            uiAudioManager.PlaySwitchSubMenuTabAudio();
        }

        public void UpdateOptionsTab(int newIndex)
        {
            currentOptionsTabIndex = newIndex;
            ResetScrollbars();
            uiAudioManager.PlaySwitchSubMenuTabAudio();
        }

        public void SetSelectedInventorySlot()
        {
            //if (!ControllerUIManager.instance.isUsingController()) return;
            //If controller is detected, set current selected button to the first item slot of the current inventory tab
            if (inventoryContents[currentInventoryTabIndex].transform.childCount > 0)
            {
                //First Child is "WeaponSlot" -> First Child of "WeaponSlot" is the gameObject containing the button to select
                //Select the button:
                //Debug.Log(currentInventoryTabIndex);
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(inventoryContents[currentInventoryTabIndex].transform.GetChild(0).transform.GetChild(0).gameObject, setRegardlessOfControls:true);
                //Now, trigger the animation of being selected (by turning on the selected window gameObject):
                inventoryContents[currentInventoryTabIndex].transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
            }
        }

        public void SetSelectedQuestSlot(int activeQuests)
        {
            //if (!ControllerUIManager.instance.isUsingController()) return;
            //If controller is detected, set current selected button to the first item slot of the current inventory tab
            if (questContents[activeQuests].transform.childCount > 0)
            {
                //First Child is "WeaponSlot" -> First Child of "WeaponSlot" is the gameObject containing the button to select
                //Select the button:
                //Debug.Log(currentInventoryTabIndex);
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(questContents[activeQuests].transform.GetChild(0).transform.GetChild(1).gameObject, setRegardlessOfControls: true);
                //Now, trigger the animation of being selected (by turning on the selected window gameObject):
                questContents[activeQuests].transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        public void SetSelectedSkillSlot()
        {
            //if (!ControllerUIManager.instance.isUsingController()) return;
            //If controller is detected, set current selected button to the first skill slot of the current skill tab
            if (skillContents[currentSkillTabIndex].transform.childCount > 0)
            {
                //First Child is "Skill00" -> Second Child of "Skill00" is the gameObject containing the button to select
                //Select the button:
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(skillContents[currentSkillTabIndex].transform.GetChild(0).transform.GetChild(0).transform.GetChild(1).gameObject, setRegardlessOfControls: true);
                // Needed to manually force Select function on Skill Button due to what I am assuming is a Unity bug not triggering OnSelect when using EventSystem.current.SetSelectedGameObject
                skillContents[currentSkillTabIndex].transform.GetChild(0).transform.GetChild(0).transform.GetChild(1).GetComponent<SkillButton>().UpdateDisplay();
            }
            else
            {
                Debug.LogError("HUH?");
            }
        }

        public void CheckAndSetLevelUpNotification()
        {
            if(InputHandler.instance.swapSpellWindow.activeSelf 
                || InputHandler.instance.swapRingWindow.activeSelf 
                || InputHandler.instance.optionsWindow.activeSelf
                || InputHandler.instance.questsWindow.activeSelf
                || noteDisplayPlayerMenu.activeSelf)
            {
                levelUpNotification.SetActive(false);
                return;
            }

            levelUpNotification.SetActive(PlayerStats.instance.remainingPoints > 0);
        }

        public void OpenLevelUpMenu()
        {
            if(levelUpNotification.activeSelf)
            {
                UIChangeSelectedButton.instance.RememberLastButton();
                levelUpButton.onClick.Invoke();
                uiAudioManager.PlayOpenMenuAudio();
            }
            else
            {
                Debug.Log("Level Up notification absent. Cannot open level up menu");
            }

        }

        public void MoveUIElements(bool menuOpened)
        {
            if (menuOpened)
            {
                statusBars.anchoredPosition = new Vector2(294.0f, 154.0f);
                statusBars.anchorMin = new Vector2(0.0f, 0.0f); // Anchored to the lower-left corner
                statusBars.anchorMax = new Vector2(0.0f, 0.0f); // Anchored to the lower-left corner

                xpBar.anchoredPosition = new Vector2(120.0f, 154.0f);
                xpBar.anchorMin = new Vector2(0.0f, 0.0f); // Anchored to the lower-left corner
                xpBar.anchorMax = new Vector2(0.0f, 0.0f); // Anchored to the lower-left corner

                consumablesHUD.anchoredPosition = new Vector2(-233.0f, 160.0f);
                consumablesHUD.anchorMin = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center
                consumablesHUD.anchorMax = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center

                statusHUD.anchoredPosition = new Vector2(-961.8f, 0.0f);
                statusHUD.anchorMin = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center
                statusHUD.anchorMax = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center

                spellsHUD.anchoredPosition = new Vector2(458.0f, 157.8f);
                spellsHUD.anchorMin = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center
                spellsHUD.anchorMax = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center

                goldHUD.anchoredPosition = new Vector2(848.0f, 1004.0f);
                goldHUD.anchorMin = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center
                goldHUD.anchorMax = new Vector2(0.5f, 0.0f); // Anchored to the bottom-center

                textNotifications.anchoredPosition = inMenuTextNotificationsPosition;

                float xOffset = 0;
                if ((float)Screen.width / (float)Screen.height > 2f) xOffset = 300f;
                acquNotifications.anchoredPosition = new Vector2(inMenuAcquNotificationsPosition.x + xOffset, inMenuAcquNotificationsPosition.y);
                acquNotifications.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }
            else
            {
                statusBars.anchoredPosition = new Vector2(230.0f, -110.0f);
                statusBars.anchorMin = new Vector2(0.0f, 1.0f); // Anchored to the top-left corner
                statusBars.anchorMax = new Vector2(0.0f, 1.0f); // Anchored to the top-left corner

                xpBar.anchoredPosition = new Vector2(52.0f, -110.0f);
                xpBar.anchorMin = new Vector2(0.0f, 1.0f); // Anchored to the top-left corner
                xpBar.anchorMax = new Vector2(0.0f, 1.0f); // Anchored to the top-left corner

                consumablesHUD.anchoredPosition = new Vector2(177.0f, 160.0f);
                consumablesHUD.anchorMin = new Vector2(0.0f, 0.0f); // Anchored to the lower-left corner
                consumablesHUD.anchorMax = new Vector2(0.0f, 0.0f); // Anchored to the lower-left corner

                statusHUD.anchoredPosition = new Vector2(0f, 0f);
                statusHUD.anchorMin = new Vector2(0.0f, 0.0f); // Anchored to the lower-left corner
                statusHUD.anchorMax = new Vector2(1.0f, 1.0f); // Anchored to the lower-left corner

                spellsHUD.anchoredPosition = new Vector2(-502.0f, 157.8f);
                spellsHUD.anchorMin = new Vector2(1.0f, 0.0f); // Anchored to the bottom-right
                spellsHUD.anchorMax = new Vector2(1.0f, 0.0f); // Anchored to the bottom-right

                goldHUD.anchoredPosition = new Vector2(-112.0f, -76.0f);
                goldHUD.anchorMin = new Vector2(1.0f, 1.0f); // Anchored to the top-right
                goldHUD.anchorMax = new Vector2(1.0f, 1.0f); // Anchored to the top-right

                textNotifications.anchoredPosition = originalTextNotificationsPosition;
                acquNotifications.anchoredPosition = originalAcquNotificationsPosition;
                acquNotifications.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            }
        }

        public void ResetScrollbars()
        {
            // Usually, scroll bars are set properly from the autoscroll script, however if
            // in some cases, such as torso armor being absent, a slot can be selected while
            // the scrollbars isn't properly synced, this will prevent any such case
            foreach (Scrollbar sb in scrollbars)
            {
                sb.value = 1.0f;
            }
            foreach (GameObject contents in inventoryContents)
            {
                Vector2 currentPosition = contents.GetComponent<RectTransform>().anchoredPosition;
                contents.GetComponent<RectTransform>().anchoredPosition = new Vector2(currentPosition.x, 0);
            }
            foreach (GameObject contents in questContents)
            {
                Vector2 currentPosition = contents.GetComponent<RectTransform>().anchoredPosition;
                contents.GetComponent<RectTransform>().anchoredPosition = new Vector2(currentPosition.x, 0);
            }
        }

        public void SelectQuestTab()
        {
            // This function is called when the Quest Menu Tab is selected, which helps select the first slot of whatever quest tab (active/completed) is active
            // The function is assigned in the inspector ON the quest menu tab button
            SortInventory();
            UpdateUI();
            int index = 0;
            if (!activeQuestsContents.activeSelf) index = 1;
            questTabs[index].GetComponent<Button>().onClick.Invoke();
            SetSelectedQuestSlot(index);
        }

        public void SelectOptionsTab()
        {
            // This function is called when the Options tab is selected (technically pressed), which helps select the first slot of whatever options tab is active
            // The function is assigned in the inspector ON the options menu tab button
            optionsTabs[currentOptionsTabIndex].GetComponent<Button>().onClick.Invoke();
        }

        public void MoveQuestHUD(bool inframe, bool forceHideUpdateText=false, bool map=false)
        {
            if (inframe)
            {
                if (map)
                {
                    trackedQuestVerticalLayoutGroup.enabled = false;
                    trackedQuestHUD.anchoredPosition = new Vector2(-160f, -246f);
                }
                else
                {
                    trackedQuestVerticalLayoutGroup.enabled = true;
                    trackedQuestHUD.anchoredPosition = originalPositionTrackedQuestHUD;
                }

                if (!forceHideUpdateText)
                {
                    questUpdateText.anchoredPosition = originalPositionQuestUpdateText;
                }
                else
                {
                    questUpdateText.anchoredPosition = new Vector2(2500f, 3000f);
                }

            }
            else
            {
                trackedQuestVerticalLayoutGroup.enabled = false;
                trackedQuestHUD.anchoredPosition = new Vector2(2500f, 3000f);
                questUpdateText.anchoredPosition = new Vector2(2500f, 3000f);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(trackedQuestVerticalLayoutGroup.GetComponent<RectTransform>());
        }

        public void MoveQuestHUDForSliderInOptionsMenu(bool inframe)
        {
            if(inframe)
            {
                trackedQuestHUD.gameObject.SetActive(true);
            }
            MoveQuestHUD(inframe, true, false);
        }

        public void CaptureCameraSnapshot()
        {
            if (mainCamera != null && preMenuSnapshot != null)
            {
                // Set the mainCameraera's target texture to the render texture
                mainCamera.targetTexture = preMenuSnapshot;

                // Render the mainCameraera's view to the render texture
                mainCamera.Render();

                // Reset the mainCameraera's target texture
                mainCamera.targetTexture = null;
            }
        }

        public void DisplayNote(KeyItem keyItem, bool display, bool showItemInfoBox=true, bool showStatsBox=true, bool showLevelUp=true)
        {     
            if(keyItem == null || keyItem.note == null || !display)
            {
                itemInfoBox.SetActive(showItemInfoBox);
                statsBox.SetActive(showItemInfoBox);
                noteDisplayPlayerMenu.SetActive(false);
                if(showLevelUp) CheckAndSetLevelUpNotification();
            }
            else
            {
                itemInfoBox.SetActive(false);
                statsBox.SetActive(false);
                noteNameText.text = keyItem.itemName;

                noteManager.SetText(keyItem.note, titleText, bodyText);
                noteManager.SetImage(keyItem.note, noteImage);

                noteDisplayPlayerMenu.SetActive(true);
                levelUpNotification.SetActive(false);
            }
        }
    }
}
