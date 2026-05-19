using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectHairColor : MonoBehaviour
{
    //HAIR
    //#CFAB8E = rgb(207, 171, 142)

    public float redAmount = 0.8117f;
    public float greenAmount = 0.6705f;
    public float blueAmount = 0.5568f;

    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    public Color currentHairColor;

    //EYEBROWS
    //#383F46 = rgb(56, 63, 70)
    public float redAmount2 = 0.8117f;
    public float greenAmount2 = 0.6705f;
    public float blueAmount2 = 0.5568f;

    public Slider redSlider2;
    public Slider greenSlider2;
    public Slider blueSlider2;

    public Color currentEyebrowColor;

    //FACIAL HAIR
    //#927563 = rgb(146, 117, 99)
    public float redAmount3 = 0.5725f;
    public float greenAmount3 = 0.4588f;
    public float blueAmount3 = 0.3882f;

    public Slider redSlider3;
    public Slider greenSlider3;
    public Slider blueSlider3;

    public Color currentFacialHairColor;

    [Header("Venus")]
    //get material from skm for venus, and change color properties of material
    public List<SkinnedMeshRenderer> rendererList = new List<SkinnedMeshRenderer>();
    public List<MeshRenderer> rendererList2 = new List<MeshRenderer>();

    public SkinnedMeshRenderer eyebrowLocation;

    [Header("Mars")]
    //get material from skm for mars, and change color properties of material
    public List<MeshRenderer> rendererListM = new List<MeshRenderer>();

    public SkinnedMeshRenderer eyebrowLocationM;

    [Header("Mars and Venus")]
    public List<MeshRenderer> facialHairRendererList = new List<MeshRenderer>();

    public void Start()
    {
        Debug.Log("Triggered A - Test");

        //init slider values without triggering on value changed method
        redSlider.SetValueWithoutNotify(redAmount * redSlider.maxValue);
        greenSlider.SetValueWithoutNotify(greenAmount * greenSlider.maxValue);
        blueSlider.SetValueWithoutNotify(blueAmount * blueSlider.maxValue);

        redSlider2.SetValueWithoutNotify(redAmount2 * redSlider2.maxValue);
        greenSlider2.SetValueWithoutNotify(greenAmount2 * greenSlider2.maxValue);
        blueSlider2.SetValueWithoutNotify(blueAmount2 * blueSlider2.maxValue);

        redSlider3.SetValueWithoutNotify(redAmount3 * redSlider3.maxValue);
        greenSlider3.SetValueWithoutNotify(greenAmount3 * greenSlider3.maxValue);
        blueSlider3.SetValueWithoutNotify(blueAmount3 * blueSlider3.maxValue);

        currentHairColor = new Color(redAmount, greenAmount, blueAmount);
        currentEyebrowColor = new Color(redAmount2, greenAmount2, blueAmount2);
        currentFacialHairColor = new Color(redAmount3, greenAmount3, blueAmount3);
    }

    public void UpdateSliders()
    {
        redAmount = redSlider.value / redSlider.maxValue;
        greenAmount = greenSlider.value / greenSlider.maxValue;
        blueAmount = blueSlider.value / blueSlider.maxValue;
        SetHairColor();
    }

    public void SetHairColor() // Used by Slider when customizing character
    {
        currentHairColor = new Color(redAmount, greenAmount, blueAmount);

        //VENUS
        for (int i = 0; i < rendererList.Count; i++)
        {
            rendererList[i].material.SetColor("_Color", currentHairColor);
        }

        for (int i = 0; i < rendererList2.Count; i++)
        {
            rendererList2[i].material.SetColor("_Color", currentHairColor);
        }

        //MARS

        for (int i = 0; i < rendererListM.Count; i++)
        {
            rendererListM[i].material.SetColor("_Color", currentHairColor);
        }
    }

    public void SetHairColor(Color newColor) // Method Overload used in Save Manager
    {
        // Update Slider Values
        redSlider.value = newColor.r * redSlider.maxValue;
        greenSlider.value = newColor.g * greenSlider.maxValue;
        blueSlider.value = newColor.b * blueSlider.maxValue;

        // Set New Colors
        currentHairColor = newColor;
        redAmount = newColor.r;
        greenAmount = newColor.g;
        blueAmount = newColor.b;

        //VENUS
        for (int i = 0; i < rendererList.Count; i++)
        {
            rendererList[i].material.SetColor("_Color", currentHairColor);
        }

        for (int i = 0; i < rendererList2.Count; i++)
        {
            rendererList2[i].material.SetColor("_Color", currentHairColor);
        }

        //MARS

        for (int i = 0; i < rendererListM.Count; i++)
        {
            rendererListM[i].material.SetColor("_Color", currentHairColor);
        }
    }

    public void UpdateSliders2()
    {
        redAmount2 = redSlider2.value / redSlider2.maxValue;
        greenAmount2 = greenSlider2.value / greenSlider2.maxValue;
        blueAmount2 = blueSlider2.value / blueSlider2.maxValue;
        SetEyebrowColor();
    }

    public void SetEyebrowColor() // Used by Slider when customizing character
    {
        currentEyebrowColor = new Color(redAmount2, greenAmount2, blueAmount2);

        eyebrowLocation.materials[1].SetColor("_Color", currentEyebrowColor);
        eyebrowLocationM.materials[4].SetColor("_Color", currentEyebrowColor);
    }

    public void SetEyebrowColor(Color newColor) // Method Overload used in Save Manager
    {
        // Update Slider Values
        redSlider2.value = newColor.r * redSlider2.maxValue;
        greenSlider2.value = newColor.g * greenSlider2.maxValue;
        blueSlider2.value = newColor.b * blueSlider2.maxValue;

        // Set New Colors
        currentEyebrowColor = newColor;
        redAmount2 = newColor.r;
        greenAmount2 = newColor.g;
        blueAmount2 = newColor.b;

        eyebrowLocation.materials[1].SetColor("_Color", currentEyebrowColor);
        eyebrowLocationM.materials[4].SetColor("_Color", currentEyebrowColor);
    }

    public void UpdateSliders3()
    {
        redAmount3 = redSlider3.value / redSlider3.maxValue;
        greenAmount3 = greenSlider3.value / greenSlider3.maxValue;
        blueAmount3 = blueSlider3.value / blueSlider3.maxValue;
        SetFacialHairColor();
    }

    public void SetFacialHairColor() // Used by Slider when customizing character
    {
        currentFacialHairColor = new Color(redAmount3, greenAmount3, blueAmount3);

        for (int i = 0; i < facialHairRendererList.Count; i++)
        {
            facialHairRendererList[i].material.SetColor("_Color", currentFacialHairColor);
        }
    }

    public void SetFacialHairColor(Color newColor) // Method Overload used in Save Manager
    {
        // Update Slider Values
        redSlider3.value = newColor.r * redSlider3.maxValue;
        greenSlider3.value = newColor.g * greenSlider3.maxValue;
        blueSlider3.value = newColor.b * blueSlider3.maxValue;

        // Set New Colors
        currentFacialHairColor = newColor;
        redAmount3 = newColor.r;
        greenAmount3 = newColor.g;
        blueAmount3 = newColor.b;

        for (int i = 0; i < facialHairRendererList.Count; i++)
        {
            facialHairRendererList[i].material.SetColor("_Color", currentFacialHairColor);
        }
    }
}
