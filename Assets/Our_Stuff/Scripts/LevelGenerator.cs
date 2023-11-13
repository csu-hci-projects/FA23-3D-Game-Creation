using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int widthInRooms = 2;
    public int heightInRooms = 2;
    public int roomSideLength = 6;
    public Sprite[] sprites;
    private Texture2D[] rooms;
    public ColorToPrefabs[] colorMappings;
    void Start()
    {
        rooms = new Texture2D[widthInRooms * heightInRooms];
        for(int i = 0; i < sprites.Length; i++) 
        {
            rooms[i] = textureFromSprite(sprites[i]);
        }
        GenerateRooms();
    }

    public static Texture2D textureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
        {
            return sprite.texture;
        }
    }

    void GenerateRooms()
    {

        for (int i = 0; i < rooms.Length; i++) {
            String name = "Sala" + i;
            GameObject roomObject = new GameObject(name);
            Vector3 roomPosition = new Vector3(roomSideLength * (i / heightInRooms), 0, roomSideLength * (i % heightInRooms));
            roomObject.transform.position = roomPosition;
            for (int x = 0; x < rooms[i].width; x++)
            {
                for (int y = 0; y < rooms[i].height; y++)
                {
                    GenerateTile(rooms[i], x, y, roomObject.transform);
                }
            }
        }

        //Eventualmente podemos mudar isto para estar mehlor conforme a imagem tipo
        //e assim podemos mandar salas gerar a sul, nao so a este das outras imagens
        /*foreach (Texture2D room in rooms)
        {
            String name = "Sala" + rooms.IndexOf(room);
            GameObject roomObject = new GameObject(name);
            Vector3 roomPosition = new Vector3(roomSideLength * rooms.IndexOf(room), 0,0);
            roomObject.transform.position = roomPosition;
            for (int x = 0; x < room.width; x++)
            {
                for (int y = 0; y < room.height; y++)
                {
                    GenerateTile(room, x, y, roomObject.transform);
                }
            }
        }*/
    }

    void GenerateTile(Texture2D room, int x, int y, Transform parent)
    {
        Color pixelColor = room.GetPixel(x, y);

        if (pixelColor.a == 0)
        {
            //transparent, do nothing
            return;
        }

        foreach (ColorToPrefabs colorMapping in colorMappings)
        {
            if (EqualColors(colorMapping.color,pixelColor))
            {
                foreach (GameObject prefab in colorMapping.prefabs)
                {
                    Vector3 positionVector = new Vector3(x, prefab.transform.position.y, y);
                    Quaternion rotation = prefab.transform.rotation;
                    Instantiate(prefab, parent.position + positionVector, rotation, parent);
                }
            }
        }
    }

    private bool EqualColors(Color color1, Color color2)
    {
        bool equal = Math.Abs(color1.r - color2.r) <= 0.1 &&
            Math.Abs(color1.g - color2.g) <= 0.1 &&
            Math.Abs(color1.b - color2.b) <= 0.1;
        return equal;
    }

}
