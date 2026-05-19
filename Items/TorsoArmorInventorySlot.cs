using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class TorsoArmorInventorySlot : MonoBehaviour
    {
        public Image icon;
        public Image childButtonImage;
        public GameObject equipped;
        public TextMeshProUGUI levelText;

        public GameObject newNotification;

        public PlayerMenuManager playerMenuManager;

        [Header("DO NOT SET MANUALLY:")]
        public TorsoArmorItem item;

        private void OnEnable()
        {
            // Had to do this to prevent first weapon inventory slot from being disabled when menu is first opened
            if (transform.parent.transform.GetChild(0).gameObject != gameObject)
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public void AddTorsoArmorItem(TorsoArmorItem newitem, bool shop = false)
        {
            item = newitem;

            if (item.itemIcon == null) item.itemIcon = PlayerInventory.instance.torsoArmorIcons[item.torsoArmorModelID];
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


        public void ChangeEquippedTorsoArmor()
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
                //UNEQUIP OLD ARMOR
                PlayerInventory.instance.equippedTorsoArmor.equipped = false;
                //EQUIP NEW ARMOR
                item.equipped = true;
                PlayerInventory.instance.equippedTorsoArmor = item;
                Debug.Log("Equipped: " + PlayerInventory.instance.equippedTorsoArmor.itemName);
                playerMenuManager.UpdateUI();

                //Update Controller UI Text
                ControllerUIManager.instance.SetSelectText("Unequip");

                //PLAY AUDIO HERE
                UIAudioManager.instance.PlayEquipItemAudio();

                //STOP PREVIEWING STAT CHANGES 
                PlayerStats.instance.DisableAllStatPreviews();
                //UPDATE STATS
                PlayerStats.instance.CalculateEffectiveStats();

                //ADD VISUAL CHANGE TO CHARACTER HERE
                PlayerInventory.instance.UpdateTorsoArmorModel(true);

            }
            else
            {
                //Debug.Log(item.itemName + " already equipped.");
                PlayerInventory.instance.equippedTorsoArmor.equipped = false;

                PlayerInventory.instance.equippedTorsoArmor = PlayerInventory.instance.emptyTorsoArmor;

                //Update Controller UI Text
                ControllerUIManager.instance.SetSelectText("Equip");

                //PLAY AUDIO HERE
                UIAudioManager.instance.PlayUnequipItemAudio();

                //UPDATE UI
                playerMenuManager.UpdateUI();

                //UPDATE STATS
                PlayerStats.instance.CalculateEffectiveStats();

                //ADD VISUAL CHANGE TO CHARACTER HERE
                PlayerInventory.instance.UpdateTorsoArmorModel(false);
            }

            // ENABLE SELECTED BORDER
            transform.GetChild(1).gameObject.SetActive(true);
            PlayerStats.instance.PreviewTorsoArmorStatChanges(item);
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
