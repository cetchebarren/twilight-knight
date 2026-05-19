using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class DamageStatusTexts : MonoBehaviour
    {
        public static DamageStatusTexts instance;

        public GameObject damageNumberPrefab;
        public int poolSize = 10;
        private int callCount = 0;

        [Header("Position Settings")]
        public float leftAndRightRadius = 0.265f;
        public float upRadius = 0.15f;
        public float downRadius = -0.1f;
        public float upRadiusReaction = 0.5f;
        public float downRadiusReaction = 0.3f;

        [Header("Text Color Settings")]
        public TMP_ColorGradient physical;
        public TMP_ColorGradient fire;
        public TMP_ColorGradient ice;
        public TMP_ColorGradient shock;
        public TMP_ColorGradient arcane;
        public TMP_ColorGradient poison;
        public TMP_ColorGradient plasmatize;
        public TMP_ColorGradient thawed;
        public TMP_ColorGradient polarize;
        public TMP_ColorGradient immune;

        [Header("Animation Settings")]
        public float popupScale = 3.5f;
        public float shrinkSpeed = 15f;
        public float floatUpDistance = 0.5f;
        public float floatDelay = 0.4f;
        public float textLifetime = 1.0f;
        public float fadeDuration = 0.2f;

        [Header("Font Size Settings")]
        public float defaultSize = 4f;
        public float criticalSizeMultiplier = 1.3f;

        [SerializeField] private Queue<GameObject> damageNumberPool;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            damageNumberPool = new Queue<GameObject>();
            for (int i = 0; i < poolSize; i++)
            {
                GameObject damageNumber = Instantiate(damageNumberPrefab, transform);
                damageNumber.SetActive(false);
                damageNumberPool.Enqueue(damageNumber);
            }
        }

        public void ShowDamageNumber(Vector3 position, string text, bool criticalHit, string damageType, float sizeMultiplier=1f)
        {
            //Debug.Log("Damage Type: " + damageType);

            if (damageNumberPool.Count == 0)
            {
                GameObject newDamageNumber = Instantiate(damageNumberPrefab, transform);
                newDamageNumber.SetActive(false);
                damageNumberPool.Enqueue(newDamageNumber);
            }

            GameObject damageNumber = damageNumberPool.Dequeue();

            // Reset color/alpha 
            damageNumber.GetComponent<TextMeshPro>().color = Color.white;

            // Random Position offset
            float xOffset = Random.Range(-leftAndRightRadius, leftAndRightRadius);
            float yOffset = Random.Range(downRadius, upRadius);

            // Set Color
            switch (damageType)
            {
                case "Fire":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = fire;
                    break;
                case "Ice":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = ice;
                    break;
                case "Shock":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = shock;
                    break;
                case "Arcane":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = arcane;
                    break;
                case "Poison":
                case "Nature":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = poison;
                    break;
                case "Plasmatize":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = plasmatize;
                    yOffset = Random.Range(downRadiusReaction, upRadiusReaction);
                    break;
                case "Thawed":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = thawed;
                    yOffset = Random.Range(downRadiusReaction, upRadiusReaction);
                    break;
                case "Polarize":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = polarize;
                    yOffset = Random.Range(downRadiusReaction, upRadiusReaction);
                    break;
                case "Immune":
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = immune;
                    break;
                default:
                    damageNumber.GetComponent<TextMeshPro>().colorGradientPreset = physical;
                    break;
            }


            Vector3 randomPosition = position + new Vector3(xOffset, yOffset, 0f);
            damageNumber.transform.position = randomPosition;

            // Add animations or effects here.
            if (criticalHit)
            {
                damageNumber.transform.localScale = Vector3.one * criticalSizeMultiplier;
                damageNumber.GetComponent<TextMeshPro>().fontStyle = FontStyles.Bold | FontStyles.Italic;
            }
            else
            {
                damageNumber.transform.localScale = Vector3.one;
                damageNumber.GetComponent<TextMeshPro>().fontStyle = FontStyles.Bold; // FontStyles.Normal;
            }
            damageNumber.transform.localScale *= Random.Range(0.95f, 1.05f);

            // Set Text & Activate
            damageNumber.GetComponent<TextMeshPro>().text = text;

            // Handle font size
            damageNumber.GetComponent<TextMeshPro>().fontSize = defaultSize * sizeMultiplier;

            // Set the initial scale
            damageNumber.transform.localScale = damageNumber.transform.localScale * popupScale;

            damageNumber.SetActive(true);

            // Animate the scale back to normal size
            StartCoroutine(ShrinkDamageNumber(damageNumber, popupScale, shrinkSpeed));

            // Animate the text to float upwards
            StartCoroutine(FloatUp(damageNumber, floatDelay, floatUpDistance));

            // Deactivate the object after a delay
            StartCoroutine(DeactivateAfterDelay(damageNumber));
        }

        private IEnumerator ShrinkDamageNumber(GameObject damageNumber, float initialScale, float speed)
        {
            float t = 0f;
            Vector3 startScale = damageNumber.transform.localScale;
            Vector3 targetScale = damageNumber.transform.localScale / initialScale; // Scale back to normal size

            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                damageNumber.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
        }

        private IEnumerator FloatUp(GameObject damageNumber, float delay, float floatUpDistance)
        {
            // Delay before starting the animation
            yield return new WaitForSeconds(delay);

            float t = 0f;

            // Get the initial position of the damage number
            Vector3 initialPosition = damageNumber.transform.position;

            // Calculate the target position by moving it upwards
            Vector3 targetPosition = initialPosition + Vector3.up * floatUpDistance;

            // Gradually move the damage number upwards
            while (t < 1f)
            {
                t += Time.deltaTime;

                // Interpolate between the initial and target positions
                damageNumber.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

                yield return null;
            }
        }


        private IEnumerator DeactivateAfterDelay(GameObject damageNumber)
        {
            TextMeshPro textMesh = damageNumber.GetComponent<TextMeshPro>();

            float elapsedTime = 0f;
            Color startColor = textMesh.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // Fades to transparent

            // Fade in
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                textMesh.color = Color.Lerp(targetColor, startColor, t); // Fades in over fadeDuration
                yield return null;
            }

            // Delay before textLifetime (text remains fully visible)
            yield return new WaitForSeconds(textLifetime); // Text remains fully visible for textLifetime

            elapsedTime = 0f; // Reset elapsed time
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                textMesh.color = Color.Lerp(startColor, targetColor, t); // Fades out over fadeDuration
                yield return null;
            }

            textMesh.color = targetColor; // Ensure it's fully transparent
            damageNumber.SetActive(false);
            damageNumberPool.Enqueue(damageNumber);
        }



    }
}
