// AttackMove.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Move")]
public class AttackMove : ScriptableObject
{
    public string displayName = "Laser Ring";
    [Min(0f)] public float cooldown = 1f;
    [Min(1)] public int size = 3;
    public AttackShapeBase shape;
    public GameObject effectPrefab;   // optional visual (e.g., LineRenderer ring)
    public AudioClip sfx;

    [System.NonSerialized] public float _nextReadyTime = 0;

    // public bool IsReady => Time.time >= _nextReadyTime;

    public bool IsReady
    {
        get
        {
            return Time.time >= _nextReadyTime;
        }
    }

    public void MarkUsed()
    {
        _nextReadyTime = Time.time + cooldown;
    }
}
