using UnityEngine;

public static class GridSettings
{
    [Header("Grid Size")]
    [Min(1)] public static int columns = 20;
    [Min(1)] public static int rows = 20;
    [Min(0.1f)] public static float cellSize = 1f;
}
