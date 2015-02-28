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
        _pathLine = new VectorLine("pathLine", new List<Vector3>(), pathLineMaterial, 10.0f, LineType.Continuous, Joins.Fill);
        _pathLine.textureScale = 1.0f;
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

    void Update()
    {
        // movement
        if (_pathToTargetTile.Count > 0 || _upcomingPathToTargetTile.Count > 0)
        {
            // update the points of the movement path line and draw it
            List<Vector3> pathlinePoints = new List<Vector3>();
            pathlinePoints.Add(transform.position);
            if (_upcomingPathToTargetTile.Count > 0)
            {
                foreach (Tile t in _upcomingPathToTargetTile)
                    pathlinePoints.Add(t.tileTransform.position);
            }
            else
            {
                foreach (Tile t in _pathToTargetTile)
                    pathlinePoints.Add(t.tileTransform.position);
            }
            _pathLine.points3.Clear();
            _pathLine.points3.AddRange(pathlinePoints);
            //_pathLine.endCap = "endCap";
            _pathLine.Draw();
            if (!_pathLine.active)
                _pathLine.active = true;

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
                if (distanceToTargetTile < 0.05f)
                {
                    isMoving = false;
                    ProgressMovementOnPath();
                }
            }
        }
        else
        {
            if (_pathLine.active)
                _pathLine.active = false;
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

    void ProgressMovementOnPath()
    {
        _pathToTargetTile.RemoveAt(0);
        if (_pathToTargetTile.Count > 0)
            character.actionToProcess = Character.ActionType.movement;
        else
        {
            // destination reached
            if (character is Packmule)
            {
                Packmule packmule = character as Packmule;
                packmule.GoalReached();
            }
        }
    }

    public void ClickedOnTile(Tile clickedOnTile)
    {
        List<Tile> path = Mountain.Instance.GetPathBetweenTiles(character.currentFace, character.closestTile, clickedOnTile);
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
