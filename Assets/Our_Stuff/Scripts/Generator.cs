using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public static Generator generator;

    private void Awake()
    {
        generator = this;
    }
    public GameObject flag;
    public GameObject goal;
    public GameObject player;
    public int seed;
    public Vector3 roomScale = Vector3.one;
    public GameObject roof;
    public GameObject outerFloor;
    public GameObject outerWalls;
    public GameObject miniWallEven;
    public GameObject miniWallOdd;
    public GameObject roomTpEven;
    public GameObject roomTpOdd;
    public GameObject corridorTpEven;
    public GameObject corridorTpOdd;
    public GameObject corridorInnerWalls;
    public List<RoomProperties> innerMaterials;
    public int maxDepth;
    public static int numberOfRooms = -1;
    public int roomDistance = 5;
    public List<Room>[] roomList;
    public List<Node> allNodes;
    [System.Serializable]
    public struct RoomProperties
    {
        [SerializeField]
        public GameObject innerFloor;
        [SerializeField]
        public GameObject innerWalls;
        [SerializeField]
        public GameObject innerDoorframe;
        [SerializeField]
        public bool hasSpecialEffects;
        [SerializeField]
        public GameObject specialEffects;
    }

    public class Node
    {
        public GameObject node;
        public int[] id;
        public int depth;
        public bool isRoom;
       
        virtual protected void CreateOuterShell()
        {
            GameObject.Instantiate(generator.roof, node.transform);
            GameObject.Instantiate(generator.outerFloor, node.transform);
            GameObject.Instantiate(generator.outerWalls, node.transform);
        }

        protected string idToString()
        {
            string idString = "";
            foreach(int i in id){
                idString += i;
            }
            return idString;
        }
        virtual public void Move() { }
        virtual public int Move(int i,int r) { return i+1; }
    }

    public class Room : Node
    {
       
        public int properties;
        public Corridor[] corridors;
        public Room()
        {
            isRoom = true;
            depth = 0;
            id = new int[8];
            for (int i = 0; i < 8; i++)
            {
                float rngVal = Random.value;
                if (rngVal > 0.5f)
                {
                    id[i]=1;
                }
                else
                {
                    id[i] = 0;
                }
            }
            node = new GameObject("Room: " + depth + " | " + idToString());
            properties = depth;
            GenerateCorridors(null, -1);
            CreateOuterShell();
            PopulateRoom(id, properties);
            node.transform.localScale = generator.roomScale;
            generator.roomList[depth].Add(this);
            generator.allNodes.Add(this);
        }

        protected override void CreateOuterShell()
        {
            base.CreateOuterShell();
            GameObject.Instantiate(generator.innerMaterials[properties].innerFloor, node.transform);
            if (generator.innerMaterials[properties].hasSpecialEffects)
            {
                GameObject.Instantiate(generator.innerMaterials[properties].specialEffects, node.transform);
            }
        }
        public Room(int[] id, int properties, Corridor corridor, int corridorPos, int depth)
        {
            this.depth = depth;
            isRoom = true;
            this.id = id;
            this.properties = properties;
            node = new GameObject("Room: " + depth + " | " + idToString()); 
            GenerateCorridors(corridor, corridorPos);
            CreateOuterShell();
            PopulateRoom(id, properties);
            node.transform.localScale = generator.roomScale;
            generator.roomList[depth].Add(this);
            generator.allNodes.Add(this);

        }
        override public void Move()
        {
            Generator.numberOfRooms++;
            int r = Generator.numberOfRooms;
            node.transform.position = new Vector3(0, 0, Generator.numberOfRooms * generator.roomDistance);
            int i = 1;
            foreach (Corridor c in corridors)
            {
                if(c!= null)
                {
                    i = c.Move(i,r)+1;
                }
            }
        }
        private void PopulateRoom(int[] id, int properties)
        {
            int[] hasDoor = new int[id.Length];
            for (int i = 0; i < id.Length; i += 2)
            {
                if(id[i] == 1 || id[i+1] == 1)
                {
                    hasDoor[i] = 1;
                    hasDoor[i+1] = 1;
                    generator.Rotate(GameObject.Instantiate(generator.innerMaterials[properties].innerDoorframe, node.transform), i);
                }
                else
                {
                    hasDoor[i] = 0;
                    hasDoor[i + 1] = 0;
                    generator.Rotate(GameObject.Instantiate(generator.innerMaterials[properties].innerWalls, node.transform), i);

                }
            }
            for(int i = 0; i < id.Length; i++)
            {
                if(id[i] != hasDoor[i])
                {
                    if(i % 2 == 0)
                    {
                        generator.Rotate(GameObject.Instantiate(generator.miniWallEven, node.transform), i);
                    }
                    else
                    {
                        generator.Rotate(GameObject.Instantiate(generator.miniWallOdd, node.transform), i);

                    }
                }
                else if(id[i] == 1)
                {
                    GameObject temp;
                    if (i % 2 == 0)
                    {
                        temp = GameObject.Instantiate(generator.roomTpEven, node.transform);

                    }
                    else
                    {
                        temp = GameObject.Instantiate(generator.roomTpOdd, node.transform);
                    }
                    generator.Rotate(temp, i);
                    temp.GetComponentInChildren<Teleporter>().SetRooms(node, corridors[i].node);
                }
            }
        }
       

        private void GenerateCorridors(Corridor corridor, int corridorPos)
        {
            this.corridors = new Corridor[8];
            for (int i = 0; i < 8; i++)
            {
                if (i == corridorPos)
                {
                    corridors[i] = corridor;
                }
                else if (int.Parse(id[i].ToString()) == 1)
                {
                    corridors[i] = new Corridor(this, i);
                }
                else
                {
                    corridors[i] = null;
                }
            }
        }
    }

    public class Corridor : Node
    {
        public Node origin;
        public Node destination;
        public GameObject[] floors;

        public Corridor(Room room, int entrance)
        {
            isRoom = false;
            floors = new GameObject[2];
            id = new int[2];
            origin = room;
            node = new GameObject("");
            int way = 1;
            if (entrance % 2 == 0)
                way = -1;
            entrance = Wrap(entrance);
            id[0] =  entrance;
            entrance -= way;
            entrance = Wrap(entrance);

            CreateOuterShell();
            CreateEntrance(true, entrance, way);
            int corridorsRemaining = (int)(Random.value * 3);
            GenerateDestination(corridorsRemaining, entrance, way,true);
            node.name = "corridor: " + idToString();
            node.transform.localScale = generator.roomScale;
            generator.allNodes.Add(this);

        }

        public Corridor(Corridor corridor, int entrance, int corridorsRemaining)
        {
            isRoom = false;
            floors = new GameObject[2];
            id = new int[2];
            origin = corridor;
            node = new GameObject("");
            int way = 1;
            if (entrance % 2 == 0)
                way = -1;
            entrance -= way;
            entrance = Wrap(entrance);
            id[0] = entrance;
            corridorsRemaining--;
            CreateOuterShell();
            CreateEntrance(false, entrance, way);

            GenerateDestination(corridorsRemaining, entrance, way);

            node.name = "corridor: " + idToString();
            node.transform.localScale = generator.roomScale;
            generator.allNodes.Add(this);

        }

        private void CopyRoomFeatures(int[] idToCopy, int propertiesToCopy,int entrance, int way,int inOut)
        {
            floors[inOut] = GameObject.Instantiate(generator.innerMaterials[propertiesToCopy].innerFloor, node.transform);
            int[] miniWalls = new int[3];
            for (int i = 0; i < 3; i++)
            {
                int position = entrance + (i * way);
                position = Wrap(position);
                if (idToCopy[position] == 0)
                {

                    miniWalls[i] = position;
                    if (position % 2 == 0)
                    {
                        generator.Rotate(GameObject.Instantiate(generator.miniWallEven, node.transform), position);
                    }
                    else
                    {
                        generator.Rotate(GameObject.Instantiate(generator.miniWallOdd, node.transform), position);
                    }
                }
                else
                {
                    miniWalls[i] = -1;
                }
            }

            
            

            generator.Rotate(GameObject.Instantiate(generator.innerMaterials[propertiesToCopy].innerDoorframe, node.transform), Wrap(entrance));
            if (miniWalls[1] > -1 && miniWalls[2] > -1)
            {
                generator.Rotate(GameObject.Instantiate(generator.innerMaterials[propertiesToCopy].innerWalls, node.transform), Wrap(entrance + way * 2));
            }
            else
            {
                generator.Rotate(GameObject.Instantiate(generator.innerMaterials[propertiesToCopy].innerDoorframe, node.transform), Wrap(entrance + way * 2));
            }
        }

        private void SetPortal(int position, GameObject destination)
        {
            GameObject temp;
            if (position%2 == 0)
            {
                temp = GameObject.Instantiate(generator.corridorTpEven, node.transform);

            }
            else
            {
                temp = GameObject.Instantiate(generator.corridorTpOdd, node.transform);
            }
            generator.Rotate(temp, Wrap(position));
            temp.GetComponentInChildren<Teleporter>().SetRooms(node, destination);
        }

        private void CreateEntrance(bool originIsRoom, int entrance, int way)
        {
            depth = origin.depth;
            if (originIsRoom)
            {
                CopyRoomFeatures(origin.id,((Room)origin).properties, entrance, -way,0);
               
            }
            else
            {
                PopulateWalls(entrance, -way);
            }
            SetPortal(id[0], origin.node);
        }

        private void CreateExit(bool destinationISRoom, int exit, int way, bool isSingle = false)
        {
            if (destinationISRoom)
            {
                CopyRoomFeatures(destination.id, ((Room)destination).properties, exit, way,1);
                if (isSingle)
                {
                    //TO-DO Triger that changes inner floor
                }
            }
            else
            {
                PopulateWalls(exit, way);
            }
            SetPortal(int.Parse(id[1].ToString()), destination.node);
        }

        private void PopulateWalls(int startPosition, int way)
        {
            generator.Rotate(GameObject.Instantiate(generator.corridorInnerWalls, node.transform), Wrap(startPosition + (way * 0)));
            generator.Rotate(GameObject.Instantiate(generator.corridorInnerWalls, node.transform), Wrap(startPosition + (way * 2)));
        }

       

        

       

        private void GenerateDestination(int corridorsRemaining, int entrance, int way, bool isSingle = false)
        {
            int exit;
            if (corridorsRemaining == 0)
            {
                int destinationDepth = depth + 1;
                exit = Wrap(entrance + 2 * way);
                id[1] = exit;
                int[] roomId = new int[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i == exit)
                    {
                        roomId[i] = 1;
                    }
                    else if (destinationDepth < generator.maxDepth)
                    {
                        Debug.Log("in");
                        Debug.Log(destinationDepth + " : " + generator.maxDepth);
                        roomId[i] = (int)Random.value;
                        Debug.Log(roomId[i]);
                        roomId[i] = generator.rng();
                    }
                    else
                    {
                        Debug.Log("out");
                        Debug.Log(destinationDepth + " : " + generator.maxDepth);
                        roomId[i] = 0;
                    }
                }
                int roomProperties = destinationDepth;
                destination = new Room(roomId, roomProperties, this, exit, destinationDepth);
                CreateExit(true, exit, way, isSingle);
            }
            else
            {
                exit = Wrap(entrance + 3 * way);
                id[1] = exit;
                destination = new Corridor(this, exit, corridorsRemaining);
                CreateExit(false, exit, way, false);
            }
        }

        private int Wrap(int position)
        {
            if (position< 0)
            {
                position += 8;
                return Wrap(position);
            }
            if (position > 7)
            {
                position -= 8;
                return Wrap(position);
            }
            return position;
        }

        override public int Move(int i,int r)
        {
            if (node.transform.position == Vector3.zero && destination != null)
            {
                node.transform.position = new Vector3(i * generator.roomDistance, 0, r * generator.roomDistance);
                if (destination.isRoom)
                {
                    destination.Move();
                    return i+1;
                }
                else
                {
                    return destination.Move(i + 1, r);
                }
            }
            return i+1;
        }
    }

    public void Rotate(GameObject obj, int position)
    {
       
        obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        switch (position)
        {
            case 0:
            case 1:
                obj.transform.Rotate(new Vector3(0, 0, 0));
                
                break;
            case 2:
            case 3:
                obj.transform.Rotate(new Vector3(0, 90, 0));
               
                break;
            case 4:
            case 5:
                obj.transform.Rotate(new Vector3(0, 180, 0));
                
                break;
            case 6:
            case 7:
                obj.transform.Rotate(new Vector3(0, -90, 0));
               
                break;
            default:
                obj.transform.Rotate(new Vector3(0, 45, 0));
               
                break;
        }
    }
    public int rng()
    {
        float rngVal = Random.value;
        if (rngVal > 0.5f)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    private void Start()
    {
        player.SetActive(false);
        maxDepth = innerMaterials.Count-1;
        Debug.Log(innerMaterials.Count);
        roomList = new List<Room>[innerMaterials.Count];
        for(int i = 0; i != innerMaterials.Count; i++)
        {
            roomList[i] = new List<Room>();
        }
        allNodes = new List<Node>();
        Random.InitState(seed);
        Node first = new Room();
        first.Move();
        for (int i = 0; i != innerMaterials.Count; i++)
        {
            Debug.Log("depth: " + i);
            foreach( Room r in roomList[i])
            {
                Debug.Log(r.node.name);
            }
        }
        int temp = Random.Range(0, roomList.Length);
        GameObject.Instantiate(flag, roomList[temp][Random.Range(0, roomList[temp].Count)].node.transform);
        temp = Random.Range(0, roomList.Length);
        GameObject.Instantiate(goal, roomList[temp][Random.Range(0, roomList[temp].Count)].node.transform);
        foreach (Node n in allNodes)
        {
            n.node.SetActive(false);
        }
        first.node.SetActive(true);
        player.SetActive(true);
       
    }

}
