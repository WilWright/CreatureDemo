using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit Type", menuName = "Scriptable Objects/Units/Unit Type")]
public class UnitType : ScriptableObject
{
    public enum UnitId
    {
        Unregisterd = -1
    }

    public string Name => name;
    public UnitId Id { get; private set; }

    public static int UnitTypeCount => _registeredUnitTypes.Count;

    static readonly Dictionary<string, UnitId> _registeredUnitTypes = new();

    public static void ClearRegistry()
    {
        _registeredUnitTypes.Clear();
    }

    public static void Register(UnitType unitType)
    {
        string name = unitType.name;
        if (_registeredUnitTypes.ContainsKey(name))
        {
            return;
        }

        var id = unitType.Id = (UnitId)_registeredUnitTypes.Count;
        _registeredUnitTypes.Add(name, id);

        SystemLog.Debug($"Registered unit type {name}:{id}");
    }
}
