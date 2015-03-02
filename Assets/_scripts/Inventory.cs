using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {

    private Player _player;

    public int currentCash;

    public Dictionary<Item, int> itemsOnPerson = new Dictionary<Item, int>();
    public Dictionary<Item, int> itemsAtBase = new Dictionary<Item, int>();

    public Weapon currentlyEquippedWeapon;

    private int _itemSlotsAtBase = 4;
    private int _itemSlotsOnPerson = 6;

    void Start()
    {
        _player = GetComponent<Player>();
        currentCash = LevelParameters.Instance.startingCash;
    }

    /// <summary>
    /// get number of unique items in one or the other backpack
    /// </summary>
    /// <param name="from">on person = "person", from base = anything else</param>
    int GetUniqueItemCount(string from)
    {
        if (from == "person")
            return itemsOnPerson.Keys.Count;
        else // (from == "base")
            return itemsAtBase.Keys.Count;
    }

    public void AddToInventory(Item item, bool onPerson)
    {
        // add the item to correct dictionary
        Dictionary<Item, int> itemList = onPerson ? itemsOnPerson : itemsAtBase;

        // first check if there are any slots left in the given list
        if (GetUniqueItemCount(onPerson == true ? "person" : "base") < (onPerson ? _itemSlotsOnPerson : _itemSlotsAtBase))
        {
            // if we DON'T own this item
            if (!itemList.ContainsKey(item))
            {
                // create a new entry
                itemList.Add(item, 1);

                
            }
        }
    }

    public void RemoveFromInventory(GameObject itemObj, bool onPerson)
    {

    }

}
