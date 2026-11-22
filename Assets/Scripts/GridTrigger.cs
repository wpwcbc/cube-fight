using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GridTrigger : MonoBehaviour
{
    [Header("Authoring")]
    public bool snapToCellCenter = true;

    [Tooltip("Size in cells. (1,1) = single cell. Interpreted as width (x) and height (y).")]
    public Vector2Int sizeInCells = new Vector2Int(1, 1);

    [Tooltip("If true, disable this after registration (logical-only trigger). Leave false if you want a visible marker.")]
    public bool disableRendererAfterRegister = true;

    private List<Vector2Int> _cells = new List<Vector2Int>();

    void OnEnable()
    {
        _cells.Clear();

        Vector2Int origin = GridMath.WorldToGrid(transform.position);
        if (snapToCellCenter)
        {
            Vector3 snapped = GridMath.GridToWorldCenter(origin, transform.position.y);
            transform.position = snapped;
        }

        int w = Mathf.Max(1, sizeInCells.x);
        int h = Mathf.Max(1, sizeInCells.y);

        for (int dy = 0; dy < h; dy++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                Vector2Int c = new Vector2Int(origin.x + dx, origin.y + dy);
                if (GridMath.IsWithinBounds(c)) _cells.Add(c);
            }
        }

        if (TriggerGrid.Instance != null) TriggerGrid.Instance.AddExitCells(_cells);

        if (disableRendererAfterRegister)
        {
            Renderer[] rends = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rends.Length; i++) rends[i].enabled = false;
        }
    }

    void OnDisable()
    {
        if (TriggerGrid.Instance == null) return;
        for (int i = 0; i < _cells.Count; i++)
            TriggerGrid.Instance.RemoveExitCell(_cells[i]);
        _cells.Clear();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);
        Vector2Int origin = Application.isPlaying ? (_cells.Count > 0 ? _cells[0] : GridMath.WorldToGrid(transform.position))
                                                  : GridMath.WorldToGrid(transform.position);
        int w = Mathf.Max(1, sizeInCells.x);
        int h = Mathf.Max(1, sizeInCells.y);

        for (int dy = 0; dy < h; dy++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                Vector2Int c = new Vector2Int(origin.x + dx, origin.y + dy);
                if (!GridMath.IsWithinBounds(c)) continue;
                Vector3 p = GridMath.GridToWorldCenter(c, 0f);
                Gizmos.DrawCube(p, new Vector3(GridSettings.cellSize * 0.95f, 0.02f, GridSettings.cellSize * 0.95f));
            }
        }
    }
#endif
}
