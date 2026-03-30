using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;
    public int gold = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gold = 25;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void UpdateUI()
    {
        UIManager.Instance?.UpdateGold(gold);
    }

    public void ResetGold()
    {
        gold = 25;
        UpdateUI();
    }
}