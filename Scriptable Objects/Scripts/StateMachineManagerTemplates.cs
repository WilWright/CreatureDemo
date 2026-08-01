using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(fileName = "State Machine Manager Templates", menuName =  "Scriptable Objects/State Machine/State Machine Manager Templates")]
    public class StateMachineManagerTemplates : ScriptableObject
    {
        [field: SerializeField] public TextAsset StateMachine { get; private set; }
        [field: SerializeField] public TextAsset Controller   { get; private set; }
        [field: SerializeField] public TextAsset ParentState  { get; private set; }
        [field: SerializeField] public TextAsset State        { get; private set; }
    }
}