using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class MountTrigger : MonoBehaviour
    {
        public bool isLeftTrigger = false;
        public bool isRightTrigger = false;

        public bool alreadyTriggered = false;

        public GameObject mountPrompt;
        public MountManager mountManager;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered && mountManager.mountIsHealthy)
            {
                alreadyTriggered = true;
                InputHandler.instance.canMountRight = isRightTrigger;
                InputHandler.instance.canMountLeft = isLeftTrigger;
                mountPrompt.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                alreadyTriggered = false;
                InputHandler.instance.canMountRight = false;
                InputHandler.instance.canMountLeft = false;
                mountPrompt.SetActive(false);
            }
        }
    }
}
