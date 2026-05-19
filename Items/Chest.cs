using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

namespace etchebarren
{
    public class Chest : MonoBehaviour
    {
        public enum RandomLootRarity
        {
            Random,
            None,
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }

        public enum MinimumRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }

        [Header("General Chest Settings")]
        [Tooltip("Chest ID to determine if chest was already opened (between scene loads).")]
        public int chestID = 0;
        [Tooltip("If true, chest will always be openable between scene loads and will not have its ID added to dictionary.")]
        public bool resetWithSceneLoad = false;
        [Tooltip("Name for the chest, used for Interact Prompt. Ex: 'Legendary' Chest.")]
        public string chestRarity = "";

        [Header("Gold")]
        public int baseGoldYield = 10;

        [Header("Determined Loot")]
        [Tooltip("Predetermined items to spawn from the chest when opened. Optional, can leave empty.")]
        public ItemDropElement[] itemsInChest;
        [Tooltip("If true, predetermined items will instantly enter player inventory. Good for crucial quest items.")]
        public bool forceTheseItemsIntoInventory = false;

        [Header("Randomized Loot")]
        [Tooltip("Set or randomize the rarity of randomized item, or do not spawn an item at all.")]
        public RandomLootRarity rarity;
        [Tooltip("Only effective if Rarity is set to Random. Set a minimum rarity for randomization.")]
        public MinimumRarity minimumRarity;

        [Header("Lock Settings")]
        [Tooltip("If null, not locked. Else a key matching this item is required to open chest.")]
        public KeyItem requiredKeyItem;
        [Tooltip("List of enemies that must be defeated to unseal chest.")]
        public List<Enemy> requiredEnemies;
        [Tooltip("Magically sealed? Note: Automatically set true if requiredEnemies is not empty.")]
        public bool magicallySealed = false;

        [Header("General References")]
        public GameObject itemDropPrefab;
        public Animator anim;
        public Transform itemSpawnPoint;

        [Header("Lock Related References")]
        public GameObject padlock;
        public BoxCollider boxCollider;
        public Rigidbody rigibody;
        public Animator padlockAnim;
        public GameObject magicLock;
        public ParticleSystem magicUnlock;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip open;
        public float openVolume = 1f;
        public AudioClip unlock;
        public float unlockVolume = 1f;
        public AudioClip unseal;
        public float unsealVolume = 1f;

        [Header("Minimap")]
        public GameObject minimapIcon;

        [Header("Flags")]
        public bool opened = false;
        public bool alreadyTriggered = false;

        [Header("Temp Values - Do Not Set Manually")]
        public string keyItemName;
        public int requiredKeyItemID;
        public bool keyLostWithUse = false;
       
