using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomList
{
    public bool IceRooms;
    public RoomType roomType;
    public RoomDir roomDirection;
    public List<GameObject> rooms;
}
