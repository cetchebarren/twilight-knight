using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class TeleportWaypointTrigger : MonoBehaviour
    {
        private bool alreadyTriggered = false;
        public TeleportWaypoint teleportWaypoint;
        public bool unlockOnTriggerEnter = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered)
            {
                alreadyTriggered = true;

                if (unlockOnTriggerEnter)
                {
                    teleportWaypoint.UnlockTeleportWaypoint(false);
                }
                else
                {
                    InteractPrompt.instance.AddInteraction(teleportWaypoint);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                alreadyTriggered = false;
                InteractPrompt.instance.currentTeleportWaypoint = null;
                InteractPrompt.instance.RemoveInteraction(teleportWaypoint);
            }
        }
    }
}
