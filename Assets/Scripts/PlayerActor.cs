using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GridAgent))]
[RequireComponent(typeof(Health))]
public class PlayerActor : MonoBehaviour
{
    [Header("Animation/VFX")]
    public Animator animator;
    public string deathTrigger = "Die";
    public GameObject deathEffect;
    public float deathEffectLifetime = 0.6f;

    [Header("Signals")]
    public UnityEvent onPlayerDied;  // LevelFlow / UI can subscribe

    bool _dead;
    public bool destroyOnDeath = false;

    void Awake()
    {
        // Ensure Health is tagged as Player
        var hp = GetComponent<Health>();
        hp.faction = Faction.Player;
    }

    public void Kill()
    {
        if (_dead) return;
        _dead = true;


        // 2) Stop any movement coroutines on the agent (safe even if none)
        var agent = GetComponent<GridAgent>();
        if (agent) agent.StopAllCoroutines();  // stops all coroutines on this MB. :contentReference[oaicite:4]{index=4}

        // 3) VFX/SFX
        if (deathEffect)
        {
            GameObject fx = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(fx, deathEffectLifetime);
        }

        // 4) (Optional) Pause enemy ticking while we show death
        if (EnemyManager.Instance) EnemyManager.Instance.enabled = false;

        // 5) Notify listeners (UI / LevelFlow)
        onPlayerDied?.Invoke();

        // 4) Remove from scene
        if (destroyOnDeath) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
