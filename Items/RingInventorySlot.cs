using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace etchebarren
{
    public class RingInventorySlot : MonoBehaviour
    {
        public Image icon;
        public Image childButtonImage;
        public GameObject equipped;
        public TextMeshProUGUI levelText;

        public GameObject newNotification;

        public PlayerMenuManager playerMenuManager;

        public GameObject swapRingWindow;
        public SwapRingInventorySlot ringSlot1;
        public SwapRingInventorySlot ringSlot2;
        public GameObject ringSlot1Button;

        [Header("DO NOT SET MANUALLY:")]
        public RingItem item;

        private void OnEnable()
        {
            // Had to do this to prevent first weapon inventory slot from being disabled when menu is first opened
            if (transform.parent.transform.GetChild(0).gameObject != gameObject)
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public void AddRingItem(RingItem newitem, bool shop = false)
        {
            item = newitem;

            if (item.itemIcon == null) item.itemIcon = PlayerInventory.instance.ringIcons[item.ringModelID];
            icon.sprite = item.itemIcon;

            icon.enabled = true;
            gameObject.SetActive(true);
            equipped.SetActive(item.equipped);
            levelText.text = "Lv." + item.level.ToString();
            icon.color = Color.white;
            newNotification.SetActive(item.flaggedAsNew);

            //lets change the background color of the button
            if (!shop)
            {
                childButtonImage.color = PlayerInventory.instance.rarityColors[item.rarity];
            }
            else
            {
                childButtonImage.color = PlayerInventory.instance.shopRarityColors[item.rarity];

                if (!item.sellable || item.equipped)
                {
                    childButtonImage.color = new Color(childButtonImage.color.r, childButtonImage.color.g, childButtonImage.color.b, 0.39f);
                    icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0.39f);
                }
            }
        }

        public void ClearInventorySlot()
        {
            item = null;
            icon.sprite = null;
            icon.enabled = false;
            gameObject.SetActive(false);
            equipped.SetActive(false);
        }


        public void ChangeEquippedRing()
        {
            if (InputHandler.instance.isAttacking)
            {
                TextNotificationsManager.instance.NewTextNotifaction("Cannot change equipment while attacking", true); //true prevents duplicate messages for 3 seconds
                return;
            }
            if (InputHandler.instance.isBlocking)
            {
                TextNotificationsManager.instance.NewTextNotifaction("Cannot change equipment while blocking", true); //true prevents duplicate messages for 3 seconds
                return;
            }
            if (!item.equipped) //if not already equipped
            {
                if(PlayerInventory.instance.equippedRing1 == null)
                {
                    //EQUIP NEW ARMOR
                    item.equipped = true;
                    PlayerInventory.instance.equippedRing1 = item;
                    Debug.Log("Equipped: " + PlayerInventory.instance.equippedRing1.itemName + " in ring slot 1");

                    playerMenuManager.UpdateUI();

                    //Update Controller UI Text
                    ControllerUIManager.instance.SetSelectText("Unequip");

                    //PLAY AUDIO HERE
                    UIAudioManager.instance.PlayEquipItemAudio();

                    //STOP PREVIEWING STAT CHANGES 
                    PlayerStats.instance.DisableAllStatPreviews();
                    //UPDATE STATS
                    PlayerStats.instance.CalculateEffectiveStats();

                    PlayerStats.instance.PreviewRingStatChanges(1, item);

                }
                else if(PlayerInventory.instance.equippedRing2 == null)
                {

                    //EQUIP NEW ARMOR
                    item.equipped = true;
                    PlayerInventory.instance.equippedRing2 = item;
                    Debug.Log("Equipped: " + PlayerInventory.instance.equippedRing2.itemName + " in ring slot 2");

                    playerMenuManager.UpdateUI();

                    //Update Controller UI Text
                    ControllerUIManager.instance.SetSelectText("Unequip");

                    //PLAY AUDIO HERE
                    UIAudioManager.instance.PlayEquipItemAudio();

                    //STOP PREVIEWING STAT CHANGES 
                    PlayerStats.instance.DisableAllStatPreviews();
                    //UPDATE STATS
                    PlayerStats.instance.CalculateEffectiveStats();

                    PlayerStats.instance.PreviewRingStatChanges(2, item);
                }
                else //bring up swap window
                {
                    bool controller = ControllerUIManager.instance.isUsingController();
                    if (controller) ControllerUIManager.instance.SetBackText("Back");
                    playerMenuManager.lastSelectedRingBeforeSwap = EventSystem.current.currentSelectedGameObject;
                    swapRingWindow.SetActive(true);
                    ringSlot1.newRing = item;
                    ringSlot2.newRing = item;
                    ringSlot1.AddRingItem(PlayerInventory.instance.equippedRing1);
                    ringSlot2.AddRingItem(PlayerInventory.instance.equippedRing2);
                    UIChangeSelectedButton.instance.ChangeSelectedButtonTo(ringSlot1Button);
                }
                // ENABLE SELECTED BORDER
                transform.GetChild(1).gameObject.SetActive(true);
            }
            else //if already equipped
            {
                if(item == PlayerInventory.instance.equippedRing1)
                {
                    PlayerInventory.instance.equippedRing1.equipped = false;
                    PlayerInventory.instance.equippedRing1 = null;// PlayerInventory.instance.emptyRing;

                    //Update Controller UI Text
                    ControllerUIManager.instance.SetSelectText("Equip");

                    //PLAY AUDIO HERE
                    UIAudioManager.instance.PlayUnequipItemAudio();

                    //UPDATE UI
                    playerMenuManager.UpdateUI();

                    //UPDATE STATS
                    PlayerStats.instance.CalculateEffectiveStats();

                    PlayerStats.instance.PreviewRingStatChanges(0, item);
                }
                else if (item == PlayerInventory.instance.equippedRing2)
                {
                    PlayerInventory.instance.equippedRing2.equipped = false;
                    PlayerInventory.instance.equippedRing2 = null;// PlayerInventory.instance.emptyRing;

                    //Update Controller UI Text
                    ControllerUIManager.instance.SetSelectText("Equip");

                    //PLAY AUDIO HERE
                    UIAudioManager.instance.PlayUnequipItemAudio();

                    //UPDATE UI
                    playerMenuManager.UpdateUI();

                    //UPDATE STATS
                    PlayerStats.instance.CalculateEffectiveStats();

                    PlayerStats.instance.PreviewRingStatChanges(0, item);
                }
                // ENABLE SELECTED BORDER
                transform.GetChild(1).gameObject.SetActive(true);

            }
        }

        public void OpenPurchaseWindow()
        {
            // Buying
            if (ShopMenuManager.instance.buying)
            {
                if (PlayerInventory.instance.goldCount >= item.goldValue)
                {
                    ShopMenuManager.instance.OpenPurchaseWindow(item);
                }
                else
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Not enough gold", true); //true prevents duplicate messages for 3 seconds
                }
            }
            // Selling
            else
            {
                if (!item.sellable)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("This item cannot be sold", true); //true prevents duplicate messages for 3 seconds
                }
                else if (item.equipped)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Cannot sell equipped items", true); //true prevents duplicate messages for 3 seconds
                }
                else
                {
                    ShopMenuManager.instance.OpenPurchaseWindow(item);
                }
            }
        }

    }
}
