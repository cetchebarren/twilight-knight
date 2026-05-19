using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    /// <summary>
    /// used to access HelpMenu functions via singleton instance, these are called via unity event trigger
    /// </summary>
    public class TutorialEventCaller : MonoBehaviour
    {
        public void UnlockTutorial(string header)
        {
            HelpMenu.instance.UnlockTip(header);
        }

        public void TutorialPopup(string header)
        {
            HelpMenu.instance.DisplayInGame(header);
        }

        public void TestPopup(string header)
        {
            HelpMenu.instance.DisplayInGame(header, true);
        }

        public void AddTutorialToDisplayQueue(string header)
        {
            HelpMenu.instance.AddHeaderToQueue(header);
        }
    }
}
