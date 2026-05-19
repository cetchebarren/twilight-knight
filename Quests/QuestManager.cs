using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class QuestManager : MonoBehaviour
    {
        public enum UpdateType
        {
            Updated,
            Completed,
            Added
        }

        public static QuestManager instance;

        public InputHandler inputHandler;

        public UIAudioManager uiAudioManager;

        // Link to quests in project folder, shared by all saves but set by quest data on Loading
        public List<Quest> quests;

        // This is essentially a backup of quest data
        public List<Quest> questData;

        // Scene quest step completed events, replaced when loading a new scene, keeps a list of events related to completing quest steps
        public SceneQuestStepCompletedEvents questStepCompletedEvents;

        [Header("Tracked Quest HUD UI Elements")]
        public GameObject trackedQuestHUD;
        public TextMeshProUGUI questNameText;
        public GameObject divider;
        public TextMeshProUGUI currentObjectiveText;
        public GameObject questUpdatedText;
        public Coroutine questUpdateTextCourotine;
        public RectTransform minimapWindowHUD;
        public GameObject trackQuestPrompt;
        public Quest currentTrackPromptQuest;

        public bool aQuestIsTracked = false;

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

            //ResetQuests(); /* Moved this out of Start() so that it is not called when games are loaded but only on new characters in ConfirmCharacter.cs */

            ValidateIDs();
        }

        public void Start()
        {
            // Start tracking tracked quest (on game loaded)
            foreach (Quest quest in quests)
            {
                if (quest.tracked) quest.TrackQuest();
            }
        }

        public void SaveQuestData()
        {
            // Clear previous data
            questData.Clear();

            // Iterate through all quests
            foreach(Quest quest in quests)
            {
                // Copy important information into each quest
                Quest copy = quest.ToSaveData();

                // Iterate through that quest's quest steps
                for (int i = 0; i < quest.questSteps.Length; i++)
                {
                    // Copy important information into each quest step
                    copy.questSteps[i] = quest.questSteps[i].ToSaveData();
                }
                
                // Add the save data for this quest (and its nested quest steps) into our save data list
                questData.Add(copy);
            }

            foreach (Quest q in questData)
            {
                q.name = $"{q.questName} Data";
            }

            // Make sure quests and questData are in the same order
            quests.Sort((q1, q2) => q1.questID.CompareTo(q2.questID));
            questData.Sort((q1, q2) => q1.questID.CompareTo(q2.questID));

            Debug.Log("Quest Data finished saving: " + questData);
        }

        public void LoadQuestData()
        {
            // Start by resetting all quest data
            ResetQuests();

            // Check if questData is valid
            if (questData == null || questData.Count == 0)
            {
                Debug.LogWarning("No quest data to load.");
                return;
            }

            // Use a dictionary to map questID to questData for faster lookups
            Dictionary<int, Quest> questDataDict = new Dictionary<int, Quest>();
            foreach(Quest questDataQuest in questData)
            {
                questDataDict[questDataQuest.questID] = questDataQuest;
            }

            // Iterate through all quests and set data based on the saved data list
            foreach (Quest quest in quests)
            {
                // Check if the questID exists in the loaded questData
                if (questDataDict.TryGetValue(quest.questID, out Quest savedData))
                {
                    quest.questActive = savedData.questActive;
                    quest.questAvailable = savedData.questAvailable;
                    quest.completed = savedData.completed;
                    quest.currentStep = savedData.currentStep;
                    quest.tracked = savedData.tracked;
                    quest.flaggedAsNew = savedData.flaggedAsNew;

                    // Iterate through all quest steps for this quest and set data based on the saved data list
                    for (int j = 0; j < quest.questSteps.Length; j++)
                    {
                        // Try to find a matching quest step by stepID
                        for (int k = 0; k < savedData.questSteps.Length; k++)
                        {
                            if (quest.questSteps[j].questStepID == savedData.questSteps[k].questStepID)
                            {
                                quest.questSteps[j].stepActive = savedData.questSteps[k].stepActive;
                                quest.questSteps[j].stepComplete = savedData.questSteps[k].stepComplete;
                                quest.questSteps[j].currentQuantity = savedData.questSteps[k].currentQuantity;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Create a new quest data copy for this new quest
                    Quest newQuestData = new Quest
                    {
                        questID = quest.questID,
                        questName = quest.questName,
                        description = quest.description,
                        flaggedAsNew = true,
                        questActive = false,
                        questAvailable = false,
                        completed = false,
                        currentStep = 0,
                        tracked = false,
                        questSteps = new QuestStep[quest.questSteps.Length]
                    };

                    // Deep copy steps
                    for (int i = 0; i < quest.questSteps.Length; i++)
                    {
                        QuestStep step = quest.questSteps[i];
                        newQuestData.questSteps[i] = new QuestStep
                        {
                            questStepID = step.questStepID,
                            stepActive = false,
                            stepComplete = false,
                            currentQuantity = 0
                        };
                    }

                    // Add the initialized data copy to questData
                    questData.Add(newQuestData);
                }

                foreach (Quest q in questData)
                {
                    q.name = $"{q.questName} Data";
                }


            }
        }

        public void ResetQuests()
        {
            foreach(Quest quest in quests)
            {
                quest.questActive = false;
                quest.questAvailable = false;
                quest.completed = false;
                quest.currentStep = 0;
                quest.tracked = false;
                quest.flaggedAsNew = true;

                foreach(QuestStep questStep in quest.questSteps)
                {
                    questStep.stepActive = false;
                    questStep.stepComplete = false;
                    questStep.currentQuantity = 0;
                }
            }

            // Start Initial Quests
            //quests[0].StartQuest(); // Chest on a Hill

            quests[1].questAvailable = true; // Where Bears

            quests[2].questAvailable = true; // Spooky Skeletons

            quests[5].questAvailable = true; // A Light in Darkness
        }

        public void StartQuest(int questID, bool trackQuest)
        {
            Quest quest = GetQuestByID(questID);

            if(quest == null)
            {
                Debug.LogError("Error trying to start quest for QuestID: " + questID + ", quest was null");
                return;
            }

            if(quest.questActive)
            {
                Debug.LogError("Error trying to start quest for QuestID: " + questID + ", quest already active");
                return;
            }

            if (quest.completed)
            {
                Debug.LogError("Error trying to start quest for QuestID: " + questID + ", quest already completed");
                return;
            }

            quest.StartQuest(trackQuest);       
        }

        public IEnumerator StartQuestAfterDelay(int questID, bool trackQuest, float delay)
        {
            yield return new WaitForSeconds(delay);
            StartQuest(questID, trackQuest);
        }


        public Quest GetQuestByID(int questID)
        {
            //Debug.Log("Checking for questID: " + questID);
            foreach(Quest quest in quests)
            {
                //Debug.Log("quest (" + quest.questName + ") ID: " + quest.questID + " == " + questID + "???");
                if(quest.questID == questID)
                {
                    return quest;
                }
            }

            Debug.LogError("Quest Not Found. There may be an Invalid Quest ID in a Function Call.");
            return null;
        }

        public void UpdateQuestsOnKill(int questStepID)
        {
            foreach(Quest quest in quests)
            {
                foreach(QuestStep questStep in quest.questSteps)
                {
                    if(questStep.stepActive && questStepID == questStep.questStepID)
                    {
                        questStep.AddProgress();
                    }
                }
            }
        }

        public QuestStep CheckQuestsOnTalk(DialogueAgent dialogueAgent)
        {
            foreach (Quest quest in quests)
            {
                if(!quest.completed && quest.questActive)
                {
                    foreach (QuestStep questStep in quest.questSteps)
                    {
                        if (questStep.questStepType == QuestStep.StepType.Talk)
                        {
                            if (questStep.stepActive && !questStep.stepComplete && questStep.dialogueAgentName == dialogueAgent.agentName)
                            {
                                // If this is returned, then we know the player is talking to an agent that will complete a quest step
                                return questStep;
                            }
                        }
                    }
                }
            }
            // If this is reached, the NPC the player is talking to is not linked to an active quest step
            return null;
        }

        public QuestStep CheckDeliveryQuestStep(DialogueAgent dialogueAgent)
        {
            foreach (Quest quest in quests)
            {
                if (!quest.completed && quest.questActive)
                {
                    foreach (QuestStep questStep in quest.questSteps)
                    {
                        if (questStep.questStepType == QuestStep.StepType.Deliver)
                        {
                            if (questStep.stepActive && !questStep.stepComplete && questStep.dialogueAgentName == dialogueAgent.agentName)
                            {
                                // If this is returned, then we know the player is talking to an agent that is expecting a delivery
                                return questStep;
                            }
                        }
                    }
                }
            }
            // If this is reached, the NPC the player is talking to is not linked to an active quest step
            return null;
        }

        public bool ReachAreaTriggered(int triggeredQuestStepID)
        {
            foreach (Quest quest in quests)
            {
                if (!quest.completed && quest.questActive)
                {
                    foreach (QuestStep questStep in quest.questSteps)
                    {
                        if (questStep.questStepType == QuestStep.StepType.Reach)
                        {
                            if (questStep.stepActive && !questStep.stepComplete && questStep.questStepID == triggeredQuestStepID)
                            {
                                // If this is returned, then we know the player has reached an active quest step trigger area
                                questStep.AddProgress();
                                return true;
                            }
                        }
                    }
                }
            }
            // If this is reached, the triggered quest step ID doesn't correspond to any currently active quest steps
            return false;
        }

        public void ValidateIDs()
        {
            // List to store encountered quest IDs
            List<int> encounteredQuestIDs = new List<int>();

            // List to store encountered quest step IDs
            List<int> encounteredStepIDs = new List<int>();

            // Iterate through the array of quests
            for (int i = 0; i < quests.Count; i++)
            {
                int questID = quests[i].questID;

                // Check if the quest ID has already been encountered
                if (encounteredQuestIDs.Contains(questID))
                {
                    Debug.LogError($"Duplicate quest ID found: {questID} for quests '{quests[i].questName}' and '{quests[encounteredQuestIDs.IndexOf(questID)].questName}'");
                    return;
                }
                else
                {
                    // Add the quest ID to the list of encountered IDs
                    encounteredQuestIDs.Add(questID);
                }

                // Iterate through the array of quest steps for the current quest
                for (int j = 0; j < quests[i].questSteps.Length; j++)
                {
                    int stepID = quests[i].questSteps[j].questStepID;

                    // Check if the quest step ID has already been encountered
                    if (encounteredStepIDs.Contains(stepID))
                    {
                        Debug.LogError($"Duplicate quest step ID found: {stepID} for quest step '{quests[i].questSteps[j].questStepDescription}' in quests '{quests[i].questName}' and '{quests[IndexOfDuplicateStepID(stepID)].questName}'");
                        return;
                    }
                    else
                    {
                        // Add the quest step ID to the list of encountered IDs
                        encounteredStepIDs.Add(stepID);
                    }
                }
            }

            Debug.Log("Quest and quest step ID validation completed.");
        }

        // Helper method to find the index of the quest with the duplicate step ID
        private int IndexOfDuplicateStepID(int stepID)
        {
            for (int i = 0; i < quests.Count; i++)
            {
                for (int j = 0; j < quests[i].questSteps.Length; j++)
                {
                    if (quests[i].questSteps[j].questStepID == stepID)
                    {
                        return i;
                    }
                }
            }
            return -1; // Indicates duplicate step ID not found
        }

        public void CheckQuestStepsForItem(Item item)
        {
            foreach (Quest quest in quests)
            {
                if (!quest.completed && quest.questActive)
                {
                    foreach (QuestStep questStep in quest.questSteps)
                    {
                        if (questStep.questStepType == QuestStep.StepType.Collect)
                        {
                            if (questStep.stepActive && !questStep.stepComplete && questStep.requiredItemID == item.itemID)
                            {
                                // If this is returned, then the player has collected an item that is required for a quest
                                questStep.AddProgress(item.count);
                                return;
                            }
                        }
                    }
                }
            }
            // If this is reached, the item was not required for a current quest
            return;
        }

        // Sorting the quest list of primarily by tracked and then by main/side quest type
        public void SortQuestList()
        {
            quests.Sort((a, b) =>
            {
                // Primary sort by tracked
                int trackedComparison = b.tracked.CompareTo(a.tracked);
                if (trackedComparison != 0)
                {
                    return trackedComparison;
                }
                else
                {
                    // Secondary sort by mainQuest
                    return b.mainQuest.CompareTo(a.mainQuest);
                }
            });
        }

        // Update the text display in game to display current quest, objective and progress
        public void UpdateActiveQuestHUD()
        {
            // Get current tracked quest, return if none
            Quest trackedQuest = GetCurrentlyTrackedQuest();
            if (trackedQuest == null)
            {
                trackedQuestHUD.SetActive(false);
                return;
            }
            if(trackedQuest.GetCurrentQuestStep() == null)
            {
                return;
            }

            // Set Quest Name
            questNameText.text = trackedQuest.questName;

            // Set Quest Current Objective
            QuestStep currentStep = trackedQuest.GetCurrentQuestStep();
            currentObjectiveText.text = currentStep.questStepDescription;
            if(currentStep.quantityRequired > 1)
            {
                currentObjectiveText.text += "\n<i>- Progress: " + currentStep.currentQuantity + " / " + currentStep.quantityRequired + "</i>";
            }
            //currentStep.TrackQuestMarker();

            //Adjust (shrink or stretch) rect transform height to fit text
            RectTransform rectTransform = currentObjectiveText.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, currentObjectiveText.preferredHeight);

            // Adjust divider length according to title/current objective text box length
            float upperClamp = 380f; // 40 left
            float lowerClamp = 180f; // 230 left
            float longest = Mathf.Max(questNameText.preferredWidth, currentObjectiveText.preferredWidth);
            float effectiveLength = Mathf.Clamp(longest, lowerClamp, upperClamp);
            // Map the effective length (380 -> 180) to left values (40 -> 230)
            float leftValue = Mathf.Lerp(40f, 230f, Mathf.InverseLerp(upperClamp, lowerClamp, effectiveLength));
            // Apply to RectTransform
            RectTransform dividerRect = divider.GetComponent<RectTransform>();
            dividerRect.offsetMin = new Vector2(leftValue, dividerRect.offsetMin.y);

            // Turn On the HUD GameObject
            trackedQuestHUD.SetActive(true);
        }

        public Quest GetCurrentlyTrackedQuest()
        {
            foreach(Quest quest in quests)
            {
                if (quest.tracked) return quest;
            }
            return null;
        }

        public void DisplayQuestUpdatedText(UpdateType updateType, bool alreadyTracked = false, Quest quest = null)
        {
            // By default, track quest prompt is off
            trackQuestPrompt.SetActive(false);
            bool trackPrompt = false;
            bool controller = ControllerUIManager.instance.isUsingController();
            bool playstation = ControllerUIManager.instance.controllerTypePlaystation;
            if (quest != null) currentTrackPromptQuest = quest;

            // Default popup duration, unless changed to allow time for tracking prompt
            float duration = 5.0f;

            // Set position based on the scale of the minimap
            // Position values at scale 0.8
            Vector2 positionAt09 = new Vector2(483f, 401f);

            // Position values at scale 1.5
            Vector2 positionAt15 = new Vector2(338f, 267f);

            // Clamp scale value between 0.8 and 1.5
            float scale = minimapWindowHUD.transform.localScale.x;
            scale = Mathf.Clamp(scale, 0.8f, 1.5f);

            // Interpolate between the two positions based on scale
            float t = Mathf.InverseLerp(0.8f, 1.5f, scale);
            Vector2 newPosition = Vector2.Lerp(positionAt09, positionAt15, t);

            questUpdatedText.GetComponent<RectTransform>().anchoredPosition = newPosition;

            // If an update is already displayed, cancel and reset it
            if (questUpdateTextCourotine != null)
            {
                StopCoroutine(questUpdateTextCourotine);
                questUpdatedText.SetActive(false);
            }

            // Determine text content
            switch (updateType)
            {
                case UpdateType.Updated:
                    questUpdatedText.GetComponent<TextMeshProUGUI>().text = "Quest Updated.";
                    break;
                case UpdateType.Completed:
                    questUpdatedText.GetComponent<TextMeshProUGUI>().text = "Quest Complete!";
                    break;
                case UpdateType.Added:
                    questUpdatedText.GetComponent<TextMeshProUGUI>().text = "New Quest Added:\n\"" + quest.questName.Trim() + "\"";
                    if(quest.tracked == false && !alreadyTracked)
                    {
                        if (!controller)
                        {
                            // Case 1: PC Controls
                            trackQuestPrompt.transform.GetChild(0).gameObject.SetActive(true);
                            trackQuestPrompt.transform.GetChild(1).gameObject.SetActive(false);
                            trackQuestPrompt.transform.GetChild(2).gameObject.SetActive(false);
                        }
                        else
                        {
                            if (playstation)
                            {
                                // Case 2: Playstation Controls
                                trackQuestPrompt.transform.GetChild(0).gameObject.SetActive(false);
                                trackQuestPrompt.transform.GetChild(1).gameObject.SetActive(true);
                                trackQuestPrompt.transform.GetChild(2).gameObject.SetActive(false);
                            }
                            else
                            {
                                // Case 3: Xbox Controls
                                trackQuestPrompt.transform.GetChild(0).gameObject.SetActive(false);
                                trackQuestPrompt.transform.GetChild(1).gameObject.SetActive(false);
                                trackQuestPrompt.transform.GetChild(2).gameObject.SetActive(true);
                            }
                        }
                        trackQuestPrompt.SetActive(true);
                        duration = 8.0f;
                        trackPrompt = true;
                    }

                    break;
                default:
                    questUpdatedText.GetComponent<TextMeshProUGUI>().text = "Quest Updated.";
                    break;

            }

            questUpdateTextCourotine = StartCoroutine(QuestUpdatedText(duration, trackPrompt));

            uiAudioManager.PlayQuestUpdatedAudio();
        }

        private IEnumerator QuestUpdatedText(float textLifetime, bool trackPrompt)
        {
            questUpdatedText.SetActive(true);
            yield return new WaitForSeconds(textLifetime);

            if (trackPrompt)
            {
                while(inputHandler.inputActions.PlayerActions.TrackQuest.ReadValue<float>() > 0)
                {
                    yield return null;
                }
            }

            questUpdatedText.SetActive(false);
            trackQuestPrompt.SetActive(false);
            currentTrackPromptQuest = null;
            questUpdateTextCourotine = null;
        }

        public void TrackPromptTriggered()
        {
            StopCoroutine(questUpdateTextCourotine);
            questUpdateTextCourotine = null;
            questUpdatedText.SetActive(false);
            trackQuestPrompt.SetActive(false);
            currentTrackPromptQuest.TrackQuest();
            currentTrackPromptQuest = null;
        }

        // Check if quest step is active (typically for spawning quest items in quest item spawn control)
        public bool CheckQuestStepActive(int _questStepID)
        {
            foreach (Quest quest in quests)
            {
                if (!quest.completed && quest.questActive)
                {
                    foreach (QuestStep questStep in quest.questSteps)
                    {
                        if (questStep.stepActive && !questStep.stepComplete && questStep.questStepID == _questStepID)
                        {
                            // active quest step found matching the given quest step ID
                            return true;
                        }     
                    }
                }
            }
            // If this is reached, there was no active quest step matching the given quest step ID
            return false;
        }

        public bool CheckQuestStepComplete(int _questStepID)
        {
            foreach (Quest quest in quests)
            {
                foreach (QuestStep questStep in quest.questSteps)
                {
                    if (questStep.stepComplete && questStep.questStepID == _questStepID)
                    {
                        // active quest step found matching the given quest step ID
                        return true;
                    }
                }        
            }
            // If this is reached, there was no active quest step matching the given quest step ID
            return false;
        }

        // For quest steps that must be completed in one life / one visit
        public void ResetProgessForResettingQuestSteps()
        {
            foreach(Quest quest in quests)
            {
                foreach(QuestStep questStep in quest.questSteps)
                {
                    if (questStep.resetProgressOnDeathOrSceneExit)
                    {
                        questStep.ResetProgress();
                    }
                }
            }
        }
    }
}
