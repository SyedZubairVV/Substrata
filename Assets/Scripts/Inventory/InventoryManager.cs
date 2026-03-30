using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public int torchCount = 0;
    public int potionCount = 0;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 🔥 Force UI refresh after scene load
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

    public void UseTorch()
    {
        if (torchCount <= 0) return;
        torchCount--;
        UpdateUI();
    }

    public void UsePotion()
    {
        if (potionCount <= 0) return;
        potionCount--;
        UpdateUI();
    }

    public void AddLoot(string name, int price)
    {
        valuables.Add(new LootItem(name, price));
    }

    public void UpdateUI()
    {
        UIManager.Instance?.UpdateTorches(torchCount);
        UIManager.Instance?.UpdatePotions(potionCount);
    }

    public void ResetInventory()
    {
        torchCount = 0;
        potionCount = 0;
        valuables.Clear();
        UpdateUI();
    }
}