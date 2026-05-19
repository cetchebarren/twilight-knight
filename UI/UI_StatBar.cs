using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class UI_StatBar : MonoBehaviour
    {
        public Slider slider;

        [Header("Transition Slider Settings")]
        public Slider transitionSlider;
        public float transitionSpeed = 1.5f;
        public float delayBeforeTransition = 2.0f;
        private Coroutine transitionCoroutine;
        //to do: variable to scale bar depending on stat changes
        //to do: secondary bar to show how much hp/stam, etc is lost from action or attack
        [Header("For Scaling Status Bars (Player Only)")]
        public GameObject background;
        public GameObject fillArea;
        public GameObject transitionFillArea;

        public void SetStat(float newValue, bool status=false, bool transition=true)
        {
            if (status && transitionCoroutine != null)
            {
                //Debug.Log("Not removed, effect already in place");
                return;
            }
            if(transition && transitionSlider != null && slider.value > newValue)
            {
                TriggerTransitionSlider(slider.value, newValue);
            }
            else if (!transition)
            {
                transitionSlider.value = slider.value;
            }

            slider.value = newValue;        
        }

        public void SetMaxStat(float maxValue, bool setValueToMax= true)
        {
            slider.maxValue = maxValue;
            
            if (setValueToMax)
                slider.value = maxValue;

            if (transitionSlider != null)
                transitionSlider.maxValue = slider.maxValue;
        }

        public void TriggerTransitionSlider(float currentValue, float targetValue)
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine); // Stop any ongoing transition

            // Start the new coroutine for smooth transition
            if (gameObject.activeInHierarchy)
            {
                transitionCoroutine = StartCoroutine(TransitionSlider(currentValue, targetValue));
            }
            else
            {
                transitionSlider.value = 0f;
            }
        }

        private IEnumerator TransitionSlider(float currentValue, float targetValue)
        {
            transitionSlider.value = currentValue;
            yield return new WaitForSeconds(delayBeforeTransition);

            while (currentValue != targetValue)
            {
                currentValue = Mathf.MoveTowards(currentValue, targetValue, transitionSpeed * Time.deltaTime);
                transitionSlider.value = currentValue;

                yield return null;
            }

            // Ensure that the transition is complete by setting current to target
            currentValue = targetValue;
            transitionCoroutine = null; // Reset the coroutine reference
        }

        public void ScaleBar(float newScaleX)
        {
            background.transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);

            float scaleFactor = 1.0f;

            float scaleIncreaseRate = 0.0169f;  // Customize this value for the desired increase rate

            scaleFactor = 1.0f + (newScaleX - 1.0f) * scaleIncreaseRate;

            Vector3 newScale = background.transform.localScale * scaleFactor;
            fillArea.transform.localScale = newScale;
            transitionFillArea.transform.localScale = fillArea.transform.localScale;
        }

    }
}
