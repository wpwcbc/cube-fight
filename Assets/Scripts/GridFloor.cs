using UnityEngine;

[ExecuteAlways]
public class GridFloor : MonoBehaviour
{
    [Header("Cell Colors")]
    public Color colorA;
    public Color colorB;

    [Header("Floor Reference")]
    public Renderer floorRenderer;  // Assign your Plane’s MeshRenderer
    public Transform floorTransform; // Assign your Plane’s Transform

    Material _material;
    Texture2D _checker;

    void OnValidate() { Apply(); }
    void Awake()
    {
        colorA = new Color(0.92f, 0.92f, 0.92f);
        colorB = new Color(0.75f, 0.75f, 0.75f);
        Apply();
    }

    void Apply()
    {
        if (!floorTransform || !floorRenderer) return;

        // 1) Scale the Plane by grid parameters
        float sizeX = GridSettings.columns * GridSettings.cellSize;
        float sizeZ = GridSettings.rows * GridSettings.cellSize;
        floorTransform.localScale = new Vector3(sizeX / 10f, 1f, sizeZ / 10f); // Unity's built-in Plane is 10x10 units at scale 1.

        // 2) Apply and find mat
        if (_material == null)
        {
            // Try URP Unlit first, then fallback to Unlit/Texture, then Standard.
            Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) sh = Shader.Find("Unlit/Texture");
            if (sh == null) sh = Shader.Find("Standard");

            _material = new Material(sh) { name = "GridFloor_RuntimeMaterial" };
            floorRenderer.sharedMaterial = _material;
        }

        // 3) Build a tiny 2x2 checker and tile it
        if (_checker == null)
        {
            _checker = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            _checker.filterMode = FilterMode.Point;                 // keep edges crisp
            _checker.wrapMode = TextureWrapMode.Repeat;            // allow tiling
        }

        _checker.SetPixel(0, 0, colorA);
        _checker.SetPixel(1, 0, colorB);
        _checker.SetPixel(0, 1, colorB);
        _checker.SetPixel(1, 1, colorA);
        _checker.Apply();

        // Assign texture to known properties across pipelines
        if (_material.HasProperty("_BaseMap"))         // URP Unlit/Lit
        {
            _material.SetTexture("_BaseMap", _checker);
            _material.SetTextureScale("_BaseMap", new Vector2(GridSettings.columns * 0.5f, GridSettings.rows * 0.5f));
        }
        else                                           // Built-in
        {
            _material.mainTexture = _checker;
            _material.mainTextureScale = new Vector2(GridSettings.columns * 0.5f, GridSettings.rows * 0.5f);
        }
    }
}
