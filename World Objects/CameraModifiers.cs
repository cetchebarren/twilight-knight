using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class CameraModifiers : MonoBehaviour
    {
        public float targetCameraDistance = 1.0f;
        public float lockedPivotModifier = 1.0f;
        public float unlockedPivotModifier = 1.0f;
        public float transitionDuration = 1.0f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                float cameraDistanceModifier = 1.0f;
                if (targetCameraDistance > CameraHandler.instance.cameraDistanceMultiplier)
                {
                    cameraDistanceModifier = targetCameraDistance / CameraHandler.instance.cameraDistanceMultiplier;
                }

                CameraHandler.instance.SetCameraModifiers(cameraDistanceModifier, lockedPivotModifier, unlockedPivotModifier, transitionDuration);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                CameraHandler.instance.SetCameraModifiers(1.0f, 1.0f, 1.0f, transitionDuration);
            }
        }

        public void ResetCameraModifiers()
        {
            CameraHandler.instance.SetCameraModifiers(1.0f, 1.0f, 1.0f, transitionDuration);
        }
    }
}
