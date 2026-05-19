using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace etchebarren
{
    public class EventOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEvent onSelectEvent;
        public UnityEvent onDeselectEvent;

        [Header("Should OnSelect and OnDeselect also be triggered by mouse?")]
        public bool eventOnPointerEnter = false;

        public UnityEvent onPointerEnterEventOnly;
        public UnityEvent onPointerExitEventOnly;

        public void OnSelect(BaseEventData eventData)
        {
            onSelectEvent.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            onDeselectEvent.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventOnPointerEnter) onSelectEvent.Invoke();
            onPointerEnterEventOnly.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventOnPointerEnter) onDeselectEvent.Invoke();
            onPointerExitEventOnly.Invoke();
        }
    }
}
