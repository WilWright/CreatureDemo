using UnityEngine;

namespace StateMachine
{
    public abstract class State : MonoBehaviour
    {
        public virtual void Init() { }
        public virtual void Begin(int previousState) { }
        public virtual void End(int nextState) { }
        public virtual void Process() { }
    }
}
