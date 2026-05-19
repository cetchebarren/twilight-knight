using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Quests/Quest")]

    public class Quest : ScriptableObject
    {
        [Header("Quest Settings")]
        public string questName = "blank";
        [TextArea]
        public string description = "blank description";
        public bool mainQuest = false;
        public bool tracked = false;

        [Header("Quest Data & Progress")]
        public int questID = -1;
        public int currentStep = 0;
        public bool questActive = false;
        public bool questAvailable = false;
        public bool completed = false;
        public bool flaggedAsNew = true;
        public QuestStep[] questSteps;
        public Quest nextQuest;
        public enum NextQuestAction
        {
            BecomeAvailable,
            BeginQuest,
            BeginAndTrackIfThisWasTracked,
        }
        public NextQuestAction nextQuestAction;
        public DialogueLine[] receiveQuestDialogue;

        [Header("Quest Rewards")]
        public int gold = 1;
        public int XP = 1;
        public enum RandomRewardType
        {
            Random,
            Sword,
            Shield,
            TorsoArmorItem,
            HandsArmorItem,
            LegsArmorItem,
            RingItem,
            AmuletItem,
            None
        }
        public RandomRewardType randomRewardType;
        public string randomRewardRarity = "random";
        public int randomRewardMinimumRarity = 0;
        public Item[] specificItems;
        public QuestStep[] questStepsToProgressOnComplete;

        public void StartQuest(bool startTracked = false)
        {
            QuestManager.instance.DisplayQuestUpdatedText(QuestManager.UpdateType.Added, startTracked, this);
            questAvailable = false;
            questActive = true;
            ActivateQuestStep(questSteps[currentStep]);
            // Check if we already grabbed the needed items for collect quest step
            if (questSteps[currentStep].questStepType == QuestStep.StepType.Collect)
            {
                questSteps[currentStep].CheckCollectOnStartStep();
            }
            // Enable quest marker (not tracked marker!)
            questSteps[currentStep].SetMarkerActive();
            if (startTracked) TrackQuest();
        }

        public void StartQuestViaTrigger(bool startTracked = false)
        {
            if(!completed && !questActive)
            {
                StartQuest(startTracked);
            }
        }

        public QuestStep GetCurrentQuestStep()
        {
            if (!questActive) return null;

            bool foundCurrentStep = false;

            for (int i = 0; i < questSteps.Length && !foundCurrentStep; i++)
            {
                if (!questSteps[i].stepComplete)
                {
                    foundCurrentStep = true;
                    return questSteps[i];
                }
            }

            return null;
        }

        // Used for displaying location properly in quest info box on completed quests
        public QuestStep GetLastQuestStep()
        {
            return questSteps[questSteps.Length - 1];
        }

        public float GetQuestOverallQuestProgress()
        {
            int questStepsCount = questSteps.Length;
            int stepsCompleted = 0;

            for (int i = 0; i < questSteps.Length; i++)
            {
                if (questSteps[i].stepComplete)
                {
                    stepsCompleted++;
                }
            }

            // Convert to percentage 0.5 -> 50
            float progress = ((float)stepsCompleted / (float)questStepsCount) * 100f;
            return progress;
        }

        public void ProgressToNextStep()
        {
            // Check for Events related to finishing the quest step
            QuestManager.instance.questStepCompletedEvents.CheckEvent(questSteps[currentStep].questStepID);

            // Display quest updated text
            QuestManager.instance.DisplayQuestUpdatedText(QuestManager.UpdateType.Updated);
            questSteps[currentStep].SetMarkerInactive();
            currentStep++;
            if(currentStep >= questSteps.Length)
            {
                CheckCompletion();
            }
            else
            {
                ActivateQuestStep(questSteps[currentStep]);
                // Since quest step has changed, we will check all Quest Items in the scene and activate them if they match the new step
                GameObject[] questItemsInScene = GameObject.FindGameObjectsWithTag("QuestItem");
                foreach(GameObject questItem in questItemsInScene)
                {
                    QuestItemSpawnControl qisc = questItem.GetComponent<QuestItemSpawnControl>();
                    qisc?.DetermineSpawn();
                }

                // Activate the new quest steps world & minimap quest marker (this doesn't mean tracked, it will just be visible now)
                questSteps[currentStep].SetMarkerActive();

                if (tracked) questSteps[currentStep].TrackQuestMarker();

                // Check if we already grabbed the needed items for collect quest step
                if (questSteps[currentStep].questStepType == QuestStep.StepType.Collect)
                {
                    questSteps[currentStep].CheckCollectOnStartStep();
                }
                else if (questSteps[currentStep].questStepType == QuestStep.StepType.Misc)
                {
                    questSteps[currentStep].CheckMiscOnStartStep();
                }

            }
            // Update tracked quest HUD
            QuestManager.instance.UpdateActiveQuestHUD();
        }

        public void CheckCompletion()
        {
            bool foundCurrentStep = false;

            for (int i = 0; i < questSteps.Length && !foundCurrentStep; i++)
            {
                if (!questSteps[i].stepComplete)
                {
                    foundCurrentStep = true;
                    // Not Complete
                    return;
                }
            }

            // Complete
            completed = true;
            questActive = false;
            bool wasTracked = tracked;
            UntrackQuest();
            Reward();
            if(questStepsToProgressOnComplete != null)
            {
                foreach(QuestStep questStep in questStepsToProgressOnComplete)
                {
                    questStep.AddProgress(1);
                }
            }
            if(nextQuest != null)
            {
                switch (nextQuestAction)
                {
                    case NextQuestAction.BecomeAvailable:
                        nextQuest.questAvailable = true;
                        break;
                    case NextQuestAction.BeginQuest:
                        nextQuest.questAvailable = true;
                        nextQuest.StartQuest();
                        break;
                    case NextQuestAction.BeginAndTrackIfThisWasTracked:
                        nextQuest.questAvailable = true;
                        nextQuest.StartQuest(wasTracked);
                        break;
                    default:
                        nextQuest.questAvailable = true;
                        break;
                }       
            }
        }

        public void Reward()
        {
            PlayerInventory.instance.AddGold(gold, true);
            AcquiredNotifications.instance.NewItemNotification(null, gold);

            PlayerStats.instance.AwardPlayerXP(XP);

            int level = PlayerStats.instance.playerLevel;

            switch (randomRewardType)
            {
                case RandomRewardType.Random:
                    ItemGeneratorManager.instance.GenerateRandomItemForQuestReward(level, randomRewardRarity, randomRewardMinimumRarity, -1);
                    break;
                case RandomRewardType.Sword:
                    ItemGeneratorManager.instance.GenerateRandomItemForQuestReward(level, randomRewardRarity, randomRewardMinimumRarity, 0);
                    break;
                case RandomRewardType.Shield:
                    ItemGeneratorManager.instance.GenerateRandomItemForQuestReward(level, randomRewardRarity, randomRewardMinimumRarity, 1);
                    break;
                case RandomRewardType.TorsoArmorItem:
                    ItemGeneratorManager.instance.GenerateRandomItemForQuestReward(level, randomRewardRarity, randomRewardMinimumRarity, 2);
                    break;
                case RandomRewardType.HandsArmorItem:
                    ItemGeneratorManager.instance.GenerateRandomItemForQuestReward(level, randomRewardRarity, randomRewardMinimumRarity, 3);
                    break;
                case RandomRewardType.LegsArmorItem:
                    ItemGeneratorManager.instance.GenerateRandomItemForQuestReward(level, randomRewardRarity, randomRewardMinimumRarity, 4);
                    break;
                case RandomRewardType.RingItem:
                    ItemGeneratorManager.instance.GenerateRandomItemForQuestReward(level, randomRewardRarity, randomRewardMinimumRarity, 5);
                    break;
                case RandomRewardType.AmuletItem:
                    ItemGeneratorManager.instance.GenerateRandomItemForQuestReward(level, randomRewardRarity, randomRewardMinimumRarity, 6);
                    break;
                case RandomRewardType.None:
                    break;
                default:
                    break;
            }

            if(specificItems != null)
            {
                foreach(Item item in specificItems)
                {
                    PlayerInventory.instance.AddToInventory(item, true);
                    AcquiredNotifications.instance.NewItemNotification(item, item.count);
                }
            }

            // Untrack the quest now that it is compelte
            UntrackQuest();

            // Update tracked quest HUD
            QuestManager.instance.UpdateActiveQuestHUD();

            // Play Audio
            UIAudioManager.instance.PlayQuestCompletedAudio();

            // Display quest updated text
            //TextNotificationsManager.instance.NewTextNotifaction("Quest Complete.", true); //true prevents duplicate messages for 3 seconds // updated to below
            QuestManager.instance.DisplayQuestUpdatedText(QuestManager.UpdateType.Completed);
        }

        public void TrackQuest()
        {
            // Untrack all other quests

            foreach(Quest quest in QuestManager.instance.quests)
            {
                quest.UntrackQuest();
            }

            // Track this quest

            tracked = true;

            if (questSteps[currentStep].questStepMarker != null) //  added
            {
                MinimapOutOfBoundsIndicator.instance.trackedDistanceContainer.SetActive(true);

                MinimapOutOfBoundsIndicator.instance.enabled = true;

                questSteps[currentStep].SetMarkerActive();

                questSteps[currentStep].TrackQuestMarker();
            }
            else
            {
                MinimapOutOfBoundsIndicator.instance.trackedDistanceContainer.SetActive(false);

                MinimapOutOfBoundsIndicator.instance.enabled = false;

                MinimapOutOfBoundsIndicator.instance.SetDistanceTexts("");
            }

            // Update tracked quest HUD
            QuestManager.instance.UpdateActiveQuestHUD();

            QuestManager.instance.aQuestIsTracked = true;
        }

        public void UntrackQuest()
        {
            tracked = false;

            MinimapOutOfBoundsIndicator.instance.trackedDistanceContainer.SetActive(false);

            foreach (QuestStep questStep in questSteps)
            {
                questStep.UntrackQuestMarker();
            }

            // Update tracked quest HUD
            QuestManager.instance.UpdateActiveQuestHUD();

            QuestManager.instance.aQuestIsTracked = false;
        }

        public void ActivateQuestStep(QuestStep questStep)
        {
            questStep.stepActive = true;
            // Since quest step has changed, we will check all Quest Items in the scene and activate them if they match the new step
            GameObject[] questItemsInScene = GameObject.FindGameObjectsWithTag("QuestItem");
            //Debug.Log(questItemsInScene);
            foreach (GameObject questItem in questItemsInScene)
            {
                QuestItemSpawnControl qisc = questItem.GetComponent<QuestItemSpawnControl>();
                qisc?.DetermineSpawn();
            }
        }

        public void SetAvailable()
        {
            if(!completed)
                questAvailable = true;
        }

        public Quest ToSaveData()
        {
            Quest clonedQuest = ScriptableObject.CreateInstance<Quest>();
            clonedQuest.questID = this.questID;
            clonedQuest.questName = this.questName; // Not Used for Setting data, just helpful to see in the inspector
            clonedQuest.description = this.description; // Not Used for Setting data, just helpful to see in the inspector
            clonedQuest.questSteps = new QuestStep[this.questSteps.Length]; // initialize quest step array based on length of original array
            clonedQuest.questActive = this.questActive;
            clonedQuest.questAvailable = this.questAvailable;
            clonedQuest.completed = this.completed;
            clonedQuest.currentStep = this.currentStep;
            clonedQuest.tracked = this.tracked;
            clonedQuest.flaggedAsNew = this.flaggedAsNew;
            //Debug.Log("Copied save data for quest: " + this.questName);
            return clonedQuest;
        }

    }
}
