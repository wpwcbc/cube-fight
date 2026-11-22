// SquareRingShape.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Attacks/Shapes/Square Ring")]
public class SquareRingShape : AttackShapeBase
{
    public override IEnumerable<Vector2Int> GetOffsets(int size)
    {
        if (size < 1) yield break;
        if (ForceOddSize && size % 2 == 0) size += 1;

        int r = size / 2;
        // top & bottom edges
        for (int x = -r; x <= r; x++)
        {
            yield return new Vector2Int(x, -r);
            yield return new Vector2Int(x, r);
        }
        // left & right edges (skip corners to avoid double entries)
        for (int y = -r + 1; y <= r - 1; y++)
        {
            yield return new Vector2Int(-r, y);
            yield return new Vector2Int(r, y);
        }
    }
}
