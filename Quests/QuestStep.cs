using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Needed for OfType and FirstOrDefault methods

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Quests/QuestStep")]

    public class QuestStep : ScriptableObject
    {
        public enum StepType
        {
            Kill,
            Talk,
            Collect,
            Deliver,
            Reach,
            Misc
        }

        [Header("Quest Step Information")]
        public Quest quest;
        public string questStepDescription = "";
        public int questStepID = -1;
        [Tooltip("The dialogue that will play when speaking to the quest giver & this quest step is active.")]
        public DialogueLine[] dialogueLines;
        [Tooltip("Location used to determine scene transitions to mark. Enter direct scene name (no spaces).")]
        public string questStepSceneLocation;

        [Header("General Settings")]
        public StepType questStepType;
        public int quantityRequired = 1;
        public bool resetProgressOnDeathOrSceneExit = false;
        
        [Header("Collect/Deliver Settings")]
        public int requiredItemID = -1;

        [Header("Talk/Deliver Settings")]
        public string dialogueAgentName = "NPC";
        [Tooltip("The dialogue that will play when speaking to the quest giver & this quest step is active.")]
        public DialogueLine[] deliveredLines;

        [Header("Not Set Manually:")]
        public QuestStepMarker questStepMarker;
        public bool stepActive = false;
        public bool stepComplete = false;
        public int currentQuantity = 0;

        public void EvaluateStep()
        {
            if (stepComplete) return;

            switch (questStepType)
            {
                case StepType.Kill:
                case StepType.Misc:
                case StepType.Collect:
                    if (currentQuantity >= quantityRequired)
                    {
                        stepComplete = true;
                        stepActive = false;
                        TextNotificationsManager.instance.NewTextNotifaction("Quest Step Complete.", true); //true prevents duplicate messages for 3 seconds
                        quest.ProgressToNextStep();
                    }
                    break;

                case StepType.Reach:
                    stepComplete = true;
                    stepActive = false;
                    TextNotificationsManager.instance.NewTextNotifaction("Quest Step Complete.", true); //true prevents duplicate messages for 3 seconds
                    quest.ProgressToNextStep();
                    break;

                case StepType.Deliver:
                    stepComplete = true;
                    stepActive = false;
                    TextNotificationsManager.instance.NewTextNotifaction("Quest Step Complete.", true); //true prevents duplicate messages for 3 seconds
                    quest.ProgressToNextStep();
                    break;

                case StepType.Talk:
                    stepComplete = true;
                    stepActive = false;
                    TextNotificationsManager.instance.NewTextNotifaction("Quest Step Complete.", true); //true prevents duplicate messages for 3 seconds
                    quest.ProgressToNextStep();
                    break;

                default:
                    break;
            }
        }

        public void AddProgress(int amount=1)
        {
            currentQuantity += amount;
            // Update tracked quest HUD
            QuestManager.instance.UpdateActiveQuestHUD();
            if(stepActive)
                EvaluateStep();
        }

        public void ResetProgress()
        {
            currentQuantity = 0;
            QuestManager.instance.UpdateActiveQuestHUD();
        }

        public Item CheckDelivery()
        {
            // Combine all inventory lists into a single list of Items
            List<Item> allItems = PlayerInventory.instance.weaponsInventory.OfType<Item>()
                .Concat(PlayerInventory.instance.shieldsInventory)
                .Concat(PlayerInventory.instance.torsoArmorInventory)
                .Concat(PlayerInventory.instance.handsArmorInventory)
                .Concat(PlayerInventory.instance.legsArmorInventory)
                .Concat(PlayerInventory.instance.ringsInventory)
                .Concat(PlayerInventory.instance.amuletsInventory)
                .Concat(PlayerInventory.instance.spellsInventory)
                .Concat(PlayerInventory.instance.consumablesInventory)
                .Concat(PlayerInventory.instance.keyItemsInventory)
                .ToList();

            // Search for the item with the given itemID
            Item item = allItems.FirstOrDefault(i => i.itemID == requiredItemID);

            // If item is found and its count is at least requiredQuantity, return that item
            if (item != null && item.count >= quantityRequired)
            {
                return item;
            }

            // Otherwise, return false
            return null;
        }

        public void CheckCollectOnStartStep()
        {
            // Combine all inventory lists into a single list of Items
            List<Item> allItems = PlayerInventory.instance.weaponsInventory.OfType<Item>()
                .Concat(PlayerInventory.instance.shieldsInventory)
                .Concat(PlayerInventory.instance.torsoArmorInventory)
                .Concat(PlayerInventory.instance.handsArmorInventory)
                .Concat(PlayerInventory.instance.legsArmorInventory)
                .Concat(PlayerInventory.instance.ringsInventory)
                .Concat(PlayerInventory.instance.amuletsInventory)
                .Concat(PlayerInventory.instance.spellsInventory)
                .Concat(PlayerInventory.instance.consumablesInventory)
                .Concat(PlayerInventory.instance.keyItemsInventory)
                .ToList();

            // Search for the item with the given itemID
            Item item = allItems.FirstOrDefault(i => i.itemID == requiredItemID);

            // If item is found, add collected quantity to quest step progress
            if (item != null)
            {
                currentQuantity += item.count;

                if(currentQuantity >= quantityRequired)
                {
                    EvaluateStep();
                }
            }
        }
        public void CheckMiscOnStartStep()
        {
            if(currentQuantity >= quantityRequired)
            {
                EvaluateStep();
            }
        }

        // Quest Marker Functions

        public void GetQuestMarker()
        {
            //Debug.Log("Calling GetQuestMarker for queststep: " + questStepDescription);

            if (QuestMarkerManager.instance != null)
            {
                if (QuestMarkerManager.instance.sceneQuestMarkersManager != null)
                {
                    questStepMarker = QuestMarkerManager.instance.sceneQuestMarkersManager.GetQuestMarker(questStepID);

                    if(questStepMarker == null) // The marker isn't in the scene, we will set the quest marker as the marker pointing to another scene
                    {
                        //Debug.Log(WorldStateManager.instance.sceneWeb);
                        string nextSceneName = WorldStateManager.instance.sceneWeb.GetNextScene(questStepSceneLocation);
                        //Debug.Log("Marker not in this scene. Target scene: " + questStepSceneLocation + ". Next scene name (from searching scene web): " + nextSceneName);
                        if(nextSceneName != null)
                        {
                            questStepMarker = QuestMarkerManager.instance.sceneQuestMarkersManager.GetQuestMarkerForScene(nextSceneName);
                        }
                    }

                    if(questStepMarker == null) // Still no marker, meaning objective is in this scene but has no marker assigned
                    {
                        if(questStepType == StepType.Talk || questStepType == StepType.Deliver)
                        {
                            questStepMarker = QuestMarkerManager.instance.sceneQuestMarkersManager.GetQuestMarkerForDialogueAgent(dialogueAgentName);
                        }
                    }
                }
            }
        }

        public void TrackQuestMarker()
        {
            GetQuestMarker();
            if (questStepMarker != null)
            {
                questStepMarker.TrackMarker();
            }
        }

        public void UntrackQuestMarker()
        {
            //GetQuestMarker(); // disabled, seems it was making excess calls to QuestMarkerManager.instance.sceneQuestMarkersManager.GetQuestMarkerForScene(nextSceneName), for example
            // Shouldn't cause any issues without this, but leaving this reminder just in case
            if (questStepMarker != null)
            {
                questStepMarker.UntrackMarker();
            }
        }

        public void SetMarkerInactive()
        {
            //GetQuestMarker(); // disabled, seems it was making excess calls to QuestMarkerManager.instance.sceneQuestMarkersManager.GetQuestMarkerForScene(nextSceneName), for example
            // Shouldn't cause any issues without this, but leaving this reminder just in case
            if (questStepMarker != null)
            {
                questStepMarker.SetInactive();
            }
        }

        public void SetMarkerActive()
        {
            GetQuestMarker();
            if (questStepMarker != null)
            {
                questStepMarker.gameObject.SetActive(true);
            }
        }

        public QuestStep ToSaveData()
        {
            QuestStep clonedStep = ScriptableObject.CreateInstance<QuestStep>();
            clonedStep.questStepID = this.questStepID;
            clonedStep.questStepDescription = this.questStepDescription; // Not Used for Setting data, just helpful to see in the inspector
            clonedStep.stepActive = this.stepActive;
            clonedStep.stepComplete = this.stepComplete;
            clonedStep.currentQuantity = this.currentQuantity;
            //Debug.Log("Copied save data for quest step: " + this.questStepDescription);
            return clonedStep;
        }

    }
}
