using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-850)]
public class TriggerGrid : MonoBehaviour
{
    public static TriggerGrid Instance { get; private set; }

    private readonly HashSet<Vector2Int> _exitCells = new HashSet<Vector2Int>();

    void Awake()
    {
        Instance = this;
    }

    public void AddExitCell(Vector2Int cell)
    {
        _exitCells.Add(cell);
    }

    public void AddExitCells(IEnumerable<Vector2Int> cells)
    {
        foreach (Vector2Int c in cells) _exitCells.Add(c);
    }

    public void RemoveExitCell(Vector2Int cell)
    {
        _exitCells.Remove(cell);
    }

    public bool IsExit(Vector2Int cell)
    {
        return _exitCells.Contains(cell);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.35f);
        foreach (Vector2Int c in _exitCells)
        {
            Vector3 p = GridMath.GridToWorldCenter(c, 0f);
            Gizmos.DrawCube(p, new Vector3(GridSettings.cellSize * 0.95f, 0.02f, GridSettings.cellSize * 0.95f));
        }
    }
#endif
}
