using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Egg : MonoBehaviour
{
    [SerializeField][Min(0f)] private float followSpeed = 25f;
    [SerializeField][Min(0f)] private float maxFollowForce = 100f;
    [SerializeField][Range(0f, 5f)] private float damping = 5f;
    [SerializeField] private SpriteRenderer sRenderer;
    [SerializeField] LayerMask ignoredWhileHeldMask = 0;

    private Rigidbody2D rb, holderRb;
    private Transform connectionPoint;
    private bool isHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!isHeld || connectionPoint == null || holderRb == null) return;

        Vector2 targetPosition = connectionPoint.position;

        // Velocity needed to reach target
        Vector2 positionError = targetPosition - rb.position;
        Vector2 desiredVelocity = positionError * followSpeed;

        // Required force
        Vector2 velocityError = desiredVelocity - rb.linearVelocity;
        Vector2 force = velocityError * damping;

        // Clamp to prevent excessive influence
        force = Vector2.ClampMagnitude(force, maxFollowForce);

        rb.AddForce(force);

        if (positionError.magnitude < 0.1f)
        {
            rb.linearVelocity = new(holderRb.linearVelocityX, rb.linearVelocityY);
        }
    }

    public void SetHolderRigidBody(Rigidbody2D holderRb, Transform connectionPoint)
    {
        this.holderRb = holderRb;
        this.connectionPoint = connectionPoint;
        isHeld = true;

        // Disable gravity and apply slight drag for stability
        rb.gravityScale = 0f;
        rb.linearDamping = rb.angularDamping = 2f;
        rb.excludeLayers = ignoredWhileHeldMask;

        if (holderRb != null)
        {
            rb.linearVelocity = holderRb.linearVelocity;
        }
    }

    public void ResetHolderRigidBody(Vector2 throwForce)
    {
        isHeld = false;
        
        // Restore physics properties
        rb.gravityScale = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.excludeLayers = 0;
        
        connectionPoint = null;
        holderRb = null;

        rb.AddForce(throwForce, ForceMode2D.Impulse);
    }

    public void CanGrabFeedback(bool flag)
    {
        sRenderer.color = flag ? Color.yellow : Color.white;
    }
}
