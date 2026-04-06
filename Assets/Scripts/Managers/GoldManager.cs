using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    public int gold = 0;

    // Starting gold amount for a new run
    private const int startingGold = 25;

    void Awake()
    {
        // Singleton pattern — persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gold = startingGold;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    // Returns true if the player had enough gold to spend, false otherwise
    public bool SpendGold(int amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;
        UpdateUI();
        return true;
    }

    public void UpdateUI()
    {
        UIManager.Instance?.UpdateGold(gold);
    }

    public void ResetGold()
    {
        gold = startingGold;
        UpdateUI();
    }
}