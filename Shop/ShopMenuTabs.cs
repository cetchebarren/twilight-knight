using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class ShopMenuTabs : MonoBehaviour
    {
        private Vector3 initialScale = Vector3.one; //new Vector3(1,1,1)
        public Image thisTab;
        public List<GameObject> otherTabs = new List<GameObject>();

        //Do this when the selectable UI object is selected.
        public void OnSelect()
        {
            //increase size and transparency of selected tab
            transform.localScale = initialScale * 1.15f; // Increase scale by 15%
            thisTab.color = new Color(1, 1, 1);

            thisTab.transform.SetAsLastSibling(); //move to front

            //reset other icons
            for (int i = 0; i < otherTabs.Count; i++)
            {
                otherTabs[i].transform.localScale = initialScale;
                Image otherTab = otherTabs[i].GetComponent<Image>();
                otherTab.color = new Color(0.6f, 0.6f, 0.6f);  // new Color(otherTab.color.r, otherTab.color.g, otherTab.color.b);
            }

        }
    }
}
