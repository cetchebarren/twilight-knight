using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class ShopTrigger : MonoBehaviour
    {
        private bool alreadyTriggered = false;
        private Vector3 originalPosition;
        private Vector3 upPosition;

        // For now, using a shop ID to determine loot, etc
        public string shopName = "Stalwart Camp Shop";
        public SphereCollider interactTrigger;
        [SerializeField] MeshRenderer visualizer;
        [SerializeField] bool hideVisualizerOnStart = true;

        void Start()
        {
            // Record original position
            originalPosition = interactTrigger.center;
            // Calculate the position to move up to (for "resetting" the trigger)
            upPosition = originalPosition + Vector3.up * 20.0f;

            if (hideVisualizerOnStart) visualizer.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered)
            {
                alreadyTriggered = true;

                InteractPrompt.instance.AddShopInteraction(shopName);

                // Update reference in Shop manager so that this trigger can be reset upon closing shop in ShopMenuManager
                ShopMenuManager.instance.currentShopTrigger = this;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                alreadyTriggered = false;
                Debug.Log("EXITTED SHOP TRIGGER");
                InteractPrompt.instance.RemoveShopInteraction(shopName);
            }
        }

        public void ResetTrigger()
        {
            StartCoroutine(ResetTriggerCoroutine());
        }

        private IEnumerator ResetTriggerCoroutine()
        {
            //This actually works by moving the trigger away and back into place
            interactTrigger.center = upPosition;
            alreadyTriggered = false;
            yield return new WaitForSeconds(0.1f);
            interactTrigger.center = originalPosition;
        }
    }
}
