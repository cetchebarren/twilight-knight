using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class QuestInfoBox : MonoBehaviour
    {
        public TextMeshProUGUI questName;
        public TextMeshProUGUI questDescription;
        public TextMeshProUGUI overallProgress;
        public TextMeshProUGUI currentObjective;
        public TextMeshProUGUI objectiveProgress;
        public TextMeshProUGUI questRewards;
        public TextMeshProUGUI rewardHeader;
        public TextMeshProUGUI goldCount;
        public GameObject goldParent;
        public TextMeshProUGUI XP;
        public TextMeshProUGUI questStepLocation;

        public void UpdateQuestBoxInfo(Quest quest)
        {
            questName.text = quest.questName;
            questDescription.text = quest.description;

            float questProgress = quest.GetQuestOverallQuestProgress();
            overallProgress.text = "Overall Quest Progress: " + questProgress.ToString("F0") + "%";

            if(!quest.completed)
            {
                QuestStep currentQuestStep = quest.GetCurrentQuestStep();
                currentObjective.text = currentQuestStep.questStepDescription;
                objectiveProgress.text = "Objective Progress: " + currentQuestStep.currentQuantity.ToString() + " / " + currentQuestStep.quantityRequired.ToString();
                rewardHeader.text = "Rewards:";
                if(currentQuestStep.questStepSceneLocation != null && currentQuestStep.questStepSceneLocation != "")
                {
                    questStepLocation.text = "Location: " + InteractPrompt.instance.AddSpacesBeforeCaps(currentQuestStep.questStepSceneLocation);
                }
                else
                {
                    questStepLocation.text = "";
                }
            }
            else
            {
                currentObjective.text = "Quest Complete!";
                questStepLocation.text = "Location: " + InteractPrompt.instance.AddSpacesBeforeCaps(quest.GetLastQuestStep().questStepSceneLocation);
                objectiveProgress.text = "";
                rewardHeader.text = "Rewarded:";
            }

            string rarityOfRandomItem = "";

            switch (quest.randomRewardRarity)
            {
                case "common":
                    rarityOfRandomItem = "Common ";
                    break;
                case "uncommon":
                    rarityOfRandomItem = "Uncommon ";
                    break;
                case "rare":
                    rarityOfRandomItem = "Rare ";
                    break;
                case "epic":
                    rarityOfRandomItem = "Epic ";
                    break;
                case "legendary":
                    rarityOfRandomItem = "Legendary ";
                    break;
                default:
                    break;
            }

            string rewards = "";
            switch (quest.randomRewardType)
            {
                case Quest.RandomRewardType.Random:
                    rewards += "Random " + rarityOfRandomItem + "Equipment x 1";
                    break;
                case Quest.RandomRewardType.Sword:
                    rewards += "Random " + rarityOfRandomItem + "Sword x 1";
                    break;
                case Quest.RandomRewardType.Shield:
                    rewards += "Random " + rarityOfRandomItem + "Shield x 1";
                    break;
                case Quest.RandomRewardType.TorsoArmorItem:
                    rewards += "Random " + rarityOfRandomItem + "Breastplate x 1";
                    break;
                case Quest.RandomRewardType.HandsArmorItem:
                    rewards += "Random " + rarityOfRandomItem + "Gauntlets x 1";
                    break;
                case Quest.RandomRewardType.LegsArmorItem:
                    rewards += "Random " + rarityOfRandomItem + "Greaves x 1";
                    break;
                case Quest.RandomRewardType.RingItem:
                    rewards += "Random " + rarityOfRandomItem + "Ring x 1";
                    break;
                case Quest.RandomRewardType.AmuletItem:
                    rewards += "Random " + rarityOfRandomItem + "Amulet x 1";
                    break;
                case Quest.RandomRewardType.None:
                    break;
                default:
                    break;
            }

            if(rewards != "" && quest.specificItems.Length > 0)
            {
                rewards += ", ";
            }

            foreach(Item item in quest.specificItems)
            {
                rewards += (item.itemName + " x " + item.count.ToString());
            }

            questRewards.text = rewards;
            questRewards.gameObject.SetActive(rewards != "");
     
            if(quest.gold > 0)
            {
                goldParent.SetActive(true);
                goldCount.text = quest.gold.ToString("#,##0");
            }
            else
            {
                goldParent.SetActive(false);
            }

            if(quest.XP > 0)
            { 
                XP.text = "+" + quest.gold.ToString("#,##0") + " XP";
                XP.gameObject.SetActive(true);
            }
            else
            {
                XP.gameObject.SetActive(false);
            }

            gameObject.SetActive(true);

        }
    }
}
