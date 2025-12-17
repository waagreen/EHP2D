using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterGrabber : MonoBehaviour
{
    [SerializeField] private CircleCollider2D range;
    [SerializeField] private LayerMask grabbableMask;
    [SerializeField] private Transform connectionPoint;
    
    private InputSystem_Actions inputMap;
    private Rigidbody2D rb;
    private Egg egg, potentialEgg;

    private float maxForce, forceIncrement, deltaForce, lastInputX, grabTime;
    private const float kTapThreshold = 0.2f;

    public bool IsHolding => egg;

    public void Setup(InputSystem_Actions inputMap, Rigidbody2D rb, float maxForce, float forceIncrement)
    {
        this.rb = rb;
        this.maxForce = maxForce;
        this.forceIncrement = forceIncrement;
        this.inputMap = inputMap;
        
        inputMap.Player.Attack.started += OnAttackStarted;
        inputMap.Player.Attack.canceled += OnAttackCanceled;
        inputMap.Player.Move.started += RegisterLastInputX;
    }

    private void FixedUpdate()
    {
        if (IsHolding && inputMap.Player.Attack.IsPressed())
        {
            deltaForce += forceIncrement * Time.deltaTime;
            if (deltaForce > maxForce)
            {
                deltaForce = maxForce;
                Release();
            }
        }
    }

    private void RegisterLastInputX(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<Vector2>().x;
        if (value != 0) lastInputX = value;
    }

    private void OnAttackStarted(InputAction.CallbackContext ctx)
    {
        if (!IsHolding)
        {
            grabTime = Time.time;
            Grab();
        }
    }

    private void OnAttackCanceled(InputAction.CallbackContext ctx)
    {
        float holdDuration = Time.time - grabTime;
        if (IsHolding && (holdDuration > kTapThreshold))
        {
            Release();
        }
    }

    private void Grab()
    {
        if (egg || !potentialEgg) return;
        
        deltaForce = 0f;
        egg = potentialEgg;
        egg.SetHolderRigidBody(rb, connectionPoint);
        egg.CanGrabFeedback(false);
        
        potentialEgg = null;
    }

    private void Release()
    {
        if (!egg) return;
        
        Vector2 throwForce = deltaForce * lastInputX * Vector2.right; 

        egg.ResetHolderRigidBody(throwForce);
        egg = null;
        deltaForce = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsHolding || potentialEgg) return;
        if (((1 << collision.gameObject.layer) & grabbableMask) != 0)
        {
            if (collision.TryGetComponent(out Egg foundEgg))
            {
                potentialEgg = foundEgg;
                potentialEgg.CanGrabFeedback(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & grabbableMask) != 0)
        {
            if (potentialEgg) potentialEgg.CanGrabFeedback(false);            
            potentialEgg = null;
        }
    }
}