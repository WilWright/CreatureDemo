using UnityEngine;
using Utils;

namespace BehaviorTree
{
    public class Selector : Node
    {
        [Tooltip("Evaluate all children in order each tick, otherwise only tick currently running child")]
        [SerializeField] bool _reevaluateChildrenOnTick;

        Node _currentChild;

        public override Node GetActiveBehavior()
        {
            return _currentChild == null ? null : _currentChild.GetActiveBehavior();
        }

        protected override string GetNodeType() => nameof(Selector);

        protected override void OnInit()
        {
            EnforceChildMinimum(2);
        }

        protected override void OnBegin()
        {
            _currentChild = null;
        }

        protected override Status OnTick()
        {
            if (_reevaluateChildrenOnTick == false && _currentChild != null)
            {
                return _currentChild.Tick();
            }

            return Select();
        }

        Status Select()
        {
            foreach (var node in _children)
            {
                _currentChild = node;

                var status = node.Tick();
                if (status == Status.Failure)
                {
                    continue;
                }

                return status;
            }

            return Status.Failure;
        }
    }
}
