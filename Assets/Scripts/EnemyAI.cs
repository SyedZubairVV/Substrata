using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;  // how far the enemy can see the player
    public float attackRange = 2f;     // how close before enemy stops and attacks

    [Header("Movement")]
    public float chaseSpeed = 3f;

    [Header("Stats")]
    public int maxHealth = 3;
    public int attackDamage = 1;
    public float attackCooldown = 1.2f; // seconds between each attack

    [Header("Attack Circle")]
    public float attackRadius = 0.5f;       // size of the damage circle
    public float attackReach = 1.26f;       // how far in front of enemy the circle appears
    public float attackHeightOffset = -0.3f; // move circle up/down to match player height

    [Header("Contact Damage")]
    public int contactDamage = 1;
    public float contactDamageCooldown = 1f; // seconds between contact damage hits

    private int currentHealth;
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private SpriteRenderer sprite;

    private float attackTimer = 0f;
    private float contactDamageTimer = 0f;
    private float attackingFallbackTimer = 0f;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        // grab all components on this GameObject
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        // find the player once at the start so we always have a reference to them
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentHealth = maxHealth;
    }

    void Update()
    {
        //if (isDead) return;
        // TEST DAMAGE (P key)
        //if (Keyboard.current.oKey.wasPressedThisFrame)
        //{
        //    TakeDamage(1);
        //}

        attackTimer -= Time.deltaTime;
        contactDamageTimer -= Time.deltaTime;

        // FALLBACK TIMER
        // if the attack animation event OnAttackEnd() somehow never fires,
        // this forces isAttacking back to false after the cooldown expires
        // so the enemy never gets permanently stuck in the attacking state
        if (isAttacking)
        {
            attackingFallbackTimer -= Time.deltaTime;
            if (attackingFallbackTimer <= 0f)
            {
                isAttacking = false;
                animator.SetBool("isAttacking", false);
            }
        }

        // DISTANCE CHECK
        // only measure horizontal distance so that the player's height difference
        // doesn't stop the enemy from attacking while standing next to them
        float horizDist = Mathf.Abs(player.position.x - transform.position.x);

        // ATTACK STATE
        // player is within attack range
        if (horizDist <= attackRange)
        {
            // stop moving horizontally, keep vertical velocity for gravity
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isMoving", false);

            // always face the player
            FacePlayer();

            // start a new attack only if not already mid-attack and cooldown is done
            if (!isAttacking && attackTimer <= 0f)
                StartAttack();
        }
        // CHASE STATE
        // player is spotted but not in attack range yet
        else if (horizDist <= detectionRange)
        {
            if (!isAttacking)
            {
                animator.SetBool("isAttacking", false);
                animator.SetBool("isMoving", true);

                // move only on X axis toward the player
                // keeping rb.linearVelocity.y means gravity still applies normally
                float dirX = player.position.x - transform.position.x;
                float moveDir = dirX > 0 ? 1f : -1f;
                rb.linearVelocity = new Vector2(moveDir * chaseSpeed, rb.linearVelocity.y);

                sprite.flipX = moveDir < 0; // flip sprite to face movement direction
            }
        }
        // IDLE STATE
        // player is out of range, stand still
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
        animator.SetBool("isAttacking", true);

        // fallback is slightly longer than the cooldown so it only
        // kicks in if the animation event genuinely never fired
        attackingFallbackTimer = attackCooldown + 0.5f;
    }

    // ANIMATION EVENT
    // hit frame of attack animation
    // fires an invisible circle in front of the enemy at that exact frame
    public void DealAttackDamage()
    {
        float facingDir = sprite.flipX ? -1f : 1f;

        // calculate where the circle appears:
        // start at enemy center, push forward by attackReach, adjust height by offset
        Vector2 attackOrigin = (Vector2)transform.position
            + Vector2.right * facingDir * attackReach
            + Vector2.up * attackHeightOffset;

        // get every collider inside that circle
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackOrigin, attackRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root == transform.root) continue; // skip ourselves

            // check if the hit object is part of the player's hierarchy
            if (hit.transform.root == player.transform.root)
            {
                // search the whole player hierarchy for PlayerHealth
                // this works even if PlayerHealth is on a parent or child object
                PlayerHealth ph = player.transform.root.GetComponentInChildren<PlayerHealth>();
                if (ph != null)
                    ph.TakeDamage(attackDamage);
                return; // stop after hitting player once
            }
        }
    }

    // ANIMATION EVENT
    // last frame of attack animation
    // tells the script the animation has finished so a new attack can begin
    public void OnAttackEnd()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    // called externally when the player attacks this enemy
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        animator.SetTrigger("takingHit");
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        animator.ResetTrigger("takingHit"); // clear any pending takingHit trigger
        animator.SetBool("isAttacking", false);
        animator.SetBool("isMoving", false);
        animator.SetBool("isDead", true);

        // stop horizontal movement but keep gravity on
        // so the enemy falls to the ground naturally during the death animation
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // lock horizontal movement so they don't slide around while dying
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        Destroy(gameObject, 2f);                    // removes the object after death anim plays
    }

    // fires every frame the player is physically touching the enemy body
    // separate cooldown from attack so bumping and attacking don't block each other
    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Player") && contactDamageTimer <= 0f)
        {
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);
            contactDamageTimer = contactDamageCooldown;
        }
    }

    void FacePlayer()
    {
        float dirX = player.position.x - transform.position.x;
        sprite.flipX = dirX < 0; // negative X means player is to the left, so flip
    }

    // draws visual helpers in the Scene view while the enemy is selected
    void OnDrawGizmosSelected()
    {
        if (sprite != null)
        {
            float facingDir = sprite.flipX ? -1f : 1f;
            Vector2 attackOrigin = (Vector2)transform.position
                + Vector2.right * facingDir * attackReach
                + Vector2.up * attackHeightOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin, attackRadius); // red = attack circle
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange); // yellow = detection range

        // blue vertical lines show the horizontal attack range on both sides
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