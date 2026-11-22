using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-900)]
public class LevelGrid : MonoBehaviour
{
    public static LevelGrid Instance { get; private set; }

    // Cells permanently blocked by level geometry (walls, obstacles).
    private readonly HashSet<Vector2Int> _blockedStatic = new HashSet<Vector2Int>();

    void Awake()
    {
        Instance = this;
    }

    public void AddBlocked(Vector2Int cell)
    {
        _blockedStatic.Add(cell);
    }

    public void RemoveBlocked(Vector2Int cell)
    {
        _blockedStatic.Remove(cell);
    }

    public bool IsWalkable(Vector2Int cell)
    {
        if (!GridMath.IsWithinBounds(cell)) return false;
        return !_blockedStatic.Contains(cell);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_blockedStatic == null) return;
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.35f);
        foreach (Vector2Int c in _blockedStatic)
        {
            Vector3 p = GridMath.GridToWorldCenter(c, 0f);
            Gizmos.DrawCube(p, new Vector3(GridSettings.cellSize * 0.98f, 0.02f, GridSettings.cellSize * 0.98f));
        }
    }
#endif
}
