using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CutsceneAnimationEvents : MonoBehaviour
{
    [Header("Crouch Stab Blood Decals")]
    public GameObject[] bloodDecals;
    public int bloodDecalCounter = 0;

    [Header("Crouch Stab Audio Clips")]
    public AudioClip[] stabClips;
    public AudioSource audioSource;
    public float stabVolume = 0.5f;

    public UnityEvent eventOnstab;

    void OnDisable()
    {
        bloodDecalCounter = 0;
    }

    public void CrouchStab()
    {
        bool countUpdated = false;
        if(bloodDecals.Length > 0 
            && bloodDecalCounter < bloodDecals.Length
            && bloodDecals[bloodDecalCounter] != null)
        {
            bloodDecals[bloodDecalCounter % bloodDecals.Length].SetActive(true);
            bloodDecalCounter++;
            countUpdated = true;
            
        }

        if (audioSource != null && stabClips.Length > 0)
        {
            audioSource.PlayOneShot(stabClips[bloodDecalCounter % stabClips.Length], stabVolume);
            if (!countUpdated) bloodDecalCounter++;
        }

        eventOnstab.Invoke();
    }
}
