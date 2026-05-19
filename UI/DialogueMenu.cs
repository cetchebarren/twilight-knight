using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class DialogueMenu : MonoBehaviour
    {
        public static DialogueMenu instance;

        [Header("Script References")]
        public InputHandler inputHandler;
        public SetHUD setHUD;
        public UIAudioManager uiAudioManager;
        public PlayerInventory playerInventory;

        [Header("Text Element References")]
        public TextMeshProUGUI speakerName;
        public TextMeshProUGUI dialogue;

        private DialogueAgent dialogueAgent;
        public DialogueLine[] currentDialogueLines;
        private int currentDialogueIndex = 0;
        //private bool drawingText = false;
        private string currentText;
        private string finalText;
        private Coroutine drawingText;
        private Coroutine allowTextProgress;

        [Header("Settings")]
        public float textSpeed = 1.0f;
        public float timeBeforeAllowProgress = 2.0f;
        private bool allowProgress = false;

        /* Flags */
        bool foundActiveQuest = false;
        bool foundAvailableQuest = false;
        private Quest quest = null;
        private QuestStep talkQuestStep = null;
        private QuestStep deliverQuestStep = null;


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

        public void OpenDialogueMenu()
        {
            inputHandler.horizontal = 0;
            inputHandler.vertical = 0;
            inputHandler.moveAmount = 0;
            setHUD.SetHUDActive(false);
            gameObject.SetActive(true);
            SetSpeakerText(dialogueAgent.agentName);
            ResetDialogueIndex();
            DetermineDialogue();
            if (dialogueAgent.thisNPC != null) dialogueAgent.thisNPC.StartRotation(PlayerLocomotion.instance.myTransform);
            // Update & display controller HUD ("buy, sell, leave")
            ControllerUIManager.instance.SetSelectText("Continue");
            ControllerUIManager.instance.SelectTextActive(true);
            //ControllerUIManager.instance.SetBackText("Leave");
            ControllerUIManager.instance.BackTextActive(false);
        }

        public void CloseDialogueMenu(bool skipQuestProgress=false)
        {
            ControllerUIManager.instance.SelectTextActive(false);
            gameObject.SetActive(false);
            setHUD.SetHUDActive(true);
            inputHandler.inMenu = false;

            if (!skipQuestProgress)
            {
                if (quest != null)
                {
                    // If quest is not null at this point, then the quest was given during this dialogue
                    quest.StartQuest();
                }

                if (talkQuestStep != null)
                {
                    // If talkQuestStep is not null at this point, then talking to that dialogue agent advances (or completes) the quest
                    talkQuestStep.AddProgress();
                }

                if (deliverQuestStep != null)
                {
                    // If deliverQuestStep is not null at this point, then talking to that dialogue agent advances (or completes) the quest
                    deliverQuestStep.AddProgress();
                }
            }

            dialogueAgent.CheckQuestAvailability();

            uiAudioManager.PlayDialogueInputAudio();

            dialogueAgent.ResetTrigger();

            if(dialogueAgent.thisNPC != null) dialogueAgent.thisNPC.ReturnToInitialRotation();          
        }

        public void DetermineDialogue()
        {
            // Reset Flags //
            foundActiveQuest = false;
            foundAvailableQuest = false;
            quest = null;
            talkQuestStep = null;
            deliverQuestStep = null;
            bool alreadyMet = WorldStateManager.instance.AlreadyMetDialogueAgent(dialogueAgent.agentName) || dialogueAgent.introduction.Length < 1;

            #region Case 1: Character has an available quest
            if (dialogueAgent.associatedQuestIDs.Length > 0)
            {
                quest = null;
                // Iterate through each assoicated quests to find an available quest
                for (int i = 0; i < dialogueAgent.associatedQuestIDs.Length && !foundAvailableQuest; i++)
                {
                    quest = QuestManager.instance.GetQuestByID(dialogueAgent.associatedQuestIDs[i]);

                    if (!quest.questActive && !quest.completed && quest.questAvailable)
                    {
                        foundAvailableQuest = true;
                        break;
                    }
                    else
                    {
                        quest = null;
                    }
                }
                if (foundAvailableQuest)
                {
                    currentDialogueLines = quest.receiveQuestDialogue;
                    SetLinesAndPlayAudio();
                    return;
                }
            }
            #endregion

            #region Case 2: Active Quest step for speaking to character
            // Check if speaking to this agent is a quest step
            talkQuestStep = QuestManager.instance.CheckQuestsOnTalk(dialogueAgent);
            if(talkQuestStep != null)
            {
                currentDialogueLines = talkQuestStep.dialogueLines;
                if (currentDialogueLines == null)
                {
                    Debug.LogError("Missing dialogue lines for quest step: " + talkQuestStep.questStepDescription);
                    return;
                }
                SetLinesAndPlayAudio();
                return;
            }
            #endregion

            #region Case 3: Character is expecting delivery
            // Check if this agent is expecting a delivery from an active quest step
            deliverQuestStep = QuestManager.instance.CheckDeliveryQuestStep(dialogueAgent);
            if (deliverQuestStep != null)
            {
                Item item = deliverQuestStep.CheckDelivery();

                if(item != null)//Player has item AND the required quantity
                {
                    playerInventory.RemoveFromInventory(item, deliverQuestStep.quantityRequired);
                    currentDialogueLines = deliverQuestStep.deliveredLines;
                    SetLinesAndPlayAudio();
                    return;
                }
                //else // Removed for now, these lines should be set later if nothing else is triggered?
                //{
                //    //currentDialogueLines = deliverQuestStep.dialogueLines;
                //}
            }
            #endregion

            #region Case 4: Character has another active (but incomplete) quest
            // If the agent has associated quests
            if (dialogueAgent.associatedQuestIDs.Length > 0)
            {
                Quest questRef = null;
                quest = null;
                // Iterate through each assoicated quests to find an active quest
                for (int i = 0; i < dialogueAgent.associatedQuestIDs.Length && !foundActiveQuest; i++)
                {
                    quest = QuestManager.instance.GetQuestByID(dialogueAgent.associatedQuestIDs[i]);

                    if (quest.questActive && !quest.completed)
                    {
                        questRef = quest;
                        foundActiveQuest = true;
                    }
                }

                quest = null; // Clear quest to prevent giving player quest again (in close dialogue menu)

                if (foundActiveQuest)
                {
                    currentDialogueLines = questRef.GetCurrentQuestStep().dialogueLines;
                    if(currentDialogueLines.Length > 0)
                    {
                        if (currentDialogueLines[currentDialogueIndex].clip != null)
                        {
                            SetLinesAndPlayAudio();
                            return;
                        }
                    }
                }
            }
            #endregion

            #region Case 5: Character currently has no currently related quests, First time speaking to character
            if (!alreadyMet)
            {
                currentDialogueLines = dialogueAgent.introduction;
                // Determine/Play Dialogue Agent Vocals
                dialogueAgent.audioSource.clip = currentDialogueLines[currentDialogueIndex].clip;
                dialogueAgent.PlayVocal();
                SetDialogueText(currentDialogueLines[currentDialogueIndex].line);
                return;
            }
            #endregion

            #region Case 6: Character currently has no currently related quests, default dialogue
            currentDialogueLines = dialogueAgent.defaultLines;
            SetLinesAndPlayAudio();
            return;
            #endregion
        }

        private void SetLinesAndPlayAudio()
        {
            // Determine/Play Dialogue Agent Vocals
            if(currentDialogueLines[currentDialogueIndex].clip != null)
            {
                dialogueAgent.audioSource.clip = currentDialogueLines[currentDialogueIndex].clip;
            }
            dialogueAgent.PlayVocal();

            SetDialogueText(currentDialogueLines[currentDialogueIndex].line);
        }

        public void ProgressDialogueMenu()
        {
            if (allowProgress)
            {
                if(drawingText == null)
                {
                    currentDialogueIndex++;

                    if (currentDialogueIndex >= currentDialogueLines.Length)
                    {
                        CloseDialogueMenu();
                        return;
                    }

                    // Determine/Play Dialogue Agent Vocals
                    dialogueAgent.audioSource.clip = currentDialogueLines[currentDialogueIndex].clip;
                    dialogueAgent.PlayVocal();

                    // Update Dialogue Text
                    SetDialogueText(currentDialogueLines[currentDialogueIndex].line);
                }
                else
                {
                    dialogue.text = finalText;
                    StopCoroutine(drawingText);
                    drawingText = null;

                    if(allowTextProgress != null)
                    {
                        StopCoroutine(allowTextProgress);
                        allowTextProgress = null;
                        allowProgress = true;
                    }
                }
                uiAudioManager.PlayDialogueInputAudio();

            }

        }
            
        public void SetDialogueAgent(DialogueAgent agent)
        {
            dialogueAgent = agent;
        }

        public void SetSpeakerText(string name)
        {
            speakerName.text = name;
        }

        public void SetDialogueText(string text)
        {
            drawingText = StartCoroutine(UpdateTextCharacters());
        }

        private void ResetDialogueIndex()
        {
            currentDialogueIndex = 0;
        }
        
        private IEnumerator UpdateTextCharacters()
        {
            allowTextProgress = StartCoroutine(AllowTextProgress());

            currentText = "";
            string text = currentDialogueLines[currentDialogueIndex].line;
            string newText = text;
            // Replace any tokens in dialogue line

            newText = text.Replace("{KNIGHT}", PlayerStats.instance.playerName);
            newText = newText.Replace("{HORSE}", MountStats.instance.horseName);

            // Update text line itself (for skipped/progressed text)
            // currentDialogueLines[currentDialogueIndex].line = newText;
            finalText = newText;

            // Continue the operation as long as continueOperation is true
            int index = 0;
            while (currentText.Length < newText.Length)
            {
                currentText += newText[index];
                index++;
                dialogue.text = currentText;
                // Wait for the specified interval before repeating the operation
                yield return new WaitForSeconds(textSpeed);
            }

            // When continueOperation becomes false, the coroutine exits
            Debug.Log("Text Finished");
            drawingText = null;
        }

        private IEnumerator AllowTextProgress()
        {
            ControllerUIManager.instance.SetSelectTextColor(Color.grey);
            allowProgress = false;
            yield return new WaitForSeconds(timeBeforeAllowProgress);
            allowProgress = true;
            ControllerUIManager.instance.SetSelectTextColor(Color.white);
        }

        public void SetCurrentDialogueLines(DialogueLine[] newDialogueLines)
        {
            currentDialogueLines = newDialogueLines;
        }

    }
}
