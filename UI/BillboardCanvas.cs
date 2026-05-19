using UnityEngine;

namespace etchebarren
{
    public class BillboardCanvas : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            // Find the main camera in the scene
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found. Make sure you have a camera in the scene.");
            }
        }

        private void Update()
        {
            // Check if the main camera is found
            if (mainCamera != null)
            {
                // Calculate the desired rotation to face the camera
                Quaternion lookRotation = Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up);

                // Apply the rotation to the canvas
                transform.rotation = lookRotation;
            }
        }
    }
}
