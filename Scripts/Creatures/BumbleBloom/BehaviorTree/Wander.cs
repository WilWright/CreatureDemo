using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Navigation;

namespace BehaviorTree.BumbleBloom
{
    public class Wander : Action
    {
        [SerializeField] NavigationUnit _navigationUnit;

        bool _wasCancelled;

        protected override void OnBegin()
        {
            _wasCancelled = false;

            _navigationUnit.OnCancelled.AddListener(CancelNavigation);

            _navigationUnit.Wander();
        }

        protected override void OnEnd()
        {
            _navigationUnit.OnCancelled.RemoveListener(CancelNavigation);
        }

        protected override Status OnTick()
        {
            if (_wasCancelled)
            {
                return Status.Failure;
            }

            return Status.Running;
        }

        protected override void OnCancel()
        {
            _navigationUnit.Stop();
            _navigationUnit.OnCancelled.RemoveListener(CancelNavigation);
        }

        protected override void OnReset()
        {
            _navigationUnit.Stop();
            _navigationUnit.OnCancelled.RemoveListener(CancelNavigation);
        }

        void CancelNavigation()
        {
            _navigationUnit.OnCancelled.RemoveListener(CancelNavigation);
            _wasCancelled = true;
        }
    }
}
