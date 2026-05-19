using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace etchebarren
{
    public class SceneExit : MonoBehaviour
    {
        //public SceneEntrance sceneEntrance;
        [Header("Should not include spaces. This is the scene name as shown in build settings.")]
        public string sceneToGoTo;

        private bool alreadyTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !alreadyTriggered)
            {
                alreadyTriggered = true;

                // Keep record of this scene exit name so that we can spawn in the correct place
                WorldStateManager.instance.lastSceneExitName = SceneManager.GetActiveScene().name;
                // Record new scene name so that we know what scene to load when called from input handler
                WorldStateManager.instance.newSceneName = sceneToGoTo;
                InteractPrompt.instance.AddInteraction(this);
                
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                alreadyTriggered = false;
                InteractPrompt.instance.RemoveInteraction(this);
                WorldStateManager.instance.lastSceneExitName = "";
            }
        }
    }
}
