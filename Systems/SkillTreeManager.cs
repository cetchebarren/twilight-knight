using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace etchebarren
{
    public class SkillTreeManager : MonoBehaviour
    {
        public UIAudioManager uiAudioManager;
        public PlayerInventory playerInventory;
        public PlayerStats playerStats;

        public TextMeshProUGUI remainingSkillPoints;
        public GameObject[] skillsArray;

        public TextMeshProUGUI skillName;
        public TextMeshProUGUI currentEffectTitleText;
        public TextMeshProUGUI currentEffectText;
        public TextMeshProUGUI nextEffectText;
        public TextMeshProUGUI requirementsText;

        public GameObject[] connectors;

        public GameObject popupWindow;

        private int[] playerBaseStats = new int[10];

        public Color lightSkill = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color darkSkill = new Color(0.33f, 0.33f, 0.33f, 1.0f);
        private Color darkConnector = new Color(0.17f, 0.17f, 0.17f, 1.0f);
        public Sprite lockedIcon;
        public Sprite unlockedIcon;
        public Sprite unavailableIcon;
        public Color lockedColor;
        public Color unlockedColor;
        public Color unavailableColor;

        private Coroutine popupCoroutine;

        [Header("Skill Upgrade-Related References")]
        public Camera minimapCamera;

        private void OnEnable()
        {
            UpdateSkillTreeUI();
        }

        public void UpdateSkillTreeUI()
        {
            remainingSkillPoints.text = "Remaining Skill Points: " + PlayerStats.instance.remainingSkillPoints;

            UpdateConnectors();

            for(int i = 0; i < skillsArray.Length; i++)
            {
                //Skill current & Skill Max Text
                skillsArray[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = PlayerStats.instance.GetSkillRankByID(i).ToString() + "/" + PlayerStats.instance.skills[i].maxRank.ToString();

                if(PlayerStats.instance.GetSkillRankByID(i) == PlayerStats.instance.skills[i].maxRank) //current == max
                {
                    skillsArray[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().color = Color.yellow;
                }

                //Previous Skill Unlocked?
                if(AllPreviousSkillsUnlocked(PlayerStats.instance.skills[i].previousSkills) && HaveRequiredStats(PlayerStats.instance.skills[i].attributeReqs))
                {
                    skillsArray[i].transform.GetChild(1).GetComponent<Image>().color = lightSkill;
                }
                else if(PlayerStats.instance.GetSkillRankByID(i) > 0) //at least 1 skill already purchased in tree, may want to change this later
                {
                    skillsArray[i].transform.GetChild(1).GetComponent<Image>().color = lightSkill;
                }
                else
                {
                    skillsArray[i].transform.GetChild(1).GetComponent<Image>().color = darkSkill;
                }

                skillsArray[i].transform.GetChild(4).gameObject.SetActive(false);
                skillsArray[i].transform.GetChild(5).gameObject.SetActive(false);
                // skill is available (unlockable (ignoring other requirements like previous skills and stats)
                if (PlayerStats.instance.skills[i].skillLocked)
                {
                    skillsArray[i].transform.GetChild(1).GetComponent<Image>().color = darkSkill;
                    if (PlayerStats.instance.skills[i].unlockRequirementText != "") // There is criteria to unlock this skill
                    {
                        skillsArray[i].transform.GetChild(4).gameObject.GetComponent<Image>().sprite = lockedIcon;
                        skillsArray[i].transform.GetChild(4).gameObject.GetComponent<Image>().color = lockedColor;
                        skillsArray[i].transform.GetChild(4).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 9f, 0f);
                        skillsArray[i].transform.GetChild(4).gameObject.GetComponent<RectTransform>().localScale = new Vector3(1.1f, 1.1f, 1.1f);
                    }
                    else
                    {
                        skillsArray[i].transform.GetChild(4).gameObject.GetComponent<Image>().sprite = unavailableIcon;
                        skillsArray[i].transform.GetChild(4).gameObject.GetComponent<Image>().color = unavailableColor;
                        skillsArray[i].transform.GetChild(4).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 3f, 0f);
                        skillsArray[i].transform.GetChild(4).gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 0.85f, 1f);
                    }
                    skillsArray[i].transform.GetChild(4).gameObject.SetActive(true);
                }
                else if (PlayerStats.instance.skills[i].unlockRequirementText == "Recently Unlocked")
                {
                    skillsArray[i].transform.GetChild(1).GetComponent<Image>().color = darkSkill;
                    skillsArray[i].transform.GetChild(4).gameObject.GetComponent<Image>().sprite = unlockedIcon;
                    skillsArray[i].transform.GetChild(4).gameObject.GetComponent<Image>().color = unlockedColor;
                    skillsArray[i].transform.GetChild(4).gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 6.4f, 0f);
                    skillsArray[i].transform.GetChild(4).gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 0.88f, 1f);
                    skillsArray[i].transform.GetChild(4).gameObject.SetActive(true);
                    skillsArray[i].transform.GetChild(5).gameObject.SetActive(true);
                }
            }
        }

        public void PurchaseSkill(int skillID)
        {
            if (!PlayerStats.instance.skills[skillID].skillLocked)
            {
                if (PlayerStats.instance.remainingSkillPoints > 0)
                {
                    if (PlayerStats.instance.GetSkillRankByID(skillID) < PlayerStats.instance.skills[skillID].maxRank) //Have enough points and not already maxed
                    {
                        if (AllPreviousSkillsUnlocked(PlayerStats.instance.skills[skillID].previousSkills)) // all previous skills required
                        {
                            if (HaveRequiredStats(PlayerStats.instance.skills[skillID].attributeReqs))//check stat requirements
                            {
                                PlayerStats.instance.remainingSkillPoints--; //spend point
                                playerStats.GetSkillByID(skillID).IncreaseRank();

                                // Increase Stat Reqs
                                int[] attributeReqs = PlayerStats.instance.skills[skillID].attributeReqs;
                                for (int i = 0; i < attributeReqs.Length; i++)
                                {
                                    attributeReqs[i] = Mathf.CeilToInt(attributeReqs[i] * 1.3f);
                                }

                                playerInventory.AddSpellFromSkillTree(skillID);

                                if (skillID == 30) CheckMinimapSkill();
                                if (skillID == 11) playerStats.UpdateStatsScreen(true);

                                UpdateSkillTreeUI();
                                UpdateDescriptions(skillID);
                                uiAudioManager.PlayPurchaseSkillAudio(true);
                            }
                            else
                            {
                                Notification("Insufficient Base Stats");
                                Debug.Log("Stat Requirements Not Met");
                                uiAudioManager.PlayPurchaseSkillAudio(false);
                            }
                        }
                        else
                        {
                            Notification("Previous Skills Required");
                            Debug.Log("Must Learn Previous Skills First");
                            uiAudioManager.PlayPurchaseSkillAudio(false);
                        }
                    }
                    else
                    {
                        Notification("Skill Already Maxed");
                        Debug.Log("Skill Already Maxed");
                        uiAudioManager.PlayPurchaseSkillAudio(false);
                    }
                }
                else
                {
                    Notification("Insufficient Skill Points");
                    uiAudioManager.PlayPurchaseSkillAudio(false);
                }
            }
            else
            {
                Notification("Skill Not Available");
                uiAudioManager.PlayPurchaseSkillAudio(false);
            }

            // SET INTERACT BUTTON UI
            SetSkillButtonInteractUI(skillID);
        }

        public void UpdateDescriptions(int skillID)
        {
            // Skill Name
            skillName.text = PlayerStats.instance.skills[skillID].name;

            // Requirements
            requirementsText.text = GetRequirementsString(PlayerStats.instance.skills[skillID].attributeReqs);
            currentEffectTitleText.text = "Current Effect:";

            if (!PlayerStats.instance.skills[skillID].skillLocked)
            {
                // Current Effect Text
                currentEffectText.text = PlayerStats.instance.skills[skillID].descriptions[PlayerStats.instance.GetSkillRankByID(skillID)];
                
                // Next Effect Text
                if (PlayerStats.instance.GetSkillRankByID(skillID) < PlayerStats.instance.skills[skillID].maxRank)
                {
                    nextEffectText.text = PlayerStats.instance.skills[skillID].descriptions[PlayerStats.instance.GetSkillRankByID(skillID) + 1];
                }
                else
                {
                    nextEffectText.text = "<i>SKILL ALREADY MAXED.</i>";
                }
            }
            else
            {
                currentEffectTitleText.text = "How to Unlock:";
                string currentEffectMessage = "Skill Not Available <size=83%>(in this version)";
                string nextEffectMessage = "N/A";
                if (PlayerStats.instance.skills[skillID].unlockRequirementText != "") // There is criteria to unlock this skill
                {        
                    currentEffectMessage = PlayerStats.instance.skills[skillID].unlockRequirementText + "\n";
                    // Set skill unlock criteria progress based on skill (if its a one time event, this is not necessary)
                    if(skillID == 7)
                    {
                        currentEffectMessage += "Progress: " + playerStats.attacksBlocked + " / " + playerStats.attacksBlockedToUnlockUnwaveringSkill;
                    }
                    nextEffectMessage = PlayerStats.instance.GetSkillByID(skillID).NextDescription();

                }
                currentEffectText.text = currentEffectMessage;
                nextEffectText.text = nextEffectMessage;
            }
        }

        public string GetRequirementsString(int[] attributeReqs)
        {
            string requirementsString = "<b>Requirements:</b> ";
            string separator = "";
            int numNonZeroReqs = 0;

            for (int i = 0; i < attributeReqs.Length; i++)
            {
                if (attributeReqs[i] != 0)
                {
                    requirementsString += separator;

                    switch (i)
                    {
                        case 0:
                            requirementsString += "STR ";
                            break;
                        case 1:
                            requirementsString += "END ";
                            break;
                        case 2:
                            requirementsString += "VIT ";
                            break;
                        case 3:
                            requirementsString += "PRE ";
                            break;
                        case 4:
                            requirementsString += "DEX ";
                            break;
                        case 5:
                            requirementsString += "EXP ";
                            break;
                        case 6:
                            requirementsString += "INT ";
                            break;
                        case 7:
                            requirementsString += "SPR ";
                            break;
                        case 8:
                            requirementsString += "WIL ";
                            break;
                        case 9:
                            requirementsString += "LCK ";
                            break;
                        default:
                            break;
                    }

                    requirementsString += attributeReqs[i];
                    numNonZeroReqs++;
                    separator = ", ";
                }
            }

            if (numNonZeroReqs == 0)
            {
                requirementsString += "None";
            }

            return requirementsString;
        }

        public void UpdateConnectors()
        {
            for (int i = 0; i < connectors.Length; i++)
            {
                string name = connectors[i].name;
                //Debug.Log(name);

                string[] parts = name.Split(new string[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 1 && int.TryParse(parts[0], out int firstNumber))
                {
                    int secondNumber = -1; // Default value for secondNumber

                    if (parts.Length == 2 && int.TryParse(parts[1], out int parsedSecondNumber))
                    {
                        secondNumber = parsedSecondNumber;
                    }

                    // firstNumber = skillID of first skill
                    if (PlayerStats.instance.GetSkillRankByID(firstNumber) > 0)
                    {
                        connectors[i].GetComponent<Image>().color = lightSkill;
                    }
                    else
                    {
                        connectors[i].GetComponent<Image>().color = darkConnector;
                    }
                }
                else
                {
                    Debug.Log("Parsing Failed, error with connector name, check format");
                }
            }

        }

        public bool HaveRequiredStats(int[] attributeReqs)
        {
            PlayerStats.instance.GetBaseStats(playerBaseStats);

            bool allConditionsMet = true;

            for (int i = 0; i < attributeReqs.Length; i++)
            {
                if (playerBaseStats[i] < attributeReqs[i])
                {
                    allConditionsMet = false;
                    break; // Exit the loop early if a condition is not met
                }
            }

            return allConditionsMet;
        }

        public bool AllPreviousSkillsUnlocked(int[] previousSkillsRequired)
        {
            //Previous Skill Unlocked?
            //int[] prevSkillReqs = PlayerStats.instance.skills[skillID].previousSkills;
            bool allPreviousSkillsUnlocked = true;

            foreach (int previousSkillID in previousSkillsRequired)
            {
                if (previousSkillID != -1 && PlayerStats.instance.GetSkillRankByID(previousSkillID) == 0)
                {
                    allPreviousSkillsUnlocked = false;
                    break; // Exit the loop early if any previous skill is locked
                }
            }

            return allPreviousSkillsUnlocked;
        }

        public void Notification(string message)
        {
            // Stop the current coroutine if it's running
            if (popupCoroutine != null)
            {
                StopCoroutine(popupCoroutine);
                popupCoroutine = null;
            }

            if (message == "OFF")
            {
                popupWindow.SetActive(false);
                return;
            }

            popupWindow.SetActive(true);
            popupWindow.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
            popupCoroutine = StartCoroutine(TurnOffNotification(1.5f));
            //play error sound?
        }

        private IEnumerator TurnOffNotification(float lifetime)
        {
            //Debug.Log("Notification ON");
            yield return new WaitForSecondsRealtime(lifetime);
            popupWindow.SetActive(false);
            //Debug.Log("Notification OFF");
            popupCoroutine = null;
        }

        private void CheckMinimapSkill()
        {
            int newSkillRank = PlayerStats.instance.GetSkillRankByID(30);
            int layerMask;

            // If the new rank is sufficient, add the corresponding layers to the camera's culling mask so that they will display

            switch (newSkillRank)
            {
                case 1:
                    layerMask = 1 << LayerMask.NameToLayer("MinimapEnemy");
                    minimapCamera.cullingMask |= layerMask;
                    break;
                case 2:
                    layerMask = 1 << LayerMask.NameToLayer("MinimapChest");
                    minimapCamera.cullingMask |= layerMask;
                    break;
                case 3:
                    layerMask = 1 << LayerMask.NameToLayer("MinimapSecret");
                    minimapCamera.cullingMask |= layerMask;
                    break;
                default:
                    break;
            }
        }

        public void SetSkillButtonInteractUI(int skillID)
        {
            Skill skill = playerStats.GetSkillByID(skillID);
            if (skill != null)
            {
                if (skill.CurrentRank() < skill.maxRank && !skill.skillLocked && PlayerStats.instance.remainingSkillPoints > 0
                && AllPreviousSkillsUnlocked(skill.previousSkills)
                && HaveRequiredStats(skill.attributeReqs) && !InputHandler.instance.tutorialPopupActive)
                {
                    ControllerUIManager.instance.SetSelectText("Unlock");
                }
                else
                {
                    ControllerUIManager.instance.SelectTextActive(false);
                }
            }
            else
            {
                Debug.Log($"Skill was null for ID {skillID}, in Set Skill Button Interact UI process.");
            }
        }
    }
}







