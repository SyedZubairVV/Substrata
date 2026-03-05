using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public float invincibleTime = 1f;

    public event Action<int> OnHealthChanged;

    int currentHealth;
    bool isInvincible;
    bool isDead;

    SpriteRenderer sprite;

    void Start()
    {
        currentHealth = maxHealth;
        sprite = GetComponent<SpriteRenderer>();

        OnHealthChanged?.Invoke(currentHealth);
    }

    void Update()
    {
        // TEST DAMAGE (P key)
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Invincibility());
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player Died");

        GetComponent<PlayerMovement>().enabled = false;
    }

    System.Collections.IEnumerator Invincibility()
    {
        isInvincible = true;

        float flashDelay = 0.1f;
        float timer = 0f;

        while (timer < invincibleTime)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(flashDelay);
            timer += flashDelay;
        }

        sprite.enabled = true;
        isInvincible = false;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}