using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedirectedWalking : MonoBehaviour
{
    //https://ieeexplore.ieee.org/abstract/document/5072212

    //translation -14% a +26%

    public Vector3 translationGain = new Vector3(1,1,1);

    //rotation -20% a +49%
    [Range(0.80f, 1.49f)]
    public float rotationGain;

    

    //public OVRCameraRig camara;
    
    public GameObject player;
    public float stoppedSpeed = 0.1f;
    public Vector3 oldPos;
    public Vector3 offset;
    bool translating;
    private float defaultSpeed;
    // Start is called before the first frame update
    void Start()
    {
        //camara = this.gameObject.GetComponent<OVRCameraRig>();
        //ver se isto nao rebenta porque nao especifico o child, mas é o OVRCameraRig
        //alternativamente passar esta classe para a rig e os getcomponent meter getcomponentinparent
        oldPos = transform.TransformPoint(transform.position);
    }
    private void Update()
    {
        offset = transform.TransformPoint(transform.position) - oldPos;
        if (translating)
        {
            offset.x *= translationGain.x;
            offset.y = 0;
            offset.z *= translationGain.z;
            player.transform.position += offset;
        }
        oldPos = transform.TransformPoint(transform.position);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Translation"))
        {
            translating = true;
            translationGain = other.gameObject.GetComponent<RD_VARs>().TranslationGain; ;
        } 
       
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Translation"))
        {
            translating = false;
        }
       
    }

}
