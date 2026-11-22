using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    [Min(1)] public int damage = 1;
    public Faction faction = Faction.Player; // set to match the owner
    public float rehitCooldown = 0.15f;      // minimal time between hits on same target

    private Health _ownerHealth;
    private readonly Dictionary<Health, float> _lastHitTime = new Dictionary<Health, float>();

    void Awake()
    {
        _ownerHealth = GetComponentInParent<Health>();
        if (_ownerHealth != null) faction = _ownerHealth.faction;

        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true; // ensure this is a trigger
    }

    void OnEnable()
    {
        _lastHitTime.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    void OnTriggerStay(Collider other)
    {
        // Optional: allow repeated damage while staying in contact, throttled by rehitCooldown
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        Hurtbox hb = other.GetComponent<Hurtbox>();
        if (hb == null) return;

        Health target = hb.owner;
        if (target == null) return;

        // ignore self & same team
        if (target == _ownerHealth) return;
        if (target.faction == faction) return;

        float now = Time.time;
        float last;
        if (_lastHitTime.TryGetValue(target, out last))
        {
            if (now - last < rehitCooldown) return;
        }

        _lastHitTime[target] = now;
        target.TakeDamage(damage, transform);
    }
}
