using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class Shop : MonoBehaviour {

    public UIGrid itemGrid;
    public GameObject shopItemPrefab;
    private string _currentlyDisplayingCategory;

    public UIButtonSelectable[] categoryButtons;

    public void UpdateItemDisplay(string category)
    {
        Debug.Log("show " + category + " items");
    }

    public void CategoryButtonClick()
    {
        foreach (UIButtonSelectable button in categoryButtons)
        {
            if (button.isSelected)
                UpdateItemDisplay(button.name);
        }
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
        VectorLine.canvas.gameObject.SetActive(true);
    }
}
