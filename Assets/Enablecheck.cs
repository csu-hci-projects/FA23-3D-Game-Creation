using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enablecheck : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("GoalKey"))
        {
            if (obj.GetComponent<OVRGrabbable>().isGrabbed)
            {
                obj.SetActive(true);
            }
        }
        
    }
}
