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
        combat.EquipItem(inventoryItemList.getItemByName(rightHandItemName).getCopy(), true);
        combat.EquipItem(inventoryItemList.getItemByName(leftHandItemName).getCopy(), false);
    }

    public void LevelUp(int level)
    {
        combat.SetLife(combat.maxLife + (combat.maxLife * (level / 2)));

        if (combat.rightHandItem != null)
        {
            //for (int i = 1; i < level; i++)
            //    combat.rightHandItem = combat.UpgradeWeapon(combat.rightHandItem);
        }
        if (combat.leftHandItem != null)
        {
            //for (int i = 1; i < level; i++)
            //    combat.leftHandItem = combat.UpgradeWeapon(combat.leftHandItem);
        }
    }
}
