using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combo Settings")]
    public float comboWindowTime = 0.5f;     // time after each hit where next hit can be queued
    public float timeBetweenHits = 0.3f;     // minimum time between each hit within a combo
    public float comboCooldown = 0.8f;         // cooldown after full combo before attacking again

    [Header("Attack Hitbox")]
    public float attackRadius = 0.6f;
    public float attackReach = 1f;
    public float attackHeightOffset = 0f;
    public int[] attackDamage = { 1, 1, 1, 2, 3 };
    
    [Header("Movement")]
    public float attackMoveSpeedMultiplier = 0.4f; // 0 = stop completely, 1 = full speed
    private float originalMoveSpeed;

    private Animator animator;
    private SpriteRenderer sprite;
    private PlayerMovement playerMovement;
    private Camera mainCamera;

    private int currentCombo = 0;
    private bool isAttacking = false;
    private bool comboQueued = false;
    private float comboWindowTimer = 0f;
    private float fallbackTimer = 0f;

    // timeBetweenHits cooldown — stops spamming within a combo
    private float hitCooldownTimer = 0f;

    // comboCooldown timer — starts after the full combo finishes
    private float comboCooldownTimer = 0f;
    private PlayerSounds playerSounds;

    void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        playerSounds = GetComponent<PlayerSounds>();
        originalMoveSpeed = playerMovement.moveSpeed;
        mainCamera = Camera.main;
    }

    void Update()
    {
        // count all timers down every frame
        if (comboWindowTimer > 0) comboWindowTimer -= Time.deltaTime;
        if (hitCooldownTimer > 0) hitCooldownTimer -= Time.deltaTime;
        if (comboCooldownTimer > 0) comboCooldownTimer -= Time.deltaTime;

        if (isAttacking)
        {
            fallbackTimer -= Time.deltaTime;
            if (fallbackTimer <= 0f)
            {
                Debug.Log("Fallback reset triggered");
                ResetAttack();
            }
        }

        if (InputSystem.actions["Attack"].WasPressedThisFrame())
            TryAttack();
    }

    void TryAttack()
    {
        // block attacking if combo cooldown is still running
        if (comboCooldownTimer > 0f) return;

        FaceMouseDirection();

        if (!isAttacking)
        {
            // block starting a new combo if between-hit cooldown is active
            if (hitCooldownTimer > 0f) return;
            StartAttack(1);
        }
        else if (currentCombo < 5)
        {
            // only queue next hit if the between-hit cooldown has passed
            if (hitCooldownTimer > 0f) return;
            comboQueued = true;
            Debug.Log("Combo queued");
        }
    }

    void FaceMouseDirection()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        sprite.flipX = mouseWorld.x < transform.position.x;
    }

    void StartAttack(int comboStep)
    {
        // slow movement when attacking
        playerMovement.moveSpeed = originalMoveSpeed * attackMoveSpeedMultiplier;
        
        for (int i = 1; i <= 5; i++)
            animator.SetBool($"attack{i}", false);

        currentCombo = comboStep;
        isAttacking = true;
        comboQueued = false;

        playerMovement.isAttacking = true;
        animator.SetBool("isAttacking", true);
        animator.SetBool($"attack{comboStep}", true);
        // play swing sound when each attack begins
        playerSounds?.PlaySwing(comboStep);

        // start the between-hit cooldown so player can't instantly chain
        hitCooldownTimer = timeBetweenHits;

        fallbackTimer = 1f;

        Debug.Log($"Attack {comboStep} started");
    }

    // --- ANIMATION EVENT --- ~60% through each attack animation
    public void OnComboWindowOpen()
    {
        comboWindowTimer = comboWindowTime;

        if (comboQueued && currentCombo < 5 && hitCooldownTimer <= 0f)
        {
            FaceMouseDirection();
            Debug.Log($"Chaining to attack {currentCombo + 1}");
            StartAttack(currentCombo + 1);
        }
    }

    // --- ANIMATION EVENT --- hit frame of each attack animation
    public void DealAttackDamage()
    {
        float facingDir = sprite.flipX ? -1f : 1f;

        Vector2 attackOrigin = (Vector2)transform.position
            + Vector2.right * facingDir * attackReach
            + Vector2.up * attackHeightOffset;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin, attackRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root == transform.root) continue;

            int damage = attackDamage[currentCombo - 1];
            Vector2 hitDir = (hit.transform.position - transform.position).normalized;

            // check for ground enemy
            EnemyAI groundEnemy = hit.GetComponent<EnemyAI>();
            if (groundEnemy == null) groundEnemy = hit.GetComponentInParent<EnemyAI>();
            if (groundEnemy == null) groundEnemy = hit.GetComponentInChildren<EnemyAI>();

            if (groundEnemy != null)
            {
                groundEnemy.TakeDamage(damage, hitDir);
                playerSounds?.PlayHitImpact();
                Debug.Log($"Hit ground enemy for {damage} damage");
                return;
            }

            // check for flying enemy
            FlyingEnemyAI flyingEnemy = hit.GetComponent<FlyingEnemyAI>();
            if (flyingEnemy == null) flyingEnemy = hit.GetComponentInParent<FlyingEnemyAI>();
            if (flyingEnemy == null) flyingEnemy = hit.GetComponentInChildren<FlyingEnemyAI>();

            if (flyingEnemy != null)
            {
                flyingEnemy.TakeDamage(damage, hitDir);
                playerSounds?.PlayHitImpact();
                Debug.Log($"Hit flying enemy for {damage} damage");
                return;
            }
        }
    }

    // --- ANIMATION EVENT --- last frame of each attack animation
    public void OnAttackEnd()
    {
        Debug.Log($"OnAttackEnd fired for attack {currentCombo}");

        animator.SetBool($"attack{currentCombo}", false);

        if (comboQueued && currentCombo < 5 && hitCooldownTimer <= 0f)
        {
            StartAttack(currentCombo + 1);
        }
        else
        {
            ResetAttack();
        }
    }

    void ResetAttack()
    {
        // restore full speed when attack ends
        playerMovement.moveSpeed = originalMoveSpeed;
        
        bool wasFullCombo = currentCombo >= 5;

        isAttacking = false;
        comboQueued = false;
        fallbackTimer = 0f;
        comboWindowTimer = 0f;

        playerMovement.isAttacking = false;
        animator.SetBool("isAttacking", false);

        for (int i = 1; i <= 5; i++)
            animator.SetBool($"attack{i}", false);

        // if player completed the full combo, apply the longer cooldown
        // otherwise just apply the short between-hit cooldown
        if (wasFullCombo)
        {
            comboCooldownTimer = comboCooldown;
            Debug.Log("Full combo done - applying combo cooldown");
        }
        else
        {
            hitCooldownTimer = timeBetweenHits;
        }

        Debug.Log("Attack reset");
    }

    void OnDrawGizmosSelected()
    {
        if (sprite != null)
        {
            float facingDir = sprite.flipX ? -1f : 1f;
            Vector2 origin = (Vector2)transform.position
                + Vector2.right * facingDir * attackReach
                + Vector2.up * attackHeightOffset;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, attackRadius);
        }
    }
}