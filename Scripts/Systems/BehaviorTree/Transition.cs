using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Utils;

namespace BehaviorTree
{
    [System.Serializable]
    public class Transition
    {
        public enum TransitionType
        {
            From,
            To
        }

        [SerializeField] TransitionType _transitionType;
        [SerializeField] Action[] _actions;
        [SerializeField] UnityEvent _event;

        public void DoTransition(TransitionType transitionType, Action action)
        {
            if (transitionType != _transitionType || action == null)
            {
                return;
            }

            if (_actions.IsNullOrEmpty() || _actions.Contains(action) == false)
            {
                return;
            }

            _event.Invoke();
        }
    }
}
