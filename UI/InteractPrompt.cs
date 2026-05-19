using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class InteractPrompt : MonoBehaviour
    {
        public static InteractPrompt instance;

        public OptionsManager optionsManager;
        public ControllerUIManager controllerUIManager;

        [System.Serializable]
        public struct Interaction
        {
            public ItemDrop itemDrop;
            public Chest chest;
            public DialogueAgent dialogueAgent;
            public TeleportWaypoint teleportWaypoint;
            public SceneExit sceneExit;
            public ClimbableEdge climbableEdge;
            public PushableObject pushableObject;
            public Readable readable;
            public string shopName;
            public string miscAction;
            public Door door;
            public InteractEvent interactEvent;
        }

        [Header("Interact Prompt Settings")]
        public Image interactIcon;
        public GameObject computerKeyText;
        public TextMeshProUGUI interactText;
        public GameObject interactContainer;
        public DialogueMenu dialogueMenu;

        [Header("Flags and Temp Values")]
        public int currentActionIndex = 0;
        public bool interactActive = false;
        public string interactType = "none";
        public DialogueAgent currentDialogueAgent;
        public TeleportWaypoint currentTeleportWaypoint;
        public Chest currentChest;
        public ItemDrop currentItemDrop;
        public SceneExit currentSceneExit;
        public Readable currentReadable;
        public Door currentDoor;
        public InteractEvent currentInteractEvent;

        [Header("Interaction Queue/List")]
        public List<Interaction> interactions = new List<Interaction>();

        [Header("Keys HUD References")]
        public GameObject keysHUD;
        private Coroutine keyCountdown;
        public TextMeshProUGUI keyCount;

        [Header("Item Drops")]
        private string[] itemTypes = { "Sword", "Shield", "Chest Armor", "Gauntlets", "Greaves", "Ring", "Amulet" };
        private string[] rarityTexts = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        public float pickUpDelay = 0.8f;
        private bool looting = false;

        [Header("Text Backgrounds")]
        public float minOffset = 90f;
        public float maxOffset = 170f;
        public float minPrefWidth = 50f;
        public float maxPrefWidth = 270f;
        public GameObject textBackground;
        public GameObject textBackgroundSwitch;
        public GameObject switchActionContainer;

        private void Awake()
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
            bool controller = controllerUIManager.isUsingController();
            SetInteractIcon(controller);
        }

        public void SetInteractIcon(bool controller)
        {
            if (controller)
            {
                computerKeyText.SetActive(false);
                if (ControllerUIManager.instance.controllerTypePlaystation)
                {
                    interactIcon.sprite = ControllerUIManager.instance.X;
                }
                else
                {
                    interactIcon.sprite = ControllerUIManager.instance.A;
                }
            }
            else
            {
                computerKeyText.SetActive(true);
                interactIcon.sprite = ControllerUIManager.instance.computerKey;
            }
        }

        public void DisableInteractPrompt()
        {
            interactActive = false;
            interactContainer.SetActive(false);
            currentActionIndex = 0;

            interactType = "none";
            currentDialogueAgent = null;
            currentTeleportWaypoint = null;
            currentChest = null;
            currentItemDrop = null;
            currentSceneExit = null;
            currentReadable = null;
            currentDoor = null;
            currentInteractEvent = null;
        }

        // INTERACTION LIST

        public void EnableInteractPrompt(int index=0)
        {
            bool resizeText = true;
            //SetInteractIcon();

            // If there are no interactions in the list, do not display any prompt
            if (interactions.Count < 1)
            {
                DisableInteractPrompt();
                return;
            }

            // If the interaction is a chest, display appropriate text
            else if (interactions[index].chest != null)
            {
                interactType = "Chest";
                currentChest = interactions[index].chest;
                if (currentChest.requiredKeyItem != null || currentChest.requiredEnemies.Count > 0)
                {
                    if (currentChest.requiredEnemies.Count > 0)
                    {
                        interactText.text = currentChest.chestRarity + " Chest (Sealed)";
                    }
                    else if (currentChest.requiredKeyItem != null)
                    {
                        interactText.text = "Unlock with " + currentChest.requiredKeyItem.itemName;
                    }
                    else
                    {
                        Debug.LogError("Unexpected condition with chest prompt based on Lock conditions");
                    }

                }
                else
                {
                    interactText.text = "Open " + currentChest.chestRarity + " Chest";
                }
                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is an item drop, display appropriate text
            else if (interactions[index].itemDrop != null)
            {
                interactType = "Item";
                Debug.Log("setting Current Item drop set to: " + interactions[index].itemDrop);

                currentItemDrop = interactions[index].itemDrop;

                Debug.Log("Current Item drop: " + currentItemDrop);

                string rarity = rarityTexts[currentItemDrop.item.rarity];

                bool addRarity = true;

                switch (currentItemDrop.item)
                {
                    case WeaponItem weaponItem:
                        interactText.text = "Pick Up " + itemTypes[0];
                        break;
                    case ShieldItem shieldItem:
                        interactText.text = "Pick Up " + itemTypes[1];
                        break;
                    case TorsoArmorItem torsoArmorItem:
                        interactText.text = "Pick Up " + itemTypes[2];
                        break;
                    case HandsArmorItem handsArmorItem:
                        interactText.text = "Pick Up " + itemTypes[3];
                        break;
                    case LegsArmorItem legsArmorItem:
                        interactText.text = "Pick Up " + itemTypes[4];
                        break;
                    case RingItem ringItem:
                        interactText.text = "Pick Up " + itemTypes[5];
                        break;
                    case AmuletItem amuletItem:
                        interactText.text = "Pick Up " + itemTypes[6];
                        break;
                    case Spell spellItem:
                        interactText.text = "Pick Up " + currentItemDrop.item.itemName;
                        addRarity = false;
                        break;
                    case Consumable consumableItem:
                        interactText.text = "Pick Up " + currentItemDrop.item.itemName;
                        addRarity = false;
                        break;
                    case KeyItem keyItem:
                        interactText.text = "Pick Up " + keyItem.itemName;
                        addRarity = false;
                        break;
                    default:
                        Debug.Log("Add to Inventory Type Error");
                        break;
                }

                if (addRarity)
                {
                    switch (rarity)
                    {
                        case "Common":
                            interactText.text += " (<color=#D3D3D3>Common</color>)"; // Light Grey
                            break;
                        case "Uncommon":
                            interactText.text += " (<color=#00FF00>Uncommon</color>)"; // Lime
                            break;
                        case "Rare":
                            interactText.text += " (<color=#00FFFF>Rare</color>)"; // Aqua
                            break;
                        case "Epic":
                            interactText.text += " (<color=#FF69B4>Epic</color>)"; //pinkish purple
                            break;
                        case "Legendary":
                            interactText.text += " (<color=#FFD700>Legendary</color>)"; // Gold-like
                            break;
                    }
                }

                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a dialogue agent, display appropriate text
            else if (interactions[index].dialogueAgent != null)
            {
                interactType = "Dialogue";

                if (interactions[index].dialogueAgent.read)
                {
                    interactText.text = "Read " + interactions[index].dialogueAgent.agentName;
                }
                else
                {
                    interactText.text = "Talk to " + interactions[index].dialogueAgent.agentName;
                }

                dialogueMenu.SetDialogueAgent(interactions[index].dialogueAgent);
                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a teleport waypoint, display appropriate text
            else if (interactions[index].teleportWaypoint != null)
            {
                interactType = "Activate Waypoint";
                interactText.text = "Activate Waypoint";
                interactContainer.SetActive(true);
                interactActive = true;
                currentTeleportWaypoint = interactions[index].teleportWaypoint;
            }

            // If the interaction is a teleport waypoint, display appropriate text
            else if (interactions[index].sceneExit != null)
            {
                interactType = "Scene Exit";
                string newLocation = AddSpacesBeforeCaps(interactions[index].sceneExit.sceneToGoTo); //Example: "RollingHills" becomes "Rolling Hills"
                currentSceneExit = interactions[index].sceneExit;
                interactText.text = "Go to " + newLocation;
                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a climbable edge, display appropriate text
            else if (interactions[index].climbableEdge != null)
            {
                interactType = "Climb";
                interactText.text = "Climb";
                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a pushable object, display appropriate text
            else if (interactions[index].pushableObject != null)
            {
                interactType = "Push";
                interactText.text = "Push";
                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a readable, display appropriate text
            else if (interactions[index].readable != null)
            {
                interactType = "Read";
                currentReadable = interactions[index].readable;

                interactText.text = "Read";

                if(currentReadable.interactableName != "")
                {
                    interactText.text += " " + currentReadable.interactableName;
                }

                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a readable, display appropriate text
            else if (interactions[index].door != null)
            {
                interactType = "Door";
                currentDoor = interactions[index].door;

                if (currentDoor.requiredKeyItem != null)
                {
                    interactText.text = "Unlock with " + currentDoor.requiredKeyItem.itemName;
                }
                else
                {
                    interactText.text = "Open " + currentDoor.doorName;
                }

                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a shop, display appropriate text
            else if (!string.IsNullOrEmpty(interactions[index].shopName))
            {        
                if (interactions[index].shopName.Contains("Stables"))
                {
                    interactType = "Stables Shop";
                }
                else
                {
                    interactType = "Shop";
                }
                interactText.text = interactType;
                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a readable, display appropriate text
            else if (interactions[index].interactEvent != null)
            {
                interactType = "InteractEvent";
                currentInteractEvent = interactions[index].interactEvent;

                interactText.text = currentInteractEvent.GetInteractText();

                interactContainer.SetActive(true);
                interactActive = true;
            }

            // If the interaction is a misc action, display appropriate text
            else if (!string.IsNullOrEmpty(interactions[index].miscAction))
            {
                interactType = interactions[index].miscAction;
                interactText.text = interactType;
                interactContainer.SetActive(true);
                interactActive = true;
            }

            else
            {
                resizeText = false;
                DisableInteractPrompt();
                // In the case that the interaction was invalid (the Interaction had no non-null elements), remove it
                Debug.LogError("An interaction was removed from the list as all of its elements were null. This should not occur, but this failsafe was triggered to remove it.");
                interactions.RemoveAt(index);
            }

            if (resizeText) ResizeTextBackground();

            // If we have more than one message, display switch action prompt
            if(interactions.Count > 1)
            {
                textBackground.SetActive(false);
                textBackgroundSwitch.SetActive(true);
                switchActionContainer.SetActive(true);
            }
            else
            {
                textBackground.SetActive(true);
                textBackgroundSwitch.SetActive(false);
                switchActionContainer.SetActive(false);
            }
        }

        public void ResizeTextBackground()
        {
            // Update text box size to fit text better
            Vector2 preferredSize = interactText.GetPreferredValues();
            interactText.GetComponent<RectTransform>().sizeDelta = new Vector2(preferredSize.x, 40f);

            float scaledOffset = ScaleBackgroundOffset(preferredSize.x);
            textBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(preferredSize.x + scaledOffset, 73.25f);
            textBackgroundSwitch.GetComponent<RectTransform>().sizeDelta = new Vector2(preferredSize.x + scaledOffset, 154.3f);
        }

        #region Chests
        public void AddInteraction(Chest chest)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.chest = chest;
            interactions.Insert(0, newInteraction);

            if (chest.requiredKeyItem != null)
            {
                if (keyCountdown != null)
                {
                    StopCoroutine(keyCountdown);
                    keyCountdown = null;
                }

                if(chest.requiredKeyItem.keyItem_ID == 800)
                {
                    keysHUD.SetActive(true);

                    KeyItem smallKey = PlayerInventory.instance.keyItemsInventory.FirstOrDefault(keyItem => keyItem.keyItem_ID == 800);
                    if (smallKey != null)
                    {
                        keyCount.text = " x " + smallKey.count.ToString();
                    }
                    else
                    {
                        keyCount.text = " x 0";
                    }
                }
            }
            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(Chest chest)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].chest == chest)
                {
                    interactions.RemoveAt(i);
                    if (keyCountdown == null)
                    {
                        keyCountdown = StartCoroutine(KeyHUDCountdown());
                    }
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Items
        public void AddInteraction(ItemDrop itemDrop)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.itemDrop = itemDrop;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(ItemDrop itemDrop)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].itemDrop == itemDrop)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Dialogue Agents
        public void AddInteraction(DialogueAgent dialogueAgent)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.dialogueAgent = dialogueAgent;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(DialogueAgent dialogueAgent)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].dialogueAgent == dialogueAgent)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Waypoints
        public void AddInteraction(TeleportWaypoint teleportWaypoint)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.teleportWaypoint = teleportWaypoint;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(TeleportWaypoint teleportWaypoint)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].teleportWaypoint == teleportWaypoint)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Scene Exits
        public void AddInteraction(SceneExit sceneExit)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.sceneExit = sceneExit;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(SceneExit sceneExit)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].sceneExit == sceneExit)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Climbable Edges
        public void AddInteraction(ClimbableEdge climbableEdge)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.climbableEdge = climbableEdge;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(ClimbableEdge climbableEdge)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].climbableEdge == climbableEdge)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Pushable Objects
        public void AddInteraction(PushableObject pushableObject)
        {
            Debug.Log("Pushable interaction added");
            Interaction newInteraction = new Interaction();
            newInteraction.pushableObject = pushableObject;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(PushableObject pushableObject)
        {
           // Debug.Log("Should remove pushable object interaction: " + pushableObject);
            //Debug.Log("Before: " + interactions);
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].pushableObject == pushableObject)
                {
                    interactions.RemoveAt(i);
                    //Debug.Log("Found at index: " + i);
                    break; // Exit the loop once the interaction is removed
                }
            }
            //Debug.Log("After: " + interactions);
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Readables
        public void AddInteraction(Readable readable)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.readable = readable;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(Readable readable)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].readable == readable)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Doors
        public void AddInteraction(Door door)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.door = door;
            interactions.Insert(0, newInteraction);

            if (door.requiredKeyItem != null)
            {
                if (keyCountdown != null)
                {
                    StopCoroutine(keyCountdown);
                    keyCountdown = null;
                }

                if (door.requiredKeyItem.keyItem_ID == 800)
                {
                    keysHUD.SetActive(true);

                    KeyItem smallKey = PlayerInventory.instance.keyItemsInventory.FirstOrDefault(keyItem => keyItem.keyItem_ID == 800);
                    if (smallKey != null)
                    {
                        keyCount.text = " x " + smallKey.count.ToString();
                    }
                    else
                    {
                        keyCount.text = " x 0";
                    }
                }
            }

            CheckAndDisplayInteraction();
        }

        public void RemoveInteraction(Door door)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].door == door)
                {
                    interactions.RemoveAt(i);
                    if (keyCountdown == null)
                    {
                        keyCountdown = StartCoroutine(KeyHUDCountdown());
                    }
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Shops
        public void AddShopInteraction(string shopName)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.shopName = shopName;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveShopInteraction(string shopName)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].shopName == shopName)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Event on Interact
        public void AddEventInteraction(InteractEvent ie)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.interactEvent = ie;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveEventInteraction(InteractEvent ie)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].interactEvent == ie)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        #region Misc
        public void AddMiscInteraction(string action)
        {
            Interaction newInteraction = new Interaction();
            newInteraction.miscAction = action;
            interactions.Insert(0, newInteraction);
            CheckAndDisplayInteraction();
        }

        public void RemoveMiscInteraction(string action)
        {
            int index = 0;
            for (int i = 0; i < interactions.Count; i++)
            {
                if (interactions[i].miscAction == action)
                {
                    interactions.RemoveAt(i);
                    index = i;
                    break; // Exit the loop once the interaction is removed
                }
            }
            CheckAndDisplayInteraction();
        }
        #endregion

        // MISC.
        public void CheckAndDisplayInteraction()
        {
            if(interactions.Count < 1)
            {
                DisableInteractPrompt();
            }
            else
            {
                while (currentActionIndex >= interactions.Count && currentActionIndex > 0)
                {
                    currentActionIndex--;
                }
                // If index becomes negative, disable the prompt as there are no valid interactions
                if (currentActionIndex < 0)
                {
                    DisableInteractPrompt();
                }
                else
                {
                    EnableInteractPrompt(currentActionIndex);
                }
            }
        }

        public IEnumerator KeyHUDCountdown()
        {
            //Debug.Log("Started");
            yield return new WaitForSeconds(5.0f);
            keysHUD.SetActive(false);
            //Debug.Log("Ended");
            keyCountdown = null;
        }

        public string AddSpacesBeforeCaps(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);

            for (int i = 1; i < text.Length; i++)
            {
                // Insert space before capital letter
                if (char.IsUpper(text[i]))
                {
                    newText.Append(' ');
                }
                newText.Append(text[i]);
            }

            return newText.ToString();
        }

        public void PickUpItem()
        {
            if(currentItemDrop == null)
            {
                return;
            }

            if (looting)
            {
                Pickup();
            }
            else
            {
                StartCoroutine(PickUpDelay());
            }
        }

        private IEnumerator PickUpDelay()
        {
            looting = true;

            if (!InputHandler.instance.isPerformingAction && !InputHandler.instance.isAttacking)
            {
                // True/False here indicates whether we are opening a chest
                PlayerLocomotion.instance.StartLooting(false);
            }

            yield return new WaitForSeconds(pickUpDelay);

            if (currentItemDrop != null)
            {
                Pickup();
            }

            looting = false;
        }

        private void Pickup()
        {
            // If the item is a gold drop
            if(currentItemDrop.goldAmount > 0)
            {
                ItemGeneratorManager.instance.GenerateGold(PlayerStats.instance.playerLevel, currentItemDrop.spawnedLocation + new Vector3(0f,0.05f,0f), currentItemDrop.goldAmount);
            }
            else // if the item is a standard inventory Item
            {
                // Add Item to Inventory. Bool represents playing audio.
                PlayerInventory.instance.AddToInventory(currentItemDrop.item, true);
                // Notify Player
                //Debug.Log(itemDropsInRange[currentSelection].item.count);
                AcquiredNotifications.instance.NewItemNotification(currentItemDrop.item, currentItemDrop.item.count);
            }

            //If the item is a quest item we need to update the quest item dictionary to record that this item has been grabbed
            currentItemDrop.questItemSpawnControl?.UpdateDictionary(true);

            // If the item is a world item, we need to update the world item list to record that this item has been grabbed
            currentItemDrop.worldItem?.UpdateWorldItemList();

            // Temporary variable to remember item drop object
            ItemDrop toDestroy = currentItemDrop;

            // Set current item drop to null to prevent picking it up again when spamming input
            currentItemDrop = null;

            // Remove item drop object from item drops in range list
            RemoveInteraction(toDestroy);

            // Destroy world item if item drop is child of world item
            if (toDestroy.worldItem != null)
            {
                Destroy(toDestroy.worldItem.gameObject);
            }
            else // If not world item, just destroy the item drop
            {
                Destroy(toDestroy.gameObject);
            }
        }

        // Function for scaling the width of the background text box based on width of text
        public float ScaleBackgroundOffset(float refValue)
        {
            // If refValue is less than or equal to lowRef, return lowestWidth
            if (refValue <= minPrefWidth)
            {
                return minOffset;
            }

            // If refValue is greater than or equal to highRef, return highestWidth
            if (refValue >= maxPrefWidth)
            {
                return maxOffset;
            }

            // Calculate the proportionate width between lowestWidth and highestWidth
            float t = (refValue - minPrefWidth) / (maxPrefWidth - minPrefWidth);
            return Mathf.Lerp(minOffset, maxOffset, t);
        }

        public void SwitchAction(bool right = true)
        {
            if (interactions.Count == 0)
            {
                currentActionIndex = 0;
                return;
            }

            if (right)
            {
                currentActionIndex++;
                if (currentActionIndex > interactions.Count - 1)
                {
                    currentActionIndex = 0;
                }

            }
            else
            {
                currentActionIndex--;
                if (currentActionIndex < 0)
                {
                    currentActionIndex = interactions.Count - 1; // Get the index of the last element
                }
            }

            CheckAndDisplayInteraction();

        }

        public void ClearInteractionList()
        {
            currentActionIndex = 0;
            interactions.Clear();
            DisableInteractPrompt();
            currentChest = null;
            currentDialogueAgent = null;
            currentItemDrop = null;
            currentSceneExit = null;
            currentTeleportWaypoint = null;
            currentDoor = null;
            currentInteractEvent = null;
        }

        public Interaction GetCurrentInteraction()
        {
            return interactions[currentActionIndex];
        }

    }
}
