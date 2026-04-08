using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;
    public string mainMenuSceneName = "MainMenu";

    private GameObject gameOverPanel;
    private GameObject hudCanvas;

    void Awake()
    {
        // Singleton pattern — persist across scenes
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

    // Called by GameOverPanelRef on the panel's own Start() to register itself
    public void RegisterPanel(GameObject panel)
    {
        gameOverPanel = panel;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Always restore time scale on any scene load
        Time.timeScale = 1f;
        StartCoroutine(InitAfterSceneLoad(scene));
    }

    IEnumerator InitAfterSceneLoad(Scene scene)
    {
        // Wait two frames for UI and Player objects to fully initialize
        yield return null;
        yield return null;

        // Hide HUD on the main menu, show it everywhere else
        if (hudCanvas != null)
            hudCanvas.SetActive(scene.name != mainMenuSceneName);
    }

    public void ShowGameOver()
    {
        // Try to recover the panel reference if it was lost (e.g. after scene reload)
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        // Fall back to searching inactive objects if still not found
        if (gameOverPanel == null)
        {
            GameOverPanelRef panelRef = FindFirstObjectByType<GameOverPanelRef>(FindObjectsInactive.Include);
            if (panelRef != null)
                gameOverPanel = panelRef.gameObject;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        ResetRun();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        ResetRun();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void ResetRun()
    {
        // Wipe all persistent run state so the next run starts clean
        PlayerHealth.ResetSavedHealth();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ResetInventory();

        if (GoldManager.Instance != null)
            GoldManager.Instance.ResetGold();

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.swordDamage = 1;
            PlayerStats.Instance.swordUpgradeTier = 0;
        }

        PlayerPrefs.DeleteKey("HermitIntroComplete");
    }
}