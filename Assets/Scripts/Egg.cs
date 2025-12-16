using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Egg : MonoBehaviour
{
    [SerializeField] private FixedJoint2D joint;
    [SerializeField] private SpriteRenderer sRenderer;

    private Rigidbody2D rb;

    private void Awake()
    {
        joint.enabled = false;
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetHolderRigidBody(Rigidbody2D rb, Transform connectionPoint)
    {        
        joint.connectedBody = rb;
        rb.linearVelocity = Vector2.zero;
        transform.SetParent(connectionPoint, true);
        transform.localPosition = Vector2.zero;
        joint.enabled = true;
    }

    public void ResetHolderRigidBody(Vector2 throwForce)
    {
        transform.SetParent(null, true);
        joint.enabled = false;
        rb.AddForce(throwForce, ForceMode2D.Impulse);
    }

    public void CanGrabFeedback(bool flag)
    {
        sRenderer.color = flag ? Color.yellow : Color.white;
    }
}
