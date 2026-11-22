using UnityEngine;

[DisallowMultipleComponent]
public class ChaseStepDecision : StepDecisionBase
{
    [Header("Target")]
    public GridAgent targetAgent;       // drag the player's GridAgent here
    public string targetTag = "Player"; // optional: auto-find by tag

    [Header("Tie Break")]
    public bool preferHorizontalOnTie = true;

    void Awake()
    {
        if (!targetAgent && !string.IsNullOrEmpty(targetTag))
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go) targetAgent = go.GetComponent<GridAgent>();
        }
    }

    public override Vector2Int DecideStep(GridAgent self)
    {
        if (!targetAgent) return Vector2Int.zero;

        var from = self.CurrentGridPos;
        var to = targetAgent.CurrentGridPos;
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        if (dx == 0 && dy == 0) return Vector2Int.zero;

        int ax = Mathf.Abs(dx);
        int ay = Mathf.Abs(dy);

        if (ax > ay) return dx > 0 ? Vector2Int.right : Vector2Int.left;
        else if (ay > ax) return dy > 0 ? Vector2Int.up : Vector2Int.down;
        else return preferHorizontalOnTie
                                ? (dx > 0 ? Vector2Int.right : Vector2Int.left)
                                : (dy > 0 ? Vector2Int.up : Vector2Int.down);
    }

    public override bool TryGetAlternate(GridAgent self, Vector2Int blockedPrimary, out Vector2Int alt)
    {
        alt = Vector2Int.zero;
        if (!targetAgent) return false;

        var from = self.CurrentGridPos;
        var to = targetAgent.CurrentGridPos;
        int dx = to.x - from.x, dy = to.y - from.y;

        // If primary was horizontal, try vertical toward the player; else try horizontal.
        if (blockedPrimary.x != 0 && dy != 0)
            alt = dy > 0 ? Vector2Int.up : Vector2Int.down;
        else if (blockedPrimary.y != 0 && dx != 0)
            alt = dx > 0 ? Vector2Int.right : Vector2Int.left;

        return alt != Vector2Int.zero;
    }
}
