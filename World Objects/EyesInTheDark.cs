using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyesInTheDark : MonoBehaviour
{
    [Header("Fade In Settings")]
    public float fadeInTime = 2f; // Duration of scaling fade-in
    public float initialYScale = 0f; // Minimum Y scale (eyes closed)
    public float targetYScale = 0.2f; // Maximum Y scale (eyes open)
    public bool startOnEnable = true;

    [Header("Blink Settings")]
    public bool blink = true;
    public float runtime = 30f; // Total duration for blinking
    public Vector2 randomBlinkIntervalRange = new Vector2(3f, 6f); // Random interval between blinks
    public float blinkSpeed = 0.3f; // Speed of each blink

    [Header("Look At Target Settings")]
    public bool lookAtCamera = true;
    private Transform lookAtTarget;
    public Transform overrideLookAtTarget;

    [Header("Fade Out On Player Near")]
    public bool fadeOutWhenPlayerIsNear = true;
    private bool alreadyTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!alreadyTriggered)
            {
                alreadyTriggered = true;
                StopAllCoroutines();
                StartCoroutine(ScaleYCoroutine(initialYScale, fadeInTime)); // Close eyes
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (alreadyTriggered)
            {
                alreadyTriggered = false;
                StopAllCoroutines();
                StartCoroutine(FadeInAndStartBlinking());
            }
        }
    }

    private void OnEnable()
    {
        transform.localScale = new Vector3(transform.localScale.x, initialYScale, transform.localScale.z);

        if (overrideLookAtTarget != null)
        {
            lookAtTarget = overrideLookAtTarget;
        }
        else
        {
            lookAtTarget = Camera.main.transform;
        }
        
        if (startOnEnable) StartCoroutine(FadeInAndStartBlinking());
    }

    void LateUpdate()
    {
        if (lookAtCamera) transform.LookAt(lookAtTarget);
    }

    private IEnumerator FadeInAndStartBlinking()
    {
        // Fade in
        yield return StartCoroutine(ScaleYCoroutine(targetYScale, fadeInTime));

        // Start blinking after fade-in completes
        if (blink) StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator ScaleYCoroutine(float targetY, float duration)
    {
        float elapsedTime = 0f;
        float startY = transform.localScale.y;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = new Vector3(startScale.x, targetY, startScale.z);

        while (elapsedTime < duration)
        {
            float newY = Mathf.Lerp(startY, targetY, elapsedTime / duration);
            transform.localScale = new Vector3(startScale.x, newY, startScale.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale; // Ensure exact final value
    }

    private IEnumerator BlinkCoroutine()
    {
        float timeElapsed = 0f;

        while (timeElapsed < runtime)
        {
            float nextBlinkTime = Random.Range(randomBlinkIntervalRange.x, randomBlinkIntervalRange.y);
            yield return new WaitForSeconds(nextBlinkTime);

            // Blink (close and open eyes)
            yield return StartCoroutine(ScaleYCoroutine(initialYScale, blinkSpeed)); // Close eyes
            yield return StartCoroutine(ScaleYCoroutine(targetYScale, blinkSpeed)); // Open eyes

            timeElapsed += nextBlinkTime;
        }
    }
}
