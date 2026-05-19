using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace etchebarren
{
    public class ReselectButtonOnInput : MonoBehaviour
    {
        public UnityEvent optionalEvent;
        private InputAction inputAction;
        public GameObject[] buttonsInOrderOfPriority;
        public bool enableOnStart = false;

        // When this gameObject is active, the cancel button (via EventSystem) will trigger this event

        void Start()
        {
            if (enableOnStart)
            {
                StartCoroutine(DelayFrameEnable());
            }
        }

        private void OnEnable()
        {
            EnableEvent();
        }

        private IEnumerator DelayFrameEnable()
        {
            yield return null;
            EnableEvent();
        }

        private void EnableEvent()
        {
            var uiModule = EventSystem.current?.currentInputModule as InputSystemUIInputModule;
            if (uiModule != null)
            {
                inputAction = uiModule.submit; // grab the bound Cancel action
                inputAction.performed += OnPerformed;
                inputAction.Enable();
            }
        }

        private void OnDisable()
        {
            if (inputAction != null)
            {
                inputAction.performed -= OnPerformed;
                inputAction.Disable();
            }
        }

        private void OnPerformed(InputAction.CallbackContext ctx)
        {
            StartCoroutine(DelayFrameSelect());
        }

        private IEnumerator DelayFrameSelect()
        {
            yield return null;
            CheckForNoButton();
        }

        private void CheckForNoButton()
        {
            Debug.Log("Check For No Button triggered in ReselectButtonOnInput.c via " + this.gameObject.name);
            if (!EventSystem.current.alreadySelecting)
            {
                if(EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
                {
                    foreach(GameObject button in buttonsInOrderOfPriority)
                    {
                        if (button.activeInHierarchy)
                        {
                            EventSystem.current.SetSelectedGameObject(button);
                            optionalEvent.Invoke();
                        }
                    }             
                }
            }          
        }
    }
}
