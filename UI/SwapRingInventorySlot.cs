using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class SwapRingInventorySlot : MonoBehaviour
    {
        public Image icon;
        public Image childButtonImage;
        public GameObject equipped;
        public TextMeshProUGUI levelText;

        public GameObject swapRingWindow;

        //public bool isNew = true;
        //public GameObject newNotification;

        public PlayerMenuManager playerMenuManager;

        [Header("DO NOT SET MANUALLY:")]
        public RingItem item;
        public RingItem newRing;

        private void OnEnable()
        {
            transform.GetChild(2).gameObject.SetActive(false);
        }

        public void AddRingItem(RingItem newitem)
        {
            item = newitem;
            icon.sprite = item.itemIcon;
            icon.enabled = true;
            gameObject.SetActive(true);
            equipped.SetActive(item.equipped);
            levelText.text = "Lv." + item.level.ToString();

            //lets change the background color of the button
            childButtonImage.color = PlayerInventory.instance.rarityColors[item.rarity];

        }

        public void ClearInventorySlot()
        {
            item = null;
            icon.sprite = null;
            icon.enabled = false;
            gameObject.SetActive(false);
            equipped.SetActive(false);
        }


        public void ChangeEquippedRing(bool slotOne)
        {
            if(slotOne)
            {
                //UNEQUIP OLD ARMOR
                PlayerInventory.instance.equippedRing1.equipped = false;
                newRing.equipped = true;
                PlayerInventory.instance.equippedRing1 = newRing;
            }
            else
            {
                //UNEQUIP OLD ARMOR
                PlayerInventory.instance.equippedRing2.equipped = false;
                newRing.equipped = true;
                PlayerInventory.instance.equippedRing2 = newRing;
            }
            playerMenuManager.UpdateUI();

            //PLAY AUDIO HERE
            UIAudioManager.instance.PlayEquipItemAudio();

            //STOP PREVIEWING STAT CHANGES 
            PlayerStats.instance.DisableAllStatPreviews();
            //UPDATE STATS
            PlayerStats.instance.CalculateEffectiveStats();

            swapRingWindow.SetActive(false);

        }

    }
}
