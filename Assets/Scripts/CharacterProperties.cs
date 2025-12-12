using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProperties", menuName = "Player/Character Properties")]
public class CharacterProperties : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 12f;
    [SerializeField] private float airControlMultiplier = 0.8f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpTimeToApex = 0.4f;
    [SerializeField] private float jumpCutGravityMultiplier = 1.5f;
    [SerializeField] private float jumpHangTimeThreshold = 0.5f;
    [SerializeField] private float jumpHangGravityMultiplier = 0.5f;
    [SerializeField] private float maxFallSpeed = 15f;

    [Header("Coyote & Buffer")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer = ~0;
    
    // Public getters with calculation on demand
    public float GravityStrength => -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);
    public float InitialJumpVelocity => Mathf.Abs(GravityStrength) * jumpTimeToApex;
    
    // Public getters
    public float MoveSpeed => moveSpeed;
    public float Acceleration => acceleration;
    public float Deceleration => deceleration;
    public float AirControlMultiplier => airControlMultiplier;
    public float JumpCutGravityMultiplier => jumpCutGravityMultiplier;
    public float JumpHangTimeThreshold => jumpHangTimeThreshold;
    public float JumpHangGravityMultiplier => jumpHangGravityMultiplier;
    public float MaxFallSpeed => maxFallSpeed;
    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;
    public float GroundCheckDistance => groundCheckDistance;
    public LayerMask GroundLayer => groundLayer;
}