using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Navigation;

namespace BehaviorTree.BumbleBloom
{
    public class GoToNearestBush : Action
    {
        [SerializeField] NavigationUnit _navigationUnit;
        [SerializeField] MinMaxValue.Float _distanceThreshold;

        BushNavigationData _currentBush;

        bool _wasDestinationReached;
        bool _wasCancelled;

        protected override void OnBegin()
        {
            _wasDestinationReached = _wasCancelled = false;

            ReleaseCurrentBush();

            _currentBush = GetNearestBush(out _wasDestinationReached);
            if (_currentBush == null)
            {
                return;
            }

            _navigationUnit.OnDestinationReached.AddListener(SetNavigationDestinationReached);
            _navigationUnit.OnCancelled         .AddListener(CancelNavigation);
            _navigationUnit.GoTo(_currentBush.WorldPosition);
        }

        protected override void OnEnd()
        {
            ReleaseCurrentBush();

            _navigationUnit.OnDestinationReached.RemoveListener(SetNavigationDestinationReached);
            _navigationUnit.OnCancelled         .RemoveListener(CancelNavigation);
        }

        protected override Status OnTick()
        {
            if (_wasDestinationReached)
            {
                return Status.Success;
            }

            if (_currentBush == null || _wasCancelled)
            {
                return Status.Failure;
            }

            return Status.Running;
        }

        protected override void OnCancel()
        {
            _navigationUnit.Stop();

            _navigationUnit.OnDestinationReached.RemoveListener(SetNavigationDestinationReached);
            _navigationUnit.OnCancelled         .RemoveListener(CancelNavigation);

            ReleaseCurrentBush();
        }

        protected override void OnReset()
        {
            _navigationUnit.Stop();

            _navigationUnit.OnDestinationReached.RemoveListener(SetNavigationDestinationReached);
            _navigationUnit.OnCancelled         .RemoveListener(CancelNavigation);

            ReleaseCurrentBush();
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

        BushNavigationData GetNearestBush(out bool alreadyNear)
        {
            alreadyNear = false;

            if (GameController.ChunkManager.TryGetNavigationDatas(transform.position, out var navigationDatas) == false)
            {
                return null;
            }

            BushNavigationData nearestBush = null;
            float nearestDistance = float.MaxValue;
            foreach (var data in navigationDatas)
            {
                if (data is BushNavigationData b == false)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, b.WorldPosition);

                // Don't consider bushes that are too far away
                if (distance > _distanceThreshold.max)
                {
                    continue;
                }

                if (distance < nearestDistance && b.ClaimCapacity(_navigationUnit.UnitConfig))
                {
                    if (nearestBush != null)
                    {
                        nearestBush.ReleaseCapacity(_navigationUnit.UnitConfig);
                    }

                    nearestBush = b;
                    nearestDistance = distance;
                }
            }

            // If we're already close enough to the nearest one then ignore it
            if (nearestDistance < _distanceThreshold.min)
            {
                nearestBush.ReleaseCapacity(_navigationUnit.UnitConfig);
                alreadyNear = true;
                return null;
            }

            return nearestBush;
        }

        void ReleaseCurrentBush()
        {
            if (_currentBush == null)
            {
                return;
            }

            _currentBush.ReleaseCapacity(_navigationUnit.UnitConfig);
            _currentBush = null;
        }
    }
}
