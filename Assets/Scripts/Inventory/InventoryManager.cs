using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public int torchCount = 0;
    public int potionCount = 0;

    // tracks whether the singleton has already loaded once
    private static bool initialized = false;
    private static int savedTorches = 0;
    private static int savedPotions = 0;
    private static int defaultTorches = 0;
    private static int defaultPotions = 0;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // on first load, keep whatever the Inspector has;
            // on subsequent loads, restore the saved values
            if (initialized)
            {
                torchCount = savedTorches;
                potionCount = savedPotions;
            }
            else
            {
                // cache the Inspector values as the run defaults
                defaultTorches = torchCount;
                defaultPotions = potionCount;
                initialized = true;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Force UI refresh after scene load
        UpdateUI();
    }

    public void AddTorch(int amount = 1)
    {
        torchCount += amount;
        SaveCounts();
        UpdateUI();
    }

    public void AddPotion(int amount = 1)
    {
        potionCount += amount;
        SaveCounts();
        UpdateUI();
    }

    public void UseTorch()
    {
        if (torchCount <= 0) return;
        torchCount--;
        SaveCounts();
        UpdateUI();
    }

    public void UsePotion()
    {
        if (potionCount <= 0) return;
        potionCount--;
        SaveCounts();
        UpdateUI();
    }

    public void UpdateUI()
    {
        UIManager.Instance?.UpdateTorches(torchCount);
        UIManager.Instance?.UpdatePotions(potionCount);
    }

    void SaveCounts()
    {
        savedTorches = torchCount;
        savedPotions = potionCount;
    }

    public void ResetInventory()
    {
        torchCount = defaultTorches;
        potionCount = defaultPotions;
        savedTorches = defaultTorches;
        savedPotions = defaultPotions;
        UpdateUI();
    }
}