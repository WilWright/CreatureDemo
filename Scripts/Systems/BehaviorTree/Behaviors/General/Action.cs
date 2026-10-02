using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace BehaviorTree
{
    public abstract class Action : Node
    {
        [SerializeField] Transition[] _transitions;
        
        public override void Init(BehaviorTree behaviorTree, Node parent = null, List<Node> selfNodeResults = null)
        {
            InitNode(behaviorTree, parent, selfNodeResults);
            EnforceNoChildren();
            OnInit();
        }

        protected override string GetNodeType() => nameof(Action);

        public void TransitionTo  (Action action) => DoTransition(Transition.TransitionType.To  , action);
        public void TransitionFrom(Action action) => DoTransition(Transition.TransitionType.From, action);

        void DoTransition(Transition.TransitionType transitionType, Action action)
        {
            if (transitionType == Transition.TransitionType.To && CurrentStatus == Status.Running)
            {
                Cancel();
            }

            if (action == null || CollectionUtils.IsNullOrEmpty(_transitions))
            {
                return;
            }

            foreach (var transition in _transitions)
            {
                transition.DoTransition(transitionType, action);
            }
        }
    }
}
