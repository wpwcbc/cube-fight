using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GridAgent : MonoBehaviour
{
    [Header("Start")]
    [SerializeField] private bool snapToGridOnStart = true;

    [Header("Motion")]
    [Min(0f)][SerializeField] private float moveDuration = 0.12f;
    [SerializeField] private bool isAnimate = true;

    [Header("Visual Style")]
    [Tooltip("If true, animate each step as a 90° roll around the bottom edge.")]
    [SerializeField] private bool useRollAnimation = false;

    [Tooltip("Turn to face the move direction for non-rolling steps.")]
    [SerializeField] private bool faceMoveDirection = true;

    [Tooltip("If true, slerp the Y-rotation during the slide; otherwise face instantly.")]
    [SerializeField] private bool smoothTurn = true;

    [Tooltip("Fallback half-height (Y) if no Renderer is found. Usually cellSize * 0.5.")]
    [SerializeField] private float fallbackHalfHeight = 0.5f;

    [Header("VFX")]
    [Tooltip("Optional dust FX spawned for non-rolling steps.")]
    [SerializeField] private GameObject nonRollingStepDustPrefab;

    [Header("Events")]
    public UnityEvent<Vector2Int> onMoved;

    public Vector2Int CurrentGridPos { get; private set; }
    public bool IsMoving { get; private set; }

    float _y;                 // base Y to keep object sitting on the floor
    float _halfHeight = 0.5f; // used to compute roll pivot

    void Awake()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        _halfHeight = r != null ? r.bounds.extents.y : Mathf.Max(0.001f, fallbackHalfHeight);

        Vector3 pos = transform.position;
        _y = pos.y;

        CurrentGridPos = GridMath.WorldToGrid(pos);

        if (snapToGridOnStart)
            transform.position = GridMath.GridToWorldCenter(CurrentGridPos, _y);
    }

    Vector2Int ClampToBounds(Vector2Int g)
    {
        return new Vector2Int(
            Mathf.Clamp(g.x, 0, GridSettings.columns - 1),
            Mathf.Clamp(g.y, 0, GridSettings.rows - 1)
        );
    }

    public bool TryMove(Vector2Int delta)
    {
        if (IsMoving) return false;
        if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1) return false; // cardinal only

        Vector2Int dest = CurrentGridPos + delta;
        if (!GridMath.IsWithinBounds(dest)) return false;
        if (!LevelGrid.Instance.IsWalkable(dest)) return false;

        StopAllCoroutines();
        StartCoroutine(MoveRoutine(dest));
        return true;
    }

    IEnumerator MoveRoutine(Vector2Int dest)
    {
        IsMoving = true;
        Vector3 dustSpawnPos = transform.position;

        if (!isAnimate || moveDuration <= 0f)
        {
            // Instant snap (and optionally face instantly)
            if (faceMoveDirection && !useRollAnimation)
            {
                Vector2Int d = dest - CurrentGridPos;
                Vector3 dir = new Vector3(d.x, 0f, d.y);
                if (dir.sqrMagnitude > 0.00001f)
                {
                    Quaternion face = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = face;
                }
            }

            transform.position = GridMath.GridToWorldCenter(dest, _y);
        }
        else if (useRollAnimation)
        {
            // Roll handles both position and orientation
            yield return RollStep(dest);
        }
        else
        {
            // Slide/lerp position and (optionally) rotate towards the direction
            Vector3 start = transform.position;
            Vector3 end = GridMath.GridToWorldCenter(dest, _y);

            Quaternion startRot = transform.rotation;
            Quaternion targetRot = startRot;

            if (faceMoveDirection)
            {
                Vector2Int d = dest - CurrentGridPos;
                Vector3 dir = new Vector3(d.x, 0f, d.y);
                if (dir.sqrMagnitude > 0.00001f)
                    targetRot = Quaternion.LookRotation(dir, Vector3.up);
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / moveDuration;
                float a = Mathf.Clamp01(t);

                transform.position = Vector3.Lerp(start, end, a);

                if (faceMoveDirection)
                {
                    if (smoothTurn)
                        transform.rotation = Quaternion.Slerp(startRot, targetRot, a); // rotate over the move
                    else
                        transform.rotation = targetRot; // face immediately while sliding
                }

                yield return null;
            }

            transform.position = end;
            if (faceMoveDirection && !smoothTurn)
            {
                transform.rotation = targetRot; // ensure exact final orientation
            }
        }

        if (!useRollAnimation && nonRollingStepDustPrefab != null)
        {
            Instantiate(nonRollingStepDustPrefab, dustSpawnPos, transform.rotation);
        }

        CurrentGridPos = dest;
        onMoved?.Invoke(CurrentGridPos);
        IsMoving = false;
    }

    // --- Roll animation (90° around the bottom edge) ---
    IEnumerator RollStep(Vector2Int dest)
    {
        Vector2Int delta = dest - CurrentGridPos;
        Vector3 worldDir = new Vector3(delta.x, 0f, delta.y);
        worldDir.Normalize();

        float stepDistance = GridSettings.cellSize;
        Vector3 start = transform.position;

        Vector3 pivot = start + worldDir * (stepDistance * 0.5f) + Vector3.down * _halfHeight;
        Vector3 axis = Vector3.Cross(Vector3.up, worldDir);

        float remaining = 90f;
        while (remaining > 0f)
        {
            float step = 90f * (Time.deltaTime / Mathf.Max(0.0001f, moveDuration));
            if (step > remaining) step = remaining;

            transform.RotateAround(pivot, axis, step);
            remaining -= step;
            yield return null;
        }

        transform.position = GridMath.GridToWorldCenter(dest, _y);
        // orientation after a roll is already correct
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float sizeX = GridSettings.columns * GridSettings.cellSize;
        float sizeZ = GridSettings.rows * GridSettings.cellSize;
        float y = Application.isPlaying ? _y : transform.position.y;
        Gizmos.DrawWireCube(new Vector3(0f, y, 0f), new Vector3(sizeX, 0.01f, sizeZ));
    }
#endif
}
