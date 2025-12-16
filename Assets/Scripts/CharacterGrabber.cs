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

    private bool isHolding, canGrab;
    private float maxForce, forceIncrement, deltaForce, lastInputX;
    
    public bool IsHolding => isHolding;

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
        if (isHolding && egg && inputMap.Player.Attack.IsPressed())
        {
            ChargeThrow();
        }
    }

    private void RegisterLastInputX(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<Vector2>().x;
        if (value != 0) lastInputX = value;
    }

    private void OnAttackStarted(InputAction.CallbackContext ctx)
    {
        if (!isHolding && canGrab)
        {
            // Grab if we're not holding anything but can grab
            Grab();
        }
        else if (isHolding && egg)
        {
            // If already holding, start charging throw
            deltaForce = 0f;
        }
    }

    private void OnAttackCanceled(InputAction.CallbackContext ctx)
    {
        if (isHolding && egg)
        {
            // Release when button is released, with the charged force
            Release();
        }
    }

    private void ChargeThrow()
    {
        deltaForce += forceIncrement * Time.deltaTime;
        if (deltaForce >= maxForce)
        {
            deltaForce = maxForce;
            // Auto-release at max force
            Release();
        }
    }

    private void Grab()
    {
        if (egg || !potentialEgg) return;
        
        deltaForce = 0f;
        egg = potentialEgg;
        egg.SetHolderRigidBody(rb, connectionPoint);
        isHolding = true;
        
        potentialEgg = null;
    }

    private void Release()
    {
        if (!egg) return;
        
        Vector2 throwForce = deltaForce * lastInputX * Vector2.right; 

        egg.ResetHolderRigidBody(throwForce);
        egg = null;
        isHolding = false;
        deltaForce = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (egg || potentialEgg) return;
        if (((1 << collision.gameObject.layer) & grabbableMask) != 0)
        {
            if (collision.TryGetComponent(out Egg foundEgg))
            {
                canGrab = true;            
                potentialEgg = foundEgg;
                potentialEgg.CanGrabFeedback(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & grabbableMask) != 0)
        {
            canGrab = false;
            if (potentialEgg) potentialEgg.CanGrabFeedback(false);            
            potentialEgg = null;
        }
    }
}