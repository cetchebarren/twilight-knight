using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class ControlsNoticeTrigger : MonoBehaviour
    {
        [Header("Inputs to display")]
        public bool lockOn;
        public bool changeTarget;
        public bool lightAttack;
        public bool heavyAttack;
        public bool block;
        public bool dodge;
        public bool jump;
        public bool sheathe;
        public bool whistle;
        public bool sprint;

        [Header("Notice duration/timeout")]
        public float duration;

        [Header("Pause game for notice")]
        public bool pause = false;

        [Header("Disable Action")]
        public ControlsNotice.DisableInput disableInput;

        [Header("Optional: follow up notice")]
        public ControlsNoticeTrigger followUpNotice;

        [Header("Flags, Do not set manually")]
        public bool alreadyTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                // If already unsheathed, skip notice
                if(disableInput == ControlsNotice.DisableInput.Sheathe)
                {
                    if (PlayerLocomotion.instance.inCombat) return;
                }

                Activate();
            }
        }

        public void Activate()
        {
            if (!alreadyTriggered)
            {
                alreadyTriggered = true;
                PlayerMenuManager.instance.controlsNotice.lockOn = lockOn;
                PlayerMenuManager.instance.controlsNotice.changeTarget = changeTarget;
                PlayerMenuManager.instance.controlsNotice.lightAttack = lightAttack;
                PlayerMenuManager.instance.controlsNotice.heavyAttack = heavyAttack;
                PlayerMenuManager.instance.controlsNotice.block = block;
                PlayerMenuManager.instance.controlsNotice.dodge = dodge;
                PlayerMenuManager.instance.controlsNotice.jump = jump;
                PlayerMenuManager.instance.controlsNotice.sheathe = sheathe;
                PlayerMenuManager.instance.controlsNotice.whistle = whistle;
                PlayerMenuManager.instance.controlsNotice.sprint = sprint;

                PlayerMenuManager.instance.controlsNotice.DisplayControlsNotice(this);
            }
        }
    }
}
