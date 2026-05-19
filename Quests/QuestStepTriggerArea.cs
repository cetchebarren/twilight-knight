using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace etchebarren
{
    public class QuestStepTriggerArea : MonoBehaviour
    {
        [Tooltip("The ID of the Quest Step (Type: Reach) that will be triggered/completed by triggering this collider.")]
        public int questStepID = -1;

        [Header("Optional events to fire off if this trigger successfully completes a Quest Step")]
        public UnityEvent events;
        private bool alreadyTriggered = false;
        public bool hideMeshOnStart = true;

        void Start()
        {
            if(hideMeshOnStart)
                GetComponent<MeshRenderer>().enabled = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                bool questStepFound = QuestManager.instance.ReachAreaTriggered(questStepID);

                if (questStepFound && events != null)
                {
                    events.Invoke();
                }
            }
        }

        public void NotificationViaUnityEvent(string message)
        {
            TextNotificationsManager.instance.NewTextNotifaction(message);
        }
    }
}
