using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace etchebarren
{
    /// <summary>
    /// This script allows the auto scroll object to adjust to the selected object in the event system when the button is selected
    /// This is used in non-inventory menus with an autoscroll componenet, as those buttons call the function from display info box
    /// Used in: character creation details screen and plaery menu option sub menus
    /// </summary>

    public class AutoScrollOnSelect : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        public AutoScroll associatedAutoScroll;

        public void OnSelect(BaseEventData eventData)
        {
            associatedAutoScroll.ScrollToSelectedElement();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            associatedAutoScroll.ScrollToSelectedElement();
        }
    }
}
