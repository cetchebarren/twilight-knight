using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [System.Serializable]
    public class Skill
    {
        public int ID = 0; 
        public string name = "Default Skill Name";
        public int rank = 0;
        public int maxRank = 3;
        public string[] descriptions;
        public float[] effects;
        //public int levelReq;
        public int[] attributeReqs;
        public int[] previousSkills;
        public bool skillLocked = false;
        public string unlockRequirementText = "";

        public Skill(int ID, string name, int maxRank, string[] descriptions,
                     float[] effects, /*int levelReq,*/ int[] attributeReqs, int[] previousSkills, bool skillLocked, string unlockRequirementText = "")
        {
            this.ID = ID;
            this.name = name;
            this.rank = 0;
            this.maxRank = maxRank;
            this.descriptions = descriptions;
            this.effects = effects;
            //this.levelReq = levelReq;
            this.attributeReqs = attributeReqs;
            this.previousSkills = previousSkills;
            this.skillLocked = skillLocked;
            this.unlockRequirementText = unlockRequirementText;
        }

        public string CurrentDescription()
        {
            return descriptions[rank];
        }

        public string NextDescription()
        {
            return descriptions[rank + 1];
        }

        public float CurrentEffect()
        {
            return effects[rank];
        }

        public float GetSpecificEffect(int targetRank)
        {
            return effects[targetRank];
        }

        public int CurrentRank()
        {
            return rank;
        }

        public void SetRank(int newRank)
        {
            rank = newRank;
        }

        public void IncreaseRank()
        {
            rank++;
        }

        public void BecomeAvailable(string message="Default")
        {
            if (skillLocked)
            {
                if (message == "Default")
                {
                    message = "Skill \"" + name + "\" criteria met! Available for unlock.";
                }
                skillLocked = false;
                TextNotificationsManager.instance.NewTextNotifaction(message, false);
            }
        }
    }
}
