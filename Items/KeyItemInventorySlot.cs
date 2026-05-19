using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class KeyItemInventorySlot : MonoBehaviour
    {
        public Image icon;
        public Image childButtonImage;
        public TextMeshProUGUI countText;

        public GameObject newNotification;

        public PlayerMenuManager playerMenuManager;

        [Header("DO NOT SET MANUALLY:")]
        public KeyItem item;

        private void OnEnable()
        {
            // Had to do this to prevent first weapon inventory slot from being disabled when menu is first opened
            if (transform.parent.transform.GetChild(0).gameObject != gameObject)
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public void AddKeyItem(KeyItem newitem, bool shop = false)
        {
            item = newitem;

            if (item.itemIcon == null) item.itemIcon = PlayerInventory.instance.keyItemIcons[item.keyItem_ID - 800]; // Key Items have an 800 offset or their IDs
            icon.sprite = item.itemIcon;

            icon.enabled = true;
            gameObject.SetActive(true);
            countText.text = "x" + item.count.ToString();
            newNotification.SetActive(item.flaggedAsNew);

            //lets change the background color of the button
            if (!shop)
            {
                childButtonImage.color = PlayerInventory.instance.rarityColors[item.rarity];
            }
            else
            {
                childButtonImage.color = PlayerInventory.instance.shopRarityColors[item.rarity];
            }

        }

        public void SelectKeyItem()
        {
            //depending on item ID, perform some operation

            // ENABLE SELECTED BORDER
            transform.GetChild(1).gameObject.SetActive(true);
        }

        public void ClearInventorySlot()
        {
            item = null;
            icon.sprite = null;
            icon.enabled = false;
            gameObject.SetActive(false);
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
