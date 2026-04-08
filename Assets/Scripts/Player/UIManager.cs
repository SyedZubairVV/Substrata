using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Health")]
    public Slider healthSlider;
    public TextMeshProUGUI hpText;

    [Header("Inventory")]
    public TextMeshProUGUI torchText;
    public TextMeshProUGUI potionText;

    [Header("Gold")]
    public TextMeshProUGUI goldText;

    [Header("Game Over")]
    public GameObject gameOverPanel;

    private Coroutine healthRoutine;
    private string previousScene = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // hide game over immediately on first frame
            // before any scene loads or coroutines run
            GameObject panel = GameObject.Find("GameOverPanel");
            if (panel != null)
                panel.SetActive(false);
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

    void Start()
    {
        StartCoroutine(RefreshUI());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(AfterSceneLoad(scene));
    }
    IEnumerator AfterSceneLoad(Scene scene)
    {
        yield return null;
        yield return null;

        // Hide Game Over every scene load
        HideGameOver();

        // re-find all UI elements since scene changed
        StartCoroutine(RefreshUI());

        previousScene = scene.name;
    }

    IEnumerator RefreshUI()
    {
        yield return null;
        yield return null;

        healthSlider = GameObject.Find("HealthBar")?.GetComponent<Slider>();
        hpText = GameObject.Find("HPText")?.GetComponent<TextMeshProUGUI>();
        torchText = GameObject.Find("TorchText")?.GetComponent<TextMeshProUGUI>();
        potionText = GameObject.Find("PotionText")?.GetComponent<TextMeshProUGUI>();
        goldText = GameObject.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
        gameOverPanel = GameObject.Find("GameOverPanel");

        // hide HUD on main menu
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameObject hud = GameObject.Find("HUD Canvas");
        if (hud != null)
            hud.SetActive(sceneName != "MainMenu");

        Debug.Log($"UI Reconnected | Scene: {sceneName}");
        RefreshAllUI();
    }

    void RefreshAllUI()
    {
        // GOLD
        if (GoldManager.Instance != null)
            UpdateGold(GoldManager.Instance.gold);

        // HEALTH
        if (PlayerHealth.Instance != null)
        {
            SetMaxHealth(PlayerHealth.Instance.maxHealth);
            UpdateHealth(
                PlayerHealth.Instance.GetCurrentHealth(),
                PlayerHealth.Instance.maxHealth
            );
        }

        // INVENTORY
        if (InventoryManager.Instance != null)
        {
            UpdateTorches(InventoryManager.Instance.torchCount);
            UpdatePotions(InventoryManager.Instance.potionCount);
        }
    }

    // --- HEALTH ---
    public void SetMaxHealth(int max)
    {
        if (healthSlider == null) return;

        healthSlider.maxValue = max;
        healthSlider.minValue = 0;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthSlider == null || hpText == null) return;

        // prevent unnecessary updates (fixes glitch)
        if (healthSlider.value == current) return;

        if (healthRoutine != null)
            StopCoroutine(healthRoutine);

        healthRoutine = StartCoroutine(SmoothHealth(current));

        hpText.text = $"HP: {current} / {max}";

        float percent = (float)current / max;
        Image fill = healthSlider.fillRect.GetComponent<Image>();

        if (percent > 0.6f) fill.color = Color.green;
        else if (percent > 0.3f) fill.color = Color.yellow;
        else fill.color = Color.red;
    }

    IEnumerator SmoothHealth(int target)
    {
        if (healthSlider == null) yield break;

        float start = healthSlider.value;
        float duration = 0.2f;
        float time = 0f;

        while (time < duration)
        {
            healthSlider.value = Mathf.Lerp(start, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        healthSlider.value = target;
    }

    // --- INVENTORY ---
    public void UpdateTorches(int count)
    {
        if (torchText != null)
            torchText.text = count.ToString();
    }

    public void UpdatePotions(int count)
    {
        if (potionText != null)
            potionText.text = count.ToString();
    }

    // --- GOLD ---
    public void UpdateGold(int amount)
    {
        if (goldText != null)
            goldText.text = $"Gold: {amount}";
    }

    // --- GAME OVER ---
    public void ShowGameOver()
    {
        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogError("GameOverPanel not found in scene!");
        }
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}