using System.Collections;
using UnityEngine;

public class CoroutineObject
{
    public enum TransitionState { Active, Inactive, Transitioning }
    public TransitionState CurrentTransitionState { get; private set; }

    public bool IsRunning { get; private set; }
    public bool IsNull => _coroutine == null;

    MonoBehaviour _coroutineObject;
    IEnumerator _coroutine;

    public CoroutineObject(MonoBehaviour coroutineObject)
    {
        Init(coroutineObject);
    }
    public CoroutineObject(MonoBehaviour coroutineObject, IEnumerator coroutine)
    {
        Init(coroutineObject, coroutine);
    }

    protected virtual void Init(MonoBehaviour coroutineObject)
    {
        _coroutineObject = coroutineObject;
        IsRunning = false;
        CurrentTransitionState = TransitionState.Inactive;
    }
    protected virtual void Init(MonoBehaviour coroutineObject, IEnumerator coroutine)
    {
        Init(coroutineObject);
        Set(coroutine);
    }

    public void SetTransitionState(TransitionState transitionState)
    {
        CurrentTransitionState = transitionState;
    }

    public virtual void Set(IEnumerator coroutine, bool start = false)
    {
        _coroutine = coroutine;

        if (start)
        {
            Start();
        }
    }

    public virtual void Start()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;

        if (_coroutine != null)
        {
            _coroutineObject.StartCoroutine(_coroutine);
        }
    }

    public virtual void Stop()
    {
        if (IsRunning == false)
        {
            return;
        }

        Done();

        if (_coroutine != null)
        {
            _coroutineObject.StopCoroutine(_coroutine);
        }
    }

    public virtual bool Restart(IEnumerator replacement = null)
    {
        bool restarted = IsRunning;

        Stop();

        if (replacement != null)
        {
            Set(replacement);
        }

        Start();

        return restarted;
    }

    public virtual void Done(TransitionState transitionState = TransitionState.Inactive)
    {
        IsRunning = false;
        CurrentTransitionState = transitionState;
    }
}
