using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEngine.EventSystems;// Required when using Event data.

namespace etchebarren
{
    public class SkillsSubMenuTabs : MonoBehaviour//, ISelectHandler// required interface when using the OnSelect method.
    {
        private Vector3 initialScale = new Vector3(1, 1, 1);
        //private Button thisButton;
        public TextMeshProUGUI thisTabText;
        public GameObject[] otherTabs;
        //public List<Image> otherIcons = new List<Image>();

        //Do this when the selectable UI object is selected.
        public void OnSelect()
        {
            //increase size and transparency of selected tab
            transform.localScale = initialScale * 1.5f; // Increase scale by 50%
            thisTabText.color = new Color(thisTabText.color.r, thisTabText.color.g, thisTabText.color.b, 1.0f);

            //reset other tabs
            for (int i = 0; i < otherTabs.Length; i++)
            {
                otherTabs[i].transform.localScale = initialScale;
                //otherTabs[i].SetActive(false);
                TextMeshProUGUI otherTab = otherTabs[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                otherTab.color = new Color(otherTab.color.r, otherTab.color.g, otherTab.color.b, 0.5f);
                //otherTabs[i].SetActive(true);
            }

        }

    }
}
