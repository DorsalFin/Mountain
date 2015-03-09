using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {

    // TODO: put this into one of the other inventory scripts, probably InvEquipment


    private Player _player;
    public int currentCash;

    private int _itemSlotsAtBase = 4;

    void Start()
    {
        _player = GetComponent<Player>();
        currentCash = LevelParameters.Instance.startingCash;
    }

}
