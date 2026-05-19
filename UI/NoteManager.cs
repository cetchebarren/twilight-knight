using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class NoteManager : MonoBehaviour
    {
        [Header("References")]
        public InteractPrompt interactPrompt;
        public WorldStateManager worldStateManager;
        public ControllerUIManager controllerUIManager;
        public AcquiredNotifications acquiredNotifications;
        public InputHandler inputHandler;
        public PlayerInventory playerInventory;
        public UIAudioManager uiAudioManager;
        public SetHUD setHUD;
        public GameObject noteUI;
        public GameObject takeButton;
        public GameObject closeButton;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI bodyText;
        public GameObject controlsHUD;
        public Image noteImage;
        public Image mountPortrait;
        public CameraHandler cameraHandler;

        [Header("Temporary Objects")]
        public Readable currentReadable;

        public void Display(Readable readable)
        {
            cameraHandler.freezeCamera = true;

            uiAudioManager.PlayOpenNoteAudio();

            currentReadable = readable;
            SetText(readable.note, titleText, bodyText);
            SetImage(readable.note, noteImage);
            noteUI.SetActive(true);
            inputHandler.isPerformingAction = true;
            inputHandler.canMove = false;
            inputHandler.canRotate = false;
            inputHandler.inMenu = true;
            setHUD.SetHUDActive(false);
            InteractPrompt.instance.keysHUD.SetActive(false);

            bool canGrabNote = currentReadable.canGrab && !worldStateManager.grabbedReadableIDs.Contains(currentReadable.readableID);
 
            if (canGrabNote)
            {
                controllerUIManager.SetSelectText("Take Note");
            }
            controllerUIManager.SetBackText("Leave");
            controlsHUD.SetActive(true);
            closeButton.SetActive(false);
            takeButton.SetActive(false);
        }

        public void Close()
        {
            // If the readable / note has not already been read
            if (!worldStateManager.readReadableIDs.Contains(currentReadable.readableID))
            {
                // If there is an item to give
                if (currentReadable.itemToGivePlayer != null)
                {
                    // Give the item
                    Item item = currentReadable.itemToGivePlayer;
                    playerInventory.AddToInventory(item, true);
                    acquiredNotifications.NewItemNotification(item, item.count);
                    currentReadable.itemToGivePlayer = null;
                }
                // Add the readable ID to the list of read readables to prevent getting item multiple times
                worldStateManager.readReadableIDs.Add(currentReadable.readableID);
            }

            inputHandler.isPerformingAction = false;
            inputHandler.canMove = true;
            inputHandler.canRotate = true;
            inputHandler.inMenu = false;
            noteUI.SetActive(false);
            setHUD.SetHUDActive(true);

            // Reset texts, UI, buttons
            controllerUIManager.SetSelectText("");
            controllerUIManager.SelectTextActive(false);
            controllerUIManager.SetBackText("");
            controllerUIManager.BackTextActive(false);
            takeButton.SetActive(false);
            closeButton.SetActive(false);

            uiAudioManager.PlayCloseNoteAudio();

            currentReadable = null;

            cameraHandler.freezeCamera = false;
        }

        public void SetImage(Note note, Image image)
        {
            image.gameObject.SetActive(false);
            image.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f); // Default
            image.GetComponent<RectTransform>().localPosition = new Vector3(0f, -350f, 0f); // Default

            if (note != null)
            {
                if (note.noteID == 902)
                {
                    if (mountPortrait.sprite != null)
                    {
                        note.sprite = mountPortrait.sprite;
                    }
                }

                if (note.sprite != null)
                {
                    image.sprite = note.sprite;
                    image.GetComponent<RectTransform>().localScale = note.imageScale;
                    image.GetComponent<RectTransform>().localPosition = new Vector3(note.hOffset, -350f + note.vOffset, 0f); // Default
                    image.gameObject.SetActive(true);
                }
            }
        }

        public void SetText(Note note, TextMeshProUGUI titleTMP, TextMeshProUGUI bodyTMP)
        {
            if (note != null)
            {
                string title = note.titleContent;
                string body = note.bodyContent;

                if (note.noteID == 902)
                {
                    body = body.Replace("{KNIGHT}", PlayerStats.instance.playerName);
                    body = body.Replace("{HORSENAME}", MountStats.instance.horseName);
                    body = body.Replace("{HORSELEVEL}", MountStats.instance.level.ToString("0"));
                    body = body.Replace("{HORSEHP}", MountStats.instance.maxHealth.ToString("0"));
                    body = body.Replace("{HORSESTAMINA}", MountStats.instance.maxStamina.ToString("0"));
                    body = body.Replace("{HORSESTRENGTH}", MountStats.instance.attack.ToString("0"));

                    string bonuses = "None";
                    string armorDamageReduction = (MountStats.instance.horseArmorDamageTakenMultiplier * 100f).ToString("0") + "%";
                    string hornDamageBonus = ((MountStats.instance.horseHornDamageDealtMultiplier - 1) * 100f).ToString("0") + "%";

                    if (MountStats.instance.horseArmorOwned || MountStats.instance.horseHornOwned) bonuses = "";

                    if (MountStats.instance.horseArmorOwned)
                    {
                        bonuses += "\n" + armorDamageReduction + " Damage Negation (<i>Armor</i>)";
                    }

                    if (MountStats.instance.horseHornOwned)
                    {
                        bonuses += "\n" + hornDamageBonus + " Damage Bonus (<i>Horn</i>)";
                    }

                    body = body.Replace("{HORSEBONUSES}", bonuses);
                }

                titleTMP.text = title;
                bodyTMP.text = body;
            }
            else
            {
                titleTMP.text = "TITLE CONTENT ERROR";
                bodyTMP.text = "BODY CONTENT ERROR";
            }
        }

        public void GrabNote()
        {
            // If the readable / note has not already been read
            if (!worldStateManager.grabbedReadableIDs.Contains(currentReadable.readableID))
            {
                worldStateManager.grabbedReadableIDs.Add(currentReadable.readableID);

                KeyItem note = ScriptableObject.CreateInstance<KeyItem>();
                note.itemName = currentReadable.keyItemName;
                note.itemIcon = playerInventory.keyItemIcons[2];
                note.keyItemSpriteID = 2;
                note.itemDescription = currentReadable.grabbedNoteItemDescription;
                note.itemID = currentReadable.readableID;
                note.sellable = currentReadable.sellable;
                note.goldValue = currentReadable.goldValue;
                note.rarity = currentReadable.keyItemRarity % 5;
                note.count = 1;
                note.keyItem_ID = currentReadable.readableID;
                note.note = currentReadable.note;

                //playerInventory.keyItemsInventory.Add(note);
                playerInventory.AddToInventory(note, true);
                acquiredNotifications.NewItemNotification(note, note.count);

                // Disable the root parent GameObject
                currentReadable.gameObject.transform.parent.gameObject.SetActive(false);

                interactPrompt.RemoveInteraction(currentReadable);

                Debug.Log("Note Grabbed");
            }

            Close();
        }
    }
}
