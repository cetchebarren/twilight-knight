using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

namespace etchebarren
{
    /* Either triggered via mouse AND controller by Event Trigger Select -> set currentHoldButton in LevelUpMenuManager.cs, 
     * followed by input event listeners that call Start/StopHolding based on the current Hold Button*/

    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent holdEvent;
        [SerializeField] bool beingHeld = false;
        [SerializeField] public float timer = 0f;
        private float interval = 0.2f;

        void Update()
        {
            if (beingHeld)
            {
                if(timer < interval)
                {
                    timer += Time.unscaledDeltaTime;
                }
                else
                {
                    timer = 0f;
                    holdEvent.Invoke();
                }
            }
        }

        void OnDisable()
        {
            StopHolding();
        }

        // Called when the controller button is pressed down (or mouse click/touch)
        public void OnPointerDown(PointerEventData eventData)
        {
            StartHolding();
        }

        // Called when the controller button is released (or mouse/touch is lifted)
        public void OnPointerUp(PointerEventData eventData)
        {
            StopHolding();
        }

        public void StartHolding()
        {
            beingHeld = true;
            holdEvent.Invoke();
            timer = 0f;
            Debug.Log("Start Holding triggered");
        }

        public void StopHolding()
        {
            beingHeld = false;
            timer = 0f;
        }
    }
}
