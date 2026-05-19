using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace etchebarren
{
    public class AutoScroll : MonoBehaviour
    {
        [Header("SCROLLING SETTINGS:")]
        public RectTransform viewport;
        public RectTransform contents;
        public float shiftScale = 3.0f;

        [Header("SCROLL RECT FOR FIXING CRUNCHING:")]
        public ScrollRect scrollRect;
        public bool rebuildLayoutOnStart = false;

        private IEnumerator Start()
        {
            if(rebuildLayoutOnStart)
            {
                yield return null; // Wait for one frame
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
                Debug.Log("Layout Rebuild Complete.");
            }
        }

        public void ScrollToSelectedElement()
        {
            GameObject selectedGameObject = EventSystem.current.currentSelectedGameObject;

            if (selectedGameObject == null)
            {
                //Debug.LogError("No selected game object.");
                return;
            }

            RectTransform selectedElement = selectedGameObject.GetComponent<RectTransform>();

            if (selectedElement == null)
            {
                Debug.LogError("Selected game object doesn't have a RectTransform.");
                return;
            }

            bool isCompletelyWithinViewport = IsRectTransformCompletelyWithinViewport(selectedElement, viewport);
            //Debug.Log("Is completely within viewport: " + isCompletelyWithinViewport);

            bool controller;

            if(ControllerUIManager.instance != null)
            {
                controller = ControllerUIManager.instance.isUsingController();
            }
            else // This happens in main menu 
            {
                controller = Gamepad.all.Count > 0;
            }

            if (!isCompletelyWithinViewport && controller)
            {
                AdjustContentPosition(selectedElement, viewport);
            }
        }

        private bool IsRectTransformCompletelyWithinViewport(RectTransform rectTransform, RectTransform viewport)
        {
            Vector3 selectedTop = rectTransform.TransformPoint(new Vector3(0, rectTransform.rect.yMax, 0));
            Vector3 selectedBottom = rectTransform.TransformPoint(new Vector3(0, rectTransform.rect.yMin, 0));

            Vector3 viewportTop = viewport.TransformPoint(new Vector3(0, viewport.rect.yMax, 0));
            Vector3 viewportBottom = viewport.TransformPoint(new Vector3(0, viewport.rect.yMin, 0));

            //Debug.Log("Selected:    Top: " + selectedTop.y + "|" + "Bottom: " + selectedBottom.y);
            //Debug.Log("Viewport:    Top: " + viewportTop.y + "|" + "Bottom: " + viewportBottom.y);

            bool isWithinVerticalBounds = selectedTop.y <= viewportTop.y && selectedBottom.y >= viewportBottom.y;
            //Debug.Log("Is within vertical bounds: " + isWithinVerticalBounds);

            return isWithinVerticalBounds;
        }

        private void AdjustContentPosition(RectTransform targetElement, RectTransform viewport)
        {
            Vector3 targetTop = targetElement.TransformPoint(new Vector3(0, targetElement.rect.yMax, 0));
            Vector3 targetBottom = targetElement.TransformPoint(new Vector3(0, targetElement.rect.yMin, 0));

            Vector3 viewportTop = viewport.TransformPoint(new Vector3(0, viewport.rect.yMax, 0));
            Vector3 viewportBottom = viewport.TransformPoint(new Vector3(0, viewport.rect.yMin, 0));

            Vector3 contentPosition = contents.localPosition;

            if (targetTop.y > viewportTop.y)
            {
                float verticalDistance = targetTop.y - viewportTop.y;
                //Debug.Log("DIFFERENCE: " + verticalDistance);
                contentPosition.y -= verticalDistance * shiftScale;
            }
            else if (targetBottom.y < viewportBottom.y)
            {
                float verticalDistance = targetBottom.y - viewportBottom.y;
                //Debug.Log("DIFFERENCE: " + verticalDistance);
                contentPosition.y -= verticalDistance * shiftScale;
            }

            contents.localPosition = contentPosition;
        }

        // FIX FOR CRUNCHED INVENTORY ITEMS (ARMOR)

    }
}
