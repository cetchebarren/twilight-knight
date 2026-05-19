using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpinManager : MonoBehaviour
{
    private bool buttonPressed;
    private int direction; //pos left, neg right

    public Image icon_imageL, icon_imageC, icon_imageR, icon_imageZ;

    public GameObject character;
    private Quaternion initialRotation;

    public void Start()
    {
        initialRotation = character.transform.localRotation;
    }

    public void Update()
    {
        if(buttonPressed)
        {
            character.transform.Rotate(0, 0.45f * direction, 0);
        }
    }

    public void Recenter()
    {
        character.transform.localRotation = initialRotation;
    }

    public void OnPress(string dir)
    {
        if(dir == "left")
        {
            direction = 1;
            icon_imageL.color = Color.green;
            buttonPressed = true;
        } 
        else if (dir == "right")
        {
            direction = -1;
            icon_imageR.color = Color.green;
            buttonPressed = true;
        } 
        else if (dir == "center")
        {
            icon_imageC.color = Color.green;
        } 
        else if (dir == "zoom")
        {
            icon_imageZ.color = Color.green;
        }
    }

    public void OnRelease(string dir)
    {
        if (dir == "left")
        {
            icon_imageL.color = Color.white;
            buttonPressed = false;
        } 
        else if (dir == "right")
        {
            icon_imageR.color = Color.white;
            buttonPressed = false;
        } 
        else if (dir == "center")
        {
            icon_imageC.color = Color.white;
            buttonPressed = false;
        }
        else if (dir == "zoom")
        {
            icon_imageZ.color = Color.white;
        }

    }

    public void OnEnter(string dir)
    {
        if (dir == "left")
        {
            icon_imageL.color = Color.yellow;
        }
        else if (dir == "right")
        {
            icon_imageR.color = Color.yellow;
        }
        else if (dir == "center")
        {
            icon_imageC.color = Color.yellow;
        }
        else if (dir == "zoom")
        {
            icon_imageZ.color = Color.yellow;
        }
    }

    public void OnExit(string dir)
    {
        if (dir == "left")
        {
            icon_imageL.color = Color.white;
        }
        else if (dir == "right")
        {
            icon_imageR.color = Color.white;
        }
        else if (dir == "center")
        {
            icon_imageC.color = Color.white;
        }
        else if (dir == "zoom")
        {
            icon_imageZ.color = Color.white;
        }
    }

    public void OnSelect(string dir)
    {
        if (dir == "left")
        {
            icon_imageL.color = Color.yellow;
        }
        else if (dir == "right")
        {
            icon_imageR.color = Color.yellow;
        }
        else if (dir == "center")
        {
            icon_imageC.color = Color.yellow;
        }
        else if (dir == "zoom")
        {
            icon_imageZ.color = Color.yellow;
        }
    }

    public void DeSelect(string dir)
    {
        if (dir == "left")
        {
            icon_imageL.color = Color.white;
        }
        else if (dir == "right")
        {
            icon_imageR.color = Color.white;
        }
        else if (dir == "center")
        {
            icon_imageC.color = Color.white;
        }
        else if (dir == "zoom")
        {
            icon_imageZ.color = Color.white;
        }
    }
}