﻿using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Inventory System -- Base Item. Note that it would be incredibly tedious to create all items by hand, Warcraft style.
/// It's a lot more straightforward to create all items to be of the same level as far as stats go, then specify an
/// appropriate level range for the item where it will appear. Effective item stats can then be calculated by lowering
/// the base stats by an appropriate amount. Add a quality modifier, and you have additional variety, Terraria 1.1 style.
/// </summary>

[System.Serializable]
public class InvBaseItem
{
	public enum Slot
	{
		None,			// First element MUST be 'None'
		Weapon,			// All the following elements are yours to customize -- edit, add or remove as you desire
		Shield,
		Body,
		Shoulders,
		Bracers,
		Boots,
		Trinket,
		_LastDoNotUse,	// Flash export doesn't support Enum.GetNames :(
	}

    public bool unlimitedSupply;
    public bool sold;

	/// <summary>
	/// 16-bit item ID, generated by the system.
	/// Not to be confused with a 32-bit item ID, which actually contains the ID of the database as its prefix.
	/// </summary>

	public int id16;

	/// <summary>
	/// Name of this item.
	/// </summary>

	public string name;

	/// <summary>
	/// This item's custom description.
	/// </summary>

	public string description;

    public int cost;

	/// <summary>
	/// Slot that this item belongs to.
	/// </summary>

	public Slot slot = Slot.None;

	/// <summary>
	/// Minimum and maximum allowed level for this item. When random loot gets generated,
	/// only items within appropriate level should be considered.
	/// </summary>

	public int minItemLevel = 1;
	public int maxItemLevel = 50;

	/// <summary>
	/// And and all base stats this item may have at a maximum level (50).
	/// Actual object's stats are calculated based on item's level and quality.
	/// </summary>

	public List<InvStat> stats = new List<InvStat>();

	/// <summary>
	/// Game Object that will be created and attached to the specified slot on the body.
	/// This should typically be a prefab with a renderer component, such as a sword,
	/// a bracer, shield, etc.
	/// </summary>

	public GameObject attachment;

	/// <summary>
	/// Object's main material color.
	/// </summary>

	public Color color = Color.white;

	/// <summary>
	/// Atlas used for the item's icon.
	/// </summary>

	public UIAtlas iconAtlas;

	/// <summary>
	/// Name of the icon's sprite within the atlas.
	/// </summary>

	public string iconName = "";
}