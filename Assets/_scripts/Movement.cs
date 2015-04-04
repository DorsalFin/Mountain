using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class Movement : MonoBehaviour {

    public Character character;

    public List<Tile> _pathToTargetTile = new List<Tile>();
    public List<Tile> _upcomingPathToTargetTile = new List<Tile>();
    public Material pathLineMaterial;
    public bool isMoving;
    public int rotatingFaceInDirection = 0;

    private VectorLine _pathLine;
    private bool _generatingPath;

    void Start()
    {
        _pathLine = new VectorLine("pathLine", new List<Vector3>(), pathLineMaterial, 6.0f, LineType.Continuous, Joins.Fill);
        _pathLine.textureScale = 2.0f;
    }       

    public void ActionMovement()
    {
        if (_generatingPath)
        {
            _generatingPath = false;
            _pathToTargetTile.Clear();
            _pathToTargetTile.AddRange(_upcomingPathToTargetTile);
            _upcomingPathToTargetTile.Clear();
        }
        isMoving = true;
    }

    public Tile GetTargetTile()
    {
        if (_upcomingPathToTargetTile.Count == 0 && _pathToTargetTile.Count == 0)
            return character.closestTile;
        else return _upcomingPathToTargetTile.Count > 0 ? _upcomingPathToTargetTile[_upcomingPathToTargetTile.Count - 1] : _pathToTargetTile[_pathToTargetTile.Count - 1];
    }

    /// <summary>
    /// get the next tile the character is moving to, will return closest tile if none in path
    /// </summary>
    public Tile GetNextTile()
    {
        if (_pathToTargetTile.Count > 0 && isMoving)
            return _pathToTargetTile[0];
        else if (_pathToTargetTile.Count > 0 && !isMoving)
            return character.closestTile;

        // i shouldn't ever have to check upcoming path since it is wiped as soon as isMoving becomes true
        return character.closestTile;
    }

    public void SetNextTileAsTarget()
    {
        _generatingPath = false;
        _upcomingPathToTargetTile.Clear();
        _pathToTargetTile.RemoveRange(1, _pathToTargetTile.Count - 1);
    }

    void Update()
    {
        // movement
        if (_pathToTargetTile.Count > 0 || _upcomingPathToTargetTile.Count > 0) //|| character.objectToAction != null)
        {
            // update the path visual representation
            UpdatePath();

            if (isMoving)
            {
                Tile tileTarget = _pathToTargetTile[0];
                transform.position = Vector3.MoveTowards(transform.position, tileTarget.tileTransform.position, character.speed);
                float distanceToTargetTile = Vector3.Distance(transform.position, tileTarget.tileTransform.position);

                // FURTHEST DISTANCE ///// update closestTile //////////////
                if (distanceToTargetTile < LevelParameters.Instance.distanceToBeConsideredClosest)
                {
                    if (character is Player)
                    {
                        Player player = character as Player;
                        if (player.closestTile != null && player.closestTile.face != "" && player.closestTile.face != tileTarget.face)
                            player.ChangeFaceFocus(rotatingFaceInDirection);
                    }

                    character.closestTile = tileTarget;
                }

                // MIDDLE DISTANCE ///// reveal tile ///////////////////////
                if (distanceToTargetTile < LevelParameters.Instance.minDistanceToRevealTile)
                    Mountain.Instance.ArrivedAtTile(character, tileTarget);

                // CLOSEST DISTANCE ///// set _moving state ////////////////
                if (distanceToTargetTile < 0.025f)
                {
                    isMoving = false;
                    ProgressMovementOnPath();
                }
            }
        }

        if (character is Player)
        {
            if (_generatingPath)
            {
                if (character.closestTile != null && character.closestTile != _upcomingPathToTargetTile[0] && character.closestTile.face == _upcomingPathToTargetTile[0].face)
                {
                    Player player = character as Player;
                    if (player.clickedOnTile != null)
                    {
                        List<Tile> path = Mountain.Instance.GetPathBetweenTiles(character.currentFace, character.closestTile, player.clickedOnTile);
                        if (path != null)
                            _upcomingPathToTargetTile = path;
                    }
                }
            }
        }
    }

    public void UpdatePath()
    {
        // refresh points of path
        List<Vector3> pathlinePoints = new List<Vector3>();

        // add the character position to start our line
        pathlinePoints.Add(transform.position);

        // populate with the appropriate path list
        // if we have an UPCOMING PATH then we draw that
        // if we DON'T have an upcoming path draw the path we are currently moving on
        if (_upcomingPathToTargetTile.Count > 0)
        {
            // this path should always contain the tile we are currently moving toward
            // at the start
            if (_pathToTargetTile.Count > 0)
            {
                Tile currentTargetTile = _pathToTargetTile[0];
                if (isMoving && !_upcomingPathToTargetTile.Contains(currentTargetTile))
                {
                    // the upcoming path we are about to draw does not have the 
                    // tile we are currently moving toward, so let's add it straight
                    // after the character's transform
                    pathlinePoints.Add(currentTargetTile.tileTransform.position);
                }
            }

            foreach (Tile t in _upcomingPathToTargetTile)
                pathlinePoints.Add(t.tileTransform.position);
        }
        else
        {
            foreach (Tile t in _pathToTargetTile)
                pathlinePoints.Add(t.tileTransform.position);
        }

        // add the object to action to the end of the list
        //if (character.objectToAction != null)
        //    pathlinePoints.Add(character.objectToAction.transform.position);

        _pathLine.points3.Clear();
        _pathLine.points3.AddRange(pathlinePoints);
        //_pathLine.endCap = "endCap";
        _pathLine.Draw3D();
    }

    public void ClearPaths(bool update)
    {
        _generatingPath = false;
        _upcomingPathToTargetTile.Clear();
        _pathToTargetTile.Clear();

        if (update) UpdatePath();
    }

    void ProgressMovementOnPath()
    {
        _pathToTargetTile.RemoveAt(0);
        if (_pathToTargetTile.Count > 0)
        {
            if (_generatingPath)
            {
                _generatingPath = false;
                _pathToTargetTile.Clear();
                _pathToTargetTile.AddRange(_upcomingPathToTargetTile);
                _upcomingPathToTargetTile.Clear();
            }
            character.actionToProcess = Character.ActionType.movement;
        }
        else
        {
            if (GetTargetTile() == character.closestTile)
                character.GoalReached();
        }
    }

    public void ClickedOnTile(Tile clickedOnTile)
    {
        // from closest tile if we aren't moving, otherwise from the destination tile
        Tile startTile = GetNextTile();

        List<Tile> path = Mountain.Instance.GetPathBetweenTiles(character.currentFace, startTile, clickedOnTile);
        if (path != null)
        {
            _upcomingPathToTargetTile = path;
            _generatingPath = true;
            character.actionToProcess = Character.ActionType.movement;
        }
    }

    public void ChangeFace(GameObject arrow)
    {
        int direction = arrow.name.Contains("right") ? Mountain.RIGHT : Mountain.LEFT;
        rotatingFaceInDirection = direction;
        //playerCamera.ChangeFaceFocus(direction);

        Tile correspondingTile = Mountain.Instance.GetTilesNeighbour(null, character.closestTile, direction, false);
        _upcomingPathToTargetTile.Clear();
        //_upcomingPathToTargetTile.Add(closestTile);
        _upcomingPathToTargetTile.Add(correspondingTile);
        _generatingPath = true;
        character.actionToProcess = Character.ActionType.movement;
    }
}
