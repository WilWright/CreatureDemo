using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StateMachine;

public class BumbleBloomStateMachine : MonoBehaviour
{
    public enum BumbleBloomState
    { 
        Idle,
        Walk
    }
    
    [SerializeField] bool _debug;

#if UNITY_EDITOR
    [MethodButton(nameof(UpdateStates))]
    [SerializeField, Space(10)] MethodButton m_0;
#endif
    [SerializeField] State_BumbleBloom[] _states;

    [SerializeField] BumbleBloomController _controller;

    StateMachine<BumbleBloomState> _stateMachine;
    public readonly int STATE_LENGTH = System.Enum.GetValues(typeof(BumbleBloomState)).Length;

    public void Init()
    {
        _stateMachine = new StateMachine<BumbleBloomState>();
        for (int i = 0; i < STATE_LENGTH; i++)
        {
            var state = _states[i];
            if (state == null)
            {
                continue;
            }

            _stateMachine.InitState((BumbleBloomState)i, state);
            state.Init(_controller, this);
        }

        _stateMachine.ChangeState(BumbleBloomState.Idle);
    }

    public bool IsCurrentState(BumbleBloomState bumbleBloomState)
    {
        return _stateMachine.IsCurrentState(bumbleBloomState);
    }
    public void ChangeState(BumbleBloomState bumbleBloomState)
    {
        _stateMachine.ChangeState(bumbleBloomState);
    }
    public State_BumbleBloom GetState(BumbleBloomState bumbleBloomState)
    {
        return _states[(int)bumbleBloomState];
    }

    public void Process() 
    {
    #if UNITY_EDITOR
        _stateMachine.IsDebugEnabled = _debug;
    #endif

        _stateMachine.Process();
    }

#if UNITY_EDITOR
    public void UpdateStates()
    {
        var stateHolder = GetStateHolder();
        var children = stateHolder.GetComponentsInChildren<Transform>();

        _states = new State_BumbleBloom[STATE_LENGTH];
        for (int i = 0; i < STATE_LENGTH; i++)
        {
            var s = (BumbleBloomState)i;
            string stateName = s.ToString();

            Transform stateTransform = null;
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child.parent == stateHolder && child.name == stateName)
                    {
                        stateTransform = child;
                        break;
                    }
                }
            }
            if (stateTransform == null)
            {
                stateTransform = new GameObject(stateName).transform;
                stateTransform.SetParent(stateHolder);
                stateTransform.localPosition = Vector3.zero;
            }

            State_BumbleBloom stateComponent;
            var stateType = System.Type.GetType($"{typeof(BumbleBloomState).Name}_{stateName}");
            if (stateTransform.TryGetComponent(stateType, out Component c))
            {
                stateComponent = (State_BumbleBloom)c;
                SystemLog.Info($"Found {s}");
            }
            else
            {
                stateComponent = (State_BumbleBloom)stateTransform.gameObject.AddComponent(stateType);
                SystemLog.Info($"Created {s}");
            }

            _states[i] = stateComponent;
        }
    }

    Transform GetStateHolder()
    {
        var children = gameObject.GetComponentsInChildren<Transform>();
        if (children != null)
        {
            foreach (var child in children)
            {
                if (child.parent == transform && child.name == "States")
                {
                    SystemLog.Info($"Found {transform.name} -> {child.name}");
                    return child;
                }
            }
        }

        var stateHolder = new GameObject("States").transform;
        stateHolder.SetParent(transform);
        stateHolder.localPosition = Vector3.zero;
        SystemLog.Info($"Created {transform.name} -> {stateHolder.name}");

        return stateHolder;
    }

    public void SetController(BumbleBloomController controller)
    {
        _controller = controller;
    }
#endif
}
