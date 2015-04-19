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
    public Combat combat;

    public bool isDead;

    public AudioClip voiceDeathClip;
    public AudioClip[] voiceHurtClips;
    public AudioClip[] voiceTriumphClips;
    public AudioSource voiceSfx;

    // tile vars
    public Tile closestTile;
    public string currentFace = "";

    // speed / rest rate
    public float speed;
    public float restInMilliseconds = 2000;
    protected float _turnTimer;

    // current action
    public ActionType actionToProcess;

    public virtual bool ShouldDisplayPaths() { return true; }
    public virtual void ReturnHome() {}
    public virtual void GoalReached() {}

    private int _turnCounter = 0;

    public void Update()
    {
        // monsters dont run this turn system
        if (this is Monster)
            return;

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

    public void UpdateAttackState()
    {
        if (combat == null || isDead)
            return;

        // TODO this currently just attacks anyone the first
        // inhabitant in the tile's list... need to make targets selective
        // i guess we just search for any other 'players' and attack
        // them as a priority for now
        Character target = null;
        foreach (Character character in closestTile.inhabitants)
        {
            if (character == this)
                continue;

            if (!character.isDead && (target == null || (character is Player)))
                target = character;
        }

        // for now not attacking anything that is un-agressive (has no combat script)
        if (target != null && target.combat != null)
            combat.EngageTarget(target);
    }

    public void PlayVoiceClip(string category)
    {
        if (voiceSfx == null)
            return;

        if (category == "hurt")
            voiceSfx.PlayOneShot(voiceHurtClips[Random.Range(0, voiceHurtClips.Length)]);
        else if (category == "death")
            voiceSfx.PlayOneShot(voiceDeathClip);
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

            // MOVEMENT ACTION ////////////////////////////////////////////////////////////////
            case ActionType.movement:
                if (!movement.isMoving)
                {
                    movement.ActionMovement();
                }
                else
                    maintainState = true;
                break;


            // CLEAR BLOCKAGE ACTION ///////////////////////////////////////////////////////////
            //case ActionType.clearBlockage:

            //    if (objectToAction == null)
            //    {
            //        if (this is Packmule)
            //            ReturnHome();
            //        else
            //            actionToProcess = ActionType.none;

            //        return;
            //    }

            //    _turnCounter++;

            //    if (_turnCounter == turnsToClearBlockage)
            //    {
            //        Blockage blockage = objectToAction.GetComponent<Blockage>();
            //        if (blockage != null)
            //            Mountain.Instance.OpenPathBetweenTiles(blockage.tileOne, blockage.tileTwo, blockage.directionFromTileOne);
            //        Destroy(objectToAction);
            //        objectToAction = null;
            //        _turnCounter = 0;
            //        UseItem();

            //        if (this is Packmule)
            //            ReturnHome();
            //        else if (this is Player)
            //        {
            //            actionToProcess = ActionType.none;
            //            movement.ClearPaths(true);
            //        }
            //    }
            //    maintainState = true;
            //    break;


        }

        if (!maintainState)
            actionToProcess = ActionType.none;
    }
}
