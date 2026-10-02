using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace BehaviorTree
{
    public class BehaviorTree : MonoBehaviour
    {
        [SerializeField] bool _debugCurrent;
        [SerializeField] bool _debugActive;
        [SerializeField] bool _debugTick;

        public Node PreviousBehavior { get; private set; }
        public Node CurrentBehavior  { get; private set; }

        bool _initialized;

        Node _root;

        string _previousDebugActiveLog;
        string _previousDebugTickLog;

        readonly Dictionary<string, Node> _nodes = new();

        readonly List<Node> _debugTicks = new();

        public void Init()
        {
            var children = transform.GetComponentsInDirectChildren<Node>();
            if (children.Length != 1)
            {
                throw new System.Exception($"Behavior Tree must have 1 root node: {gameObject.GetHierarchy()}");
            }

            _root = children[0];
            _root.Init(this);

            _initialized = true;
        }

        public void Tick()
        {
            if (_initialized == false)
            {
                return;
            }

            var status = _root.Tick();

            if (_debugTick)
            {
                LogDebugTicks();
                _debugTicks.Clear();
            }

            if (status == Node.Status.Failure || status == Node.Status.None)
            {
                SetCurrentBehavior(null);
                return;
            }

            var next = _root.GetActiveBehavior();

            if (_debugActive)
            {
                string log = $"Active: {next.PathName}".Color(Node.GetStatusColor(status));
                if (log != _previousDebugActiveLog)
                {
                    SystemLog.Debug(log);
                    _previousDebugActiveLog = log;
                }
            }

            SetCurrentBehavior(next);
        }

        public void RegisterNode(Node node)
        {
            _nodes.Add(node.Name, node);
        }

        public void RegisterStatus(Node node)
        {
            if (_debugTick)
            {
                _debugTicks.Add(node);
            }
        }

        public void Cancel()
        {
            _root.Cancel();
        }

        public void ResetTree()
        {
            _root.ResetNode();

            PreviousBehavior = null;
            CurrentBehavior  = null;

            _previousDebugActiveLog = null;
            _previousDebugTickLog = null;
            _debugTicks.Clear();
        }

        void SetCurrentBehavior(Node behavior)
        {
            if (CurrentBehavior == null && behavior == null)
            {
                return;
            }

            if (CurrentBehavior != null && behavior != null && CurrentBehavior == behavior)
            {
                return;
            }

            PreviousBehavior = CurrentBehavior;
            CurrentBehavior = behavior;

            if (PreviousBehavior != null)
            {
                if (PreviousBehavior.CurrentStatus == Node.Status.Running)
                {
                    PreviousBehavior.Cancel();
                }

                if (CurrentBehavior != null && 
                    PreviousBehavior is Action from && CurrentBehavior is Action to)
                {
                    from.TransitionTo  (to);
                    to  .TransitionFrom(from);
                }
            }

            if (_debugCurrent)
            {
                string previous = PreviousBehavior == null ? "null" : PreviousBehavior.Name;
                string current  = CurrentBehavior  == null ? "null" : CurrentBehavior .Name;
                SystemLog.Debug($"{previous} -> {current}".Color(ColorUtils.PINK));
            }
        }

        void LogDebugTicks()
        {
            if (_debugTicks.Count == 0)
            {
                return;
            }

            string log = "";
            string statusLog = _debugTicks[0].Name;
            var previousStatus = _debugTicks[0].CurrentStatus;
            for (int i = 1; i < _debugTicks.Count; i++)
            {
                var node = _debugTicks[i];
                if (node.CurrentStatus != previousStatus)
                {
                    log += statusLog.Color(Node.GetStatusColor(previousStatus));
                    statusLog = string.Empty;
                    previousStatus = node.CurrentStatus;
                }

                statusLog += " -> " + node.Name;
            }
            log += statusLog.Color(Node.GetStatusColor(previousStatus));

            if (log != _previousDebugTickLog)
            {
                SystemLog.Debug($"Tick: " + log);
                _previousDebugTickLog = log;
            }
        }
    }
}
