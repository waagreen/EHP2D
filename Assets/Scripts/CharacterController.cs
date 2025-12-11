using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform groundCheck;
    [SerializeField] private CharacterProperties props;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private InputSystem_Actions actionMap;
    private Vector2 velocity;

    float horizontalInput;
    private bool isGrounded;
    private bool jumpHeld;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        actionMap = new();
        actionMap.Enable(); 

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        isGrounded = true;
    }

    private void Update()
    {
        CheckInputs();
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, Vector2.one * props.groundCheckSize, 0f, props.groundMask);
        velocity = rb.linearVelocity;

        HandleJump();
        AdjustGravity();
        AdjustVelocity();
        
        rb.linearVelocity = velocity;
    }

    private void CheckInputs()
    {
        horizontalInput = actionMap.Player.Move.ReadValue<Vector2>().x;
        jumpHeld = actionMap.Player.Jump.IsPressed();
    }

    private void HandleJump()
    {
        if (!jumpHeld && !isGrounded) return;
        velocity.y = props.jumpForce;
    }

    private void AdjustGravity()
    {
        if (velocity.y > 0)
        {
            float gravity = props.inAirGravity * (jumpHeld ? 1f : 3f);
            velocity.y += gravity * Time.fixedDeltaTime;
        }
        else
        {
            velocity.y += props.baseGravity * Time.fixedDeltaTime;
        }

        if (velocity.y < props.maxFallSpeed) velocity.y = props.maxFallSpeed;
    }


    private void AdjustVelocity()
    {
        float speed = horizontalInput * props.moveSpeed;

        if (horizontalInput != 0f)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, speed, props.acceleration * Time.deltaTime);
        }
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0f, props.deceleration * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, Vector2.one * props.groundCheckSize);
    }
}
