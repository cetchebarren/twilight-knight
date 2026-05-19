using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectEyeColor : MonoBehaviour
{
    public Texture yellow, green, blue, brown, gray, red;
    private Texture colorToSet;

    public SkinnedMeshRenderer eyeLocation;
    public SkinnedMeshRenderer eyeLocation_M;

    public string currentEyeColor = "yellow";

    public void SetEyeColor(string color)
    {
        currentEyeColor = color;

        //DETERMINE COLOR
        if (color == "yellow")
        {
            colorToSet = yellow;
        }
        else if (color == "green")
        {
            colorToSet = green;
        }
        else if (color == "blue")
        {
            colorToSet = blue;
        }
        else if (color == "brown")
        {
            colorToSet = brown;
        }
        else if (color == "gray")
        {
            colorToSet = gray;
        }
        else if (color == "red")
        {
            colorToSet = red;
        }

        //SET COLOR
        if (colorToSet != null)
        {
           eyeLocation.materials[3].SetTexture("_MainTex", colorToSet);
           eyeLocation_M.materials[2].SetTexture("_MainTex", colorToSet);
        }

    }
}
