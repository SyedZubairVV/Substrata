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
    public float jumpSpeed = 14f;

    [Header("Jump Forgiveness")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Gravity")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    Vector2 moveInput;

    float coyoteTimer;
    float jumpBufferTimer;

    bool isGrounded;
    bool wasGrounded;

    void Update()
    {
        moveInput = InputSystem.actions["Move"].ReadValue<Vector2>();

        if (InputSystem.actions["Jump"].WasPressedThisFrame())
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        isGrounded = IsGrounded();

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (InputSystem.actions["Jump"].WasReleasedThisFrame() && body.linearVelocity.y > 0)
        {
            body.linearVelocity = new Vector2(
                body.linearVelocity.x,
                body.linearVelocity.y * 0.5f
            );
        }

        animator.SetBool("isWalking", Mathf.Abs(moveInput.x) > 0.01f);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", body.linearVelocity.y);

        if (!wasGrounded && isGrounded)
        {
            animator.SetTrigger("land");
        }


        wasGrounded = isGrounded;

        if (moveInput.x > 0.01f)
            sprite.flipX = false;
        else if (moveInput.x < -0.01f)
            sprite.flipX = true;
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
        ApplyBetterGravity();
    }

    void HandleMovement()
    {
        float targetSpeed = moveInput.x * moveSpeed;

        float accelRate = isGrounded ? acceleration : airAcceleration;

        float newVelocityX = Mathf.MoveTowards(
            body.linearVelocity.x,
            targetSpeed,
            accelRate * Time.fixedDeltaTime
        );

        body.linearVelocity = new Vector2(
            newVelocityX,
            body.linearVelocity.y
        );
    }

    void HandleJump()
    {
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            body.linearVelocity = new Vector2(
                body.linearVelocity.x,
                jumpSpeed
            );

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
    }

    void ApplyBetterGravity()
    {
        if (body.linearVelocity.y < 0)
        {
            body.linearVelocity += Vector2.up * Physics2D.gravity.y *
                (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (body.linearVelocity.y > 0 && !InputSystem.actions["Jump"].IsPressed())
        {
            body.linearVelocity += Vector2.up * Physics2D.gravity.y *
                (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapAreaAll(
            groundCheck.bounds.min,
            groundCheck.bounds.max,
            groundLayer
        ).Length > 0;
    }
}
