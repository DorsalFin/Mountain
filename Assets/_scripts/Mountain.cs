using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mountain : MonoBehaviour {

    [System.Serializable]
    public class Face
    {
        public string side;
        public List<Tile> tiles = new List<Tile>();
    }
    [System.Serializable]
    public class Tile
    {
        public Transform tileTransform;
        public TileProperty property;
        public bool revealed = false;
        public GameObject propertyObject;

        public Tile() { }
        public Tile(Transform tileTransform)
        {
            this.tileTransform = tileTransform;
            // can generate on start? and reveal when player enters tile?
            this.property = GenerateProperty();
        }

        private TileProperty GenerateProperty()
        {
            int num = Random.Range(0, 100);
            if (num < 20)
                return TileProperty.empty;
            else if (num < 60)
                return TileProperty.minerals;
            else
                return TileProperty.monsters;
        }
    }

    public enum TileProperty
    {
        empty = 0,
        minerals = 1,
        monsters = 2
    }

    public static Mountain Instance;
    public List<Face> faces = new List<Face>();

    // tile property prefabs
    public Vector3 tilePropertyOffset;
    public GameObject mineralPrefab;
    public GameObject monsterPrefab;

    void Awake()
    {
        Instance = this;
        // populate tile references for each face
        foreach (Face face in faces)
        {
            Transform faceTransform = transform.Find(face.side);
            foreach (Transform child in faceTransform.transform)
            {
                Tile newTile = new Tile(child);
                face.tiles.Add(newTile);
            }
        }
    }

    public Tile GetStartPosition(string side)
    {
        foreach (Face face in faces)
        {
            if (face.side == side)
            {
                foreach (Tile tile in face.tiles)
                {
                    if (tile.tileTransform.name == "1.3")
                        return tile;
                }
            }
        }
        return null;
    }

	public Tile GetClosestTile(string side, Vector3 fromPosition)
    {
        Tile closestTile = new Tile();
        float closestDistance = Mathf.Infinity;
        foreach (Face face in faces)
        {
            if (face.side != side)
                continue;

            foreach (Tile tile in face.tiles)
            {
                float distance = Vector3.Distance(fromPosition, tile.tileTransform.position);
                if (distance < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = distance;
                }
            }
        }
        return closestTile;
    }

    public void ArrivedAtTile(Player player, Tile tile)
    {
        if (tile.revealed)
            return;

        tile.revealed = true;
        switch (tile.property)
        {
            case TileProperty.minerals:
                tile.propertyObject = (GameObject)Instantiate(mineralPrefab, tile.tileTransform.position + tilePropertyOffset, mineralPrefab.transform.rotation);
                break;
            case TileProperty.monsters:
                tile.propertyObject = (GameObject)Instantiate(monsterPrefab, tile.tileTransform.position + tilePropertyOffset, monsterPrefab.transform.rotation);
                break;
        }
    }
}
