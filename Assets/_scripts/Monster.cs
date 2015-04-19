using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Monster : Character {

    public string rightHandItemName;
    public string leftHandItemName;

    void Awake()
    {
        // set weapons on this monster
        ItemDataBaseList inventoryItemList = (ItemDataBaseList)Resources.Load("ItemDatabase");
        combat.EquipItem(inventoryItemList.getItemByName(rightHandItemName), true);
        combat.EquipItem(inventoryItemList.getItemByName(leftHandItemName), false);
    }
}
