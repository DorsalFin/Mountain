//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract UI component observing an item somewhere in the inventory. This item can be equipped on
/// the character, it can be lying in a chest, or it can be hot-linked by another player. Either way,
/// all the common behavior is in this class. What the observed item actually is...
/// that's up to the derived class to determine.
/// </summary>

public abstract class UIItemSlot : MonoBehaviour
{
	public UISprite icon;
	public UIWidget background;
	public UILabel label;

	public AudioClip grabSound;
	public AudioClip placeSound;
	public AudioClip errorSound;

	InvGameItem mItem;
	string mText = "";

	static InvGameItem mDraggedItem;

    public UIItemStorage.Location storageType;
    public UIItemStorage fromStorage;
    public Player controllingPlayer;

	/// <summary>
	/// This function should return the item observed by this UI class.
	/// </summary>

	abstract protected InvGameItem observedItem { get; }

	/// <summary>
	/// Replace the observed item with the specified value. Should return the item that was replaced.
	/// </summary>

	abstract public InvGameItem Replace (InvGameItem item);

    //void Start()
    //{
    //    if (fromStorage != null)
    //        controllingPlayer = fromStorage.fromShop.player;
    //}

	/// <summary>
	/// Show a tooltip for the item.
	/// </summary>

	void OnTooltip (bool show)
	{
		InvGameItem item = show ? mItem : null;

		if (item != null)
		{
			InvBaseItem bi = item.baseItem;

			if (bi != null)
			{
				string t = "[" + NGUIText.EncodeColor(item.color) + "]" + item.name + "[-]\n";

				t += "[AFAFAF]Level " + item.itemLevel + " " + bi.slot;

				List<InvStat> stats = item.CalculateStats();

				for (int i = 0, imax = stats.Count; i < imax; ++i)
				{
					InvStat stat = stats[i];
					if (stat.amount == 0) continue;

					if (stat.amount < 0)
					{
						t += "\n[FF0000]" + stat.amount;
					}
					else
					{
						t += "\n[00FF00]+" + stat.amount;
					}

					if (stat.modifier == InvStat.Modifier.Percent) t += "%";
					t += " " + stat.id;
					t += "[-]";
				}

				if (!string.IsNullOrEmpty(bi.description)) t += "\n[FF9900]" + bi.description;
				UITooltip.ShowText(t);
				return;
			}
		}
		UITooltip.ShowText(null);
	}

	/// <summary>
	/// Allow to move objects around via drag & drop.
	/// </summary>

	void OnClick ()
	{
		if (mDraggedItem != null)
		{
            OnDrop(null);
		}
		else if (mItem != null)
		{
            // left click
            if (UICamera.currentTouchID == -1)
            {
                if (mItem.location == UIItemStorage.Location.Shop)
                {
                }
                else // item is not in shop
                {
                    //if (mItem.baseItem.slot == InvBaseItem.Slot.Trinket || mItem.location == UIItemStorage.Location.AtBase)
                        //controllingPlayer.SelectOrDeselectItem(mItem, background, this);

                    //else
                    //{
                    //    mDraggedItem = Replace(null);
                    //    if (mDraggedItem != null) NGUITools.PlaySound(grabSound);
                    //    UpdateCursor();
                    //}
                }
            }
            // right click
            else if (UICamera.currentTouchID == -2)
            {
                if (mItem.location == UIItemStorage.Location.Shop)
                {
                    //Player buyer = mItem.FromShop.player;
                    //bool successfulPurchase = buyer.PurchaseItem(mItem);
                    //if (successfulPurchase && !mItem.baseItem.unlimitedSupply)
                    //{
                    //    mItem.baseItem.sold = true;
                    //    Replace(null);
                    //    fromStorage.UpdateShopItems();
                    //}
                }
            }
		}
	}

    void OnHover(bool isOver)
    {
        if (isOver)
            OnTooltip(true);
        else
            OnTooltip(false);
    }

	/// <summary>
	/// Start dragging the item.
	/// </summary>

	void OnDrag (Vector2 delta)
	{
        if (mDraggedItem == null && mItem != null && mItem.location != UIItemStorage.Location.Shop) // can't drag things in the shop
        {
            //controllingPlayer.SelectOrDeselectItem(null, null, null);
            UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
            mDraggedItem = Replace(null);
            NGUITools.PlaySound(grabSound);
            UpdateCursor();
        }
	}

	/// <summary>
	/// Stop dragging the item.
	/// </summary>

	void OnDrop (GameObject go)
	{
        if (mDraggedItem == null) return;

        bool allowed = mDraggedItem.location == storageType;
        if (mDraggedItem.location != storageType)
            allowed = controllingPlayer.IsInHomeTile;

        if (allowed)
        {
            InvGameItem item = Replace(mDraggedItem);
            mDraggedItem.location = storageType;
            if (mDraggedItem == item || item != null)
            {
                UIItemSlot previousSlot = UICamera.DraggedFromSlot.GetComponent<UIItemSlot>();
                previousSlot.Replace(item);
                mDraggedItem = null;
                UpdateCursor();
            }
            else
            {
                NGUITools.PlaySound(placeSound);
                mDraggedItem = item;
                UpdateCursor();
            }
        }
	}

	/// <summary>
	/// Set the cursor to the icon of the item being dragged.
	/// </summary>

	void UpdateCursor ()
	{
		if (mDraggedItem != null && mDraggedItem.baseItem != null)
		{
			UICursor.Set(mDraggedItem.baseItem.iconAtlas, mDraggedItem.baseItem.iconName);
		}
		else
		{
			UICursor.Clear();
		}
	}

	/// <summary>
	/// Keep an eye on the item and update the icon when it changes.
	/// </summary>

	void Update ()
	{
		InvGameItem i = observedItem;

		if (mItem != i)
		{
			mItem = i;

			InvBaseItem baseItem = (i != null) ? i.baseItem : null;

			if (label != null)
			{
				string itemName = (i != null) ? i.name : null;
				if (string.IsNullOrEmpty(mText)) mText = label.text;
				label.text = (itemName != null) ? itemName : mText;
			}
			
			if (icon != null)
			{
				if (baseItem == null || baseItem.iconAtlas == null)
				{
					icon.enabled = false;
				}
				else
				{
					icon.atlas = baseItem.iconAtlas;
					icon.spriteName = baseItem.iconName;
					icon.enabled = true;
					icon.MakePixelPerfect();
				}
			}

            //if (background != null)
            //{
            //    background.color = (i != null) ? i.color : Color.white;
            //}
		}
	}
}
