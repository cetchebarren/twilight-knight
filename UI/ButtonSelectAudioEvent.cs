using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using etchebarren;

public class ButtonSelectAudioEvent : MonoBehaviour, ISelectHandler/*, IPointerEnterHandler*/
{
    public void OnSelect(BaseEventData eventData)
    {
        //Debug.LogError("SELECT AUDIO PLAYED FOR: " + gameObject.name);

        UIAudioManager.instance.PlaySelectButtonAudio();
    }
}
