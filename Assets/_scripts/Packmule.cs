using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

// a basic extension of the base Character class which mainly walks between
// supplied targets and performs an action
public class Packmule : Character {

    [System.Serializable]
    public enum MuleType
    {
        Null = 0,
        ResourceGatherer = 1,
        ItemDeliverer = 2,
        Explorer = 3
    }
    public MuleType muleType;

    private Player _owner;
    private bool _returningHome;

    // various speed and efficiency packmules vars, taken from the player
    private float _resourceGatheringTime;
    private bool _gathering;
    public int _currentMinerals;
    private int _totalMineralsThisMine;
    private float _timer;

    void Update()
    {
        // remember to call the base class update
        base.Update();

        if (_gathering)
        {
            _timer += Time.deltaTime;

            int previous = _currentMinerals;
            float tParam = Mathf.InverseLerp(0, _resourceGatheringTime, _timer);
            _currentMinerals = Mathf.RoundToInt(tParam * (float)_totalMineralsThisMine);
            closestTile.MineTile(_currentMinerals - previous);

            if (_timer > _resourceGatheringTime)
            {
                _gathering = false;
                closestTile.propertyObject.GetComponent<ColliderDisplayText>().ToggleMiningState(false);
                ReturnHome();
            }
        }
    }

    public override bool ShouldDisplayPaths()
    {
        return _owner.ShouldDisplayPaths();
    }

    public void SetMuleType(Player owner, Tile tileGoal, MuleType forcedType = MuleType.Null)
    {
        _owner = owner;

        Tile.TileProperty property = tileGoal.property;
        if (property == Tile.TileProperty.minerals || forcedType == MuleType.ResourceGatherer)
        {
            muleType = MuleType.ResourceGatherer;
            _resourceGatheringTime = _owner.packmuleResourceGatheringTime;
        }
        else if (property == Tile.TileProperty.empty || forcedType == MuleType.Explorer)
        {
            muleType = MuleType.Explorer;
        }
    }

    public void GoalReached()
    {
        bool homeThisTurn = _returningHome;

        if (!homeThisTurn)
        {
            switch (muleType)
            {
                case MuleType.ResourceGatherer:
                    _timer = 0;
                    _totalMineralsThisMine = closestTile.GetMineAmount();
                    _gathering = true;
                    closestTile.propertyObject.GetComponent<ColliderDisplayText>().ToggleMiningState(true);
                    break;
                case MuleType.Explorer:
                    ReturnHome();
                    break;
            }
        }

        if (homeThisTurn)
            FinishedJourney();
    }

    public void ReturnHome()
    {
        _returningHome = true;
        movement.ClickedOnTile(Mountain.Instance.GetHomeBaseTile(Mountain.Instance.faces[currentFace]));
    }

    void FinishedJourney()
    {
        _owner.currentPackmules.Remove(this);
        _owner.packmulesWaiting++;
        if (muleType == MuleType.ResourceGatherer)
            _owner.DepositCash(_currentMinerals);
        Destroy(gameObject);
    }

}
