using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
	public static InventoryManager Instance;

	[Header("Item Counts")]
	public int torchCount = 0;
	public int potionCount = 0;

	[Header("UI Text References")]
	public TextMeshProUGUI torchText; 
	public TextMeshProUGUI potionText;

	public List<LootItem> valuables = new List<LootItem>();

	[System.Serializable]
	public class LootItem
	{
		public string name;
		public int goldValue;
		public LootItem(string n, int v) { name = n; goldValue = v; }
	}

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		UpdateUI();
	}

	public void AddTorch(int amount = 1)
	{
		torchCount += amount;
		UpdateUI();
	}

	public void AddPotion(int amount = 1)
	{
		potionCount += amount;
		UpdateUI();
	}

	public void AddLoot(string name, int price)
	{
		valuables.Add(new LootItem(name, price));
		Debug.Log($"Picked up {name} worth {price} gold!");
	}

	public void UseTorch()
	{
		torchCount--;
		UpdateUI();
		Debug.Log("Torch Used!");
	}

	public void UsePotion()
	{
		potionCount--;
		UpdateUI();
		Debug.Log("Potion Used!");
	}

	public void UpdateUI()
	{
		torchText.text = torchCount.ToString();
		potionText.text = potionCount.ToString();
	}
}
