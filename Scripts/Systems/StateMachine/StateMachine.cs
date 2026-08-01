using System;
using System.Collections.Generic;

namespace StateMachine
{
    public class StateMachine<EnumType> where EnumType : Enum
    {
        public bool IsDebugEnabled { get; set; }

        public bool IsLocked => _isLocked;

        readonly Dictionary<int, State> _states = new();

        State _currentState;
        State _previousState;

        int _currentStateIndex  = -1;
        int _previousStateIndex = -1;

        bool _isLocked = false;

        public void SetLockStatus(bool isLocked) => _isLocked = isLocked;

        public bool IsCurrentState(EnumType state) => _currentStateIndex == Convert.ToInt32(state);

        public void InitState(EnumType state, State stateMethods)
        {
            if (Enum.IsDefined(typeof(EnumType), state) == false)
            {
                SystemLog.Error($"{state} is not a valid value for this stateMachine");
                return;
            }

            _states[Convert.ToInt32(state)] = stateMethods;
        }

        public void ChangeState(EnumType newState)
        {
            int newStateIndex = Convert.ToInt32(newState);

            if (_isLocked)
            {
                SystemLog.Warn($"Trying to change to state '{newState}' on locked state machine");
                return;
            }

            if (_states.TryGetValue(newStateIndex, out State newStateMethod) == false)
            {
                SystemLog.Error($"Could not find state for key {newState}");
                return;
            }

            if (IsDebugEnabled)
            {
                SystemLog.Debug($"{_currentState.GetType()} -> {newStateMethod.GetType()}");
            }

            _previousState = _currentState;
            _currentState  = newStateMethod;

            _previousStateIndex = _currentStateIndex;
            _currentStateIndex  = newStateIndex;

            if (_previousState != null)
            {
                _previousState.End(_currentStateIndex);
            }

            _currentState.Begin(_previousStateIndex);
        }

        public void Process()
        {
            if (_currentState == null)
            {
                return;
            }

            _currentState.Process();
        }
    }
}
