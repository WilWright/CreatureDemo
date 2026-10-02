using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class Inverter : Node
    {
        Node _child;

        public override Node GetActiveBehavior()
        {
            return _child.GetActiveBehavior();
        }

        protected override string GetNodeType() => nameof(Inverter);

        protected override void OnInit()
        {
            EnforceChildCount(1);

            _child = _children[0];
        }

        protected override Status OnTick()
        {
            var status = _child.Tick();
            return status switch
            {
                Status.Failure => Status.Success,
                Status.Success => Status.Failure,
                _ => status
            };
        }
    }
}
