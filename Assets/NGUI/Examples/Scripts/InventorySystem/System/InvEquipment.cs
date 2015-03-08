using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Inventory system -- Equipment class works with InvAttachmentPoints and allows to visually equip and remove items.
/// </summary>

[AddComponentMenu("NGUI/Examples/Equipment")]
public class InvEquipment : MonoBehaviour
{
    public Player player;

	InvGameItem[] mItems;
	InvAttachmentPoint[] mAttachments;

	/// <summary>
	/// List of equipped items (with a finite number of equipment slots).
	/// </summary>

	public InvGameItem[] equippedItems { get { return mItems; } }

    void Start()
    {
        player = GetComponent<Player>();
    }

	/// <summary>
	/// Equip the specified item automatically replacing an existing one.
	/// </summary>

	public InvGameItem Replace (InvBaseItem.Slot slot, InvGameItem item)
	{
		InvBaseItem baseItem = (item != null) ? item.baseItem : null;

        if (mItems == null)
        {
            // Automatically figure out how many item slots we need
            int count = (int)InvBaseItem.Slot._LastDoNotUse;
            mItems = new InvGameItem[count];
        }

		if (slot != InvBaseItem.Slot.None)
		{
			// If the item is not of appropriate type, we shouldn't do anything
            //if (baseItem != null && baseItem.slot != slot) return item;

            InvGameItem itemToReturn = null;

            // check if we have a valid slot
            if (baseItem != null)
            {
                // weapon slotted items can go in either left or right hand slots
                if (baseItem.slot == InvBaseItem.Slot.Weapon || baseItem.slot == InvBaseItem.Slot.Shield)
                {
                    if (slot != InvBaseItem.Slot.LeftHand && slot != InvBaseItem.Slot.RightHand)
                        return item;

                    // get the items we are currently holding in our hands
                    List<InvGameItem> itemsInHands = new List<InvGameItem>();
                    foreach (InvGameItem itemInHand in mItems)
                        if (itemInHand != null && (itemInHand.baseItem.slot == InvBaseItem.Slot.Weapon || itemInHand.baseItem.slot == InvBaseItem.Slot.Shield))
                            itemsInHands.Add(itemInHand);

                    // now deal with any special cases
                    if (itemsInHands.Count == 1)
                    {
                        // return any weapon that might be in the other hand
                        // since we need two slots for the two handed weapon
                        if (baseItem.twoHanded)
                            itemToReturn = itemsInHands[0];
                        else
                        {
                            // if equipping a single handed weapon into a two hander,
                            // make sure to return the two hander
                            if (itemsInHands[0].baseItem.twoHanded)
                                itemToReturn = itemsInHands[0];
                        }
                    }
                    else if (itemsInHands.Count > 1)
                    {
                        if (baseItem.twoHanded)
                            // if two items in hands, failure - don't equip
                            return item;
                    }
                }
                else
                    if (baseItem.slot != slot) 
                        return item;
            }

            int indexToReturn = 99;
            if (itemToReturn != null)
            {
                for (int i = 0; i < mItems.Length; i++)
                {
                    if (mItems[i] == itemToReturn)
                    {
                        indexToReturn = i;
                        break;
                    }
                }
            }

			// Equip this item
            InvGameItem prev = indexToReturn == 99 ? mItems[(int)slot - 1] : mItems[indexToReturn];
            if (itemToReturn != null)
                Replace((slot == InvBaseItem.Slot.RightHand ? InvBaseItem.Slot.LeftHand : InvBaseItem.Slot.RightHand), null);


			mItems[(int)slot - 1] = item;

			// Get the list of all attachment points
			if (mAttachments == null) mAttachments = GetComponentsInChildren<InvAttachmentPoint>();

			// Equip the item visually
			for (int i = 0, imax = mAttachments.Length; i < imax; ++i)
			{
				InvAttachmentPoint ip = mAttachments[i];

				if (ip.slot == slot)
				{
					GameObject go = ip.Attach(baseItem != null ? baseItem.attachment : null);

					if (baseItem != null && go != null)
					{
						Renderer ren = go.GetComponent<Renderer>();
						if (ren != null) ren.material.color = baseItem.color;
					}
				}
			}
			return prev;
		}
		else if (item != null)
		{
			Debug.LogWarning("Can't equip \"" + item.name + "\" because it doesn't specify an item slot");
		}
		return item;
	}

	/// <summary>
	/// Equip the specified item and return the item that was replaced.
	/// </summary>

	public InvGameItem Equip (InvGameItem item)
	{
		if (item != null)
		{
			InvBaseItem baseItem = item.baseItem;
			if (baseItem != null) return Replace(baseItem.slot, item);
			else Debug.LogWarning("Can't resolve the item ID of " + item.baseItemID);
		}
		return item;
	}

	/// <summary>
	/// Unequip the specified item, returning it if the operation was successful.
	/// </summary>

	public InvGameItem Unequip (InvGameItem item)
	{
		if (item != null)
		{
			InvBaseItem baseItem = item.baseItem;
			if (baseItem != null) return Replace(baseItem.slot, null);
		}
		return item;
	}

	/// <summary>
	/// Unequip the item from the specified slot, returning the item that was unequipped.
	/// </summary>

	public InvGameItem Unequip (InvBaseItem.Slot slot) { return Replace(slot, null); }

	/// <summary>
	/// Whether the specified item is currently equipped.
	/// </summary>

	public bool HasEquipped (InvGameItem item)
	{
		if (mItems != null)
		{
			for (int i = 0, imax = mItems.Length; i < imax; ++i)
			{
				if (mItems[i] == item) return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Whether the specified slot currently has an item equipped.
	/// </summary>

	public bool HasEquipped (InvBaseItem.Slot slot)
	{
		if (mItems != null)
		{
			for (int i = 0, imax = mItems.Length; i < imax; ++i)
			{
				InvBaseItem baseItem = mItems[i].baseItem;
				if (baseItem != null && baseItem.slot == slot) return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Retrieves the item in the specified slot.
	/// </summary>

	public InvGameItem GetItem (InvBaseItem.Slot slot)
	{
		if (slot != InvBaseItem.Slot.None)
		{
			int index = (int)slot - 1;

			if (mItems != null && index < mItems.Length)
			{
				return mItems[index];
			}
		}
		return null;
	}
}