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

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    void Update()
    {
        // test damage with P key
        if (Keyboard.current.pKey.wasPressedThisFrame)
            TakeDamage(1, Vector2.left);
    }

    // now accepts a hit direction so knockback pushes the right way
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (isInvincible || isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
            Die();
        else
        {
            // play hurt animation
            animator.SetTrigger("hurt");

            StartCoroutine(KnockbackRoutine(hitDirection));
            StartCoroutine(Invincibility());
        }
    }

    // overload with no direction for backwards compatibility
    // e.g. the P key test above, or anything that doesn't have a direction
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector2.left);
    }

    public void Heal(int healing)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth +  healing, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
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

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // disable all control
        playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;

        // stop all movement
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // play death animation and freeze there
        animator.SetTrigger("die");

        Debug.Log("Player died");
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
}