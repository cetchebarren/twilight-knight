using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class StablesTrigger : MonoBehaviour
    {
        private bool alreadyTriggered = false;

        public GameObject stables;
        public Camera stablesCamera;
        public PortraitCapture portraitCapture;
        public GameObject spotLight;
        public SphereCollider interactTrigger;
        private Vector3 originalPosition;
        private Vector3 upPosition;
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
                StablesShop.instance.stables = stables;
                StablesShop.instance.currentStablesTrigger = this;
                StablesShop.instance.stablesCamera = stablesCamera;
                StablesShop.instance.portraitCapture = portraitCapture;
                StablesShop.instance.spotLight = spotLight;

                InteractPrompt.instance.AddShopInteraction("Stables Shop");

                // Moved to Input Handler
                //StablesShop.instance.gameObject.SetActive(true);
                //UIChangeSelectedButton.instance.ChangeSelectedButtonTo(StablesShop.instance.nameButton);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                alreadyTriggered = false;
                InteractPrompt.instance.RemoveShopInteraction("Stables Shop");
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
