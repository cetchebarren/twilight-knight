using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace etchebarren
{
    public class Door : MonoBehaviour
    {
        [Header("Information")]
        public int doorID;
        public string doorName = "Door";

        [Header("Lock Info")]
        public KeyItem requiredKeyItem;

        [Header("References")]
        public Animator anim;
        public GameObject frontCollider;
        public GameObject rearCollider;

        [Header("Audio")]
        public AudioSource openAudio;
        public float audioDelay = 0f;

        [Header("Flags")]
        public bool opened = false;
        public bool playerInFront = true;

        void Awake()
        {
            if (requiredKeyItem == null) return;
            if (WorldStateManager.instance.unlockedDoorIDs.Contains(doorID))
            {
                requiredKeyItem = null;
            }
        }

        public void Open()
        {
            // Locked
            if (requiredKeyItem != null)
            {
                // Find key in player inventory
                KeyItem requiredKey = PlayerInventory.instance.keyItemsInventory.FirstOrDefault(keyItem => keyItem.keyItem_ID == requiredKeyItem.keyItem_ID);

                string message = requiredKeyItem.itemName;
                // If key was not found in inventory
                if (requiredKey == null || requiredKey.count < 1)
                {
                    message = "Missing " + message;
                    TextNotificationsManager.instance.NewTextNotifaction(message, true);
                    return;
                }

                // Remove key from inventory if necessary

                if (requiredKey.lostWithUse)
                {
                    message += " used and discarded.";
                    requiredKey.count--;
                    // display new key count only if small key
                    if (requiredKeyItem.keyItem_ID == 800)
                    {
                        InteractPrompt.instance.keyCount.text = " x " + requiredKey.count;
                    }
                }                                
                else message += " used.";

                TextNotificationsManager.instance.NewTextNotifaction(message, true);
                WorldStateManager.instance.unlockedDoorIDs.Add(doorID);
            }

            if (playerInFront)
            {
                anim.SetBool("openForward", true);
            }
            else
            {
                anim.SetBool("openBackward", true);
            }

            frontCollider.SetActive(false);
            rearCollider.SetActive(false);

            if (openAudio != null) StartCoroutine(PlayOpenAudio(audioDelay));

            InteractPrompt.instance.RemoveInteraction(this);
        }

        private IEnumerator PlayOpenAudio(float delay)
        {
            yield return new WaitForSeconds(delay);
            openAudio.Play();
        }
    }
}
