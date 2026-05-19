using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace etchebarren
{
    [Serializable]
    public class DialogueLine
    {
        [TextArea(5,10)]public string line;
        public AudioClip clip;
    }

    public class DialogueAgent : MonoBehaviour
    {
        [Header("Agent Settings")]
        public string agentName = "NPC";
        public bool read = false; // for prompt, if read instead of talk
        public int[] associatedQuestIDs;

        public DialogueLine[] introduction;
        public DialogueLine[] defaultLines;

        public GameObject availableQuestMarker;

        public SphereCollider interactTrigger;
        private Vector3 originalPosition;
        private Vector3 upPosition;
        public NPC thisNPC;
        public QuestStepMarker questStepMarker;

        public AudioSource audioSource;
        public float vocalVolume = 1.0f;

        /* Flags */
        private bool alreadyTriggered = false;
        private int lastVocalIndex = 0;

        void Start()
        {
            CheckQuestAvailability();

            // Record original position
            originalPosition = interactTrigger.center;
            // Calculate the position to move up to (for "resetting" the trigger)
            upPosition = originalPosition + Vector3.up * 20.0f;
        }

        void OnDisable()
        {
            alreadyTriggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered)
            {
                alreadyTriggered = true;
                InteractPrompt.instance.AddInteraction(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                alreadyTriggered = false;
                InteractPrompt.instance.RemoveInteraction(this);
            }
        }

        public void PlayVocal()
        {
            // Prevent Audio Overlap
            if (audioSource.isPlaying) return;
            audioSource.volume = vocalVolume;
            if(audioSource.clip != null) audioSource.Play();
        }

        public void CheckQuestAvailability()
        {
            foreach(int ID in associatedQuestIDs)
            {
                Quest quest = QuestManager.instance.GetQuestByID(ID);
                if(!quest.completed && !quest.questActive && quest.questAvailable)
                {
                    availableQuestMarker.SetActive(true);
                    return;
                }
                else
                {
                    availableQuestMarker.SetActive(false);
                }
            }
        }

        public void ResetTrigger()
        {
            StartCoroutine(ResetTriggerCoroutine());
        }

        private IEnumerator ResetTriggerCoroutine()
        {
            //This actually works by moving the trigger away and back into place
            interactTrigger.center = upPosition;
            alreadyTriggered = false;
            yield return new WaitForSeconds(0.1f);
            interactTrigger.center = originalPosition;
        }
    }
}
