using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Tile {

    public enum TileProperty
    {
        empty = 0,
        minerals = 1,
        monsters = 2
    }

    public Transform tileTransform;
    public TileProperty property;
    public bool revealed = false;
    public GameObject propertyObject;
    public SpriteRenderer sprite;
    public int distanceSteps = 100;
    public int x;
    public int y;
    public string face;

    // mineral tile variables
    public int currentYield;

    public Monster currentMonster;
    public List<Character> inhabitants = new List<Character>();

    public int[] openPaths = new int[]  { 0, 0,
                                         0,   0,
                                          0, 0 };

    private float _timer;

    public Tile() { }
    public Tile(Transform tileTransform)
    {
        this.tileTransform = tileTransform.Find("tileTransform").transform;
        sprite = tileTransform.GetComponent<SpriteRenderer>();
        SetTileHorizontalPosition();
        SetTileLevel();
        this.property = GenerateProperty();
        this.face = tileTransform.parent.name;
    }

    /// <summary>
    /// deep copy is for pathfinding without changing variables on the main tiles
    /// </summary>
    /// <returns></returns>
    public Tile DeepCopy()
    {
        return new Tile
        {
            tileTransform = this.tileTransform,
            revealed = this.revealed,
            distanceSteps = 100,
            x = this.x,
            y = this.y,
            openPaths = this.openPaths,
            face = this.face
        };
    }

    public int GetMineAmount()
    {
        float amountFloat = ((float)currentYield / 100) * LevelParameters.Instance.percentageMineralsDrainedEachHarvest;
        int amountInt = Mathf.RoundToInt(amountFloat);
        return amountInt;
    }

    public void MineTile(int amount)
    {
        currentYield -= amount;

        // TODO update some kind of mineral graphic
    }

    public void ForceProperty(TileProperty tileProperty)
    {
        this.property = tileProperty;
    }

    private TileProperty GenerateProperty()
    {
        int num = Random.Range(0, 100);
        if (num < 20)
            return TileProperty.empty;
        else if (num < 60)
            return TileProperty.minerals;
        else
        {
            //monster = new Monster(this);
            return TileProperty.monsters;
        }
    }

    private void SetTileLevel()
    {
        int level = 0;
        char levelString = tileTransform.parent.name[0];
        // special case bottom tile
        if (levelString == '-')
        {
            y = -1;
            revealed = true;
        }
        else
        {
            int.TryParse(levelString.ToString(), out level);
            y = level;
        }
    }

    private void SetTileHorizontalPosition()
    {
        int horizPos = 0;
        char posString = tileTransform.parent.name[tileTransform.parent.name.Length - 1];
        // special case bottom tile
        if (posString == '-')
        {
            x = -1;
            revealed = true;
        }
        else
        {
            int.TryParse(posString.ToString(), out horizPos);
            x = horizPos;
        }
    }

    public void DisplayPaths(bool reveal, int selectedPathOnly = 99)
    {
        for (int i = 0; i < openPaths.Length; i++)
        {
            // get the neighboring tile in this direction, if it exists
            Tile neighbor = Mountain.Instance.GetTilesNeighbour(Mountain.Instance.faces[face], this, i, false);

            // if we don't have a neighbor there is no path to show so turn off sprite and continue
            if (neighbor == null)
            {
                tileTransform.parent.Find(GetStringForDirection(i)).gameObject.SetActive(false);
                continue;
            }

            int pathType = Mountain.Instance.GetConnectionTypeBetweenTiles(this, i, neighbor);

            if (selectedPathOnly == 99 || selectedPathOnly == i)
                tileTransform.parent.Find(GetStringForDirection(i)).gameObject.SetActive(reveal && pathType != 0);

            // if it's a full path then show the other half
            if (pathType != 0)
                neighbor.tileTransform.parent.Find(GetStringForDirection(Mountain.Instance.GetOpposingDirection(i))).gameObject.SetActive(reveal);

            // if it's a half path...
            if (pathType == 1 && reveal)
            {
                // first make sure this rock is not already displayed
                if (neighbor.revealed)
                    continue;

                // otherwise notify the mountain it needs to instantiate a barrier between these tiles
                Mountain.Instance.CreateBarrierBetweenTiles(this, i, neighbor);
            }
        }
    }

    string GetStringForDirection(int direction)
    {
        if (direction == Mountain.TOP_LEFT)
            return "top_left";
        else if (direction == Mountain.TOP_RIGHT)
            return "top_right";
        else if (direction == Mountain.LEFT)
            return "left";
        else if (direction == Mountain.RIGHT)
            return "right";
        else if (direction == Mountain.BOTTOM_LEFT)
            return "bottom_left";
        else //if (direction == Mountain.BOTTOM_RIGHT)
            return "bottom_right";
    }

}
