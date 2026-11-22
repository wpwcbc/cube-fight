// LaserRingEffect.cs
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserRingEffect : MonoBehaviour
{
    private float lineWidth = 0.5f;
    private float yOffset = 0.2f;

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.widthMultiplier = lineWidth;
    }

    // Call this right after Instantiate
    public void Setup(Vector3 centerWorld, int sizeCells, float cellSize)
    {
        if (sizeCells % 2 == 0) sizeCells += 1;
        float half = (sizeCells * cellSize) * 0.5f;
        float minX = centerWorld.x - half + cellSize / 2;
        float maxX = centerWorld.x + half - cellSize / 2;
        float minZ = centerWorld.z - half + cellSize / 2;
        float maxZ = centerWorld.z + half - cellSize / 2;
        float y = yOffset;

        var pts = new Vector3[]
        {
            new Vector3(minX, y, minZ),
            new Vector3(minX, y, maxZ),
            new Vector3(maxX, y, maxZ),
            new Vector3(maxX, y, minZ)
        };
        lr.positionCount = pts.Length;
        lr.SetPositions(pts);
    }
}
