using UnityEngine;using static BumbleBloomController;using static BumbleBloomStateMachine;public class BumbleBloomState_Walk : State_BumbleBloom{    public bool IsGoingToBush { get; private set; }    Vector3? _nextDestination;    public override void InitState()    {    }    public override void Begin(int previousState)    {        if (_nextDestination.HasValue)
        {
            _controller.NavigationUnit.OnDestinationReached.AddListener(OnReachedDestination);            _controller.NavigationUnit.GoTo(_nextDestination.Value);            _nextDestination = null;        }    }    public override void End(int nextState)    {        _controller.NavigationUnit.OnDestinationReached.RemoveListener(OnReachedDestination);    }    public override void Process()    {    }    public void SetNextDestination(Vector3 worldPosition, bool isGoingToBush = false)
    {
        _nextDestination = worldPosition;

        IsGoingToBush = isGoingToBush;
    }    void OnReachedDestination()
    {
        if (IsGoingToBush)
        {            IsGoingToBush = false;

            _controller.NavigationUnit.OnDestinationReached.RemoveListener(OnReachedDestination);
            _controller.NavigationUnit.Wander();
        }
        else
        {
            _parentStateMachine.ChangeState(BumbleBloomState.Idle);
        }
    }}