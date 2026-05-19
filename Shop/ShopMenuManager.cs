using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace etchebarren
{
    public class ShopMenuManager : MonoBehaviour
    {

        /* Add new shops:
        1. create lists for item inventory like under [Header("STALWART CAMP SHOP INVENTORY")]
        2. create refresh function like RefreshStalwartCampShop()
        3. add new shop to UpdateShopUI()
         * */
        public static ShopMenuManager instance;

        public PlayerInventory playerInventory;
        public PlayerMenuManager playerMenuManager;
        public OptionsManager optionsManager;
        public ControllerUIManager controllerUIManager;

        public GameObject shopMenuWindow;

        public GameObject controllerHUD;

        public TextMeshProUGUI shopNameText;

        public PlayerStats playerStats;
        public SetHUD setHUD;
        public UIAudioManager uiAudioManager;

        public bool buying = true;

        [Header("Reset Timer")]
        public float resetIntervalInSeconds = 900.0f;
        public float timeRemaining = 0f;
        public TextMeshProUGUI shopResetTimerText;

        [Header("Single Item purchase window:")]
        public TextMeshProUGUI purchaseWindowText;
        public GameObject purchaseWindow;
        public GameObject cancelButton; //Used as default

        [Header("Quantity purchase window:")]
        public int purchaseQuantity = 1;
        public GameObject quantityWindow;
        public Slider quantitySlider;

        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI maximumText;
        public TextMeshProUGUI totalCostText;

        [Header("DO NOT SET MANUALLY:")]
        public Item selectedItem;
        public string shopName;
        public GameObject lastSelectedButton;
        public ShopTrigger currentShopTrigger;

        // Set shop inventories to use based on shopID
        List<WeaponItem> weaponsInventory;
        List<ShieldItem> shieldsInventory;
        List<TorsoArmorItem> torsoArmorInventory;
        List<HandsArmorItem> handsArmorInventory;
        List<LegsArmorItem> legsArmorInventory;
        List<RingItem> ringsInventory;
        List<AmuletItem> amuletsInventory;
        List<Spell> spellsInventory;
        List<Consumable> consumablesInventory;
        List<KeyItem> keyItemsInventory;

        // Set shop inventories to use based on shopID
        List<WeaponItem> shopWeaponsInventory;
        List<ShieldItem> shopShieldsInventory;
        List<TorsoArmorItem> shopTorsoArmorInventory;
        List<HandsArmorItem> shopHandsArmorInventory;
        List<LegsArmorItem> shopLegsArmorInventory;
        List<RingItem> shopRingsInventory;
        List<AmuletItem> shopAmuletsInventory;
        List<Spell> shopSpellsInventory;
        List<Consumable> shopConsumablesInventory;
        List<KeyItem> shopKeyItemsInventory;

        [Header("STALWART CAMP SHOP INVENTORY")]
        public List<WeaponItem> weaponsInventory_0 = new List<WeaponItem>();
        public List<ShieldItem> shieldsInventory_0 = new List<ShieldItem>();
        public List<TorsoArmorItem> torsoArmorInventory_0 = new List<TorsoArmorItem>();
        public List<HandsArmorItem> handsArmorInventory_0 = new List<HandsArmorItem>();
        public List<LegsArmorItem> legsArmorInventory_0 = new List<LegsArmorItem>();
        public List<RingItem> ringsInventory_0 = new List<RingItem>();
        public List<AmuletItem> amuletsInventory_0 = new List<AmuletItem>();
        public List<Spell> spellsInventory_0 = new List<Spell>();
        public List<Consumable> consumablesInventory_0 = new List<Consumable>();
        public List<KeyItem> keyItemsInventory_0 = new List<KeyItem>();

        [Header("References for moving Text Notifications")]
        public RectTransform textNotifications;
        public RectTransform acquiredNotifications;
        public Vector3 inMenuTextNotificationsPosition = new Vector3(-838f, 985f, 0f);
        public Vector3 inMenuAcquiredNotificationsPosition = new Vector3(123f, 1017f, 0f);

        [Header("References for Refreshing Menu")]
        public GameObject itemInfoBox;
        [HideInInspector] public GameObject lastSelectedBorder;
        public GameObject equipRatingDescription;
        public GameObject primaryStatDescription;
        public GameObject secondaryStatsDescription;
        private Color hidden = new Color(0.37f, 0.27f, 0.15f, 0.0f);
        [HideInInspector] public Image lastStatButtonImage;
        public TextMeshProUGUI playerNameAndLevelText;
        public Scrollbar[] scrollbars;

        [Header("References for switching menu tabs")]
        public int currentTabIndex;
        public GameObject[] menuTabs;
        public int currentInventoryTabIndex;
        public GameObject[] inventoryTabs;
        public GameObject[] inventoryContents;
        public GameObject inventoryWindow;

        [Header("UI Elements to Hide if no gear of that type is present in that shop")]
        public GameObject breastplateText;
        public GameObject gauntletsText;
        public GameObject greavesText;
        public GameObject ringsText;
        public GameObject amuletsText;

        [Header("UI elements to hide/show on close/open playerMenu")]
        public GameObject goldHUDObj;
        public GameObject minimapWindow;

        [Header("UI elements to hide/show on purchase/quantity window")]
        public List<GameObject> hideOnPurchase;
        public List<GameObject> hideOnPurchaseController;
        public GameObject leaveButtonPC;

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

        public void Awake()
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

        public void Start()
        {
            FindInventorySlots();
            StartCoroutine(ShopResetTimer());
        }

        private IEnumerator ShopResetTimer()
        {
            while (timeRemaining > 0)
            {
                //unscaled delta time ignores timescale, works when paused
                timeRemaining -= Time.unscaledDeltaTime;

                shopResetTimerText.text = "Inventory Reset in: " + FormatTime(timeRemaining);

                yield return null;
            }

            timeRemaining = resetIntervalInSeconds;

            // Refresh the inventory of shops
            RefreshShop(0);

            if (shopMenuWindow.activeSelf && buying)
            {
                // Update the inventory slots of shop
                UpdateShopUI();

                // Close any popup windows related to buying from shop
                quantityWindow.SetActive(false);
                purchaseWindow.SetActive(false);

                // Display elements that were hidden when purchase window was displayed
                foreach (GameObject elem in hideOnPurchase)
                {
                    elem.SetActive(true);
                }
                // IF on PC, hide leave button
                if (!ControllerUIManager.instance.isUsingController())
                {
                    leaveButtonPC.SetActive(true);
                }
                else
                {
                    foreach (GameObject elem in hideOnPurchaseController)
                    {
                        elem.SetActive(true);
                    }
                }

                // Select a new inventory slot button IF shop is open
                SetSelectedInventorySlot();
            }

            // Start the timer again
            ResetShopTimer();
        }

        public void ResetShopTimer()
        {
            Debug.Log("Shop Timer Reset");
            StartCoroutine(ShopResetTimer());
        }

        string FormatTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            int secs = Mathf.FloorToInt(seconds % 60);

            return string.Format("{0}h {1}m {2}s", hours, minutes, secs);
        }

        public void SetLastSelected(GameObject selected)
        {
            lastSelectedButton = selected;
        }

        public void SetBuyingState(bool isBuying)
        {
            //Debug.Log("Set buying state called: buying = " + isBuying);
            buying = isBuying;

            UpdateShopUI();

            SetSelectedInventorySlot();
        }

        public void OpenPurchaseWindow(Item item)
        {
            // Check item count in shop before allowing purchase prompt
            if(item.count < 1)
            {
                return;
            }

            uiAudioManager.PlaySwitchMenuTabAudio();

            foreach (GameObject elem in hideOnPurchase)
            {
                elem.SetActive(false);
            }
            // IF on PC, hide leave button
            if (!ControllerUIManager.instance.isUsingController())
            {
                leaveButtonPC.SetActive(false);
            }
            else
            {
                foreach (GameObject elem in hideOnPurchaseController)
                {
                    elem.SetActive(false);
                }
            }

            selectedItem = item;
            itemInfoBox.SetActive(true);
            purchaseQuantity = 1;

            // If the selected item to purchase has quantity
            if(selectedItem.count > 1)
            {
                Debug.Log("Quantity");
                itemNameText.text = selectedItem.itemName + " (Value: " + selectedItem.goldValue.ToString("N0") + ")";
                maximumText.text = selectedItem.count.ToString("N0");
                quantitySlider.maxValue = selectedItem.count;
                quantitySlider.value = 1;
                if (buying)
                {
                    totalCostText.text = "Purchase <b>" + purchaseQuantity.ToString("N0") + "</b> for <b>" + (selectedItem.goldValue * purchaseQuantity).ToString("N0") + "</b> Gold?";
                }
                else
                {
                    totalCostText.text = "Sell <b>" + purchaseQuantity.ToString("N0") + "</b> for <b>" + (selectedItem.goldValue * purchaseQuantity).ToString("N0") + "</b> Gold?";
                }

                quantityWindow.SetActive(true);
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(quantitySlider.gameObject);
            }
            // A singular item was selected
            else
            {
                Debug.Log("Single");
                if (buying)
                {
                    purchaseWindowText.text = "Purchase <i>" + selectedItem.itemName + "</i> for <b>" + selectedItem.goldValue.ToString("N0") + "</b> Gold?";
                }
                else
                {
                    purchaseWindowText.text = "Sell <i>" + selectedItem.itemName + "</i> for <b>" + selectedItem.goldValue.ToString("N0") + "</b> Gold?";
                }

                purchaseWindow.SetActive(true);
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(cancelButton);
            }
            Debug.Log("item to purchase count: " + selectedItem.count);

        }

        public void UpdatePurchaseQuantity()
        {
            purchaseQuantity = (int)quantitySlider.value;
            totalCostText.text = "Purchase <b>" + purchaseQuantity.ToString("N0") + "</b> for <b>" + (selectedItem.goldValue * purchaseQuantity).ToString("N0") + "</b> Gold?";
        }

        public void ConfirmTransaction()
        {
            if (buying)
            {
                ConfirmPurchase();
            }
            else
            {
                ConfirmSale();
            }
        }

        public void ConfirmPurchase()
        {
            // Confirm player has enough gold
            if(playerInventory.goldCount < (selectedItem.goldValue * purchaseQuantity))
            {
                TextNotificationsManager.instance.NewTextNotifaction("Not enough gold", true); //true prevents duplicate messages for 3 seconds
                return;
            }
            
            // Display elements that were hidden when purchase window was displayed
            foreach (GameObject elem in hideOnPurchase)
            {
                elem.SetActive(true);
            }
            // IF on PC, display leave button
            if (!ControllerUIManager.instance.isUsingController())
            {
                leaveButtonPC.SetActive(true);
            }
            // IF using controller, display controller elements such as button icons
            else
            {
                foreach (GameObject elem in hideOnPurchaseController)
                {
                    elem.SetActive(true);
                }
            }

            // Create a copy of the purchased item
            Item item = ScriptableObject.Instantiate(selectedItem);
            // Set the copy's count to the amount that was purchased
            item.count = purchaseQuantity;
            // Added modified item (fixed quantity) to player inventory
            playerInventory.AddToInventory(item);
            // Remove the item (or count) from shop inventory
            RemoveFromShopInventory(selectedItem, purchaseQuantity);
            // Hide purchase window(s)
            itemInfoBox.SetActive(false);
            purchaseWindow.SetActive(false);
            quantityWindow.SetActive(false);
            // Update UI to reflect updated shop inventory
            UpdateShopUI();
            // Remove gold from player inventory (also updates HUD)
            playerInventory.RemoveGold(selectedItem.goldValue * purchaseQuantity);
            // Select inventory slot after going back to shop inventory (from purchase window)
            SetSelectedInventorySlot();
            // Send an acquired notification
            AcquiredNotifications.instance.NewItemNotification(item, item.count);

            uiAudioManager.PlayBuyItemAudio();

        }

        public void ConfirmSale()
        {
            // Display elements that were hidden when purchase window was displayed
            foreach (GameObject elem in hideOnPurchase)
            {
                elem.SetActive(true);
            }
            // IF on PC, display leave button
            if (!ControllerUIManager.instance.isUsingController())
            {
                leaveButtonPC.SetActive(true);
            }
            // IF using controller, display controller elements such as button icons
            else
            {
                foreach (GameObject elem in hideOnPurchaseController)
                {
                    elem.SetActive(true);
                }
            }
            // First lets copy the item(s) to the shop inventory
            // Create a copy of the purchased item
            Item soldItem = ScriptableObject.Instantiate(selectedItem);
            // Set the copy's count to the amount that was purchased
            soldItem.count = purchaseQuantity;
            // Add the item (or update its count) to the selected shop
            AddToShopInventory(soldItem, purchaseQuantity);

            // Now, remove the item (or decrement) from player inventory
            playerInventory.RemoveFromInventory(selectedItem, purchaseQuantity);

            // Calculate and add gold to player's inventory after sale
            playerInventory.AddGold(soldItem.goldValue * soldItem.count);

            // Hide purchase window(s)
            purchaseWindow.SetActive(false);
            quantityWindow.SetActive(false);
            // Update UI to reflect updated shop inventory
            UpdateShopUI();
            // Select inventory slot after going back to shop inventory (from purchase window)
            SetSelectedInventorySlot();
            // Send an acquired notification, null sends a gold notification
            AcquiredNotifications.instance.NewItemNotification(null, soldItem.goldValue * soldItem.count);
            string message = "Sold " + soldItem.itemName + " x" + soldItem.count;
            TextNotificationsManager.instance.NewTextNotifaction(message, true); //true prevents duplicate messages for 3 seconds

            uiAudioManager.PlaySellItemAudio();
        }

        public void CancelTransaction()
        {
            foreach (GameObject elem in hideOnPurchase)
            {
                elem.SetActive(true);
            }
            // IF on PC, display leave button
            if (!ControllerUIManager.instance.isUsingController())
            {
                leaveButtonPC.SetActive(true);
            }
            else
            {
                foreach (GameObject elem in hideOnPurchaseController)
                {
                    elem.SetActive(true);
                }
            }

            purchaseWindow.SetActive(false);
            quantityWindow.SetActive(false);
            UIChangeSelectedButton.instance.ChangeSelectedButtonTo(lastSelectedButton);
        }

        public void RefreshShop(int shopID)
        {
            switch (shopID)
            {
                case 0:
                    RefreshStalwartCampShop();
                    break;
            }
        }

        public void RefreshStalwartCampShop()
        {
            // First clear all old inventory, we destroy before clearing to prevent memory leak
            foreach (WeaponItem item in weaponsInventory_0)
            {
                Destroy(item);
            }
            weaponsInventory_0.Clear();

            foreach (ShieldItem item in shieldsInventory_0)
            {
                Destroy(item);
            }
            shieldsInventory_0.Clear();

            foreach (TorsoArmorItem item in torsoArmorInventory_0)
            {
                Destroy(item);
            }
            torsoArmorInventory_0.Clear();

            foreach (HandsArmorItem item in handsArmorInventory_0)
            {
                Destroy(item);
            }
            handsArmorInventory_0.Clear();

            foreach (LegsArmorItem item in legsArmorInventory_0)
            {
                Destroy(item);
            }
            legsArmorInventory_0.Clear();

            foreach (RingItem item in ringsInventory_0)
            {
                Destroy(item);
            }
            ringsInventory_0.Clear();

            foreach (AmuletItem item in amuletsInventory_0)
            {
                Destroy(item);
            }
            amuletsInventory_0.Clear();

            foreach (Spell item in spellsInventory_0)
            {
                Destroy(item);
            }
            spellsInventory_0.Clear();

            foreach (Consumable item in consumablesInventory_0)
            {
                Destroy(item);
            }
            consumablesInventory_0.Clear();

            foreach (KeyItem item in keyItemsInventory_0)
            {
                Destroy(item);
            }
            keyItemsInventory_0.Clear();

            // Custom shop inventory here:

            for (int i = 0; i < 5; i++)
            {
                // Generate item and add it to appropriate shop inventory
                ItemGeneratorManager.instance.GenerateSword(playerStats.playerLevel + 2, playerStats.playerLevel - 2, Vector3.zero, "random", 0, false, weaponsInventory_0);

                ItemGeneratorManager.instance.GenerateShield(playerStats.playerLevel + 2, playerStats.playerLevel - 2, Vector3.zero, "random", 0, false, shieldsInventory_0);

                ItemGeneratorManager.instance.GenerateTorsoArmor(playerStats.playerLevel + 2, playerStats.playerLevel - 2, Vector3.zero, "random", 0, false, torsoArmorInventory_0);

                ItemGeneratorManager.instance.GenerateHandsArmor(playerStats.playerLevel + 2, playerStats.playerLevel - 2, Vector3.zero, "random", 0, false, handsArmorInventory_0);

                ItemGeneratorManager.instance.GenerateLegsArmor(playerStats.playerLevel + 2, playerStats.playerLevel - 2, Vector3.zero, "random", 0, false, legsArmorInventory_0);

                ItemGeneratorManager.instance.GenerateRing(playerStats.playerLevel + 2, playerStats.playerLevel - 2, Vector3.zero, "random", 0, false, ringsInventory_0);

                ItemGeneratorManager.instance.GenerateAmulet(playerStats.playerLevel + 2, playerStats.playerLevel - 2, Vector3.zero, "random", 0, false, amuletsInventory_0);

                ItemGeneratorManager.instance.GenerateConsumable(0, 1, Vector3.zero, false, consumablesInventory_0);
            }
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
        }

        public void UpdateShopUI()
        {
            //Update Player Name & Level in Stats Box
            playerNameAndLevelText.text = playerStats.playerName + " - Lv." + playerStats.playerLevel;

            // Determine shop
            switch (shopName)
            {
                case "Stalwart Camp Shop":
                    shopWeaponsInventory = weaponsInventory_0;
                    shopShieldsInventory = shieldsInventory_0;
                    shopTorsoArmorInventory = torsoArmorInventory_0;
                    shopHandsArmorInventory = handsArmorInventory_0;
                    shopLegsArmorInventory = legsArmorInventory_0;
                    shopRingsInventory = ringsInventory_0;
                    shopAmuletsInventory = amuletsInventory_0;
                    shopSpellsInventory = spellsInventory_0;
                    shopConsumablesInventory = consumablesInventory_0;
                    shopKeyItemsInventory = keyItemsInventory_0;
                    break;
                default:
                    shopWeaponsInventory = weaponsInventory_0;
                    shopShieldsInventory = shieldsInventory_0;
                    shopTorsoArmorInventory = torsoArmorInventory_0;
                    shopHandsArmorInventory = handsArmorInventory_0;
                    shopLegsArmorInventory = legsArmorInventory_0;
                    shopRingsInventory = ringsInventory_0;
                    shopAmuletsInventory = amuletsInventory_0;
                    shopSpellsInventory = spellsInventory_0;
                    shopConsumablesInventory = consumablesInventory_0;
                    shopKeyItemsInventory = keyItemsInventory_0;
                    break;
            }

            if (buying)
            {
                weaponsInventory = shopWeaponsInventory;
                shieldsInventory = shopShieldsInventory;
                torsoArmorInventory = shopTorsoArmorInventory;
                handsArmorInventory = shopHandsArmorInventory;
                legsArmorInventory = shopLegsArmorInventory;
                ringsInventory = shopRingsInventory;
                amuletsInventory = shopAmuletsInventory;
                spellsInventory = shopSpellsInventory;
                consumablesInventory = shopConsumablesInventory;
                keyItemsInventory = shopKeyItemsInventory;          
            }
            else
            {
                weaponsInventory = playerInventory.weaponsInventory;
                shieldsInventory = playerInventory.shieldsInventory;
                torsoArmorInventory = playerInventory.torsoArmorInventory;
                handsArmorInventory = playerInventory.handsArmorInventory;
                legsArmorInventory = playerInventory.legsArmorInventory;
                ringsInventory = playerInventory.ringsInventory;
                amuletsInventory = playerInventory.amuletsInventory;
                spellsInventory = playerInventory.spellsInventory;
                consumablesInventory = playerInventory.consumablesInventory;
                keyItemsInventory = playerInventory.keyItemsInventory;
            }

            SortEquippedSellableLevel();

            #region SHOP WEAPON INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < weaponInventorySlots.Length; i++)
            {
                weaponInventorySlots[i].ClearInventorySlot();
                weaponInventorySlots[i].gameObject.SetActive(false);
            }

            if (weaponsInventory.Count > 0) // If there is inventory, create the slots
            {
                weaponInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < weaponsInventory.Count; i++)
                {
                    if (i < weaponInventorySlots.Length)
                    {
                        // Reuse existing slot
                        weaponInventorySlots[i].gameObject.SetActive(true);
                        weaponInventorySlots[i].AddWeaponItem(weaponsInventory[i], true);
                        // Debug.Log($"Updated slot {i} with weapon {weaponsInventory[i].name}");
                    }
                    else
                    {
                        // Debug.Log($"Created new slot slot {i} with weapon {weaponsInventory[i].name}");

                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(weaponInventorySlotPrefab, weaponInventorySlotsParent);
                        WeaponInventorySlot newSlotComponent = newSlot.GetComponent<WeaponInventorySlot>();
                        newSlotComponent.AddWeaponItem(weaponsInventory[i], true);

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

            #region SHOP SHIELD INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < shieldInventorySlots.Length; i++)
            {
                shieldInventorySlots[i].ClearInventorySlot();
                shieldInventorySlots[i].gameObject.SetActive(false);
            }

            if (shieldsInventory.Count > 0) // If there is inventory, create the slots
            {
                shieldInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < shieldsInventory.Count; i++)
                {
                    if (i < shieldInventorySlots.Length)
                    {
                        // Reuse existing slot
                        shieldInventorySlots[i].gameObject.SetActive(true);
                        shieldInventorySlots[i].AddShieldItem(shieldsInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(shieldInventorySlotPrefab, shieldInventorySlotsParent);
                        ShieldInventorySlot newSlotComponent = newSlot.GetComponent<ShieldInventorySlot>();
                        newSlotComponent.AddShieldItem(shieldsInventory[i], true);

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

            #region SHOP TORSO ARMOR INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < torsoArmorInventorySlots.Length; i++)
            {
                torsoArmorInventorySlots[i].ClearInventorySlot();
                torsoArmorInventorySlots[i].gameObject.SetActive(false);
            }

            if (torsoArmorInventory.Count > 0) // If there is inventory, create the slots
            {
                breastplateText.SetActive(true);
                torsoArmorInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < torsoArmorInventory.Count; i++)
                {
                    if (i < torsoArmorInventorySlots.Length)
                    {
                        // Reuse existing slot
                        torsoArmorInventorySlots[i].gameObject.SetActive(true);
                        torsoArmorInventorySlots[i].AddTorsoArmorItem(torsoArmorInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(torsoArmorInventorySlotPrefab, torsoArmorInventorySlotsParent);
                        TorsoArmorInventorySlot newSlotComponent = newSlot.GetComponent<TorsoArmorInventorySlot>();
                        newSlotComponent.AddTorsoArmorItem(torsoArmorInventory[i], true);

                        // Update the slots array
                        List<TorsoArmorInventorySlot> slotsList = new List<TorsoArmorInventorySlot>(torsoArmorInventorySlots);
                        slotsList.Add(newSlotComponent);
                        torsoArmorInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                breastplateText.SetActive(false);
                torsoArmorInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region SHOP HANDS ARMOR INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < handsArmorInventorySlots.Length; i++)
            {
                handsArmorInventorySlots[i].ClearInventorySlot();
                handsArmorInventorySlots[i].gameObject.SetActive(false);
            }

            if (handsArmorInventory.Count > 0) // If there is inventory, create the slots
            {
                gauntletsText.SetActive(true);
                handsArmorInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < handsArmorInventory.Count; i++)
                {
                    if (i < handsArmorInventorySlots.Length)
                    {
                        // Reuse existing slot
                        handsArmorInventorySlots[i].gameObject.SetActive(true);
                        handsArmorInventorySlots[i].AddHandsArmorItem(handsArmorInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(handsArmorInventorySlotPrefab, handsArmorInventorySlotsParent);
                        HandsArmorInventorySlot newSlotComponent = newSlot.GetComponent<HandsArmorInventorySlot>();
                        newSlotComponent.AddHandsArmorItem(handsArmorInventory[i], true);

                        // Update the slots array
                        List<HandsArmorInventorySlot> slotsList = new List<HandsArmorInventorySlot>(handsArmorInventorySlots);
                        slotsList.Add(newSlotComponent);
                        handsArmorInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                gauntletsText.SetActive(false);
                handsArmorInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region SHOP LEGS ARMOR INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < legsArmorInventorySlots.Length; i++)
            {
                legsArmorInventorySlots[i].ClearInventorySlot();
                legsArmorInventorySlots[i].gameObject.SetActive(false);
            }

            if (legsArmorInventory.Count > 0) // If there is inventory, create the slots
            {
                greavesText.SetActive(true);
                legsArmorInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < legsArmorInventory.Count; i++)
                {
                    if (i < legsArmorInventorySlots.Length)
                    {
                        // Reuse existing slot
                        legsArmorInventorySlots[i].gameObject.SetActive(true);
                        legsArmorInventorySlots[i].AddLegsArmorItem(legsArmorInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(legsArmorInventorySlotPrefab, legsArmorInventorySlotsParent);
                        LegsArmorInventorySlot newSlotComponent = newSlot.GetComponent<LegsArmorInventorySlot>();
                        newSlotComponent.AddLegsArmorItem(legsArmorInventory[i], true);

                        // Update the slots array
                        List<LegsArmorInventorySlot> slotsList = new List<LegsArmorInventorySlot>(legsArmorInventorySlots);
                        slotsList.Add(newSlotComponent);
                        legsArmorInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                greavesText.SetActive(false);
                legsArmorInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region SHOP RING INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < ringInventorySlots.Length; i++)
            {
                ringInventorySlots[i].ClearInventorySlot();
                ringInventorySlots[i].gameObject.SetActive(false);
            }

            if (ringsInventory.Count > 0) // If there is inventory, create the slots
            {
                ringsText.SetActive(true);
                ringInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < ringsInventory.Count; i++)
                {
                    if (i < ringInventorySlots.Length)
                    {
                        // Reuse existing slot
                        ringInventorySlots[i].gameObject.SetActive(true);
                        ringInventorySlots[i].AddRingItem(ringsInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(ringInventorySlotPrefab, ringInventorySlotsParent);
                        RingInventorySlot newSlotComponent = newSlot.GetComponent<RingInventorySlot>();
                        newSlotComponent.AddRingItem(ringsInventory[i], true);

                        // Update the slots array
                        List<RingInventorySlot> slotsList = new List<RingInventorySlot>(ringInventorySlots);
                        slotsList.Add(newSlotComponent);
                        ringInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                ringsText.SetActive(false);
                ringInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region SHOP AMULETS INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < amuletInventorySlots.Length; i++)
            {
                amuletInventorySlots[i].ClearInventorySlot();
                amuletInventorySlots[i].gameObject.SetActive(false);
            }

            if (amuletsInventory.Count > 0) // If there is inventory, create the slots
            {
                amuletsText.SetActive(true);
                amuletInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < amuletsInventory.Count; i++)
                {
                    if (i < amuletInventorySlots.Length)
                    {
                        // Reuse existing slot
                        amuletInventorySlots[i].gameObject.SetActive(true);
                        amuletInventorySlots[i].AddAmuletItem(amuletsInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(amuletInventorySlotPrefab, amuletInventorySlotsParent);
                        AmuletInventorySlot newSlotComponent = newSlot.GetComponent<AmuletInventorySlot>();
                        newSlotComponent.AddAmuletItem(amuletsInventory[i], true);

                        // Update the slots array
                        List<AmuletInventorySlot> slotsList = new List<AmuletInventorySlot>(amuletInventorySlots);
                        slotsList.Add(newSlotComponent);
                        amuletInventorySlots = slotsList.ToArray();
                    }
                }
            }
            else
            {
                amuletsText.SetActive(false);
                amuletInventorySlotPrefab.SetActive(false); // Hide unused slot template 
            }
            #endregion

            #region SHOP SPELLS INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < spellInventorySlots.Length; i++)
            {
                spellInventorySlots[i].ClearInventorySlot();
                spellInventorySlots[i].gameObject.SetActive(false);
            }

            if (spellsInventory.Count > 0) // If there is inventory, create the slots
            {
                spellInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < spellsInventory.Count; i++)
                {
                    if (i < spellInventorySlots.Length)
                    {
                        // Reuse existing slot
                        spellInventorySlots[i].gameObject.SetActive(true);
                        spellInventorySlots[i].AddSpell(spellsInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(spellInventorySlotPrefab, spellInventorySlotsParent);
                        SpellInventorySlot newSlotComponent = newSlot.GetComponent<SpellInventorySlot>();
                        newSlotComponent.AddSpell(spellsInventory[i], true);

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

            #region SHOP CONSUMABLES INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < consumableInventorySlots.Length; i++)
            {
                consumableInventorySlots[i].ClearInventorySlot();
                consumableInventorySlots[i].gameObject.SetActive(false);
            }

            if (consumablesInventory.Count > 0) // If there is inventory, create the slots
            {
                consumableInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < consumablesInventory.Count; i++)
                {
                    if (i < consumableInventorySlots.Length)
                    {
                        // Reuse existing slot
                        consumableInventorySlots[i].gameObject.SetActive(true);
                        consumableInventorySlots[i].AddConsumableItem(consumablesInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(consumableInventorySlotPrefab, consumableInventorySlotsParent);
                        ConsumableInventorySlot newSlotComponent = newSlot.GetComponent<ConsumableInventorySlot>();
                        newSlotComponent.AddConsumableItem(consumablesInventory[i], true);

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

            #region SHOP KEY ITEMS INVENTORY SLOTS
            // Clear older slots and deactivate them
            for (int i = 0; i < keyItemInventorySlots.Length; i++)
            {
                keyItemInventorySlots[i].ClearInventorySlot();
                keyItemInventorySlots[i].gameObject.SetActive(false);
            }

            if (keyItemsInventory.Count > 0) // If there is inventory, create the slots
            {
                keyItemInventorySlotPrefab.SetActive(true); // Display used slot template

                for (int i = 0; i < keyItemsInventory.Count; i++)
                {
                    if (i < keyItemInventorySlots.Length)
                    {
                        // Reuse existing slot
                        keyItemInventorySlots[i].gameObject.SetActive(true);
                        keyItemInventorySlots[i].AddKeyItem(keyItemsInventory[i], true);
                    }
                    else
                    {
                        // Instantiate new slot if not enough existing slots
                        GameObject newSlot = Instantiate(keyItemInventorySlotPrefab, keyItemInventorySlotsParent);
                        KeyItemInventorySlot newSlotComponent = newSlot.GetComponent<KeyItemInventorySlot>();
                        newSlotComponent.AddKeyItem(keyItemsInventory[i], true);

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
        }

        private void SortEquippedSellableLevel()
        {
            weaponsInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || ( a.sellable && !b.sellable ))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Sort primarily by level
                    int levelComparison = b.level.CompareTo(a.level);

                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        // Secondary Sort by Rarity
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });

            shieldsInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || (a.sellable && !b.sellable))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Sort primarily by level
                    int levelComparison = b.level.CompareTo(a.level);

                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        // Secondary Sort by Rarity
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });

            torsoArmorInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || (a.sellable && !b.sellable))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Sort primarily by level
                    int levelComparison = b.level.CompareTo(a.level);

                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        // Secondary Sort by Rarity
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });

            handsArmorInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || (a.sellable && !b.sellable))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Sort primarily by level
                    int levelComparison = b.level.CompareTo(a.level);

                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        // Secondary Sort by Rarity
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });

            legsArmorInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || (a.sellable && !b.sellable))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Sort primarily by level
                    int levelComparison = b.level.CompareTo(a.level);

                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        // Secondary Sort by Rarity
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });

            ringsInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || (a.sellable && !b.sellable))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Sort primarily by level
                    int levelComparison = b.level.CompareTo(a.level);

                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        // Secondary Sort by Rarity
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });

            amuletsInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || (a.sellable && !b.sellable))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Sort primarily by level
                    int levelComparison = b.level.CompareTo(a.level);

                    if (levelComparison != 0)
                    {
                        return levelComparison;
                    }
                    else
                    {
                        // Secondary Sort by Rarity
                        return b.rarity.CompareTo(a.rarity);
                    }
                }
            });

            spellsInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || (a.sellable && !b.sellable))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Secondary Sort by Rarity
                    return b.rarity.CompareTo(a.rarity);
                }
            });

            consumablesInventory.Sort((a, b) =>
            {
                if ((a.equipped && !b.equipped) || (!a.sellable && b.sellable))
                {
                    return 1; // Move 'a' to the back if 'a' is equipped and 'b' is not
                }
                else if ((!a.equipped && b.equipped) || (a.sellable && !b.sellable))
                {
                    return -1; // Move 'b' to the front if 'b' is equipped and 'a' is not
                }
                else
                {
                    // Secondary Sort by Rarity
                    return b.consumable_ID.CompareTo(a.consumable_ID);
                }
            });

            keyItemsInventory.Sort((a, b) =>
            {
                // Secondary Sort by Rarity
                return b.rarity.CompareTo(a.rarity);
            });

        }

        public void OpenShopMenu(string name="Error")
        {
            // Display background image and stop rendering camera to save performance
            playerMenuManager.SetMenuBackground(true);

            // Make sure all UI elements are visible
            foreach (GameObject elem in hideOnPurchase)
            {
                elem.SetActive(true);
            }
            // IF on PC, hide leave button

            bool controller = controllerUIManager.isUsingController();

            if (!controller)
            {
                leaveButtonPC.SetActive(true);
            }
            else
            {
                foreach (GameObject elem in hideOnPurchaseController)
                {
                    elem.SetActive(true);
                }
            }

            shopName = name;
            shopNameText.text = shopName;

            //FindInventorySlots();

            //open controller HUD
            controllerHUD.SetActive(true);
            //pause game
            Time.timeScale = 0;
            //Reset menu tabs
            currentTabIndex = 0;
            currentInventoryTabIndex = 0;
            menuTabs[currentTabIndex].GetComponent<Button>().onClick.Invoke();
            inventoryTabs[currentInventoryTabIndex].GetComponent<Button>().onClick.Invoke();

            SpellsHUDManager.instance.UpdateSpellHUD();
            UpdateShopUI();
            playerStats.UpdateStatsScreen();
            shopMenuWindow.SetActive(true);
            SetSelectedInventorySlot();

            playerStats.ScaleStatusBars(true);

            goldHUDObj.SetActive(true);

            minimapWindow.SetActive(false);

            setHUD.SetHUDActive(false);

            SetNotificationsTextLocation(false);

            // Update & display controller HUD ("buy, sell, leave")
            ControllerUIManager.instance.SetSelectText("Buy");
            ControllerUIManager.instance.SelectTextActive(true);
            ControllerUIManager.instance.SetBackText("Leave");
            ControllerUIManager.instance.BackTextActive(true);

            uiAudioManager.PlayOpenMenuAudio();
        }

        public void CloseShopMenu()
        {
            // Hide background image and resume main camera rendering
            playerMenuManager.SetMenuBackground(false);

            //close controller HUD
            controllerHUD.SetActive(false);

            Time.timeScale = 1;

            shopMenuWindow.SetActive(false);

            itemInfoBox.SetActive(false);
            equipRatingDescription.SetActive(false);
            primaryStatDescription.SetActive(false);
            secondaryStatsDescription.SetActive(false);
            if (lastSelectedBorder != null)
                lastSelectedBorder.SetActive(false);
            if (lastStatButtonImage != null)
                lastStatButtonImage.color = hidden;

            SortEquippedSellableLevel();
            SpellsHUDManager.instance.UpdateSpellHUD();

            playerStats.ScaleStatusBars(false);

            goldHUDObj.SetActive(false);

            minimapWindow.SetActive(true);

            setHUD.SetHUDActive(true);

            SetNotificationsTextLocation(true);

            InputHandler.instance.inMenu = false;

            // Update & display controller HUD ("buy, sell, leave")
            ControllerUIManager.instance.SetSelectText("Select");
            ControllerUIManager.instance.SelectTextActive(false);
            ControllerUIManager.instance.SetBackText("Back");
            ControllerUIManager.instance.BackTextActive(false);

            // Reenable interact prompt
            InteractPrompt.instance.interactActive = true;
            InteractPrompt.instance.interactContainer.SetActive(true);

            uiAudioManager.PlayCloseMenuAudio();

            currentShopTrigger?.ResetTrigger();

            Cursor.lockState = CursorLockMode.Locked;
        }

        public void SwitchMenuTab()
        {
            int newIndex = 0;
            
            if(currentTabIndex == 0)
            {
                newIndex = 1;
            }

            // Switch between Buy and Sell
            currentTabIndex = newIndex;
            menuTabs[newIndex].GetComponent<Button>().onClick.Invoke();
        }

        public void UpdateMenuTab(bool buying = true)
        {
            if (buying) currentTabIndex = 0;
            else currentTabIndex = 1;
            UpdateShopUI();
            inventoryTabs[currentInventoryTabIndex].GetComponent<Button>().onClick.Invoke();
            // Rebuild layout to prevent ui elements being scrunched and overlapped
            ScrollRect scrollRect = inventoryContents[currentInventoryTabIndex].transform.parent.transform.parent.GetComponent<ScrollRect>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            ResetScrollbars();
            playerStats.DisableAllStatPreviews();
            uiAudioManager.PlaySwitchMenuTabAudio();
        }

        public void SwitchInventoryTab(string direction)
        {
            if (direction == "left")
            {
                if (currentInventoryTabIndex - 1 > -1)
                {
                    inventoryTabs[currentInventoryTabIndex - 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else if (direction == "right")
            {
                if (currentInventoryTabIndex + 1 < 6)
                {
                    inventoryTabs[currentInventoryTabIndex + 1].GetComponent<Button>().onClick.Invoke();
                }
            }
            else
            {
                Debug.Log("Inv Tab not active or issue with Direction from SwitchMenuTab(string direction) via PlayerMenuManager.cs not recogmized");
                return;
            }

        }

        public void UpdateInventoryTab(int _currentInventoryTabIndex)
        {
            currentInventoryTabIndex = _currentInventoryTabIndex;
            SortEquippedSellableLevel();
            UpdateShopUI();
            SetSelectedInventorySlot();
            // Rebuild layout to prevent ui elements being scrunched and overlapped
            ScrollRect scrollRect = inventoryContents[currentInventoryTabIndex].transform.parent.transform.parent.GetComponent<ScrollRect>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            ResetScrollbars();
            playerStats.DisableAllStatPreviews();
            uiAudioManager.PlaySwitchSubMenuTabAudio();
        }

        public void SetCurrentInventoryTabIndex(int newIndex)
        {
            currentInventoryTabIndex = newIndex;
        }

        public void SetSelectedInventorySlot()
        {
            itemInfoBox.SetActive(false);

            //If controller is detected, set current selected button to the first item slot of the current inventory tab
            if (inventoryContents[currentInventoryTabIndex].transform.childCount > 0)
            {
                //First Child is "WeaponSlot" -> First Child of "WeaponSlot" is the gameObject containing the button to select
                
                // If not the armor section, select child of child (item slot)
                if(currentInventoryTabIndex != 2)
                {
                    // Had an issue where the first button when switching menu tabs in the shop is not displaying its info box
                    // (likely due to conflicts with selecting a gameobject with the event system)
                    // So here, we are forcing that box to open
                    if (inventoryContents[currentInventoryTabIndex].transform.GetChild(0).gameObject.activeSelf)
                    {
                        DisplayInfoBox dib = inventoryContents[currentInventoryTabIndex].transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<DisplayInfoBox>();
                        dib?.Display();
                    }

                    // Change selected button in the Event System
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(inventoryContents[currentInventoryTabIndex].transform.GetChild(0).transform.GetChild(0).gameObject);
                    //Now, trigger the animation of being selected (by turning on the selected window gameObject):
                    inventoryContents[currentInventoryTabIndex].transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                }
                // However, if we are dealing with the armor tab, we may need to keep checking the next section until we find a button
                else
                {
                    // Torso Section                                     torso section               slot
                    if(inventoryContents[currentInventoryTabIndex].transform.GetChild(1).transform.GetChild(0).gameObject.activeSelf)
                    {
                        // Path: armor contents  -> torso section ->  slot -> button
                        DisplayInfoBox dib = inventoryContents[currentInventoryTabIndex].transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).GetComponent<DisplayInfoBox>();
                        dib?.Display();
                        
                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(inventoryContents[currentInventoryTabIndex].transform.GetChild(1).transform.GetChild(0).transform.GetChild(0).gameObject);
                        //Now, trigger the animation of being selected (by turning on the selected window gameObject):
                        inventoryContents[currentInventoryTabIndex].transform.GetChild(1).transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                    }
                    // Hands Section
                    else if (inventoryContents[currentInventoryTabIndex].transform.GetChild(3).transform.GetChild(0).gameObject.activeSelf)
                    {
                        // Path: armor contents  -> hands section ->  slot -> button
                        DisplayInfoBox dib = inventoryContents[currentInventoryTabIndex].transform.GetChild(3).transform.GetChild(0).transform.GetChild(0).GetComponent<DisplayInfoBox>();
                        dib?.Display();

                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(inventoryContents[currentInventoryTabIndex].transform.GetChild(3).transform.GetChild(0).transform.GetChild(0).gameObject);
                        //Now, trigger the animation of being selected (by turning on the selected window gameObject):
                        inventoryContents[currentInventoryTabIndex].transform.GetChild(3).transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                    }
                    // Legs Section
                    else if (inventoryContents[currentInventoryTabIndex].transform.GetChild(5).transform.GetChild(0).gameObject.activeSelf)
                    {
                        // Path: armor contents  -> legs section ->  slot -> button
                        DisplayInfoBox dib = inventoryContents[currentInventoryTabIndex].transform.GetChild(5).transform.GetChild(0).transform.GetChild(0).GetComponent<DisplayInfoBox>();
                        dib?.Display();

                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(inventoryContents[currentInventoryTabIndex].transform.GetChild(5).transform.GetChild(0).transform.GetChild(0).gameObject);
                        //Now, trigger the animation of being selected (by turning on the selected window gameObject):
                        inventoryContents[currentInventoryTabIndex].transform.GetChild(5).transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                    }
                    //Finger Section
                    else if (inventoryContents[currentInventoryTabIndex].transform.GetChild(7).transform.GetChild(0).gameObject.activeSelf)
                    {
                        // Path: armor contents  -> finger section ->  slot -> button
                        DisplayInfoBox dib = inventoryContents[currentInventoryTabIndex].transform.GetChild(7).transform.GetChild(0).transform.GetChild(0).GetComponent<DisplayInfoBox>();
                        dib?.Display();

                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(inventoryContents[currentInventoryTabIndex].transform.GetChild(7).transform.GetChild(0).transform.GetChild(0).gameObject);
                        //Now, trigger the animation of being selected (by turning on the selected window gameObject):
                        inventoryContents[currentInventoryTabIndex].transform.GetChild(7).transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                    }
                    //Neck Section
                    else if (inventoryContents[currentInventoryTabIndex].transform.GetChild(9).transform.GetChild(0).gameObject.activeSelf)
                    {
                        // Path: armor contents  -> neeck section ->  slot -> button
                        DisplayInfoBox dib = inventoryContents[currentInventoryTabIndex].transform.GetChild(9).transform.GetChild(0).transform.GetChild(0).GetComponent<DisplayInfoBox>();
                        dib?.Display();

                        UIChangeSelectedButton.instance.ChangeSelectedButtonTo(inventoryContents[currentInventoryTabIndex].transform.GetChild(9).transform.GetChild(0).transform.GetChild(0).gameObject);
                        //Now, trigger the animation of being selected (by turning on the selected window gameObject):
                        inventoryContents[currentInventoryTabIndex].transform.GetChild(9).transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                    }

                }

            }
        }

        public void AddToShopInventory(Item item, int count= 1)
        {
            int found = -1;

            switch (item)
            {
                case WeaponItem weaponItem:
                    shopWeaponsInventory.Add(weaponItem);
                    break;
                case ShieldItem shieldItem:
                    shopShieldsInventory.Add(shieldItem);
                    break;
                case TorsoArmorItem torsoArmorItem:
                    shopTorsoArmorInventory.Add(torsoArmorItem);
                    break;
                case HandsArmorItem handsArmorItem:
                    shopHandsArmorInventory.Add(handsArmorItem);
                    break;
                case LegsArmorItem legsArmorItem:
                    shopLegsArmorInventory.Add(legsArmorItem);
                    break;
                case RingItem ringItem:
                    shopRingsInventory.Add(ringItem);
                    break;
                case AmuletItem amuletItem:
                    shopAmuletsInventory.Add(amuletItem);
                    break;
                case Spell spellItem:
                    shopSpellsInventory.Add(spellItem);
                    break;
                case Consumable consumableItem:
                    for (int i = 0; i < shopConsumablesInventory.Count; i++)
                    {
                        if (shopConsumablesInventory[i].consumable_ID == consumableItem.consumable_ID)
                        {
                            found = i;
                            i = shopConsumablesInventory.Count; //break the loop
                        }
                    }
                    if (found > -1)
                    {
                        shopConsumablesInventory[found].count += consumableItem.count;
                    }
                    else
                    {
                        shopConsumablesInventory.Add(consumableItem);
                    }
                    break;
                case KeyItem keyItem:
                    for (int i = 0; i < shopKeyItemsInventory.Count; i++)
                    {
                        if (shopKeyItemsInventory[i].keyItem_ID == keyItem.keyItem_ID)
                        {
                            found = i;
                            i = shopKeyItemsInventory.Count;
                        }
                    }

                    if (found > -1)
                    {
                        shopKeyItemsInventory[found].count += keyItem.count;
                    }
                    else
                    {
                        shopKeyItemsInventory.Add(keyItem);
                    }
                    break;
                default:
                    Debug.Log("Adding to Shop Inventory Type Error - did not add");
                    break;
            }
        }

        public void RemoveFromShopInventory(Item item, int count=1)
        {
                switch (item)
                {
                    case WeaponItem weaponItem:
                        shopWeaponsInventory.Remove(weaponItem);
                        break;
                    case ShieldItem shieldItem:
                        shopShieldsInventory.Remove(shieldItem);
                        break;
                    case TorsoArmorItem torsoArmorItem:
                        shopTorsoArmorInventory.Remove(torsoArmorItem);
                        break;
                    case HandsArmorItem handsArmorItem:
                        shopHandsArmorInventory.Remove(handsArmorItem);
                        break;
                    case LegsArmorItem legsArmorItem:
                        shopLegsArmorInventory.Remove(legsArmorItem);
                        break;
                    case RingItem ringItem:
                        shopRingsInventory.Remove(ringItem);
                        break;
                    case AmuletItem amuletItem:
                        shopAmuletsInventory.Remove(amuletItem);
                        break;
                    case Spell spellItem:
                        shopSpellsInventory.Remove(spellItem);
                        break;
                    case Consumable consumableItem:
                        consumableItem.count -= count;
                        break;
                    case KeyItem keyItem:
                        keyItem.count -= count;
                        break;
                    default:
                        Debug.Log("Remove From Shop Inventory Type Error");
                        break;
                }
            
        }

        public void SetNotificationsTextLocation(bool originalLocation)
        {
            if (originalLocation)
            {
                textNotifications.anchoredPosition = playerMenuManager.originalTextNotificationsPosition;

                acquiredNotifications.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                acquiredNotifications.anchoredPosition = playerMenuManager.originalAcquNotificationsPosition;
            }
            else
            {
                textNotifications.anchoredPosition = inMenuTextNotificationsPosition;
                acquiredNotifications.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                float xOffset = 0;
                if ((float)Screen.width / (float)Screen.height > 2f) xOffset = 300f;
                acquiredNotifications.anchoredPosition = new Vector2(inMenuAcquiredNotificationsPosition.x + xOffset, inMenuAcquiredNotificationsPosition.y);

                //acquiredNotifications.anchoredPosition = inMenuAcquiredNotificationsPosition;
            }
        }

        public void AdjustSlider(string buttonType)
        {
            // Need to calculate a quarter of the max value rounded down as an int
            int quarterQuantity = (int)Mathf.Floor(quantitySlider.maxValue / 4.0f);

            switch (buttonType)
            {

                case "leftShoulder":
                    // We clamp the results between 0 and max quantity for the slider (which is according to the item)
                    quantitySlider.value = Mathf.Clamp(quantitySlider.value - quarterQuantity, 0, quantitySlider.maxValue);
                    break;
                case "rightShoulder":
                    // We clamp the results between 0 and max quantity for the slider (which is according to the item)
                    quantitySlider.value = Mathf.Clamp(quantitySlider.value + quarterQuantity, 0, quantitySlider.maxValue);
                    break;
                case "leftTrigger":
                    quantitySlider.value = quantitySlider.minValue;
                    break;
                case "rightTrigger":
                    quantitySlider.value = quantitySlider.maxValue;
                    break;
                default:
                    break;
            }
        }

        public void ResetScrollbars()
        {
            // Usually, scroll bars are set properly from the autoscroll script, however if
            // in some cases, such as torso armor being absent, a slot can be selected while
            // the scrollbars isn't properly synced, this will prevent any such case
            foreach(Scrollbar sb in scrollbars)
            {
                sb.value = 1.0f;
            }
            foreach (GameObject contents in inventoryContents)
            {
                Vector2 currentPosition = contents.GetComponent<RectTransform>().anchoredPosition;
                contents.GetComponent<RectTransform>().anchoredPosition = new Vector2(currentPosition.x, 0);
            }

        }

    }
}
