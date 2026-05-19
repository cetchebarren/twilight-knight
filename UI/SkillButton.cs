using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace etchebarren
{
    public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        public int skillID = 0;

        public GameObject skillInfoBox;

        public SkillTreeManager skillTreeManager;

        private Color unselected = new Color(0, 0, 0, 0.51f);

        public Image border;

        public GameObject skillLocked;

        private bool performOnPointerExit = false;

        public void OnDisable()
        {
            skillInfoBox.SetActive(false);
            border.color = unselected;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Unselect previous skill (if any)
            SkillButton currentlySelected = EventSystem.current.currentSelectedGameObject?.GetComponentInChildren<SkillButton>();
            if (currentlySelected != null)
            {
                currentlySelected.skillInfoBox.SetActive(false);
                ControllerUIManager.instance.SelectTextActive(false);
                currentlySelected.border.color = unselected;
                performOnPointerExit = false;
            }
            else performOnPointerExit = true;
            UIChangeSelectedButton.instance.ChangeSelectedButtonTo(this.gameObject, setRegardlessOfControls: true);

            skillTreeManager.UpdateDescriptions(skillID);
            skillInfoBox.SetActive(true);
            UpdateDisplay();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!performOnPointerExit) return;
            skillInfoBox.SetActive(false);
            ControllerUIManager.instance.SelectTextActive(false);
            border.color = unselected;
        }

        public void OnSelect(BaseEventData eventData)
        {
            //Debug.Log("Selected skill " + skillID);
            UpdateDisplay();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            //ControllerUIManager.instance.SetSelectText("Select");
            skillInfoBox.SetActive(false);
            border.color = unselected;
        }

        public void UpdateDisplay()
        {
            if (PlayerStats.instance.GetSkillRankByID(skillID) < PlayerStats.instance.skills[skillID].maxRank)
            {
                ControllerUIManager.instance.SetSelectText("Unlock");
            }
            else
            {
                ControllerUIManager.instance.SelectTextActive(false);
            }
            skillTreeManager.UpdateDescriptions(skillID);
            border.color = Color.white;
            skillInfoBox.SetActive(true);
            skillTreeManager.SetSkillButtonInteractUI(skillID);
            if (PlayerStats.instance.skills[skillID].unlockRequirementText == "Recently Unlocked")
            {
                skillTreeManager.skillsArray[skillID].transform.GetChild(4).gameObject.SetActive(false);
                skillTreeManager.skillsArray[skillID].transform.GetChild(5).gameObject.SetActive(false);
                skillTreeManager.skillsArray[skillID].transform.GetChild(1).GetComponent<Image>().color = skillTreeManager.lightSkill;
                PlayerStats.instance.skills[skillID].unlockRequirementText = "";
            }
        }
    }
}
