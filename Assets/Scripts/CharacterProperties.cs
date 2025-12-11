using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProperties", menuName = "Characters/Properties")]
public class CharacterProperties : ScriptableObject
{
    [Header("Movement")]
    [Range(0f, 100f)] public float moveSpeed;
    [Min(0f)] public float acceleration, deceleration;
    
    [Header("Gravity")]
    public float baseGravity; 
    public float inAirGravity;
    [Range(-300f, -0.1f)] public float maxFallSpeed;

    [Header("Jump")]
    public float jumpForce;
    

    [Header("Physics Queries")]
    public LayerMask groundMask = 1;
    public float groundCheckSize = 5f;
}
