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
        Explorer = 3,
        BlockageClearer = 4,
        CorpseCollector = 5
    }
    public MuleType muleType;

    // a list of the items this mule is carrying
    public List<Item> itemsOnMule = new List<Item>();

    private Player _owner;
    private bool _returningHome;

    // various speed and efficiency packmules vars, taken from the player
    private float _resourceGatheringTime;
    private bool _gathering;
    public int currentMinerals;
    private int _totalMineralsThisMine;
    private float _timer;
    private GameObject _carrying;

    void Update()
    {
        // remember to call the base class update
        base.Update();

        if (_gathering)
        {
            _timer += Time.deltaTime;

            int previous = currentMinerals;
            float tParam = Mathf.InverseLerp(0, _resourceGatheringTime, _timer);
            currentMinerals = Mathf.RoundToInt(tParam * (float)_totalMineralsThisMine);
            closestTile.MineTile(currentMinerals - previous);

            if (_timer > _resourceGatheringTime)
            {
                _gathering = false;
                //closestTile.propertyObject.GetComponent<ColliderDisplayText>().ToggleForcedState(false);
                ReturnHome();
            }
        }
    }

    public override bool ShouldDisplayPaths()
    {
        return _owner.ShouldDisplayPaths();
    }

    public override void CollectItems()
    {
        if (muleType == MuleType.CorpseCollector)
        {
            Monster deadMonster = closestTile.currentMonster;
            // generate a corpse item
            ItemDataBaseList itemList = (ItemDataBaseList)Resources.Load("ItemDatabase");
            Item corpse = itemList.getItemByName("corpse");
            corpse.monsterReference = deadMonster;
            itemsOnMule.Add(corpse);
            _carrying = deadMonster.gameObject;
            _carrying.transform.parent = transform;
            _carrying.transform.localPosition = Vector3.zero;
            ReturnHome();
        }
    }

    public override GameObject GetFinalPathPoint()
    {
        if (muleType == MuleType.CorpseCollector || muleType == MuleType.ResourceGatherer)
            return movement.GetTargetTile().propertyObject;

        return null;
    }

    public void DepositItems()
    {
        // deposit any cash on the mule
        _owner.DepositCash(currentMinerals);

        foreach (Item item in itemsOnMule)
        {
            if (item.itemType == ItemType.Corpse)
            {
                Debug.Log("Tell tile " + item.monsterReference.closestTile.x + "-" + item.monsterReference.closestTile.y + " to regenerate");
                item.monsterReference.closestTile.MonsterReturned();
            }
        }
    }

    public void SetMuleType(Player owner, Tile tileGoal, MuleType forcedType = MuleType.Null)
    {
        _owner = owner;
        if (forcedType != MuleType.Null)
        {
            muleType = forcedType;
            return;
        }

        Tile.TileProperty property = tileGoal.property;
        if (property == Tile.TileProperty.minerals)
        {
            muleType = MuleType.ResourceGatherer;
            _resourceGatheringTime = _owner.packmuleResourceGatheringTime;
        }
        else if (property == Tile.TileProperty.empty)
        {
            muleType = MuleType.Explorer;
        }
    }

    public override void GoalReached()
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
                    //closestTile.propertyObject.GetComponent<ColliderDisplayText>().ToggleForcedState(true);
                    break;
                case MuleType.Explorer:
                    ReturnHome();
                    break;
                //case MuleType.BlockageClearer:
                //    actionToProcess = ActionType.clearBlockage;
                //    break;
                case MuleType.CorpseCollector:
                    actionToProcess = ActionType.collect;
                    break;
            }
        }

        if (homeThisTurn)
            FinishedJourney();
    }

    public override void ReturnHome()
    {
        _returningHome = true;
        movement.ClickedOnTile(Mountain.Instance.GetHomeBaseTile(Mountain.Instance.faces[currentFace]));
    }

    void FinishedJourney()
    {
        DepositItems();
        _owner.currentPackmules.Remove(this);
        _owner.packmulesWaiting++;
        NetworkHandler.Instance.ownedCharacters.Remove(this);
        Destroy(gameObject);
    }

}
