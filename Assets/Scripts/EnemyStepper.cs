using UnityEngine;

[RequireComponent(typeof(GridAgent))]
public class EnemyStepper : MonoBehaviour
{
    [Header("Decision Module")]
    public StepDecisionBase decision; // FixedStepDecision / ChaseStepDecision

    GridAgent _agent;
    public GridAgent Agent => _agent;
    public StepDecisionBase Decision => decision;

    void Awake()
    {
        _agent = GetComponent<GridAgent>();
    }

    void OnEnable()
    {
        EnemyManager.Instance?.Register(this);
    }

    void OnDisable()
    {
        EnemyManager.Instance?.Unregister(this);
    }
}
