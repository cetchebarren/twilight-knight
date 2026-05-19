using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace etchebarren
{
    public class SceneQuestMarkersManager : MonoBehaviour
    {
        public QuestStepMarker[] questStepMarkersInScene;

        public QuestStepMarker[] sceneTransitionQuestStepMarkers;

        public DialogueAgent[] dialogueAgentsInScene;


        void Awake()
        {
            var allMarkers = FindObjectsOfType<QuestStepMarker>(true);

            questStepMarkersInScene = allMarkers
                .Where(m => m.sceneExit == null)
                .ToArray();

            sceneTransitionQuestStepMarkers = allMarkers
                .Where(m => m.sceneExit != null)
                .ToArray();

            dialogueAgentsInScene = FindObjectsOfType<DialogueAgent>(true);
        }

        public QuestStepMarker GetQuestMarker(int questStepID)
        {
            if(questStepMarkersInScene.Length > 0)
            {
                foreach (QuestStepMarker marker in questStepMarkersInScene)
                {
                    if (marker.questStepID == questStepID)
                    {
                        return marker;
                    }
                }
            }

            return null;
        }

        public QuestStepMarker GetQuestMarkerForScene(string newScene)
        {
            if (sceneTransitionQuestStepMarkers.Length > 0)
            {
                foreach (QuestStepMarker marker in sceneTransitionQuestStepMarkers)
                {
                    if (marker.sceneExit != null)
                    {
                        if (marker.sceneExit.sceneToGoTo == newScene)
                        {
                            return marker;
                        }
                    }
                }
            }
            return null;
        }

        public QuestStepMarker GetQuestMarkerForDialogueAgent(string dialogueAgentName)
        {
            if (dialogueAgentsInScene.Length > 0)
            {
                foreach (DialogueAgent agent in dialogueAgentsInScene)
                {
                    if(agent.agentName == dialogueAgentName)
                    {
                        return agent.questStepMarker;
                    }
                }
            }
            return null;
        }

        void OnEnable()
        {
            QuestMarkerManager.instance.sceneQuestMarkersManager = this;
            SetMarkers();
        }

        /* This function will be called when loading a scene to check what markers should currently be active */
        public void SetMarkers()
        {
            // Disable All Quest Markers (Visible/Active in Scene by default)
            foreach (QuestStepMarker qsm in questStepMarkersInScene)
            {
                qsm.gameObject.SetActive(false);
            }

            // Enable quest markers based on quest status
            foreach (Quest quest in QuestManager.instance.quests)
            {
                if (quest.questActive && !quest.completed)
                {
                    quest.GetCurrentQuestStep().SetMarkerActive();
                }
                if (quest.tracked) // Also, reset tracked quest when loading scene (this sets the references for the minimapoutofboundsindicator.cs
                {
                    quest.TrackQuest();
                }
            }
        }
    }
}
