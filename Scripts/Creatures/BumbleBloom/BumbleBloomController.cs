using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Navigation;

public class BumbleBloomController : MonoBehaviour
{
    [SerializeField] BehaviorTree.BehaviorTree _behaviorTree;

    [field: SerializeField] public NavigationUnit NavigationUnit { get; private set; }

    void Start()
    {
        _behaviorTree.Init();
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        _behaviorTree.Tick();
    }
}
