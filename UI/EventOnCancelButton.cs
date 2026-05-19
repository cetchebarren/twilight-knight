using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace etchebarren
{
    public class EventOnCancelButton : MonoBehaviour
    {
        public UnityEvent eventOnCancelButton;
        private InputAction cancelAction;

        // When this gameObject is active, the cancel button (via EventSystem) will trigger this event

        private void OnEnable()
        {
            var uiModule = EventSystem.current?.currentInputModule as InputSystemUIInputModule;
            if (uiModule != null)
            {
                cancelAction = uiModule.cancel; // grab the bound Cancel action
                cancelAction.performed += OnCancelPerformed;
                cancelAction.Enable();
            }
            
        }

        private void OnDisable()
        {
            if (cancelAction != null)
            {
                cancelAction.performed -= OnCancelPerformed;
                cancelAction.Disable();
            }
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            eventOnCancelButton.Invoke();
        }
    }
}
