using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.EventSystems;// Required when using Event data.

namespace etchebarren
{
    public class InvSubMenuButtons : MonoBehaviour//, ISelectHandler// required interface when using the OnSelect method.
    {
        private Vector3 initialScale = new Vector3(1, 1, 1);
        //private Button thisButton;
        public Image thisIcon;
        public List<GameObject> otherIcons = new List<GameObject>();
        //public List<Image> otherIcons = new List<Image>();

        //Do this when the selectable UI object is selected.
        public void OnSelect()
        {
            //increase size and transparency of selected tab
            transform.localScale = initialScale * 1.35f; // Increase scale by 35%
            thisIcon.color = new Color(thisIcon.color.r, thisIcon.color.g, thisIcon.color.b, 1.0f);

            //reset other icons
            for(int i = 0; i < otherIcons.Count; i++)
            {
                otherIcons[i].transform.localScale = initialScale;
                Image otherIcon = otherIcons[i].GetComponent<Image>();
                otherIcon.color = new Color(otherIcon.color.r, otherIcon.color.g, otherIcon.color.b, 0.5f);
            }

        }

    }
}
