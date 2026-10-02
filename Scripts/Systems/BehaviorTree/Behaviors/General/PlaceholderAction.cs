using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class PlaceholderAction : Action
    {
        protected override Status OnTick()
        {
            SystemLog.Debug("Placeholder " + Name);
            return Status.Success;
        }
    }
}
