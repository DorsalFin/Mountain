using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class Shop : MonoBehaviour {

    public UIGrid itemGrid;
    public GameObject shopItemPrefab;
    private string _currentlyDisplayingCategory;

    public Player player;

    public void CloseShop()
    {
        gameObject.SetActive(false);
        VectorLine.canvas.gameObject.SetActive(true);
    }
}
