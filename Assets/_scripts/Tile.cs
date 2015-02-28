using UnityEngine;
using System.Collections;

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
    public int currentYield;

    public int[] openPaths = new int[]  { 0, 0,
                                         0,   0,
                                          0, 0 };

    public Tile() { }
    public Tile(Transform tileTransform)
    {
        this.tileTransform = tileTransform;
        sprite = tileTransform.GetComponent<SpriteRenderer>();
        SetTileHorizontalPosition();
        SetTileLevel();
        this.property = GenerateProperty();
        this.face = tileTransform.parent.name;
    }

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

        // TODO: update some kind of mineral graphic
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
            return TileProperty.monsters;
    }

    private void SetTileLevel()
    {
        int level = 0;
        char levelString = tileTransform.name[0];
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
        char posString = tileTransform.name[tileTransform.name.Length - 1];
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
            if (i == Mountain.TOP_LEFT && (selectedPathOnly == 99 || selectedPathOnly == Mountain.TOP_LEFT))
                tileTransform.Find("top_left").gameObject.SetActive(openPaths[i] == 1 && reveal);
            else if (i == Mountain.TOP_RIGHT && (selectedPathOnly == 99 || selectedPathOnly == Mountain.TOP_RIGHT))
                tileTransform.Find("top_right").gameObject.SetActive(openPaths[i] == 1 && reveal);
            else if (i == Mountain.LEFT && (selectedPathOnly == 99 || selectedPathOnly == Mountain.LEFT))
                tileTransform.Find("left").gameObject.SetActive(openPaths[i] == 1 && reveal);
            else if (i == Mountain.RIGHT && (selectedPathOnly == 99 || selectedPathOnly == Mountain.RIGHT))
                tileTransform.Find("right").gameObject.SetActive(openPaths[i] == 1 && reveal);
            else if (i == Mountain.BOTTOM_LEFT && (selectedPathOnly == 99 || selectedPathOnly == Mountain.BOTTOM_LEFT))
                tileTransform.Find("bottom_left").gameObject.SetActive(openPaths[i] == 1 && reveal);
            else if (i == Mountain.BOTTOM_RIGHT && (selectedPathOnly == 99 || selectedPathOnly == Mountain.BOTTOM_RIGHT))
                tileTransform.Find("bottom_right").gameObject.SetActive(openPaths[i] == 1 && reveal);
        }
    }

}
