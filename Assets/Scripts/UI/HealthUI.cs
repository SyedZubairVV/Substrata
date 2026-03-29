using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public Slider slider;
    public TMP_Text hpText;

    private PlayerHealth playerHealth;
    private Coroutine smoothCoroutine;

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(RefreshUIReferences());
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(RefreshUIReferences());
    }

    IEnumerator RefreshUIReferences()
    {
        yield return null;
        yield return null;

        // hide on main menu
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu")
        {
            gameObject.SetActive(false);
            yield break;
        }

        // unsubscribe from old reference before finding new one
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth not found!");
            yield break;
        }

        slider.minValue = 0;
        slider.maxValue = playerHealth.maxHealth;

        playerHealth.OnHealthChanged += UpdateHealthUI;

        // immediately show correct value
        UpdateHealthUI(playerHealth.GetCurrentHealth());

        Debug.Log($"HealthUI refreshed | HP: {playerHealth.GetCurrentHealth()}");
    }

    void UpdateHealthUI(int currentHealth)
    {
        if (smoothCoroutine != null)
            StopCoroutine(smoothCoroutine);
        smoothCoroutine = StartCoroutine(SmoothHealth(currentHealth));
        hpText.text = "HP: " + currentHealth + " / " + (int)slider.maxValue;
        UpdateColor(currentHealth);
    }

    IEnumerator SmoothHealth(int target)
    {
        float start = slider.value;
        float duration = 0.2f;
        float time = 0f;
        while (time < duration)
        {
            slider.value = Mathf.Lerp(start, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        slider.value = target;
    }

    void UpdateColor(int currentHealth)
    {
        float percent = currentHealth / slider.maxValue;
        Image fillImage = slider.fillRect.GetComponent<Image>();
        if (percent > 0.6f) fillImage.color = Color.green;
        else if (percent > 0.3f) fillImage.color = Color.yellow;
        else fillImage.color = Color.red;
    }
}