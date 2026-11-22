using UnityEngine;

public static class GridMath
{
    // World origin (0,0,0) is at the center of the Plane.
    // Grid (0,0) is bottom-left
    // grid.y +: forward (+Z), grid.x +: right (+X).

    public static Vector3 GridToWorldCenter(Vector2Int gridPos, float y = 0f)
    {
        float sizeX = GridSettings.columns * GridSettings.cellSize;
        float sizeZ = GridSettings.rows * GridSettings.cellSize;

        // Shift to center
        float wx = -sizeX * 0.5f + (gridPos.x + 0.5f) * GridSettings.cellSize;
        float wz = -sizeZ * 0.5f + (gridPos.y + 0.5f) * GridSettings.cellSize;

        return new Vector3(wx, y, wz);
    }

    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float sizeX = GridSettings.columns * GridSettings.cellSize;
        float sizeZ = GridSettings.rows * GridSettings.cellSize;

        // Shift to bottom-left
        float lx = worldPos.x + sizeX * 0.5f;
        float lz = worldPos.z + sizeZ * 0.5f;

        int gx = Mathf.FloorToInt(lx / GridSettings.cellSize);
        int gy = Mathf.FloorToInt(lz / GridSettings.cellSize);

        return new Vector2Int(
            Mathf.Clamp(gx, 0, GridSettings.columns - 1),
            Mathf.Clamp(gy, 0, GridSettings.rows - 1)
        );
    }

    public static bool IsWithinBounds(Vector2Int gridPos) =>
        gridPos.x >= 0 && gridPos.x < GridSettings.columns && gridPos.y >= 0 && gridPos.y < GridSettings.rows;
}
