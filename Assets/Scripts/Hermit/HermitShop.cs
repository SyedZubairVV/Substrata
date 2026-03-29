using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HermitShop : MonoBehaviour
{
    [Header("Costs")]
    public int potionCost = 10;
    public int torchCost = 5;
    public int swordUpgradeCost = 25;

    [Header("Sword Upgrade")]
    public int maxUpgradeTier = 3;
    public int damageIncreasePerTier = 1;

    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI potionCostText;
    public TextMeshProUGUI torchCostText;
    public TextMeshProUGUI swordUpgradeText;
    public TextMeshProUGUI swordUpgradeCostText;
    public TextMeshProUGUI feedbackText;

    [Header("Buttons")]
    public Button buyPotionButton;
    public Button buyTorchButton;
    public Button upgradeSwordButton;

    void Start()
    {
        buyPotionButton.onClick.AddListener(BuyPotion);
        buyTorchButton.onClick.AddListener(BuyTorch);
        upgradeSwordButton.onClick.AddListener(UpgradeSword);

        RefreshUI(); // 🔥 important
    }

    public void RefreshUI()
    {
        if (GoldManager.Instance == null || PlayerStats.Instance == null) return;

        goldText.text = $"Gold: {GoldManager.Instance.gold}";
        potionCostText.text = $"{potionCost}g";
        torchCostText.text = $"{torchCost}g";

        int tier = PlayerStats.Instance.swordUpgradeTier;

        if (tier >= maxUpgradeTier)
        {
            swordUpgradeText.text = "MAX";
            swordUpgradeCostText.text = "";
        }
        else
        {
            swordUpgradeText.text = $"Tier {tier + 1}/{maxUpgradeTier}";
            swordUpgradeCostText.text = $"{swordUpgradeCost}g";
        }

        upgradeSwordButton.interactable = tier < maxUpgradeTier;

        feedbackText.text = "";
    }

    void BuyPotion()
    {
        if (GoldManager.Instance.SpendGold(potionCost))
        {
            InventoryManager.Instance.AddPotion(1);
            ShowFeedback("Health potion added!");
            RefreshUI();
        }
        else
        {
            ShowFeedback("Not enough gold!");
        }
    }

    void BuyTorch()
    {
        if (GoldManager.Instance.SpendGold(torchCost))
        {
            InventoryManager.Instance.AddTorch(1);
            ShowFeedback("Torch added!");
            RefreshUI();
        }
        else
        {
            ShowFeedback("Not enough gold!");
        }
    }

    void UpgradeSword()
    {
        if (PlayerStats.Instance.swordUpgradeTier >= maxUpgradeTier)
        {
            ShowFeedback("Sword is fully upgraded!");
            return;
        }

        if (GoldManager.Instance.SpendGold(swordUpgradeCost))
        {
            PlayerStats.Instance.swordUpgradeTier++;
            PlayerStats.Instance.swordDamage += damageIncreasePerTier;

            ShowFeedback($"Sword upgraded to tier {PlayerStats.Instance.swordUpgradeTier}!");
            RefreshUI();
        }
        else
        {
            ShowFeedback("Not enough gold!");
        }
    }

    public void ResetUpgrades()
    {
        if (PlayerStats.Instance == null) return;

        PlayerStats.Instance.swordUpgradeTier = 0;
        PlayerStats.Instance.swordDamage = 1;
    }

    void ShowFeedback(string message)
    {
        feedbackText.text = message;
        CancelInvoke(nameof(ClearFeedback));
        Invoke(nameof(ClearFeedback), 2f);
    }

    void ClearFeedback()
    {
        feedbackText.text = "";
    }
}