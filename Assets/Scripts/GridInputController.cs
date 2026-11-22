using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GridAgent))]
[RequireComponent(typeof(PlayerInput))]
public class GridInputController : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)]
    private float deadzone = 0.5f;   // ignore tiny stick/analog noise

    private GridAgent _agent;
    private PlayerInput _playerInput;
    private InputAction _move;

    // we step once per press & only again when the direction changes or the input is released
    private Vector2Int _lastStepDir = Vector2Int.zero;

    void Awake()
    {
        _agent = GetComponent<GridAgent>();
        _playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        _move = _playerInput.actions["Move"]; // must match the action name in your asset
        _move.started += OnMoveStarted;      // fires when a direction first actuates
        _move.performed += OnMovePerformed;   // fires on direction changes while held (stick/D-pad)
        _move.canceled += OnMoveCanceled;     // reset when released
    }

    void OnDisable()
    {
        if (_move == null) return;
        _move.started -= OnMoveStarted;
        _move.performed -= OnMovePerformed;
        _move.canceled -= OnMoveCanceled;
    }

    void OnMoveStarted(InputAction.CallbackContext ctx)
    {
        TryStep(ctx.ReadValue<Vector2>());
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        // If the held direction changes (e.g., Right -> Up), allow another step.
        var dir = Quantize(ctx.ReadValue<Vector2>());
        if (dir != Vector2Int.zero && dir != _lastStepDir)
        {
            TryStep(ctx.ReadValue<Vector2>());
        }
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _lastStepDir = Vector2Int.zero;
    }

    void TryStep(Vector2 input)
    {
        var dir = Quantize(input);
        if (dir == Vector2Int.zero)
        {
            return;
        }

        if (_agent.TryMove(dir))
        {
            _lastStepDir = dir;
        }
    }

    Vector2Int Quantize(Vector2 v)
    {
        if (Mathf.Abs(v.x) < deadzone && Mathf.Abs(v.y) < deadzone)
            return Vector2Int.zero;

        // Pick the dominant axis to enforce cardinal steps only.
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return v.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            return v.y > 0 ? Vector2Int.up : Vector2Int.down;
    }

    public void UI_StepUp()
    {
        TryStep(Vector2Int.up);
    }
    public void UI_StepDown()
    {
        TryStep(Vector2Int.down);
    }
    public void UI_StepLeft()
    {
        TryStep(Vector2Int.left);
    }
    public void UI_StepRight()
    {
        TryStep(Vector2Int.right);
    }
}