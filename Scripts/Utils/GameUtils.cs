using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public static class GameUtils
    {
        public static void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public static void Delay(this MonoBehaviour monoBehaviour, Action callback, float delay, bool useRealTime = false)
        {
            monoBehaviour.StartCoroutine(IeDelay(callback, delay, useRealTime));
        }
        static IEnumerator IeDelay(Action callback, float delay, bool useRealTime = false)
        {
            if (useRealTime)
            {
                var wait = new WaitForSecondsRealtime(delay);
                yield return wait;
            }
            else
            {
                var wait = new WaitForSeconds(delay);
                yield return wait;
            }

            callback.Invoke();
        }
    }
}
