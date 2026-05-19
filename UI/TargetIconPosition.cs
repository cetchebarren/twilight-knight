using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class TargetIconPosition : MonoBehaviour
    {
        private CameraHandler cameraHandler;
        //public Transform target; // Reference to the currently targeted enemy
        public Image icon; // Reference to the UI Image icon
        public Camera mainCamera; // Reference to the main camera

        private RectTransform iconRectTransform;
        private Coroutine iconUpdateCoroutine; // Coroutine reference

        public float minScale = 0.8f; // Minimum scale
        public float maxScale = 1.2f; // Maximum scale
        public float scaleSpeed = 1.0f; // Scale change speed

        private void Start()
        {
            cameraHandler = CameraHandler.instance;
            // Get the RectTransform of the icon
            iconRectTransform = icon.GetComponent<RectTransform>();

            // Start the coroutine to update the icon's position
            //StartUpdatingIconPosition();
        }

        private void Update()
        {
            // Additional logic if needed
        }

        // Start the coroutine to update the icon's position
        public void StartUpdatingIconPosition()
        {
            if (iconUpdateCoroutine == null)
            {
                icon.enabled = true;
                iconUpdateCoroutine = StartCoroutine(UpdateIconPosition());
            }
        }

        // Stop the coroutine updating the icon's position
        public void StopUpdatingIconPosition()
        {
            if (iconUpdateCoroutine != null)
            {
                StopCoroutine(iconUpdateCoroutine);
                icon.enabled = false;
                iconUpdateCoroutine = null;
            }
        }

        private IEnumerator UpdateIconPosition()
        {
            while (true)
            {
                if (cameraHandler.currentLockOnTarget != null && mainCamera != null) 
                {
                    //Debug.Log("Runnnig icon code...");
                    // Calculate the world space position where you want the icon to appear (e.g., above the enemy)
                    Vector3 targetPosition = cameraHandler.currentLockOnTarget.lockOnPoint.position; // + Vector3.up * 2.0f; // Adjust the offset as needed

                    // Convert the world position to screen space using the main camera
                    Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

                    // Set the icon's position in world space
                    iconRectTransform.position = screenPosition;

                    // Scale the icon up and down within the specified range
                    float scale = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(Time.time * scaleSpeed, 1.0f));
                    iconRectTransform.localScale = new Vector3(scale, scale, 1.0f);
                }

                yield return null;
            }
        }
    }
}
