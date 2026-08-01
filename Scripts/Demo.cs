using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class Demo : MonoBehaviour
{
    [SerializeField] float _timeScale;
    [SerializeField] float _timeDelay;

    void Start()
    {
        Time.timeScale = 0;

        // Allows for some preloading in demo scene, such as navigation maps
        this.Delay(() => { Time.timeScale = _timeScale; }, _timeDelay, true);
    }
}
