using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class Cooldown : Node
    {
        [SerializeField] float _cooldown;
        [SerializeField] bool _onlyCooldownOnSuccess;
        [Tooltip("If false, return failure during cooldown, otherwise return last ticked status")]
        [SerializeField] bool _cacheLastStatus;

        float _time;

        Status _lastStatus;

        Node _child;


        void Update()
        {
            if (_time > 0)
            {
                _time -= Time.deltaTime;
            }
        }

        public override Node GetActiveBehavior()
        {
            return _child.GetActiveBehavior();
        }

        protected override string GetNodeType() => nameof(Cooldown);

        protected override void OnInit()
        {
            EnforceChildCount(1);

            _child = _children[0];
        }

        protected override Status OnTick()
        {
            if (_time > 0)
            {
                return _cacheLastStatus ? _lastStatus : Status.Failure;
            }

            var status = _child.Tick();
            _lastStatus = status;
            if (status == Status.Running)
            {
                _time = 0;
                return Status.Running;
            }

            if (_onlyCooldownOnSuccess && status == Status.Failure)
            {
                _time = 0;
                return Status.Failure;
            }

            _time += _cooldown;
            return status;
        }

        protected override void OnReset()
        {
            _time = 0;
            _lastStatus = Status.None;
        }
    }
}
