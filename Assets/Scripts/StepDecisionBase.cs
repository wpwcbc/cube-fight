using UnityEngine;

public abstract class StepDecisionBase : MonoBehaviour
{
    // Return one cardinal step (Vector2Int.right/left/up/down) or Vector2Int.zero to skip.
    public abstract Vector2Int DecideStep(GridAgent self);

    // Let decisions suggest a single alternate step if the primary is blocked.
    public virtual bool TryGetAlternate(GridAgent self, Vector2Int blockedPrimary, out Vector2Int alt)
    {
        alt = Vector2Int.zero; return false;
    }

    protected static Vector2Int QuantizeCardinal(Vector2Int v)
    {
        if (v == Vector2Int.zero) return Vector2Int.zero;
        if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
            return v.x >= 0 ? Vector2Int.right : Vector2Int.left;
        else
            return v.y >= 0 ? Vector2Int.up : Vector2Int.down;
    }
}
