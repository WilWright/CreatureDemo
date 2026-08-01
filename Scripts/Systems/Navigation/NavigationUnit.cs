using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

using Utils;

namespace Navigation
{
    public class NavigationUnit : MonoBehaviour
    {
        [SerializeField] bool _debug;

        [field: SerializeField] public NavigationUnitConfig UnitConfig { get; private set; }

        public UnitType UnitType => UnitConfig.UnitType;

        public Coordinates3D CurrentCoordinates { get; private set; }

        public bool IsNavigating { get; private set; }

        public UnityEvent OnDestinationReached = new();

        NavigationPath _currentPath;
        float _currentDistanceNavigated;
        CancellationTokenSource _currentRequestCancellation;

        CoroutineObject _wanderCoroutine;

        Debugging.SceneDebug _pathDebug;

        void Start()
        {
            _wanderCoroutine = new(this);
        }

        void Update()
        {
            bool wasNavigating = IsNavigating;

            IsNavigating = _currentPath != null && _currentPath.IsDone == false;

            if (IsNavigating)
            {
                // For now, unit will follow the path directly at a speed proportional to distance needed to travel defined in config
                // TODO: Add local avoidance and attraction forces to desired position/rotation

                var currentPosition = _currentPath.CurrentStep.HasValue ? _currentPath.CurrentStep.Value.Position : transform.position;
                var nextPosition    = _currentPath.NextStep   .HasValue ? _currentPath.NextStep   .Value.Position : transform.position;
                float distanceNavigated = Vector3.Dot(transform.position - currentPosition, _currentPath.CurrentStep.Value.DirectionToNextStep);

                var targetVector = nextPosition - transform.position;
                float targetDistance = targetVector.magnitude;
                if (targetDistance <= 0.1f)
                {
                    _currentDistanceNavigated += distanceNavigated;
                    transform.position = nextPosition;
                    _currentPath.AdvanceStep();
                    return;
                }

                var moveDirection = targetVector.normalized;
                if (TransformUtils.GetLookRotationXZ(moveDirection, out var lookRotation))
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
                }

                float distanceLeft = _currentPath.TotalDistance - (_currentDistanceNavigated + distanceNavigated);
                float moveSpeedPercent = UnitConfig.MoveDistance.InverseLerp(distanceLeft);

                var moveVector = moveDirection * (Time.deltaTime * UnitConfig.MoveSpeed.Lerp(moveSpeedPercent));
                moveVector = Vector3.ClampMagnitude(moveVector, targetDistance);
                transform.position += moveVector;
            }
            else
            {
                if (wasNavigating && _currentPath != null && _currentPath.IsDone)
                {
                    ClearDebug();
                    OnDestinationReached.Invoke();
                }
            }
        }

        public void GoTo(Vector3 worldPosition)
        {
            Stop();

            _currentRequestCancellation = RequestNavigationPath(worldPosition,
                (path) =>
                {
                    _currentRequestCancellation = null;
                    SetPath(path);
                }
            );
        }

        public void Stop()
        {
            ClearDebug();

            if (_currentRequestCancellation != null)
            {
                _currentRequestCancellation.Cancel();
                _currentRequestCancellation = null;
            }

            _currentPath = null;
            _wanderCoroutine.Stop();
        }

        public void Wander()
        {
            var randomCirclePoint = UnityEngine.Random.insideUnitCircle.normalized;
            var randomDirection = new Vector3(randomCirclePoint.x, 0, randomCirclePoint.y);
            var randomPosition = transform.position + randomDirection * UnitConfig.WanderDistance.GetRandomValue();
            _wanderCoroutine.Restart(IeWander(randomPosition));
        }

        IEnumerator IeWander(Vector3 worldPosition)
        {
            var wait = new WaitForSeconds(UnitConfig.WanderWaitDuration.GetRandomValue());
            yield return wait;

            if (_wanderCoroutine.IsRunning == false)
            {
                yield break;
            }

            _currentRequestCancellation = RequestNavigationPath(worldPosition,
                (path) =>
                {
                    _currentRequestCancellation = null;
                    SetPath(path);
                }
            );

            while (IsNavigating)
            {
                yield return null;

                if (_wanderCoroutine.IsRunning == false)
                {
                    yield break;
                }
            }

            Wander();
        }

        CancellationTokenSource RequestNavigationPath(Vector3 to, Action<NavigationPath> onReady)
        {
            if (GameController.ChunkManager.TryGetChunk(transform.position, out var chunk) == false)
            {
                onReady(null);
                return null;
            }
            return chunk.RequestNavigationPath(this, to, onReady);
        }

        void SetPath(NavigationPath path)
        {
            _currentPath = path;
            _currentDistanceNavigated = 0;

            SetPathDebug(path);
        }

        void ClearDebug()
        {
            if (_pathDebug != null)
            {
                _pathDebug.Destroy();
            }
        }

        void SetPathDebug(NavigationPath path)
        {
            ClearDebug();

            if (_debug == false || path == null)
            {
                return;
            }

            _pathDebug = Debugging.SceneDebug.CreateDebug("Path");
            _pathDebug.CreatePrimitive("Target", PrimitiveType.Sphere, path.Steps[^1].Position, Quaternion.identity, Vector3.one * 0.2f, ColorUtils.PURPLE);
            for (int i = 1; i < path.Steps.Length; i++)
            {
                var point = path.Steps[i];
                _pathDebug.CreateLine(point.Position.ToString(), path.Steps[i - 1].Position, point.Position, ColorUtils.PURPLE);
            }
        }
    }
}
