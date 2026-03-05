using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public Slider slider;
    public TMP_Text hpText;

    PlayerHealth playerHealth;
    Coroutine smoothCoroutine;

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth not found in scene!");
            return;
        }

        slider.minValue = 0;
        slider.maxValue = playerHealth.maxHealth;
        slider.value = playerHealth.maxHealth;

        playerHealth.OnHealthChanged += UpdateHealthUI;

        UpdateHealthUI(playerHealth.GetCurrentHealth());
    }

    void UpdateHealthUI(int currentHealth)
    {
        if (smoothCoroutine != null)
            StopCoroutine(smoothCoroutine);

        smoothCoroutine = StartCoroutine(SmoothHealth(currentHealth));

        hpText.text = "HP: "+ currentHealth + " / " + slider.maxValue;

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

        if (percent > 0.6f)
            fillImage.color = Color.green;
        else if (percent > 0.3f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = Color.red;
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealthUI;
    }
}