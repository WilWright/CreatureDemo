using UnityEngine;

namespace Navigation
{
    [RequireComponent(typeof(Collider))]
    public class NavigationTerrain : MonoBehaviour
    {
        [field: SerializeField] public NavigationTerrainConfig Config { get; private set; }
    }
}
