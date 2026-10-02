using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class TimeCheck : Node
    {
        [SerializeField] MinMaxValue.Float _timeRange;

        protected override string GetNodeType() => nameof(TimeCheck);

        protected override Status OnTick()
        {
            return _timeRange.IsWithinRange(DayNightCycle.CurrentHour) ? Status.Success : Status.Failure;
        }
    }
}
