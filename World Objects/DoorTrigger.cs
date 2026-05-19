using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class DoorTrigger : MonoBehaviour
    {
        public Door door;
        public bool frontTrigger = true;

        public bool alreadyTriggered = false;
        public MeshRenderer visualizer;
        [SerializeField] bool hideVisualizerOnAwake = true;

        void Awake()
        {
            if(hideVisualizerOnAwake) visualizer.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered)
            {
                alreadyTriggered = true;
                door.playerInFront = frontTrigger;
                InteractPrompt.instance.AddInteraction(door);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                alreadyTriggered = false;
                InteractPrompt.instance.RemoveInteraction(door);
            }
        }
    }
}
