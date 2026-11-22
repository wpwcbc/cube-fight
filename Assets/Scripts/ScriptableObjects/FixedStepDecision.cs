using UnityEngine;

[DisallowMultipleComponent]
public class FixedStepDecision : StepDecisionBase
{
    public Vector2Int stepDirection = Vector2Int.right;

    void OnValidate() => stepDirection = QuantizeCardinal(stepDirection);

    public override Vector2Int DecideStep(GridAgent self)
        => QuantizeCardinal(stepDirection);
}
