using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hurtbox : MonoBehaviour
{
    public Health owner; // auto-filled if null

    void Awake()
    {
        if (owner == null) owner = GetComponentInParent<Health>();
    }
}
