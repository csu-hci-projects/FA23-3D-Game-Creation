using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    //Sala de onde sais
    public GameObject InitialRoom;

    //Sala para onde teletransportas
    public GameObject DestinationRoom;

    //Direcao do portal (preciso disto porque se uma sala tiver 2 portais, preciso de saber qual sala-filho dou link)
    public RoomDir direction;

    public void SetRooms(GameObject _InitalRoom, GameObject _DestinationRoom)
    {
        InitialRoom = _InitalRoom;
        DestinationRoom = _DestinationRoom;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 change = DestinationRoom.transform.position - InitialRoom.transform.position;
            //Debug.Log(change);
            DestinationRoom.SetActive(true);
            other.GetComponent<TPreference>().player.transform.position += change;
            InitialRoom.SetActive(false);
            //GenerationManager.instance.OnPortalPass(DestinationRoom);
        }
    }
}
