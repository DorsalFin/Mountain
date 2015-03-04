//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Storage container that stores items.
/// </summary>

[AddComponentMenu("NGUI/Examples/UI Item Storage")]
public class UIItemStorage : MonoBehaviour
{

    public Shop fromShop;

	/// <summary>
	/// Maximum size of the container. Adding more items than this number will not work.
	/// </summary>

	public int maxItemCount = 8;

	/// <summary>
	/// Maximum number of rows to create.
	/// </summary>

	public int maxRows = 4;

	/// <summary>
	/// Maximum number of columns to create.
	/// </summary>

	public int maxColumns = 4;

	/// <summary>
	/// Template used to create inventory icons.
	/// </summary>

	public GameObject template;

	/// <summary>
	/// Background widget to scale after the item slots have been created.
	/// </summary>

	public UIWidget background;

	/// <summary>
	/// Spacing between icons.
	/// </summary>

	public int spacing = 128;

	/// <summary>
	/// Padding around the border.
	/// </summary>

	public int padding = 10;

	List<InvGameItem> mItems = new List<InvGameItem>();

	/// <summary>
	/// List of items in the container.
	/// </summary>

	public List<InvGameItem> items { get { while (mItems.Count < maxItemCount) mItems.Add(null); return mItems; } }

	/// <summary>
	/// Convenience function that returns an item at the specified slot.
	/// </summary>

	public InvGameItem GetItem (int slot) { return (slot < items.Count) ? mItems[slot] : null; }

	/// <summary>
	/// Replace an item in the container with the specified one.
	/// </summary>
	/// <returns>An item that was replaced.</returns>

	public InvGameItem Replace (int slot, InvGameItem item)
	{
		if (slot < maxItemCount)
		{
			InvGameItem prev = items[slot];
			mItems[slot] = item;
			return prev;
		}
		return item;
	}

	/// <summary>
	/// Initialize the container and create an appropriate number of UI slots.
	/// </summary>

	void Start ()
	{
		if (template != null)
		{
			int count = 0;
			Bounds b = new Bounds();

			for (int y = 0; y < maxRows; ++y)
			{
				for (int x = 0; x < maxColumns; ++x)
				{
					GameObject go = NGUITools.AddChild(gameObject, template);
					Transform t = go.transform;
					t.localPosition = new Vector3(padding + (x + 0.5f) * spacing, -padding - (y + 0.5f) * spacing, 0f);

					UIStorageSlot slot = go.GetComponent<UIStorageSlot>();
					
					if (slot != null)
					{
						slot.storage = this;
						slot.slot = count;
					}

					b.Encapsulate(new Vector3(padding * 2f + (x + 1) * spacing, -padding * 2f - (y + 1) * spacing, 0f));

					if (++count >= maxItemCount)
					{
						if (background != null)
						{
							background.transform.localScale = b.size;
						}
						return;
					}
				}
			}
			if (background != null) background.transform.localScale = b.size;
		}
	}

#region RHYS
    /// <summary>
    /// called from the category buttons on the shop screen - fills out the shop item storage
    /// </summary>
    public void FillWithItems(string category)
    {
        int index = 0;
        foreach (InvBaseItem item in InvDatabase.list[0].items)
        {
            if (category == item.slot.ToString())
            {
                InvGameItem gi = new InvGameItem(index, item, fromShop);
                Replace(index, gi);
                index++;
            }
        }
    }

    public bool AddItemToStorage(InvGameItem item)
    {
        int currentItemsInStorage = GetCountOfItemsOnDisplay();
        if (currentItemsInStorage < maxItemCount)
        {
            Replace(currentItemsInStorage, item);
        }
        return false;
    }

    int GetCountOfItemsOnDisplay()
    {
        int counter = 0;
        foreach (InvGameItem item in mItems)
            if (item != null) counter++;
        return counter;
    }
#endregion
}