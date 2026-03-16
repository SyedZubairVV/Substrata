using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 2f;

    [Header("Movement")]
    public float chaseSpeed = 3f;

    [Header("Stats")]
    public int maxHealth = 3;
    public int attackDamage = 1;
    public float attackCooldown = 1.2f;

    [Header("Knockback")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.15f;

    [Header("Attack Circle")]
    public float attackRadius = 0.5f;
    public float attackReach = 1.26f;
    public float attackHeightOffset = -0.3f;

    [Header("Contact Damage")]
    public int contactDamage = 1;
    public float contactDamageCooldown = 1f;

    private int currentHealth;
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private SpriteRenderer sprite;

    private float attackTimer = 0f;
    private float contactDamageTimer = 0f;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isKnockedBack = false;

    // counts up while attacking — if it exceeds maxAttackStateDuration
    // the enemy is forced out of the attack state regardless of animation events
    private float attackStateTimer = 0f;
    public float maxAttackStateDuration = 0.4f;
    private EnemySounds enemySounds;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        enemySounds = GetComponent<EnemySounds>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;
        if (isKnockedBack) return;

        attackTimer -= Time.deltaTime;
        contactDamageTimer -= Time.deltaTime;

        // --- ATTACK STATE TIMEOUT ---
        // counts up every frame while isAttacking is true
        // if the animation event OnAttackEnd never fires, this forces a reset
        // maxAttackStateDuration should be slightly longer than your attack animation
        if (isAttacking)
        {
            attackStateTimer += Time.deltaTime;
            if (attackStateTimer >= maxAttackStateDuration)
            {
                Debug.Log("Attack timeout - forcing reset");
                ForceResetAttack();
            }
        }

        float horizDist = Mathf.Abs(player.position.x - transform.position.x);

        // --- ATTACK STATE ---
        if (horizDist <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isMoving", false);
            FacePlayer();

            if (!isAttacking && attackTimer <= 0f)
                StartAttack();
        }
        // --- CHASE STATE ---
        else if (horizDist <= detectionRange)
        {
            // play aggro sound the first time enemy spots the player
            enemySounds?.PlayAggro();
            
            if (!isAttacking)
            {
                animator.SetBool("isAttacking", false);
                animator.SetBool("isMoving", true);

                float dirX = player.position.x - transform.position.x;
                float moveDir = dirX > 0 ? 1f : -1f;
                rb.linearVelocity = new Vector2(moveDir * chaseSpeed, rb.linearVelocity.y);
                sprite.flipX = moveDir < 0;
            }
        }
        // --- IDLE STATE ---
        else
        {
            if (!isAttacking)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("isMoving", false);
            }
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        attackStateTimer = 0f; // reset timer on every new attack
        animator.SetBool("isAttacking", true);
        enemySounds?.PlayAttack();
        Debug.Log("Enemy attack started");
    }

    // single place to reset attack state
    // called by both OnAttackEnd animation event AND the timeout
    void ForceResetAttack()
    {
        isAttacking = false;
        attackStateTimer = 0f;
        animator.SetBool("isAttacking", false);
        Debug.Log("Enemy attack reset");
    }

    // --- ANIMATION EVENT --- hit frame
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
            if (hit.transform.root == player.transform.root)
            {
                PlayerHealth ph = player.transform.root.GetComponentInChildren<PlayerHealth>();
                if (ph != null)
                {
                    Vector2 hitDir = (player.position - transform.position).normalized;
                    ph.TakeDamage(attackDamage, hitDir);
                }
                return;
            }
        }
    }

    // --- ANIMATION EVENT --- last frame
    public void OnAttackEnd()
    {
        Debug.Log("Enemy OnAttackEnd fired");
        ForceResetAttack();
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (!isAttacking)
        {
            animator.SetTrigger("takingHit");
            StartCoroutine(Knockback(hitDirection));
        }

        if (currentHealth <= 0) Die();
    }

    IEnumerator Knockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.linearVelocity = new Vector2(direction.x * knockbackForce, knockbackForce * 0.5f);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
        rb.linearVelocity = Vector2.zero;
    }

    void Die()
    {
        isDead = true;
        animator.ResetTrigger("takingHit");
        animator.SetBool("isAttacking", false);
        animator.SetBool("isMoving", false);
        animator.SetBool("isDead", true);
        enemySounds?.PlayDeath();
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        if (col == null) col = GetComponentInChildren<CapsuleCollider2D>();
        if (col != null)
            col.offset = new Vector2(col.offset.x, col.offset.y - 0.28f);

        Destroy(gameObject, 5f);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Player") && contactDamageTimer <= 0f)
        {
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(contactDamage, hitDir);
            contactDamageTimer = contactDamageCooldown;
        }
    }

    void FacePlayer()
    {
        float dirX = player.position.x - transform.position.x;
        sprite.flipX = dirX < 0;
    }

    void OnDrawGizmosSelected()
    {
        if (sprite != null)
        {
            float facingDir = sprite.flipX ? -1f : 1f;
            Vector2 attackOrigin = (Vector2)transform.position
                + Vector2.right * facingDir * attackReach
                + Vector2.up * attackHeightOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin, attackRadius);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - attackRange, transform.position.y - 2f),
            new Vector3(transform.position.x - attackRange, transform.position.y + 2f)
        );
        Gizmos.DrawLine(
            new Vector3(transform.position.x + attackRange, transform.position.y - 2f),
            new Vector3(transform.position.x + attackRange, transform.position.y + 2f)
        );
    }
}