using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Navigation;
using Utils;

using static BumbleBloomStateMachine;

public class BumbleBloomController : MonoBehaviour
{
    [Header("General")]
#if UNITY_EDITOR
    [MethodButton(nameof(InitStateMachine))]
    [SerializeField, Space(10)] MethodButton m_0;
#endif
    [field: SerializeField] public BumbleBloomStateMachine StateMachine { get; private set; }

    [field: SerializeField] public NavigationUnit NavigationUnit { get; private set; }

    [SerializeField] MinMaxValue.Float _goToFieldDelay;
    [SerializeField] MinMaxValue.Float _goToBushDelay;

    BushNavigationData _currentBush;

    const float OUT_OF_BUSH_RANGE_SQR = 0.25f; // 0.5²

    const int GO_TO_FIELD_HOUR = 7;
    const int GO_TO_BUSH_HOUR  = 19;

    void Start()
    {
        DayNightCycle.OnHourStart.AddListener(CheckDailyRoutine);

        StateMachine.Init();
    }

    void Update()
    {
        StateMachine.Process();

        if (StateMachine.IsCurrentState(BumbleBloomState.Walk))
        {
            var walkState = (BumbleBloomState_Walk)StateMachine.GetState(BumbleBloomState.Walk);
            if (walkState.IsGoingToBush == false)
            {
                CheckCurrentBush();
            }
        }
    }

    void CheckDailyRoutine(int hour)
    {
        NavigationData[] navigationDatas;

        switch (hour)
        {
            case GO_TO_FIELD_HOUR:
                if (GameController.ChunkManager.TryGetNavigationDatas(transform.position, out navigationDatas))
                {
                    foreach (var data in navigationDatas)
                    {
                        if (data is BumbleBloomFieldNavigationData bbf)
                        {
                            var walkState = (BumbleBloomState_Walk)StateMachine.GetState(BumbleBloomState.Walk);
                            this.Delay(() => 
                            {
                                walkState.SetNextDestination(bbf.GetRandomRestPosition());
                                StateMachine.ChangeState(BumbleBloomState.Walk);
                            }, _goToFieldDelay.GetRandomValue());
                            break;
                        }
                    }
                }
                break;

            case GO_TO_BUSH_HOUR:
                if (_currentBush != null)
                {
                    break;
                }

                if (GameController.ChunkManager.TryGetNavigationDatas(transform.position, out navigationDatas))
                {
                    foreach (var data in navigationDatas)
                    {
                        if (data is BushNavigationData b)
                        {
                            if (b.ClaimCapacity(NavigationUnit.UnitConfig))
                            {
                                _currentBush = b;

                                var walkState = (BumbleBloomState_Walk)StateMachine.GetState(BumbleBloomState.Walk);
                                this.Delay(() =>
                                {
                                    walkState.SetNextDestination(b.WorldPosition, true);
                                    StateMachine.ChangeState(BumbleBloomState.Walk);
                                }, _goToBushDelay.GetRandomValue());
                                break;
                            }
                        }
                    }
                }
                break;
        }
    }

    void CheckCurrentBush()
    {
        if (_currentBush == null)
        {
            return;
        }

        var vector = transform.position - _currentBush.WorldPosition;
        if (vector.sqrMagnitude < OUT_OF_BUSH_RANGE_SQR)
        {
            return;
        }

        _currentBush.ReleaseCapacity(NavigationUnit.UnitConfig);
        _currentBush = null;
    }

#if UNITY_EDITOR
    void InitStateMachine()
    {
        if (StateMachine != null)
        {
            SystemLog.Info("BumbleBloomStateMachine already initialized");
            return;
        }

        StateMachine = gameObject.AddComponent<BumbleBloomStateMachine>();
        SystemLog.Info("Created BumbleBloomStateMachine");

        StateMachine.SetController(this);
        StateMachine.UpdateStates();
    }
#endif
}
