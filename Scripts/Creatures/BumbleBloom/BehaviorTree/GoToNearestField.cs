using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Navigation;

namespace BehaviorTree.BumbleBloom
{
    public class GoToNearestField : Action
    {
        [SerializeField] NavigationUnit _navigationUnit;
        [SerializeField] MinMaxValue.Float _distanceThreshold;

        BumbleBloomFieldNavigationData _currentField;

        bool _wasDestinationReached;
        bool _wasCancelled;

        protected override void OnBegin()
        {
            _wasDestinationReached = _wasCancelled = false;

            _currentField = GetNearestField(out _wasDestinationReached);
            if (_currentField == null)
            {
                return;
            }

            _navigationUnit.OnDestinationReached.AddListener(SetNavigationDestinationReached);
            _navigationUnit.OnCancelled         .AddListener(CancelNavigation);
            _navigationUnit.GoTo(_currentField.GetRandomRestPosition());
        }

        protected override Status OnTick()
        {
            if (_wasDestinationReached)
            {
                return Status.Success;
            }

            if (_currentField == null || _wasCancelled)
            {
                return Status.Failure;
            }

            return Status.Running;
        }

        protected override void OnCancel()
        {
            _navigationUnit.Stop();
        }

        protected override void OnReset()
        {
            _navigationUnit.Stop();

            _currentField = null;
        }

        void SetNavigationDestinationReached()
        {
            _navigationUnit.OnDestinationReached.RemoveListener(SetNavigationDestinationReached);
            _wasDestinationReached = true;
        }

        void CancelNavigation()
        {
            _navigationUnit.OnCancelled.RemoveListener(CancelNavigation);
            _wasCancelled = true;
        }

        BumbleBloomFieldNavigationData GetNearestField(out bool alreadyNear)
        {
            alreadyNear = false;

            if (GameController.ChunkManager.TryGetNavigationDatas(transform.position, out var navigationDatas) == false)
            {
                return null;
            }

            BumbleBloomFieldNavigationData nearestField = null;
            float nearestDistance = float.MaxValue;
            foreach (var data in navigationDatas)
            {
                if (data is BumbleBloomFieldNavigationData bbf == false)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, bbf.transform.position);

                // Don't consider fields that are too far away
                if (distance > _distanceThreshold.max)
                {
                    continue;
                }

                if (distance < nearestDistance)
                {
                    nearestField = bbf;
                    nearestDistance = distance;
                }
            }

            // If we're already close enough to the nearest one then ignore it
            if (nearestDistance < _distanceThreshold.min)
            {
                alreadyNear = true;
                return null;
            }

            return nearestField;
        }
    }
}
