﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Mountain : Photon.MonoBehaviour {

    public const int TOP_LEFT = 0;
    public const int TOP_RIGHT = 1;
    public const int LEFT = 2;
    public const int RIGHT = 3;
    public const int BOTTOM_LEFT = 4;
    public const int BOTTOM_RIGHT = 5;

    public static Mountain Instance;
    public GameObject cameraTarget;

    /// <summary>
    /// fill these transforms with any parent faces you have in the scene
    /// </summary>
    public Transform[] faceTransforms;
    public Dictionary<string, List<Tile>> faces = new Dictionary<string, List<Tile>>();

    /// <summary>
    /// different monster types that may spawn on the mountain
    /// </summary>
    public List<GameObject> monsters = new List<GameObject>();

    public bool mountainGenerationComplete = false;

    // tile property prefabs
    public Vector3 tilePropertyOffset;
    public GameObject mineralPrefab;
    public GameObject monsterPrefab;

    // path blockage prefab
    public GameObject blockagePrefab;

    private float _timer;

    //public GameObject arrowChangeFace;

    void Awake()
    {
        Instance = this;

        // populate tile references for each face
        foreach (Transform faceTransform in faceTransforms)
        {
            List<Tile> childTiles = new List<Tile>();
            Transform tileParent = faceTransform.Find("tiles");
            foreach (Transform child in tileParent)
            {
                Tile newTile = new Tile(child);
                childTiles.Add(newTile);
            }
            faces.Add(faceTransform.name, childTiles);
        }

        // complete path generation in a coroutine
        //StartCoroutine(RunPathGeneration());
    }

    void Start()
    {
        if (PhotonNetwork.isMasterClient || NetworkHandler.Instance.Offline)
        {
            StartCoroutine(RunPathGeneration());
        }
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > LevelParameters.Instance.mineralRefreshNumSeconds)
        {
            _timer = 0;
            RegenMinerals();
        }
    }

    public void RegenMinerals()
    {
        foreach (string faceKey in faces.Keys)
        {
            foreach (Tile tile in faces[faceKey])
            {
                if (tile.property == Tile.TileProperty.minerals)
                {
                    if (tile.currentYield < LevelParameters.Instance.initialMineralYield)
                        tile.currentYield++;
                }
            }
        }
    }

    /// <summary>
    /// each time a monster is spawned on a tile
    /// </summary>
    void SpawnMonster(Tile tile)
    {
        GameObject randomMonster = monsters[Random.Range(0, monsters.Count - 1)];
        tile.propertyObject = (GameObject)Instantiate(randomMonster, tile.tileTransform.position + tilePropertyOffset, monsterPrefab.transform.rotation);
        tile.propertyObject.GetComponent<Monster>().closestTile = tile;
        tile.currentMonster = tile.propertyObject.GetComponent<Monster>();
        tile.currentMonster.LevelUp(tile.monsterLevel);
        tile.inhabitants.Add(tile.propertyObject.GetComponent<Monster>());
    }

    public IEnumerator RespawnMonster(Tile tile)
    {
        // wait the requisite amount of respawn seconds
        yield return new WaitForSeconds(LevelParameters.Instance.timeBetweenMonsterRespawns);

        // then wait until all characters have left the tile
        while (tile.inhabitants.Count > 0)
            yield return null;

        SpawnMonster(tile);
    }

    /// <summary>
    /// we have to wait frames between path generation algorithms to avoid them
    /// overlapping and overwriting path values
    /// </summary>
    IEnumerator RunPathGeneration()
    {
        yield return new WaitForEndOfFrame();

        // generate the initial pathing between tiles
        foreach (string faceKey in faces.Keys)
            GeneratePaths(faceKey);

        yield return new WaitForEndOfFrame();

        foreach (string faceKey in faces.Keys)
        {
            GetStartPositionTile(faces[faceKey]).ForceProperty(Tile.TileProperty.empty);
            CheckPathsFromStartTile(faceKey, GetStartPositionTile(faces[faceKey]));
        }

        yield return new WaitForEndOfFrame();

        OpenPathsBetweenFaces();
        CloseAllUnReachableDirections();
        SetInitialTileMineralYields();

        yield return new WaitForEndOfFrame();

        //SetPlayerInitialStates();
        //yield return new WaitForEndOfFrame();

        SetAllTilesDisplayedState(false);
        mountainGenerationComplete = true;

        // this is only run by the master client, so i have to now pass
        // all generated variables to other client's mountain
        List<int> allPathList = new List<int>();
        List<int> tilePropertyTypes = new List<int>();

        foreach (string faceKey in faces.Keys)
        {
            foreach (Tile tile in faces[faceKey])
            {
                tilePropertyTypes.Add((int)tile.property);
                foreach (int path in tile.openPaths)
                    allPathList.Add(path);
            }
        }

        if (NetworkHandler.Instance.Online)
        {
            // pass path lists and tile property types
            int[] pathInts = allPathList.ToArray();
            int[] propertyInts = tilePropertyTypes.ToArray();
            photonView.RPC("InheritMountainVariables", PhotonTargets.OthersBuffered, pathInts, propertyInts);

            NetworkHandler.Instance.BeginFade();
        }
        else if (NetworkHandler.Instance.Offline)
        {
            NetworkHandler.Instance.SpawnPlayer();
        }
    }

    [RPC]
    public void InheritMountainVariables(int[] generatedPaths, int[] propertyInts)
    {
        // set same pathing as master client
        int currentPath = 0;
        // set same proprties as master client
        int currentProperty = 0;

        foreach (string faceKey in faces.Keys)
        {
            foreach (Tile tile in faces[faceKey])
            {
                tile.property = (Tile.TileProperty)propertyInts[currentProperty];
                currentProperty++;

                for (int i = 0; i < tile.openPaths.Length; i++)
                {
                    tile.openPaths[i] = generatedPaths[currentPath];
                    currentPath++;
                }
            }
        }

        SetInitialTileMineralYields();
        SetAllTilesDisplayedState(false);
        mountainGenerationComplete = true;
    }

    void SetInitialTileMineralYields()
    {
        foreach (string faceKey in faces.Keys)
        {
            foreach (Tile tile in faces[faceKey])
            {
                if (tile.property == Tile.TileProperty.minerals)
                    tile.currentYield = LevelParameters.Instance.initialMineralYield;
            }
        }
    }

    void CloseAllUnReachableDirections()
    {
        foreach (string faceKey in faces.Keys)
        {
            foreach (Tile tile in faces[faceKey])
            {
                if (tile.x == 0)
                    tile.openPaths[TOP_LEFT] = 0;
                if (tile.x == GetNumTilesOnLevel(tile.y))
                    tile.openPaths[TOP_RIGHT] = 0;
                if (tile.x == -1 || tile.x == 99)
                    tile.openPaths = new int[] { 0, 0, 0, 0, 0, 0 };
            }
        }
    }

    void OpenPathsBetweenFaces()
    {
        // this key will store the previous face, but we start with the last face in the
        // array so we can wrap through them all
        string previousFaceKey = faceTransforms[faceTransforms.Length - 1].name;

        // we need to shut the right hand side of tiles on the final face as a special case initially
        foreach (Tile tile in faces[previousFaceKey])
        {
            if (tile.x == GetNumTilesOnLevel(tile.y))
                tile.openPaths[RIGHT] = 0;
        }

        foreach (Transform faceTrans in faceTransforms)
        {
            string faceKey = faceTrans.name;

            // first let's close all the paths along the face edges
            foreach (Tile tile in faces[faceKey])
            {
                if (tile.x == 0)
                    tile.openPaths[LEFT] = 0;
                if (tile.x == GetNumTilesOnLevel(tile.y) && faceKey != faceTransforms[faceTransforms.Length-1].name)
                    tile.openPaths[RIGHT] = 0;
            }

            // generate the random levels to place paths on
            List<int> levels = new List<int>();
            for (int i = 0; i < LevelParameters.Instance.numPathsBetweenOneFaceAndAnother; i++)
            {
                while (true)
                {
                    int randomNum = Random.Range(1, 5); // on any level except bottom
                    if (!levels.Contains(randomNum))
                    {
                        levels.Add(randomNum);
                        break;
                    }
                }
            }

            // now open the paths
            for (int j = 0; j < levels.Count; j++)
            {
                Tile startTile = GetTileAtCoordinate(faces[faceKey], 0, levels[j]);
                Tile endTile = GetTileAtCoordinate(faces[previousFaceKey], GetNumTilesOnLevel(levels[j]), levels[j]);
                OpenPathBetweenTiles(startTile, endTile, LEFT);
            }

            // update the previous faceKey so we can use it next loop
            previousFaceKey = faceKey;
        }
    }

    /// <summary>
    /// simple function that adds up the openPath ints
    /// </summary>
    /// <returns>0 for no path, 1 for half path, 2 for full path</returns>
    public int GetConnectionTypeBetweenTiles(Tile startTile, int direction, Tile endTile)
    {
        return startTile.openPaths[direction] + endTile.openPaths[GetOpposingDirection(direction)];
    }

    public void CreateBarrierBetweenTiles(Tile tile1, int directionFromTile1, Tile tile2)
    {
        Vector3 middle = Vector3.Lerp(tile1.tileTransform.parent.position, tile2.tileTransform.parent.position, 0.5f);
        Vector3 dir = (tile2.tileTransform.position - tile1.tileTransform.position).normalized;
        
        GameObject b = (GameObject)Instantiate(blockagePrefab, middle, Quaternion.LookRotation(dir));

        // less gap between horizontal tiles so make the blockage a bit smaller
        if (tile1.y == tile2.y)
            b.transform.localScale = Vector3.one * 0.35f;

        Blockage blockage = b.AddComponent<Blockage>();
        blockage.tileOne = tile1;
        blockage.directionFromTileOne = directionFromTile1;
        blockage.tileTwo = tile2;
    }

    public void SetAllTilesDisplayedState(bool reveal)
    {
        foreach (Transform faceTrans in faceTransforms)
        {
            string faceKey = faceTrans.name;
            foreach (Tile tile in faces[faceKey])
                tile.DisplayPaths(reveal);
        }
    }

    /// <summary>
    /// show one face while hiding all others
    /// </summary>
    /// <param name="face">the face a player wants to reveal, pass "all" to show all</param>
    public void SetFaceVisibility(string face)
    {
        foreach (Transform t in faceTransforms)
            t.gameObject.SetActive(t.name == face || face == "all");
    }

    void CheckPathsFromStartTile(string face, Tile tile)
    {
        List<Tile> connectedTiles = GetConnectedTiles(faces[face], tile);
        int goal = LevelParameters.Instance.guaranteedPathsFromFirstTile + 1; // plus one since the path to home tile is counted also
        if (connectedTiles.Count < goal)
        {
            int additionalTilesNeeded = goal - connectedTiles.Count;
            for (int i = 0; i < additionalTilesNeeded; i++)
            {
                OpenRandomPath(face, tile);
            }
        }
    }

    void OpenRandomPath(string face, Tile tile)
    {
        while (true)
        {
            int direction = Random.Range(TOP_LEFT, BOTTOM_RIGHT + 1);
            Tile tileInThatDirection = GetTilesNeighbour(faces[face], tile, direction, true);
            if (tileInThatDirection != null)
            {
                if (!AreTilesConnected(faces[face], tile, direction, true))
                {
                    OpenPathBetweenTiles(tile, tileInThatDirection, direction);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// this will invisibly open a path between two tiles... might need to allow this
    /// to be visible for certain situations eg. roping between tiles, destroying rockage, etc
    /// </summary>
    /// <param name="startTile"></param>
    /// <param name="endTile"></param>
    /// <param name="directionFromStart"></param>
    public void OpenPathBetweenTiles(Tile startTile, Tile endTile, int directionFromStart)
    {
        Debug.Log("Opening " +startTile.face +" path between " + startTile.y + "/" + startTile.x + " and " + endTile.y + "/" + endTile.x 
                        +" ... opening start tile's " +GetStringForDirection(directionFromStart).ToUpper() +" and opening end tile's " +GetStringForDirection(GetOpposingDirection(directionFromStart)).ToUpper());
        startTile.openPaths[directionFromStart] = 1;
        endTile.openPaths[GetOpposingDirection(directionFromStart)] = 1;
    }

    public Tile GetStartPositionTile(List<Tile> faceTiles)
    {
        int middleTileInt = (int)GetNumTilesOnLevel(0) / 2;
        foreach (Tile tile in faceTiles)
        {
            if (tile.x == middleTileInt)
                return tile;
        }
        return null;
    }

    public List<Tile> GetPathBetweenTiles(string face, Tile startTile, Tile endTile)
    {
        // operate on a copy of the face list so we aren't updating tile distance values in real time
        // which could possibly be a conflict in multiplayer in the future... 
        // also saves the need to reset all tile distances after running this method
        List<Tile> tilesCopy = new List<Tile>();
        foreach (Tile t in faces[face])
            tilesCopy.Add(t.DeepCopy());

        bool addStartTile = false;
        if (startTile == null)
        {
            addStartTile = true;
            startTile = GetHomeBaseTile(faces[face]);
            //startTile = GetStartPositionTile(faces[face]);
        }

        int startX = endTile.x;
        int startY = endTile.y;

        GetTileAtCoordinate(tilesCopy, startX, startY).distanceSteps = 0;

        while (true)
        {
            bool progressMade = false;

            // look at each tile on face
            foreach (Tile tile in tilesCopy)
            {
                // if this tile has not been revealed and has no path to any revealed tiles, skip it
                // ?? and is not connected to endtile?
                if (!tile.revealed && !IsTileConnectedToRevealedTile(tilesCopy, tile))
                    continue;

                // if this tile has not been revealed and is not the end tile clicked on
                //if (!tile.revealed && tile != endTile)
                //    continue;

                int x = tile.x;
                int y = tile.y;

                int passHere = tile.distanceSteps;

                foreach (Tile innerTile in GetConnectedTiles(tilesCopy, tile))
                {
                    int newX = innerTile.x;
                    int newY = innerTile.y;
                    int newPass = passHere + 1;

                    if (innerTile.distanceSteps > newPass)
                    {
                        innerTile.distanceSteps = newPass;
                        progressMade = true;
                    }
                }
            }
            if (!progressMade)
                break;
        }

        List<Tile> pathFound = new List<Tile>();
        int pointX = startTile.x;
        int pointY = startTile.y;

        while (true)
        {
            Vector2 lowestPoint = Vector2.zero;
            int lowest = 100;
            foreach (Tile tile in GetConnectedTiles(tilesCopy, GetTileAtCoordinate(tilesCopy, pointX, pointY)))
            {
                int count = tile.distanceSteps;
                if (count < lowest)
                {
                    lowest = count;
                    lowestPoint.x = tile.x;
                    lowestPoint.y = tile.y;
                }
            }
            if (lowest != 100)
            {
                pathFound.Add(GetTileAtCoordinate(faces[face], (int)lowestPoint.x, (int)lowestPoint.y));
                pointX = (int)lowestPoint.x;
                pointY = (int)lowestPoint.y;
            }
            else
                break;

            if (GetTileAtCoordinate(faces[face], pointX, pointY) == endTile)
            {
                if (addStartTile)
                    pathFound.Insert(0, startTile);
                return pathFound;
            }
        }
        return null;
    }

    public List<Tile> GetConnectedTiles(List<Tile> tilesOnFace, Tile tile, List<Tile> excludedTiles = null)
    {
        List<Tile> foundTiles = new List<Tile>();

        // special case for bottom tile
        if (tile == GetHomeBaseTile(tilesOnFace))
        {
            foundTiles.Add(GetStartPositionTile(tilesOnFace));
            return foundTiles;
        }

        for (int i = TOP_LEFT; i <= BOTTOM_RIGHT; i++)
        {
            Tile neighbour = GetTilesNeighbour(tilesOnFace, tile, i, true);
            if (neighbour != null)
            {
                // now check if they are connected
                if (AreTilesConnected(tilesOnFace, tile, i, true) && (excludedTiles == null || (excludedTiles != null && !excludedTiles.Contains(neighbour))))
                    foundTiles.Add(neighbour);
            }
        }

        // another special case for bottom tile
        if (tile == GetStartPositionTile(tilesOnFace))
        {
            Tile homeBaseTile = GetHomeBaseTile(tilesOnFace);
            if (homeBaseTile != null)
                foundTiles.Add(homeBaseTile);
        }

        return foundTiles;
    }

    public Tile GetHomeBaseTile(List<Tile> fromList)
    {
        foreach (Tile tile in fromList)
            if (tile.x == -1)
                return tile;

        return null;
    }

    bool IsTileConnectedToRevealedTile(List<Tile> tilesOnFace, Tile hiddenTile)
    {
        List<Tile> connectedTiles = GetConnectedTiles(tilesOnFace, hiddenTile);
        bool isConnected = false;
        foreach (Tile foundTile in connectedTiles)
        {
            if (foundTile.revealed)
            {
                isConnected = true;
                break;
            }
        }
        return isConnected;
    }

    bool AreTilesConnected(List<Tile> tilesOnFace, Tile startTile, int directionFromStart, bool singleFace)
    {
        if (startTile.openPaths[directionFromStart] == 1)
        {
            if (GetTilesNeighbour(tilesOnFace, startTile, directionFromStart, singleFace).openPaths[GetOpposingDirection(directionFromStart)] == 1)
                return true;
        }
        return false;
    }

    public int GetOpposingDirection(int direction)
    {
        if (direction == TOP_LEFT)
            return BOTTOM_RIGHT;
        else if (direction == TOP_RIGHT)
            return BOTTOM_LEFT;
        else if (direction == LEFT)
            return RIGHT;
        else if (direction == RIGHT)
            return LEFT;
        else if (direction == BOTTOM_LEFT)
            return TOP_RIGHT;
        else
            return TOP_LEFT;
    }

    public string GetStringForDirection(int direction)
    {
        string directionString = "";
        switch (direction)
        {
            case TOP_LEFT:
                directionString = "Top-Left";
                break;
            case TOP_RIGHT:
                directionString = "Top-Right";
                break;
            case LEFT:
                directionString = "Left";
                break;
            case RIGHT:
                directionString = "Right";
                break;
            case BOTTOM_LEFT:
                directionString = "Bottom-Left";
                break;
            case BOTTOM_RIGHT:
                directionString = "Bottom-Right";
                break;
        }
        return directionString;
    }

    public int GetFaceIndex(string face)
    {
        for (int i = 0; i < faceTransforms.Length; i++)
        {
            if (faceTransforms[i].name == face)
                return i;
        }

        return 0;
    }

    public string GetFaceNameByIndex(int index)
    {
        return faceTransforms[index].name;
    }

    public string GetNextFaceInDirection(string face, int direction)
    {
        // assign intial index
        int initialFaceIndex = GetFaceIndex(face);
        //for (int i = 0; i < faceTransforms.Length; i++)
        //{
        //    if (faceTransforms[i].name == face)
        //    {
        //        initialFaceIndex = i;
        //        break;
        //    }
        //}

        // get the left or right based on arguments
        if (initialFaceIndex == 0 && direction == LEFT)
            return faceTransforms[faceTransforms.Length - 1].name;
        else if (initialFaceIndex == faceTransforms.Length - 1 && direction == RIGHT)
            return faceTransforms[0].name;
        else
            return direction == LEFT ? faceTransforms[initialFaceIndex - 1].name : faceTransforms[initialFaceIndex + 1].name;
    }

    public Tile GetTilesNeighbour(List<Tile> tilesOnFace, Tile tile, int direction, bool singleFace)
    {
        int level = tile.y;
        int pos = tile.x;

        if (direction == TOP_LEFT)
        {
            return GetTileAtCoordinate(tilesOnFace, pos - 1, level + 1);
        }
        else if (direction == TOP_RIGHT)
        {
            return GetTileAtCoordinate(tilesOnFace, pos, level + 1);
        }
        else if (direction == LEFT)
        {
            if (pos == 0 && !singleFace)
            {
                string previousFace = GetNextFaceInDirection(tile.face, LEFT);
                return GetTileAtCoordinate(faces[previousFace], GetNumTilesOnLevel(level), level);
            }
            else
                return GetTileAtCoordinate(tilesOnFace, pos - 1, level);
        }
        else if (direction == RIGHT)
        {
            if (pos == GetNumTilesOnLevel(level) && !singleFace)
            {
                string nextFace = GetNextFaceInDirection(tile.face, RIGHT);
                return GetTileAtCoordinate(faces[nextFace], 0, level);
            }
            else
                return GetTileAtCoordinate(tilesOnFace, pos + 1, level);
        }
        else if (direction == BOTTOM_LEFT)
        {
            return GetTileAtCoordinate(tilesOnFace, pos, level - 1);
        }
        else // BOTTOM_RIGHT
        {
            return GetTileAtCoordinate(tilesOnFace, pos + 1, level - 1);
        }
    }

	public Tile GetClosestTile(string side, Vector3 fromPosition)
    {
        Tile closestTile = new Tile();
        float closestDistance = Mathf.Infinity;
        foreach (Tile tile in faces[side])
        {
            float distance = Vector3.Distance(fromPosition, tile.tileTransform.position);
            if (distance < closestDistance)
            {
                closestTile = tile;
                closestDistance = distance;
            }
        }
        return closestTile;
    }

    public float GetRotationValueForFace(string face)
    {
        float angle = 0;
        float facesCount = faceTransforms.Length;
        for (int i = 0; i < facesCount; i++)
        {
            if (faceTransforms[i].name == face)
            {
                angle = (faces.Count - i) * (360 / facesCount);
                break;
            }
        }
        if (Mathf.Approximately(angle, 360))
            return 0;

        return angle;
    }

    public void ArrivedAtTile(Character character, Tile tile)
    {
        if (character is Player)
        {
            Player player = character as Player;
            // check if we need to display the change face arrows
            player.ToggleChangeFaceArrow(LEFT, IsTileOnEdgeOfFace(tile) && tile.x == 0 && AreTilesConnected(faces[tile.face], tile, LEFT, false));
            player.ToggleChangeFaceArrow(RIGHT, IsTileOnEdgeOfFace(tile) && tile.x == GetNumTilesOnLevel(tile.y) && AreTilesConnected(faces[tile.face], tile, RIGHT, false));
        }

        if (character.currentFace != tile.face)
            character.currentFace = tile.face;

        // update inhabitants of tile
        tile.inhabitants.Add(character);






        // update our network character's tile details on other players local machines
        int[] detailArray = new int[] { PhotonNetwork.player.ID, GetFaceIndex(character.currentFace), tile.x, tile.y };
        character.networkCharacter.photonView.RPC("ChangedTile", PhotonTargets.Others, detailArray);






        // reveal tile and instantiate any objects if necessary
        if (!tile.revealed)
        {
            // display all paths leading from the arrived at tile
            tile.DisplayPaths(true);
            tile.revealed = true;
            tile.sprite.color = Color.white;

            switch (tile.property)
            {
                case Tile.TileProperty.minerals:
                    tile.propertyObject = (GameObject)Instantiate(mineralPrefab, tile.tileTransform.position + tilePropertyOffset, mineralPrefab.transform.rotation);
                    //tile.propertyObject.GetComponent<ColliderDisplayText>().myType = tile;
                    break;

                case Tile.TileProperty.monsters:
                    if (tile.propertyObject == null)
                        SpawnMonster(tile);
                    break;
            }
            // parent the object if we created one so it will hide with the face
            if (tile.propertyObject != null)
            {
                foreach (Transform t in faceTransforms)
                    if (tile.face == t.name)
                        tile.propertyObject.transform.parent = t;
            }
        }

        foreach (Character c in tile.inhabitants)
            c.UpdateAttackState();
    }

    bool IsTileOnEdgeOfFace(Tile tile)
    {
        if (tile.x == 0 || tile.x == GetNumTilesOnLevel(tile.y))
            return true;
        return false;
    }

    /// <summary>
    /// this method assumes tiles ARE CONNECTED and will fail if not. check connectivity before calling.
    /// </summary>
    /// <returns>the direction from startTile toward endTile</returns>
    int GetDirectionFromTileToTile(Tile startTile, Tile endTile)
    {
        if (startTile.y > endTile.y)        // start tile is HIGHER than end tile
        {
            if (startTile.x == endTile.x)   // end tile is LEFT of start tile
                return BOTTOM_LEFT;
            else                            // end tile is RIGHT of start tile
                return BOTTOM_RIGHT;
        }
        else if (startTile.y == endTile.y)  // tiles are on the SAME LEVEL
        {
            if (startTile.x < endTile.x)    // end tile is RIGHT of start tile
                return RIGHT;
            else                            // end tile is LEFT of start tile
                return LEFT;
        }
        else                                // start tile is LOWER than end tile
        {
            if (startTile.x == endTile.x)   // end tile is RIGHT of start tile
                return TOP_RIGHT;
            else                            // end tile is LEFT of start tile
                return TOP_LEFT;
        }
    }

    public int GetMountainHeight()
    {
        int heighestTile = -1;
        foreach (Tile tile in faces[faceTransforms[0].name])
        {
            int tileHeight = tile.y;
            if (tileHeight > heighestTile)
                heighestTile = tileHeight;                
        }
        return heighestTile;
    }

    public Tile GetTileAtCoordinate(List<Tile> tilesOnFace, int x, int y)
    {
        foreach (Tile tile in tilesOnFace)
        {
            if (x == tile.x && y == tile.y)
                return tile;
        }
        return null;
    }

    public int GetNumTilesOnLevel(int level)
    {
        int count = 0;
        // doesn't matter which face since all faces are to be symetrical, so just get the first one
        string faceKey = faceTransforms[0].name;
        foreach (Tile tile in faces[faceKey])
        {
            if (tile.y == level)
                count++;
        }
        return count-1;
    }

    private void GeneratePaths(string faceKey)
    {
        foreach (string keyString in faces.Keys)
        {
            foreach (Tile tile in faces[keyString])
            {
                tile.openPaths = GenerateTilePaths();
                if (tile.y == 0)
                    CloseBottomTilePaths(tile);
                SecondChanceToMatchPaths(tile, faceKey);
            }
        }
    }

    int[] GenerateTilePaths()
    {
        int TL = Random.Range(0, 100) < LevelParameters.Instance.probabilityOfTilePaths ? 1 : 0;
        int TR = Random.Range(0, 100) < LevelParameters.Instance.probabilityOfTilePaths ? 1 : 0;
        int L = Random.Range(0, 100) < LevelParameters.Instance.probabilityOfTilePaths ? 1 : 0;
        int R = Random.Range(0, 100) < LevelParameters.Instance.probabilityOfTilePaths ? 1 : 0;
        int BL = Random.Range(0, 100) < LevelParameters.Instance.probabilityOfTilePaths ? 1 : 0;
        int BR = Random.Range(0, 100) < LevelParameters.Instance.probabilityOfTilePaths ? 1 : 0;

        int[] newPaths = new int[]
        {
              TL, TR,
            L,       R,
              BL, BR
        };
        return newPaths;
    }
    void CloseBottomTilePaths(Tile tile)
    {
        for (int i = BOTTOM_LEFT; i <= BOTTOM_RIGHT; i++)
            tile.openPaths[i] = 0;
    }
    /// <summary>
    /// iterate through all touching tiles on this face and give another 'probabilityOfJoinedTiles' chance
    /// to generate a matching path on this one if the way is blocked
    /// </summary>
    /// <param name="tile">the tile to perform this on</param>
    void SecondChanceToMatchPaths(Tile tile, string faceKey)
    {
        int level = tile.y;
        int pos = tile.x;

        if (level == -1 || pos == -1)
            return;

        List<Tile> tilesOnFace = faces[faceKey];

        // start with tiles beneath
        if (level != 0)
        {
            // get tile one level below and to left...
            Tile bottomLeft = GetTileAtCoordinate(tilesOnFace, pos, level - 1);
            if (bottomLeft.openPaths[TOP_RIGHT] == 1)
                if (Random.Range(1, 100) < LevelParameters.Instance.extraProbabilityOfJoinedTiles)
                    tile.openPaths[BOTTOM_LEFT] = 1;
            //... then below and to right
            Tile bottomRight = GetTileAtCoordinate(tilesOnFace, pos + 1, level - 1);
            if (bottomRight.openPaths[TOP_LEFT] == 1)
                if (Random.Range(1, 100) < LevelParameters.Instance.extraProbabilityOfJoinedTiles)
                    tile.openPaths[BOTTOM_RIGHT] = 1;
        }

        

        // now tiles above
        if (level != GetMountainHeight())
        {
            if (pos != 0)
            {
                // tile above and left
                Tile topLeft = GetTileAtCoordinate(tilesOnFace, pos - 1, level + 1);
                if (topLeft.openPaths[BOTTOM_RIGHT] == 1)
                    if (Random.Range(1, 100) < LevelParameters.Instance.extraProbabilityOfJoinedTiles)
                        tile.openPaths[TOP_LEFT] = 1;
            }
            if (pos != GetNumTilesOnLevel(level))
            {
                // tile above and right
                Tile topRight = GetTileAtCoordinate(tilesOnFace, pos, level + 1);
                if (topRight.openPaths[BOTTOM_LEFT] == 1)
                    if (Random.Range(1, 100) < LevelParameters.Instance.extraProbabilityOfJoinedTiles)
                        tile.openPaths[TOP_RIGHT] = 1;
            }
        }

        // now tiles left and right...
        if (pos != 0)
        {
            // tile to the left
            Tile left = GetTileAtCoordinate(tilesOnFace, pos - 1, level);
            if (left.openPaths[RIGHT] == 1)
                if (Random.Range(1, 100) < LevelParameters.Instance.extraProbabilityOfJoinedTiles)
                    tile.openPaths[LEFT] = 1;
        }
        if (pos != GetNumTilesOnLevel(level))
        {
            // tile to the right
            Tile right = GetTileAtCoordinate(tilesOnFace, pos + 1, level);
            if (right.openPaths[LEFT] == 1)
                if (Random.Range(1, 100) < LevelParameters.Instance.extraProbabilityOfJoinedTiles)
                    tile.openPaths[RIGHT] = 1;
        }
    }
}
