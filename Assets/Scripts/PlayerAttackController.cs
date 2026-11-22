// PlayerAttackController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(GridAgent))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerAttackController : MonoBehaviour
{
    [Tooltip("Index 0->Attack1, 1->Attack2, 2->Attack3")]
    public List<AttackMove> moves;
    public float effectLifetime = 0.25f;

    GridAgent _agent;
    PlayerInput _pi;

    void Awake()
    {
        _agent = GetComponent<GridAgent>();
        _pi = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        Bind("Attack1", () => TryCast(0));
        Bind("Attack2", () => TryCast(1));
        Bind("Attack3", () => TryCast(2));
    }

    void Bind(string actionName, System.Action fire)
    {
        InputAction action = _pi.actions[actionName];
        if (action != null) action.performed += _ => fire(); // Use Press interaction in your actions
    }

    void TryCast(int index)
    {
        if (index < 0 || index >= moves.Count) return;
        AttackMove move = moves[index];
        Debug.Log(move + " " + move.IsReady + " " + move.shape);
        if (move == null || !move.IsReady || move.shape == null) return;

        var center = _agent.CurrentGridPos;

        // 1) Compute target cells deterministically (grid logic > physics here)
        var targets = new HashSet<Vector2Int>();
        foreach (var off in move.shape.GetOffsets(move.size))
        {
            var cell = center + off;
            if (GridMath.IsWithinBounds(cell)) targets.Add(cell);
        }

        // 2) Apply damage: destroy/disable enemies whose grid pos ∈ targets
        // For best perf, use your EnemyManager's cached list or an EnemyRegistry.
        EnemyStepper[] enemies = EnemyManager.Instance._enemies.ToArray();
        foreach (EnemyStepper e in enemies)
        {
            if (e && e.Agent && targets.Contains(e.Agent.CurrentGridPos))
            {
                var actor = e.GetComponent<EnemyActor>();
                if (actor) actor.Kill();
                else e.gameObject.SetActive(false); // fallback
            }
        }

        // 3) Optional effect & audio
        if (move.effectPrefab != null)
        {
            Vector3 worldCenter = GridMath.GridToWorldCenter(center, transform.position.y);
            GameObject fx = Instantiate(move.effectPrefab, worldCenter, Quaternion.identity);
            LaserRingEffect lr = fx.GetComponent<LaserRingEffect>();
            if (lr) lr.Setup(worldCenter, move.size, GridSettings.cellSize);
            Destroy(fx, effectLifetime);
        }
        if (move.sfx) AudioSource.PlayClipAtPoint(move.sfx, transform.position);

        move.MarkUsed();
    }
}
