using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class QuestMarkerManager : MonoBehaviour
    {
        public static QuestMarkerManager instance;

        public SceneQuestMarkersManager sceneQuestMarkersManager;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}