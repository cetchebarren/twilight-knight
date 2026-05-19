using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace etchebarren
{
    public class SpellInventorySlot : MonoBehaviour
    {
        public ControllerUIManager controllerUImanager;

        public Image icon;
        public Image childButtonImage;
        public GameObject equipped;
        //public TextMeshProUGUI levelText;
        public Image buttonIcon; //keybinding
        public TextMeshProUGUI spellSlotText;

        public GameObject newNotification;

        public PlayerMenuManager playerMenuManager;
        public SpellsHUDManager spellsHUDManager;
        public GameObject spell1Slot;

        [Header("DO NOT SET MANUALLY:")]
        public Spell spell;

        private void OnEnable()
        {
            // Had to do this to prevent first weapon inventory slot from being disabled when menu is first opened
            if (transform.parent.transform.GetChild(0).gameObject != gameObject)
            {
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        public void AddSpell(Spell newSpell, bool shop = false)
        {
            spell = newSpell;

            if(spell.itemIcon == null) spell.itemIcon = PlayerInventory.instance.spellIcons[spell.spell_ID];
            icon.sprite = spell.itemIcon;

            icon.enabled = true;
            gameObject.SetActive(true);
            icon.color = Color.white;
            newNotification.SetActive(spell.flaggedAsNew);

            bool controller = ControllerUIManager.instance.isUsingController();

            //lets change the background color of the button
            if (!shop)
            {
                childButtonImage.color = PlayerInventory.instance.rarityColors[spell.rarity];
            }
            else
            {
                childButtonImage.color = PlayerInventory.instance.shopRarityColors[spell.rarity];

                bool equipped = false;

                foreach(Spell _spell in PlayerInventory.instance.equippedSpells)
                {
                    if(_spell == spell)
                    {
                        equipped = true;
                    }
                }

                if (!spell.sellable || equipped)
                {
                    childButtonImage.color = new Color(childButtonImage.color.r, childButtonImage.color.g, childButtonImage.color.b, 0.39f);
                    icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0.39f);
                }
            }

            int spellIndex = Array.IndexOf(PlayerInventory.instance.equippedSpells, spell);

            //No controller, spell assigned
            if (!controller && spellIndex != -1)
            {
                buttonIcon.enabled = true;
                buttonIcon.sprite = ControllerUIManager.instance.computerKey;
                if(spellIndex == 1)
                {
                    spellIndex = 2;
                }
                else if(spellIndex == 2)
                {
                    spellIndex = 1;
                }
                if (spellIndex == 5)
                {
                    spellIndex = 6;
                }
                else if (spellIndex == 6)
                {
                    spellIndex = 5;
                }
                spellSlotText.text = (spellIndex + 1).ToString();
            }
            //No controller, no spell assigned
            else if (!controller)
            {
                buttonIcon.enabled = false;
                spellSlotText.text = "";
            }
            // Controller, spell assigned
            else if (controller && spellIndex != -1)
            {
                buttonIcon.enabled = true;
                spellSlotText.text = "";
                switch(spellIndex)
                {
                    case 0:
                    case 4:
                        if (controllerUImanager.controllerTypePlaystation)
                        {
                            buttonIcon.sprite = controllerUImanager.triangle;
                        }
                        else
                        {
                            buttonIcon.sprite = controllerUImanager.Y;
                        }
                        break;
                    case 1:
                    case 5:
                        if (controllerUImanager.controllerTypePlaystation)
                        {
                            buttonIcon.sprite = controllerUImanager.square;
                        }
                        else
                        {
                            buttonIcon.sprite = controllerUImanager.xboxX;
                        }
                        break;
                    case 2:
                    case 6:
                        if (controllerUImanager.controllerTypePlaystation)
                        {
                            buttonIcon.sprite = controllerUImanager.O;
                        }
                        else
                        {
                            buttonIcon.sprite = controllerUImanager.B;
                        }
                        break;
                    case 3:
                    case 7:
                        if (controllerUImanager.controllerTypePlaystation)
                        {
                            buttonIcon.sprite = controllerUImanager.X;
                        }
                        else
                        {
                            buttonIcon.sprite = controllerUImanager.A;
                        }
                        break;
                    default:
                        break;
                }
            }
            else if(controller)
            {
                spellSlotText.text = "";
                buttonIcon.enabled = false;
            }
        }

        public void ClearInventorySlot()
        {
            spell = null;
            icon.sprite = null;
            icon.enabled = false;
            gameObject.SetActive(false);
            equipped.SetActive(false);
        }


        public void ChangeEquippedSpell()
        {
            int spellIndex = Array.IndexOf(PlayerInventory.instance.equippedSpells, spell);
            Debug.Log("Spell is equipped in slot: " + spellIndex);

            if (spellIndex >= 0) // Spell is equipped
            {
                spell.equipped = false;
                PlayerInventory.instance.equippedSpells[spellIndex] = null;
                //Update Controller UI Text
                ControllerUIManager.instance.SetSelectText("Equip");
                playerMenuManager.UpdateUI();
                SpellsHUDManager.instance.UpdateSpellHUD();

                //PLAY UNEQUIP
                UIAudioManager.instance.PlayUnequipSpellAudio();
            }
            else
            {
                playerMenuManager.lastSelectedSpellBeforeSwap = EventSystem.current.currentSelectedGameObject;
                spellsHUDManager.selectedSpell = spell;
                spellsHUDManager.OpenSelectSpellSlotScreen();
                UIChangeSelectedButton.instance.ChangeSelectedButtonTo(spell1Slot);
            }
            // ENABLE SELECTED BORDER
            transform.GetChild(1).gameObject.SetActive(true);
        }

        public void OpenPurchaseWindow()
        {
            // Buying
            if (ShopMenuManager.instance.buying)
            {
                if(PlayerInventory.instance.goldCount >= spell.goldValue)
                {
                    ShopMenuManager.instance.OpenPurchaseWindow(spell);
                }
                else
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Not enough gold", true); //true prevents duplicate messages for 3 seconds
                }
            }
            // Selling
            else
            {
                if (!spell.sellable)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("This item cannot be sold", true); //true prevents duplicate messages for 3 seconds
                }
                else if (spell.equipped)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Cannot sell equipped spells", true); //true prevents duplicate messages for 3 seconds
                }
                else
                {
                    ShopMenuManager.instance.OpenPurchaseWindow(spell);
                }
            }
        }

    }
}
