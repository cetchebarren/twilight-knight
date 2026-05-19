using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace etchebarren
{
    public class ControlsNotice : MonoBehaviour
    {
        [Header("Script References")]
        public InputHandler inputHandler;

        [Header("Object References")]
        public GameObject noticePointer;
        public GameObject lockOnNotice;
        public GameObject changeTargetNotice;
        public GameObject lightAttackNotice;
        public GameObject heavyAttackNotice;
        public GameObject blockNotice;
        public GameObject dodgeNotice;
        public GameObject jumpNotice;
        public GameObject sheatheNotice;
        public GameObject whistleNotice;
        public GameObject sprintNotice;

        [Header("Flags, set by triggers")]
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

        [Header("Temp Referencesa and Flags")]
        [SerializeField] private ControlsNoticeTrigger currentNotice;
        public enum DisableInput { None, LockOn, Sheathe, Whistle, Interact }
        private Coroutine disableCoroutine;
        [SerializeField] private bool paused;

        public void DisplayControlsNotice(ControlsNoticeTrigger controlsNoticeTrigger)
        {
            currentNotice = controlsNoticeTrigger;
            DisableInput disableInput = currentNotice.disableInput;
            float duration = currentNotice.duration;
            bool pause = currentNotice.pause;

            if (disableInput != DisableInput.None)
            {
                noticePointer.SetActive(true);
            }

            if (pause)
            {
                inputHandler.pausedForNotice = true;
                Time.timeScale = 0f;
            }

            Display();

            if (disableCoroutine != null)
            {
                StopCoroutine(disableCoroutine);
                disableCoroutine = null;
            }
            disableCoroutine = StartCoroutine(DisableNotice(duration, disableInput));
        }

        private IEnumerator DisableNotice(float duration, DisableInput disableType)
        {
            bool inputReceived = false;

            // Attach listener
            switch (disableType)
            {
                case DisableInput.LockOn:
                    inputHandler.inputActions.PlayerCamera.LockOn.performed += LockOnCancelNotice;
                    break;
                case DisableInput.Sheathe:
                    inputHandler.inputActions.PlayerActions.SheatheWeapon.performed += SheatheCancelNotice;
                    break;
                case DisableInput.Whistle:
                    inputHandler.inputActions.PlayerActions.CallHorse.performed += WhistleCancelNotice;
                    break;
                case DisableInput.Interact:
                    inputHandler.inputActions.PlayerActions.Interact.performed += InteractCancelNotice;
                    break;
                default:
                    break;
            }

            // Wait for duration
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // Wait until not paused
                while (paused)
                    yield return null;

                // Increment elapsed time by unscaled time
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Only reached if HandleInput wasn't already called via input listeners created above
            // because in that case, this coroutine was stopped already
            HandleInput(disableType);
            disableCoroutine = null;
        }

        public void SetPause(bool paused)
        {
            if (disableCoroutine == null) return;
            this.paused = paused;
            if (paused)
            {
                Hide(true);
            }
            else
            {
                Display();
            }
        }

        // Directly defined methods
        private void LockOnCancelNotice(InputAction.CallbackContext context)
        {
            HandleInput(DisableInput.LockOn);
        }

        private void SheatheCancelNotice(InputAction.CallbackContext context)
        {
            HandleInput(DisableInput.Sheathe);
        }

        private void WhistleCancelNotice(InputAction.CallbackContext context)
        {
            HandleInput(DisableInput.Whistle);
        }

        private void InteractCancelNotice(InputAction.CallbackContext context)
        {
            HandleInput(DisableInput.Interact);
        }

        private void HandleInput(DisableInput disableType)
        {
            // Stop notice timer coroutine
            if (disableCoroutine != null)
            {
                StopCoroutine(disableCoroutine);
                disableCoroutine = null;
            }

            // Remove Listeners
            switch (disableType)
            {
                case DisableInput.LockOn:
                    inputHandler.inputActions.PlayerCamera.LockOn.performed -= LockOnCancelNotice;
                    break;
                case DisableInput.Sheathe:
                    inputHandler.inputActions.PlayerActions.SheatheWeapon.performed -= SheatheCancelNotice;
                    break;
                case DisableInput.Whistle:
                    inputHandler.inputActions.PlayerActions.CallHorse.performed -= WhistleCancelNotice;
                    break;
                case DisableInput.Interact:
                    inputHandler.inputActions.PlayerActions.Interact.performed -= InteractCancelNotice;              
                    break;
                default:
                    break;
            }

            Hide();

            if (inputHandler.pausedForNotice)
            {
                // Unpause
                Time.timeScale = 1f;
                // Reset inputs to prevent input from carrying over, such as pausing during notice leading to menu opening after notice disables
                inputHandler.ResetInputs(false, false);
                // Update flag for input handler to allow input
                inputHandler.pausedForNotice = false;
                // Since inputs were reset, need to force input so that action carries over
                switch (disableType)
                {
                    case DisableInput.LockOn:
                        inputHandler.lockOnInput = true;
                        break;
                    case DisableInput.Sheathe:
                        inputHandler.sheatheWeaponInput = true;
                        break;
                    case DisableInput.Whistle:
                        inputHandler.callHorseInput = true;
                        break;
                    case DisableInput.Interact:
                        break;
                    default:
                        break;
                }

            }

            noticePointer.SetActive(false);
        }

        public void Display()
        {
            bool controller = ControllerUIManager.instance.isUsingController();
            bool playstation = ControllerUIManager.instance.controllerTypePlaystation;

            if (lockOn)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    lockOnNotice.transform.GetChild(0).gameObject.SetActive(true);
                    lockOnNotice.transform.GetChild(1).gameObject.SetActive(false);
                    lockOnNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        lockOnNotice.transform.GetChild(0).gameObject.SetActive(false);
                        lockOnNotice.transform.GetChild(1).gameObject.SetActive(true);
                        lockOnNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        lockOnNotice.transform.GetChild(0).gameObject.SetActive(false);
                        lockOnNotice.transform.GetChild(1).gameObject.SetActive(false);
                        lockOnNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                lockOnNotice.SetActive(true);
            }

            if (changeTarget)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    changeTargetNotice.transform.GetChild(0).gameObject.SetActive(true);
                    changeTargetNotice.transform.GetChild(1).gameObject.SetActive(false);
                    changeTargetNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        changeTargetNotice.transform.GetChild(0).gameObject.SetActive(false);
                        changeTargetNotice.transform.GetChild(1).gameObject.SetActive(true);
                        changeTargetNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        changeTargetNotice.transform.GetChild(0).gameObject.SetActive(false);
                        changeTargetNotice.transform.GetChild(1).gameObject.SetActive(false);
                        changeTargetNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                changeTargetNotice.SetActive(true);
            }

            if (lightAttack)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    lightAttackNotice.transform.GetChild(0).gameObject.SetActive(true);
                    lightAttackNotice.transform.GetChild(1).gameObject.SetActive(false);
                    lightAttackNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        lightAttackNotice.transform.GetChild(0).gameObject.SetActive(false);
                        lightAttackNotice.transform.GetChild(1).gameObject.SetActive(true);
                        lightAttackNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        lightAttackNotice.transform.GetChild(0).gameObject.SetActive(false);
                        lightAttackNotice.transform.GetChild(1).gameObject.SetActive(false);
                        lightAttackNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                lightAttackNotice.SetActive(true);
            }

            if (heavyAttack)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    heavyAttackNotice.transform.GetChild(0).gameObject.SetActive(true);
                    heavyAttackNotice.transform.GetChild(1).gameObject.SetActive(false);
                    heavyAttackNotice.transform.GetChild(2).gameObject.SetActive(false);
                    heavyAttackNotice.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "(Hold) Heavy Attack";
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        heavyAttackNotice.transform.GetChild(0).gameObject.SetActive(false);
                        heavyAttackNotice.transform.GetChild(1).gameObject.SetActive(true);
                        heavyAttackNotice.transform.GetChild(2).gameObject.SetActive(false);
                        heavyAttackNotice.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Heavy Attack";
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        heavyAttackNotice.transform.GetChild(0).gameObject.SetActive(false);
                        heavyAttackNotice.transform.GetChild(1).gameObject.SetActive(false);
                        heavyAttackNotice.transform.GetChild(2).gameObject.SetActive(true);
                        heavyAttackNotice.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Heavy Attack";
                    }
                }
                heavyAttackNotice.SetActive(true);
            }

            if (block)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    blockNotice.transform.GetChild(0).gameObject.SetActive(true);
                    blockNotice.transform.GetChild(1).gameObject.SetActive(false);
                    blockNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        blockNotice.transform.GetChild(0).gameObject.SetActive(false);
                        blockNotice.transform.GetChild(1).gameObject.SetActive(true);
                        blockNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        blockNotice.transform.GetChild(0).gameObject.SetActive(false);
                        blockNotice.transform.GetChild(1).gameObject.SetActive(false);
                        blockNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                blockNotice.SetActive(true);
            }

            if (dodge)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    dodgeNotice.transform.GetChild(0).gameObject.SetActive(true);
                    dodgeNotice.transform.GetChild(1).gameObject.SetActive(false);
                    dodgeNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        dodgeNotice.transform.GetChild(0).gameObject.SetActive(false);
                        dodgeNotice.transform.GetChild(1).gameObject.SetActive(true);
                        dodgeNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        dodgeNotice.transform.GetChild(0).gameObject.SetActive(false);
                        dodgeNotice.transform.GetChild(1).gameObject.SetActive(false);
                        dodgeNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                dodgeNotice.SetActive(true);
            }

            if (jump)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    jumpNotice.transform.GetChild(0).gameObject.SetActive(true);
                    jumpNotice.transform.GetChild(1).gameObject.SetActive(false);
                    jumpNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        jumpNotice.transform.GetChild(0).gameObject.SetActive(false);
                        jumpNotice.transform.GetChild(1).gameObject.SetActive(true);
                        jumpNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        jumpNotice.transform.GetChild(0).gameObject.SetActive(false);
                        jumpNotice.transform.GetChild(1).gameObject.SetActive(false);
                        jumpNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                jumpNotice.SetActive(true);
            }

            if (sheathe)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    sheatheNotice.transform.GetChild(0).gameObject.SetActive(true);
                    sheatheNotice.transform.GetChild(1).gameObject.SetActive(false);
                    sheatheNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        sheatheNotice.transform.GetChild(0).gameObject.SetActive(false);
                        sheatheNotice.transform.GetChild(1).gameObject.SetActive(true);
                        sheatheNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        sheatheNotice.transform.GetChild(0).gameObject.SetActive(false);
                        sheatheNotice.transform.GetChild(1).gameObject.SetActive(false);
                        sheatheNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                sheatheNotice.SetActive(true);
            }

            if (whistle)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    whistleNotice.transform.GetChild(0).gameObject.SetActive(true);
                    whistleNotice.transform.GetChild(1).gameObject.SetActive(false);
                    whistleNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        whistleNotice.transform.GetChild(0).gameObject.SetActive(false);
                        whistleNotice.transform.GetChild(1).gameObject.SetActive(true);
                        whistleNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        whistleNotice.transform.GetChild(0).gameObject.SetActive(false);
                        whistleNotice.transform.GetChild(1).gameObject.SetActive(false);
                        whistleNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                whistleNotice.SetActive(true);
            }

            if (sprint)
            {
                if (!controller)
                {
                    // Case 1: PC Controls
                    sprintNotice.transform.GetChild(0).gameObject.SetActive(true);
                    sprintNotice.transform.GetChild(1).gameObject.SetActive(false);
                    sprintNotice.transform.GetChild(2).gameObject.SetActive(false);
                }
                else
                {
                    if (playstation)
                    {
                        // Case 2: Playstation Controls
                        sprintNotice.transform.GetChild(0).gameObject.SetActive(false);
                        sprintNotice.transform.GetChild(1).gameObject.SetActive(true);
                        sprintNotice.transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else
                    {
                        // Case 3: Xbox Controls
                        sprintNotice.transform.GetChild(0).gameObject.SetActive(false);
                        sprintNotice.transform.GetChild(1).gameObject.SetActive(false);
                        sprintNotice.transform.GetChild(2).gameObject.SetActive(true);
                    }
                }
                sprintNotice.SetActive(true);
            }
        }

        public void Hide(bool pause = false)
        {
            lockOnNotice.SetActive(false);
            changeTargetNotice.SetActive(false);
            lightAttackNotice.SetActive(false);
            heavyAttackNotice.SetActive(false);
            blockNotice.SetActive(false);
            dodgeNotice.SetActive(false);
            jumpNotice.SetActive(false);
            sheatheNotice.SetActive(false);
            whistleNotice.SetActive(false);
            sprintNotice.SetActive(false);

            if(currentNotice != null && !pause)
            {
                if(currentNotice.followUpNotice != null)
                {
                    currentNotice.followUpNotice.Activate();
                }
                else
                {
                    currentNotice = null;
                }
            }
        }
    }
}
