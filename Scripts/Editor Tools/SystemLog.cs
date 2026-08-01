using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LogLevel { Test, Debug, Info, Warning, Error }
public static class SystemLog
{
    public static LogLevel LogLevel { get; private set; } = LogLevel.Test;

    static bool _isColorEnabled = true;

    static readonly bool IS_DARK_MODE = EditorGUIUtility.isProSkin;

    static readonly string[] LIGHT_MODE_COLORS =
    {
        "B57725",
        "19852A",
        "1F75A6",
        "CBC746",
        "A61F24"
    };

    static readonly string[] DARK_MODE_COLORS =
    {
        "E08C1D",
        "23EC43",
        "29A2E7",
        "FFFA28",
        "E52D34"
    };

    static readonly string[] COLORS = IS_DARK_MODE ? DARK_MODE_COLORS : LIGHT_MODE_COLORS;
    static readonly string DATA_COLOR = IS_DARK_MODE ? "B142E3" : "7B1FA4";

    public static void SetLogLevel(LogLevel logLevel)
    {
        LogLevel = logLevel;
    }

    public static void SetColorEnabled(bool enabled)
    {
        _isColorEnabled = enabled;
    }

    public static void Test (string message, object data = null) => Log(message, data, LogLevel.Test);
    public static void Debug(string message, object data = null) => Log(message, data, LogLevel.Debug);
    public static void Info (string message, object data = null) => Log(message, data, LogLevel.Info);
    public static void Warn (string message, object data = null) => Log(message, data, LogLevel.Warning);
    public static void Error(string message, object data = null) => Log(message, data, LogLevel.Error);

    static void Log(string message, object data, LogLevel logLevel = LogLevel.Info)
    {
        if (LogLevel > logLevel)
        {
            return;
        }

        if (data != null)
        {
            message += $"\n{ColorString(data.ToString(), DATA_COLOR)}";
        }

        UnityEngine.Debug.Log(ColorString(message, COLORS[(int)logLevel]));
    }

    public static bool PopUp(string title, string message, string ok = "Ok", string cancel = "")
    {
    #if UNITY_EDITOR
        return EditorUtility.DisplayDialog(title, message, ok, cancel);
    #endif
    }

    static string ColorString(string s, string color)
    {
        return _isColorEnabled ? $"<b><color=#{color}>{s}</color></b>" : s;
    }
}
