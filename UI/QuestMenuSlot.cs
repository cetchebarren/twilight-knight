using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class QuestMenuSlot : MonoBehaviour
    {
        public TextMeshProUGUI questName;
        public TextMeshProUGUI questType;

        public GameObject newNotification;
        public GameObject trackedIndicator;

        public PlayerMenuManager playerMenuManager;

        [Header("DO NOT SET MANUALLY:")]
        public Quest quest;

        private void OnEnable()
        {
            // Had to do this to prevent first weapon inventory slot from being disabled when menu is first opened
            if (transform.parent.transform.GetChild(0).gameObject != gameObject)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        public void AddQuestData(Quest _quest, bool activeQuest)
        {
            quest = _quest;
            gameObject.SetActive(true);
            questName.text = quest.questName;
            newNotification.SetActive(quest.flaggedAsNew);
            if (quest.mainQuest)
            {
                questType.text = "Main Quest";
            }
            else
            {
                questType.text = "Side Quest";
            }
            // Only update for active quest slots, as complete quests do not have this component
            if (activeQuest)
            {
                trackedIndicator.SetActive(quest.tracked);
            }
            newNotification.SetActive(quest.flaggedAsNew);     
        }

        public void ClearSlot()
        {
            quest = null;
            gameObject.SetActive(false);
        }

        public void TrackThisQuest()
        {
            //Debug.Log("Quest tracked:" + quest.questName);
            if (quest.tracked)
            {
                UIAudioManager.instance.PlayTrackQuestAudio(true);
                quest.UntrackQuest();
                ControllerUIManager.instance.SetSelectText("Track");
            }
            else
            {
                UIAudioManager.instance.PlayTrackQuestAudio(false);
                quest.TrackQuest();
                ControllerUIManager.instance.SetSelectText("Untrack");
            }

            // Update UI without sorting. Sorting will be executed only when changing menus (from PlayerMenuManager.cs SelectQuestTab and SwitchInventoryTab()
            PlayerMenuManager.instance.UpdateUI();
        }


    }
}
