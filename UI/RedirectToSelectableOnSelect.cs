using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace etchebarren
{
    public class RedirectToSelectableOnSelect : MonoBehaviour, ISelectHandler
    {
        // This script allows to select buttons/sliders from a list. This is useful in cases where
        // Explicit navigation is required for specific UI behavior but the target selectable
        // will change depending on which submenu (male hair styles vs female hair styles) is active
        public GameObject[] selectables;

        public void OnSelect(BaseEventData eventData)
        {
            foreach(GameObject selectable in selectables)
            {
                if (selectable.gameObject.activeInHierarchy)
                {
                    StartCoroutine(SelectNew(selectable.gameObject));
                    return;
                }
            }
        }

        public IEnumerator SelectNew(GameObject newSelectable)
        {
            // Wait until the end of the frame
            yield return new WaitForEndOfFrame();

            EventSystem.current.SetSelectedGameObject(newSelectable);
        }
    }
}
