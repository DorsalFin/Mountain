using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {

    public int currentCash;

    public List<Item> itemsOnPerson = new List<Item>();
    public List<Item> itemsAtBase = new List<Item>();

    void Start()
    {
        currentCash = LevelParameters.Instance.startingCash;
    }

    public void AddToInventory(Item item)
    {

    }

}
