using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class Sequence : Node
    {
        [Tooltip("Evaluate all children in order each tick, otherwise only tick currently running child")]
        [SerializeField] bool _reevaluateChildrenOnTick;

        int _currentChildIndex;

        public override Node GetActiveBehavior()
        {
            return _children[_currentChildIndex].GetActiveBehavior();
        }

        protected override string GetNodeType() => nameof(Sequence);

        protected override void OnInit()
        {
            EnforceChildMinimum(2);
        }

        protected override void OnBegin()
        {
            _currentChildIndex = 0;
        }

        protected override Status OnTick()
        {
            if (_reevaluateChildrenOnTick)
            {
                _currentChildIndex = 0;
            }

            for (int i = _currentChildIndex; i < _children.Length; i++)
            {
                _currentChildIndex = i;

                var node = _children[i];
                var status = node.Tick();
                if (status == Status.Success)
                {
                    continue;
                }

                return status;
            }

            return Status.Success;
        }
    }
}
