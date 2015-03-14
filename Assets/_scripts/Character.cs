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
        movement = 1,
        clearBlockage = 2
    }

    public Movement movement;

    // tile vars
    public Tile closestTile;
    public string currentFace = "";

    // speed / rest rate
    public float speed;
    public float restInMilliseconds = 2000;
    protected float _turnTimer;
    public GameObject objectToAction;

    // various efficiency settings
    public int turnsToClearBlockage = 3;

    // current action
    public ActionType actionToProcess;

    public virtual bool ShouldDisplayPaths() { return true; }
    public virtual void ReturnHome() {}

    private int _turnCounter = 0;

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

            case ActionType.clearBlockage:
                if (objectToAction == null)
                {
                    Debug.LogError("character has clear blockage action but objectToAction is null!");
                    return;
                }
                _turnCounter++;
                if (_turnCounter == turnsToClearBlockage)
                {
                    Blockage blockage = objectToAction.GetComponent<Blockage>();
                    if (blockage != null)
                        Mountain.Instance.OpenPathBetweenTiles(blockage.tileOne, blockage.tileTwo, blockage.directionFromTileOne);
                    Destroy(objectToAction);
                    objectToAction = null;
                    _turnCounter = 0;
                    if (this is Packmule)
                        ReturnHome();
                }
                maintainState = true;
                break;
        }
        if (!maintainState)
            actionToProcess = ActionType.none;
    }
}
