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

    // Called by GameOverPanelRef
    public void RegisterPanel(GameObject panel)
    {
        gameOverPanel = panel;
        //gameOverPanel.SetActive(false);
        Debug.Log("GameOverPanel registered");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        StartCoroutine(InitAfterSceneLoad(scene));
    }

    IEnumerator InitAfterSceneLoad(Scene scene)
    {
        // wait for UI + Player to initialize
        yield return null;
        yield return null;

        // Toggle HUD visibility
        if (hudCanvas != null)
        {
            hudCanvas.SetActive(scene.name != mainMenuSceneName);
        }

        // Reset player in gameplay scenes
        if (scene.name != mainMenuSceneName)
        {
            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null)
                ph.ResetPlayer();
        }
    }

    public void ShowGameOver()
    {
        Debug.Log("ShowGameOver CALLED");

        // if reference is lost find it again
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        // if still null search inactive objects too
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
            Debug.Log("Game Over Panel shown");
        }
        else
        {
            Debug.LogError("GameOverPanel still not found!");
        }
    }

    IEnumerator ShowGameOverRoutine()
    {
        // 🔥 wait until panel registers (fixes your bug)
        while (gameOverPanel == null)
        {
            yield return null;
        }

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
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