        void OnEnable()
        {
            // We will not handle anything related to the dictionary if this is a resetting chest
            if (!resetWithSceneLoad)
            {
                // Check if the ChestID is already in use
                if (WorldStateManager.instance.chestDictionary.ContainsKey(chestID))
                {
                    //Debug.Log("Duplicate ChestID found: " + chestID); // This happens even if its a unique key because of reloading scenes, not an error.

                    // Try to get the value associated with the chestID
                    if (WorldStateManager.instance.chestDictionary.TryGetValue(chestID, out bool isOpen))
                    {
                        // The 'isLocked' variable now contains the boolean value for the chestID
                        opened = isOpen;
                    }
                    else
                    {
                        opened = false;
                    }
                    anim.SetBool("isOpen", opened);
                }
                else
                {
                    // Add the chest to the dictionary with its ChestID
                    WorldStateManager.instance.chestDictionary.Add(chestID, opened);
                }
            }

            if (opened)
            {
                minimapIcon.SetActive(false);
                padlock.SetActive(false);
                magicLock.SetActive(false);
            }
            else
            {
                foreach(Enemy enemy in requiredEnemies)
                {
                    enemy.SetChest(this);
                }

                if (requiredKeyItem != null)
                {
                    padlock.SetActive(true);
                }
                else
                {
                    padlock.SetActive(false);
                }

                if(requiredEnemies.Count > 0 || magicallySealed)
                {
                    magicallySealed = true;
                    magicLock.SetActive(true);
                }
                else
                {
                    magicLock.SetActive(false);
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !opened && !alreadyTriggered)
            {
                alreadyTriggered = true;
                InteractPrompt.instance.AddInteraction(this);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player" && alreadyTriggered)
            {
                alreadyTriggered = false;
                InteractPrompt.instance.RemoveInteraction(this);
            }
        }

        public void Open(TextMeshProUGUI keyCountText=null)
        {
            if (!opened)
            {
                // Case: Unlocked and Unsealed
                if (requiredKeyItem == null && !magicallySealed)
                {
                    opened = true;
                    WorldStateManager.instance.chestDictionary[chestID] = opened;
                    InteractPrompt.instance.RemoveInteraction(this);
                    StartCoroutine(OpenDelay());
                }
                // Case: Sealed
                else if (magicallySealed)
                {
                    TextNotificationsManager.instance.NewTextNotifaction("Find a way to release this chest's magical seal", true);
                }
                // Case: Locked, but unsealed
                else
                {
                    KeyItem requiredKey = PlayerInventory.instance.keyItemsInventory.FirstOrDefault(keyItem => keyItem.keyItem_ID == requiredKeyItem.keyItem_ID);

                    if (requiredKey != null && requiredKey.count > 0)
                    {
                        // store key item name for notifaction use later
                        keyItemName = requiredKey.itemName;
                        requiredKeyItemID = requiredKey.keyItem_ID;

                        // remove quantity only if required by key item settings
                        if (requiredKey.lostWithUse)
                        {
                            requiredKey.count--;
                            keyLostWithUse = true;
                        }

                        // remove interaction prompt and start unlock ienumerator
                        InteractPrompt.instance.RemoveInteraction(this);
                        Unlock();

                        // display new key count only if small key
                        if(requiredKeyItemID == 800)
                        {
                            InteractPrompt.instance.keyCount.text = " x " + requiredKey.count;
                        }
                    }
                    else
                    {
                        string message;
                        switch (requiredKeyItemID)
                        {
                            case 800:
                                message = "Not enough small keys";
                                break;
                            default:
                                message = "Missing required key";
                                break;
                        }
                        TextNotificationsManager.instance.NewTextNotifaction(message, true);
                    }
                }
            }
        }

        private IEnumerator OpenDelay()
        {
            audioSource.PlayOneShot(open, openVolume);

            yield return new WaitForSeconds(0.4f); // originally 0.5

            anim.SetBool("isOpen", opened);

            minimapIcon.SetActive(false);

            // Spawn random item
            if(rarity != RandomLootRarity.None)
            {
                // Arguments: player level, spawn, base gold yield, rarity as determined in the inspector, and true to alter item physics, and gold yield
                string randomLootRarity = rarity.ToString().ToLower();
                int minimumLootRarity = (int)minimumRarity;
                ItemGeneratorManager.instance.GenerateRandomItem(PlayerStats.instance.playerLevel, itemSpawnPoint.position, randomLootRarity, minimumLootRarity, true);
                
            }

            // Spawn custom items included in chest
            SpawnCustomItems();

            // Spawn gold
            if(baseGoldYield > 0) ItemGeneratorManager.instance.GenerateGold(PlayerStats.instance.playerLevel, itemSpawnPoint.position, baseGoldYield);

        }

        public void Unlock()
        {
            if (requiredKeyItem != null)
            {
                requiredKeyItem = null;
                //lockedByKey = false;
                StartCoroutine(UnlockDelay());
            }
        }

        private IEnumerator UnlockDelay()
        {
            audioSource.PlayOneShot(unlock, unlockVolume);
            padlockAnim.SetTrigger("Unlock");
            yield return new WaitForSeconds(0.5f);

            string message = keyItemName;
            if (keyLostWithUse) message += " used and discarded.";
            else message += " used.";        
            TextNotificationsManager.instance.NewTextNotifaction(message, true);

            boxCollider.enabled = true;
            rigibody.useGravity = true;

            yield return new WaitForSeconds(0.3f);
            padlockAnim.enabled = false;
            Open();
        }

        public void DeactivateMagicLock()
        {
            if (!opened)
            {      
                StartCoroutine(DeactivateMagicLockDelay());
            }
        }

        private IEnumerator DeactivateMagicLockDelay()
        {
            magicUnlock.Play();
            audioSource.PlayOneShot(unseal, unsealVolume);
            yield return new WaitForSeconds(0.2f);
            magicLock.SetActive(false);
            magicallySealed = false;
        }

        public void SpawnCustomItems()
        {
            if(itemsInChest.Length > 0)
            {
                foreach (ItemDropElement itemDropElement in itemsInChest)
                {

                    float roll = Random.Range(0.0f, 100.0f);
                    if (roll < itemDropElement.dropRate)
                    {
                    
                        int quantity = GetDroppedQuantity(itemDropElement);
                        // Create a new instance of the item
                        Item newItem = Instantiate(itemDropElement.item); // Clone the ScriptableObject
                        newItem.count = quantity; // Set the quantity on the new instance

                        // Drop Item
                        if (forceTheseItemsIntoInventory)
                        {
                            PlayerInventory.instance.AddToInventory(newItem, true);
                            AcquiredNotifications.instance.NewItemNotification(newItem, newItem.count);
                        }
                        else
                        {
                            GameObject spawnedObject = Instantiate(itemDropPrefab, itemSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
                            spawnedObject.GetComponent<ItemDrop>().item = newItem;
                            spawnedObject.GetComponent<ItemDrop>().SetColors(newItem.rarity);
                            spawnedObject.GetComponent<ItemDrop>().SetForcesAndApply(true);
                        }
                    }
                }
            }
        }

        private int GetDroppedQuantity(ItemDropElement itemDrop)
        {
            float roll = Random.Range(0.0f, 100.0f);
            float cumulativeChance = 0f;

            foreach (var quantityChance in itemDrop.quantityChances)
            {
                cumulativeChance += quantityChance.chance;
                if (roll <= cumulativeChance)
                {
                    return quantityChance.quantity;
                }
            }

            return 1; // Default to 1 if no match (quantity chances is left blank)
        }

        public void RemoveEnemy(Enemy enemy)
        {
            requiredEnemies.Remove(enemy);

            if(requiredEnemies.Count < 1)
            {
                DeactivateMagicLock();
            }
        }

    }
}
