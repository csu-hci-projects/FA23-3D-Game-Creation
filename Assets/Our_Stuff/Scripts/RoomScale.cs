using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomScale : MonoBehaviour
{
    public List<GameObject> rooms;
    public List<Vector3> defaultScale;
    public float defualtS = 1f;
    public Vector3 scale = new Vector3(1, 0, 1);

    private void Start()
    {
      
        ScaleRooms(defualtS);
        
    }

    private void Update()
    {
        OVRInput.Update();
        if (OVRInput.Get(OVRInput.RawButton.A))
        {
            ScaleRooms(0.8f);
        }
        else if (OVRInput.Get(OVRInput.RawButton.B))
        {
            ScaleRooms(1.25f);
        }
        else if (OVRInput.Get(OVRInput.RawButton.X))
        {
            SceneManager.LoadScene(0);
        }
        else if (OVRInput.Get(OVRInput.RawButton.Y))
        {
            SceneManager.LoadScene(1);
        }
    }


    private void ScaleRooms(float a)
    {
        foreach (GameObject room in rooms)
        {
            Vector3 nScale = new Vector3(room.transform.localScale.x * a, room.transform.localScale.y * (a), room.transform.localScale.z * a);
            room.transform.localScale = nScale;
        }
    }
}
