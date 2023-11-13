using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFactory : MonoBehaviour
{
    private enum Directions
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }
    private enum Orientation
    {
        Right = 0,
        Left = 1,
        LeftRight = 2,
        RightLeft = 3,
    }

    List<Directions> allDirections = Enum.GetValues(typeof(Directions))
                          .Cast<Directions>()
                          .ToList();

    class RoomDirection
    {
        public Directions dir { get; set; }
        public Orientation ori { get; set; }

        //TO-DO
        //compare method
        public static bool operator ==(RoomDirection dir1, RoomDirection dir2)
        {
            return (dir1.dir == dir2.dir && dir1.ori == dir2.ori);
        }

        public static bool operator !=(RoomDirection dir1, RoomDirection dir2)
        {
            return !(dir1.dir == dir2.dir && dir1.ori == dir2.ori);
        }
    }

    private Dictionary<RoomDir, RoomDirection> DirToDirMap = new Dictionary<RoomDir, RoomDirection>()
    {
        { RoomDir.East_L, new RoomDirection {dir = Directions.East, ori = Orientation.Left } },
        { RoomDir.East_R, new RoomDirection {dir = Directions.East, ori = Orientation.Right } },
        { RoomDir.East_LR, new RoomDirection {dir = Directions.East, ori = Orientation.LeftRight } },
        { RoomDir.East_RL, new RoomDirection {dir = Directions.East, ori = Orientation.RightLeft } },

        { RoomDir.North_L, new RoomDirection {dir = Directions.North, ori = Orientation.Left } },
        { RoomDir.North_R, new RoomDirection {dir = Directions.North, ori = Orientation.Right } },
        { RoomDir.North_LR, new RoomDirection {dir = Directions.North, ori = Orientation.LeftRight } },
        { RoomDir.North_RL, new RoomDirection {dir = Directions.North, ori = Orientation.RightLeft } },

        { RoomDir.South_L, new RoomDirection {dir = Directions.South, ori = Orientation.Left } },
        { RoomDir.South_R, new RoomDirection {dir = Directions.South, ori = Orientation.Right } },
        { RoomDir.South_LR, new RoomDirection {dir = Directions.South, ori = Orientation.LeftRight } },
        { RoomDir.South_RL, new RoomDirection {dir = Directions.South, ori = Orientation.RightLeft } },

        { RoomDir.West_L, new RoomDirection {dir = Directions.West, ori = Orientation.Left } },
        { RoomDir.West_R, new RoomDirection {dir = Directions.West, ori = Orientation.Right } },
        { RoomDir.West_LR, new RoomDirection {dir = Directions.West, ori = Orientation.LeftRight } },
        { RoomDir.West_RL, new RoomDirection {dir = Directions.West, ori = Orientation.RightLeft } },
    };
    public GameObject BaseStructure;
    public GameObject PortalRight;
    public GameObject PortalLeft;
    public GameObject MiniWall_R;
    public GameObject MiniWall_L;
    public GameObject InnerWall;
    public GameObject Door;


    private void SetTeleporterDir(RoomDirection direction, GameObject teleporter)
    {
        foreach (KeyValuePair<RoomDir, RoomDirection> rd in DirToDirMap)
        {
            if (rd.Value == direction)
            {
                teleporter.GetComponentInChildren<Teleporter>().direction = rd.Key;
                break;
            }
        }
    }
    private int CreateEntrance(RoomDirection entrance, GameObject room, int numberOfExits, List<Directions> directionsUsed)
    {
        int newNumberOfExits = numberOfExits;
       
        GameObject entranceObj = new GameObject("entrance: "+entrance.dir.ToString() + entrance.ori.ToString());
        Instantiate(Door, entranceObj.transform);
        directionsUsed.Add(entrance.dir);
        newNumberOfExits = CreateDoor(entranceObj,entrance,newNumberOfExits);
        RotateToDir(entranceObj, entrance.dir);
        entranceObj.transform.SetParent(room.transform);
        return newNumberOfExits;
    }
    private int CreateDoor(GameObject doorObj,RoomDirection entrance, int numberOfExits){
        GameObject portal_L;
        GameObject portal_R;
        int newNumberOfExits = numberOfExits;
        switch (entrance.ori)
        {
            case Orientation.Left:
                portal_L = Instantiate(PortalLeft, doorObj.transform);
                Instantiate(MiniWall_R, doorObj.transform);
                SetTeleporterDir(entrance, portal_L);
                break;
            case Orientation.Right:
                portal_R = Instantiate(PortalRight, doorObj.transform);
                Instantiate(MiniWall_L, doorObj.transform);
                SetTeleporterDir(entrance, portal_R);
                break;
            case Orientation.LeftRight:
                portal_L = Instantiate(PortalLeft, doorObj.transform);
                portal_R = Instantiate(PortalRight, doorObj.transform);
                RoomDirection RDir = new RoomDirection { dir = entrance.dir, ori = Orientation.RightLeft };
                SetTeleporterDir(entrance, portal_L);
                SetTeleporterDir(RDir, portal_R);
                newNumberOfExits--;
                break;
            case Orientation.RightLeft:
                portal_L = Instantiate(PortalLeft, doorObj.transform);
                portal_R = Instantiate(PortalRight, doorObj.transform);
                RoomDirection LDir = new RoomDirection { dir = entrance.dir, ori = Orientation.LeftRight };
                SetTeleporterDir(entrance, portal_R);
                SetTeleporterDir(LDir, portal_L);
                newNumberOfExits--;
                break;
        }
        return newNumberOfExits;
    }
    private void RotateToDir(GameObject obj,Directions dir)
    {
        Vector3 rotVal;
        switch (dir)
        {
            case Directions.North:
                rotVal = new Vector3(0, 0, 0);
                break;
            case Directions.East:
                rotVal = new Vector3(0, 90, 0);
                break;
            case Directions.South:
                rotVal = new Vector3(0, 180, 0);
                break;
            case Directions.West:
                rotVal = new Vector3(0, -90, 0);
                break;
            default:
                rotVal = new Vector3(0, 0, 0);
                break;
        }
        obj.transform.Rotate(rotVal);
    }

    public GameObject CreateRoom(RoomDir entranceDir,int maxNumberOfExits)//numberOfExits 0-7
    {
        if (maxNumberOfExits > 7) maxNumberOfExits = 7;
        List<Directions> directionsUsed = new List<Directions>();
        GameObject room = new GameObject(entranceDir.ToString()+maxNumberOfExits);
        Instantiate(BaseStructure, room.transform);
        RoomDirection entranceDirection = DirToDirMap[entranceDir];
        maxNumberOfExits = CreateEntrance(entranceDirection, room, maxNumberOfExits, directionsUsed);
        while (maxNumberOfExits > 0)
        {   
            maxNumberOfExits = CreateExit(room,maxNumberOfExits,directionsUsed);
            if (directionsUsed.Count >= 4)
            {
                maxNumberOfExits = 0;
            }
        }
        foreach(Directions d in System.Enum.GetValues(typeof(Directions)))
        {
            if (!(directionsUsed.Contains(d)))
            {
                CreateWall(room,d);
            }
        }
        //TO_DO Create Exits
        Debug.Log("created room: "+ entranceDir.ToString() + maxNumberOfExits);
        Debug.Log("With exits in: ");
        foreach(Directions debugDir in directionsUsed){
            Debug.Log(debugDir.ToString());
        }
        room.AddComponent<RoomDirections>();
        return room;
    }

    private void CreateWall(GameObject room, Directions d)
    {
        GameObject wallObj = new GameObject("wall"+d.ToString());
        Instantiate(InnerWall, wallObj.transform);
        RotateToDir(wallObj, d);
        wallObj.transform.SetParent(room.transform);
    }

    private int CreateExit(GameObject room, int maxNumberOfExits, List<Directions> directionsUsed)
    {
        int newNumberOfExits = maxNumberOfExits;
        List<Directions> remainingDirection = allDirections.Except<Directions>(directionsUsed).ToList();
        GameObject doorObj;

        if (remainingDirection.Count == 0) 
        {
            newNumberOfExits = 0;
        }
        else
        {
            Directions randomDir = remainingDirection.ElementAt(UnityEngine.Random.Range(0, remainingDirection.Count));
            //(Directions)dirValues.GetValue(UnityEngine.Random.Range(0, dirValues.Length-1));
            newNumberOfExits--;
            System.Array values = System.Enum.GetValues(typeof(Orientation));
            Orientation randomOri = (Orientation)values.GetValue(UnityEngine.Random.Range(0, values.Length - 1));
            RoomDirection doorDir = new RoomDirection { dir = randomDir, ori = randomOri };
            if (maxNumberOfExits < 2)
            {
                switch (randomOri)
                {
                    case Orientation.LeftRight:
                        doorDir = new RoomDirection { dir = randomDir, ori = Orientation.Left };
                        doorObj = new GameObject("entrance: " + randomDir + Orientation.Left.ToString());
                        newNumberOfExits = CreateDoor(doorObj, doorDir, newNumberOfExits);
                        break;
                    case Orientation.RightLeft:
                        doorDir = new RoomDirection { dir = randomDir, ori = Orientation.Right };
                        doorObj = new GameObject("entrance: " + randomDir + randomOri.ToString());
                        newNumberOfExits = CreateDoor(doorObj, doorDir, newNumberOfExits);
                        break;
                    default:
                        doorObj = new GameObject("entrance: " + randomDir + Orientation.Right.ToString());
                        newNumberOfExits = CreateDoor(doorObj, doorDir, newNumberOfExits);
                        break;
                }
            }
            else
            {
                doorObj = new GameObject("entrance: " + randomDir + randomOri.ToString());
                newNumberOfExits = CreateDoor(doorObj, doorDir, newNumberOfExits);
            }
            directionsUsed.Add(randomDir);
            doorObj.transform.SetParent(room.transform);
            Instantiate(Door, doorObj.transform);
            RotateToDir(doorObj, randomDir);
        }
        return newNumberOfExits;
    }

    /*private void Start()
    {
        Debug.Log("starting");
     
        System.Array dirValues = System.Enum.GetValues(typeof(RoomDir));
        RoomDir randomDir = (RoomDir)dirValues.GetValue(UnityEngine.Random.Range(1,dirValues.Length - 2));
        GameObject room = CreateRoom(randomDir, 0);
        randomDir = (RoomDir)dirValues.GetValue(UnityEngine.Random.Range(1, dirValues.Length - 2));
        room = CreateRoom(randomDir, 1);
        randomDir = (RoomDir)dirValues.GetValue(UnityEngine.Random.Range(1, dirValues.Length - 2));
        room = CreateRoom(randomDir, 2);
        randomDir = (RoomDir)dirValues.GetValue(UnityEngine.Random.Range(1, dirValues.Length - 2));
        room = CreateRoom(randomDir, 3);
        randomDir = (RoomDir)dirValues.GetValue(UnityEngine.Random.Range(1, dirValues.Length - 2));
        room = CreateRoom(randomDir, 4);
        randomDir = (RoomDir)dirValues.GetValue(UnityEngine.Random.Range(1, dirValues.Length - 2));
        room = CreateRoom(randomDir, 5);
        randomDir = (RoomDir)dirValues.GetValue(UnityEngine.Random.Range(1, dirValues.Length - 2));
        room = CreateRoom(randomDir, 6);
        randomDir = (RoomDir)dirValues.GetValue(UnityEngine.Random.Range(1, dirValues.Length - 2));
        room = CreateRoom(randomDir, 7);
    }
    */
}
