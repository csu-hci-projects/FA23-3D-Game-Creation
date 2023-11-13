using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementChecker : MonoBehaviour
{
    public float LastMovement { get; private set; }
    private Vector3 lastPosition;
    void Start()
    {
        lastPosition = this.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        LastMovement = (lastPosition - this.gameObject.transform.position).magnitude;
        lastPosition = this.gameObject.transform.position;
    }
}
