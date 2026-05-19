using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class MinimapOutOfBoundsIndicator : MonoBehaviour
    {
        public static MinimapOutOfBoundsIndicator instance;

        public Transform trackedIconPivot; // Reference to the tracked icon pivot

        public Camera minimapCamera;

        public Transform playerLocationReference;

        public GameObject target; // Reference to the target icon location

        public float minimapRange = 20.0f;

        public float distance = 0.0f;

        public Image trackedIcon;

        public TextMeshProUGUI[] trackedDistanceTexts;

        public GameObject trackedDistanceContainer;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            if (target == null || playerLocationReference == null) return;

            UpdateTrackedDistance();

            // Calculate the direction from the player to the target
            Vector3 direction = target.transform.position - playerLocationReference.position;

            // Calculate distance here
            distance = direction.magnitude;

            QuestStepMarker qsm = target.GetComponent<QuestStepMarker>();

            if (distance <= minimapRange)
            {
                //Debug.Log("INSIDE Minimap Range");
                trackedIcon.enabled = false;
                trackedDistanceContainer.SetActive(false);
                if (qsm != null)
                {
                    if (!qsm.isSearchArea) qsm.questMarker.SetActive(true);
                    if (qsm.defaultMinimapIconToHide != null)
                    {
                        qsm.defaultMinimapIconToHide.SetActive(true);
                    }

                }
                return;
            }

            trackedDistanceContainer.SetActive(true);
            trackedIcon.enabled = true;

            // hide default icons when displaying out of minimap range icon
            if(qsm != null)
            {
                qsm.questMarker.SetActive(false);
                if (qsm.defaultMinimapIconToHide != null)
                {
                    qsm.defaultMinimapIconToHide.SetActive(false);
                }

            }

            // Ignore the z-coordinate of the direction vector
            direction.y = 0f;

            // Ensure the direction is not zero (avoid division by zero)
            if (direction != Vector3.zero)
            {
                // Normalize the direction vector
                direction.Normalize();

                // Calculate the rotation angle in degrees
                float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

                // Adjust the angle to match the desired rotation
                angle -= 90f; // Apply an offset

                // Apply the rotation to the pivot
                trackedIconPivot.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public void SetTargetReference(GameObject newTarget, SpriteRenderer spriteLocation)
        {
            distance = minimapCamera.orthographicSize;

            target = newTarget;

            trackedIcon.sprite = spriteLocation.sprite;
            trackedIcon.color = spriteLocation.color;

            trackedIcon.enabled = true;
        }

        public void UntrackTarget(GameObject targetToUntrack)
        {
            if (target == targetToUntrack)
            {
                target = null;

                trackedIcon.enabled = false;
            }
        }

        public void UpdateTrackedDistance()
        {
            string distanceTxt = Mathf.RoundToInt(distance).ToString() + "m";
            SetDistanceTexts(distanceTxt);
        }

        public void SetDistanceTexts(string distanceTxt)
        {
            foreach (TextMeshProUGUI distanceText in trackedDistanceTexts)
            {
                distanceText.text = distanceTxt;
            }
        }

    }
}
