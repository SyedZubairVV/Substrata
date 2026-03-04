using UnityEngine;
using UnityEngine.InputSystem;

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

        // Flip sprite depending on movement direction
        if (moveInput.x > 0.01f)
            sprite.flipX = false;
        else if (moveInput.x < -0.01f)
            sprite.flipX = true;

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
        // Check if ground collider overlaps with ground layer
        return Physics2D.OverlapArea(
            groundCheck.bounds.min,
            groundCheck.bounds.max,
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
}