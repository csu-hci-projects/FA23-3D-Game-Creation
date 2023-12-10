using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScript : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("GoalKey"))
        {
            if (!other.GetComponent<OVRGrabbable>().isGrabbed)
            {
                other.GetComponent<Rigidbody>().useGravity = false;
                other.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("GoalKey"))
        {
            if (!other.GetComponent<OVRGrabbable>().isGrabbed)
            {
                other.GetComponent<Rigidbody>().useGravity = false;
                other.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            }
            else
            {
                other.GetComponent<Rigidbody>().useGravity = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GoalKey"))
        {
            other.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}   
