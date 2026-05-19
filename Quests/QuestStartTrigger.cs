using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class QuestStartTrigger : MonoBehaviour
    {
        [Tooltip("The ID of the Quest that will be triggered by this collider.")]
        public Quest questToStart;
        public bool forceTrack = false;
        public QuestStep[] prereqQueststeps;

        private bool alreadyTriggered = false;
        public bool hideMeshOnStart = true;

        void Start()
        {
            if (hideMeshOnStart)
                GetComponent<MeshRenderer>().enabled = false;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                foreach(QuestStep prereq in prereqQueststeps)
                {
                    if (!QuestManager.instance.CheckQuestStepComplete(prereq.questStepID)) return;
                }

                QuestManager.instance.StartQuest(questToStart.questID, forceTrack);
            }
        }
    }
}
