using UnityEngine;

[DisallowMultipleComponent]
public class GridBlocker : MonoBehaviour
{
    public bool snapToCellCenter = true;
    public bool initAsBlock = true;

    Vector2Int _cell;
    bool _registered;

    void OnEnable()
    {
        _cell = GridMath.WorldToGrid(transform.position);

        if (snapToCellCenter)
        {
            Vector3 snapped = GridMath.GridToWorldCenter(_cell, transform.position.y);
            transform.position = snapped;
        }

        if (initAsBlock) SetAsBlock();
    }

    void OnDisable()
    {
        SetAsNonBlock();
    }

    public void SetAsBlock()
    {
        if (LevelGrid.Instance != null)
        {
            LevelGrid.Instance.AddBlocked(_cell);
            _registered = true;
        }
    }

    public void SetAsNonBlock()
    {
        if (_registered && LevelGrid.Instance != null)
        {
            LevelGrid.Instance.RemoveBlocked(_cell);
            _registered = false;
        }
    }
}
