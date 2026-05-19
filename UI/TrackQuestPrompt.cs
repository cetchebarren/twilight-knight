using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class TrackQuestPrompt : MonoBehaviour
    {
        public InputHandler inputHandler;

        void OnEnable()
        {
            inputHandler.UpdateTrackQuestListeners(true);
        }

        void Update()
        {
            inputHandler.HandleTrackQuestInput();
        }

        void OnDisable()
        {
            inputHandler.UpdateTrackQuestListeners(false);
        }
    }
}
