using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gainStart : MonoBehaviour
{
    public float gain = 0.5f    ;
    public VRTK.VRTK_StepMultiplier step;
    private void Start()
    {
        step =GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<StepReference>().step;
        step.additionalMovementMultiplier = 0f;
        step.enabled = false;
    }
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            step = other.GetComponent<StepReference>().step;
            step.enabled = true;
            step.additionalMovementMultiplier = 0.5f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            step = other.GetComponent<StepReference>().step;
            step.enabled = true;
            step.additionalMovementMultiplier = 0.5f;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            
            step.additionalMovementMultiplier = 0f;
            step.enabled = false;
        }
    }
}
