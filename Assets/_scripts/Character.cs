using UnityEngine;
using System.Collections;

/// <summary>
/// Contains the base elements of a character that runs turns and has movement.
/// Inherited by more specialised extended classes like Player, or Packmule
/// </summary>
public class Character : MonoBehaviour {

    public enum ActionType
    {
        none = 0,
        movement = 1
    }

    public Movement movement;

    // tile vars
    public Tile closestTile;
    public string currentFace = "";

    // speed / rest rate
    public float speed;
    public float restInMilliseconds = 2000;
    protected float _turnTimer;

    // current action
    public ActionType actionToProcess;

    public void Update()
    {
#region TURN_COUNTER
        if (IsResting())
            _turnTimer += Time.deltaTime;
        if (_turnTimer * 1000 > restInMilliseconds)
        {
            if (actionToProcess != ActionType.none)
            {
                ProcessTurn();
                _turnTimer = 0;
            }
        }
#endregion
    }

    bool IsResting()
    {
        if (movement.isMoving)
            return false;

        return true;
    }

    public void ProcessTurn()
    {
        Debug.Log(gameObject.name +" process turn with " + actionToProcess.ToString() + " action");
        bool maintainState = false;
        switch (actionToProcess)
        {
            case ActionType.movement:
                if (!movement.isMoving)
                {
                    movement.ActionMovement();
                }
                else
                    maintainState = true;
                break;
        }
        if (!maintainState)
            actionToProcess = ActionType.none;
    }
}
