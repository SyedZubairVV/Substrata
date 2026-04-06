using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D body;
    public BoxCollider2D groundCheck;
    public LayerMask groundLayer;
    public SpriteRenderer sprite;
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 60f;
    public float airAcceleration = 40f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Dash")]
    public float dashPower = 15f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;

    [Header("Equipment")]
    public GameObject torchPrefab;

    [Header("Torch Settings")]
    public float torchOffset = 0.8f;

    [Header("Healing")]
    public int healAmount = 20;


    // set by PlayerCombat to stop movement script overriding sprite direction
    [HideInInspector] public bool isAttacking = false;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

	void Update()
    {
        // Read movement input
        moveInput = InputSystem.actions["Move"].ReadValue<Vector2>();

        // Check if player is touching the ground
        isGrounded = IsGrounded();

        if (InputSystem.actions["Jump"].WasPressedThisFrame() && isGrounded && !isDashing)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpForce);
        }

        // Start dash
        if (InputSystem.actions["Dash"].WasPressedThisFrame() && canDash)
        {
            StartCoroutine(Dash());
        }

        if (InputSystem.actions["Torch"].WasPressedThisFrame() && SceneManager.GetActiveScene().name != "Surface")
        {
            PlaceTorch();
        }

        if (InputSystem.actions["Potion"].WasPressedThisFrame())
        {
            UsePotion();
        }

        // only flip sprite based on movement direction when NOT attacking
        // when attacking, PlayerCombat controls the sprite direction via FaceMouseDirection
        if (!isAttacking)
        {
            if (moveInput.x > 0.01f)
                sprite.flipX = false;
            else if (moveInput.x < -0.01f)
                sprite.flipX = true;
        }

		// Update animations
		animator.SetBool("isRunning", Mathf.Abs(moveInput.x) > 0.01f);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", body.linearVelocity.y);
        animator.SetBool("isDashing", isDashing);
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (isDashing) return;

        // Target horizontal speed
        float targetSpeed = moveInput.x * moveSpeed;

        // Use different acceleration in air vs ground
        float accelRate = isGrounded ? acceleration : airAcceleration;

        // Smoothly move current velocity toward target speed
        float newVelocityX = Mathf.MoveTowards(
            body.linearVelocity.x,
            targetSpeed,
            accelRate * Time.fixedDeltaTime
        );

        body.linearVelocity = new Vector2(newVelocityX, body.linearVelocity.y);
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapBox(
            groundCheck.bounds.center,
            groundCheck.bounds.size,
            0f,
            groundLayer
        );
    }

    private System.Collections.IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = body.gravityScale;
        body.gravityScale = 0f; // Disable gravity during dash

        // Determine dash direction
        float dashDir;
        if (moveInput.x != 0)
            dashDir = Mathf.Sign(moveInput.x);
        else
            dashDir = sprite.flipX ? -1f : 1f;

        // Apply dash velocity
        body.linearVelocity = new Vector2(dashDir * dashPower, 0f);

        yield return new WaitForSeconds(dashTime);

        body.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    public bool IsGroundedPublic()
    {
        return IsGrounded();
    }

    void PlaceTorch()
    {
		// Check if player has any torches left
		if (InventoryManager.Instance.torchCount > 0)
		{
			// Subtract one from the inventory, update UI
            InventoryManager.Instance.UseTorch();

			// Spawn logic
			float direction = sprite.flipX ? -1f : 1f;
			Vector3 spawnPos = transform.position + new Vector3(direction * torchOffset, 1, 0);
			Instantiate(torchPrefab, spawnPos, Quaternion.identity);
		}
	}

    void UsePotion()
    {
        if (InventoryManager.Instance.potionCount <= 0)
            return;

        // Check if player exists
        if (PlayerHealth.Instance == null)
            return;

        int current = PlayerHealth.Instance.GetCurrentHealth();
        int max = PlayerHealth.Instance.maxHealth;

        // Don't allow potion use at full health
        if (current >= max)
        {
            return;
        }

        // Now it's safe to use potion
        InventoryManager.Instance.UsePotion();
        PlayerHealth.Instance.Heal(healAmount);
    }
}