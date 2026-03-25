using UnityEngine;

public enum ItemType { Torch, Potion, Loot }

public class ItemPickup : MonoBehaviour
{
	public ItemType type;
	public string lootName; // e.g., "Silver Ring"
	public int value;       // How much it's worth to a merchant

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			if (type == ItemType.Torch)
			{
				InventoryManager.Instance.AddTorch(1);
			}
			else if (type == ItemType.Potion)
			{
				InventoryManager.Instance.AddPotion(1);
			}
			else if (type == ItemType.Loot)
			{
				// This would call a different method for your valuables
				InventoryManager.Instance.AddLoot(lootName, value);
			}

			Destroy(gameObject);
		}
	}
}
