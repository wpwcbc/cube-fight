// AttackShapeBase.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class AttackShapeBase : ScriptableObject
{
    // Return offsets around the center grid cell (player)
    public abstract IEnumerable<Vector2Int> GetOffsets(int size);

    // most shapes (like rings/crosses) look best with odd sizes
    public virtual bool ForceOddSize => true;
}
