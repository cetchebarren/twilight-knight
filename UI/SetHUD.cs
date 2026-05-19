using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class SetHUD : MonoBehaviour
    {
        [Header("Elements to Set")]
        public GameObject[] UIElementsToHide;
        public SpellsHUDManager spellsHUDManager;
        public GameObject leftTriggerSpellIcon;
        public GameObject horseStatusHUD;

        [Header("References")]
        public GameObject horseAIVer;
        public GameObject horseMountVer;

        public void SetHUDActive(bool show)
        {
            foreach (GameObject element in UIElementsToHide)
            {
                element.SetActive(show);
            }

            if (!show)
            {
                //leftTriggerSpellIcon.SetActive(false);
                spellsHUDManager.StopManaCheck();
                spellsHUDManager.gameObject.SetActive(false);
            }
            else
            {
                //leftTriggerSpellIcon.SetActive(true);
                spellsHUDManager.gameObject.SetActive(true);
            }

            bool horseHUD = horseAIVer.activeSelf || horseMountVer.activeSelf;
            horseStatusHUD.SetActive(horseHUD);
        }
    }
}
