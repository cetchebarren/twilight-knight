using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Reselector : MonoBehaviour
{
    [Header("Main Unity Event - Reselct Button")]
    public UnityEvent reselectEvent;

    [Header("Optional - Button to Click")]
    public Button buttonToClick;


    private void OnEnable()
    {
        // Subscribe to the Input System's Submit action
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        // Check if the action was performed (like "Submit" was pressed)
        if (obj is InputAction action && change == InputActionChange.ActionPerformed)
        {
            //Debug.Log("Reselect Checking...");

            if (action.name == "Submit") // Only run if we're not ignoring Submit
            {
                // Check if there is no currently selected UI element
                if (EventSystem.current.currentSelectedGameObject == null)
                {

                    Debug.Log("Reselect Invoked");

                    // Temporarily ignore Submit input to avoid pressing the button immediately
                    StartCoroutine(DisableSubmitForFrame());
                }
            }
        }
    }

    private IEnumerator DisableSubmitForFrame()
    {
        if (buttonToClick != null)
        {
            buttonToClick.onClick.Invoke();
        }

        yield return null; // Wait one frame

        // Trigger the reselection event
        reselectEvent.Invoke();
    }
}
