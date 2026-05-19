using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class WhistleIconManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject whistleIcon;
        public GameObject haltIcon;

        public void DetermineAndSetIcon(bool aiMountActive, bool isFollowing)
        {
            // AI is Active
            if(aiMountActive)
            {
                if (isFollowing)
                {
                    SetIconToHalt();
                }
                else
                {
                    SetIconToWhistle();
                }

            }
            // Mount Version Version
            else
            {
                SetIconToWhistle();
            }
        }

        public void SetIconToWhistle()
        {
            // This functiion is also called directly in the Companion Behvaior event for exceeded max distance
            haltIcon.SetActive(false);
            whistleIcon.SetActive(true);
        }

        public void SetIconToHalt()
        {
            whistleIcon.SetActive(false);
            haltIcon.SetActive(true);
        }
    }
}
