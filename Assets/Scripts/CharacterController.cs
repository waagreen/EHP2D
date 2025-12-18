using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private CharacterProperties stats;
    
    // Components
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private CharacterGrabber grabber;
    
    // Input
    private InputSystem_Actions actionMap;
    private PlayerInputs currentInputs;
    
    // State
    private bool isGrounded;
    private bool isJumping;
    private bool isFalling;
    private bool hitCeilingThisFrame;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float lastJumpPressedTime;
    private float gravityScale;
    private float lastInputX;
    
    // Cached scale
    private Vector2 lastScale;
    private Vector2 scaledColliderSize;
    private Vector2 scaledGroundCheckSize;
    private Vector2 ScaledUpVector => Vector2.up * (scaledColliderSize.y * 0.5f);
    private Vector2 LocalScaledVector => Vector2.Scale(col.offset, transform.localScale);
    
    // Constants
    private const float SKIN_WIDTH = 0.02f;
    private const float CEILING_CHECK_DISTANCE = 0.1f;
    
    private float GravityScale
    {
        get
        {
            if (stats == null) return 0f;
            return stats.GravityStrength / Physics2D.gravity.y;
        }
    }
    
    private float JumpVelocity
    {
        get
        {
            if (stats == null) return 0f;
            return stats.InitialJumpVelocity;
        }
    }
    
    public struct PlayerInputs
    {
        public bool jumpHeld;
        public bool jumpDown;
        public Vector2 movement;
    }
    
    #region Unity Lifecycle

    private void Start()
    {
        col = GetComponent<CapsuleCollider2D>();
        
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        if(TryGetComponent(out CharacterGrabber attachedGrabber)) grabber = attachedGrabber;

        actionMap = new InputSystem_Actions();
        actionMap.Enable();
        grabber.Setup(actionMap, rb, stats.MaxThrowForce, stats.ThrowForceIncrement);

        transform.localScale = Vector3.one * stats.CharacterInitalSize;
        CacheScale();
    }
    
    private void OnDestroy()
    {
        actionMap?.Disable();
        actionMap = null;
    }
    
    private void Update()
    {
        // Recalculate scaled values if scale changed
        if (transform.localScale != (Vector3)lastScale)
        {
            CacheScale();
        }
        
        ReadInputs();
        HandleTimers();
        HandleJumpInput();
    }
    
    private void FixedUpdate()
    {
        UpdateGroundCheck();
        UpdateCeilingCheck();
        
        HandleHorizontalMovement();
        HandleVerticalMovement();
        
        ApplyGravity();
        ClampFallSpeed();
    }
    
    #endregion
    
    #region Initialization
    
    private void CacheScale()
    {
        lastScale = transform.localScale;
        
        // Scale the collider size by the transform's scale
        scaledColliderSize = new Vector2
        (
            col.size.x * Mathf.Abs(transform.localScale.x),
            col.size.y * Mathf.Abs(transform.localScale.y)
        );
        
        // Ground check width should be slightly smaller than collider
        scaledGroundCheckSize = new Vector2
        (
            scaledColliderSize.x * 0.95f,
            stats.GroundCheckDistance * Mathf.Abs(transform.localScale.y)
        );
    }
    
    #endregion
    
    #region Input Handling
    
    private void ReadInputs()
    {
        currentInputs = new PlayerInputs()
        {
            jumpDown = actionMap.Player.Jump.WasPressedThisFrame(),
            jumpHeld = actionMap.Player.Jump.IsPressed(),
            movement = actionMap.Player.Move.ReadValue<Vector2>()
        };

        if (currentInputs.movement.x != 0) lastInputX = currentInputs.movement.x;

        if (currentInputs.jumpDown)
        {
            lastJumpPressedTime = Time.time;
        }
    }
    
    private void HandleTimers()
    {
        if (stats == null) return;
        
        // Coyote time
        coyoteTimeCounter = isGrounded ? stats.CoyoteTime : coyoteTimeCounter - Time.deltaTime;
        
        // Jump buffer
        jumpBufferCounter = currentInputs.jumpDown ? stats.JumpBufferTime : jumpBufferCounter - Time.deltaTime;
    }
    
    private void HandleJumpInput()
    {
        if (jumpBufferCounter > 0f && CanJump())
        {
            ExecuteJump();
        }
    }
    
    #endregion
    
    #region Movement
    
    private void HandleHorizontalMovement()
    {
        if (stats == null) return;
        
        float targetSpeed = currentInputs.movement.x * stats.MoveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelerationRate = Mathf.Abs(targetSpeed) > 0.01f ? stats.Acceleration : stats.Deceleration;
        
        // Reduce air control
        if (!isGrounded)
        {
            accelerationRate *= stats.AirControlMultiplier;
        }
        
        // Non-linear acceleration for smoother movement 
        // “When I’m far from my target speed, accelerate smoothly. When I’m close, correct aggressively.”
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelerationRate, stats.AccelerationCurve) * Mathf.Sign(speedDiff);
        rb.AddForce(movement * Vector2.right);
        
        // Re-orient transform based on input direction
        Vector3 newScale = Vector3.one * stats.CharacterInitalSize;
        newScale.x *= Mathf.Sign(lastInputX);
        transform.localScale = newScale;
    }
    
    private void HandleVerticalMovement()
    {
        if (hitCeilingThisFrame && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            isJumping = false;
        }
    }
    
    private void ApplyGravity()
    {
        // Stop applying force when grounded
        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -0.1f);
            isJumping = false;
            isFalling = false;
            return;
        }
        
        float currentGravity = GravityScale;
        
        // Jump cut (quick fall when jump button released)
        if (!currentInputs.jumpHeld && rb.linearVelocity.y > 0 && isJumping)
        {
            currentGravity *= stats.JumpCutGravityMultiplier;
        }
        
        // Jump hang (float at peak of jump)
        if (Mathf.Abs(rb.linearVelocity.y) < stats.JumpHangTimeThreshold && isJumping)
        {
            currentGravity *= stats.JumpHangGravityMultiplier;
        }
        
        rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * currentGravity * Time.fixedDeltaTime);
        
        // Update jump/fall states
        if (rb.linearVelocity.y > 0 && !isGrounded)
        {
            isJumping = true;
            isFalling = false;
        }
        else if (rb.linearVelocity.y < 0 && !isGrounded)
        {
            isJumping = false;
            isFalling = true;
        }
    }
    
    private void ClampFallSpeed()
    {
        if (stats == null) return;
        
        if (rb.linearVelocity.y < -stats.MaxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -stats.MaxFallSpeed);
        }
    }
    
    private bool CanJump()
    {
        if (stats == null) return false;
        
        return (isGrounded || coyoteTimeCounter > 0) && !isJumping;
    }
    
    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpVelocity);
        isJumping = true;
        isFalling = false;
        coyoteTimeCounter = 0;
        jumpBufferCounter = 0;
    }
    
    #endregion
    
    #region Physics Checks
    
    
    private void UpdateGroundCheck()
    {
        if (stats == null) return;
        
        Vector2 checkPosition = (Vector2)transform.position + LocalScaledVector - ScaledUpVector;
        RaycastHit2D hit = Physics2D.BoxCast
        (
            checkPosition, 
            scaledGroundCheckSize, 
            0f, 
            Vector2.down, 
            SKIN_WIDTH * Mathf.Abs(transform.localScale.y), 
            stats.GroundLayer
        );
        
        isGrounded = hit.collider != null;
    }
    
    private void UpdateCeilingCheck()
    {
        if (stats == null) return;
        
        Vector2 checkPosition = (Vector2)transform.position + LocalScaledVector + ScaledUpVector;
        RaycastHit2D hit = Physics2D.BoxCast
        (
            checkPosition, 
            scaledGroundCheckSize, 
            0f, 
            Vector2.up, 
            CEILING_CHECK_DISTANCE * Mathf.Abs(transform.localScale.y), 
            stats.GroundLayer
        );
        
        hitCeilingThisFrame = hit.collider != null;
    }
    
    #endregion
    
    #region Helper Properties
    
    // Helper property to get world-space collider bounds
    public Bounds WorldColliderBounds
    {
        get
        {
            Vector2 scaledOffset = Vector2.Scale(col.offset, transform.localScale);
            Vector2 center = (Vector2)transform.position + scaledOffset;
            Vector2 size = scaledColliderSize;
            return new Bounds(center, size);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void Teleport(Vector2 position)
    {
        rb.position = position;
        rb.linearVelocity = Vector2.zero;
    }
    
    public void ResetVelocity()
    {
        rb.linearVelocity = Vector2.zero;
    }
    
    public void RefreshStats()
    {
        // Recalculate scale if needed
        if (transform.localScale != (Vector3)lastScale)
        {
            CacheScale();
        }
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        if (col == null || stats == null) return;
        
        Vector2 currentScale = Application.isPlaying ? lastScale : (Vector2)transform.localScale;
        Vector2 scaledSize = new
        (
            col.size.x * Mathf.Abs(currentScale.x),
            col.size.y * Mathf.Abs(currentScale.y)
        );
        
        Vector2 scaledOffset = Vector2.Scale(col.offset, currentScale);
        
        // Ground check
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector2 groundCheckPos = (Vector2)transform.position + scaledOffset - Vector2.up * (scaledSize.y * 0.5f);
        Vector2 groundCheckDisplaySize = new
        (
            scaledSize.x * 0.95f,
            stats.GroundCheckDistance * Mathf.Abs(currentScale.y)
        );
        
        Gizmos.DrawWireCube
        (
            groundCheckPos - Vector2.down * (stats.GroundCheckDistance * Mathf.Abs(currentScale.y) * 0.5f), 
            groundCheckDisplaySize
        );
        
        // Ceiling check
        Gizmos.color = Color.yellow;
        Vector2 ceilingCheckPos = (Vector2)transform.position + scaledOffset + Vector2.up * (scaledSize.y * 0.5f);
        Gizmos.DrawWireCube
        (
            ceilingCheckPos + Vector2.up * (CEILING_CHECK_DISTANCE * Mathf.Abs(currentScale.y) * 0.5f), 
            groundCheckDisplaySize
        );
        
        // Collider bounds
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube((Vector2)transform.position + scaledOffset, scaledSize);
    }
    
    #endregion
}