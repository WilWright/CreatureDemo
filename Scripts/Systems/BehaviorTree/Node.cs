using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

namespace BehaviorTree
{
    public abstract class Node : MonoBehaviour
    {
        public enum Status
        {
            None,
            Failure,
            Running,
            Success
        }

        [SerializeField] string _name;

        public string Name => string.IsNullOrEmpty(_name) ? GetType().Name : _name;

        public string PathName { get; private set; }

        public Status CurrentStatus { get; private set; }

        public Node Parent { get; private set; }

        protected BehaviorTree _behaviorTree;

        protected Node[] _children;

        public virtual void Init(BehaviorTree behaviorTree, Node parent = null, List<Node> selfNodeResults = null)
        {
            InitNode(behaviorTree, parent, selfNodeResults);
            OnInit();
        }

        public bool PathContains(Node node)
        {
            var n = this;
            while (n != null)
            {
                if (n == node)
                {
                    return true;
                }

                n = n.Parent;
            }

            return false;
        }

        public Status Tick()
        {
            if (CurrentStatus != Status.Running)
            {
                OnBegin();
            }

            CurrentStatus = OnTick();
            if (CurrentStatus != Status.Running)
            {
                OnEnd();
            }

            _behaviorTree.RegisterStatus(this);

            return CurrentStatus;
        }

        public void Cancel()
        {
            CurrentStatus = Status.None;

            if (_children != null)
            {
                foreach (var node in _children)
                {
                    node.Cancel();
                }
            }

            OnCancel();
        }

        public void ResetNode()
        {
            CurrentStatus = Status.None;

            if (_children != null)
            {
                foreach (var node in _children)
                {
                    node.ResetNode();
                }
            }

            OnReset();
        }

        public static Color GetStatusColor(Status status)
        {
            return status switch
            {
                Status.Failure => ColorUtils.RED,
                Status.Running => ColorUtils.YELLOW,
                Status.Success => ColorUtils.GREEN,
                _ => ColorUtils.PINK
            };
        }

        protected void InitNode(BehaviorTree behaviorTree, Node parent, List<Node> selfNodeResults)
        {
        #if UNITY_EDITOR
            selfNodeResults ??= new List<Node>();
            GetComponents(selfNodeResults);
            if (selfNodeResults.Count > 1)
            {
                throw new System.Exception($"Node should only have 1 Node component on game object: {gameObject.GetHierarchy()}");
            }
        #endif

            _behaviorTree = behaviorTree;

            Parent = parent;

            var p = parent;
            var builder = new CollectionStringBuilder(" < ");
            builder.Append(Name);
            while (p != null)
            {
                builder.Append(p.Name);
                p = p.Parent;
            }
            PathName = builder.Build();

            var children = transform.GetComponentsInDirectChildren<Node>();
            _children = children.Length > 0 ? children : null;
            foreach (var node in children)
            {
                node.Init(behaviorTree, this, selfNodeResults);
            }

            _behaviorTree.RegisterNode(this);
        }

        protected void EnforceNoChildren()
        {
        #if UNITY_EDITOR
            if (_children != null)
            {
                throw new System.Exception($"Node cannot have any children: {gameObject.GetHierarchy()}");
            }
        #endif
        }

        protected void EnforceChildMinimum(int min)
        {
        #if UNITY_EDITOR
            if (_children == null || _children.Length < min)
            {
                throw new System.Exception($"Node requires at least {GetChildPlurality(min)}: {gameObject.GetHierarchy()}");
            }
        #endif
        }

        protected void EnforceChildCount(int count)
        {
        #if UNITY_EDITOR
            if (_children == null || _children.Length != count)
            {
                throw new System.Exception($"Node must have {GetChildPlurality(count)}: {gameObject.GetHierarchy()}");
            }
        #endif
        }

        protected void EnforceChildMaximum(int max)
        {
        #if UNITY_EDITOR
            if (_children != null && _children.Length > max)
            {
                throw new System.Exception($"Node cannot have more than {GetChildPlurality(max)}: {gameObject.GetHierarchy()}");
            }
        #endif
        }

        public virtual Node GetActiveBehavior() => this;

        protected virtual string GetNodeType() => nameof(Node);

        protected virtual void OnInit  () {}
        protected virtual void OnBegin () {}
        protected virtual void OnEnd   () {}
        protected virtual void OnCancel() {}
        protected virtual void OnReset () {}
        protected abstract Status OnTick();

        string GetChildPlurality(int count) => $"{count} {(count == 1 ? "child" : "children")}";

    #if UNITY_EDITOR
        void OnValidate()
        {
            gameObject.name = $"{Name} - {GetNodeType()}";
        }
    #endif
    }
}
