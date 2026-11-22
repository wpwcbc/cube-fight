using UnityEngine;

public enum CardinalDir { Right, Up, Left, Down }

[DisallowMultipleComponent]
public class SquareLoopStepDecision : StepDecisionBase
{
    [Header("Square Loop")]
    [Min(1)] public int sideLength = 3;          // cells per side
    public bool clockwise = true;                // loop direction
    public CardinalDir startDirection = CardinalDir.Right;
    [Tooltip("If the agent gets teleported or drifted off the loop, re-anchor the loop here.")]
    public bool reanchorIfTeleported = true;

    // runtime state
    Vector2Int _anchor;                          // corner we started from
    Vector2Int _lastPos;
    int _progress;                               // steps taken since anchor along current side [0..sideLength)
    CardinalDir _currentDir;
    bool _initialized;

    void OnValidate()
    {
        sideLength = Mathf.Max(1, sideLength);
    }

    public override Vector2Int DecideStep(GridAgent self)
    {
        EnsureInit(self);
        UpdateStateFromPosition(self);

        // Primary step is the current facing
        return ToVec(_currentDir);
    }

    // If primary is blocked (wall/reservation), turn early in the loop direction.
    public override bool TryGetAlternate(GridAgent self, Vector2Int blockedPrimary, out Vector2Int alt)
    {
        var turned = clockwise ? TurnCW(_currentDir) : TurnCCW(_currentDir);
        alt = ToVec(turned);
        return true;
    }

    // ----- helpers -----

    void EnsureInit(GridAgent self)
    {
        if (_initialized) return;
        _anchor = self.CurrentGridPos;   // start corner = where the enemy sits at level start
        _lastPos = _anchor;
        _currentDir = startDirection;
        _progress = 0;
        _initialized = true;
    }

    void UpdateStateFromPosition(GridAgent self)
    {
        if (!_initialized) return;

        var cur = self.CurrentGridPos;
        if (cur == _lastPos) return;

        var delta = cur - _lastPos;
        int manhattan = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);

        // If we didn't move by exactly one cardinal cell, assume teleport / external reposition.
        if (manhattan != 1)
        {
            if (reanchorIfTeleported)
            {
                _anchor = cur;
                _lastPos = cur;
                _currentDir = startDirection;
                _progress = 0;
            }
            else
            {
                _lastPos = cur; // keep going, but state may be off until it realigns
            }
            return;
        }

        // We advanced one step on the current side.
        _progress++;

        // Reached a corner? (end of side) -> rotate facing & reset side progress.
        if (_progress >= sideLength)
        {
            _currentDir = clockwise ? TurnCW(_currentDir) : TurnCCW(_currentDir);
            _progress = 0;
            // Optionally you could update _anchor here, but not required for deciding steps.
        }

        _lastPos = cur;
    }

    static Vector2Int ToVec(CardinalDir d)
    {
        switch (d)
        {
            case CardinalDir.Right: return Vector2Int.right;
            case CardinalDir.Left: return Vector2Int.left;
            case CardinalDir.Up: return Vector2Int.up;
            default: return Vector2Int.down;
        }
    }

    static CardinalDir TurnCW(CardinalDir d)
    {
        switch (d)
        {
            case CardinalDir.Right: return CardinalDir.Down;
            case CardinalDir.Down: return CardinalDir.Left;
            case CardinalDir.Left: return CardinalDir.Up;
            default: return CardinalDir.Right;
        }
    }

    static CardinalDir TurnCCW(CardinalDir d)
    {
        switch (d)
        {
            case CardinalDir.Right: return CardinalDir.Up;
            case CardinalDir.Up: return CardinalDir.Left;
            case CardinalDir.Left: return CardinalDir.Down;
            default: return CardinalDir.Right;
        }
    }
}
