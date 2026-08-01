using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
    public class NavigationPath
    {
        public readonly struct Step
        {
            public readonly NavigationPoint NavigationPoint;
            public readonly Vector3 Position;
            public readonly Vector3 DirectionToNextStep;
            public readonly float DistanceToNextStep;

            public Step(NavigationPoint navigationPoint) : this(navigationPoint, Vector3.zero, 0) {}
            public Step(NavigationPoint navigationPoint, Vector3 directionToNextStep, float distanceToNextStep)
            {
                NavigationPoint     = navigationPoint;
                Position            = navigationPoint.Hit.Point;
                DirectionToNextStep = directionToNextStep;
                DistanceToNextStep  = distanceToNextStep;
            }
        }

        public Step? CurrentStep { get; private set; }
        public Step? NextStep    { get; private set; }

        public int CurrentStepIndex { get; private set; } = -1;

        public bool IsDone => CurrentStepIndex >= Steps.Length;

        public readonly Step[] Steps;

        public readonly float TotalDistance;

        public NavigationPath(NavigationNode pathEnd)
        {
            var points = new List<NavigationPoint>();

            var current = pathEnd;
            var lastdirection = Vector3.zero;
            while (current != null)
            {
                // If going in a straight line, don't add unnecessary inbetween steps
                var parent = current.Parent;
                while (parent != null)
                {
                    var direction = (parent.Point.Hit.Point - current.Point.Hit.Point).normalized;
                    if (direction != lastdirection)
                    {
                        lastdirection = direction;
                        break;
                    }

                    current = parent;
                    parent = current.Parent;
                }

                points.Add(current.Point);

                current = parent;
            }

            if (points.Count == 0)
            {
                return;
            }

            points.Reverse();

            Steps = new Step[points.Count];
            for (int i = 1; i < points.Count; i++)
            {
                var fromPoint = points[i - 1];
                var toPoint   = points[i];
                var vector = toPoint.Hit.Point - fromPoint.Hit.Point;
                float distance = vector.magnitude;
                TotalDistance += distance;

                Steps[i - 1] = new Step(fromPoint, vector.normalized, distance);
            }
            Steps[^1] = new Step(points[^1]);

            AdvanceStep();
        }

        public void AdvanceStep()
        {
            if (Steps == null || IsDone)
            {
                return;
            }

            CurrentStep = ++CurrentStepIndex >= Steps.Length ? null : Steps[CurrentStepIndex];

            int nextStepIndex = CurrentStepIndex + 1;
            NextStep = nextStepIndex >= Steps.Length ? null : Steps[nextStepIndex];
        }
    }
}
