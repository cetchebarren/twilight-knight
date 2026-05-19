using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class Readable : MonoBehaviour
    {
        [Header("References")]
        public Note note;
        public GameObject parent;
        
        [Header("Optional Settings")]
        public bool canGrab = false;
        public string interactableName = "Note";
        public int keyItemRarity = 0;
        public Item itemToGivePlayer;
        public string keyItemName = "NOTE";
        public bool sellable = false;
        public int goldValue = 0;
        [TextArea(3, 10)]
        public string grabbedNoteItemDescription = "";

        [Header("Set Automatically")]
        public int readableID = -1;

        [Header("Flags")]
        private bool alreadyTriggered = false;

        public void Start()
        {
            if(note != null)
            {
                readableID = note.noteID;

                if (WorldStateManager.instance.grabbedReadableIDs.Contains(readableID))
                {
                    parent.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Issue with note object assignment, destroying: " + gameObject.name);
                Destroy(parent);
            }
        }

        public void OnEnable()
        {

            if (WorldStateManager.instance.grabbedReadableIDs.Contains(readableID))
            {
                parent.SetActive(false);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered)
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
    }
}
