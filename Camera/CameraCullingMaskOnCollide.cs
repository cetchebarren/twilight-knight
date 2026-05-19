using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used to hide the player mesh when the camera is too close. This prevents ugly clipping and being able to see inside of the player model.
/// Typically happens when the camera is stuck against the way with the player very close to it.
/// </summary>

namespace etchebarren
{
    public class CameraCullingMaskOnCollide : MonoBehaviour
    {
        public Camera mainCamera;

        private bool alreadyTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered)
            {
                alreadyTriggered = true;

                RemoveLayer("Controller");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                alreadyTriggered = false;

                AddLayer("Controller");
            }
        }

        public void AddLayer(string layerName)
        {
            // Get the layer index
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex != -1)
            {
                // Add the layer to the camera's culling mask
                mainCamera.cullingMask |= 1 << layerIndex;
            }
        }

        public void RemoveLayer(string layerName)
        {
            // Get the layer index
            int layerIndex = LayerMask.NameToLayer(layerName);
            if (layerIndex != -1)
            {
                // Remove the layer from the camera's culling mask
                mainCamera.cullingMask &= ~(1 << layerIndex);
            }
        }
    }
}
