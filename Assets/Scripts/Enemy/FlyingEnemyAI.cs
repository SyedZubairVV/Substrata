using UnityEngine;
using System.Collections;

public class FlyingEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1.5f;

    [Header("Movement")]
    public float chaseSpeed = 3f;
    // how closely the enemy tries to match the player's position before attacking
    // increase this if the enemy overshoots and jitters around the player
    public float stoppingDistance = 1.2f;

    [Header("Hovering")]
    // when idle the enemy bobs up and down slightly to look alive
    public float hoverAmplitude = 0.15f;
    public float hoverSpeed = 2f;

    [Header("Stats")]
    public int maxHealth = 3;
    public int attackDamage = 1;
    public float attackCooldown = 1.2f;

    [Header("Attack Circle")]
    public float attackRadius = 0.6f;
    public float attackReach = 0.8f;
    public float attackHeightOffset = 0f;

    [Header("Contact Damage")]
    public int contactDamage = 1;
    public float contactDamageCooldown = 1f;

    [Header("Hit Flash")]
    public Color hitFlashColor = new Color(1f, 0.3f, 0.3f, 0.7f);
    public float hitFlashDuration = 0.15f;

    [Header("Drops")]
    public int goldDropAmount = 5;

    private int currentHealth;
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private SpriteRenderer sprite;
    private Color originalColor;

    private float attackTimer = 0f;
    private float contactDamageTimer = 0f;
    private bool isDead = false;
    private bool isAttacking = false;
    private float attackStateTimer = 0f;
    public float maxAttackStateDuration = 1.5f;

    // used for idle hovering
    private Vector3 startPosition;
    private float hoverTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalColor = sprite.color;
        currentHealth = maxHealth;
        startPosition = transform.position;

        // flying enemies don't use gravity
        rb.gravityScale = 0f;
    }

    void Update()
    {
        if (isDead) return;

        attackTimer -= Time.deltaTime;
        contactDamageTimer -= Time.deltaTime;
        hoverTimer += Time.deltaTime;

        // attack state timeout same as ground enemy
        if (isAttacking)
        {
            attackStateTimer += Time.deltaTime;
            if (attackStateTimer >= maxAttackStateDuration)
            {
                Debug.Log("Flying enemy attack timeout");
                ForceResetAttack();
            }
        }

        // use full 2D distance since this enemy moves in both X and Y
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange)
        {
            // stop moving and attack
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isMoving", false);
            FacePlayer();

            if (!isAttacking && attackTimer <= 0f)
                StartAttack();
        }
        else if (distToPlayer <= detectionRange)
        {
            // chase player in both X and Y directions
            if (!isAttacking)
            {
                ChasePlayer();
            }
        }
        else
        {
            // idle — hover in place with a gentle bob
            if (!isAttacking)
            {
                animator.SetBool("isMoving", false);
                Hover();
            }
        }
    }

    void ChasePlayer()
    {
        animator.SetBool("isMoving", true);
        animator.SetBool("isAttacking", false);

        Vector2 dir = (player.position - transform.position).normalized;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > stoppingDistance)
            rb.linearVelocity = dir * chaseSpeed;
        else
            rb.linearVelocity = Vector2.zero;

        // update hover origin to current position so when the enemy
        // goes idle it hovers from where it currently is, not where it started
        startPosition = transform.position;

        // reset hover timer so the bob starts smoothly from the new position
        hoverTimer = 0f;

        FacePlayer();
    }

    void Hover()
    {
        // gentle up and down bob when idle
        float newY = startPosition.y + Mathf.Sin(hoverTimer * hoverSpeed) * hoverAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        rb.linearVelocity = Vector2.zero;
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        attackStateTimer = 0f;
        animator.SetBool("isAttacking", true);
        rb.linearVelocity = Vector2.zero;

        // update hover origin here too
        startPosition = transform.position;
    }

    void ForceResetAttack()
    {
        isAttacking = false;
        attackStateTimer = 0f;
        animator.SetBool("isAttacking", false);
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
        ForceResetAttack();
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (isDead) return;
        currentHealth -= damage;

        StopCoroutine("HitFlash");
        StartCoroutine("HitFlash");

        if (!isAttacking)
            animator.SetTrigger("takingHit");

        if (currentHealth <= 0) Die();
    }

    IEnumerator HitFlash()
    {
        sprite.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        sprite.color = originalColor;
    }

    void Die()
    {
        isDead = true;
        animator.ResetTrigger("takingHit");
        animator.SetBool("isAttacking", false);
        animator.SetBool("isMoving", false);
        animator.SetBool("isDead", true);

        // re-enable gravity so the enemy falls to the ground on death
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;

        // disable main collider so player doesn't hover on the corpse
        // foot collider on child object stays active to stop it falling through floor
        BoxCollider2D mainCol = GetComponent<BoxCollider2D>();
        if (mainCol != null) mainCol.enabled = false;
        
        GoldManager.Instance?.AddGold(goldDropAmount);
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
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}