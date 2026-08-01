using StateMachine;

public class State_BumbleBloom : State
{
    protected BumbleBloomController _controller = null;
    protected BumbleBloomStateMachine _parentStateMachine = null;

    public void Init(BumbleBloomController controller, BumbleBloomStateMachine parentStateMachine)
    {
        _controller = controller;
        _parentStateMachine = parentStateMachine;
        InitState();
    }
    public virtual void InitState() { }
}
