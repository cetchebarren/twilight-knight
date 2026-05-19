using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnTriggerCollider : MonoBehaviour
{
    public string tag;
    public string tag2 = "Unset";

    public UnityEvent onEnter;
    public UnityEvent onExit;

    public bool alreadyTriggered = false;
    public bool repeatable = false;

    public bool hideMeshOnStart = true;

    public bool debugPrintTriggers = false;

    void Start()
    {
        if (hideMeshOnStart)
            GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.tag == tag || other.tag == tag2) && (!alreadyTriggered || repeatable))
        {
            alreadyTriggered = true;

            if (debugPrintTriggers) Debug.Log("EVENT ON TRIGGER w/ " + other.name);

            onEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.tag == tag || other.tag == tag2) && (alreadyTriggered || repeatable))
        {
            alreadyTriggered = false;

            onExit.Invoke();

        }
    }
}
