using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class GenerationManager : MonoBehaviour
{
    //Isto é para o teleporter conseguir encontrar o Manager facilmente
    public static GenerationManager instance;

    //Onde vamos por as salas todas no inspetor
    public List<RoomList> roomLists;

    //Prefabs de salas de onde escolher
    //private Dictionary<RoomDir, List<GameObject>> rooms = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de salas de onde escolher salas sem saida (finais)
    //private Dictionary<RoomDir, List<GameObject>> finalRooms = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de corredores de onde escolher
    private Dictionary<RoomDir, List<GameObject>> corridors = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de corredores finais de onde escohler
    private Dictionary<RoomDir, List<GameObject>> finalCorridors = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de salas de gelo onde escolher
    //private Dictionary<RoomDir, List<GameObject>> iceRooms = new Dictionary<RoomDir, List<GameObject>>();
    //Prefabs de corredores de gelo onde escolher
    //private Dictionary<RoomDir, List<GameObject>> iceCorridors = new Dictionary<RoomDir, List<GameObject>>();

    //Posições ocupadas por cada sala (grid, não espaço real, daí Vector2)
    private List<Vector2> roomPositions = new List<Vector2>();

    //Player controller/prefab a instanciar
    public GameObject player;

    //Raiz da árvore
    private TreeNode<Room> treeRoot;
    //Nodes todos da árvore
    private List<TreeNode<Room>> treeNodes = new List<TreeNode<Room>>();

    //-1 se nao houver limite, depois podemos meter outro valor, as este é o default
    public int depthLimit = 3;
    //Tamanho da maior sala, para quando metermos as salas como se numa grid cabem todas
    public int gridSize = 10;
    //Quando começa a criar as salas numa linha diferente
    public int maxSpawnWidth = 10;
    //Se começo com gelo
    public bool iceRoot = false;

    //Objeto a pegar e levar ao objetivo
    public GameObject interactivePrefab;
    //Objetivo onde entregar
    public GameObject goalPrefab;
    //Vamos dividir a lista de nodes em x partes diferentes. 
    //Vamos escolher a primeira e a ultima parte
    //Escolhemos da primeira parte onde metemos o interactable
    //E na ultima parte onde metemos o goal
    //Como isto faz recursivo/depth first
    //Se for 2, por exemplo, em principio ficam em lados opostos da árvore
    [Range(1, 10)]
    public int distanceFactor = 2;
    [Range(1, 7)]
    public int maxNumExits = 2;
    //Numero a imprimir na sala;
    private int roomNumber = 1;

    RoomFactory factory;

    private void Awake()
    {
        instance = this;

        int lenght = roomLists.Count;

        //Meter keys para cada direção no inspetor, poupar algum trabalho
        for (int i = 0; i < lenght; i++)
        {
            /*
            if (roomLists[i].roomType == RoomType.Room)
            {
                if (roomLists[i].IceRooms)
                {
                    iceRooms.Add(roomLists[i].roomDirection, roomLists[i].rooms);
                }
                else
                {
                    rooms.Add(roomLists[i].roomDirection, roomLists[i].rooms);
                }
            }
            else*/
            if (roomLists[i].roomType == RoomType.Corridor)
            {
                if (roomLists[i].IceRooms)
                {
                    //iceCorridors.Add(roomLists[i].roomDirection, roomLists[i].rooms);
                    //do nothing
                }
                else
                {
                corridors.Add(roomLists[i].roomDirection, roomLists[i].rooms);
                }
            }
            /*
            else if (roomLists[i].roomType == RoomType.Final)
            {
                finalRooms.Add(roomLists[i].roomDirection, roomLists[i].rooms);
            }*/
            else if (roomLists[i].roomType == RoomType.FinalCorridor)
            {
                finalCorridors.Add(roomLists[i].roomDirection, roomLists[i].rooms);
            }
        }

    }

    void Start()
    {
        if (depthLimit > 1)
        {
            factory = this.GetComponent<RoomFactory>();

            RoomDir dir = RandomEnum<RoomDir>();

            //Escolher primeira sala aleatoriamente
            GameObject firstRoom = factory.CreateRoom(dir, UnityEngine.Random.Range(1, maxNumExits + 1));

            //Meter na posição certa
            //GameObject aux = Instantiate(firstRoom, Vector3.zero, Quaternion.identity, this.gameObject.transform);
            firstRoom.transform.SetParent(this.gameObject.transform);
            firstRoom.transform.localScale = new Vector3(1, 1, 1);

            //Criar raiz da árvore (depois de instanciar, porque instancia != prefab e porque só se cria o node se instanciar bem)
            treeRoot = new TreeNode<Room>(new Room(firstRoom, RoomType.Room, RoomDir.Root, iceRoot));
            treeNodes.Add(treeRoot);

            /*
            foreach (TextMesh t in firstRoom.GetComponentsInChildren<TextMesh>())
            {
                t.text = roomNumber.ToString();
            }
            */

            //Instanciar o player (vai ter que ser depois de instanciar a sala, não podemos pô-lo na cena no editor)
            _ = Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);

            //Guardar a posição ocupada pela sala
            roomPositions.Add(Vector2.zero);

            //Dar spawn dos filhos
            SpawnChildren(treeRoot);

            //Dar spawn dos interactables
            SpawnInteractables();

            //Desativar o mapa nao necessário
            GarbageCleanup(treeRoot);
        }
        else
        {
            Debug.Log("ERROR: Depth must be higher than 1");
        }
    }

    //public static System.Random RNG = new System.Random();

    public static RoomDir RandomEnum<RoomDir>()
    {
        Type type = typeof(RoomDir);
        Array values = Enum.GetValues(type);

        object value = values.GetValue(UnityEngine.Random.Range(1, values.Length - 1));
        return (RoomDir)Convert.ChangeType(value, type);

    }

    void SpawnInteractables()
    {
        TreeNode<Room> interactiveRoom = null;
        TreeNode<Room> goalRoom = null;
        if (distanceFactor > 1)
        {
            List<List<TreeNode<Room>>> lists = SplitRoomsList();
            List<TreeNode<Room>> firstBlock = lists[0];
            List<TreeNode<Room>> lastBlock = lists[lists.Count - 1];

            int c = 0;
            while (interactiveRoom == null && c < 1000)
            {
                TreeNode<Room> aux = firstBlock[UnityEngine.Random.Range(1, firstBlock.Count)];
                if (aux.Data.RoomType == RoomType.Room || aux.Data.RoomType == RoomType.Final)
                {
                    interactiveRoom = aux;
                }
            }
            if (c == 1000)
            {
                Debug.Log("ERROR: Could not find room to spawn interactive prefab");
            }

            c = 0;
            while (goalRoom == null && c < 1000)
            {
                TreeNode<Room> aux = lastBlock[UnityEngine.Random.Range(0, lastBlock.Count)];
                if (aux.Data.RoomType == RoomType.Room || aux.Data.RoomType == RoomType.Final)
                {
                    goalRoom = aux;
                }
            }
            if (c == 1000)
            {
                Debug.Log("ERROR: Could not find room to spawn goal prefab");
            }
        }
        else
        {
            int c = 0;
            while (interactiveRoom == null && c < 1000)
            {
                TreeNode<Room> aux = treeNodes[UnityEngine.Random.Range(1, treeNodes.Count)];
                if (aux.Data.RoomType == RoomType.Room || aux.Data.RoomType == RoomType.Final)
                {
                    interactiveRoom = aux;
                }
            }
            if (c == 1000)
            {
                Debug.Log("ERROR: Could not find room to spawn interactive prefab");
            }

            c = 0;
            while (goalRoom == null && c < 1000)
            {
                TreeNode<Room> aux = treeNodes[UnityEngine.Random.Range(0, treeNodes.Count)];
                if (aux != interactiveRoom && (aux.Data.RoomType == RoomType.Room || aux.Data.RoomType == RoomType.Final))
                {
                    goalRoom = aux;
                }
            }
            if (c == 1000)
            {
                Debug.Log("ERROR: Could not find room to spawn goal prefab");
            }
        }

        //Dar os spawns em si
        //interactiveRoom.Data.roomInstance.transform
        GameObject interactable = Instantiate(interactivePrefab, interactiveRoom.Data.roomInstance.transform);
        /*if (Instantiate(interactivePrefab,  interactiveRoom.Data.roomInstance.transform))
        {
            Debug.Log("Spawned interactable in " + treeNodes.IndexOf(interactiveRoom) + " - " + interactiveRoom.Data.roomInstance);
        }
        else
        {
            Debug.Log("ERROR: Could not spawn interactable");
        }*/
        //goalRoom.Data.roomInstance.transform
        if (Instantiate(goalPrefab, goalRoom.Data.roomInstance.transform))
        {
            Debug.Log("Spawned goal in " + treeNodes.IndexOf(goalRoom) + " - " + goalRoom.Data.roomInstance);
        }
        else
        {
            Debug.Log("ERROR: Could not spawn goal");
        }

    }

    /// <summary>
    /// Método com as operações a realizar quando passas para uma nova sala
    /// Ou seja, quando se entre num portal, nesse OnTriggerEnter, chama-se isto
    /// </summary>
    /// <param name="newRoom">A nova sala para onde o player vai (passada pelo portal)</param>
    public void OnPortalPass(GameObject obj)
    {
        //Mudar os objetos ativos na cena, para optimização
        GarbageCleanup(GetTreeNode(obj));
    }

    /// <summary>
    /// Certifica-se que só as salas para onde o player pode ir ficam ativas.
    /// </summary>
    /// <param name="newRoom">A nova sala para onde o player vai</param>
    private void GarbageCleanup(TreeNode<Room> newRoom)
    {
        int depth = newRoom.Level;
        foreach (TreeNode<Room> room in treeNodes)
        {
            //TODO ver se as comparações depois do && funcionam
            //Nao sei porque nenhuma destas funcionam
            //&& (newRoom.Parent == room || newRoom.HasChild(room.Data))
            //&& newRoom.Related(room)
            if (Mathf.Abs(room.Level - depth) < 2)
            {
                room.Data.roomInstance.SetActive(true);
            }
            else
            {
                room.Data.roomInstance.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Escolhe filhos de um node aleatoriamente e instancia-os
    /// </summary>
    /// <param name="parent">Node pai</param>
    private void SpawnChildren(TreeNode<Room> parent)
    {
        if (parent.IsLeaf && parent.Level <= depthLimit && parent.Data.RoomType != RoomType.Final)
        {
            List<RoomDir> directions = parent.Data.PortalPositions;
            directions.Remove(parent.Data.EntranceDirection);
            foreach (RoomDir direction in directions)
            {
                GameObject obj;
                RoomType type;
                bool ice = false;
                //Sala normal (não interessa ser de gelo o GetRandomCorridor vai à lista correcta)
                if (parent.Data.RoomType == RoomType.Room)
                {
                    //Se a sala for a penultima em depth nao pode dar spawn de uma sala final a seguir
                    //Logo dá spawn de um corredor final
                    if (parent.Level == depthLimit - 2 || parent.Level == depthLimit - 3)
                    {
                        obj = GetRandomFinalCorridor(direction);
                        type = RoomType.FinalCorridor;
                    }
                    //Senão escolhe um corredor ao calhas
                    else
                    {
                        //Só vai buscar corredor de gelo se o pai for sala de gelo
                        /*if (parent.Data.IceRoom && parent.Data.RoomType == RoomType.Room)
                        {
                            obj = GetRandomIceCorridor(direction);
                            type = RoomType.Corridor;
                            ice = true;
                        }
                        else
                        {*/
                        obj = GetRandomCorridor(direction);
                        type = RoomType.Corridor;
                        //}
                    }

                }
                //Se for corredor
                else if (parent.Data.RoomType == RoomType.Corridor)
                {
                    //Se o corredor for de gelo, vai buscar um corredor normal
                    /*if (parent.Data.IceRoom)
                    {
                        obj = GetRandomCorridor(direction);
                        type = RoomType.Corridor;
                    }
                    //Se não for de gelo não liga
                    else
                    {*/
                        obj = factory.CreateRoom(direction, UnityEngine.Random.Range(1, maxNumExits + 1));
                        type = RoomType.Room;
                        roomNumber++;
                    //}
                }
                //Final corridor
                else
                {
                    //Buscar uma sala final
                    obj = factory.CreateRoom(direction, 0);
                    type = RoomType.Final;
                    roomNumber++;
                }
                Vector2 position = GetNewPosition();
                if (type == RoomType.Corridor || type == RoomType.FinalCorridor)
                {
                    GameObject GenCorridor = Instantiate(obj, new Vector3(position.y * gridSize, 0, position.x * gridSize), Quaternion.identity, this.gameObject.transform);
                    if (GenCorridor != null)
                    {
                        int c = 0;
                        //Passar parametros aos portais do pai para fazerem bem a ligação
                        List<Teleporter> parentPortals = parent.Data.roomInstance.GetComponent<RoomDirections>().Portals;
                        foreach (Teleporter portal in parentPortals)
                        {
                            //Se o pai/currente tiver 2 portais tenho de saber qual vai ligar
                            if (portal.direction == direction)
                            {
                                //Vai do pai para o filho
                                portal.SetRooms(parent.Data.roomInstance, GenCorridor);
                                c++;
                            }
                        }

                        //Passar parametros aos portais do filho para fazerem bem a ligação
                        List<Teleporter> childPortals = GenCorridor.GetComponent<RoomDirections>().Portals;
                        foreach (Teleporter portal in childPortals)
                        {
                            //Se o filho tiver 2 portais tenho de saber para onde vai (partilham direcao)
                            if (portal.direction == direction)
                            {
                                //Vai do filho para o pai
                                portal.SetRooms(GenCorridor, parent.Data.roomInstance);
                                c++;
                            }
                        }

                        if (c % 2 != 0)
                        {
                            Debug.Log("FODEU INVESTIGA");
                        }

                        TreeNode<Room> child = parent.AddChild(new Room(GenCorridor, type, direction, ice));
                        treeNodes.Add(child);
                        //Não faço no GetPosition porque só aqui é que dou spawn da sala
                        roomPositions.Add(position);

                        /*
                        if (type == RoomType.Room)
                        {
                            foreach (TextMesh t in GenCorridor.GetComponentsInChildren<TextMesh>())
                            {
                                t.text = roomNumber.ToString();
                            }
                        }
                        */
                        //Dar spawn dos filhos desta sala tambem
                        SpawnChildren(child);

                    }
                    else
                    {
                        Debug.Log("Error: Could not instantiate child for " + treeNodes.IndexOf(parent) + " - " + parent.Data.roomInstance);
                    }
                }
                else
                {
                    if (obj != null)
                    {
                        obj.transform.SetParent(this.gameObject.transform);
                        obj.transform.localScale = new Vector3(1, 1, 1);
                        obj.transform.position = new Vector3(position.y * gridSize, 0, position.x * gridSize);

                        int c = 0;
                        //Passar parametros aos portais do pai para fazerem bem a ligação
                        List<Teleporter> parentPortals = parent.Data.roomInstance.GetComponent<RoomDirections>().Portals;
                        foreach (Teleporter portal in parentPortals)
                        {
                            //Se o pai/currente tiver 2 portais tenho de saber qual vai ligar
                            if (portal.direction == direction)
                            {
                                //Vai do pai para o filho
                                portal.SetRooms(parent.Data.roomInstance, obj);
                                c++;
                            }
                        }

                        //Passar parametros aos portais do filho para fazerem bem a ligação
                        List<Teleporter> childPortals = obj.GetComponent<RoomDirections>().Portals;
                        foreach (Teleporter portal in childPortals)
                        {
                            //Se o filho tiver 2 portais tenho de saber para onde vai (partilham direcao)
                            if (portal.direction == direction)
                            {
                                //Vai do filho para o pai
                                portal.SetRooms(obj, parent.Data.roomInstance);
                                c++;
                            }
                        }

                        if (c % 2 != 0)
                        {
                            Debug.Log("FODEU INVESTIGA");
                        }


                        TreeNode<Room> child = parent.AddChild(new Room(obj, type, direction, ice));
                        treeNodes.Add(child);
                        //Não faço no GetPosition porque só aqui é que dou spawn da sala
                        roomPositions.Add(position);

                        /*
                        if (type == RoomType.Room)
                        {
                            foreach (TextMesh t in GenCorridor.GetComponentsInChildren<TextMesh>())
                            {
                                t.text = roomNumber.ToString();
                            }
                        }
                        */

                        //Dar spawn dos filhos desta sala tambem
                        SpawnChildren(child);
                    }
                    else
                    {
                        Debug.Log("Error: Could not instantiate child for " + treeNodes.IndexOf(parent) + " - " + parent.Data.roomInstance);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Vai buscar uma sala qualquer para raiz
    /// </summary>
    /// <returns> Um prefab de uma sala para servir de raiz</returns>
   /*
    private GameObject GetRandomRoot(bool iceRoot)
    {
        if (iceRoot)
        {
            return iceRooms[RoomDir.Root][Random.Range(0, iceRooms[RoomDir.Root].Count)];
        }
        return rooms[RoomDir.Root][Random.Range(0, rooms[RoomDir.Root].Count)];
    }
   */
    /// <summary>
    /// Escolhe uma sala ao calhas
    /// </summary>
    /// <param name="direction">Direção de entrada na sala</param>
    /// <returns>Prefab para instanciar</returns>
  /*
    private GameObject GetRandomRoom(RoomDir direction)
    {
        return rooms[direction][Random.Range(0, rooms[direction].Count)];
    }
    */
    /// <summary>
    /// Retorna uma sala sem saidas, para fechar o mapa
    /// </summary>
    /// <param name="direction">Direção por onde o player vai entrar na sala</param>
    /// <param name="iceRoom">Se o pai é de gelo</param>
    /// <returns>Sala sem saidas, para fechar o mapa</returns>
    /*
    private GameObject GetFinalRoom(RoomDir direction)
    {
        return finalRooms[direction][Random.Range(0, finalRooms[direction].Count)];
    }
    */
    /// <summary>
    /// Retorna um corredor aleatório
    /// </summary>
    /// <param name="direction">Direção de entrada no corredor</param>
    /// <param name="iceRoom">Se o pai é de gelo</param>
    /// <returns>Prefab de um corredor para instanciar</returns>
    private GameObject GetRandomCorridor(RoomDir direction)
    {
        return corridors[direction][UnityEngine.Random.Range(0, corridors[direction].Count)];
    }

    /*
    private GameObject GetRandomIceCorridor(RoomDir direction)
    {
        return iceCorridors[direction][UnityEngine.Random.Range(0, iceCorridors[direction].Count)];
    }
    */

    //Ainda nao pus para dar gelo
    /// <summary>
    /// Retorna um corredor que tem que vir obrigatóriamente antes de uma sala final
    /// </summary>
    /// <param name="direction">Direção de entrada no corredor</param>
    /// <returns>Corredor final</returns>
    private GameObject GetRandomFinalCorridor(RoomDir direction)
    {
        return finalCorridors[direction][UnityEngine.Random.Range(0, finalCorridors[direction].Count)];
        /*
        int c = 0;
        while (c < 1000) {

            GameObject corridor = corridors[direction][UnityEngine.Random.Range(0, corridors[direction].Count)];
            if (!corridor.GetComponent<RoomDirections>().PortalPositions.Contains(RoomDir.C))
            {
                return corridor;
            }
            c++;
        }

        return null;*/
    }

    /// <summary>
    /// Vai buscar a próxima posição para onde dar spawn da sala
    /// </summary>
    /// <returns>A posição onde dar o próximo spawn</returns>
    private Vector2 GetNewPosition()
    {
        //supostamente faz (0,0), (1,0), (2,0) e quando chega aos 10, sobe de linha
        Vector2 lastVector = roomPositions[roomPositions.Count - 1];
        if (lastVector.x < maxSpawnWidth - 1)
        {
            return lastVector + Vector2.right;
        }
        else
        {
            return new Vector2(0, lastVector.y + 1);
        }
    }

    /// <summary>
    /// Sabendo a instância da sala (pai do portal ou assim), ir buscar o node correcto na árvore
    /// </summary>
    /// <param name="obj">Sala instanciada cujo node queremos</param>
    /// <returns></returns>
    public TreeNode<Room> GetTreeNode(GameObject obj)
    {
        //TODO ver se isto do equals funciona
        //Mas em principio dá porque instanciamos a sala e depois fazemos a Room, logo a referencia será a mesma
        //senão temos que repensar isto de como a partir do obj vou buscar o node, 
        //porque vai ficar mais complicado se isto nao der
        foreach (TreeNode<Room> room in treeNodes)
        {
            if (room.Data.roomInstance.Equals(obj))
            {
                return room;
            }
        }
        return null;
    }

    private List<List<TreeNode<Room>>> SplitRoomsList()
    {
        var list = new List<List<TreeNode<Room>>>();

        for (int i = 0; i < treeNodes.Count; i += distanceFactor)
        {
            list.Add(treeNodes.GetRange(i, Mathf.Min(distanceFactor, treeNodes.Count - i)));
        }

        return list;
    }


}
