using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
	public List<Item> items = new List<Item>();
	public Transform slotParent;
	private Image[] itemIcons;

	void Start()
	{
		// Get all "ItemIcon" images from the children of your slots
		// This assumes each Slot has a child Image named "ItemIcon"
		itemIcons = slotParent.GetComponentsInChildren<Image>();
		UpdateUI();
	}

	public void AddItem(Item newItem)
	{
		if (items.Count < itemIcons.Length)
		{
			items.Add(newItem);
			UpdateUI();
		}
	}

	public void UpdateUI()
	{
		for (int i = 0; i < itemIcons.Length; i++)
		{
			// If there is an item for this slot index
			if (i < items.Count)
			{
				itemIcons[i].sprite = items[i].icon;
				itemIcons[i].enabled = true; // Show the icon

				// Optional: Set the Alpha to 1 if it was transparent
				Color tempColor = itemIcons[i].color;
				tempColor.a = 1f;
				itemIcons[i].color = tempColor;
			}
			else
			{
				// If the slot is empty, hide the icon sprite
				itemIcons[i].sprite = null;
				itemIcons[i].enabled = false;
			}
		}
	}
}
