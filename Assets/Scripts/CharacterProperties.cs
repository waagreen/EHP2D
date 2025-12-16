using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProperties", menuName = "Player/Character Properties")]
public class CharacterProperties : ScriptableObject
{
    [Range(0f, 1f)] public float CharacterInitalSize = 0.7f;

    [Header("Movement")]
    public float MoveSpeed = 8f;
    [Range(0f, 1f)] public float HoldingMultiplier = 0.8f;
    [Min(0f)] public float Acceleration = 15f;
    [Min(0f)] public float Deceleration = 12f;
    [Range(0f, 1f)] public float AccelerationCurve = 0.75f;
    [Range(0f, 1f)] public float AirControlMultiplier = 0.8f;

    [Header("Jump")]
    [Min(0f)] public float JumpHeight = 3f;
    [Min(0f)] public float JumpTimeToApex = 0.4f;
    [Min(1f)] public float JumpCutGravityMultiplier = 1.5f;
    [Min(0f)] public float JumpHangTimeThreshold = 0.5f;
    [Min(0f)] public float JumpHangGravityMultiplier = 0.5f;
    [Min(0f)] public float MaxFallSpeed = 15f;

    [Header("Coyote & Buffer")]
    [Min(0f)] public float CoyoteTime = 0.1f;
    [Min(0f)] public float JumpBufferTime = 0.15f;

    [Header("Throw")]
    [Min(0f)] public float MaxThrowForce = 10f;
    [Min(0f)] public float ThrowForceIncrement = 1f;

    [Header("Ground Check")]
    public float GroundCheckDistance = 0.1f;
    public LayerMask GroundLayer = ~0;
    
    // Public getters with calculation on demand
    public float GravityStrength => -(2 * JumpHeight) / (JumpTimeToApex * JumpTimeToApex);
    public float InitialJumpVelocity => Mathf.Abs(GravityStrength) * JumpTimeToApex;
}