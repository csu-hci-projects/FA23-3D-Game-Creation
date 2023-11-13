using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Varias direções para ligar as salas 
public enum RoomDir
{
    Root = 0,
    North_L = 1, North_R = 2, North_LR = 3,
    South_L = 4, South_R = 5, South_LR = 6,
    East_L = 7, East_R = 8, East_LR = 9,
    West_L = 10, West_R = 11, West_LR = 12,
    South_RL = 13, East_RL = 14, North_RL = 15, West_RL = 16, C =17,
};

//Este script é para dar attach aos prefabs das salas e preencher manualmente
public class RoomDirections : MonoBehaviour
{
    //Onde está o portal na sala
    public List<RoomDir> PortalPositions;

    //Portais da sala
    public List<Teleporter> Portals;

    private void Awake()
    {
        PortalPositions = new List<RoomDir>();
        Portals = new List<Teleporter>();
        foreach (Teleporter tp in GetComponentsInChildren<Teleporter>())
        {
            Portals.Add(tp);
            PortalPositions.Add(tp.direction);
        }
    }

    public void getValues()
    {
        PortalPositions.Clear();
        Portals.Clear();
        foreach (Teleporter tp in GetComponentsInChildren<Teleporter>())
        {
            Portals.Add(tp);
            PortalPositions.Add(tp.direction);
        }
    }


}
