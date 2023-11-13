using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeAnimation : MonoBehaviour
{
    bool up = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y > 0.5f)
        {
            up = false;
        }
        else if (gameObject.transform.position.y < 0.1f)
        {
            up = true;
        }
        if (up)
        {
            gameObject.transform.position += new Vector3(0, 0.003f, 0);
        }
        else
        {
            gameObject.transform.position -= new Vector3(0, 0.003f, 0);
        }
    }
}
