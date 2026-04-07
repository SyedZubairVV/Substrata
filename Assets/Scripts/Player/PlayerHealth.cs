using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Health")]
    public int maxHealth = 5;
    public float invincibleTime = 0.5f;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.15f;

    public event Action<int> OnHealthChanged;

    // references
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sprite;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;

    private int currentHealth;
    private bool isInvincible;
    private bool isDead;
    private bool isKnockedBack;

    // static field survives scene loads without DontDestroyOnLoad
    private static int savedHealth = -1;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        sprite = GetComponent<SpriteRenderer>();
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

        // restore saved health from previous scene, or use max if first load
        if (savedHealth >= 0)
            currentHealth = savedHealth;
        else
            currentHealth = maxHealth;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetMaxHealth(maxHealth);
            UIManager.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (isInvincible || isDead) return;
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        savedHealth = currentHealth;
        OnHealthChanged?.Invoke(currentHealth);

        // update UI directly
        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
        else
        {
            animator.SetTrigger("hurt");
            StartCoroutine(KnockbackRoutine(hitDirection));
            StartCoroutine(Invincibility());
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        if (currentHealth >= maxHealth) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        savedHealth = currentHealth;
        OnHealthChanged?.Invoke(currentHealth);
        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        // reset all states before triggering death
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDashing", false);
        animator.ResetTrigger("takingHit");
        animator.SetTrigger("die");

        // show game over after death animation
        Invoke(nameof(TriggerGameOver), 1.5f);
    }

    void TriggerGameOver()
    {
        GameOverManager.Instance?.ShowGameOver();
    }

    public void ResetPlayer()
    {
        isDead = false;
        isInvincible = false;
        isKnockedBack = false;
        currentHealth = maxHealth;
        savedHealth = -1;
        OnHealthChanged?.Invoke(currentHealth);

        if (rb != null) rb.simulated = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;

        StopAllCoroutines();

        if (animator != null)
        {
            animator.ResetTrigger("die");
            animator.ResetTrigger("hurt");
            animator.SetBool("isDead", false);
        }

        // hide game over and update UI
        UIManager.Instance?.HideGameOver();
        UIManager.Instance?.UpdateHealth(currentHealth, maxHealth);
    }

    System.Collections.IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockedBack = true;

        // disable movement control during knockback
        playerMovement.enabled = false;

        // push player in hit direction
        rb.linearVelocity = new Vector2(direction.x * knockbackForce, knockbackForce * 0.4f);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isKnockedBack = false;

        // only re-enable movement if still alive
        if (!isDead)
            playerMovement.enabled = true;
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

    public int GetCurrentHealth() => currentHealth;
    public bool IsDead() => isDead;

    public static void ResetSavedHealth()
    {
        savedHealth = -1;
    }
}