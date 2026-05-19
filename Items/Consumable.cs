using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Items/Consumable")]

    public class Consumable : Item
    {
        public int capacity = 5; //currently unused

        public bool useDefaultAnimation = true;
        public bool keepInInventoryAtZeroCount = true;
        public bool showCountInConsumableHUD = true;

        public int consumable_ID = 99;

        public bool equipped = false;

        // Values such as potency are actually set in Player Inventory, where the potions are generated
        public float potency;

        public string affectedStat;

        // Used for the sound that plays when the item is clicked (equipped/unequipped) in the player inventory menu, called in ConsumableInventorySlot.cs
        // Also determines the item audio index in PlayItemAudio() in this script
        public enum AudioType
        {
            Potion,
            SpellTome,
            None
        }
        public AudioType audioType;
        // Bool that controls if secondary audio clip should play when using an item, it is a general sound used by some times by default
        private bool playSecondAudioClip = true;

        public void UseConsumable()
        {
            //add animation + delay here?
            Debug.Log("used consumable ID: " + consumable_ID);

            switch(consumable_ID)
            {
                case 0:
                case 3:
                case 6:
                    HealingPotion();
                    break;
                case 9:
                    HealingPotion(true);
                    break;
                case 1:
                case 4:
                case 7:
                    ManaPotion();
                    break;
                case 10:
                    ManaPotion(true);
                    break;
                case 2:
                case 5:
                case 8:
                    StaminaPotion();
                    break;
                case 11:
                    StaminaPotion(true);
                    break;
                case 12:
                    Elixir();
                    break;
                case 13:
                    SpellTome(consumable_ID);
                    break;
                case 14:
                    SpiderVenom();
                    break;
                case 15:
                    Dig();
                    break;
                default:
                    break;
            }
        }

        public void HealingPotion(bool fullRestore = false)
        {
            //For now, heal a flat 30 HP. Can update to be a % if desired, add a bool for percentages
            if(count > 0 )
            {
                if(PlayerStats.instance.currentHealth < PlayerStats.instance.maxHealth)
                {
                    ConsumablesHUDManager.instance.StartConsumableCooldown();
                    if (fullRestore)
                    {
                        PlayerStats.instance.RestoreHealth(PlayerStats.instance.maxHealth - PlayerStats.instance.currentHealth);
                    }
                    else
                    {
                        PlayerStats.instance.RestoreHealth(potency);
                    }
                    count--;

                    SelfEffectsManager.instance.RestoreHealthEffect();

                    if (!keepInInventoryAtZeroCount && count < 1)
                    {
                        ConsumablesHUDManager.instance.RemoveEquippedConsumableByID(consumable_ID);

                    }
                    ConsumablesHUDManager.instance.UpdateConsumablesHUD();
                }
                else
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Health already full");
                }

            }
            else
            {
                Debug.Log("Insufficient inventory");
                TextNotificationsManager.instance.NewTextNotifaction("Not enough in inventory");
            }

        }

        public void PlayItemAudio()
        {
            bool playClip = true;
            // Int that determines which audio clip to play from item audio list in Player Audio Manager .cs
            int itemAudioIndex;
            // Bool that determines if the general item tick audio should play after the intial audio clip
            bool playSecondClip;
            // Set values for PlayItemAudio function call from Player Audio Managaer
            switch (audioType)
            {
                case AudioType.Potion:
                    itemAudioIndex = 0;
                    playSecondClip = true;
                    break;
                case AudioType.SpellTome:
                    itemAudioIndex = 1;
                    playSecondClip = false;
                    break;
                case AudioType.None:
                    playClip = false;
                    itemAudioIndex = 0;
                    playSecondClip = false;
                    break;
                default:
                    itemAudioIndex = 0;
                    playSecondClip = true;
                    break;
            }
            if (playClip) PlayerStats.instance.playerAudioManager.PlayItemAudio(itemAudioIndex, playSecondClip);
        }

        public void ManaPotion(bool fullRestore = false)
        {
            //For now, heal a flat 30 HP. Can update to be a % if desired, add a bool for percentages
            if (count > 0)
            {
                if (PlayerStats.instance.currentMana < PlayerStats.instance.maxMana)
                {
                    ConsumablesHUDManager.instance.StartConsumableCooldown();
                    if (fullRestore)
                    {
                        PlayerStats.instance.RestoreMana(PlayerStats.instance.maxMana - PlayerStats.instance.currentMana);
                    }
                    else
                    {
                        PlayerStats.instance.RestoreMana(potency);
                    }
                    count--;

                    SelfEffectsManager.instance.RestoreManaEffect();

                    if (!keepInInventoryAtZeroCount && count < 1)
                    {
                        ConsumablesHUDManager.instance.RemoveEquippedConsumableByID(consumable_ID);
                    }
                    ConsumablesHUDManager.instance.UpdateConsumablesHUD();
                }
                else
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Mana already full");
                }
            }
            else
            {
                Debug.Log("Insufficient inventory");
                TextNotificationsManager.instance.NewTextNotifaction("Not enough in inventory");
            }
        }

        public void StaminaPotion(bool fullRestore = false)
        {
            //For now, heal a flat 30 HP. Can update to be a % if desired, add a bool for percentages

            if (count > 0)
            {
                if (PlayerStats.instance.currentStamina < PlayerStats.instance.maxStamina)
                {
                    ConsumablesHUDManager.instance.StartConsumableCooldown();
                    if (fullRestore)
                    {
                        PlayerStats.instance.RestoreStamina(PlayerStats.instance.maxStamina - PlayerStats.instance.currentStamina);
                    }
                    else
                    {
                        PlayerStats.instance.RestoreStamina(potency);
                    }
                    count--;

                    SelfEffectsManager.instance.RestoreStaminaEffect();

                    if (!keepInInventoryAtZeroCount && count < 1)
                    {
                        ConsumablesHUDManager.instance.RemoveEquippedConsumableByID(consumable_ID);
                    }
                    ConsumablesHUDManager.instance.UpdateConsumablesHUD();
                }
                else
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Stamina already full");
                }
            }
            
            else
            {
                Debug.Log("Insufficient inventory");
                TextNotificationsManager.instance.NewTextNotifaction("Not enough in inventory");
            }
        }

        public void Elixir()
        {
            if (count > 0)
            {
                if (PlayerStats.instance.currentHealth < PlayerStats.instance.maxHealth
                    || PlayerStats.instance.currentMana < PlayerStats.instance.maxMana
                    || PlayerStats.instance.currentStamina < PlayerStats.instance.maxStamina)
                {
                    ConsumablesHUDManager.instance.StartConsumableCooldown();
                    PlayerStats.instance.RestoreHealth(PlayerStats.instance.maxHealth - PlayerStats.instance.currentHealth);
                    PlayerStats.instance.RestoreMana(PlayerStats.instance.maxMana - PlayerStats.instance.currentMana);
                    PlayerStats.instance.RestoreStamina(PlayerStats.instance.maxStamina - PlayerStats.instance.currentStamina);
                    count--;
                    if (!keepInInventoryAtZeroCount && count < 1)
                    {
                        ConsumablesHUDManager.instance.RemoveEquippedConsumableByID(consumable_ID);
                    }
                    ConsumablesHUDManager.instance.UpdateConsumablesHUD();
                }
                else
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Health, mana, stamina already full");
                }
            }

            else
            {
                Debug.Log("Insufficient inventory");
                TextNotificationsManager.instance.NewTextNotifaction("Not enough in inventory");
            }
        }

        public void SpellTome(int consumableID)
        {
            switch (consumableID)
            {
                case 13:
                    // Ad Spell to Inventory
                    PlayerInventory.instance.GenerateSpell_ArcanaLumina();
                    // Start consumable cooldown
                    ConsumablesHUDManager.instance.StartConsumableCooldown();
                    // Remove from inventory
                    PlayerInventory.instance.consumablesInventory.Remove(this);
                    // Remove from consumable HUD list of consumables
                    ConsumablesHUDManager.instance.RemoveEquippedConsumableByID(consumableID);
                    // Update HUD
                    ConsumablesHUDManager.instance.UpdateConsumablesHUD();
                    // Send UI Notification to Player
                    TextNotificationsManager.instance.NewTextNotifaction("Spell Learned: Arcana Lumina");
                    break;
                default:
                    break;
            }
        }

        public void SpiderVenom()
        {
            PlayerStats.instance.startingPoisonCharges = 10;
            PlayerStats.instance.poisonMultiplier = 1.30f;
            AnimEvents.instance.StartSpiderVenom();
            count--;
            if (!keepInInventoryAtZeroCount && count < 1)
            {
                ConsumablesHUDManager.instance.RemoveEquippedConsumableByID(consumable_ID);
            }
            ConsumablesHUDManager.instance.UpdateConsumablesHUD();
        }

        public void Dig()
        {
            string targetAnimation = "DiggingM";
            if (PlayerStats.instance.gender != "mars") targetAnimation = "DiggingF";

            if (PlayerLocomotion.instance.inCombat)
            {
                PlayerLocomotion.instance.SheatheUnsheatheWeapon(isPerformingAction: true, canMove: false, canRotate: false);
                AnimatorHandler.instance.QueueAction(targetAnimation, true, true, false, false);
            }
            else
            {
                AnimatorHandler.instance.PlayTargetActionAnimation(_targetAnimation: targetAnimation, _isPerformingAction: true, _applyRootMotion: true, _canRotate: false, _canMove: false);
            }
        }
    }
}

