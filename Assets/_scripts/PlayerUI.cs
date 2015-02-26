﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerUI : MonoBehaviour {

    public Camera playerUICamera;
    public UIProgressBar staminaBar;
    public UIProgressBar restBar;

    public GameObject changeFaceLeftButton;
    public GameObject changeFaceRightButton;

    public Shop shop;

    public void ShopClicked()
    {
        shop.gameObject.SetActive(!shop.gameObject.activeSelf);
    }
}
