using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class PushableTriggerCollider : MonoBehaviour
    {
        public enum Direction
        {
            Forward,
            Backward
        }

        public Direction direction;

        public PushableObject pushable;

        public bool alreadyTriggered = false;
        public bool hideMeshOnStart = true;

        void Start()
        {
            if (hideMeshOnStart)
                GetComponent<MeshRenderer>().enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered)
            {
                alreadyTriggered = true;

                if (direction == Direction.Forward)
                {
                    if (pushable.currentPosition < pushable.maxPosition)
                    {
                        Debug.Log("Trigger entered!");
                        pushable.currentTrigger = this;
                        InteractPrompt.instance.AddInteraction(pushable);
                    }
                }
                else
                {
                    if (pushable.currentPosition > pushable.minPosition)
                    {
                        Debug.Log("Trigger entered!");
                        pushable.currentTrigger = this;
                        InteractPrompt.instance.AddInteraction(pushable);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player" && alreadyTriggered)
            {
                Debug.Log("Trigger exited!");
                alreadyTriggered = false;
                InteractPrompt.instance.RemoveInteraction(pushable);
            }
        }
    }
}
