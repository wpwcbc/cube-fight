using UnityEngine;

[RequireComponent(typeof(GridAgent))]
[RequireComponent(typeof(EnemyStepper))]
public class EnemyActor : MonoBehaviour
{
    public GameObject deathEffect;
    public float deathEffectLifetime = 5f;
    public bool destroyOnDeath = false;

    bool _dead;

    public void Kill()
    {
        if (_dead) return;
        _dead = true;

        // 1) Unregister so no more scheduling this tick
        var stepper = GetComponent<EnemyStepper>();
        EnemyManager.Instance?.Unregister(stepper);

        // 2) Stop any movement coroutines on the agent (safe even if none)
        var agent = GetComponent<GridAgent>();
        if (agent) agent.StopAllCoroutines();  // stops all coroutines on this MB. :contentReference[oaicite:4]{index=4}

        // 3) VFX/SFX
        if (deathEffect)
        {
            GameObject fx = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(fx, deathEffectLifetime);
        }

        // 4) Remove from scene
        if (destroyOnDeath) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
