using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rot_test : MonoBehaviour
{
    public OVRCameraRig camRig;


    private void Update()
    {
        transform.rotation = camRig.centerEyeAnchor.rotation;
    }
}
