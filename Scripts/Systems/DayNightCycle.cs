using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    [Serializable]
    public class TimeTracker
    {
        [SerializeField] float _totalMinutes;
        [SerializeField] Gradient _lightColor;
        [SerializeField] AnimationCurve _lightIntensity;
        [SerializeField] Gradient _skyColor;

        public float TotalSeconds   { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public float TimePercent    { get; private set; }

        public Color LightColor { get; private set; }
        public float LightIntensityPercent { get; private set; }
        public Color SkyColor { get; private set; }

        public void Init(float startSeconds = 0)
        {
            ElapsedSeconds = startSeconds;
            TotalSeconds = _totalMinutes * 60;
        }

        public void Update()
        {
            ElapsedSeconds = Mathf.Clamp(ElapsedSeconds, ElapsedSeconds + Time.deltaTime, TotalSeconds);
            UpdateProperties();
        }

        public void Reset()
        {
            ElapsedSeconds = 0;
            UpdateProperties();
        }

        void UpdateProperties()
        {
            TimePercent = ElapsedSeconds / TotalSeconds;
            LightColor = _lightColor.Evaluate(TimePercent);
            LightIntensityPercent = _lightIntensity.Evaluate(TimePercent);
            SkyColor = _skyColor.Evaluate(TimePercent);
        }
    }

    [SerializeField] TimeTracker _dayTimeTracker;
    [SerializeField] TimeTracker _nightTimeTracker;
    [SerializeField] Light _light;
    [SerializeField] MinMaxValue.Float _lightIntensityRange;
    [SerializeField] Text _timerText;

    public static bool IsDayTime   { get; private set; }
    public static bool IsNightTime { get; private set; }

    public static float TimePercent { get; private set; }

    public static float CurrentHour => (TimePercent * HOUR_SCALE + HOUR_SCALE_START) % HOUR_SCALE;

    public static UnityEvent OnDayStart   = new();
    public static UnityEvent OnNightStart = new();

    public static UnityEvent<int> OnHourStart = new();

    TimeTracker[] _timeTrackers;
    int _currentTimeTrackerIndex;

    Vector3 _lightRotation = new(0, -90, 0);

    int _previousHour;
    float _totalSeconds;

    const float HOUR_SCALE_START = 6;
    const float HOUR_SCALE = 24;

    void Awake()
    {
        _timeTrackers = new TimeTracker[] { _dayTimeTracker, _nightTimeTracker };

        _dayTimeTracker  .Init();
        _nightTimeTracker.Init();

        foreach (var timeTracker in _timeTrackers)
        {
            _totalSeconds += timeTracker.TotalSeconds;
        }
    }

    void Update()
    {
        int currentHour = (int)CurrentHour;
        if (currentHour != _previousHour)
        {
            OnHourStart.Invoke(currentHour);
            _previousHour = currentHour;
        }

        bool wasDayTime   = IsDayTime;
        bool wasNightTime = IsNightTime;

        var currentTimeTracker = _timeTrackers[_currentTimeTrackerIndex];
        currentTimeTracker.Update();
        if (currentTimeTracker.TimePercent >= 1)
        {
            if (++_currentTimeTrackerIndex >= _timeTrackers.Length)
            {
                _currentTimeTrackerIndex = 0;

                foreach (var timeTracker in _timeTrackers)
                {
                    timeTracker.Reset();
                }
            }
        }

        IsDayTime   = _currentTimeTrackerIndex == 0;
        IsNightTime = _currentTimeTrackerIndex == 1;

        if (wasDayTime == false && IsDayTime)
        {
            OnDayStart.Invoke();
        }

        if (wasNightTime == false && IsNightTime)
        {
            OnNightStart.Invoke();
        }

        float elapsedSeconds = 0;
        foreach (var timeTracker in _timeTrackers)
        {
            elapsedSeconds += timeTracker.ElapsedSeconds;
        }
        TimePercent = elapsedSeconds / _totalSeconds;

        _light.color = currentTimeTracker.LightColor;
        _light.intensity = _lightIntensityRange.Lerp(currentTimeTracker.LightIntensityPercent);

        Camera.main.backgroundColor = currentTimeTracker.SkyColor;

        if (IsDayTime)
        {
            _lightRotation.x = currentTimeTracker.TimePercent * 180.0f;
        }
        else
        {
            _lightRotation.x = 90;
        }

        _light.transform.rotation = Quaternion.Euler(_lightRotation);

        _timerText.text = new DateTime().AddHours(CurrentHour).ToShortTimeString();
    }
}
