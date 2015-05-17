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

    public AudioClip[] footstepClips;
    public AudioSource footstepSfx;
    private float _footstepTimer;
    private bool _leftFoot;

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
                // footstep audio
                _footstepTimer -= Time.deltaTime;
                if (_footstepTimer < 0)
                {
                    // play next sound
                    footstepSfx.PlayOneShot(footstepClips[_leftFoot ? 0 : 1]);

                    // TODO fix this - it's currently getting LONGER between footsteps the faster you are
                    _footstepTimer = 0.50f * (character.speed * 100);
                }

                Tile tileTarget = _pathToTargetTile[0];
                transform.position = Vector3.MoveTowards(transform.position, tileTarget.tileTransform.position, character.speed);
                float distanceToTargetTile = Vector3.Distance(transform.position, tileTarget.tileTransform.position);

                /* 
                 * FURTHEST DISTANCE 
                 * here we update our characters closest tile and also remove ourselves
                 * from the previous tile's inhabitant list
                 */
                if (distanceToTargetTile < LevelParameters.Instance.distanceToBeConsideredClosest)
                {
                    if (character is Player)
                    {
                        Player player = character as Player;
                        if (player.closestTile != null && player.closestTile.face != "" && player.closestTile.face != tileTarget.face)
                            Mountain.Instance.SetPlayerFaceFocus(player, tileTarget.face);
                            //player.ChangeFaceFocus(tileTarget.face);//rotatingFaceInDirection);
                    }

                    // remove ourselves from the previous tiles inhabitants
                    if (character.closestTile != null)
                        character.closestTile.inhabitants.Remove(character);

                    character.closestTile = tileTarget;
                }

                /* 
                 * MIDDLE DISTANCE 
                 * here we reveal the tile and add ourselves to the tile's inhabitant
                 * list. attacking does not start until we reach the short distance
                 */
                if (distanceToTargetTile < LevelParameters.Instance.minDistanceToRevealTile)
                    Mountain.Instance.ArrivedAtTile(character, tileTarget);

                /* 
                 * SHORT DISTANCE 
                 * we can stop movement here. we want to set any attacking states neccessary
                 * and progress any other actions we have
                 */
                if (distanceToTargetTile < 0.025f)
                {
                    isMoving = false;
                    bool stillMoving = ProgressMovementOnPath();
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

        // add any end points (monsters, items, blockage, etc)
        GameObject finalPoint = character.GetFinalPathPoint();
        if (finalPoint != null)
            pathlinePoints.Add(finalPoint.transform.position);

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

    bool ProgressMovementOnPath()
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
            return true;
        }
        else
        {
            if (GetTargetTile() == character.closestTile)
                character.GoalReached();
            return false;
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
