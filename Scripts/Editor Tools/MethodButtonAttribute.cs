using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field)]
public class MethodButtonAttribute : PropertyAttribute
{
    public string Method { get; private set; }
    public string Label  { get; private set; }
    public string[] Parameters { get; private set; }

    public MethodButtonAttribute(string method, params string[] parameters)
    {
        Method = method;
        Parameters = parameters;

    #if UNITY_EDITOR
        Label = ObjectNames.NicifyVariableName(method);
    #else
        Label = method;
    #endif
    }
}

// Base type for attribute field to use to enforce consistency,
// since field type is not needed
[Serializable]
public class MethodButton
{
    // Script templates
    [MethodButton(nameof(NoParametersExample))]
    [SerializeField, Space(10)] MethodButton m_0;

    [MethodButton(nameof(WithParametersExample), nameof(m_ex))]
    [SerializeField, Space(10)] MethodButton m_1;
    [SerializeField] int m_ex;

    void NoParametersExample() {}
    void WithParametersExample(int ex) { Debug.Log(ex); }
}