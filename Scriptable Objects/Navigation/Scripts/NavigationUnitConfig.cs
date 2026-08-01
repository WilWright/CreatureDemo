using UnityEngine;

namespace Navigation
{
    [CreateAssetMenu(fileName = "Unit Config", menuName = "Scriptable Objects/Navigation/Unit Config")]
    public class NavigationUnitConfig : ScriptableObject
    {
        [field: SerializeField] public UnitType UnitType     { get; private set; }
        [field: SerializeField] public Vector3  UnitSize     { get; private set; }
        [field: SerializeField] public float UnitStepHeight  { get; private set; }
        [field: SerializeField] public float MaxSlopeDegrees { get; private set; }

        [field: SerializeField] public MinMaxValue.Float MoveSpeed          { get; private set; }
        [Tooltip("Range in which distance to move affects move speed")]
        [field: SerializeField] public MinMaxValue.Float MoveDistance       { get; private set; }
        [field: SerializeField] public MinMaxValue.Float WanderDistance     { get; private set; }
        [field: SerializeField] public MinMaxValue.Float WanderWaitDuration { get; private set; }
    }
}
