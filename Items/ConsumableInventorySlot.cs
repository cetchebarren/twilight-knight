using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class ConsumableInventorySlot : MonoBehaviour
    {
        public Image icon;
        public Image childButtonImage;
        public GameObject equipped;
        public TextMeshProUGUI countText;

        public GameObject newNotification;

        public PlayerMenuManager playerMenuManager;

        [Header("DO NOT SET MANUALLY:")]
        public Consumable item;

        private void OnEnable()
        {
            // Had to do this to prevent first weapon inventory slot from being disabled when menu is first opened
            if (transform.parent.transform.GetChild(0).gameObject != gameObject)
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public void AddConsumableItem(Consumable newitem, bool shop = false)
        {
            item = newitem;
            if (item.itemIcon == null) item.itemIcon = PlayerInventory.instance.consumableIcons[item.consumable_ID]; // Key Items have an 800 offset or their IDs
            icon.sprite = item.itemIcon;
            icon.enabled = true;
            gameObject.SetActive(true);
            equipped.SetActive(item.equipped);
            countText.text = "x" + item.count.ToString();
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

                if (!item.sellable)
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


        public void ChangeEquippedConsumable()
        {
            if (!item.equipped) //if not already equipped
            {
                item.equipped = true;
                PlayerInventory.instance.equippedConsumables.Add(item);
                playerMenuManager.UpdateUI();

                //Update Controller UI Text
                ControllerUIManager.instance.SetSelectText("Unequip");

                //PLAY AUDIO
                switch (item.audioType)
                {
                    case Consumable.AudioType.Potion:
                        UIAudioManager.instance.PlayEquipPotionAudio();
                        break;
                    case Consumable.AudioType.SpellTome:
                        UIAudioManager.instance.PlayOpenNoteAudio();
                        break;
                    default:
                        UIAudioManager.instance.PlayEquipItemAudio();
                        break;
                }
            }
            else
            {
                item.equipped = false;
                PlayerInventory.instance.equippedConsumables.Remove(item);
                playerMenuManager.UpdateUI();

                //Update Controller UI Text
                ControllerUIManager.instance.SetSelectText("Equip");

                //PLAY AUDIO
                switch (item.audioType)
                {
                    case Consumable.AudioType.Potion:
                        UIAudioManager.instance.PlayUnequipPotionAudio();
                        break;
                    case Consumable.AudioType.SpellTome:
                        UIAudioManager.instance.PlayCloseNoteAudio();
                        break;
                    default:
                        UIAudioManager.instance.PlayUnequipItemAudio();
                        break;
                }
            }
            // ENABLE SELECTED BORDER
            transform.GetChild(1).gameObject.SetActive(true);

            ConsumablesHUDManager.instance.UpdateConsumablesHUD();
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
                else
                {
                    ShopMenuManager.instance.OpenPurchaseWindow(item);
                }
            }
        }

    }
}
