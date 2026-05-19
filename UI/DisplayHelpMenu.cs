using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace etchebarren
{
    public class DisplayHelpMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("TMP Element containing Header Text")]
        public TextMeshProUGUI listItemHeaderTMP;

        public void OnPointerEnter(PointerEventData eventData)
        {
            HelpMenu.instance.DisplayFromMenu(listItemHeaderTMP.text);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //HelpMenu.instance.Close(); // may not want to close on these events, no reason to and bad ux
        }

        public void OnSelect(BaseEventData eventData)
        {
            HelpMenu.instance.DisplayFromMenu(listItemHeaderTMP.text);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            //HelpMenu.instance.Close();
        }
    }
}
