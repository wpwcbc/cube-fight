using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [Header("Health")]
    [Min(1)] public int maxHP = 1;
    public Faction faction = Faction.Enemy;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDied;

    public int CurrentHP { get; private set; }

    void Awake()
    {
        CurrentHP = maxHP;
    }

    public void TakeDamage(int amount, Transform source)
    {
        if (CurrentHP <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - Mathf.Max(0, amount));
        if (onDamaged != null) onDamaged.Invoke();

        if (CurrentHP == 0)
        {
            onDied?.Invoke();

            var enemy = GetComponent<EnemyActor>();
            if (enemy) { enemy.Kill(); return; }

            var player = GetComponent<PlayerActor>();
            if (player) { player.Kill(); return; }

            // fallback
            gameObject.SetActive(false);
        }
    }

    public void ResetHP()
    {
        CurrentHP = maxHP;
    }
}
