using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace BehaviorTree
{
    public class Randomizer : Node
    {
        [Tooltip("The next selection can't be a repeat of the previous selection")]
        [SerializeField] bool _dontRepeatTwiceInARow;
        [Tooltip("Selections will be shuffled and used in order before being shuffled again")]
        [SerializeField] bool _exhaustAllBeforeAnyRepeats;
        [Tooltip("If weights aren't used, all selections have an equal chance to be picked")]
        [SerializeField] bool _useWeights;

        [SerializeField] List<float> _weights;

        Node _previousChild;
        Node _currentChild;

        readonly WeightedRoll<Node> _weightedRoll = new();

        readonly Queue<Node> _queue = new();

        public override Node GetActiveBehavior()
        {
            return _currentChild == null ? null : _currentChild.GetActiveBehavior();
        }

        protected override string GetNodeType() => nameof(Randomizer);

        protected override void OnInit()
        {
            EnforceChildMinimum(2);

            if (_useWeights)
            {
                if (_weights.Count != _children.Length)
                {
                    throw new System.Exception($"Randomizer weights count ({_weights.Count}) must match children count ({_children.Length}): {gameObject.GetHierarchy()}");
                }
            }
            else
            {
                for (int i = 0; i < _children.Length; i++)
                {
                    _weights.Add(1);
                }
            }

            FillWeights();
        }

        protected override void OnBegin()
        {
            _currentChild = null;
        }

        protected override void OnEnd()
        {
            _previousChild = _currentChild;
        }

        protected override Status OnTick()
        {
            if (_currentChild != null)
            {
                return _currentChild.Tick();
            }

            if (_exhaustAllBeforeAnyRepeats)
            {
                if (_queue.Count == 0)
                {
                    ShuffleQueue();
                }

                _currentChild = _queue.Dequeue();
                return _currentChild.Tick();
            }

            _currentChild = GetRandom();
            return _currentChild == null ? Status.Failure : _currentChild.Tick();
        }

        protected override void OnReset()
        {
            _previousChild = null;
            _currentChild  = null;

            _queue.Clear();

            FillWeights();
        }

        void FillWeights()
        {
            _weightedRoll.Clear();

            for (int i = 0; i < _children.Length; i++)
            {
                _weightedRoll.AddWeight(_children[i], _weights[i]);
            }
        }

        Node GetRandom()
        {
            Node repeated = null;
            double repeatedWeight = 0;
            if (_dontRepeatTwiceInARow && _previousChild != null)
            {
                for (int i = 0; i < _children.Length; i++)
                {
                    var child = _children[i];
                    if (child == _previousChild)
                    {
                        repeated = child;
                        _weightedRoll.RemoveWeight(child);
                        break;
                    }
                }
            }

            if (_weightedRoll.Roll(out var roll) == false)
            {
                return null;
            }

            if (repeated != null)
            {
                _weightedRoll.AddWeight(repeated, repeatedWeight);
            }

            return roll;
        }

        void ShuffleQueue()
        {
            _queue.Clear();

            int rolls = _weightedRoll.Count;
            for (int i = 0; i < rolls; i++)
            {
                if (_weightedRoll.Roll(out var roll) == false)
                {
                    continue;
                }

                _weightedRoll.RemoveWeight(roll);
                _queue.Enqueue(roll);
            }

            FillWeights();

            // Check for repeat of last pick
            if (_dontRepeatTwiceInARow && _previousChild != null && _queue.TryPeek(out var peek) && peek == _previousChild)
            {
                // Cycle by random amount
                int cycle = Random.Range(1, _queue.Count);
                for (int i = 0; i < cycle; i++)
                {
                    _queue.Enqueue(_queue.Dequeue());
                }
            }
        }
    }
}
