using UnityEngine;

namespace Navigation
{
    [CreateAssetMenu(fileName = "Terrain Config", menuName = "Scriptable Objects/Navigation/Terrain Config")]
    public class NavigationTerrainConfig : ScriptableObject
    {
        [field: SerializeField] public bool IsNavigable { get; private set; }
    }
}
